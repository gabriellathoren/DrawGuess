using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DrawGuess.ViewModels
{
    public class NavigationViewModel : INotifyPropertyChanged
    {
        private string userName;
        public String UserName
        {
            get { return this.userName; }
            set
            {
                this.userName = value;
                this.OnPropertyChanged();
            }
        }

        private string profilePicture;
        public String ProfilePicture
        {
            get { return this.profilePicture; }
            set
            {
                this.profilePicture = value;
                this.OnPropertyChanged();
            }
        }

        private string initials;
        public String Initials
        {
            get { return this.initials; }
            set
            {
                this.initials = value;
                this.OnPropertyChanged();
            }
        }

        private Models.User user;
        public Models.User User
        {
            get { return this.user; }
            set
            {
                this.user = value;
                this.OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public NavigationViewModel()
        {
            User = (App.Current as App).User;
            UserName = User.FirstName + " " + User.LastName;
            Initials = User.FirstName.Substring(0, 1).ToUpper() + User.LastName.Substring(0, 1).ToUpper();
        }
    }
}
