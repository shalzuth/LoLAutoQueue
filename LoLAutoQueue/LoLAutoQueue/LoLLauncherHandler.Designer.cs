namespace LoLAutoQueue
{
    partial class LoLLauncherHandler
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.accountList = new System.Windows.Forms.ListBox();
            this.accountLog = new System.Windows.Forms.RichTextBox();
            this.startAllButton = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadAccountsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contactToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.shalzuthgmailcomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gamePanel = new System.Windows.Forms.Panel();
            this.accountStatus = new System.Windows.Forms.Label();
            this.stopAllButton = new System.Windows.Forms.Button();
            this.showLogBox = new System.Windows.Forms.CheckBox();
            this.githubToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // accountList
            // 
            this.accountList.DisplayMember = "status";
            this.accountList.FormattingEnabled = true;
            this.accountList.Location = new System.Drawing.Point(12, 27);
            this.accountList.Name = "accountList";
            this.accountList.Size = new System.Drawing.Size(244, 433);
            this.accountList.TabIndex = 0;
            this.accountList.SelectedIndexChanged += new System.EventHandler(this.accountList_SelectedIndexChanged);
            this.accountList.MouseDown += new System.Windows.Forms.MouseEventHandler(this.accountList_MouseDown);
            // 
            // accountLog
            // 
            this.accountLog.Location = new System.Drawing.Point(262, 27);
            this.accountLog.Name = "accountLog";
            this.accountLog.ReadOnly = true;
            this.accountLog.Size = new System.Drawing.Size(600, 400);
            this.accountLog.TabIndex = 1;
            this.accountLog.Text = "";
            this.accountLog.WordWrap = false;
            // 
            // startAllButton
            // 
            this.startAllButton.Location = new System.Drawing.Point(787, 439);
            this.startAllButton.Name = "startAllButton";
            this.startAllButton.Size = new System.Drawing.Size(75, 23);
            this.startAllButton.TabIndex = 2;
            this.startAllButton.Text = "Start All";
            this.startAllButton.UseVisualStyleBackColor = true;
            this.startAllButton.Click += new System.EventHandler(this.startAllButton_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.contactToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(868, 24);
            this.menuStrip1.TabIndex = 5;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadAccountsToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // loadAccountsToolStripMenuItem
            // 
            this.loadAccountsToolStripMenuItem.Name = "loadAccountsToolStripMenuItem";
            this.loadAccountsToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
            this.loadAccountsToolStripMenuItem.Text = "Load Accounts";
            this.loadAccountsToolStripMenuItem.Click += new System.EventHandler(this.loadAccountsToolStripMenuItem_Click);
            // 
            // contactToolStripMenuItem
            // 
            this.contactToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.shalzuthgmailcomToolStripMenuItem,
            this.githubToolStripMenuItem});
            this.contactToolStripMenuItem.Name = "contactToolStripMenuItem";
            this.contactToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.contactToolStripMenuItem.Text = "Contact";
            // 
            // shalzuthgmailcomToolStripMenuItem
            // 
            this.shalzuthgmailcomToolStripMenuItem.Name = "shalzuthgmailcomToolStripMenuItem";
            this.shalzuthgmailcomToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.shalzuthgmailcomToolStripMenuItem.Text = "shalzuth@gmail.com";
            // 
            // gamePanel
            // 
            this.gamePanel.Location = new System.Drawing.Point(262, 27);
            this.gamePanel.Name = "gamePanel";
            this.gamePanel.Size = new System.Drawing.Size(600, 400);
            this.gamePanel.TabIndex = 6;
            // 
            // accountStatus
            // 
            this.accountStatus.AutoSize = true;
            this.accountStatus.Location = new System.Drawing.Point(262, 442);
            this.accountStatus.Name = "accountStatus";
            this.accountStatus.Size = new System.Drawing.Size(0, 13);
            this.accountStatus.TabIndex = 7;
            // 
            // stopAllButton
            // 
            this.stopAllButton.Location = new System.Drawing.Point(706, 439);
            this.stopAllButton.Name = "stopAllButton";
            this.stopAllButton.Size = new System.Drawing.Size(75, 23);
            this.stopAllButton.TabIndex = 9;
            this.stopAllButton.Text = "Stop All";
            this.stopAllButton.UseVisualStyleBackColor = true;
            this.stopAllButton.Click += new System.EventHandler(this.stopAllButton_Click);
            // 
            // showLogBox
            // 
            this.showLogBox.AutoSize = true;
            this.showLogBox.Location = new System.Drawing.Point(626, 442);
            this.showLogBox.Name = "showLogBox";
            this.showLogBox.Size = new System.Drawing.Size(74, 17);
            this.showLogBox.TabIndex = 10;
            this.showLogBox.Text = "Show Log";
            this.showLogBox.UseVisualStyleBackColor = true;
            this.showLogBox.CheckedChanged += new System.EventHandler(this.showLogBox_CheckedChanged);
            // 
            // githubToolStripMenuItem
            // 
            this.githubToolStripMenuItem.Name = "githubToolStripMenuItem";
            this.githubToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.githubToolStripMenuItem.Text = "Github";
            this.githubToolStripMenuItem.Click += new System.EventHandler(this.githubToolStripMenuItem_Click);
            // 
            // LoLLauncherHandler
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(868, 470);
            this.Controls.Add(this.showLogBox);
            this.Controls.Add(this.stopAllButton);
            this.Controls.Add(this.accountStatus);
            this.Controls.Add(this.startAllButton);
            this.Controls.Add(this.accountList);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.gamePanel);
            this.Controls.Add(this.accountLog);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "LoLLauncherHandler";
            this.Text = "Autobots, Roll Out!";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LoLLauncherHandler_FormClosing);
            this.Load += new System.EventHandler(this.LoLLauncherHandler_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox accountList;
        private System.Windows.Forms.RichTextBox accountLog;
        private System.Windows.Forms.Button startAllButton;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadAccountsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem contactToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem shalzuthgmailcomToolStripMenuItem;
        private System.Windows.Forms.Panel gamePanel;
        private System.Windows.Forms.Label accountStatus;
        private System.Windows.Forms.Button stopAllButton;
        private System.Windows.Forms.CheckBox showLogBox;
        private System.Windows.Forms.ToolStripMenuItem githubToolStripMenuItem;

    }
}