using DrawGuess.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Credentials;
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

        public SettingsPage()
        {
            this.InitializeComponent();
            this.ViewModel = new SettingsViewModel();
        }
        
        private void Logo_Tapped(object sender, EventArgs e)
        {
            this.Frame.Navigate(typeof(StartPage), null, new SuppressNavigationTransitionInfo());
        }

        private void SignOut_Click(object sender, RoutedEventArgs e)
        {
            var vault = new PasswordVault();            
            vault.Remove(vault.FindAllByUserName((App.Current as App).User.Email).First());
            this.Frame.Navigate(typeof(LoginPage), null, new SuppressNavigationTransitionInfo());
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ChangePasswordPage), null, new SuppressNavigationTransitionInfo());
        }
    }
}
