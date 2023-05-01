using AdvancedSharpAdbClient;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Windows.Forms;

namespace AdbUninstaller
{
    public partial class Form1 : Form
    {
        private readonly AdbClient _adbClient = new AdbClient();
        private readonly DeviceData _device;
        private List<PackageInfo> appList = new List<PackageInfo>();
        private List<PackageInfo> filterList = new List<PackageInfo>();

        public Form1()
        {
            InitializeComponent();

            var devices = _adbClient.GetDevices();
            if (!devices.Any())
            {
                MessageBox.Show("δ�����κ��豸", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
            _device = devices[0];

            UpdateDeviceStatus();
            RefreshAppList();
        }

        private void UpdateDeviceStatus()
        {
            var deviceOnline = IsDeviceOnline();
            if (!deviceOnline)
            {
                statusText.Text = "�豸δ����";
                statusText.ForeColor = System.Drawing.Color.Red;
                return;
            }

            var model = ExecuteShellCommand("getprop ro.product.model").TrimEnd('\r', '\n');
            var serial = ExecuteShellCommand("getprop ro.serialno").TrimEnd('\r', '\n');
            statusText.Text = $"�豸�����ӣ�{model} ({serial})";
            statusText.ForeColor = System.Drawing.Color.Green;
        }

        private bool IsDeviceOnline()
        {
            var devices = _adbClient.GetDevices();
            foreach (var device in devices)
            {
                if (device.State == DeviceState.Online)
                {
                    return true; // �����豸�Ƿ�����
                }
            }
            return false;
        }

        //Ӧ���б�ʵʱˢ��
        private delegate void RefreshAppListDelegate();

        private async void RefreshAppList()
        {
            Log("����ˢ��Ӧ���б�...");

            // �첽��ȡӦ���б�
            var packageTask = Task.Run(() =>
            {
                appList.Clear();
                var packages = ExecuteShellCommand("pm list packages -f");
                if (string.IsNullOrEmpty(packages))
                {
                    Log("��ȡӦ���б�ʧ��");
                    return;
                }

                var packageLines = packages.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in packageLines)
                {
                    var pair = line.Split('=');

                    var packagePath = pair[0].Replace("package:", "");
                    var packageName = pair[1];

                    string appType;
                    if (packagePath.StartsWith("/system/"))
                    {
                        appType = "ϵͳӦ��";
                    }
                    else if (packagePath.StartsWith("/data/"))
                    {
                        appType = "�û�Ӧ��";
                    }
                    else
                    {
                        appType = "δ֪Ӧ��";
                    }

                    var appInfoJson = ExecuteShellCommand($"dumpsys package {packagePath}");
                    var appInfo = JsonConvert.DeserializeObject<AppInfo>(appInfoJson);
                    var appName = appInfo?.Application?.Label ?? packagePath;

                    appList.Add(new PackageInfo
                    {
                        PackageName = packageName,
                        AppName = appName,
                        PackagePath = packagePath,
                        AppType = appType
                    });
                }

                appList = appList.OrderByDescending(p => p.AppType).ToList();
                filterList = appList;
            });

            await packageTask;

            // ������ʾ�б�ͼ�¼��־
            BeginInvoke(new RefreshAppListDelegate(() =>
            {
                ShowAppList();
                Log($"Ӧ���б��Ѿ�ˢ�£����� {appList.Count} ��Ӧ��");
            }));
        }


        private void FilterAppList(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                filterList = appList;
            }
            else
            {
                filterList = appList.Where(p => p.PackageName.ToLower().Contains(keyword.ToLower())).ToList();
            }
            ShowAppList();
        }

        private void ShowAppList()
        {
            appListView.Items.Clear();
            filterList.ForEach(p =>
            {
                var item = new ListViewItem(new string[] { p.AppType, p.AppName, p.PackageName });
                appListView.Items.Add(item);
            });
        }
        //�첽ж�ز���

        private async Task UninstallSelectedAppsAsync()
        {
            Log($"ж�ز�����ʼ...");
            try
            {
                var selectedItems = appListView.CheckedItems;
                if (selectedItems.Count == 0)
                {
                    MessageBox.Show("������ѡ��һ��Ӧ��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Log($"δѡ���κ�Ӧ�ã�ж�ز�����ֹ��");
                    return;
                }
                foreach (ListViewItem item in selectedItems)
                {
                    var packageName = item.SubItems[2].Text;
                    if (!string.IsNullOrEmpty(packageName))
                    {
                        Log($"�� {packageName} ����ж����...");
                        var result = await Task.Run(() => ExecuteShellCommand($"pm uninstall --user 0 {packageName}"));
                        if (result.Contains("Success"))
                        {
                            Log($"{item.SubItems[1].Text} ��ж��");
                        }
                        else
                        {
                            Log($"ж��ʧ�ܣ�{result}");
                        }
                    }
                }
                Log($"ж�ز���������");
                RefreshAppList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Log($"ж�ز���ʧ�ܣ�{ex.Message}");
            }
        }





        private async Task InstallApkFilesAsync(string[] apkFiles)
        {
            OnOperationStarted("��װ������ʼ...");

            foreach (var filePath in apkFiles)
            {
                if (File.Exists(filePath))
                {
                    try
                    {
                        Log($"APK {filePath} ���ڰ�װ��...");
                        var result = await Task.Run(() =>
                        {
                            // ִ�а�װ����
                            var arguments = $"install \"{filePath}\"";
                            return ExecuteCommand("adb", arguments);
                        });

                        if (result.Contains("Success"))
                        {
                            Log($"APK {filePath} ��װ�ɹ�");
                        }
                        else
                        {
                            Log($"��װʧ�ܣ�{result}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log($"��װʧ�ܣ�{ex.Message}");
                    }
                }
                else
                {
                    Log($"�ļ������ڣ�{filePath}");
                }
            }
            OnOperationFinished("��װ����������");
            RefreshAppList();
        }
        public event EventHandler<string> OperationStarted;
        public event EventHandler<string> OperationFinished;

        private void OnOperationStarted(string message)
        {
            OperationStarted?.Invoke(this, message);
        }

        private void OnOperationFinished(string message)
        {
            OperationFinished?.Invoke(this, message);
        }








        private string ExecuteCommand(string fileName, string arguments)
        {
            string result = null;
            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            using (var process = new Process())
            {
                process.StartInfo = psi;

                process.Start();

                result = process.StandardOutput.ReadToEnd();

                process.WaitForExit();
            }

            return result;
        }

        private string ExecuteShellCommand(string command)
        {
            if (!IsDeviceOnline())
            {
                // �豸δ�������ߣ����ؿ��ַ���
                return string.Empty;
            }

            var receiver = new ConsoleOutputReceiver();
            try
            {
                _adbClient.ExecuteRemoteCommand(command, _device, receiver);
            }
            catch (Exception ex)
            {
                Log($"ִ������ {command} ʧ�ܣ�{ex.Message}");
            }
            return receiver.ToString().TrimEnd('\r', '\n');
        }

        private void Log(string message)
        {
            logBox.AppendText($"{DateTime.Now:HH:mm:ss} {message}\r\n");
            logBox.ScrollToCaret();
        }

        private void statusButton_Click(object sender, EventArgs e)
        {
            UpdateDeviceStatus();
        }

        private void searchBox_TextChanged(object sender, EventArgs e)
        {
            var keyword = searchBox.Text.Trim();
            FilterAppList(keyword);
        }

        private async void uninstallButton_Click(object sender, EventArgs e)
        {
            await UninstallSelectedAppsAsync();
        }


        private async void installButton_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "APK �ļ�|*.apk"
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                await InstallApkFilesAsync(openFileDialog.FileNames);
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            appListView.Columns.Add("Ӧ������", 140);
            appListView.Columns.Add("Ӧ�õ�ַ", 300);
            appListView.Columns.Add("����", 300);
            appListView.FullRowSelect = true;
            appListView.MultiSelect = true;
        }

        private void RefreshAppListbtn_Click(object sender, EventArgs e)
        {
            RefreshAppList();
        }

        private void githubLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenUrl("https://github.com/Muytec/AdbUninstaller/releases");
        }

        private void groupLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            OpenUrl("http://qm.qq.com/cgi-bin/qm/qr?_wv=1027&k=mGOBfWQd7G0vp7FqVkAocuXyw4fH9XbA&authKey=6bRAnn3ilyhQY2Os1ku6I0glT0%2FFC%2FCIQcNBZ5tAeIHnIfAXDl4iOP8ZunAplR14&noverify=0&group_code=156115036");
        }
        private void AuthorLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("΢�Ź��ںţ�Ľ��" + Environment.NewLine + "QQ:2535093954", "��ϵ����", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void OpenUrl(string url)
        {
            // ��ȡע����е�Ĭ�������

            try
            {
                // ���Դ�Ĭ�����������������
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = "/c start " + url.Replace("\"", ""),
                    CreateNoWindow = true
                });
                return;
            }
            catch
            {
                Clipboard.SetText(url);
                MessageBox.Show("�޷���ָ����ҳ�������Ѹ��ƣ����ֶ��򿪣�");
            }
        }


    }

    public class AppInfo
    {
        [JsonProperty("isSystemApp")]
        public bool IsSystemApp { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("versionCode")]
        public int VersionCode { get; set; }

        [JsonProperty("versionName")]
        public string VersionName { get; set; }

        [JsonProperty("packageName")]
        public string PackageName { get; set; }

        [JsonProperty("application")]
        public ApplicationInfo Application { get; set; }
    }

    public class ApplicationInfo
    {
        [JsonProperty("label")]
        public string Label { get; set; }
    }

    public class PackageInfo
    {
        public string AppType { get; set; }

        public string AppName { get; set; }

        public string PackageName { get; set; }

        public string PackagePath { get; set; }
    }
}
