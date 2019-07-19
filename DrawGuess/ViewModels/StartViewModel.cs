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


        private ObservableCollection<Game> items;
        public ObservableCollection<Game> Items
        {
            get { return this.items; }
            set
            {
                this.items = value;
                this.OnPropertyChanged(nameof(Items));
            }
        }

        private Game selectedItem;
        public Game SelectedItem
        {
            get { return this.selectedItem; }
            set
            {
                this.selectedItem = value;
                this.OnPropertyChanged();
            }
        }

        public StartViewModel()
        {
            Items = new ObservableCollection<Game>();
            User = (App.Current as App).User;
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
