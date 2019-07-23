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
using System.Text;
using System.Threading.Tasks;

namespace DrawGuess.Models
{
    enum EventCode : byte { StartGame = 1, StopGame, NewRound };

    public class Game
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Boolean Full { get; set; }
        public string SecretWord { get; set; }
        public string RandomLetters { get; set; }
        public int Round { get; set; }
        public bool Started { get; set; }
        public bool LeftRoom = false;

        private LoadBalancingClient LoadBalancingClient = (App.Current as App).LoadBalancingClient;

        public int NumberOfPlayers
        {
            get { return _numberOfPlayers; }
            set
            {
                _numberOfPlayers = value;
                if (this.NumberOfPlayers >= 8) { Full = true; }
                else { Full = false; }
            }
        }
        private int _numberOfPlayers;

        public Game()
        {
            LoadBalancingClient.MatchMakingCallbackTargets.LeftRoom += RoomLeft;
        }

        private void RoomLeft(object sender, EventArgs e)
        {
            LeftRoom = true;
        }

        public static ObservableCollection<Player> GetPlayers()
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


                    if(p.Value.CustomProperties.ContainsKey("points"))
                    {
                        player.Points = (int)p.Value.CustomProperties["points"];
                    }
                    else
                    {
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

        //public static void NewRound(int round, ObservableCollection<Player> players)
        //{
        //    try
        //    {
        //        //Get secret word and random letters for the hint
        //        string secretWord = WordHelper.RandomizeSecretWord();
        //        string randomLetters = WordHelper.SetRandomLetters(secretWord);
                
        //        Hashtable customProperties = new Hashtable() {
        //            { "secret_word", secretWord }, //Secret word
        //            { "random_letters", randomLetters }, //Set random letters
        //            { "round", round}, //Set round
        //        };
        //        (App.Current as App).LoadBalancingClient.CurrentRoom.SetCustomProperties(customProperties);
                
        //        //Set current user to painter
        //        Hashtable playerProperties = new Hashtable() { { "painter", true } };
        //        (App.Current as App).LoadBalancingClient.LocalPlayer.SetCustomProperties(playerProperties);
        //    }
        //    catch (Exception e)
        //    {
        //        throw new PhotonException("Could not start game", e);
        //    }
        //}
        
        public static void StartGame()
        {
            try
            {
                //Get secret word and random letters for the hint
                string secretWord = WordHelper.RandomizeSecretWord();
                string randomLetters = WordHelper.SetRandomLetters(secretWord);

                Hashtable customProperties = new Hashtable() {
                    { "started", true }, //Change game status to started
                    { "secret_word", secretWord }, //Secret word
                    { "random_letters", randomLetters }, //Set random letters
                    { "round", 1 }, //Set round
                };
                (App.Current as App).LoadBalancingClient.CurrentRoom.SetCustomProperties(customProperties);

                //Set current user to painter
                Hashtable playerProperties = new Hashtable() { { "painter", true } };
                (App.Current as App).LoadBalancingClient.LocalPlayer.SetCustomProperties(playerProperties);
            }
            catch (Exception e)
            {
                throw new PhotonException("Could not start game", e);
            }
        }

        public static void StopGame()
        {
            //Change game status to stopped
            Hashtable customProperties = new Hashtable() { { "started", false } };
            (App.Current as App).LoadBalancingClient.CurrentRoom.SetCustomProperties(customProperties);
        }

        public static void SetPlayerPoints(int points)
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
                        { "started", false }
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

        public static string GetSecretWord()
        {
            try
            {
                Room room = (App.Current as App).LoadBalancingClient.CurrentRoom;
                return (string)room.CustomProperties["secret_word"];
            }
            catch (Exception e)
            {
                throw new PhotonException("Could not get secret word", e);
            }
        }

        public static string GetRandomLetters()
        {
            try
            {
                Room room = (App.Current as App).LoadBalancingClient.CurrentRoom;
                return (string)room.CustomProperties["random_letters"];
            }
            catch (Exception e)
            {
                throw new PhotonException("Could not get random letters", e);
            }
        }

        public static Game GetGame()
        {
            try
            {
                Room room = (App.Current as App).LoadBalancingClient.CurrentRoom;

                Game game = new Game()
                {
                    Name = room.Name,
                    Started = (bool)room.CustomProperties["started"]
                };

                if(room.CustomProperties.ContainsKey("round"))
                {
                    game.Round = (int)room.CustomProperties["round"];
                }
                if(room.CustomProperties.ContainsKey("random_letters"))
                {
                    game.RandomLetters = (string)room.CustomProperties["random_letters"];
                }
                if(room.CustomProperties.ContainsKey("secret_word"))
                {
                    game.SecretWord = (string)room.CustomProperties["secret_word"];
                }   

                return game;
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
    }
}
