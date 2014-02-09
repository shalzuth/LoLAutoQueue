using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;

using System.Windows.Forms;
using LoLLauncher;
namespace LoLAutoQueue
{
    class LoLLauncherClient
    {
        #region Initial Vars
        public LoLLauncher.RiotObjects.Platform.Clientfacade.Domain.LoginDataPacket loginPacket = new LoLLauncher.RiotObjects.Platform.Clientfacade.Domain.LoginDataPacket();
        public LoLLauncher.RiotObjects.Platform.Game.GameDTO currentGame = new LoLLauncher.RiotObjects.Platform.Game.GameDTO();
        public List<LoLLauncher.RiotObjects.Platform.Catalog.Champion.ChampionDTO> availableChamps = new List<LoLLauncher.RiotObjects.Platform.Catalog.Champion.ChampionDTO>();
        public LoLLauncher.Region region = new LoLLauncher.Region();
        public LoLConnection connection = new LoLConnection();
        public RichTextBox statusBox;
        public bool firstTimeInLobby = true;
        public bool firstTimeInQueuePop = true;
        public int timer = 0;
        public System.Diagnostics.Process exeProcess;
        #endregion
        #region Constructor
        #endregion
        #region GUI Functions
        public LoLLauncherClient(String userName, String password, LoLLauncher.Region region, RichTextBox richTextBox)
        {
            statusBox = richTextBox;
            connection.OnConnect += connection_OnConnect;
            connection.OnDisconnect += connection_OnDisconnect;
            connection.OnError += connection_OnError;
            connection.OnLogin += connection_OnLogin;
            connection.OnLoginQueueUpdate += connection_OnLoginQueueUpdate;
            connection.OnMessageReceived += connection_OnMessageReceived;
            connection.Connect(
                userName,
                password,
                region,
                "4.1." + ".foobar");
        }
        #endregion
        #region Events
        async void RegisterNotifications()
        {
            await connection.Subscribe("bc", connection.AccountID());
            await connection.Subscribe("cn", connection.AccountID());
            await connection.Subscribe("gn", connection.AccountID());
        }
        private void updateStatus(String status)
        {
            if (statusBox.InvokeRequired)
            {
                statusBox.Invoke((MethodInvoker)delegate
                {
                    statusBox.Text += DateTime.Now + " : " + status + "\n";
                });
                return;
            }
            Console.WriteLine(DateTime.Now + " : " + status);
        }
        public async void connection_OnMessageReceived(object sender, object message)
        {
            //Console.WriteLine("message received:" + message);
            if (message is LoLLauncher.RiotObjects.Platform.Game.GameDTO)
            {
                LoLLauncher.RiotObjects.Platform.Game.GameDTO game = message as LoLLauncher.RiotObjects.Platform.Game.GameDTO;
                //Console.WriteLine(game.GameState);
                switch (game.GameState)
                {
                    case "IDLE":
                        break;
                    case "TEAM_SELECT":
                        break;
                    case "CHAMP_SELECT":
                        if (firstTimeInLobby)
                        {
                            updateStatus("Champion Select - Waiting for game to start");
                            await connection.SetClientReceivedGameMessage(game.Id, "CHAMP_SELECT_CLIENT");
                            //await connection.SelectChampion(16);
                            //await connection.ChampionSelectCompleted();
                            firstTimeInLobby = false;
                        }
                        break;
                    case "POST_CHAMP_SELECT":
                        break;
                    case "PRE_CHAMP_SELECT":
                        break;
                    case "START_REQUESTED":
                        break;
                    case "GAME_START_CLIENT":
                        break;
                    case "GameClientConnectedToServer":
                        break;
                    case "IN_PROGRESS":
                        break;
                    case "IN_QUEUE":
                        break;
                    case "POST_GAME":
                        break;
                    case "TERMINATED":
                        updateStatus("Re-entering queue due to dodge");
                        firstTimeInQueuePop = true;
                        break;
                    case "TERMINATED_IN_ERROR":
                        break;
                    case "CHAMP_SELECT_CLIENT":
                        break;
                    case "GameReconnect":
                        break;
                    case "GAME_IN_PROGRESS":
                        break;
                    case "JOINING_CHAMP_SELECT":
                        if (firstTimeInQueuePop)
                        {
                            if (game.StatusOfParticipants.Contains("1"))
                            {
                                updateStatus("Accepted Queue");
                                firstTimeInQueuePop = false;
                                firstTimeInLobby = true;
                                await connection.AcceptPoppedGame(true);
                            }
                        }
                        break;
                    case "WAITING":
                        break;
                    case "DISCONNECTED":
                        break;
                    default:
                        break;
                }
            }
            else if (message is LoLLauncher.RiotObjects.Platform.Game.PlayerCredentialsDto)
            {
                String installPath = "notfound";
                String regPath = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall";
                foreach (var item in Microsoft.Win32.Registry.LocalMachine.OpenSubKey(regPath).GetSubKeyNames())
                {
                    if (string.Equals(Microsoft.Win32.Registry.LocalMachine.OpenSubKey(regPath + "\\" + item).GetValue("DisplayName"), "League of Legends"))
                    {
                        installPath = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(regPath + "\\" + item).GetValue("InstallLocation").ToString();
                    }
                }
                regPath = "SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall";
                foreach (var item in Microsoft.Win32.Registry.LocalMachine.OpenSubKey(regPath).GetSubKeyNames())
                {
                    if (string.Equals(Microsoft.Win32.Registry.LocalMachine.OpenSubKey(regPath + "\\" + item).GetValue("DisplayName"), "League of Legends"))
                    {
                        installPath = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(regPath + "\\" + item).GetValue("InstallLocation").ToString();
                    }
                }
                installPath += @"RADS\solutions\lol_game_client_sln\releases\";
                installPath = Directory.EnumerateDirectories(installPath).OrderBy(f => new DirectoryInfo(f).CreationTime).Last();
                installPath += @"\deploy\";
                LoLLauncher.RiotObjects.Platform.Game.PlayerCredentialsDto credentials = message as LoLLauncher.RiotObjects.Platform.Game.PlayerCredentialsDto;
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.CreateNoWindow = false;
                startInfo.WorkingDirectory = installPath;
                startInfo.FileName = "League of Legends.exe";
                startInfo.Arguments = "\"8394\" \"LoLLauncher.exe\" \"\" \"" + credentials.ServerIp + " " +
                    credentials.ServerPort + " " + credentials.EncryptionKey + " " + credentials.SummonerId + "\"";
                updateStatus("Launching League of Legends");
                exeProcess = System.Diagnostics.Process.Start(startInfo);
            }
            else if (message is LoLLauncher.RiotObjects.Platform.Game.Message.GameNotification)
            {
            }
            else if (message is LoLLauncher.RiotObjects.Platform.Matchmaking.SearchingForMatchNotification)
            {
            }
            else if (message is LoLLauncher.RiotObjects.Platform.Statistics.EndOfGameStats)
            {
                LoLLauncher.RiotObjects.Platform.Matchmaking.MatchMakerParams matchParams = new LoLLauncher.RiotObjects.Platform.Matchmaking.MatchMakerParams();
                //matchParams.BotDifficulty = "EASY";
                matchParams.QueueIds = new Int32[1] { 65 };
                LoLLauncher.RiotObjects.Platform.Matchmaking.SearchingForMatchNotification m = await connection.AttachToQueue(matchParams);
                if (m.PlayerJoinFailures == null)
                {
                    updateStatus("Joining Queue");
                }
                else
                {
                    updateStatus("Couldn't enter Q - " + m.PlayerJoinFailures.ReasonFailed);
                    updateStatus(m.PlayerJoinFailures.ReasonFailed);
                    updateStatus(m.PlayerJoinFailures.Summoner.Name);
                }
            }
            else
            {
                //Console.WriteLine("uk:" + message.ToString());
                if (message.ToString().Contains("EndOfGameStats"))
                {
                    //Console.WriteLine("eog:" + message.ToString());
                    LoLLauncher.RiotObjects.Platform.Statistics.EndOfGameStats eog = new LoLLauncher.RiotObjects.Platform.Statistics.EndOfGameStats();
                    connection_OnMessageReceived(sender, eog);
                }
            }
        }

        void connection_OnLoginQueueUpdate(object sender, int positionInLine)
        {
            if (positionInLine > 0)
                updateStatus("Position in line to login: " + positionInLine);
        }

        void connection_OnLogin(object sender, string username, string ipAddress)
        {
            new Thread(async delegate()
            {
                RegisterNotifications();
                loginPacket = await connection.GetLoginDataPacketForUser();
                updateStatus("Logged in on " + loginPacket.AllSummonerData.Summoner.Name);
                FileSystemWatcher watcher = new FileSystemWatcher();
                watcher.Path = @"..\Common\";
                watcher.Filter = "summoner_" + loginPacket.AllSummonerData.Summoner.Name + "_finished.txt";
                watcher.NotifyFilter = NotifyFilters.LastAccess |
                         NotifyFilters.LastWrite |
                         NotifyFilters.FileName |
                         NotifyFilters.DirectoryName;
                watcher.Created += watcher_Created;
                watcher.EnableRaisingEvents = true;
                if (loginPacket.ReconnectInfo != null && loginPacket.ReconnectInfo.Game != null)
                {
                    connection_OnMessageReceived(sender, loginPacket.ReconnectInfo.PlayerCredentials);
                }
                else
                {
                    LoLLauncher.RiotObjects.Platform.Statistics.EndOfGameStats eog = new LoLLauncher.RiotObjects.Platform.Statistics.EndOfGameStats();
                    connection_OnMessageReceived(sender, eog);
                }
                /*
                UpdateLevel(loginPacket.AllSummonerData.SummonerLevel.Level);
                UpdateIpRpBalance(loginPacket.IpBalance, loginPacket.RpBalance);
                //customGamePage.addToCustomGameTypes(loginPacket.GameTypeConfigs);
                LoLLauncher.RiotObjects.Platform.Matchmaking.GameQueueConfig[] availableQueues = await connection.GetAvailableQueues();
                UpdateQueues(availableQueues);
                LoLLauncher.RiotObjects.Platform.Summoner.Boost.SummonerActiveBoostsDTO boosts = await connection.GetSumonerActiveBoosts();
                //RiotObjects.Platform.Leagues.Client.Dto.SummonerLeagueItemAndProgresssDTO leaguePosProg = await connection.GetMyLeaguePositionsAndProgress();
                LoLLauncher.RiotObjects.Platform.Catalog.Champion.ChampionDTO[] availableChampsArray = await connection.GetAvailableChampions();
                /*availableChamps = availableChampsArray.ToList();
                availableChamps.Sort(delegate(LoLLauncher.RiotObjects.Platform.Catalog.Champion.ChampionDTO one, LoLLauncher.RiotObjects.Platform.Catalog.Champion.ChampionDTO two)
                {
                    return LoLLauncher.Assets.ChampNames.Get(one.ChampionId).CompareTo(LoLLauncher.Assets.ChampNames.Get(two.ChampionId));
                });
                LoLLauncher.RiotObjects.Platform.Summoner.Runes.SummonerRuneInventory sumRuneInven = await connection.GetSummonerRuneInventory(loginPacket.AllSummonerData.Summoner.SumId);
                LoLLauncher.RiotObjects.Platform.Leagues.Client.Dto.SummonerLeagueItemsDTO leaguePos = await connection.GetMyLeaguePositions();
                object preferences = await connection.LoadPreferencesByKey("KEY BINDINGS", 1, false);
                LoLLauncher.RiotObjects.Platform.Summoner.Masterybook.MasteryBookDTO masteryBook = await connection.GetMasteryBook(loginPacket.AllSummonerData.Summoner.SumId);
                LoLLauncher.RiotObjects.Team.Dto.PlayerDTO player = await connection.CreatePlayer();*/
            }).Start();
        }

        void watcher_Created(object sender, FileSystemEventArgs e)
        {
            updateStatus("Game ended.");
            exeProcess.Kill();
            if (exeProcess.HasExited)
            {
                File.Delete(@"..\Common\summoner_" + loginPacket.AllSummonerData.Summoner.Name + "_finished.txt");
            }
        }
        void connection_OnError(object sender, Error error)
        {
            updateStatus("error received:" + error.Message);
        }
        void connection_OnDisconnect(object sender, EventArgs e)
        {
            //Console.WriteLine("disconnected");
        }
        void connection_OnConnect(object sender, EventArgs e)
        {
            //updateStatus("Connected");
        }
        #endregion
        #region GUI Altering Funcs
        private static bool updatedQueues = false;
        private void UpdateQueues(LoLLauncher.RiotObjects.Platform.Matchmaking.GameQueueConfig[] availableQueues)
        {
            updatedQueues = true;
            int x = 0;
            int y = 0;
            int xSize = 300;
            int ySize = 30;
            List<LoLLauncher.RiotObjects.Platform.Matchmaking.GameQueueConfig> qList = availableQueues.ToList();
            qList.Sort(delegate(LoLLauncher.RiotObjects.Platform.Matchmaking.GameQueueConfig one, LoLLauncher.RiotObjects.Platform.Matchmaking.GameQueueConfig two)
            {
                return one.Id.CompareTo(two.Id);
            });
            foreach (LoLLauncher.RiotObjects.Platform.Matchmaking.GameQueueConfig queue in qList)
            {
                Console.WriteLine("queue info:");
                Console.WriteLine(queue.CacheName);
                Console.WriteLine(queue.GameTypeConfigId);
                Console.WriteLine(queue.Id);
                Console.WriteLine(queue.MinLevel);
                Console.WriteLine("");
                /*
                MetroFramework.Controls.MetroTile tile = new MetroFramework.Controls.MetroTile();
                tile.Location = new Point(x, y);
                tile.Size = new Size(xSize, ySize);
                if (y + 2*ySize + 5 > matchmakingTab.Size.Height)
                {
                    y = 0;
                    x += xSize + 5;
                }
                else
                {
                    y += ySize + 5;
                }
                tile.Text = queue.CacheName.Substring(15);
                tile.Text = tile.Text.Substring(0, tile.Text.Length - 11);
                tile.Tag = queue.Id;
                tile.MouseClick += tile_MouseClick;
                //Console.WriteLine("[StringValue(\"" + tile.Text + "\")]");
                //Console.WriteLine(tile.Text.Replace("-","") + " = " + queue.Id + ",");
                Console.WriteLine("{\"" + tile.Text + "\", " + queue.Id + "},");
                //Console.WriteLine(tile.Text.Replace("-","") + " = " + queue.Id + ",");
                this.matchmakingTab.Controls.Add(tile);*/
            }
        }
        private void UpdateLevel(double level)
        {
        }
        private void UpdateIpRpBalance(double IpBalance, double RpBalance)
        {
        }
        
        #endregion

    }
}
