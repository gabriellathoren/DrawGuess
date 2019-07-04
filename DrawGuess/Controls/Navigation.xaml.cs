using DrawGuess.Pages;
using DrawGuess.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace DrawGuess.Controls
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Navigation : UserControl
    {

        public NavigationViewModel ViewModel { get; set; }

        public event EventHandler SettingsClick;
        public event EventHandler LogoTapped;


        public Navigation()
        {
            this.InitializeComponent();
            ViewModel = new NavigationViewModel();

            ViewModel.User = (App.Current as App).User;
            ViewModel.UserName = ViewModel.User.FirstName + " " + ViewModel.User.LastName;
            ViewModel.Initials = ViewModel.User.FirstName.Substring(0,1).ToUpper() + ViewModel.User.LastName.Substring(0,1).ToUpper();
            ViewModel.ProfilePicture = ViewModel.User.ProfilePicture;

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
            this.SettingsClick?.Invoke(this, new EventArgs());
        }

        private void Logo_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.LogoTapped?.Invoke(this, new EventArgs());
        }
    }
}
