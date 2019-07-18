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
        public bool connectedToPhoton = false; 

        public GameEngine()
        {
            LoadBalancingClient.ConnectionCallbackTargets.ConnectedToMaster += ConnectedToMaster;
            LoadBalancingClient.ConnectionCallbackTargets.Disconnected += Disconnected;
        }

        private void ConnectedToMaster(object sender, EventArgs e)
        {
            connectedToPhoton = true;
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

            //Start game loop to have continues connection to Photon
            Task task = Task.Run((Action)GameLoop);
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
