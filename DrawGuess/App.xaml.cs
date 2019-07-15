using DrawGuess.Models;
using DrawGuess.Security;
using Photon.Realtime;
using PlayFab;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Credentials;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace DrawGuess
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        // Connection string for Azure database
        public string username;
        public string Username { get => username; set => username = value; }

        private string password;
        public string Password { get => password; set => password = value; }

        private string connectionString;
        public string ConnectionString { get => connectionString; set => connectionString = value; }
        
        private string resourceName = "DrawGuess";
        public string ResourceName { get => resourceName; set => resourceName = value; }

        private User user = new User();
        public User User { get => user; set => user = value; }

        private string playFabTitleId = "650C2";
        public string PlayFabTitleId { get => playFabTitleId; set => playFabTitleId = value; }

        private string photonAppId = "2d9f6ff3-d9c5-4374-ac7e-681e0baa2f0e";
        public string PhotonAppId { get => photonAppId; set => photonAppId = value; }

        private LoadBalancingClient loadBalancingClient = new LoadBalancingClient();
        public LoadBalancingClient LoadBalancingClient { get => loadBalancingClient; set => loadBalancingClient = value; }

        private bool shouldExit = false;
        public bool ShouldExit { get => shouldExit; set => shouldExit = value; }

        private bool connected = false;
        public bool Connected { get => connected; set => connected = value; }


        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;

            //Set connectionsstring
            var resources = new Windows.ApplicationModel.Resources.ResourceLoader("Resources");
            Username = resources.GetString("user");
            Password = resources.GetString("password");
            connectionString = @"Server=tcp:drawguess.database.windows.net,1433;Initial Catalog=DrawGuess;Persist Security Info=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;User ID=" + username + ";Password=" + password + ";";

            PlayFabSettings.staticSettings.TitleId = PlayFabTitleId;

            //Set Load Balancing Client
            LoadBalancingClient.AppId = PhotonAppId;
            LoadBalancingClient.AppVersion = "1.0";
        }


        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ForegroundColor = Windows.UI.Colors.White;
            titleBar.BackgroundColor = Windows.UI.Colors.Black;
            titleBar.ButtonForegroundColor = Windows.UI.Colors.White;
            titleBar.ButtonBackgroundColor = Windows.UI.Colors.SeaGreen;
            titleBar.ButtonHoverForegroundColor = Windows.UI.Colors.White;
            titleBar.ButtonHoverBackgroundColor = Windows.UI.Colors.DarkSeaGreen;
            titleBar.ButtonPressedForegroundColor = Windows.UI.Colors.Gray;
            titleBar.ButtonPressedBackgroundColor = Windows.UI.Colors.LightGreen;

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    var credentialControl = new CredentialControl();
                    var loginCredential = credentialControl.GetCredentialFromLocker();
                    try
                    {                 
                        //Control if user is already logged on
                        if (loginCredential != null)
                        {
                            loginCredential.RetrievePassword();
                            await credentialControl.SystemLogIn(loginCredential.UserName, loginCredential.Password);
                            rootFrame.Navigate(typeof(Pages.StartPage), e.Arguments);
                        }
                        else
                        {
                            rootFrame.Navigate(typeof(Pages.LoginPage), e.Arguments);
                        }
                    }
                    catch(Exception)
                    {
                        PasswordVault vault = new PasswordVault();

                        foreach(PasswordCredential p in vault.RetrieveAll()) {
                            vault.Remove(p);
                        }

                        rootFrame.Navigate(typeof(Pages.LoginPage), e.Arguments);
                    }
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
