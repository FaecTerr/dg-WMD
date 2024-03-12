using System;
using System.Collections.Generic;

namespace DuckGame.WMD
{
    public class NMTurretReady : NMEvent
    {
        public Duck own;
        public SentryGun turret;

        public NMTurretReady()
        {

        }

        public NMTurretReady(SentryGun s, Duck d)
        {
            own = d;
            turret = s;
        }

        public override void Activate()
        {
            if (turret != null && own != null)
            {
                turret.user = own;
                own.doThrow = true;
                turret.canPickUp = false;
                turret.Placed = true;
            }
        }
    }
}