using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DrawGuess.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private string email;
        public String Email
        {
            get { return this.email; }
            set
            {
                this.email = value;
                this.OnPropertyChanged();
            }
        }

        private string password;
        public String Password
        {
            get { return this.password; }
            set
            {
                this.password = value;
                this.OnPropertyChanged();
            }
        }

        private string errorMessage;
        public String ErrorMessage
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
    }
}
