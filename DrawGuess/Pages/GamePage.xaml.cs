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
            InkCanvas.InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Mouse | CoreInputDeviceTypes.Pen;

            //Listen to callbacks from Photon
            LoadBalancingClient.InRoomCallbackTargets.PlayerEnteredRoom += PlayerEnteredRoom;
            LoadBalancingClient.InRoomCallbackTargets.PlayerLeftRoom += PlayerLeftRoom;
            LoadBalancingClient.InRoomCallbackTargets.RoomPropertiesUpdate += RoomPropertiesUpdate;
            LoadBalancingClient.InRoomCallbackTargets.PlayerPropertiesUpdate += PlayerPropertiesUpdate;
        }

        //Listener for player changes
        private async void PlayerPropertiesUpdate(object sender, EventArgs e)
        {
            try
            {
                Hashtable data = (Hashtable)sender;

                if(data.ContainsKey("painter"))
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        UpdatePlayerList();
                    });
                }
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

        //Listener for room changes
        private async void RoomPropertiesUpdate(object sender, EventArgs e)
        {
            try
            {
                Hashtable data = (Hashtable)sender;

                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        GetGame();
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
            catch (Exception ex)
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
                //SetGameMode();
            }
            catch(Exception)
            {
                ViewModel.ErrorMessage = "Could not add player properly";
            }
        }

        public async void RemovePlayer(Models.Player player)
        {
            try
            {
                Models.Player p = ViewModel.Players.Where(x => x.UserId.Equals(player.UserId)).First();

                //If leaving player was painter
                if(p.Painter == true)
                {
                    ViewModel.Players.Remove(p);

                    //Change game mode
                    await ViewModel.Game.SetMode(GameMode.PainterLeft, 0);

                    //Set new random painter
                    Random rnd = new Random();
                    int newPainterIndex = rnd.Next(ViewModel.Players.Count);
                    ViewModel.Game.SetSpecificPainter(ViewModel.Players[newPainterIndex]);
                }
                else
                {
                    ViewModel.Players.Remove(p);
                }
                
                SetPlacement();
                SetGameMode();
                GetGame();
            }
            catch (Exception)
            {
                ViewModel.ErrorMessage = "Could not remove player properly";
            }
        }

        public void SetGameMode()
        {
            try
            {
                //Start game if there are 2 or more players 
                if (ViewModel.Players.Count > 1 && ViewModel.Game.Mode.Equals(GameMode.WaitingForPlayers))
                {
                    StartGame();
                }
                //Stop game if there are less players than two
                else if (ViewModel.Players.Count < 2 && !ViewModel.Game.Mode.Equals(GameMode.WaitingForPlayers))
                {
                    ViewModel.Game.StopGame();
                }
            }
            catch(Exception e)
            {
                ViewModel.ErrorMessage = "Could not set game mode";
            }
        }
        
        public void StartGame()
        {
            try
            {
                ViewModel.Game.StartGame();
            }
            catch(Exception)
            {
                ViewModel.ErrorMessage = "Could not start game";
            }
        }

        public void UpdatePlayerList()
        {
            GetPlayers();
            SetPlacement();
        }
               
        public void SetPlayerPoints(int points)
        {
            try
            {
                ViewModel.Game.SetPlayerPoints(points);
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
                ViewModel.Players = ViewModel.Game.GetPlayers();
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

        public void UpdateInfoView()
        {
            InfoView.TwoRows = false;
            InfoView.Row1 = "";
            InfoView.Row2 = "";

            switch (ViewModel.Game.Mode)
            {
               case GameMode.WaitingForPlayers:
                    ViewModel.ShowInfoView = true;
                    ViewModel.ShowGame = false;
                    InfoView.Row1 = "Waiting for other players\r\nto join...";
                    break;
                case GameMode.StartingGame:
                    ViewModel.ShowInfoView = true;
                    ViewModel.ShowGame = false;
                    InfoView.Row1 = "Starting new game...";
                    break;
                case GameMode.StartingRound:
                    ViewModel.ShowInfoView = true;
                    ViewModel.ShowGame = false;
                    InfoView.Row1 = "ROUND " + ViewModel.Game.Round.ToString();
                    break;
                case GameMode.RevealingRoles:
                    ViewModel.ShowInfoView = true;
                    ViewModel.ShowGame = false;
                    InfoView.Row1 = "Painter: " + ViewModel.Players.Where(x => x.Painter.Equals(true)).First().NickName;
                    InfoView.ShowSecretWord = false;
                    if (ViewModel.Players.Where(x => x.IsCurrentUser == true).First().Painter) {
                        InfoView.ShowSecretWord = true;
                        InfoView.Row2 = "Secret word: " + ViewModel.Game.SecretWord;
                        InfoView.TwoRows = true;                        
                    }
                    break;
                case GameMode.Playing:
                    ViewModel.ShowInfoView = false;
                    ViewModel.ShowGame = true;
                    break;
                case GameMode.EndingRound:
                    ViewModel.ShowInfoView = true;
                    ViewModel.ShowGame = true;
                    InfoView.Row1 = "The secret word was:\r\n" + ViewModel.Game.SecretWord;
                    break;
                case GameMode.EndingGame:
                    ViewModel.ShowInfoView = true;
                    ViewModel.ShowGame = true;
                    InfoView.Row1 = "Winner: " + GetWinners();
                    break;
                case GameMode.PainterLeft:
                    ViewModel.ShowInfoView = true;
                    InfoView.Row1 = "Painter left the game";
                    break;
                default:
                    break;
            }
        }

        public string GetWinners()
        {
            string winners = "";
            var maxPoints = ViewModel.Players.Max(x => x.Points);
            var winnerList = ViewModel.Players.Where(x => x.Points == maxPoints).ToList();

            foreach (var winner in winnerList)
            {
                winners += winner.NickName + " ";
            }

            return winners;
        }

        public void GetGame()
        {
            try
            {         
                ViewModel.Game.UpdateGame();

                if (!string.IsNullOrEmpty(ViewModel.Game.SecretWord))
                {
                    SetHint();
                    GetRandomLetters();
                }

                UpdateInfoView();
            }
            catch(Exception e)
            {
                ViewModel.ErrorMessage = e.Message;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                UpdatePlayerList();
                GetGame();
                SetGameMode();
                SetPlayerPoints(0);
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
