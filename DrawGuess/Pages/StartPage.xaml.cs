using DrawGuess.Helpers;
using DrawGuess.Models;
using DrawGuess.ViewModels;
using Photon.Realtime;
using PlayFab;
using PlayFab.GroupsModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;


namespace DrawGuess.Pages
{
    public sealed partial class StartPage : Page
    {
        public StartViewModel ViewModel { get; set; }
        public LoadBalancingClient LoadBalancingClient { get; set; }
        public bool GetRooms { get; set; }

        public StartPage()
        {
            if ((App.Current as App).User.Equals(null))
            {
                this.Frame.Navigate(typeof(LoginPage), null, new SuppressNavigationTransitionInfo());
                return;
            }

            this.InitializeComponent();
            this.ViewModel = new StartViewModel();
            ViewModel.Items.Insert(0, new Game() { Id = -1 });

            LoadBalancingClient = (App.Current as App).LoadBalancingClient;
            LoadBalancingClient.MatchMakingCallbackTargets.JoinedRoom += JoinedRoom;
            LoadBalancingClient.MatchMakingCallbackTargets.CreateRoomFailed += CreateRoomFailed;
            LoadBalancingClient.MatchMakingCallbackTargets.JoinRoomFailed += JoinRoomFailed;
            LoadBalancingClient.LobbyCallbackTargets.UpdateRoomList += UpdateRoomList;            
        }

        public void ConnectToMaster()
        {
            try
            {
                //Connect user to Photon Cloud Server
                GameEngine gameEngine = new GameEngine();
                gameEngine.ConnectToMaster();

                while (!gameEngine.connected)
                {
                    Task.Delay(25);
                }
            }
            catch(Exception e)
            {
                ViewModel.ErrorMessage = "Could not connect to Photon";
            }
        }

        public void ConnectToLobby()
        {
            try
            {
                //Connect user to the right lobby
                GameEngine gameEngine = new GameEngine();                
                gameEngine.ConnectToLobby();
                while (!gameEngine.connectedToLobby)
                {
                    Task.Delay(25);
                }
            }
            catch (Exception e)
            {
                ViewModel.ErrorMessage = "Could not connect to Photon";
            }
        }

        private async void UpdateRoomList(object sender, EventArgs e)
        {
            try
            {
                List<RoomInfo> rooms = (List<RoomInfo>)sender;                
                
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        if (GetRooms)
                        {
                            SetGames(rooms);
                            GetRooms = false; 
                        }
                        else
                        {
                            GetGames();
                        }
                    });
            }
            catch(Exception)
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        ViewModel.ErrorMessage = "Could not get games";
                    });
            }
        }

        private async void JoinedRoom(object sender, EventArgs e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    NavigateToGame();
                });
        }

        private void CreateRoomFailed(object sender, EventArgs e)
        {
            ViewModel.ErrorMessage = "Could not create new room";
        }

        private void JoinRoomFailed(object sender, EventArgs e)
        {
            ViewModel.ErrorMessage = "Could not join room";
        }

        public void SetGames(List<RoomInfo> rooms)
        {
            try
            {
                foreach (var room in rooms)
                {
                    //If the room list does not contain the room, add it to the list 
                    if(!ViewModel.Items.Any(x => x.Name == room.Name)) {
                        
                        ViewModel.Items.Add(new Models.Game()
                        {
                            Name = room.Name,
                            NumberOfPlayers = room.PlayerCount
                        });
                    }
                    //If the room list contains the room, update it
                    else
                    {
                        var updatedRoom = rooms.Where(x => x.Name == room.Name).First();
                        var oldRoom = ViewModel.Items.Where(x => x.Name == room.Name).First();

                        if (updatedRoom.PlayerCount != oldRoom.NumberOfPlayers)
                        {
                            oldRoom.NumberOfPlayers = updatedRoom.PlayerCount;
                        }
                    }
                }

                //Remove all rooms that exists in ViewModel, but not in new, updated list
                foreach(var room in ViewModel.Items.ToList())
                {
                    if(room.Id == -1) { continue; }
                    if(!rooms.Any(x => x.Name == room.Name))
                    {
                        ViewModel.Items.Remove(room);
                    }
                }

                SortGameList();

            }
            catch(Exception e)
            {
                ViewModel.ErrorMessage = "Could not get games";
            }
        }

        public void GetGames()
        {
            try
            {
                GetRooms = true;
                GameHelper.GetGames();                
            }
            catch (Exception e)
            {
                ViewModel.ErrorMessage = e.Message;
            }
        }

        public void SortGameList()
        {
            if(ViewModel.Items.Any(x => x.Full))
            {
                ViewModel.Items = new ObservableCollection<Game>(ViewModel.Items.OrderBy(x => x.Full).ToList());
            }            
        }

        private void GameList_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                Game game = (Game)gameList.SelectedItem;
                string gameName = game.Name;

                if (game.Id.Equals(-1))
                {
                    gameName = RoomHelper.RandomizeRoomName(ViewModel.Items);
                    GameHelper.AddGame(gameName);
                }
                else if(game.Full)
                {
                    return; //Do not do anything if game is full
                }
                else
                {
                    GameHelper.JoinGame(gameName);
                }
            }
            catch (Exception ex)
            {
                ViewModel.ErrorMessage = ex.Message;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                base.OnNavigatedTo(e);

                var comingFromSettingsPages = false; 
                if (!string.IsNullOrEmpty(e.Parameter.ToString()))
                {
                    comingFromSettingsPages = (bool)e.Parameter;
                }                

                if (!comingFromSettingsPages)
                {
                    ConnectToMaster();

                    if (!(App.Current as App).LoadBalancingClient.InLobby)
                    {
                        ConnectToLobby();
                    }
                }

                GetGames();
            }
            catch (Exception ex)
            {
                ViewModel.ErrorMessage = ex.Message;
            }
        }

        public void NavigateToGame()
        {
            this.Frame.Navigate(typeof(GamePage), null, new DrillInNavigationTransitionInfo());
        }
    }
}
