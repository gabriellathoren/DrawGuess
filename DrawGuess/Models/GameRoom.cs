using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawGuess.Models
{
    public class GameRoom
    {
        public String RoomName { get; set; }
        public int NumberOfPlayers
        {
            get { return _numberOfPlayers; }
            set
            {
                _numberOfPlayers = value;
                if (this.NumberOfPlayers >= 10) { Full = true; }
                else { Full = false; }
            }
        }
        private int _numberOfPlayers;
   
        public Boolean Full { get; set; }
    }
}
