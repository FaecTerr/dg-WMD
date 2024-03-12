using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckGame.WMD
{
    [EditorGroup("Faecterr's|Weapons|Mountable")]
    public class MountableFlamethrower : MountableGun
    {
        private float minFirePower = 3;
        private float firePower = 6; 
        private float firePowerStep = 0.5f;
        private Sound fireSound = SFX.Get(Mod.GetPath<WMD>("SFX/StaticWeaponFlameLoop.wav"), 1f, 0f, 0f, false);
        private float heat;
        public MountableFlamethrower(float xval, float yval) : base(xval, yval)
        {
            MountedGun = new SpriteMap(GetPath("Sprites/MountFlameThrower"), 42, 11);
            MountedGun.CenterOrigin();

            reloadable = true;
            reloadTime = 6;

            ammo = 30;
            magSize = 30;
            fireCooldownTime = 0.05f;

            barrelSize = new Vec2(26, -6);
        }
        public override void StartReload()
        {
            base.StartReload();
            SFX.Play(GetPath("SFX/StaticWeaponFlameRelease.wav"), 1f, 0f, 0f, false);
        }

        public override void Update()
        {
            base.Update();
            if(mounter != null)
            {
                if (mounter.inputProfile.Released("SHOOT"))
                {
                    if (heat > 0.9f && ammo > 0)
                    {
                        SFX.Play(GetPath("SFX/StaticWeaponFlameRelease.wav"), 1f, 0f, 0f, false);
                    }
                    fireSound.Pause();
                }
                if (mounter.inputProfile.Pressed("SHOOT") && heat < 0.1f && ammo > 0)
                {
                    SFX.Play(GetPath("SFX/StaticWeaponFlameStart.wav"), 1f, 0f, 0f, false);
                }
                if ((mounter.inputProfile.Down("SHOOT") || fireCooldown > 0) && !isReloading)
                {
                    heat += 0.02f;
                }
                else
                {
                    heat -= 0.01f;
                }

                if(heat > 1)
                {
                    heat = 1;
                }
                if(heat < 0)
                {
                    heat = 0;
                }
            }
        }

        public override void Fire()
        {
            base.Fire();

            for (float i = minFirePower; i <= firePower; i += firePowerStep)
            {
                Vec2 fireVelocity = new Vec2((float)Math.Cos(Maths.DegToRad(-GunAngleDegrees)) * offDir, (float)Math.Sin(Maths.DegToRad(-GunAngleDegrees))) * i;
                SmallFire fire = SmallFire.New(position.x + barrelOffset.x, position.y + barrelOffset.y, fireVelocity.x, fireVelocity.y);

                Level.Add(fire);
            }
            if (fireSound != null)
            {
                if (fireSound.State != Microsoft.Xna.Framework.Audio.SoundState.Playing)
                {
                    fireSound.Play();
                }
            }
        }
    }
}
