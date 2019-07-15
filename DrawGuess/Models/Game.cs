﻿using DrawGuess.Exceptions;
using ExitGames.Client.Photon;
using Photon.Realtime;
using PlayFab;
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

        public int NumberOfPlayers
        {
            get { return _numberOfPlayers; }
            set
            {
                _numberOfPlayers = value;
                if (this.NumberOfPlayers >= 10) { Full = true; }
                else { Full = false; }
            }
        }
        private int _numberOfPlayers;

        public static void AddPlayer(int playerId, int gameId)
        {
            string query =
                 "INSERT INTO dbo.GamePlayer (UserId, Points, GameId) " +
                 "VALUES('" + playerId + "','" + 0 + "','" + gameId + ")";

            try
            {
                using (SqlConnection conn = new SqlConnection((App.Current as App).ConnectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand
                    {
                        CommandType = System.Data.CommandType.Text,
                        CommandText = query,
                        Connection = conn
                    };

                    cmd.ExecuteNonQuery();

                    conn.Close();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static ObservableCollection<Player> GetPlayers(int id)
        {
            string query =
                "SELECT u.Id, u.FirstName, u.LastName, u.ProfileImage, p.Points " +
                "FROM dbo.GamePlayer p " +
                "JOIN dbo.Users u " +
                "ON p.UserId = u.Id " +
                "WHERE p.GameId = " + id;

            var players = new ObservableCollection<Player>();

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
                                    players.Add(new Player()
                                    {
                                        Id = reader.GetInt32(0),
                                        FirstName = reader.GetString(1),
                                        LastName = reader.GetString(2),
                                        ProfilePicture = reader.GetString(3),
                                        Points = reader.GetInt32(4)
                                    });
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

        public static void JoinGame(string gameName, User player)
        {
            (App.Current as App).LoadBalancingClient.OpJoinRoom(
                new EnterRoomParams() { RoomName = gameName });
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
                    CustomRoomProperties = new Hashtable() { { "round", 1 }, { "C0", 1 } },
                    PublishUserId = true,
                }
            };

            if (!(App.Current as App).LoadBalancingClient.OpCreateRoom(roomParams))
            {
                throw new PhotonException("Could not create room right now");
            }

            
            //////////////////////////////////////////////////////////////////////////////////////////

            //Add game to database
            string query =
                "INSERT INTO dbo.Game (Name) " +
                "VALUES ('" + gameName + "')";

            try
            {
                using (SqlConnection conn = new SqlConnection((App.Current as App).ConnectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand
                    {
                        CommandType = System.Data.CommandType.Text,
                        CommandText = query,
                        Connection = conn
                    };

                    cmd.ExecuteNonQuery();

                    conn.Close();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static Game GetGame(string gameName)
        {
            // join random rooms easily, filtering for specific room properties, if needed
            Hashtable expectedCustomRoomProperties = new Hashtable();
            //(App.Current as App).LoadBalancingClient.OpGetGameList();

            // custom props can have any name but the key must be string
            expectedCustomRoomProperties["map"] = 1;

            (App.Current as App).LoadBalancingClient.OpGetGameList(TypedLobby.Default, "");

            string query = "SELECT Id, Name, SecretWord, Round, RandomLetters FROM dbo.Game WHERE Name = '" + gameName + "'";
            Game game = new Game();

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
                                    game = new Game()
                                    {
                                        Id = reader.GetInt32(0),
                                        Name = reader.GetString(1),
                                        NumberOfPlayers = GetNumberOfPlayers(reader.GetInt32(0)),
                                        SecretWord = reader.IsDBNull(reader.GetOrdinal("SecretWord")) ? null : reader.GetString(2),
                                        Round = reader.GetInt32(3),
                                        RandomLetters = reader.IsDBNull(reader.GetOrdinal("RandomLetters")) ? null : reader.GetString(4),
                                    };
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

            return game;
        }

        public static async Task RemoveGameMember(string gameName, User user)
        {
            try
            {
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static ObservableCollection<Game> GetGames()
        {

            var games = new ObservableCollection<Game>();

            const string query = "SELECT Id, Name FROM dbo.Game";

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
                                    games.Add(new Game()
                                    {
                                        Id = reader.GetInt32(0),
                                        Name = reader.GetString(1),
                                        NumberOfPlayers = GetNumberOfPlayers(reader.GetInt32(0))
                                    });
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

            return games;
        }
    }
}
