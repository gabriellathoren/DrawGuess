using DrawGuess.Exceptions;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawGuess
{
    public class GameEngine
    {

        private LoadBalancingClient LoadBalancingClient = (App.Current as App).LoadBalancingClient;
        public bool connectedToLobby = false;

        public GameEngine()
        {
            LoadBalancingClient.ConnectionCallbackTargets.ConnectedToMaster += ConnectedToMaster;
            LoadBalancingClient.ConnectionCallbackTargets.Disconnected += Disconnected;
            LoadBalancingClient.LobbyCallbackTargets.JoinedLobby += JoinedLobby;
        }

        private void JoinedLobby(object sender, EventArgs e)
        {
            connectedToLobby = true;
        }

        private void ConnectedToMaster(object sender, EventArgs e)
        {
            (App.Current as App).Connected = true; 
        }

        private void Disconnected(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public void ConnectToMaster()
        {
            //Connect to master server (Photon Cloud)
            if (!LoadBalancingClient.ConnectToRegionMaster("eu"))
            {
                throw new PhotonException("Could not authenticate user");
            }
        }

        public void ConnectToLobby()
        {
            if (!LoadBalancingClient.InLobby)
            {
                if (!LoadBalancingClient.OpJoinLobby(new TypedLobby("Lobby1", LobbyType.SqlLobby)))
                {
                    throw new PhotonException("Could not join lobby");
                }
            }
        }

        //Get and send updates to Photon 
        public async void GameLoop()
        {
            while (!(App.Current as App).ShouldExit)
            {
                LoadBalancingClient.Service();
                // wait for few frames/milliseconds
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }

            Disconnect();
        }

        public void Disconnect()
        {
            if ((App.Current as App).Connected)
            {
                LoadBalancingClient.Disconnect();
            }
        }
    }
}
