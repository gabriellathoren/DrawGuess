using DrawGuess.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Input.Inking;

namespace DrawGuess.ViewModels
{
    public class GameViewModel : INotifyPropertyChanged
    {
        private Game game;
        public Game Game
        {
            get { return this.game; }
            set
            {
                this.game = value;
                this.OnPropertyChanged();
            }
        }

        private User user;
        public User User
        {
            get { return this.user; }
            set
            {
                this.user = value;
                this.OnPropertyChanged();
            }
        }

        private Player currentPlayer; 
        public Player CurrentPlayer
        {
            get { return this.currentPlayer; }
            set
            {
                this.currentPlayer = value;
                this.OnPropertyChanged();
            }
        }

        private string errorMessage;
        public string ErrorMessage
        {
            get { return this.errorMessage; }
            set
            {
                this.errorMessage = value;
                this.OnPropertyChanged();
            }
        }

        private ObservableCollection<Player> players;
        public ObservableCollection<Player> Players
        {
            get { return this.players; }
            set
            {
                this.players = value;
                this.currentPlayer = players.Where(x => x.IsCurrentUser == true).FirstOrDefault();
                this.OnPropertyChanged();
            }
        }

        private ObservableCollection<Letter> randomLetters;
        public ObservableCollection<Letter> RandomLetters
        {
            get { return this.randomLetters; }
            set
            {
                this.randomLetters = value;
                this.OnPropertyChanged();
            }
        }

        private ObservableCollection<Letter> guess;
        public ObservableCollection<Letter> Guess
        {
            get { return this.guess; }
            set
            {
                this.guess = value;
                this.OnPropertyChanged();
            }
        }
        
        private bool showInfoView;
        public bool ShowInfoView
        {
            get { return this.showInfoView; }
            set
            {
                this.showInfoView = value;
                this.OnPropertyChanged();
            }
        }

        private bool painterView;
        public bool PainterView
        {
            get { return this.painterView; }
            set
            {
                this.painterView = value;
                this.OnPropertyChanged();
            }
        }

        private bool showGame;
        public bool ShowGame
        {
            get { return this.showGame; }
            set
            {
                this.showGame = value;
                this.OnPropertyChanged();
            }
        }

        private bool showImage;
        public bool ShowImage
        {
            get { return this.showImage; }
            set
            {
                this.showImage = value;
                this.OnPropertyChanged();
            }
        }

        private bool showPlacement;
        public bool ShowPlacement
        {
            get { return this.showPlacement; }
            set
            {
                this.showPlacement = value;
                this.OnPropertyChanged();
            }
        }

        private GameMode currentMode;
        public GameMode CurrentMode
        {
            get { return currentMode; }
            set
            {
                currentMode = value;
                this.OnPropertyChanged();
            }
        }

        public GameViewModel()
        {
            Game = new Game();
            Players = new ObservableCollection<Player>();
            Guess = new ObservableCollection<Letter>();
            RandomLetters = new ObservableCollection<Letter>();
            User = (App.Current as App).User;
            ShowGame = false;
            ShowInfoView = false;
            ShowPlacement = false; 
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
