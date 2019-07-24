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
                Game.Name = Game.Name.ToUpper();
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

        private string infoViewRow1;
        public string InfoViewRow1
        {
            get { return this.infoViewRow1; }
            set
            {
                this.infoViewRow1 = value;
                this.OnPropertyChanged();
            }
        }

        private string infoViewRow2;
        public string InfoViewRow2
        {
            get { return this.infoViewRow2; }
            set
            {
                this.infoViewRow2 = value;
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

        public bool ShowInfoView { get; set; }

        private bool showGame;
        public bool ShowGame
        {
            get { return this.showGame; }
            set
            {
                this.showGame = value;
                ShowInfoView = !value;
                this.OnPropertyChanged();
            }
        }


        public GameViewModel()
        {
            Players = new ObservableCollection<Player>();
            Guess = new ObservableCollection<string>();
            RandomLetters = new ObservableCollection<string>();
            User = (App.Current as App).User;
            InfoViewRow1 = "";
            InfoViewRow2 = "";
            ShowGame = false;
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
