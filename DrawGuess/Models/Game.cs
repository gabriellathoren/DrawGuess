﻿using DrawGuess.Exceptions;
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
        PainterLeft,
        StartingRoundAfterPainterLeft
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

        public bool LeftRoom = false;
        public bool ChangingMode = false;
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

                    if (p.Value.CustomProperties.ContainsKey("painter"))
                    {
                        player.Painter = (bool)p.Value.CustomProperties["painter"];
                    }
                    else
                    {
                        player.Painter = false;
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
                    { "painter", false }, //Set round         
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
        
        public void SetPainter()
        {
            try
            {
                //Set painter                
                Hashtable painterProperties = new Hashtable() { { "painter", true } };
                Hashtable notPainterProperties = new Hashtable() { { "painter", false } };
                Dictionary<int, Photon.Realtime.Player> photonPlayers = (App.Current as App).LoadBalancingClient.CurrentRoom.Players;

                int playerIndex = 0;
                foreach (var p in photonPlayers)
                {
                    if (p.Value.CustomProperties.ContainsKey("painter"))
                    {
                        if ((bool)p.Value.CustomProperties["painter"])
                        {
                            //If a painter existed last round, set the next player in the list as painter 
                            if (photonPlayers.Count > (playerIndex + 1))
                            {
                                photonPlayers.ElementAt(playerIndex + 1).Value.SetCustomProperties(painterProperties);
                            }
                            //If a painter existed last round, but there are no next player in the list, set the first player in the list as painter
                            else
                            {
                                photonPlayers.First().Value.SetCustomProperties(painterProperties);
                            }

                            //Remove current painter as painter
                            p.Value.SetCustomProperties(notPainterProperties);
                            return;
                        }
                    }

                    playerIndex++;
                }
                //Set current user to painter if there are no painter already
                (App.Current as App).LoadBalancingClient.LocalPlayer.SetCustomProperties(painterProperties);
            }
            catch(Exception e)
            {
                throw new PhotonException("Could not set painter", e);
            }
        }

        public void SetSpecificPainter(Models.Player p)
        {
            try
            {
                Hashtable painterProperties = new Hashtable() { { "painter", true } };
                Dictionary<int, Photon.Realtime.Player> photonPlayers = (App.Current as App).LoadBalancingClient.CurrentRoom.Players;
                Photon.Realtime.Player newPainter = photonPlayers.Where(x => x.Value.UserId == p.UserId).FirstOrDefault().Value;
                newPainter.SetCustomProperties(painterProperties);
            }
            catch(Exception e)
            {
                throw new PhotonException("Could not set painter", e);
            }
        }

        public void AddStrokes(byte[] strokes) //, byte[] newStrokes)
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
            int i = 0;
            bool done = false;
            while(!StopTasks && !done)
            {
                await Task.Delay(100);
                
                if(i>=(waitingTimeSec*10))
                {
                    done = true;
                }
                i++;
            }

            if(!StopTasks)
            {
                Hashtable customProperties = new Hashtable() {
                    { "mode", mode }
                };
                LoadBalancingClient.CurrentRoom.SetCustomProperties(customProperties, new Hashtable(), new WebFlags(0) { HttpForward = true });
            }            
        }

        public void SetRound(int round)
        {
            Hashtable customProperties = new Hashtable() { { "round", round } };
            (App.Current as App).LoadBalancingClient.CurrentRoom.SetCustomProperties(customProperties, new Hashtable(), new WebFlags(0) { HttpForward = true });
            Round = round;
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
                    //Set game mode to StartingRound
                    StopTasks = false;
                    Task startingRoundTask = SetMode(GameMode.StartingRound, 3);
                    break;
                case GameMode.StartingRound:
                    //Set game mode to RevealingRoles
                    StartRound(Round);
                    if (Round > 1) { SetPainter(); }
                    Task revealTask = SetMode(GameMode.RevealingRoles, 3);
                    break;
                case GameMode.StartingRoundAfterPainterLeft:
                    //Set game mode to RevealingRoles
                    StartRound(Round);
                    Task revealTask2 = SetMode(GameMode.RevealingRoles, 3);
                    break;
                case GameMode.RevealingRoles:
                    //Set game mode to RevealingRoles
                    Task playTtask = SetMode(GameMode.Playing, 5);
                    break;
                case GameMode.Playing:
                    //Set game mode to EndingRound
                    Task endRoundTask = SetMode(GameMode.EndingRound, 60);
                    break;
                case GameMode.EndingRound:
                    //If round is 8, the game has come to an end
                    if (Round == 8)
                    {
                        //Set game mode to end game
                        Task endGameTask = SetMode(GameMode.EndingGame, 3);
                    }
                    else
                    {
                        //Set game mode to start new round
                        SetRound(Round + 1);
                        Task startNewRoundTask = SetMode(GameMode.StartingRound, 3);
                    }
                    break;
                case GameMode.EndingGame:
                    //Set game mode to RevealingRoles
                    SetRound(1);
                    Task startNewGameTask = SetMode(GameMode.StartingGame, 3);
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
            Hashtable customProperties = new Hashtable() { { "points", points } };
            (App.Current as App).LoadBalancingClient.LocalPlayer.SetCustomProperties(customProperties);
        }

        public void SetCorrectAnswer(bool correctGuess)
        {
            Hashtable customProperties = new Hashtable() { { "correct_guess", correctGuess } };
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
