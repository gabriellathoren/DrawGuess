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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace DrawGuess.Pages
{
    public sealed partial class GamePage : Page
    {
        public GameViewModel ViewModel { get; set; }
        private Random Random = new Random();


        public GamePage()
        {
            this.InitializeComponent();
            ViewModel = new GameViewModel();
            ViewModel.Round = 1;
            ViewModel.SecretWord = "LEMONS";

            // Initialize the InkCanvas
            InkCanvas.InkPresenter.InputDeviceTypes =
                Windows.UI.Core.CoreInputDeviceTypes.Mouse |
                Windows.UI.Core.CoreInputDeviceTypes.Pen;

            SetPlayers();
            SetPlacement();
            SetHint();
            SetRandomLetters();
        }

        public void SetPlayers()
        {
            ViewModel.Players.Add(new PlayersViewModel { Id = 1, Name = "Gabriella Thorén", Points = 10, Painter = false, RightAnswer = false, IsCurrentUser = true, ProfilePicture = "https://scontent-arn2-1.xx.fbcdn.net/v/t1.0-9/55807325_2778073192210585_6307069374352064512_n.jpg?_nc_cat=108&_nc_oc=AQk9gg8bPNJLK_rSrDfDZOCNwg-GhY1tHWPwHkZRSSwBpLIVfb71EAsO8zbSJzxQzJg&_nc_ht=scontent-arn2-1.xx&oh=106558072dcc414195889611c828acd5&oe=5DBBAA6F"});;
            ViewModel.Players.Add(new PlayersViewModel { Id = 2, Name = "Anna Anna Anna Anna Anna Anna Anna Anna Thorén", Points = 100, Painter = false, RightAnswer = false, ProfilePicture = "https://scontent-arn2-1.xx.fbcdn.net/v/t1.0-9/55807325_2778073192210585_6307069374352064512_n.jpg?_nc_cat=108&_nc_oc=AQk9gg8bPNJLK_rSrDfDZOCNwg-GhY1tHWPwHkZRSSwBpLIVfb71EAsO8zbSJzxQzJg&_nc_ht=scontent-arn2-1.xx&oh=106558072dcc414195889611c828acd5&oe=5DBBAA6F" });
            ViewModel.Players.Add(new PlayersViewModel { Id = 3, Name = "Tomas Thorén", Points = 60, Painter = true, RightAnswer = false, ProfilePicture = "https://scontent-arn2-1.xx.fbcdn.net/v/t1.0-9/55807325_2778073192210585_6307069374352064512_n.jpg?_nc_cat=108&_nc_oc=AQk9gg8bPNJLK_rSrDfDZOCNwg-GhY1tHWPwHkZRSSwBpLIVfb71EAsO8zbSJzxQzJg&_nc_ht=scontent-arn2-1.xx&oh=106558072dcc414195889611c828acd5&oe=5DBBAA6F" });
            ViewModel.Players.Add(new PlayersViewModel { Id = 4, Name = "Mia Thorén", Points = 50, Painter = false, RightAnswer = false, ProfilePicture = "https://scontent-arn2-1.xx.fbcdn.net/v/t1.0-9/55807325_2778073192210585_6307069374352064512_n.jpg?_nc_cat=108&_nc_oc=AQk9gg8bPNJLK_rSrDfDZOCNwg-GhY1tHWPwHkZRSSwBpLIVfb71EAsO8zbSJzxQzJg&_nc_ht=scontent-arn2-1.xx&oh=106558072dcc414195889611c828acd5&oe=5DBBAA6F" });
            ViewModel.Players.Add(new PlayersViewModel { Id = 5, Name = "Peter Thorén", Points = 50, Painter = false, RightAnswer = false, ProfilePicture = "https://scontent-arn2-1.xx.fbcdn.net/v/t1.0-9/55807325_2778073192210585_6307069374352064512_n.jpg?_nc_cat=108&_nc_oc=AQk9gg8bPNJLK_rSrDfDZOCNwg-GhY1tHWPwHkZRSSwBpLIVfb71EAsO8zbSJzxQzJg&_nc_ht=scontent-arn2-1.xx&oh=106558072dcc414195889611c828acd5&oe=5DBBAA6F" });
            ViewModel.Players.Add(new PlayersViewModel { Id = 6, Name = "Anton Thorén", Points = 56, Painter = false, RightAnswer = false, ProfilePicture = "https://scontent-arn2-1.xx.fbcdn.net/v/t1.0-9/55807325_2778073192210585_6307069374352064512_n.jpg?_nc_cat=108&_nc_oc=AQk9gg8bPNJLK_rSrDfDZOCNwg-GhY1tHWPwHkZRSSwBpLIVfb71EAsO8zbSJzxQzJg&_nc_ht=scontent-arn2-1.xx&oh=106558072dcc414195889611c828acd5&oe=5DBBAA6F" });
            ViewModel.Players.Add(new PlayersViewModel { Id = 7, Name = "Anja Thorén", Points = 49, Painter = false, RightAnswer = true, ProfilePicture = "https://scontent-arn2-1.xx.fbcdn.net/v/t1.0-9/55807325_2778073192210585_6307069374352064512_n.jpg?_nc_cat=108&_nc_oc=AQk9gg8bPNJLK_rSrDfDZOCNwg-GhY1tHWPwHkZRSSwBpLIVfb71EAsO8zbSJzxQzJg&_nc_ht=scontent-arn2-1.xx&oh=106558072dcc414195889611c828acd5&oe=5DBBAA6F" });
            ViewModel.Players.Add(new PlayersViewModel { Id = 8, Name = "Nova Thorén", Points = 1, Painter = false, RightAnswer = false, ProfilePicture = "https://scontent-arn2-1.xx.fbcdn.net/v/t1.0-9/55807325_2778073192210585_6307069374352064512_n.jpg?_nc_cat=108&_nc_oc=AQk9gg8bPNJLK_rSrDfDZOCNwg-GhY1tHWPwHkZRSSwBpLIVfb71EAsO8zbSJzxQzJg&_nc_ht=scontent-arn2-1.xx&oh=106558072dcc414195889611c828acd5&oe=5DBBAA6F" });
            ViewModel.Players.Add(new PlayersViewModel { Id = 9, Name = "Lovisa Thorén", Points = 35, Painter = false, RightAnswer = true, ProfilePicture = "https://scontent-arn2-1.xx.fbcdn.net/v/t1.0-9/55807325_2778073192210585_6307069374352064512_n.jpg?_nc_cat=108&_nc_oc=AQk9gg8bPNJLK_rSrDfDZOCNwg-GhY1tHWPwHkZRSSwBpLIVfb71EAsO8zbSJzxQzJg&_nc_ht=scontent-arn2-1.xx&oh=106558072dcc414195889611c828acd5&oe=5DBBAA6F" });
            ViewModel.Players.Add(new PlayersViewModel { Id = 10, Name = "Laura Thorén", Points = 10, Painter = false, RightAnswer = false, ProfilePicture = "https://scontent-arn2-1.xx.fbcdn.net/v/t1.0-9/55807325_2778073192210585_6307069374352064512_n.jpg?_nc_cat=108&_nc_oc=AQk9gg8bPNJLK_rSrDfDZOCNwg-GhY1tHWPwHkZRSSwBpLIVfb71EAsO8zbSJzxQzJg&_nc_ht=scontent-arn2-1.xx&oh=106558072dcc414195889611c828acd5&oe=5DBBAA6F" });
        }

        public void SetRandomLetters()
        {
            //Get letters from secret word to add to hinting letters
            for (int i = 0; i < ViewModel.SecretWord.Length; i++)
            {
                ViewModel.RandomLetters.Add(ViewModel.SecretWord[i].ToString());
            }

            //Add random letters
            int noOfLetters = 15 - ViewModel.SecretWord.Length;
            
            for (int i = 0; i < noOfLetters; i++)
            {
                ViewModel.RandomLetters.Add(GetRandomLetter());
            }

            //Randomize order of letters
            Shuffle();

        }

        public void Shuffle()
        {
            int n = ViewModel.RandomLetters.Count;

            while (n > 1)
            {
                n--;
                int k = Random.Next(n + 1);
                string value = ViewModel.RandomLetters[k];
                ViewModel.RandomLetters[k] = ViewModel.RandomLetters[n];
                ViewModel.RandomLetters[n] = value;
            }
        }

        public string GetRandomLetter()
        {            
            string st = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(st, 1).Select(s => s[Random.Next(s.Length)]).ToArray());
        }

        public void SetHint()
        {
            //Set hinting boxes based on number of letters in secret word
            for(int i = 0; i < ViewModel.SecretWord.Length; i++)
            {
                ViewModel.Guess.Add("");
            }
        }

        public void SetPlacement()
        {
            ViewModel.Players = new ObservableCollection<PlayersViewModel>(ViewModel.Players.OrderBy(x => x.Points).ToList());

            int placement = 1;
            foreach(PlayersViewModel p in ViewModel.Players)
            {
                if(ViewModel.Players.IndexOf(p) == 0)
                {
                    p.Placement = placement;
                }
                else if(p.Points == ViewModel.Players[ViewModel.Players.IndexOf(p)-1].Points)
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel.Room = (GameRoom) e.Parameter;  
        }
        
        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(StartPage), "", new SuppressNavigationTransitionInfo());
        }

        private void InkToolbar_EraseAllClicked(InkToolbar sender, object args)
        {

        }
    }
}
