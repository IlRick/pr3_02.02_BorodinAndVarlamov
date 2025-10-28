using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class ViewModelGames
    {
        public Shakes ShakesPlayers= new Shakes();
        public Shakes.Point Points= new Shakes.Point();
        public int Top = 0;
        public int IdShake {  get; set; }
    }
}
