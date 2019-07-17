using DrawGuess.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawGuess
{
    public class GameEngine
    {
        public static void ConnectToMaster()
        {
            //Connect to master server (Photon Cloud)
            if (!(App.Current as App).LoadBalancingClient.ConnectToRegionMaster("eu"))
            {
                throw new PhotonException("Could not authenticate user");
            }

            //Start game loop connected to Photon
            Task task = Task.Run((Action)GameLoop);
        }

        //Get and send updates to Photon 
        public static async void GameLoop()
        {
            while (!(App.Current as App).ShouldExit)
            {
                (App.Current as App).LoadBalancingClient.Service();
                // wait for few frames/milliseconds
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }

            GameEngine.Disconnect();
        }

        public static void Disconnect()
        {
            if ((App.Current as App).Connected)
            {
                (App.Current as App).LoadBalancingClient.Disconnect();
            }
        }
    }
}
