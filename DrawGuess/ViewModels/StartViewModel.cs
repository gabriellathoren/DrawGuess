using DrawGuess.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawGuess.ViewModels
{
    public class StartViewModel
    {
        public String UserName { get; set; }
        public String ProfilePicture { get; set; }
        public String Points { get; set; }
        public ObservableCollection<GameRoom> Items { get; set; }

        public StartViewModel()
        {
            Items = new ObservableCollection<GameRoom>();
        }
    }
}
