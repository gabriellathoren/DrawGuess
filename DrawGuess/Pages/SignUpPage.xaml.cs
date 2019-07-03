using DrawGuess.Models;
using DrawGuess.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace DrawGuess.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
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
                allObligatoryFieldsFilled = false; 
            }
            if (string.IsNullOrEmpty(ViewModel.User.LastName))
            {
                allObligatoryFieldsFilled = false;
            }
            if (string.IsNullOrEmpty(ViewModel.User.Email))
            {
                allObligatoryFieldsFilled = false;
            }
            if (string.IsNullOrEmpty(ViewModel.Password))
            {
                allObligatoryFieldsFilled = false;
            }
            if (string.IsNullOrEmpty(ViewModel.ConfirmedPassword))
            {
                allObligatoryFieldsFilled = false;
            }

            if (!allObligatoryFieldsFilled)
            {
                ViewModel.ErrorMessage = "Please fill in all obligatory fields to register";
            }

            return allObligatoryFieldsFilled;

        }

        private bool IsEmailValidated()
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(ViewModel.User.Email);
                return addr.Address == ViewModel.User.Email;
            }
            catch
            {
                ViewModel.ErrorMessage = "Please specify a correct email";
                return false;
            }
            
        }
        
        private bool IsPasswordsAccepted()
        {

            if(ViewModel.Password.Length < 8)
            {
                ViewModel.ErrorMessage = "Password must be longer than 8 characters";
                return false;
            }

            if(!ViewModel.Password.Equals(ViewModel.ConfirmedPassword))
            {
                ViewModel.ErrorMessage = "The confirmed passwords do not match";
                return false;
            }

            return true;
        }


        private void Register_Click(object sender, RoutedEventArgs e)
        {
            if (Register_clicked) { return; }
            else { Register_clicked = true; }

            ViewModel.ErrorMessage = "";

            if(IsAllObligatoryFieldsFilled() && IsEmailValidated() && IsPasswordsAccepted())
            {
                try
                {
                    Models.User.AddUser(ViewModel.User, ViewModel.Password);
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
