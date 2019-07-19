using DrawGuess.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DrawGuess.ViewModels
{
    public class PlayersViewModel : INotifyPropertyChanged
    {
        private Player player;
        public Player Player
        {
            get { return this.player; }
            set
            {
                this.player = value;
                this.OnPropertyChanged();
            }
        }

        private int placement;
        public int Placement
        {
            get { return this.placement; }
            set
            {
                this.placement = value;
                this.OnPropertyChanged();
            }
        }

        private bool painter;
        public bool Painter
        {
            get { return this.painter; }
            set
            {
                this.painter = value;
                this.OnPropertyChanged();
            }
        }

        private bool rightAnswer;
        public bool RightAnswer
        {
            get { return this.rightAnswer; }
            set
            {
                this.rightAnswer = value;
                this.OnPropertyChanged();
            }
        }                

        public PlayersViewModel(Player player)
        {
            Placement = 0;
            Player = player;
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
