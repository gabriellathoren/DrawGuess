using DrawGuess.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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
                this.OnPropertyChanged();
            }
        }

        private ObservableCollection<string> randomLetters;
        public ObservableCollection<string> RandomLetters
        {
            get { return this.randomLetters; }
            set
            {
                this.randomLetters = value;
                this.OnPropertyChanged();
            }
        }

        private ObservableCollection<string> guess;
        public ObservableCollection<string> Guess
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


        public GameViewModel()
        {
            Game = new Game();
            Players = new ObservableCollection<Player>();
            Guess = new ObservableCollection<string>();
            RandomLetters = new ObservableCollection<string>();
            User = (App.Current as App).User;
            ShowGame = false;
            ShowInfoView = false;
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
