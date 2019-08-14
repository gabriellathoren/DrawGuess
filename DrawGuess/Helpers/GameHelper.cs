using DrawGuess.Exceptions;
using DrawGuess.Models;
using ExitGames.Client.Photon;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawGuess.Helpers
{
    public class GameHelper
    {
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
    }    
}
