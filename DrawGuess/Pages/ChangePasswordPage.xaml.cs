using DrawGuess.Exceptions;
using DrawGuess.Security;
using DrawGuess.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

    public sealed partial class ChangePasswordPage : Page
    {
        public SettingsViewModel ViewModel { get; set; }
        public bool SavePassword_Clicked { get; set; }

        public ChangePasswordPage()
        {
            this.InitializeComponent();
            this.ViewModel = new SettingsViewModel();
        }

        private bool CheckOldPassword()
        {
            try
            {
                Models.User.GetUser(ViewModel.User.Email, ViewModel.OldPassword);
                return true;
            }
            catch(UserNotFoundException)
            {
                oldPasswordBox.BorderBrush = new SolidColorBrush(Colors.Red);
                ViewModel.ErrorMessage = "The old password is incorrect";
                return false; 
            }
            catch (Exception) {
                oldPasswordBox.BorderBrush = new SolidColorBrush(Colors.Red);
                ViewModel.ErrorMessage = "Something went wrong, please try again later";
                return false;
            }
        }

        private bool CheckNewPassword()
        {
            bool PasswordAccepted = true;

            if(!ViewModel.NewPassword.Equals(ViewModel.ConfirmNewPassword))
            {
                passwordConfirmBox.BorderBrush = new SolidColorBrush(Colors.Red);
                newPasswordBox.BorderBrush = new SolidColorBrush(Colors.Red);
                ViewModel.ErrorMessage = "The specified passwords do not match";
                PasswordAccepted = false;
            }
            else if(ViewModel.NewPassword.Length < 8)
            {
                passwordConfirmBox.BorderBrush = new SolidColorBrush(Colors.Red);
                newPasswordBox.BorderBrush = new SolidColorBrush(Colors.Red);
                ViewModel.ErrorMessage = "Password must be longer than 8 characters";
                PasswordAccepted = false;
            }

            return PasswordAccepted;
        }

        private void ResetErrorIndicators()
        {
            passwordConfirmBox.BorderBrush = new SolidColorBrush(Color.FromArgb(66, 0, 0, 0));
            newPasswordBox.BorderBrush = new SolidColorBrush(Color.FromArgb(66, 0, 0, 0));
            oldPasswordBox.BorderBrush = new SolidColorBrush(Color.FromArgb(66, 0, 0, 0));
            ViewModel.ErrorMessage = "";
        }

        private void SavePassword_Click(object sender, RoutedEventArgs e)
        {
            if(SavePassword_Clicked) { return; }
            SavePassword_Clicked = true;

            ResetErrorIndicators();

            try
            {
                if (CheckOldPassword())
                {
                    if (CheckNewPassword())
                    {
                        Models.User.UpdatePassword(ViewModel.User.Email, ViewModel.NewPassword);
                        UpdatePasswordVault();
                        this.Frame.Navigate(typeof(StartPage), true, new SuppressNavigationTransitionInfo());
                    }
                }
            }
            catch (Exception)
            {
                ViewModel.ErrorMessage = "Something went wrong, please try again later";
            }               

            SavePassword_Clicked = false;
        }

        private void UpdatePasswordVault()
        {
            var vault = new PasswordVault();

            //Remove existing credidential i password vault

            vault.Remove(vault.Retrieve((App.Current as App).ResourceName, ViewModel.User.Email));
            //Add new credidential i password vault
            vault.Add(new PasswordCredential((App.Current as App).ResourceName, ViewModel.User.Email, ViewModel.NewPassword));
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(StartPage), true, new SuppressNavigationTransitionInfo());
        }

        private void EnterPressed(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                SavePassword_Click(sender, e);
            }
        }
    }
}
