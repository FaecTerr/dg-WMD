using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckGame.WMD
{
    [EditorGroup("Faecterr's|Weapons|Mountable")]
    public class MountableMortar : MountableGun
    {
        private float localSecondShotTimer;
        private float firePower = 15;
        public MountableMortar(float xval, float yval) : base(xval, yval)
        {
            MountedGun = new SpriteMap(GetPath("Sprites/MountMortar"), 24, 18);
            MountedGun.CenterOrigin();

            reloadable = true;
            reloadTime = 3;

            autoFire = true;

            ammo = 6;
            magSize = 6;
            fireCooldownTime = 0.45f;

            barrelSize = new Vec2(16, -2);
        }

        public override void Update()
        {
            base.Update();
            if(localSecondShotTimer > 0)
            {
                localSecondShotTimer -= step;
                if(localSecondShotTimer <= 0)
                {
                    Fire();
                }
            }
        }

        public override void Fire()
        {
            base.Fire();
            if(ammo % 2 == 1 && ammo > 0)
            {
                localSecondShotTimer = 0.15f;
            }

            Vec2 firePos = new Vec2(position.x + barrelOffset.x, position.y + barrelOffset.y);
            Vec2 fireVelocity = new Vec2((float)Math.Cos(Maths.DegToRad(-GunAngleDegrees)) * offDir, (float)Math.Sin(Maths.DegToRad(-GunAngleDegrees))) * firePower;

            Cluster cluster = new Cluster(firePos.x, firePos.y);
            cluster.velocity = fireVelocity;
            Level.Add(cluster); 
            
            SFX.Play(GetPath("SFX/StaticWeaponMortarFire_01.wav"), 1f, 0f, 0f, false);
        }
    }
}
