using DrawGuess.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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

        public ChangePasswordPage()
        {
            this.InitializeComponent();
            this.ViewModel = new SettingsViewModel();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Logo_Tapped(object sender, EventArgs e)
        {
            this.Frame.Navigate(typeof(StartPage), null, new SuppressNavigationTransitionInfo());
        }

        private void CheckOldPassword()
        {

        }

        private void CheckNewPassword()
        {
            if(!ViewModel.NewPassword.Equals(ViewModel.ConfirmNewPassword))
            {

            }
        }

        private void SavePassword_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SettingsPage), null, new SuppressNavigationTransitionInfo());
        }
    }
}
