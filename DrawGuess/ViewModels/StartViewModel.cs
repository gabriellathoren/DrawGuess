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
    public class StartViewModel : INotifyPropertyChanged
    {

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

        private ObservableCollection<GameRoom> items;
        public ObservableCollection<GameRoom> Items
        {
            get { return this.items; }
            set
            {
                this.items = value;
                this.OnPropertyChanged();
            }
        }
        
        public StartViewModel()
        {
            Items = new ObservableCollection<GameRoom>();
            User = (App.Current as App).User;
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
