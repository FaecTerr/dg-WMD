using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckGame.WMD
{
    [EditorGroup("Faecterr's|Weapons|Mountable")]
    public class MountableSniper : MountableGun
    {
        private Vec2 firePos;
        private Vec2 wallPos;
        private ATHighCalSniper ammoT;
        public MountableSniper(float xval, float yval) : base(xval, yval)
        {
            MountedGun = new SpriteMap(GetPath("Sprites/MountSniperRifle"), 42, 11);
            MountedGun.center = new Vec2(14, 5.5f);

            reloadable = true;
            reloadTime = 2.7f;

            magSize = 3;
            ammo = 3;
            fireCooldownTime = 0.45f;
            fireCooldown = 0;
            isReloading = false;

            barrelSize = new Vec2(24, -2); 
        }

        public override void Update()
        {
            base.Update();
            if(ammoT == null)
            {
                ammoT = new ATHighCalSniper();
            }
            if (mounter != null && ammoT != null)
            {
                firePos = new Vec2(position.x + barrelOffset.x, position.y + barrelOffset.y);
                ATTracer attracer = new ATTracer();
                attracer.range = ammoT.range;
                float num2 = MountedGun.angleDegrees;
                num2 *= -1f;
                if (offDir < 0)
                {
                    num2 += 180f;
                }
                Vec2 vec = firePos;
                attracer.penetration = 0.4f;
                Bullet bullet = new Bullet(vec.x, vec.y, attracer, num2, mounter, false, -1f, true, true);
                wallPos = bullet.end;
            }
        }

        public override void Fire()
        {
            base.Fire();
            if (ammoT != null)
            {
                float fireAngle = -GunAngleDegrees; 
                if (offDir < 0)
                {
                    fireAngle = -(180 - GunAngleDegrees);
                }
                SFX.Play(GetPath("SFX/StaticWeaponSniperFire_01.wav"), 1f, 0f, 0f, false);
                ammoT.FireBullet(firePos, mounter, fireAngle);
            }
        }

        public override void Draw()
        {
            if (mounter != null && fireCooldown < 0 && !isReloading)
            {
                Graphics.DrawLine(firePos, wallPos, Color.Red * 0.4f, 1f, depth - 1);
            }
            base.Draw();
        }
    }
}
