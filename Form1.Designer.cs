namespace AdbUninstaller
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            statusText = new Label();
            statusButton = new Button();
            uninstallButton = new Button();
            installButton = new Button();
            searchBox = new TextBox();
            logBox = new RichTextBox();
            appListView = new ListView();
            RefreshAppListbtn = new Button();
            groupLabel = new LinkLabel();
            githubLabel = new LinkLabel();
            AuthorLabel = new LinkLabel();
            SuspendLayout();
            // 
            // statusText
            // 
            statusText.AutoSize = true;
            statusText.Location = new Point(12, 847);
            statusText.Name = "statusText";
            statusText.Size = new Size(82, 24);
            statusText.TabIndex = 0;
            statusText.Text = "连接状态";
            // 
            // statusButton
            // 
            statusButton.Location = new Point(901, 783);
            statusButton.Name = "statusButton";
            statusButton.Size = new Size(178, 48);
            statusButton.TabIndex = 1;
            statusButton.Text = "连接状态检测";
            statusButton.UseVisualStyleBackColor = true;
            statusButton.Click += statusButton_Click;
            // 
            // uninstallButton
            // 
            uninstallButton.BackColor = Color.Brown;
            uninstallButton.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            uninstallButton.ForeColor = SystemColors.ButtonFace;
            uninstallButton.Location = new Point(1303, 783);
            uninstallButton.Name = "uninstallButton";
            uninstallButton.Size = new Size(139, 49);
            uninstallButton.TabIndex = 2;
            uninstallButton.Text = "卸载选中";
            uninstallButton.UseVisualStyleBackColor = false;
            uninstallButton.Click += uninstallButton_Click;
            // 
            // installButton
            // 
            installButton.Location = new Point(1466, 784);
            installButton.Name = "installButton";
            installButton.Size = new Size(132, 48);
            installButton.TabIndex = 3;
            installButton.Text = "安装应用";
            installButton.UseVisualStyleBackColor = true;
            installButton.Click += installButton_Click;
            // 
            // searchBox
            // 
            searchBox.ForeColor = Color.DimGray;
            searchBox.Location = new Point(901, 738);
            searchBox.Multiline = true;
            searchBox.Name = "searchBox";
            searchBox.Size = new Size(697, 30);
            searchBox.TabIndex = 5;
            searchBox.Text = "在此处搜索，回车返回搜索结果";
            searchBox.TextChanged += searchBox_TextChanged;
            // 
            // logBox
            // 
            logBox.Location = new Point(901, 12);
            logBox.Name = "logBox";
            logBox.ScrollBars = RichTextBoxScrollBars.ForcedBoth;
            logBox.Size = new Size(697, 711);
            logBox.TabIndex = 6;
            logBox.Text = "";
            // 
            // appListView
            // 
            appListView.CheckBoxes = true;
            appListView.Location = new Point(12, 12);
            appListView.Name = "appListView";
            appListView.Size = new Size(863, 819);
            appListView.TabIndex = 7;
            appListView.UseCompatibleStateImageBehavior = false;
            appListView.View = View.Details;
            // 
            // RefreshAppListbtn
            // 
            RefreshAppListbtn.Location = new Point(1103, 783);
            RefreshAppListbtn.Name = "RefreshAppListbtn";
            RefreshAppListbtn.Size = new Size(172, 48);
            RefreshAppListbtn.TabIndex = 8;
            RefreshAppListbtn.Text = "刷新应用列表";
            RefreshAppListbtn.UseVisualStyleBackColor = true;
            RefreshAppListbtn.Click += RefreshAppListbtn_Click;
            // 
            // groupLabel
            // 
            groupLabel.AutoSize = true;
            groupLabel.Location = new Point(1378, 856);
            groupLabel.Name = "groupLabel";
            groupLabel.Size = new Size(100, 24);
            groupLabel.TabIndex = 9;
            groupLabel.TabStop = true;
            groupLabel.Text = "加入交流群";
            groupLabel.LinkClicked += groupLabel_LinkClicked;
            // 
            // githubLabel
            // 
            githubLabel.AutoSize = true;
            githubLabel.Location = new Point(1498, 856);
            githubLabel.Name = "githubLabel";
            githubLabel.Size = new Size(100, 24);
            githubLabel.TabIndex = 10;
            githubLabel.TabStop = true;
            githubLabel.Text = "获取最新版";
            githubLabel.LinkClicked += githubLabel_LinkClicked;
            // 
            // AuthorLabel
            // 
            AuthorLabel.AutoSize = true;
            AuthorLabel.Location = new Point(1275, 856);
            AuthorLabel.Name = "AuthorLabel";
            AuthorLabel.Size = new Size(82, 24);
            AuthorLabel.TabIndex = 11;
            AuthorLabel.TabStop = true;
            AuthorLabel.Text = "联系作者";
            AuthorLabel.LinkClicked += AuthorLabel_LinkClicked;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1623, 896);
            Controls.Add(AuthorLabel);
            Controls.Add(githubLabel);
            Controls.Add(groupLabel);
            Controls.Add(RefreshAppListbtn);
            Controls.Add(appListView);
            Controls.Add(logBox);
            Controls.Add(searchBox);
            Controls.Add(installButton);
            Controls.Add(uninstallButton);
            Controls.Add(statusButton);
            Controls.Add(statusText);
            Name = "Form1";
            Text = "ADBUninstaller v0.1.1";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label statusText;
        private Button statusButton;
        private Button uninstallButton;
        private Button installButton;
        private TextBox searchBox;
        private RichTextBox logBox;
        private ListView appListView;
        private Button RefreshAppListbtn;
        private LinkLabel groupLabel;
        private LinkLabel githubLabel;
        private LinkLabel AuthorLabel;
    }
}