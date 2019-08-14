using DrawGuess.Exceptions;
using DrawGuess.Helpers;
using ExitGames.Client.Photon;
using Photon.Realtime;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.GroupsModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Input.Inking;

namespace DrawGuess.Models
{
    public enum GameMode : int
    {
        WaitingForPlayers = 1,
        StartingGame,
        StartingRound,
        RevealingRoles,
        Playing,
        EndingRound,
        EndingGame,
        PainterLeft
    }

    public class Game : INotifyPropertyChanged
    {
        private int id;
        public int Id
        {
            get { return this.id; }
            set
            {
                this.id = value;
                this.OnPropertyChanged();
            }
        }

        private string name;
        public string Name
        {
            get { return this.name; }
            set
            {
                this.name = value;
                this.OnPropertyChanged();
            }
        }

        private bool full;
        public bool Full
        {
            get { return this.full; }
            set
            {
                this.full = value;
                this.OnPropertyChanged();
            }
        }

        private string secretWord;
        public string SecretWord
        {
            get { return this.secretWord; }
            set
            {
                this.secretWord = value;
                this.OnPropertyChanged();
            }
        }

        private string randomLetters;
        public string RandomLetters
        {
            get { return this.randomLetters; }
            set
            {
                this.randomLetters = value;
                this.OnPropertyChanged();
            }
        }

        private int round;
        public int Round
        {
            get { return this.round; }
            set
            {
                this.round = value;
                this.OnPropertyChanged();
            }
        }

        private GameMode mode;
        public GameMode Mode
        {
            get { return mode; }
            set
            {
                if (!(mode.Equals(value)))
                {
                    mode = value;

                    if (LoadBalancingClient.LocalPlayer.CustomProperties.ContainsKey("painter"))
                    {
                        if ((bool)LoadBalancingClient.LocalPlayer.CustomProperties["painter"])
                        {
                            ChangeMode();
                        }
                    }
                }
                else
                {
                    mode = value;
                }
                this.OnPropertyChanged();
            }
        }

        private int numberOfPlayers;
        public int NumberOfPlayers
        {
            get { return numberOfPlayers; }
            set
            {
                numberOfPlayers = value;
                if (this.NumberOfPlayers >= 8) { Full = true; }
                else { Full = false; }
                this.OnPropertyChanged();
            }
        }

        private bool stopTasks;
        public bool StopTasks
        {
            get { return this.stopTasks; }
            set
            {
                this.stopTasks = value;
                this.OnPropertyChanged();
            }
        }
        
        private int timer;
        public int Timer
        {
            get { return timer; }
            set
            {
                timer = value;
                this.OnPropertyChanged();
            }
        }

        public bool LeftRoom = false;
        public bool ChangingMode = false;
        private LoadBalancingClient LoadBalancingClient = (App.Current as App).LoadBalancingClient;
        
        public Game()
        {
            LoadBalancingClient.MatchMakingCallbackTargets.LeftRoom += RoomLeft;
            Timer = 0;
        }

        private void RoomLeft(object sender, EventArgs e)
        {
            LeftRoom = true;
        }

        public ObservableCollection<Player> GetPlayers()
        {
            var players = new ObservableCollection<Player>();

            try
            {
                Dictionary<int, Photon.Realtime.Player> photonPlayers = (App.Current as App).LoadBalancingClient.CurrentRoom.Players;

                foreach (var p in photonPlayers)
                {
                    var player = new Player();

                    if (p.Value.UserId.Equals((App.Current as App).LoadBalancingClient.LocalPlayer.UserId))
                    {
                        player.IsCurrentUser = true;
                    }

                    player.NickName = p.Value.NickName;
                    player.UserId = p.Value.UserId;


                    if (p.Value.CustomProperties.ContainsKey("points"))
                    {
                        player.Points = (int)p.Value.CustomProperties["points"];
                    }
                    else
                    {
                        SetPlayerPoints(0);
                        player.Points = 0;
                    }

                    if (p.Value.CustomProperties.ContainsKey("painter"))
                    {
                        player.Painter = (bool)p.Value.CustomProperties["painter"];
                    }
                    else
                    {
                        player.Painter = false;
                    }

                    if (p.Value.CustomProperties.ContainsKey("was_painter"))
                    {
                        player.WasPainter = (bool)p.Value.CustomProperties["was_painter"];
                    }
                    else
                    {
                        player.WasPainter = false;
                    }

                    if (p.Value.CustomProperties.ContainsKey("correct_guess"))
                    {
                        player.RightAnswer = (bool)p.Value.CustomProperties["correct_guess"];
                    }
                    else
                    {
                        player.RightAnswer = false;
                    }

                    players.Add(player);
                }
            }
            catch (Exception e)
            {
                throw new PhotonException("Could not get players", e);
            }

            return players;
        }

        public static void JoinGame(string gameName)
        {
            var roomParams = new EnterRoomParams()
            {
                RoomName = gameName,
            };

            if (!(App.Current as App).LoadBalancingClient.OpJoinRoom(roomParams))
            {
                throw new PhotonException("Could not join room");
            }
        }

        public void StartGame()
        {
            try
            {
                Round = 1;
                Hashtable customProperties = new Hashtable() {
                    { "mode", GameMode.StartingGame }, //Change game status to started
                    { "round", Round }, //Set round         
                };
                (App.Current as App).LoadBalancingClient.CurrentRoom.SetCustomProperties(customProperties, new Hashtable(), new WebFlags(0) { HttpForward = true });

                SetPainter();
            }
            catch (Exception e)
            {
                throw new PhotonException("Could not start game", e);
            }
        }

        public void ClearGame()
        {
            try
            {
                StopTasks = true;

                Round = 1;
                Hashtable customProperties = new Hashtable() {
                    { "round", Round }, //Set round         
                };
                (App.Current as App).LoadBalancingClient.CurrentRoom.SetCustomProperties(customProperties, new Hashtable(), new WebFlags(0) { HttpForward = true });

                Dictionary<int, Photon.Realtime.Player> photonPlayers = (App.Current as App).LoadBalancingClient.CurrentRoom.Players;
                customProperties = new Hashtable() {
                    { "painter", false },
                    { "points", 0 },   
                };

                foreach (var player in photonPlayers)
                {
                    player.Value.SetCustomProperties(customProperties);
                }
            }
            catch (Exception e)
            {
                throw new PhotonException("Could not start game", e);
            }
        }

        public void NewPlayer()
        {
            try
            {
                Hashtable customProperties = new Hashtable() {
                    { "painter", false },
                    { "points", 0 },
                    { "correct_guess", false },
                    { "was_painter", false }
                };
                (App.Current as App).LoadBalancingClient.LocalPlayer.SetCustomProperties(customProperties);
            }
            catch(Exception e)
            {
                throw new PhotonException("Could not set properties for new player", e);
            }
        }

        public void SetPainter()
        {
            try
            {
                //Check if there are a painter already and remove it              
                Hashtable notPainterProperties = new Hashtable() { { "painter", false }, { "was_painter", true } };
                Dictionary<int, Photon.Realtime.Player> photonPlayers = (App.Current as App).LoadBalancingClient.CurrentRoom.Players;

                foreach (var p in photonPlayers)
                {
                    if (p.Value.CustomProperties.ContainsKey("painter"))
                    {
                        if ((bool)p.Value.CustomProperties["painter"])
                        {
                            //Remove current painter as painter
                            p.Value.SetCustomProperties(notPainterProperties);
                        }
                    }
                }

                //Get players
                var players = GetPlayers();
                //Get players that have not been painters
                var nonPainters = players.Where(x => x.WasPainter == false).ToList();

                Random rand = new Random();
                int newPainterIndex = 0;
                newPainterIndex = rand.Next(players.Count);
                var painter = players[newPainterIndex];

                //If there are players that never been painters yet
                if (nonPainters.Count > 0)
                {
                    painter = nonPainters[newPainterIndex];
                }
                else
                {
                    Hashtable wasPainterProperties = new Hashtable() { { "was_painter", false } };
                    foreach (var p in photonPlayers)
                    {
                        if (p.Value.CustomProperties.ContainsKey("was_painter"))
                        {
                            if ((bool)p.Value.CustomProperties["was_painter"])
                            {
                                //Remove current painter as painter
                                p.Value.SetCustomProperties(wasPainterProperties);
                            }
                        }
                    }
                }

                Hashtable painterProperties = new Hashtable() { { "painter", true } };
                Photon.Realtime.Player newPainter = photonPlayers.Where(x => x.Value.UserId == painter.UserId).FirstOrDefault().Value;
                newPainter.SetCustomProperties(painterProperties);
            }
            catch(Exception e)
            {
                throw new PhotonException("Could not set random painter", e);
            }
        }
        
        public void AddStrokes(byte[] strokes) 
        {
            try
            {
                Hashtable customProperties = new Hashtable() {
                    { "strokes", strokes },
                   // { "newStrokes", newStrokes }
                };
                (App.Current as App).LoadBalancingClient.CurrentRoom.SetCustomProperties(customProperties, new Hashtable(), new WebFlags(0) { HttpForward = true });

            }
            catch (Exception e)
            {
                throw new PhotonException("Could not set strokes", e);
            }
        }

        public void ClearCorrectAnswer()
        {
            try
            {
                //Set painter                
                Hashtable properties = new Hashtable() { { "correct_guess", false } };
                Dictionary<int, Photon.Realtime.Player> photonPlayers = (App.Current as App).LoadBalancingClient.CurrentRoom.Players;

                foreach (var p in photonPlayers)
                {
                    if (p.Value.CustomProperties.ContainsKey("correct_guess"))
                    {
                        //Clear correct guess if true
                        if((bool)p.Value.CustomProperties["correct_guess"])
                        {
                            Photon.Realtime.Player player = photonPlayers.Where(x => x.Value.UserId == p.Value.UserId).FirstOrDefault().Value;
                            player.SetCustomProperties(properties);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new PhotonException("Could not clear info", e);
            }
        }

        public void StartRound(int round)
        {
            try
            {
                //Get secret word and random letters for the hint
                string secretWord = WordHelper.RandomizeSecretWord();
                string randomLetters = WordHelper.SetRandomLetters(secretWord);

                Hashtable customProperties = new Hashtable() {
                    { "secret_word", secretWord }, //Secret word
                    { "random_letters", randomLetters }, //Set random letters
                    { "round", round }, //Set round     
                    { "strokes", new byte[0] } //Clear strokes
                };
                (App.Current as App).LoadBalancingClient.CurrentRoom.SetCustomProperties(customProperties, new Hashtable(), new WebFlags(0) { HttpForward = true });

            }
            catch (Exception e)
            {
                throw new PhotonException("Could not start round", e);
            }
        }

        public async Task SetMode(GameMode mode, int waitingTimeSec)
        {
            try
            {
                int i = 0;
                bool done = false;
                while (!StopTasks && !done)
                {
                    await Task.Delay(100);

                    if (i >= (waitingTimeSec * 10))
                    {
                        done = true;
                    }
                    i++;
                }

                if (!StopTasks)
                {
                    Hashtable customProperties = new Hashtable() {
                    { "mode", mode }
                };
                    LoadBalancingClient.CurrentRoom.SetCustomProperties(customProperties, new Hashtable(), new WebFlags(0) { HttpForward = true });
                }
            }
            catch(Exception e)
            {
                throw new PhotonException("Could not set mode", e);
            }      
        }

        public async Task MoveFromPlayingMode(GameMode mode)
        {
            try
            {
                bool done = false;
                while (!StopTasks && !done)
                {
                    await Task.Delay(100);

                    if (Timer <= 0)
                    {
                        done = true;
                    }
                }

                if (!StopTasks)
                {
                    Hashtable customProperties = new Hashtable() {
                    { "mode", mode }
                };
                    LoadBalancingClient.CurrentRoom.SetCustomProperties(customProperties, new Hashtable(), new WebFlags(0) { HttpForward = true });
                }
            }
            catch(Exception e)
            {
                throw new PhotonException("Could not move from playing mode", e);
            }
        }

        public void SetRound(int round)
        {
            try
            {
                Hashtable customProperties = new Hashtable() { { "round", round } };
                (App.Current as App).LoadBalancingClient.CurrentRoom.SetCustomProperties(customProperties, new Hashtable(), new WebFlags(0) { HttpForward = true });
                Round = round;
            }
            catch(Exception e)
            {
                throw new PhotonException("Could not set round",e);
            }
        }

        public byte[] GetStrokes()
        {
            Room room = (App.Current as App).LoadBalancingClient.CurrentRoom;

            if (room.CustomProperties.ContainsKey("strokes"))
            {
                return (byte[])room.CustomProperties["strokes"];
            }

            return new byte[0]; 
        }

        public void ChangeMode()
        {
            switch (Mode)
            {
                case GameMode.WaitingForPlayers:
                    ClearGame();
                    break;
                case GameMode.StartingGame:                    
                    StopTasks = false;
                    Task startingRoundTask = SetMode(GameMode.StartingRound, 3); //Set game mode to StartingRound                    
                    break;
                case GameMode.StartingRound:                    
                    ClearCorrectAnswer(); //Clear indicators for correct answer from the game before
                    StartRound(Round);
                    if (Round > 1) { SetPainter(); }
                    Task revealTask = SetMode(GameMode.RevealingRoles, 3); //Set game mode to RevealingRoles
                    break;
                case GameMode.RevealingRoles:                    
                    Task playTtask = SetMode(GameMode.Playing, 7); //Set game mode to RevealingRoles
                    break;
                case GameMode.Playing:                    
                    Task endRoundTask = MoveFromPlayingMode(GameMode.EndingRound); //Set game mode to EndingRound
                    break;
                case GameMode.EndingRound:
                    //If round is 8, the game has come to an end
                    if (Round == 8)
                    {                        
                        Task endGameTask = SetMode(GameMode.EndingGame, 3); //Set game mode to end game
                    }
                    else
                    {                        
                        SetRound(Round + 1);
                        Task startNewRoundTask = SetMode(GameMode.StartingRound, 3); //Set game mode to start new round
                    }
                    break;
                case GameMode.EndingGame:                    
                    SetRound(1);
                    NewPlayer(); //Clear player info
                    Task startNewGameTask = SetMode(GameMode.StartingGame, 3); //Set game mode to RevealingRoles
                    break;
                default:
                    break;
            }
        }

        public void StopGame()
        {
            try
            {
                StopTasks = true;
                //Change game status to stopped
                Hashtable customProperties = new Hashtable() { { "mode", GameMode.WaitingForPlayers } };
                (App.Current as App).LoadBalancingClient.CurrentRoom.SetCustomProperties(customProperties);
            }
            catch(Exception e)
            {
                throw new PhotonException("Could not stop game", e);
            }
        }

        public void SetPlayerPoints(int points)
        {
            try
            {
                Hashtable customProperties = new Hashtable() { { "points", points } };
                (App.Current as App).LoadBalancingClient.LocalPlayer.SetCustomProperties(customProperties);
            }
            catch(Exception e)
            {
                throw new PhotonException("Could not set player points", e);
            }
        }

        public void SetPlayerPoints(int points, Models.Player p)
        {
            try
            {
                Hashtable painterProperties = new Hashtable() { { "points", points } };
                Dictionary<int, Photon.Realtime.Player> photonPlayers = (App.Current as App).LoadBalancingClient.CurrentRoom.Players;
                Photon.Realtime.Player newPainter = photonPlayers.Where(x => x.Value.UserId == p.UserId).FirstOrDefault().Value;
                newPainter.SetCustomProperties(painterProperties);
            }
            catch(Exception e)
            {
                throw new PhotonException("Could not set points", e);
            }
        }

        public void SetCorrectAnswer(bool correctGuess)
        {
            try
            {
                Hashtable customProperties = new Hashtable() { { "correct_guess", correctGuess } };
                (App.Current as App).LoadBalancingClient.LocalPlayer.SetCustomProperties(customProperties);
            }
            catch(Exception e)
            {
                throw new PhotonException("Could not set correct answer", e);
            }            
        }

        public static void AddGame(string gameName)
        {
            var roomParams = new EnterRoomParams()
            {
                RoomName = gameName,
                Lobby = new TypedLobby("Lobby1", LobbyType.SqlLobby),
                CreateIfNotExists = true,
                RoomOptions = new RoomOptions()
                {
                    MaxPlayers = 8,
                    IsVisible = true,
                    IsOpen = true,
                    CustomRoomProperties = new Hashtable() {
                        { "C0", 1 },
                        { "mode", GameMode.WaitingForPlayers }
                    },
                    EmptyRoomTtl = 0, //Keep room 0 seconds after the last person leaves room 
                    PlayerTtl = 0, //Keep actor in room 30 seconds after it was disconnected  
                    CustomRoomPropertiesForLobby = new string[] { "C0" }, // this makes "C0" available in the lobby
                    PublishUserId = true,
                }
            };

            if (!(App.Current as App).LoadBalancingClient.OpCreateRoom(roomParams))
            {
                throw new PhotonException("Could not create room");
            }
        }

        public bool UpdateGame()
        {
            bool ChangedSecreWord = false; 

            try
            {
                Room room = (App.Current as App).LoadBalancingClient.CurrentRoom;

                if (Name != room.Name)
                {
                    Name = room.Name.ToUpper();
                }     
                if (room.CustomProperties.ContainsKey("mode"))
                {
                    if(Mode != (GameMode)room.CustomProperties["mode"])
                    {
                        Mode = (GameMode)room.CustomProperties["mode"];
                    }
                }
                if (room.CustomProperties.ContainsKey("round"))
                {
                    if(Round != (int)room.CustomProperties["round"])
                    {
                        Round = (int)room.CustomProperties["round"];
                    }
                }
                if (room.CustomProperties.ContainsKey("random_letters"))
                {
                    if(RandomLetters != (string)room.CustomProperties["random_letters"])
                    {
                        RandomLetters = (string)room.CustomProperties["random_letters"];
                    }
                }
                if (room.CustomProperties.ContainsKey("secret_word"))
                {
                    if (SecretWord != (string)room.CustomProperties["secret_word"])
                    {
                        SecretWord = (string)room.CustomProperties["secret_word"];
                        ChangedSecreWord = true; 
                    }
                }
            }
            catch (Exception e)
            {
                throw new PhotonException("Could not get game", e);
            }

            return ChangedSecreWord; 
        }

        public void LeaveGame()
        {
            try
            {
                if (!(App.Current as App).LoadBalancingClient.OpLeaveRoom(false))
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                throw new PhotonException("Could not leave room");
            }
        }

        public static void GetGames()
        {
            try
            {
                //Get list of game rooms from Photon
                if (!(App.Current as App).LoadBalancingClient.OpGetGameList(new TypedLobby("Lobby1", LobbyType.SqlLobby), "C0=1"))
                {
                    throw new PhotonException();
                }
            }
            catch (Exception)
            {
                throw new PhotonException("Could not get games");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
