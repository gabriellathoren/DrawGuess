using DrawGuess.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawGuess.ViewModels
{
    public class GameViewModel
    {

        public int Round { get; set; }
        public GameRoom Room { get; set; }
        public ObservableCollection<PlayersViewModel> Players { get; set; }
        public ObservableCollection<string> Guess { get; set; }
        public ObservableCollection<string> RandomLetters { get; set; }
        public string SecretWord { get; set; }
        public bool PainterView { get; set; }

        public GameViewModel()
        {
            Players = new ObservableCollection<PlayersViewModel>();
            Guess = new ObservableCollection<string>();
            RandomLetters = new ObservableCollection<string>();
        }

    }
}
