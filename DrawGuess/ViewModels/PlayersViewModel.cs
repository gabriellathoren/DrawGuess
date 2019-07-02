using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawGuess.ViewModels
{
    public class PlayersViewModel
    {
        public int Id { get; set; }
        public int Placement { get; set; }
        public string Name { get; set; }
        public int Points { get; set; }
        public string ProfilePicture { get; set; }
        public bool Painter { get; set; }
        public bool RightAnswer { get; set; }
        public bool IsCurrentUser { get; set; }
        

        public PlayersViewModel()
        {
            Placement = 0;
            IsCurrentUser = false;
        }

    }
}
