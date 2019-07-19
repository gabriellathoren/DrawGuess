using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawGuess.Models
{
    public class Player
    {
        public string NickName { get; set; }
        public int Points { get; set; }
        public bool IsCurrentUser { get; set; }
    }
}
