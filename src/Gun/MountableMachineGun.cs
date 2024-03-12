using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckGame.WMD
{
    [EditorGroup("Faecterr's|Weapons|Mountable")]
    public class MountableMachineGun : MountableGun
    {
        private Vec2 firePos;
        private AT9mm ammoT;
        private Sound fireSound = SFX.Get(Mod.GetPath<WMD>("SFX/StaticWeaponMachineGunFireAlt.wav"), 1f, 0f, 0f, false);
        private float fire;
        public MountableMachineGun(float xval, float yval) : base(xval, yval)
        {
            MountedGun = new SpriteMap(GetPath("Sprites/MountMachineGun"), 24, 18);
            MountedGun.CenterOrigin();

            reloadable = true;
            reloadTime = 2.7f;

            magSize = 45;
            ammo = 45;
            fireCooldownTime = 0.09f;

            barrelSize = new Vec2(8, 4);
        }

        public override void Update()
        {
            base.Update();
            if(fire < 0 || isReloading)
            {
                fireSound.Pause();
            }
            if(fire > 0)
            {
                fire -= step;
            }
            if (ammoT == null)
            {
                ammoT = new AT9mm();
            }
            if (mounter != null && ammoT != null)
            {
                firePos = new Vec2(position.x + barrelOffset.x, position.y + barrelOffset.y);
            }
        }

        public override void Fire()
        {
            base.Fire();
            if (ammoT != null)
            {
                fire = fireCooldownTime + step * 2;
                float fireAngle = -GunAngleDegrees;
                if (offDir < 0)
                {
                    fireAngle = -(180 - GunAngleDegrees);
                }
                ammoT.FireBullet(firePos, mounter, fireAngle);
                if (fireSound != null)
                {
                    if (fireSound.State != Microsoft.Xna.Framework.Audio.SoundState.Playing)
                    {
                        fireSound.Play();
                    }
                }
            }
        }

        public override void Draw()
        {
            base.Draw();
        }
    }
}