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
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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


        public StartPage()
        {
            if ((App.Current as App).User.Equals(null))
            {
                this.Frame.Navigate(typeof(LoginPage), null, new SuppressNavigationTransitionInfo());
                return;
            }

            this.InitializeComponent();
            this.ViewModel = new StartViewModel();

            GetGames();
            SortGameList();
        }

        public void GetGames()
        {
            try
            {
                ViewModel.Items = Models.Game.GetGames();
                ViewModel.Items.Insert(0, new Game()
                {
                    Id = -1
                });
            }
            catch(Exception)
            {

            }
        }

        public void SortGameList()
        {
            ViewModel.Items = new ObservableCollection<Game>(ViewModel.Items.OrderBy(x => x.Full).ToList());

        }
        
        private void GameList_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                Game game = (Game)gameList.SelectedItem;
                string gameName = game.Name;

                if (game.Id.Equals(-1))
                {
                    gameName = Models.Game.RandomizeRoomName(ViewModel.Items);
                    Models.Game.AddGame(gameName);
                    NavigateToGame(true);
                }
                else
                {
                    Models.Game.JoinGame(gameName);
                    NavigateToGame();
                }

                
            }
            catch(Exception ex)
            {
                ViewModel.ErrorMessage = ex.Message;
            }
        }

        public void NavigateToGame(bool newGame = false)
        {            
            if(newGame)
            {
                //Wait for room being created in Photon
                (App.Current as App).LoadBalancingClient.MatchMakingCallbackTargets.StopWaitCreatedRoom.WaitOne();
            }
            
            //Wait for user to join game in Photon
            (App.Current as App).LoadBalancingClient.MatchMakingCallbackTargets.StopWaitJoinedRoom.WaitOne();

            this.Frame.Navigate(typeof(GamePage), null, new DrillInNavigationTransitionInfo());
        }
    }


    public class GameDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate NewGameTemplate { get; set; }
        public DataTemplate GameTemplate { get; set; }
        public DataTemplate FullGameTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            Game game = (Game)item;

            DataTemplate _returnTemplate = new DataTemplate();
            var itemsControl = ItemsControl.ItemsControlFromItemContainer(container);

            if(itemsControl.IndexFromContainer(container) == 0)
            {
                _returnTemplate = NewGameTemplate;
            }
            else if (game.Full)
            {
                _returnTemplate = FullGameTemplate;
            }
            else
            {
                _returnTemplate = GameTemplate;
            }

            return _returnTemplate;
        }
    }

}
