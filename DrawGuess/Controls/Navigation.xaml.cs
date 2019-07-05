using DrawGuess.Pages;
using DrawGuess.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Credentials;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;


namespace DrawGuess.Controls
{
    public sealed partial class Navigation : UserControl
    {

        public NavigationViewModel ViewModel { get; set; }

        public Navigation()
        {
            this.InitializeComponent();
            ViewModel = new NavigationViewModel();
            
            if(!string.IsNullOrEmpty(ViewModel.ProfilePicture))
            {
                _ = SetProfilePictureAsync();
            }
        }

        private async System.Threading.Tasks.Task SetProfilePictureAsync()
        {
            // Get the app folder where the images are stored.
            var appInstalledFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            var assets = await appInstalledFolder.GetFolderAsync("Assets");
            var imageFile = await assets.GetFileAsync(ViewModel.User.ProfilePicture);
            
            BitmapImage image = new BitmapImage(new Uri(imageFile.ToString()));
            profilePicture.ProfilePicture = image;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ((Frame)Window.Current.Content).Navigate(typeof(SettingsPage), null, new SuppressNavigationTransitionInfo());
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            ((Frame)Window.Current.Content).Navigate(typeof(ChangePasswordPage), null, new SuppressNavigationTransitionInfo());
        }
        
        private void SignOut_Click(object sender, RoutedEventArgs e)
        {
            var vault = new PasswordVault();
            vault.Remove(vault.FindAllByUserName((App.Current as App).User.Email).First());
            ((Frame)Window.Current.Content).Navigate(typeof(LoginPage), null, new SuppressNavigationTransitionInfo());
        }

        private void Logo_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ((Frame)Window.Current.Content).Navigate(typeof(StartPage), null, new SuppressNavigationTransitionInfo());
        }
    }
}
