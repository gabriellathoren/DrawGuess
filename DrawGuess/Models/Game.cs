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
        EndingGame
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

        public bool LeftRoom = false;
        private LoadBalancingClient LoadBalancingClient = (App.Current as App).LoadBalancingClient;

        public Game()
        {
            LoadBalancingClient.MatchMakingCallbackTargets.LeftRoom += RoomLeft;
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
                //Get secret word and random letters for the hint
                string secretWord = WordHelper.RandomizeSecretWord();
                string randomLetters = WordHelper.SetRandomLetters(secretWord);

                Hashtable customProperties = new Hashtable() {
                    { "mode", GameMode.StartingGame }, //Change game status to started
                    { "secret_word", secretWord }, //Secret word
                    { "random_letters", randomLetters }, //Set random letters
                    { "round", 1 }, //Set round              
                };
                (App.Current as App).LoadBalancingClient.CurrentRoom.SetCustomProperties(customProperties, new Hashtable(), new WebFlags(0) { HttpForward = true });

                //Set current user to painter
                Hashtable playerProperties = new Hashtable() { { "painter", true } };
                (App.Current as App).LoadBalancingClient.LocalPlayer.SetCustomProperties(playerProperties);
            }
            catch (Exception e)
            {
                throw new PhotonException("Could not start game", e);
            }
        }

        public async Task SetMode(GameMode mode, int waitingTime)
        {
            await Task.Delay(waitingTime);
            
            Hashtable customProperties = new Hashtable() {
                { "mode", mode }
            };
            (App.Current as App).LoadBalancingClient.CurrentRoom.SetCustomProperties(customProperties, new Hashtable(), new WebFlags(0) { HttpForward = true });
        }

        public void SetRound(int round)
        {
            Hashtable customProperties = new Hashtable() { { "round", round } };
            (App.Current as App).LoadBalancingClient.CurrentRoom.SetCustomProperties(customProperties, new Hashtable(), new WebFlags(0) { HttpForward = true });
        }

        public void ChangeMode()
        {
            switch (Mode)
            {
                case GameMode.WaitingForPlayers:
                    //TODO: Stop all change mode tasks
                    break;
                case GameMode.StartingGame:
                    //Set game mode to StartingRound
                    StartGame();
                    Task startTask = SetMode(GameMode.StartingRound, 7000);
                    break;
                case GameMode.StartingRound:
                    //Set game mode to RevealingRoles
                    Task revealTask = SetMode(GameMode.RevealingRoles, 7000);
                    break;
                case GameMode.RevealingRoles:
                    //Set game mode to RevealingRoles
                    Task playTtask = SetMode(GameMode.Playing, 7000);
                    break;
                case GameMode.Playing:
                    //Set game mode to RevealingRoles
                    Task endRoundTask = SetMode(GameMode.EndingRound, 60000);
                    break;
                case GameMode.EndingRound:
                    //If round is 8, the game has come to an end
                    if (Round == 8)
                    {
                        //Set game mode to end game
                        Task endGameTask = SetMode(GameMode.EndingGame, 7000);
                    }
                    else
                    {
                        //Set game mode to start new round
                        SetRound(Round + 1);
                        Task startNewRoundTask = SetMode(GameMode.StartingRound, 7000);
                    }
                    break;
                case GameMode.EndingGame:
                    //Set game mode to RevealingRoles
                    Task startNewGameTask = SetMode(GameMode.StartingGame, 7000);
                    break;
                default:
                    break;
            }
        }

        public void StopGame()
        {
            //Change game status to stopped
            Hashtable customProperties = new Hashtable() { { "mode", GameMode.WaitingForPlayers } };
            (App.Current as App).LoadBalancingClient.CurrentRoom.SetCustomProperties(customProperties);
        }

        public void SetPlayerPoints(int points)
        {
            Hashtable customProperties = new Hashtable() { { "points", points } };
            (App.Current as App).LoadBalancingClient.LocalPlayer.SetCustomProperties(customProperties);
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
                    PlayerTtl = 30000, //Keep actor in room 30 seconds after it was disconnected  
                    CustomRoomPropertiesForLobby = new string[] { "C0" }, // this makes "C0" available in the lobby
                    PublishUserId = true,
                }
            };

            if (!(App.Current as App).LoadBalancingClient.OpCreateRoom(roomParams))
            {
                throw new PhotonException("Could not create room");
            }
        }

        public void UpdateGame()
        {
            try
            {
                Room room = (App.Current as App).LoadBalancingClient.CurrentRoom;

                Name = room.Name.ToUpper();

                if (room.CustomProperties.ContainsKey("mode"))
                {
                    Mode = (GameMode)room.CustomProperties["mode"];
                }
                if (room.CustomProperties.ContainsKey("round"))
                {
                    Round = (int)room.CustomProperties["round"];
                }
                if (room.CustomProperties.ContainsKey("random_letters"))
                {
                    RandomLetters = (string)room.CustomProperties["random_letters"];
                }
                if (room.CustomProperties.ContainsKey("secret_word"))
                {
                    SecretWord = (string)room.CustomProperties["secret_word"];
                }
            }
            catch (Exception e)
            {
                throw new PhotonException("Could not get game", e);
            }
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
