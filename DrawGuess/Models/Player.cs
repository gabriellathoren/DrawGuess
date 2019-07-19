using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawGuess.Models
{
    public class Player
    {
        public string UserId { get; set; }
        public string NickName { get; set; }
        public int Points { get; set; }
        public bool IsCurrentUser { get; set; }
        public int Placement { get; set; }
        public bool Painter { get; set; }
        public bool RightAnswer { get; set; }

        public Player()
        {
            Placement = 0;
        }
    }
}
