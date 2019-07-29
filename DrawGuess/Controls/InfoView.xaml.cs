using DrawGuess.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace DrawGuess.Controls
{
    public sealed partial class InfoView : UserControl, INotifyPropertyChanged
    {

        private int row1FontSize;
        public int Row1FontSize
        {
            get { return this.row1FontSize; }
            set
            {
                this.row1FontSize = value;
                this.OnPropertyChanged();
            }
        }

        private int row2FontSize;
        public int Row2FontSize
        {
            get { return this.row2FontSize; }
            set
            {
                this.row2FontSize = value;
                this.OnPropertyChanged();
            }
        }

        private string row1;
        public string Row1
        {
            get { return this.row1; }
            set
            {
                this.row1 = value;
                this.OnPropertyChanged();
            }
        }

        private string row2;
        public string Row2
        {
            get { return this.row2; }
            set
            {
                this.row2 = value;
                this.OnPropertyChanged();
            }
        }

        private string specificInfoToRow1;
        public string SpecificInfoToRow1
        {
            get { return this.specificInfoToRow1; }
            set
            {
                this.specificInfoToRow1 = value;
                this.OnPropertyChanged();
            }
        }

        private string specificInfoToRow2;
        public string SpecificInfoToRow2
        {
            get { return this.specificInfoToRow2; }
            set
            {
                this.specificInfoToRow2 = value;
                this.OnPropertyChanged();
            }
        }

        private bool twoRows;
        public bool TwoRows
        {
            get { return this.twoRows; }
            set
            {
                this.twoRows = value;
                this.OnPropertyChanged();
            }
        }

        private bool showSecretWord;
        public bool ShowSecretWord
        {
            get { return this.showSecretWord; }
            set
            {
                this.showSecretWord = value;
                this.OnPropertyChanged();
            }
        }

        private GameMode mode;
        public GameMode Mode {
            get { return this.mode; }
            set
            {
                this.mode = value;
                SetMode();
                this.OnPropertyChanged();
            }
        }

        public InfoView()
        {
            this.InitializeComponent();

            Row1FontSize = 62;
            Row2FontSize = 40;
            Row1 = "";
            Row2 = "";
            SpecificInfoToRow1 = "";
            SpecificInfoToRow2 = "";
            TwoRows = false;
            ShowSecretWord = false;
        }

        public void SetMode()
        {

            switch (Mode)
            {
                case GameMode.WaitingForPlayers:
                    Row1FontSize = 62;
                    Row1 = "Waiting for other players\r\nto join...";
                    break;
                case GameMode.StartingGame:
                    Row1FontSize = 62;
                    Row1 = "Starting new game...";
                    break;
                case GameMode.RevealingRoles:
                    Row1FontSize = 62;
                    Row1 = "Painter: " + SpecificInfoToRow1;
                    if (ShowSecretWord)
                    {
                        TwoRows = true;
                        Row2 = "Secret word: " + SpecificInfoToRow2;
                        Row2FontSize = 40;
                    }
                    break;
                case GameMode.StartingRound:
                    Row1FontSize = 62;
                    Row1 = "ROUND " + SpecificInfoToRow1;
                    break;                
                case GameMode.EndingGame:
                    Row1FontSize = 62;
                    Row1 = "Winner: " + SpecificInfoToRow1;
                    break;
                default:
                    break;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
