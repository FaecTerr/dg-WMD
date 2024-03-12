using System;
using System.Collections.Generic;


namespace DuckGame.WMD
{
    public class NMDefineDrop : NMEvent
    {
        public bool boost;
        public Airdrop drop;
 
        public NMDefineDrop()
        {
        }

        public NMDefineDrop(Airdrop d, bool b)
        {
            boost = b;
            drop = d;
        }

        public override void Activate()
        {
            if (drop != null)
            {
                drop.boost = boost;
            }
        }
    }
}