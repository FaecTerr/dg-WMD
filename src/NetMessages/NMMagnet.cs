using System;
using System.Collections.Generic;

namespace DuckGame.WMD
{
    public class NMMagnet : NMEvent
    {
        public bool repulse;
        public Magnet magnet;

        public NMMagnet()
        {

        }

        public NMMagnet(Magnet m, bool r)
        {
            repulse = r;
            magnet = m;
        }

        public override void Activate()
        {
            if (magnet != null)
            {
                magnet.repulse = repulse;
                magnet.placd = true;
            }
        }
    }
}