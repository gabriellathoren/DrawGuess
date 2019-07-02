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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace DrawGuess.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ChangePasswordPage : Page
    {
        public SettingsViewModel ViewModel { get; set; }

        public ChangePasswordPage()
        {
            this.InitializeComponent();
            this.ViewModel = new SettingsViewModel();

            ViewModel.User = new Models.User { FirstName = "Gabriella", LastName = "Thorén", Email = "gabriella_thoren@hotmail.com", ProfilePicture = "https://scontent-arn2-1.xx.fbcdn.net/v/t1.0-9/55807325_2778073192210585_6307069374352064512_n.jpg?_nc_cat=108&_nc_oc=AQk9gg8bPNJLK_rSrDfDZOCNwg-GhY1tHWPwHkZRSSwBpLIVfb71EAsO8zbSJzxQzJg&_nc_ht=scontent-arn2-1.xx&oh=106558072dcc414195889611c828acd5&oe=5DBBAA6F" };
            ViewModel.UserName = ViewModel.User.FirstName + " " + ViewModel.User.LastName;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Logo_Tapped(object sender, EventArgs e)
        {
            this.Frame.Navigate(typeof(StartPage), null, new SuppressNavigationTransitionInfo());
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
