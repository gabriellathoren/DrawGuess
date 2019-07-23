using DrawGuess.Models;
using DrawGuess.ViewModels;
using ExitGames.Client.Photon;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace DrawGuess.Pages
{
    public sealed partial class GamePage : Page
    {
        public GameViewModel ViewModel { get; set; }
        private LoadBalancingClient LoadBalancingClient = (App.Current as App).LoadBalancingClient;

        public GamePage()
        {
            this.InitializeComponent();
            ViewModel = new GameViewModel();

            // Initialize the InkCanvas
            InkCanvas.InkPresenter.InputDeviceTypes =
                Windows.UI.Core.CoreInputDeviceTypes.Mouse |
                Windows.UI.Core.CoreInputDeviceTypes.Pen;

            //Listen to callbacks from Photon
            LoadBalancingClient.InRoomCallbackTargets.PlayerEnteredRoom += PlayerEnteredRoom;
            LoadBalancingClient.InRoomCallbackTargets.PlayerLeftRoom += PlayerLeftRoom;
            LoadBalancingClient.InRoomCallbackTargets.RoomPropertiesUpdate += RoomPropertiesUpdate;
        }

        //Listener for room changes
        private async void RoomPropertiesUpdate(object sender, EventArgs e)
        {
            try
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        UpdateGame();
                    });
            }
            catch (Exception)
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        ViewModel.ErrorMessage = "Could not update room";
                    });
            }
        }

        //Listener for player leaving room
        private async void PlayerLeftRoom(object sender, EventArgs e)
        {
            try
            {
                Photon.Realtime.Player newPlayer = (Photon.Realtime.Player)sender;
                Models.Player player = new Models.Player()
                {
                    UserId = newPlayer.UserId
                };

                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        RemovePlayer(player);
                    });
            }
            catch (Exception)
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        ViewModel.ErrorMessage = "Could not update player list";
                    });
            }
        }

        //Listener for player entering room
        private async void PlayerEnteredRoom(object sender, EventArgs e)
        {
            try
            {
                Photon.Realtime.Player newPlayer = (Photon.Realtime.Player)sender;
                Models.Player player = new Models.Player()
                {
                    UserId = newPlayer.UserId,
                    NickName = newPlayer.NickName,
                    IsCurrentUser = false,
                    Points = (int)newPlayer.CustomProperties["points"]
                };

                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        AddPlayer(player);
                    });
            }
            catch (Exception)
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        ViewModel.ErrorMessage = "Could not update player list";
                    });
            }
        }

        public void AddPlayer(Models.Player player)
        {
            try
            {
                ViewModel.Players.Add(player);
                SetPlacement();
                SetGame();
            }
            catch(Exception)
            {
                ViewModel.ErrorMessage = "Could not add player properly";
            }
        }

        public void RemovePlayer(Models.Player player)
        {
            try
            {
                Models.Player p = ViewModel.Players.Where(x => x.UserId.Equals(player.UserId)).First();
                ViewModel.Players.Remove(p);
                SetPlacement();
                SetGame();
            }
            catch (Exception)
            {
                ViewModel.ErrorMessage = "Could not remove player properly";
            }
        }

        public void SetGame()
        {
            GetPlayers();
            SetPlacement();

            //Start game if there are 2 or more players 
            if (ViewModel.Players.Count > 1 && !ViewModel.Game.Started)
            {
                StartGame();

            }
            //Stop game if there are less players than two
            else if(ViewModel.Players.Count < 2 && ViewModel.Game.Started)
            {
                StopGame();
            }
        }

        public void UpdateGame()
        {
            GetSecretWord();
            SetHint();
            GetRandomLetters();
        }

        public void StopGame()
        {
            try
            {
                ViewModel.Game.Started = false;
                Game.StopGame();
            }
            catch(Exception)
            {
                ViewModel.ErrorMessage = "Could not stop game properly";
            }
        }

        public void StartGame()
        {            
            try
            {
                ViewModel.Game.Started = true;
                Game.StartGame();                
            }
            catch(Exception)
            {
                ViewModel.ErrorMessage = "Could not start game";
            }

        }

        public void SetPlayerPoints(int points)
        {
            try
            {
                Game.SetPlayerPoints(points);
            }
            catch (Exception)
            {
                ViewModel.ErrorMessage = "Could not set player points";
            }
        }

        public void GetPlayers()
        {
            try
            {
                ViewModel.Players = Game.GetPlayers();
            }
            catch (Exception e)
            {
                ViewModel.ErrorMessage = e.Message;
            }
        }

        public void GetSecretWord()
        {
            try
            {
                ViewModel.Game.SecretWord = Models.Game.GetSecretWord();
            }
            catch (Exception e)
            {
                ViewModel.ErrorMessage = e.Message;
            }
        }

        public void SetPainter()
        {
            try
            {
                
            }
            catch(Exception e)
            {
                ViewModel.ErrorMessage = e.Message;
            }
        }

        public void GetRandomLetters()
        {
            try
            {
                ViewModel.Game.RandomLetters = Models.Game.GetRandomLetters();

                var letters = new ObservableCollection<string>();

                //Set hinting boxes based on number of letters in secret word
                for (int i = 0; i < ViewModel.Game.RandomLetters.Length; i++)
                {
                    letters.Add(ViewModel.Game.RandomLetters[i].ToString());
                }

                ViewModel.RandomLetters = letters;
            }
            catch (Exception e)
            {
                ViewModel.ErrorMessage = e.Message;
            }
        }

        public void SetHint()
        {
            var hint = new ObservableCollection<string>();

            //Set hinting boxes based on number of letters in secret word
            for (int i = 0; i < ViewModel.Game.SecretWord.Length; i++)
            {
                if(ViewModel.Game.SecretWord[i].ToString().Equals(" "))
                {
                    hint.Add(" ");
                }
                else
                {
                    hint.Add("");
                }                
            }

            ViewModel.Guess = hint;
        }
        
        public void SetPlacement()
        {
            ViewModel.Players = new ObservableCollection<Models.Player>(ViewModel.Players.OrderBy(x => x.Points).ToList());

            int placement = 1;
            foreach (Models.Player p in ViewModel.Players)
            {
                if (ViewModel.Players.IndexOf(p) == 0)
                {
                    p.Placement = placement;
                }
                else if (p.Points == ViewModel.Players[ViewModel.Players.IndexOf(p) - 1].Points)
                {
                    p.Placement = placement;
                }
                else
                {
                    placement++;
                    p.Placement = placement;
                }
            }
        }

        public void GetGame()
        {
            ViewModel.Game = Game.GetGame();
            SetPlayerPoints(0);
            SetGame();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                GetGame();
            }
            catch (Exception ex)
            {
                //Show error message and navgate back to start page
                ViewModel.ErrorMessage = ex.Message;
                Quit();
            }
        }

        private void Quit()
        {
            ViewModel.Game.LeaveGame();

            while (!ViewModel.Game.LeftRoom)
            {
                Task.Delay(TimeSpan.FromMilliseconds(100));
            }

            this.Frame.Navigate(typeof(StartPage), "", new SuppressNavigationTransitionInfo());
        }

        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            Quit();
        }

    }
}
