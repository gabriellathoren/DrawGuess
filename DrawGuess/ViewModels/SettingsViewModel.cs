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
    public class SettingsViewModel : INotifyPropertyChanged
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

        private string initials;
        public string Initials
        {
            get { return this.initials; }
            set
            {
                this.initials = value;
                this.OnPropertyChanged();
            }
        }

        private string oldPassword;
        public string OldPassword
        {
            get { return this.oldPassword; }
            set
            {
                this.oldPassword = value;
                this.OnPropertyChanged();
            }
        }

        private string newPassword;
        public string NewPassword
        {
            get { return this.newPassword; }
            set
            {
                this.newPassword = value;
                this.OnPropertyChanged();
            }
        }

        private string confirmNewPassword;
        public string ConfirmNewPassword
        {
            get { return this.confirmNewPassword; }
            set
            {
                this.confirmNewPassword = value;
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

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public SettingsViewModel()
        {
            User = (App.Current as App).User;
            Initials = User.FirstName.Substring(0, 1).ToUpper() + User.LastName.Substring(0, 1).ToUpper();
        }

    }
}
