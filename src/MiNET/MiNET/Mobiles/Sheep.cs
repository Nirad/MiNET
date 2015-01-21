using MiNET.Worlds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiNET.Mobiles
{
    public class Sheep : Mobile
    {
        public override int Type { get { return 13; } }

        public Sheep(Level level) : base(level)
        {
            Flag = 0x00;
        }

   
    }

}
