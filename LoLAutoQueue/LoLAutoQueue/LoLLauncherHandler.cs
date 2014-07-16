using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
        private Dictionary<String, Panel> panels = new Dictionary<String, Panel>();
        public LoLLauncherHandler()
        {
            InitializeComponent();
        }

        private void LoLLauncherHandler_Load(object sender, EventArgs e)
        {
            EditCfg();
            if (System.IO.File.Exists("acc.txt"))
            {
                LoadFile("acc.txt");
            }
            MenuItem restart = new MenuItem("Restart");
            MenuItem stop = new MenuItem("Stop");
            restart.Click += restart_Click;
            stop.Click += stop_Click;
            menuItems = new MenuItem[]{restart, stop};
            BeginWork();
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
        void restart_Click(object sender, EventArgs e)
        {
            LoLLauncherClient client = (LoLLauncherClient)accountList.SelectedItem;
            client.Disconnect();
            client.Connect();
        }
        void stop_Click(object sender, EventArgs e)
        {
            LoLLauncherClient client = (LoLLauncherClient)accountList.SelectedItem;
            client.Disconnect();
        }

        private async void BeginWork()
        {
            while (true)
            {
                await Wait();
                List<LoLLauncherClient> clients = new List<LoLLauncherClient>();
                int oldIndex = accountList.SelectedIndex;
                foreach (object acc in accountList.Items)
                {
                    LoLLauncherClient account = (LoLLauncherClient)acc;
                    clients.Add(account);
                }
                accountList.Items.Clear();
                foreach (LoLLauncherClient client in clients)
                {
                    accountList.Items.Add(client);
                }
                accountList.SelectedIndex = oldIndex;

            }
        }

        private async Task Wait()
        {
            await Task.Delay(300);
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
                ContextMenu buttonMenu = new ContextMenu(menuItems);
                buttonMenu.Show(accountList, e.Location);
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
    }
}
