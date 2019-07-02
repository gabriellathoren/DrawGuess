using DrawGuess.Models;
using DrawGuess.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace DrawGuess.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class StartPage : Page
    {

        public StartViewModel ViewModel { get; set; }

       
        public StartPage()
        {
            this.InitializeComponent();
            this.ViewModel = new StartViewModel();

            //TESTKOD
            ViewModel.UserName = "Gabriella Thorén";
            ViewModel.ProfilePicture = "https://scontent-arn2-1.xx.fbcdn.net/v/t1.0-9/55807325_2778073192210585_6307069374352064512_n.jpg?_nc_cat=108&_nc_oc=AQk9gg8bPNJLK_rSrDfDZOCNwg-GhY1tHWPwHkZRSSwBpLIVfb71EAsO8zbSJzxQzJg&_nc_ht=scontent-arn2-1.xx&oh=106558072dcc414195889611c828acd5&oe=5DBBAA6F";
            ViewModel.Points = "1000 XP";
            ViewModel.Items.Add(new GameRoom() { RoomName = "Violett", NumberOfPlayers = 3 });
            ViewModel.Items.Add(new GameRoom() { RoomName = "Golden", NumberOfPlayers = 5 });
            ViewModel.Items.Add(new GameRoom() { RoomName = "Marin", NumberOfPlayers = 7 });
            ViewModel.Items.Add(new GameRoom() { RoomName = "Silver", NumberOfPlayers = 3 });
            ViewModel.Items.Add(new GameRoom() { RoomName = "Shimmer", NumberOfPlayers = 3 });
            ViewModel.Items.Add(new GameRoom() { RoomName = "Coco white", NumberOfPlayers = 9 });
            ViewModel.Items.Add(new GameRoom() { RoomName = "Ocean blue", NumberOfPlayers = 8 });
            ViewModel.Items.Add(new GameRoom() { RoomName = "Marmor", NumberOfPlayers = 3 });
            ViewModel.Items.Add(new GameRoom() { RoomName = "Coco white", NumberOfPlayers = 10 });
            ViewModel.Items.Add(new GameRoom() { RoomName = "Ocean blue", NumberOfPlayers = 10 });
            ViewModel.Items.Add(new GameRoom() { RoomName = "Marmor", NumberOfPlayers = 10 });
            ViewModel.Items.Add(new GameRoom() { RoomName = "Violett", NumberOfPlayers = 3 });
            ViewModel.Items.Add(new GameRoom() { RoomName = "Golden", NumberOfPlayers = 5 });
            ViewModel.Items.Add(new GameRoom() { RoomName = "Marin", NumberOfPlayers = 7 });
            ViewModel.Items.Add(new GameRoom() { RoomName = "Silver", NumberOfPlayers = 3 });
            ViewModel.Items.Add(new GameRoom() { RoomName = "Shimmer", NumberOfPlayers = 3 });
            ViewModel.Items.Add(new GameRoom() { RoomName = "Coco white", NumberOfPlayers = 9 });
            ViewModel.Items.Add(new GameRoom() { RoomName = "Ocean blue", NumberOfPlayers = 8 });
            ViewModel.Items.Add(new GameRoom() { RoomName = "Marmor", NumberOfPlayers = 3 });
            ViewModel.Items.Add(new GameRoom() { RoomName = "Coco white", NumberOfPlayers = 10 });
            ViewModel.Items.Add(new GameRoom() { RoomName = "Ocean blue", NumberOfPlayers = 10 });
            ViewModel.Items.Add(new GameRoom() { RoomName = "Marmor", NumberOfPlayers = 10 });

            SortRoomList();
        }

        public void SortRoomList()
        {
            ViewModel.Items = new ObservableCollection<GameRoom>(ViewModel.Items.OrderBy(x => x.Full).ToList());

        }

        private void GameRoomsList_Tapped(object sender, TappedRoutedEventArgs e)
        {
            GameRoom room = (GameRoom) gameRoomsList.SelectedItem;
            room.RoomName = room.RoomName.ToUpper();

            this.Frame.Navigate(typeof(GamePage), room, new DrillInNavigationTransitionInfo());
        }

        private void Navigation_SettingsClick(object sender, EventArgs e)
        {
            this.Frame.Navigate(typeof(SettingsPage), null, new SuppressNavigationTransitionInfo());
        }
    }

    public class GameRoomDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate NewGameTemplate { get; set; }
        public DataTemplate GameRoomTemplate { get; set; }
        public DataTemplate FullRoomTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            GameRoom room = (GameRoom)item;

            DataTemplate _returnTemplate = new DataTemplate();
            var itemsControl = ItemsControl.ItemsControlFromItemContainer(container);

            if(itemsControl.IndexFromContainer(container) == 0)
            {
                _returnTemplate = NewGameTemplate;
            }
            else if (room.Full)
            {
                _returnTemplate = FullRoomTemplate;
            }
            else
            {
                _returnTemplate = GameRoomTemplate;
            }

            return _returnTemplate;
        }
    }

}
