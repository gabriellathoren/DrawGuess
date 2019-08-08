using DrawGuess.Models;
using DrawGuess.ViewModels;
using ExitGames.Client.Photon;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
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
    public sealed partial class GamePage : Page
    {
        public GameViewModel ViewModel { get; set; }
        private LoadBalancingClient LoadBalancingClient = (App.Current as App).LoadBalancingClient;

        public GamePage()
        {
            this.InitializeComponent();
            ViewModel = new GameViewModel();

            // Initialize the InkCanvas
            InkCanvas.InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Mouse | CoreInputDeviceTypes.Pen;
            InkCanvas.InkPresenter.StrokeContainer = new InkStrokeContainer();
            InkCanvas.InkPresenter.StrokesCollected += Strokes_StrokesChanged;
            InkCanvas.InkPresenter.StrokesErased += Strokes_StrokesErased;

            //Listen to callbacks from Photon
            LoadBalancingClient.InRoomCallbackTargets.PlayerEnteredRoom += PlayerEnteredRoom;
            LoadBalancingClient.InRoomCallbackTargets.PlayerLeftRoom += PlayerLeftRoom;
            LoadBalancingClient.InRoomCallbackTargets.RoomPropertiesUpdate += RoomPropertiesUpdate;
            LoadBalancingClient.InRoomCallbackTargets.PlayerPropertiesUpdate += PlayerPropertiesUpdate;
        }

        //Listener for removing strokes to InkCanvas
        private async void Strokes_StrokesErased(InkPresenter sender, InkStrokesErasedEventArgs args)
        {
            await UpdateStrokes();
        }

        //Listener for adding new strokes to InkCanvas
        private async void Strokes_StrokesChanged(InkPresenter sender, InkStrokesCollectedEventArgs args)
        {
            await UpdateStrokes();
        }

        //Listener for player changes
        private async void PlayerPropertiesUpdate(object sender, EventArgs e)
        {
            try
            {
                Hashtable data = (Hashtable)sender;

                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    UpdatePlayers();

                    //Only update order of list if points are changed
                    if (data.ContainsKey("points"))
                    {
                        SetPlacement();
                    }
                 });
            }
            catch (Exception)
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        ViewModel.ErrorMessage = "Could not update room";
                    });
            }
        }

        //Listener for room changes
        private async void RoomPropertiesUpdate(object sender, EventArgs e)
        {
            try
            {
                Hashtable data = (Hashtable)sender;

                if (data.ContainsKey("strokes"))
                {
                    if (!ViewModel.CurrentPlayer.Painter)
                    {
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            GetStrokes();
                        });
                    }
                }
                else
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        GetGame();
                    });
                }
            }
            catch (Exception)
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        ViewModel.ErrorMessage = "Could not update room";
                    });
            }
        }

        //Listener for player leaving room
        private async void PlayerLeftRoom(object sender, EventArgs e)
        {
            try
            {
                Photon.Realtime.Player newPlayer = (Photon.Realtime.Player)sender;
                Models.Player player = new Models.Player()
                {
                    UserId = newPlayer.UserId
                };

                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        Task removeTask = RemovePlayer(player);
                    });
            }
            catch (Exception)
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        ViewModel.ErrorMessage = "Could not update player list";
                    });
            }
        }

        //Listener for player entering room
        private async void PlayerEnteredRoom(object sender, EventArgs e)
        {
            try
            {
                Photon.Realtime.Player newPlayer = (Photon.Realtime.Player)sender;
                Models.Player player = new Models.Player()
                {
                    UserId = newPlayer.UserId,
                    NickName = newPlayer.NickName,
                    IsCurrentUser = false
                };

                if(newPlayer.CustomProperties.ContainsKey("Points"))
                {
                    player.Points = (int)newPlayer.CustomProperties["points"];
                }
                else
                {
                    player.Points = 0;
                }

                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        AddPlayer(player);
                    });
            }
            catch (Exception ex)
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        ViewModel.ErrorMessage = "Could not update player list";
                    });
            }
        }


        public async void GetStrokes()
        {
            try
            {
                byte[] strokesByte = ViewModel.Game.GetStrokes();

                //If strokes is empty, clear the InkCanvas
                if (strokesByte == null || strokesByte.Length < 1)
                {
                    InkCanvas.InkPresenter.StrokeContainer.Clear();
                    return;
                }

                //From bytes to strokes
                using (InMemoryRandomAccessStream stream2 = new InMemoryRandomAccessStream())
                {
                    await stream2.WriteAsync(strokesByte.AsBuffer());
                    //await InkCanvas.InkPresenter.StrokeContainer.LoadAsync(stream);

                    using (var inputStream = stream2.GetInputStreamAt(0))
                    {
                        await InkCanvas.InkPresenter.StrokeContainer.LoadAsync(inputStream);
                    }
                    stream2.Dispose();
                }
            }
            catch (Exception e)
            {
                ViewModel.ErrorMessage = "Could not set strokes";
            }
        }

        public async Task UpdateStrokes()
        {
            try
            {
                //Update strokes in Photon if player is painter
                if (ViewModel.CurrentPlayer.Painter)
                {
                    //create stream
                    InMemoryRandomAccessStream testStream = new InMemoryRandomAccessStream();
                    using (IOutputStream outputStream = testStream.GetOutputStreamAt(0))
                    {
                        //save inkstrokes to the stream 
                        await InkCanvas.InkPresenter.StrokeContainer.SaveAsync(outputStream); //All strokes
                        await outputStream.FlushAsync();
                    }
                    //use datareader to read the stream
                    var dr = new DataReader(testStream.GetInputStreamAt(0));
                    //create byte array
                    var bytes = new byte[testStream.Size];
                    //load stream
                    await dr.LoadAsync((uint)testStream.Size);
                    //save to byte array
                    dr.ReadBytes(bytes);

                    //Send strokes to Photon
                    ViewModel.Game.AddStrokes(bytes);
                }
            }
            catch (Exception e)
            {
                ViewModel.ErrorMessage = "Could not set strokes";
            }
        }

        public void AddPlayer(Models.Player player)
        {
            try
            {
                ViewModel.Players.Add(player);
                SetPlacement();
            }
            catch (Exception)
            {
                ViewModel.ErrorMessage = "Could not add player properly";
            }
        }

        public async Task RemovePlayer(Models.Player player)
        {
            try
            {
                Models.Player p = ViewModel.Players.Where(x => x.UserId.Equals(player.UserId)).First();

                //If leaving player was painter
                if (p.Painter == true)
                {
                    ViewModel.Players.Remove(p);

                    //Change game mode
                    await ViewModel.Game.SetMode(GameMode.PainterLeft, 0);

                    if (LoadBalancingClient.CurrentRoom.Players.Count < 2)
                    {
                        Task waitingTask = ViewModel.Game.SetMode(GameMode.WaitingForPlayers, 5);
                    }
                    else
                    {
                        //Set new random painter
                        Random rnd = new Random();
                        int newPainterIndex = rnd.Next(ViewModel.Players.Count);
                        ViewModel.Game.SetSpecificPainter(ViewModel.Players[newPainterIndex]);
                        Task startNewRoundTask = ViewModel.Game.SetMode(GameMode.StartingRoundAfterPainterLeft, 5);
                    }
                }
                else
                {
                    ViewModel.Players.Remove(p);
                    SetGameMode();
                }

                SetPlacement();
                GetGame();
            }
            catch (Exception e)
            {
                ViewModel.ErrorMessage = "Could not remove player properly";
            }
        }

        public void SetGameMode()
        {
            try
            {
                //Start game if there are 2 or more players 
                if (ViewModel.Players.Count > 1 && ViewModel.Game.Mode.Equals(GameMode.WaitingForPlayers))
                {
                    StartGame();
                }
                //Stop game if there are less players than two
                else if (ViewModel.Players.Count < 2 && !ViewModel.Game.Mode.Equals(GameMode.WaitingForPlayers))
                {
                    ViewModel.Game.StopGame();
                }
            }
            catch (Exception e)
            {
                ViewModel.ErrorMessage = "Could not set game mode";
            }
        }

        public void StartGame()
        {
            try
            {
                ViewModel.Game.StartGame();
            }
            catch (Exception)
            {
                ViewModel.ErrorMessage = "Could not start game";
            }
        }

        public void SetPlayerPoints(int points)
        {
            try
            {
                ViewModel.Game.SetPlayerPoints(points);
                SetPlacement(); 
            }
            catch (Exception)
            {
                ViewModel.ErrorMessage = "Could not set player points";
            }
        }

        public void SetCorrectAnswer(bool correctAnswer)
        {
            try
            {
                ViewModel.Game.SetCorrectAnswer(correctAnswer);
            }
            catch (Exception)
            {
                ViewModel.ErrorMessage = "Could not set indicator if guess was correct or not";
            }
        }

        public void GetPlayers()
        {
            try
            {
                ViewModel.Players = ViewModel.Game.GetPlayers();
                SetPlacement();
            }
            catch (Exception e)
            {
                ViewModel.ErrorMessage = e.Message;
            }
        }

        public async void UpdatePlayers()
        {
            try
            {
                //Update existing player's information
                foreach (var p in ViewModel.Game.GetPlayers())
                {
                    var player = ViewModel.Players.Where(x => x.UserId == p.UserId).First(); 

                    if (ViewModel.CurrentPlayer.Points != p.Points)
                    {
                        player.Points = p.Points;
                    }
                    if (ViewModel.CurrentPlayer.RightAnswer != p.RightAnswer)
                    {
                        player.RightAnswer = p.RightAnswer;
                    }
                    if (ViewModel.CurrentPlayer.Painter != p.Painter)
                    {
                        player.Painter = p.Painter;
                    }
                }

                //If all users have guessed correct, continue to next step
                if(ViewModel.CurrentPlayer.Painter)
                {
                    if (!ViewModel.Players.Any(x => !x.RightAnswer && !x.Painter))
                    {
                        await ViewModel.Game.SetMode(GameMode.EndingRound, 5);
                    }
                }
                
            }
            catch (Exception e)
            {
                ViewModel.ErrorMessage = e.Message;
            }
        }

        public void SetPlacement()
        {
            ViewModel.Players = new ObservableCollection<Models.Player>(ViewModel.Players.OrderByDescending(x => x.Points).ToList());

            if (!ViewModel.Players.Any(p => p.Points != 0))
            {
                foreach (Models.Player p in ViewModel.Players)
                {
                    p.Placement = 0;
                }
                return; 
            }

            int placement = 1;
            foreach (Models.Player p in ViewModel.Players)
            {
                if (ViewModel.Players.IndexOf(p) == 0)
                {
                    p.Placement = placement;
                }
                else if (p.Points == ViewModel.Players[ViewModel.Players.IndexOf(p) - 1].Points)
                {
                    p.Placement = placement;
                }
                else
                {
                    placement++;
                    p.Placement = placement;
                }
            }
        }

        public void UpdateInfoView()
        {
            if(ViewModel.CurrentMode == ViewModel.Game.Mode) { return; }

            InfoView.TwoRows = false;
            InfoView.Row1 = "";
            InfoView.Row2 = "";

            switch (ViewModel.Game.Mode)
            {
                case GameMode.WaitingForPlayers:
                    ViewModel.CurrentMode = GameMode.WaitingForPlayers; 
                    ViewModel.ShowInfoView = true;
                    ViewModel.ShowGame = false;
                    InfoView.Row1 = "Waiting for other players\r\nto join...";
                    break;
                case GameMode.StartingGame:                    
                    SetPlayerPoints(0); //Clear player points every new game
                    ViewModel.CurrentMode = GameMode.StartingGame;
                    ViewModel.ShowInfoView = true;
                    ViewModel.ShowGame = false;
                    InfoView.Row1 = "Starting new game...";
                    break;
                case GameMode.StartingRound:
                    ViewModel.CurrentMode = GameMode.StartingRound;
                    ViewModel.ShowInfoView = true;
                    ViewModel.ShowGame = false;
                    InfoView.Row1 = "ROUND " + ViewModel.Game.Round.ToString();
                    InkCanvas.InkPresenter.StrokeContainer = new InkStrokeContainer();
                    break;
                case GameMode.RevealingRoles:
                    ViewModel.CurrentMode = GameMode.RevealingRoles;
                    ViewModel.ShowInfoView = true;
                    ViewModel.ShowGame = false;
                    InfoView.Row1 = "Painter: " + ViewModel.Players.Where(x => x.Painter.Equals(true)).First().NickName;
                    InfoView.ShowSecretWord = false;
                    if (ViewModel.CurrentPlayer.Painter)
                    {
                        InfoView.ShowSecretWord = true;
                        InfoView.Row2 = "Secret word: " + ViewModel.Game.SecretWord;
                        InfoView.TwoRows = true;
                    }
                    break;
                case GameMode.Playing:
                    ViewModel.CurrentMode = GameMode.Playing;
                    if (ViewModel.CurrentPlayer.Painter)
                    {
                        ViewModel.PainterView = true;
                        var secret = new ObservableCollection<Letter>();
                        foreach (var letter in ViewModel.Game.SecretWord)
                        {
                            secret.Add(new Letter() { Character = letter.ToString() });
                        }
                        ViewModel.Guess = secret;
                    }
                    else
                    {
                        ViewModel.PainterView = false;
                    }
                    ViewModel.ShowInfoView = false;
                    ViewModel.ShowGame = true;
                    ViewModel.Game.Timer = 60;                    
                    Task task = Task.Run((Action)CountDown); //Start CoundDown task
                    break;
                case GameMode.EndingRound:
                    ViewModel.CurrentMode = GameMode.EndingRound;
                    ViewModel.ShowInfoView = true;
                    ViewModel.ShowGame = true;
                    InfoView.Row1 = "The secret word was:\r\n" + ViewModel.Game.SecretWord;
                    break;
                case GameMode.EndingGame:
                    ViewModel.CurrentMode = GameMode.EndingGame;
                    ViewModel.ShowInfoView = true;
                    ViewModel.ShowGame = true;
                    InfoView.Row1 = "Winner:\r\n" + GetWinners();
                    break;
                case GameMode.PainterLeft:
                    ViewModel.CurrentMode = GameMode.PainterLeft;
                    ViewModel.ShowInfoView = true;
                    InfoView.Row1 = "Painter left the game";
                    break;
                default:
                    break;
            }
        }

        public string GetWinners()
        {
            string winners = "";
            var maxPoints = ViewModel.Players.Max(x => x.Points);
            var winnerList = ViewModel.Players.Where(x => x.Points == maxPoints).ToList();

            foreach (var winner in winnerList)
            {
                winners += winner.NickName + "\r\n";
            }

            return winners;
        }

        public void GetRandomLetters()
        {
            try
            {
                var letters = new ObservableCollection<Letter>();

                //Set hinting boxes based on number of letters in secret word
                for (int i = 0; i < ViewModel.Game.RandomLetters.Length; i++)
                {
                    letters.Add(new Letter()
                    {
                        Character = ViewModel.Game.RandomLetters[i].ToString()
                    });
                }

                ViewModel.RandomLetters = letters;

                //Set correct answer to false when new round is set
                SetCorrectAnswer(false);
            }
            catch (Exception e)
            {
                ViewModel.ErrorMessage = e.Message;
            }
        }

        public void SetHint()
        {
            var hint = new ObservableCollection<Letter>();

            //Set hinting boxes based on number of letters in secret word
            for (int i = 0; i < ViewModel.Game.SecretWord.Length; i++)
            {
                if (ViewModel.Game.SecretWord[i].ToString().Equals(" "))
                {
                    hint.Add(new Letter() { Character = " " });
                }
                else
                {
                    hint.Add(new Letter() { Character = "" });
                }
            }

            ViewModel.Guess = hint;
        }

        public async void CountDown()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SetTimer();
            });
        }

        public async void SetTimer()
        {
            //Count down from 60 seconds to 0, so players nows how long the round has left 
            while (ViewModel.Game.Timer > 0)
            {
                ViewModel.Game.Timer -= 1;
                await Task.Delay(1000);
            }
        }

        public void GetGame()
        {
            try
            {
                //Update game, if the secret word was updated, the viewmodel must be updated as well
                if (ViewModel.Game.UpdateGame())
                {
                    if (!string.IsNullOrEmpty(ViewModel.Game.SecretWord))
                    {
                        SetHint();
                        GetRandomLetters();
                    }
                }

                UpdateInfoView();
            }
            catch (Exception e)
            {
                ViewModel.ErrorMessage = e.Message;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                GetPlayers();
                GetGame();
                SetGameMode();
                SetPlayerPoints(0);
                SetCorrectAnswer(false);
                GetStrokes();
            }
            catch (Exception ex)
            {
                //Show error message and navgate back to start page
                ViewModel.ErrorMessage = ex.Message;
                Quit();
            }
        }

        private void Quit()
        {
            ViewModel.Game.LeaveGame();

            while (!ViewModel.Game.LeftRoom)
            {
                Task.Delay(TimeSpan.FromMilliseconds(100));
            }

            this.Frame.Navigate(typeof(StartPage), "", new SuppressNavigationTransitionInfo());
        }

        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            Quit();
        }

        public bool CheckGuess()
        {
            string guess = "";

            foreach (var letter in ViewModel.Guess)
            {
                guess += letter.Character;
            }

            if (guess.Equals(ViewModel.Game.SecretWord))
            {
                return true;
            }

            return false;
        }

        private void Letter_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                //Get selected letter
                var letter = new Letter()
                {
                    Character = ((Letter)HintGrid.SelectedItem).Character,
                    Visibility = ((Letter)HintGrid.SelectedItem).Visibility
                };

                //Do not do anything if it is empty
                if (letter.Character == "" || letter.Visibility == false) { return; }

                //Get index of guess to replace with guessed letter
                var letterPlace = ViewModel.Guess.IndexOf(ViewModel.Guess.Where(x => x.Character == "").First());
                letter.Visibility = true;
                ViewModel.Guess[letterPlace] = letter;

                //Hide guess letter from hinting letters
                ViewModel.RandomLetters[HintGrid.SelectedIndex].Visibility = false;

                //If the letter is placed in the last space of letter check if correct
                if (ViewModel.Guess.Count <= (letterPlace + 1))
                {
                    //If guess is correct
                    if (CheckGuess())
                    {
                        //Setpoints depending on how fast the answer was made
                        var points = ViewModel.Game.Timer;                         
                        SetPlayerPoints(ViewModel.CurrentPlayer.Points + points);
                        SetCorrectAnswer(true);
                        //TODO: Show correct guess view
                        return;
                    }

                    //If guess is incorrect, remove guess from hint
                    SetHint();

                    //Show all letters in random letters again
                    foreach (var l in ViewModel.RandomLetters)
                    {
                        l.Visibility = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ViewModel.ErrorMessage = "Could not add letter to guess";
            }
        }

        private void GuessLetter_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                //Remove possibility to remove letters if user is painter or user has made the correct guess already
                if (ViewModel.PainterView || ViewModel.CurrentPlayer.RightAnswer == true)
                {
                    return;
                }

                //Get selected letter
                var letter = (Letter)GuessGrid.SelectedItem;

                //Do not do anything if it is empty
                if (letter.Character == "") { return; }

                //Remove letter from 
                ViewModel.Guess[GuessGrid.SelectedIndex] = new Letter() { Character = "" };

                //Show letter in random letters again
                var hintLetter = ViewModel.RandomLetters.Where(x => x.Character == letter.Character && x.Visibility == !false).FirstOrDefault();
                hintLetter.Visibility = true;
            }
            catch (Exception ex)
            {
                ViewModel.ErrorMessage = "Could not remove letter from guess";
            }
        }
    }
}
