using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace LoLAutoQueue
{
    public partial class LoLLauncherHandler : Form
    {
        [DllImport("user32.dll")]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr GetParent(IntPtr hWnd);
        private MenuItem[] menuItems;
        private MenuItem[] menuItemsNone;
        private Dictionary<String, Panel> panels = new Dictionary<String, Panel>();
        public LoLLauncherHandler()
        {
            InitializeComponent();
        }

        private void LoLLauncherHandler_Load(object sender, EventArgs e)
        {
            CheckForUpdate();
            EditCfg();
            if (System.IO.File.Exists("acc.txt"))
            {
                LoadFile("acc.txt");
            }
            SetupMenuItems();
            BeginWork();
        }
        private void SetupMenuItems()
        {
            MenuItem restart = new MenuItem("Restart");
            MenuItem stop = new MenuItem("Stop");
            MenuItem delete = new MenuItem("Delete");
            restart.Click += ((object sender2, EventArgs e2) =>
            {
                ((LoLLauncherClient)accountList.SelectedItem).Disconnect();
                ((LoLLauncherClient)accountList.SelectedItem).Connect();
            });
            stop.Click += ((object sender2, EventArgs e2) =>
            {
                ((LoLLauncherClient)accountList.SelectedItem).Disconnect();
            });
            delete.Click += ((object sender2, EventArgs e2) =>
            {
                accountList.Items.Remove(accountList.SelectedItem);
            });
            MenuItem add = new MenuItem("Add");
            add.Click += ((object sender2, EventArgs e2) =>
            {
                LoLLauncherClient client = new LoLLauncherClient();
                if (AddAccountForm(ref client) == DialogResult.OK)
                {
                    Panel tempPanel = new Panel();
                    if (!panels.ContainsKey(client.userName))
                    {
                        tempPanel.BackColor = Color.Black;
                        tempPanel.Size = gamePanel.Size;
                        tempPanel.Anchor = gamePanel.Anchor;
                        panels.Add(client.userName, tempPanel);
                        client.panelHandle = tempPanel.Handle;
                    }
                    client.installPath = FindLoLExe();
                    accountList.Items.Add(client);
                }
            });
            menuItems = new MenuItem[] { restart, stop, delete };
            menuItemsNone = new MenuItem[] { add };
        }
        private static DialogResult AddAccountForm(ref LoLLauncherClient client)
        {
            Form form = new Form();
            TextBox user = new TextBox();
            TextBox pw = new TextBox();
            ComboBox region = new ComboBox();
            ComboBox queue = new ComboBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();
            user.Text = "Username";
            pw.Text = "Password";
            region.Text = "Region";
            region.Items.AddRange(Enum.GetNames(typeof(LoLLauncher.Region)));
            queue.Text = "Queue";
            queue.Items.AddRange(Enum.GetNames(typeof(LoLLauncher.QueueTypes)));
            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            user.SetBounds(10, 10, 100, 23);
            pw.SetBounds(120, 10, 100, 23);
            region.SetBounds(10, 43, 100, 23);
            queue.SetBounds(120, 43, 100, 23);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] { user, pw, region, queue, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, user.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                client.Init(user.Text, pw.Text, (LoLLauncher.Region)Enum.Parse(typeof(LoLLauncher.Region), region.SelectedItem.ToString()),
                    (LoLLauncher.QueueTypes)Enum.Parse(typeof(LoLLauncher.QueueTypes), queue.SelectedItem.ToString()));
            }
            return dialogResult;
        }
        private void CheckForUpdate()
        {
            WebClient Client = new WebClient();
            //Client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            //String version = Client.DownloadString("https://api.github.com/repos/shalzuth/LoLAutoQueue/releases");
            //version = version.Substring(version.IndexOf("\"browser_download_url\":\"") + "\"browser_download_url\":\"".Length);
            //version = version.Substring(0, version.IndexOf("\""));
            Byte[] newExe = Client.DownloadData("https://github.com/shalzuth/LoLAutoQueue/raw/master/LoLAutoQueue.exe");
            Byte[] oldExe = File.ReadAllBytes("LoLAutoQueue.exe");
            if (!StructuralComparisons.StructuralEqualityComparer.Equals(newExe, oldExe))
            {
                this.Text = this.Text + " (Outdated)";
            }
        }
        private String FindLoLBase()
        {
            String regPath = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall";
            foreach (var item in Microsoft.Win32.Registry.LocalMachine.OpenSubKey(regPath).GetSubKeyNames())
            {
                if (string.Equals(Microsoft.Win32.Registry.LocalMachine.OpenSubKey(regPath + "\\" + item).GetValue("DisplayName"), "League of Legends"))
                {
                    return Microsoft.Win32.Registry.LocalMachine.OpenSubKey(regPath + "\\" + item).GetValue("InstallLocation").ToString();
                }
            }
            regPath = "SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall";
            foreach (var item in Microsoft.Win32.Registry.LocalMachine.OpenSubKey(regPath).GetSubKeyNames())
            {
                if (string.Equals(Microsoft.Win32.Registry.LocalMachine.OpenSubKey(regPath + "\\" + item).GetValue("DisplayName"), "League of Legends"))
                {
                    return Microsoft.Win32.Registry.LocalMachine.OpenSubKey(regPath + "\\" + item).GetValue("InstallLocation").ToString();
                }
            }
            return "notfound";
        }
        private String FindLoLExe()
        {
            String installPath = FindLoLBase();
            if (installPath.Contains("notfound"))
                return installPath;
            installPath += @"RADS\solutions\lol_game_client_sln\releases\";
            installPath = Directory.EnumerateDirectories(installPath).OrderBy(f => new DirectoryInfo(f).CreationTime).Last();
            installPath += @"\deploy\";
            return installPath;
        }
        private String FindLoLCfg()
        {
            String cfgFile = FindLoLBase();
            if (cfgFile.Contains("notfound"))
                return cfgFile;
            cfgFile += @"Config\game.cfg";
            return cfgFile;
        }
        private void EditCfg()
        {
            String cfgFile = FindLoLCfg();
            if (cfgFile.Contains("notfound"))
            {
                MessageBox.Show("Couldn't find LoL path");
                Application.Exit();
            }
            String cfgContents = File.ReadAllText(cfgFile);
            String heightPattern = @"\nHeight=[0-9]+";
            String widthPattern = @"\nWidth=[0-9]+";
            var regex = new Regex(heightPattern, RegexOptions.IgnoreCase);
            cfgContents = regex.Replace(cfgContents, "\nHeight=400");
            regex = new Regex(widthPattern, RegexOptions.IgnoreCase);
            cfgContents = regex.Replace(cfgContents, "\nWidth=600");
            File.WriteAllText(cfgFile, cfgContents);
        }

        private async void BeginWork()
        {
            while (true)
            {
                await Wait();
                String temp = accountList.DisplayMember;
                accountList.DisplayMember = null;
                accountList.DisplayMember = temp;
            }
        }

        private async Task Wait()
        {
            await Task.Delay(1000);
        }

        private void LoLLauncherHandler_FormClosing(object sender, FormClosingEventArgs e)
        {
            for (int i = 0; i < 5; i++)
            {
                foreach (object acc in accountList.Items)
                {
                    LoLLauncherClient account = (LoLLauncherClient)acc;
                    account.Disconnect();
                }
            }
        }

        private void startAllButton_Click(object sender, EventArgs e)
        {
            foreach (object acc in accountList.Items)
            {
                try
                {
                    LoLLauncherClient account = (LoLLauncherClient)acc;
                    account.Connect();
                }
                catch { }
            }
        }

        private void stopAllButton_Click(object sender, EventArgs e)
        {
            foreach (object acc in accountList.Items)
            {
                LoLLauncherClient account = (LoLLauncherClient)acc;
                account.Disconnect();
            }
        }

        private void accountList_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                accountLog.Text = ((LoLLauncherClient)accountList.SelectedItem).log;
                accountStatus.Text = ((LoLLauncherClient)accountList.SelectedItem).status;
                while (accountStatus.Location.X + accountStatus.Size.Width > showLogBox.Location.X)
                {
                    accountStatus.Text = accountStatus.Text.Substring(0, accountStatus.Text.Length - 1);
                }
                Boolean foundPanel = false;
                foreach (Control tempControl in gamePanel.Controls)
                {
                    if (tempControl.Handle == panels[((LoLLauncherClient)accountList.SelectedItem).userName].Handle)
                        foundPanel = true;
                }
                if (foundPanel == false)
                {
                    gamePanel.Controls.Clear();
                    gamePanel.Controls.Add(panels[((LoLLauncherClient)accountList.SelectedItem).userName]);
                }
            }
            catch
            {
                accountLog.Text = "Error";
                accountStatus.Text = "Error";
            }
        }

        private void accountList_MouseDown(object sender, MouseEventArgs e)
        {
            accountList.SelectedIndex = accountList.IndexFromPoint(e.X, e.Y);
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                if (accountList.SelectedIndex >= 0)
                {
                    ContextMenu buttonMenu = new ContextMenu(menuItems);
                    buttonMenu.Show(accountList, e.Location);
                }
                else
                {
                    ContextMenu buttonMenu = new ContextMenu(menuItemsNone);
                    buttonMenu.Show(accountList, e.Location);
                }
            }
        }
        private void LoadFile(String fileName)
        {
            try
            {
                accountList.Items.Clear();
                String[] filelines = File.ReadAllLines(fileName);
                foreach (String line in filelines)
                {
                    if (line.Split(' ').Count() > 3)
                    {
                        LoLLauncher.Region reg = (LoLLauncher.Region)Enum.Parse(typeof(LoLLauncher.Region), line.Split(' ')[2]);
                        LoLLauncher.QueueTypes q = (LoLLauncher.QueueTypes)Enum.Parse(typeof(LoLLauncher.QueueTypes), line.Split(' ')[3]);
                        Panel tempPanel = new Panel();
                        if (!panels.ContainsKey(line.Split(' ')[0]))
                        {
                            tempPanel.BackColor = Color.Black;
                            tempPanel.Size = gamePanel.Size;
                            tempPanel.Anchor = gamePanel.Anchor;
                            panels.Add(line.Split(' ')[0], tempPanel);
                        }
                        else
                        {
                            tempPanel = panels[line.Split(' ')[0]];
                        }
                        LoLLauncherClient acct = new LoLLauncherClient(line.Split(' ')[0], line.Split(' ')[1], reg, q, tempPanel.Handle, FindLoLExe());
                        accountList.Items.Add(acct);
                    }
                }
            }
            catch (Exception ex)
            {
                String serverList = String.Join(", ", Enum.GetNames(typeof(LoLLauncher.Region)));
                String gameList = String.Join(", ", Enum.GetNames(typeof(LoLLauncher.QueueTypes)));
                MessageBox.Show("Error: Incorrect format for input.\nPlease use:\n\tusername password server gamemode\nOne account per line\nServers: " + serverList + "\nGame modes: " + gameList);
            }

        }
        private void loadAccountsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            file.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            if (file.ShowDialog() == DialogResult.OK)
            {
                LoadFile(file.FileName);
            }
        }

        private void showLogBox_CheckedChanged(object sender, EventArgs e)
        {
            accountLog.Visible = showLogBox.Checked;
            gamePanel.Visible = !showLogBox.Checked;
        }

        private void githubToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/shalzuth/LoLAutoQueue");
        }
    }
}
