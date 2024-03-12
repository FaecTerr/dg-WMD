using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckGame.WMD
{
    [EditorGroup("Faecterr's|Weapons")]
    public class LandMine : Gun
    {
        EditorProperty<bool> Toggled;

        private SpriteMap _sprite;

        private float activationRange = 48;
        private float explosionRange = 144;
        private float activationTime = 0.9f; //0.01f per frame
        public bool toggled;
        private bool triggered;

        private bool explosionCreated;

        private int prevNum = 0;
        
        public LandMine(float xval, float yval) : base(xval, yval)
        {
            ammo = 1;

            _sprite = new SpriteMap(GetPath("Sprites/LandMine.png"), 12, 10);
            graphic = _sprite;
            _sprite.CenterOrigin();
            center = new Vec2(6, 7);

            collisionSize = new Vec2(10, 6);
            collisionOffset = new Vec2(-5, -3);

            bouncy = 0.35f;

            Toggled = new EditorProperty<bool>(false);
        }

        public override void Initialize()
        {
            base.Initialize(); 
            if (!toggled)
            {
                toggled = Toggled;
            }
        }

        public override void Fire()
        {
            if (!toggled)
            {
                toggled = true;
                canPickUp = false;
                if(owner != null && owner is Duck)
                {
                    (owner as Duck).doThrow = true;
                }
                grounded = false;
            }
        }

        public override void Update()
        {
            base.Update();
            if (toggled)
            {
                if (!triggered)
                {
                    if (grounded)
                    {
                        triggered = Level.CheckCircle<Duck>(position, activationRange) != null;
                        if (triggered)
                        {
                            _sprite.frame = (_sprite.frame + 1) % 2;
                            SFX.Play(GetPath("SFX/MineTick.wav"), 1f, 0f, 0f, false);
                        }
                    }
                }
                else
                {
                    activationTime -= 0.01f;
                    int Num = (int)(Math.Pow(1 / (activationTime * 0.5f), 2) * 0.5f);
                    if(Num != prevNum)
                    {
                        prevNum = Num;
                        _sprite.frame = (_sprite.frame + 1) % 2;
                        SFX.Play(GetPath("SFX/MineTick.wav"), 1f, 0f, 0f, false);
                    }

                    if(activationTime <= 0)
                    {
                        CreateExplosion(position);
                    }
                }
            }
        }

        public void CreateExplosion(Vec2 pos)
        {
            if (!explosionCreated)
            {
                float cx = pos.x;
                float cy = pos.y;
                Level.Add(new ExplosionPart(cx, cy, true));
                int num = 6;
                if (Graphics.effectsLevel < 2)
                {
                    num = 3;
                }
                for (int i = 0; i < num; i++)
                {
                    float dir = i * 60f + Rando.Float(-10f, 10f);
                    float dist = Rando.Float(12f, 20f);
                    Level.Add(new ExplosionPart(cx + (float)(Math.Cos(Maths.DegToRad(dir)) * dist), cy - (float)(Math.Sin(Maths.DegToRad(dir)) * dist), true));
                }
                for (int i = 0; i < 40; i++)
                {
                    float dir = i * (360 / 40) - 5f + Rando.Float(10f);
                    ATShrapnel shrap = new ATShrapnel();
                    shrap.range = explosionRange + Rando.Float(explosionRange * 0.2f);
                    Bullet bullet = new Bullet(cx + (float)(Math.Cos((double)Maths.DegToRad(dir)) * 6.0), cy - (float)(Math.Sin((double)Maths.DegToRad(dir)) * 6.0), shrap, dir, null, false, -1f, false, true);
                    bullet.firedFrom = this;
                    firedBullets.Add(bullet);
                    Level.Add(bullet);
                }

                explosionCreated = true;

                SFX.Play(GetPath("SFX/MineExplosion.wav"), 1f, 0f, 0f, false);

                RumbleManager.AddRumbleEvent(pos, new RumbleEvent(RumbleIntensity.Heavy, RumbleDuration.Short, RumbleFalloff.Medium, RumbleType.Gameplay));
                Level.Remove(this);
            }
        }
    }
}
