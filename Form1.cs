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
                MessageBox.Show("未连接任何设备", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                statusText.Text = "设备未连接";
                statusText.ForeColor = System.Drawing.Color.Red;
                return;
            }

            var model = ExecuteShellCommand("getprop ro.product.model").TrimEnd('\r', '\n');
            var serial = ExecuteShellCommand("getprop ro.serialno").TrimEnd('\r', '\n');
            statusText.Text = $"设备已连接：{model} ({serial})";
            statusText.ForeColor = System.Drawing.Color.Green;
        }

        private bool IsDeviceOnline()
        {
            var devices = _adbClient.GetDevices();
            foreach (var device in devices)
            {
                if (device.State == DeviceState.Online)
                {
                    return true; // 返回设备是否在线
                }
            }
            return false;
        }

        //应用列表实时刷新
        private delegate void RefreshAppListDelegate();

        private async void RefreshAppList()
        {
            Log("正在刷新应用列表...");

            // 异步获取应用列表
            var packageTask = Task.Run(() =>
            {
                appList.Clear();
                var packages = ExecuteShellCommand("pm list packages -f");
                if (string.IsNullOrEmpty(packages))
                {
                    Log("获取应用列表失败");
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
                        appType = "系统应用";
                    }
                    else if (packagePath.StartsWith("/data/"))
                    {
                        appType = "用户应用";
                    }
                    else
                    {
                        appType = "未知应用";
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

            // 更新显示列表和记录日志
            BeginInvoke(new RefreshAppListDelegate(() =>
            {
                ShowAppList();
                Log($"应用列表已经刷新，共有 {appList.Count} 个应用");
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
        //异步卸载操作

        private async Task UninstallSelectedAppsAsync()
        {
            Log($"卸载操作开始...");
            try
            {
                var selectedItems = appListView.CheckedItems;
                if (selectedItems.Count == 0)
                {
                    MessageBox.Show("请至少选中一个应用", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Log($"未选中任何应用，卸载操作终止。");
                    return;
                }
                foreach (ListViewItem item in selectedItems)
                {
                    var packageName = item.SubItems[2].Text;
                    if (!string.IsNullOrEmpty(packageName))
                    {
                        Log($"包 {packageName} 正在卸载中...");
                        var result = await Task.Run(() => ExecuteShellCommand($"pm uninstall --user 0 {packageName}"));
                        if (result.Contains("Success"))
                        {
                            Log($"{item.SubItems[1].Text} 已卸载");
                        }
                        else
                        {
                            Log($"卸载失败：{result}");
                        }
                    }
                }
                Log($"卸载操作结束。");
                RefreshAppList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Log($"卸载操作失败：{ex.Message}");
            }
        }





        private async Task InstallApkFilesAsync(string[] apkFiles)
        {
            OnOperationStarted("安装操作开始...");

            foreach (var filePath in apkFiles)
            {
                if (File.Exists(filePath))
                {
                    try
                    {
                        Log($"APK {filePath} 正在安装中...");
                        var result = await Task.Run(() =>
                        {
                            // 执行安装命令
                            var arguments = $"install \"{filePath}\"";
                            return ExecuteCommand("adb", arguments);
                        });

                        if (result.Contains("Success"))
                        {
                            Log($"APK {filePath} 安装成功");
                        }
                        else
                        {
                            Log($"安装失败：{result}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log($"安装失败：{ex.Message}");
                    }
                }
                else
                {
                    Log($"文件不存在：{filePath}");
                }
            }
            OnOperationFinished("安装操作结束。");
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
                // 设备未连接在线，返回空字符串
                return string.Empty;
            }

            var receiver = new ConsoleOutputReceiver();
            try
            {
                _adbClient.ExecuteRemoteCommand(command, _device, receiver);
            }
            catch (Exception ex)
            {
                Log($"执行命令 {command} 失败：{ex.Message}");
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
                Filter = "APK 文件|*.apk"
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                await InstallApkFilesAsync(openFileDialog.FileNames);
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            appListView.Columns.Add("应用类型", 140);
            appListView.Columns.Add("应用地址", 300);
            appListView.Columns.Add("包名", 300);
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
            MessageBox.Show("微信公众号：慕研" + Environment.NewLine + "QQ:2535093954", "联系作者", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void OpenUrl(string url)
        {
            // 获取注册表中的默认浏览器

            try
            {
                // 尝试打开默认浏览器并访问链接
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
                MessageBox.Show("无法打开指定网页，链接已复制，请手动打开！");
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
