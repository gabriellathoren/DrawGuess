using DrawGuess.Models;
using DrawGuess.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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

            // Initialize the InkCanvas
            InkCanvas.InkPresenter.InputDeviceTypes =
                Windows.UI.Core.CoreInputDeviceTypes.Mouse |
                Windows.UI.Core.CoreInputDeviceTypes.Pen;

        }

        public void SetGame()
        {
            SetPlayerPoints(0);
            SetPlayers();
            SetPlacement();
            SetSecretWord();
            SetHint();
            //SetRandomLetters(); 
        }

        public void SetPlayerPoints(int points)
        {
            try
            {
                Game.SetPlayerPoints(points);
            }
            catch(Exception)
            {
                ViewModel.ErrorMessage = "Could not set player points";
            }
        }

        public void SetPlayers()
        {
            try
            {
                ObservableCollection<Player> players = Game.GetPlayers();

                foreach (Player player in players)
                {
                    
                    ViewModel.Players.Add(new PlayersViewModel(player));
                }
            }
            catch (Exception e)
            {
                ViewModel.ErrorMessage = e.Message;
            }
        }

        public void SetSecretWord()
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

        //public void SetRandomLetters()
        //{
        //    //Get letters from secret word to add to hinting letters
        //    for (int i = 0; i < ViewModel.Game.SecretWord.Length; i++)
        //    {
        //        ViewModel.Game.RandomLetters.Add(ViewModel.Game.SecretWord[i].ToString());
        //    }

        //    //Add random letters
        //    int noOfLetters = 15 - ViewModel.Game.SecretWord.Length;

        //    for (int i = 0; i < noOfLetters; i++)
        //    {
        //        ViewModel.Game.RandomLetters.Add(GetRandomLetter());
        //    }

        //    //Randomize order of letters
        //    Shuffle();
        //}

        //public void Shuffle()
        //{
        //    int n = ViewModel.Game.RandomLetters.Count;

        //    while (n > 1)
        //    {
        //        n--;
        //        int k = Random.Next(n + 1);
        //        string value = ViewModel.Game.RandomLetters[k];
        //        ViewModel.Game.RandomLetters[k] = ViewModel.Game.RandomLetters[n];
        //        ViewModel.Game.RandomLetters[n] = value;
        //    }
        //}

        //public string GetRandomLetter()
        //{            
        //    string st = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        //    return new string(Enumerable.Repeat(st, 1).Select(s => s[Random.Next(s.Length)]).ToArray());
        //}

        public void SetHint()
        {
            //Set hinting boxes based on number of letters in secret word
            for (int i = 0; i < ViewModel.Game.SecretWord.Length; i++)
            {
                ViewModel.Guess.Add("");
            }
        }

        public void SetPlacement()
        {
            ViewModel.Players = new ObservableCollection<PlayersViewModel>(ViewModel.Players.OrderBy(x => x.Player.Points).ToList());

            int placement = 1;
            foreach (PlayersViewModel p in ViewModel.Players)
            {
                if (ViewModel.Players.IndexOf(p) == 0)
                {
                    p.Placement = placement;
                }
                else if (p.Player.Points == ViewModel.Players[ViewModel.Players.IndexOf(p) - 1].Player.Points)
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
            try
            {
                ViewModel.Game = Game.GetGame();
                SetGame();
            }
            catch(Exception ex)
            {
                ViewModel.ErrorMessage = ex.Message;
            }
        }

        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            Game.LeaveGame();

            while (!Game.LeftRoom)
            {
                Task.Delay(TimeSpan.FromMilliseconds(100));
            }

            this.Frame.Navigate(typeof(StartPage), "", new SuppressNavigationTransitionInfo());
        }

        private void InkToolbar_EraseAllClicked(InkToolbar sender, object args)
        {

        }
    }
}
