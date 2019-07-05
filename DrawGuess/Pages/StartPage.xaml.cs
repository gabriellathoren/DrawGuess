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
            

            //TESTKOD
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
