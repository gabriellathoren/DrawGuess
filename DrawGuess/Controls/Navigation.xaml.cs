using DrawGuess.Pages;
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

namespace DrawGuess.Controls
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Navigation : UserControl
    {

        public string UserName { get; set; }
        public string ProfilePicture { get; set; }

        public event EventHandler SettingsClick;
        public event EventHandler LogoTapped;


        public Navigation()
        {
            this.InitializeComponent();
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
