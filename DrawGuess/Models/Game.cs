using DrawGuess.Exceptions;
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
    public class Game
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Boolean Full { get; set; }
        public string SecretWord { get; set; }
        public string RandomLetters { get; set; }
        public int Round { get; set; }
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

                    if(p.Value.UserId.Equals((App.Current as App).LoadBalancingClient.LocalPlayer.UserId))
                    {
                        player.IsCurrentUser = true;
                    }

                    player.NickName = p.Value.NickName;
                    player.Points = (int)p.Value.CustomProperties["points"];

                    players.Add(player);
                }
            }
            catch(Exception e)
            {
                throw new PhotonException("Could not get players");
            }
            
            return players;
        }

        public static int GetNumberOfPlayers(int id)
        {
            string query = "SELECT COUNT(*) FROM dbo.GamePlayer WHERE GameId = " + id;
            int numberOfPlayers = 0;

            try
            {
                using (SqlConnection conn = new SqlConnection((App.Current as App).ConnectionString))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = query;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    numberOfPlayers = reader.GetInt32(0);
                                }
                            }
                        }
                    }
                    conn.Close();
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return numberOfPlayers;
        }

        public static string RandomizeRoomName(ObservableCollection<Game> currentGames)
        {

            string query = "SELECT Top 1 Name FROM GameNames ";

            int counter = 1;
            foreach (Game game in currentGames)
            {
                if (counter == 1)
                {
                    query += "WHERE Name != '" + game.Name + "' ";
                }
                else
                {
                    query += "AND Name != '" + game.Name + "' ";
                }
                counter++;
            }

            query += "ORDER BY NEWID()";

            string randomGameName = "";

            try
            {
                using (SqlConnection conn = new SqlConnection((App.Current as App).ConnectionString))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = query;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    randomGameName = reader.GetString(0);
                                }
                            }
                        }
                    }
                    conn.Close();
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return randomGameName;
        }

        public static void JoinGame(string gameName)
        {
            var roomParams = new EnterRoomParams()
            {
                RoomName = gameName, 
            };

            if(!(App.Current as App).LoadBalancingClient.OpJoinRoom(roomParams))
            {
                throw new PhotonException("Could not join room");
            } 
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
                        { "round", 1 },
                        { "C0", 1 },
                        { "secret_word", "test" }
                    },
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
            catch(Exception)
            {
                throw new PhotonException("Could not get secret word");
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
                    Round = (int)room.CustomProperties["round"],
                    RandomLetters = (string)room.CustomProperties["random_letters"],
                    SecretWord = (string)room.CustomProperties["secret_word"]
                };

                return game;
            }
            catch(Exception)
            {
                throw new PhotonException("Could not get game");
            }
        }

        public void LeaveGame()
        {
            try
            {
                if(!(App.Current as App).LoadBalancingClient.OpLeaveRoom(false))
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
                if(!(App.Current as App).LoadBalancingClient.OpGetGameList(new TypedLobby("Lobby1", LobbyType.SqlLobby), "C0=1"))
                {
                    throw new PhotonException();
                }
                
            }
            catch(Exception)
            {
                throw new PhotonException("Could not get games");
            }
        }
    }
}
