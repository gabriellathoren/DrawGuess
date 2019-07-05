using DrawGuess.Models;
using DrawGuess.Security;
using DrawGuess.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Credentials;
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
    public sealed partial class SignUpPage : Page
    {
        private bool Register_clicked { get; set; }
        private SignUpViewModel ViewModel { get; set; }

        public SignUpPage()
        {
            this.InitializeComponent();
            ViewModel = new SignUpViewModel();
            ViewModel.User = new Models.User();
            Register_clicked = false;
        }


        private void Return_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(LoginPage), "", new SuppressNavigationTransitionInfo());
        }

        private bool IsAllObligatoryFieldsFilled()
        {
            bool allObligatoryFieldsFilled = true;

            if (string.IsNullOrEmpty(ViewModel.User.FirstName))
            {
                firstNameBox.BorderBrush = new SolidColorBrush(Colors.Red);
                allObligatoryFieldsFilled = false; 
            }
            if (string.IsNullOrEmpty(ViewModel.User.LastName))
            {
                lastNameBox.BorderBrush = new SolidColorBrush(Colors.Red);
                allObligatoryFieldsFilled = false;
            }
            if (string.IsNullOrEmpty(ViewModel.User.Email))
            {
                emailBox.BorderBrush = new SolidColorBrush(Colors.Red);
                allObligatoryFieldsFilled = false;
            }
            if (string.IsNullOrEmpty(ViewModel.Password))
            {
                passwordBox.BorderBrush = new SolidColorBrush(Colors.Red);
                allObligatoryFieldsFilled = false;
            }
            if (string.IsNullOrEmpty(ViewModel.ConfirmedPassword))
            {
                passwordConfirmBox.BorderBrush = new SolidColorBrush(Colors.Red);
                allObligatoryFieldsFilled = false;
            }

            if (!allObligatoryFieldsFilled)
            {
                ViewModel.ErrorMessage = "Please fill in all obligatory fields to register";
            }

            return allObligatoryFieldsFilled;

        }

        private void ResetErrorIndicators()
        {
            
            firstNameBox.BorderBrush = new SolidColorBrush(Color.FromArgb(66, 0, 0, 0));
            lastNameBox.BorderBrush = new SolidColorBrush(Color.FromArgb(66, 0, 0, 0));
            emailBox.BorderBrush = new SolidColorBrush(Color.FromArgb(66, 0, 0, 0));
            passwordBox.BorderBrush = new SolidColorBrush(Color.FromArgb(66, 0, 0, 0));
            ViewModel.ErrorMessage = "";
        }

        private bool IsEmailValidated()
        {
            bool IsEmailAccepted = true;

            try
            {
                var addr = new System.Net.Mail.MailAddress(ViewModel.User.Email);
                IsEmailAccepted = addr.Address == ViewModel.User.Email;
            }
            catch
            {
                ViewModel.ErrorMessage = "Please specify a correct email";
                emailBox.BorderBrush = new SolidColorBrush(Colors.Red);
                return false;
            }

            if(Models.User.DoesUserExists(ViewModel.User.Email))
            {
                ViewModel.ErrorMessage = "Email is already registered";
                emailBox.BorderBrush = new SolidColorBrush(Colors.Red);
                return false;
            }

            return IsEmailAccepted;
        }
        
        private bool IsPasswordsAccepted()
        {

            if(ViewModel.Password.Length < 8)
            {
                passwordConfirmBox.BorderBrush = new SolidColorBrush(Colors.Red);
                passwordBox.BorderBrush = new SolidColorBrush(Colors.Red);
                ViewModel.ErrorMessage = "Password must be longer than 8 characters";
                return false;
            }

            if(!ViewModel.Password.Equals(ViewModel.ConfirmedPassword))
            {
                passwordConfirmBox.BorderBrush = new SolidColorBrush(Colors.Red);
                passwordBox.BorderBrush = new SolidColorBrush(Colors.Red);
                ViewModel.ErrorMessage = "The specified passwords do not match";
                return false;
            }

            return true;
        }


        private void Register_Click(object sender, RoutedEventArgs e)
        {
            if (Register_clicked) { return; }
            else { Register_clicked = true; }

            ResetErrorIndicators();

            if (IsAllObligatoryFieldsFilled() && IsEmailValidated() && IsPasswordsAccepted())
            {
                try
                {
                    Models.User.AddUser(ViewModel.User, ViewModel.Password);                    
                    CredentialControl.SystemLogIn(ViewModel.User.Email, ViewModel.Password);
                    this.Frame.Navigate(typeof(StartPage), "", new SuppressNavigationTransitionInfo());
                }
                catch(Exception)
                {
                    ViewModel.ErrorMessage = "Something went wrong, please try again later";
                }
            }

            Register_clicked = false;
        }

        private void EnterPressed(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                Register_Click(sender, e);
            }
        }
    }
}
