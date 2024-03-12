using System;
using System.Collections.Generic;

namespace DuckGame.WMD
{
    public class NMNinjaHookCount : NMEvent
    {
        public int count;
        public NinjaGrapple hook;

        public NMNinjaHookCount()
        {

        }

        public NMNinjaHookCount(NinjaGrapple n, int c)
        {
            count = c;
            hook = n;
        }

        public override void Activate()
        {
            if (hook != null)
            {
                hook.uses = count;
            }
        }
    }
}