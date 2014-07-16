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
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

using System.Windows.Forms;
using LoLLauncher;
namespace LoLAutoQueue
{
    class LoLLauncherClient
    {
        [DllImport("user32.dll")]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
        #region Initial Vars
        public LoLLauncher.RiotObjects.Platform.Clientfacade.Domain.LoginDataPacket loginPacket = new LoLLauncher.RiotObjects.Platform.Clientfacade.Domain.LoginDataPacket();
        public LoLLauncher.RiotObjects.Platform.Game.GameDTO currentGame = new LoLLauncher.RiotObjects.Platform.Game.GameDTO();
        public List<LoLLauncher.RiotObjects.Platform.Catalog.Champion.ChampionDTO> availableChamps = new List<LoLLauncher.RiotObjects.Platform.Catalog.Champion.ChampionDTO>();
        public LoLLauncher.Region region = new LoLLauncher.Region();
        public LoLConnection connection = new LoLConnection();
        public bool firstTimeInLobby = true;
        public bool firstTimeInQueuePop = true;
        public bool firstTimeInCustom = true;
        public int timer = 0;
        public System.Diagnostics.Process exeProcess;
        public String userName { get; set; }
        public String password { get; set; }
        public String log { get; set; }
        public String status { get; set; }
        public QueueTypes queueType { get; set; }
        public String installPath = "notfound";
        public LoLLauncher.RiotObjects.Platform.Catalog.Champion.ChampionDTO[] availableChampsArray;
        public IntPtr panelHandle;
        #endregion
        #region Constructor
        #endregion
        #region GUI Functions
        public LoLLauncherClient(String username, String pwd, LoLLauncher.Region region, QueueTypes queuetype, IntPtr panelhnd, String installExe)
        {
            panelHandle = panelhnd;
            userName = username;
            password = pwd;
            queueType = queuetype;
            connection.OnConnect += connection_OnConnect;
            connection.OnDisconnect += connection_OnDisconnect;
            connection.OnError += connection_OnError;
            connection.OnLogin += connection_OnLogin;
            connection.OnLoginQueueUpdate += connection_OnLoginQueueUpdate;
            connection.OnMessageReceived += connection_OnMessageReceived;
            installPath = installExe;
            updateStatus(": Init");
        }
        #endregion
        #region Events
        async void RegisterNotifications()
        {
            await connection.Subscribe("bc", connection.AccountID());
            await connection.Subscribe("cn", connection.AccountID());
            await connection.Subscribe("gn", connection.AccountID());
        }
        private void updateStatus(String update)
        {
            log += DateTime.Now + " : " + update + "\n";
            status = userName + " : " + update + "\n";
        }
        public void Connect()
        {
            if (!connection.IsConnected())
            {
                connection.Connect(userName, password, region, Properties.Settings.Default.clientVer);
            }
        }
        public void Disconnect()
        {
            if (connection.IsConnected())
            {
                updateStatus("Disconnecting");
                connection.Disconnect();
                while (connection.IsConnected())
                {
                    connection.Disconnect();
                    System.Threading.Thread.Sleep(100);
                }
            }
        }
        public async void connection_OnMessageReceived(object sender, object message)
        {
            if (message.ToString().Contains("LobbyStatus"))
            {
                String metaData = (String)((TypedObject)message)["gameMetaData"];
                Match match = Regex.Match(metaData, "gameId\":([0-9]+),");
                if (match.Success)
                {
                    string gameId = match.Groups[1].Value;
                    await connection.SwitchTeams(Convert.ToDouble(gameId));
                }
            } 
            else if (message is LoLLauncher.RiotObjects.Platform.Game.GameDTO)
            {
                LoLLauncher.RiotObjects.Platform.Game.GameDTO game = message as LoLLauncher.RiotObjects.Platform.Game.GameDTO;
                switch (game.GameState)
                {
                    case "IDLE":
                        break;
                    case "TEAM_SELECT":

                        if (firstTimeInCustom)
                        {
                            updateStatus("Entering champion selection");
                            await connection.StartChampionSelection(game.Id, game.OptimisticLock);
                            firstTimeInCustom = false;
                        }
                        break;
                    case "CHAMP_SELECT":
                        if (firstTimeInLobby)
                        {
                            updateStatus("Champion Select - Waiting for game to start");
                            await connection.SetClientReceivedGameMessage(game.Id, "CHAMP_SELECT_CLIENT");
                            if (queueType == QueueTypes.BOT || queueType == QueueTypes.CUSTOM)
                            {
                                await connection.SelectChampion(availableChampsArray[0].ChampionId);
                                await connection.ChampionSelectCompleted();
                            }
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
                        updateStatus("Re-entering queue due to someone dodging");
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
                            updateStatus("Queue popped");
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
                LoLLauncher.RiotObjects.Platform.Game.PlayerCredentialsDto credentials = message as LoLLauncher.RiotObjects.Platform.Game.PlayerCredentialsDto;
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.CreateNoWindow = false;
                startInfo.WorkingDirectory = installPath;
                startInfo.FileName = "League of Legends.exe";
                startInfo.Arguments = "\"8394\" \"LoLLauncher.exe\" \"\" \"" + credentials.ServerIp + " " +
                    credentials.ServerPort + " " + credentials.EncryptionKey + " " + credentials.SummonerId + "\"";
                updateStatus("Launching/playing League of Legends");
                new Thread(() =>
                {
                    exeProcess = System.Diagnostics.Process.Start(startInfo);
                    while (exeProcess.MainWindowHandle == IntPtr.Zero) { }
                    SetParent(exeProcess.MainWindowHandle, panelHandle);
                    MoveWindow(exeProcess.MainWindowHandle, 0, 0, 600, 400, true);
                }).Start();
            }
            else if (message is LoLLauncher.RiotObjects.Platform.Game.Message.GameNotification)
            {
            }
            else if (message is LoLLauncher.RiotObjects.Platform.Matchmaking.SearchingForMatchNotification)
            {
            }
            else if (message is LoLLauncher.RiotObjects.Platform.Statistics.EndOfGameStats)
            {
                if (queueType == QueueTypes.CUSTOM)
                {

                }
                else
                {
                    updateStatus("Joining Queue");
                    LoLLauncher.RiotObjects.Platform.Matchmaking.MatchMakerParams matchParams = new LoLLauncher.RiotObjects.Platform.Matchmaking.MatchMakerParams();
                    if (queueType == QueueTypes.BOT)
                    {
                        matchParams.BotDifficulty = "EASY";
                    }
                    matchParams.QueueIds = new Int32[1] { (int)queueType };
                    LoLLauncher.RiotObjects.Platform.Matchmaking.SearchingForMatchNotification m = await connection.AttachToQueue(matchParams);
                    if (m.PlayerJoinFailures == null)
                    {
                        updateStatus("Joined Queue");
                    }
                    else
                    {
                        updateStatus("Couldn't enter Q - " + m.PlayerJoinFailures.Summoner.Name + " : " + m.PlayerJoinFailures.ReasonFailed);
                    }
                }
            }
            else
            {
                if (message.ToString().Contains("EndOfGameStats"))
                {
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
                /*FileSystemWatcher watcher = new FileSystemWatcher();
                watcher.Path = @"..\Common\";
                watcher.Filter = "summoner_" + loginPacket.AllSummonerData.Summoner.Name + "_finished.txt";
                watcher.NotifyFilter = NotifyFilters.LastAccess |
                         NotifyFilters.LastWrite |
                         NotifyFilters.FileName |
                         NotifyFilters.DirectoryName;
                watcher.Created += watcher_Created;
                watcher.EnableRaisingEvents = true;*/
                updateStatus("Starting at lvl " + loginPacket.AllSummonerData.SummonerLevel.Level);
                updateStatus("Starting with " + loginPacket.IpBalance + "ip");
                LoLLauncher.RiotObjects.Platform.Matchmaking.GameQueueConfig[] availableQueues = await connection.GetAvailableQueues();
                LoLLauncher.RiotObjects.Platform.Summoner.Boost.SummonerActiveBoostsDTO boosts = await connection.GetSumonerActiveBoosts();
                //LoLLauncher.RiotObjects.Platform.Leagues.Client.Dto.SummonerLeagueItemAndProgresssDTO leaguePosProg = await connection.GetMyLeaguePositionsAndProgress();
                availableChampsArray = await connection.GetAvailableChampions();
                LoLLauncher.RiotObjects.Platform.Summoner.Runes.SummonerRuneInventory sumRuneInven = await connection.GetSummonerRuneInventory(loginPacket.AllSummonerData.Summoner.SumId);
                LoLLauncher.RiotObjects.Platform.Leagues.Client.Dto.SummonerLeagueItemsDTO leaguePos = await connection.GetMyLeaguePositions();
                object preferences = await connection.LoadPreferencesByKey("KEY BINDINGS", 1, false);
                LoLLauncher.RiotObjects.Platform.Summoner.Masterybook.MasteryBookDTO masteryBook = await connection.GetMasteryBook(loginPacket.AllSummonerData.Summoner.SumId);
                LoLLauncher.RiotObjects.Team.Dto.PlayerDTO player = await connection.CreatePlayer();

                if (loginPacket.ReconnectInfo != null && loginPacket.ReconnectInfo.Game != null)
                {
                    updateStatus("Reconnecting to game");
                    connection_OnMessageReceived(sender, loginPacket.ReconnectInfo.PlayerCredentials);
                }
                else
                {
                    if (queueType == QueueTypes.CUSTOM)
                    {
                        updateStatus("Creating custom game");
                        LoLLauncher.RiotObjects.Platform.Game.PracticeGameConfig cfg = new LoLLauncher.RiotObjects.Platform.Game.PracticeGameConfig();
                        cfg.GameName = "funtime lol" + new Random().Next().ToString();
                        LoLLauncher.RiotObjects.Platform.Game.Map.GameMap map = new LoLLauncher.RiotObjects.Platform.Game.Map.GameMap();
                        map.Description = "desc";
                        map.DisplayName = "dummy";
                        map.TotalPlayers = 10;
                        map.Name = "dummy";
                        map.MapId = (int)GameMode.SummonersRift;
                        map.MinCustomPlayers = 1;
                        cfg.GameMap = map;
                        cfg.MaxNumPlayers = 10;
                        cfg.GameTypeConfig = 1;
                        cfg.AllowSpectators = "NONE";
                        cfg.GameMode = StringEnum.GetStringValue(GameMode.SummonersRift);
                        await connection.CreatePracticeGame(cfg);
                    }
                    else
                    {
                        LoLLauncher.RiotObjects.Platform.Statistics.EndOfGameStats eog = new LoLLauncher.RiotObjects.Platform.Statistics.EndOfGameStats();
                        connection_OnMessageReceived(sender, eog);
                    }
                }
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
            updateStatus("Err : " + error.Message);
        }
        void connection_OnDisconnect(object sender, EventArgs e)
        {
            updateStatus("Disconnected");
        }
        void connection_OnConnect(object sender, EventArgs e)
        {
            updateStatus("Connected");
        }
        #endregion

    }
}
