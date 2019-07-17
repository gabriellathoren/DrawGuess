using DrawGuess.Exceptions;
using DrawGuess.Security;
using DrawGuess.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Credentials;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;


namespace DrawGuess.Pages
{
    public sealed partial class LoginPage : Page
    {
        public LoginViewModel ViewModel;
        private bool Login_clicked = false;
        private bool SignUp_clicked = false;

        public LoginPage()
        {
            this.InitializeComponent();
            ViewModel = new LoginViewModel();
        }

        private void SignUp_Click(object sender, RoutedEventArgs e)
        {
            if (SignUp_clicked) { return; }
            else { SignUp_clicked = true; }

            this.Frame.Navigate(typeof(SignUpPage), "", new SuppressNavigationTransitionInfo());

            SignUp_clicked = false;
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            if (Login_clicked) { return; }
            else { Login_clicked = true; }

            ResetErrorIndicators();

            if(IsAllObligatoryFieldsFilled())
            {
                try
                {
                    Models.User user = Models.User.GetUser(ViewModel.Email, ViewModel.Password);

                    if (user != null)
                    {
                        LogIn(user);
                    }
                }
                catch(UserNotFoundException)
                {
                    ViewModel.ErrorMessage = "Wrong email or password";
                }
                catch(Exception)
                {
                    ViewModel.ErrorMessage = "Something went wrong, try again later";
                }
            }
            
            Login_clicked = false;
        }

        private async void LogIn(Models.User user)
        {
            try
            {
                var credentialControl = new CredentialControl();
                await credentialControl.SystemLogIn(user.Email, ViewModel.Password, user);
                this.Frame.Navigate(typeof(StartPage), "", new SuppressNavigationTransitionInfo());
            }
            catch(Exception e)
            {
                ViewModel.ErrorMessage = e.Message;
            }
        }

        private void EnterPressed(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                Login_Click(sender, e);
            }
        }

        private bool IsAllObligatoryFieldsFilled()
        {
            bool allObligatoryFieldsFilled = true;

            if (string.IsNullOrEmpty(ViewModel.Email))
            {
                emailBox.BorderBrush = new SolidColorBrush(Colors.Red);
                allObligatoryFieldsFilled = false;
            }
            if (string.IsNullOrEmpty(ViewModel.Password))
            {
                passwordBox.BorderBrush = new SolidColorBrush(Colors.Red);
                allObligatoryFieldsFilled = false;
            }

            if (!allObligatoryFieldsFilled)
            {
                ViewModel.ErrorMessage = "Please fill in both email and password to log in";
            }

            return allObligatoryFieldsFilled;
        }

        private void ResetErrorIndicators()
        {
            emailBox.BorderBrush = new SolidColorBrush(Color.FromArgb(66, 0, 0, 0));
            passwordBox.BorderBrush = new SolidColorBrush(Color.FromArgb(66, 0, 0, 0));
            ViewModel.ErrorMessage = "";
        }
    }
}
