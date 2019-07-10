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

        private ObservableCollection<PlayersViewModel> players;
        public ObservableCollection<PlayersViewModel> Players
        {
            get { return this.players; }
            set
            {
                this.players = value;
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
        

        public GameViewModel()
        {
            Players = new ObservableCollection<PlayersViewModel>();
            Guess = new ObservableCollection<string>();
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
