using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckGame.WMD
{
    public class MountableGun : Thing
    {
        private SpriteMap basePlatform;
        private SpriteMap tripod;
        public SpriteMap MountedGun;

        public Duck mounter;

        private float mountRange = 16;

        protected int magSize;
        protected int ammo;
        protected float step = 0.01f;

        protected float fireCooldownTime;
        protected float fireCooldown;
        protected bool autoFire;
        private bool isFiring;

        protected bool reloadable;
        protected bool isReloading;
        protected float reloadTime;
        protected float reload;

        private float rotationSpeedDegrees = 0.8f;
        private float gunAngleDegrees;
        protected float TopBorderOfRotation = 70;
        protected float LowBorderOfRotation = -30;
        protected float dropGun;
        public float GunAngleDegrees 
        { 
            get 
            {
                return gunAngleDegrees;
            }
            set 
            { 
                if(value > TopBorderOfRotation)
                {
                    value = TopBorderOfRotation;
                }
                if(value < LowBorderOfRotation)
                {
                    value = LowBorderOfRotation;
                }
                gunAngleDegrees = value;
            }
        }

        protected Vec2 barrelSize;
        public Vec2 barrelOffset
        {
            get 
            {
                float s = (float)Math.Sin(Maths.DegToRad(GunAngleDegrees));
                float c = (float)Math.Cos(Maths.DegToRad(GunAngleDegrees));

                Vec2 translatedPosition = new Vec2(barrelSize.x, barrelSize.y);
                translatedPosition.x = barrelSize.x * c - barrelSize.y * s;
                translatedPosition.y = barrelSize.x * s + barrelSize.y * c;

                return new Vec2(translatedPosition.x * offDir + 7 * offDir, -translatedPosition.y - 5.5f);
            }
        }

        public MountableGun(float xval, float yval) : base(xval, yval)
        {
            ammo = magSize;

            tripod = new SpriteMap(GetPath("Sprites/MountStation.png"), 11, 18);
            tripod.CenterOrigin();

            basePlatform = new SpriteMap(GetPath("Sprites/MountPlatform.png"), 18, 18);
            basePlatform.CenterOrigin();
        }

        public virtual void Fire()
        {
            fireCooldown = fireCooldownTime;
            ammo--;

            if (autoFire && !isFiring)
            {
                isFiring = true;
            }
        }
        public virtual void StartReload()
        {
            isReloading = true;
            reload = reloadTime;
        }
        public void Reload()
        {
            if (reload > 0)
            {
                reload -= step;
            }
            else
            {
                isReloading = false;
                ammo = magSize;
                isFiring = false;
            }
        }

        public override void Update()
        {
            if(mounter != null)
            {
                //If picked item while mounting
                if (mounter.holdObject != null)
                {
                    mounter.doThrow = true;
                }

                if (mounter.inputProfile.Down("UP") || mounter.inputProfile.Down("JUMP"))
                {
                    GunAngleDegrees += rotationSpeedDegrees;
                }
                if (mounter.inputProfile.Down("DOWN"))
                {
                    GunAngleDegrees -= rotationSpeedDegrees;
                }

                dropGun = GunAngleDegrees;

                if (isFiring && autoFire && ammo > 0 && fireCooldown <= 0)
                {
                    Fire();
                }
                if (isReloading)
                {
                    Reload();
                }

                if (mounter.inputProfile.Down("SHOOT"))
                {
                    if (ammo > 0)
                    {
                        if (fireCooldown <= 0)
                        {
                            Fire();
                        }
                    }
                    else
                    {
                        if (reloadable && !isReloading)
                        {
                            StartReload();
                        }
                    }
                }
                if (mounter.inputProfile.Pressed("GRAB"))
                {
                    Dismount();
                    SFX.Play(GetPath("SFX/StaticWeaponExit.wav"), 1f, 0f, 0f, false);
                }
            }
            else
            {
                foreach(Duck d in Level.CheckCircleAll<Duck>(position, mountRange))
                {
                    if(d.holdObject == null && d.inputProfile.Pressed("GRAB") && Level.CheckLine<Block>(d.position, position) == null)
                    {
                        mounter = d;
                        SFX.Play(GetPath("SFX/StaticWeaponEnter.wav"), 1f, 0f, 0f, false);
                    }
                }
                if (mounter != null)
                {
                    mounter.immobilized = true;
                    mounter.enablePhysics = false;
                    mounter.position = position + new Vec2(0, -7);
                    mounter.grounded = true;
                    mounter.offDir = offDir;
                    mounter.velocity = new Vec2();
                    dropGun = Maths.LerpTowards(dropGun, GunAngleDegrees, 1);
                }

                if(mounter == null)
                {
                    float dropSpeed = 0.01f;
                    if(dropGun <= GunAngleDegrees - 0.12f)
                    {
                        dropSpeed = 3;
                    }
                    dropGun = Maths.LerpTowards(dropGun, LowBorderOfRotation, dropSpeed);
                }
            }

            if(fireCooldown > 0)
            {
                fireCooldown -= step;
            }
            base.Update();
        }

        void Dismount()
        {
            if (mounter != null)
            {
                mounter.enablePhysics = true;
                mounter.immobilized = false;
                mounter = null;
                dropGun = GunAngleDegrees;
            }
        }

        public override void Draw()
        {
            basePlatform.depth = depth.value - 0.01f;
            Graphics.Draw(tripod, position.x + 8 * offDir, position.y);
            Graphics.Draw(basePlatform, position.x, position.y + 7f);

            if (MountedGun != null)
            {
                MountedGun.flipH = offDir == -1;
                MountedGun.depth = depth.value + 0.01f;
                MountedGun.angleDegrees = GunAngleDegrees;
                if(mounter == null)
                {
                    MountedGun.angleDegrees = dropGun;
                }
                if(offDir == 1)
                {
                    MountedGun.angleDegrees = 360 - MountedGun.angleDegrees;
                }
                Graphics.Draw(MountedGun, position.x + 7 * offDir, position.y - 5.5f);
            }
            if(mounter != null)
            {
                if (isReloading)
                {
                    string text = "Reloading";
                    Graphics.DrawStringOutline(text, position + new Vec2(-text.Length * 4), Color.White, Color.Black, depth, null, 1);
                }
            }
            base.Draw();
        }
    }
}
