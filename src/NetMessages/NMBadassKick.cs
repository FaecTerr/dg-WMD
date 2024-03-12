using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckGame.WMD
{
    public class NMBadassKick : NMEvent
    {
        public Bat bat;

        public NMBadassKick()
        {

        }

        public NMBadassKick(Bat b)
        {
            bat = b;
        }

        public override void Activate()
        {
            if (bat != null)
            {
                bat.Punch();
            }
        }
    }
}
