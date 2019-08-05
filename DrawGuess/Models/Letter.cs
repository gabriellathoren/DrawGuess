using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DrawGuess.Models
{
    public class Letter : INotifyPropertyChanged
    {
        private string letter;
        public string Character
        {
            get { return this.letter; }
            set
            {
                this.letter = value;
                this.OnPropertyChanged();
            }
        }

        private bool visibility;
        public bool Visibility
        {
            get { return this.visibility; }
            set
            {
                this.visibility = !value;
                this.OnPropertyChanged();
            }
        }

        public Letter()
        {
            Character = "";
            Visibility = true; 
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
