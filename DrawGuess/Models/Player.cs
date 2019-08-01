using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DrawGuess.Models
{
    public class Player : INotifyPropertyChanged
    {
        private string userId;
        public string UserId
        {
            get { return this.userId; }
            set
            {
                this.userId = value;
                this.OnPropertyChanged();
            }
        }

        private string nickName;
        public string NickName
        {
            get { return this.nickName; }
            set
            {
                this.nickName = value;
                this.OnPropertyChanged();
            }
        }

        private int points;
        public int Points
        {
            get { return this.points; }
            set
            {
                this.points = value;
                this.OnPropertyChanged();
            }
        }

        private bool isCurrentUser;
        public bool IsCurrentUser
        {
            get { return this.isCurrentUser; }
            set
            {
                this.isCurrentUser = value;
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


        public Player()
        {
            Placement = 0;
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
