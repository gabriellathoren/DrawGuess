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
    public sealed partial class SettingsPage : Page
    {
        public SettingsViewModel ViewModel { get; set; }
        public bool Save_Clicked { get; set; }

        public SettingsPage()
        {
            this.InitializeComponent();
            this.ViewModel = new SettingsViewModel();
        }

        private void ResetErrorIndicators()
        {
            firstNameBox.BorderBrush = new SolidColorBrush(Color.FromArgb(66, 0, 0, 0));
            lastNameBox.BorderBrush = new SolidColorBrush(Color.FromArgb(66, 0, 0, 0));
            ViewModel.ErrorMessage = "";
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

            if (!allObligatoryFieldsFilled)
            {
                ViewModel.ErrorMessage = "Please fill in all obligatory fields to register";
            }

            return allObligatoryFieldsFilled;

        }

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            if (Save_Clicked) { return; }
            Save_Clicked = true;

            ResetErrorIndicators();

            if (IsAllObligatoryFieldsFilled())
            {
                try
                {
                    Models.User.UpdateUser(ViewModel.User.FirstName, ViewModel.User.LastName, ViewModel.User.Email, ViewModel.User.ProfilePicture);
                    this.Frame.Navigate(typeof(StartPage), "", new SuppressNavigationTransitionInfo());
                }
                catch (Exception)
                {
                    ViewModel.ErrorMessage = "Something went wrong, please try again later";
                }
            }

            Save_Clicked = false;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(StartPage), "", new SuppressNavigationTransitionInfo());
        }
    }
}