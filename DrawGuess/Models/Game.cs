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
using System.Threading;
using System.Threading.Tasks;

namespace DrawGuess.Models
{
    public enum GameMode : int {
        WaitingForPlayers = 1,
        StartingGame,
        StartingRound,
        RevealingRoles, 
        Playing, 
        EndingRound,
        EndingGame
    }

    public class Game
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Boolean Full { get; set; }
        public string SecretWord { get; set; }
        public string RandomLetters { get; set; }
        public int Round { get; set; }
        public GameMode Mode { get; set; }       

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
            
            var members = new List<string>();
            members.Add((App.Current as App).User.PlayFabId); 

            //Add player to shared group 
            var addToSharedGroupTask = Task.Run(() =>
            {
                PlayFabClientAPI.AddSharedGroupMembersAsync(
                    new AddSharedGroupMembersRequest()
                    {
                        PlayFabIds = members,
                        SharedGroupId = gameName                         
                    }
                );
            });

            addToSharedGroupTask.Wait();
        }
        
        public static void StartGame()
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

                var data = new Hashtable() {
                    { "mode" , GameMode.StartingGame },
                    { "gameName", (App.Current as App).LoadBalancingClient.CurrentRoom.Name },
                    { "eventType", GameMode.StartingGame }
                };

                //Raise starting game event
                (App.Current as App).LoadBalancingClient.OpRaiseEvent(
                    Convert.ToByte(GameMode.StartingGame), 
                    data,
                    new RaiseEventOptions() { Flags = new WebFlags(0) { HttpForward = true } },
                    SendOptions.SendReliable
                 );
                
                //Set current user to painter
                Hashtable playerProperties = new Hashtable() { { "painter", true } };
                (App.Current as App).LoadBalancingClient.LocalPlayer.SetCustomProperties(playerProperties);


                var updatedata = new Dictionary<string, string>();
                updatedata.Add("mode", GameMode.StartingGame.ToString());

                var update = new UpdateSharedGroupDataRequest()
                {
                    SharedGroupId = (App.Current as App).LoadBalancingClient.CurrentRoom.Name,
                    Data = updatedata,
                    Permission = UserDataPermission.Public
                };

                var updateSharedGroupTask = Task.Run(() =>
                {
                    PlayFabClientAPI.UpdateSharedGroupDataAsync(
                        update
                    );
                });

                updateSharedGroupTask.Wait();
            }
            catch (Exception e)
            {
                throw new PhotonException("Could not start game", e);
            }
        }

        public static void StopGame()
        {
            //Change game status to stopped
            Hashtable customProperties = new Hashtable() { { "mode", GameMode.WaitingForPlayers } };
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

            //ta bort om finns

            //Create shared group data
            var createSharedGroupTask = Task.Run(() =>
            {
                PlayFabClientAPI.CreateSharedGroupAsync(
                    new CreateSharedGroupRequest() { SharedGroupId = gameName }
                );
            });

            createSharedGroupTask.Wait();
        }

        public static Game GetGame()
        {
            try
            {
                Room room = (App.Current as App).LoadBalancingClient.CurrentRoom;

                Game game = new Game()
                {
                    Name = room.Name,
                    Mode = (GameMode)room.CustomProperties["mode"]
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
