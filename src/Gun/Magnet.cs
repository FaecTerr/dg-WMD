using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckGame.WMD
{
    [EditorGroup("Faecterr's|Stuff")]
    public class Magnet : Holdable, IDrawToDifferentLayers
    {
        public bool repulse = true;
        public float time = 0.1f * 60 * 30; // magnet will be active 30 seconds
        public float range = 80;
        public float power = 5;
        public bool placd;

        public SpriteMap _sprite;
        public SpriteMap _pulse;

        public float step1 = 0;
        public float step2 = 30;
        public float step3 = 60;

        public bool powered;

        public Magnet(float xval, float yval) : base(xval, yval)
        {
            _sprite = new SpriteMap(GetPath("Sprites/Magnet.png"), 16, 16);
            graphic = _sprite;
            center = new Vec2(8f, 8f);
            _holdOffset = new Vec2(0, 3);
            _sprite.CenterOrigin();
            collisionOffset = new Vec2(-7f, -7f);
            collisionSize = new Vec2(14f, 14f);
            _editorName = "Magnet";

            _pulse = new SpriteMap(GetPath("Sprites/Pulse"), 64, 64);
            _pulse.CenterOrigin();

            friction = 0.3f;
            dontCrush = true;

            _sprite.AddAnimation("idle", 1f, false, new int[] { 0 });
            _sprite.AddAnimation("pulsor", 0.3f, true, new int[] { 1, 2, 3 });
            _sprite.AddAnimation("repulsor", 0.3f, true, new int[] { 4, 5, 6});
            _sprite.AddAnimation("dry", 1f, false, new int[] { 7 });
        }

        public virtual void PowerUp()
        {
            powered = true;
            range = 160;
            power = 10;
            string anim = _sprite.currentAnimation;
            int fram = _sprite.frame;
            time = 0.1f * 60 * 30; // magnet will be active 30 seconds

            _sprite = new SpriteMap(GetPath("Sprites/MagnetPUP.png"), 16, 16);
            _sprite.CenterOrigin();
            graphic = _sprite;
            _sprite.SetAnimation(anim);
            _sprite.frame = fram;
        }

        public void OnDrawLayer(Layer pLayer)
        {
            if(pLayer == Layer.Foreground)
            {
                if (placd)
                {
                    if (repulse)
                    {
                        step1++;
                        step2++;
                        step3++;

                        if(step1 > 90)
                        {
                            if(time > 0)
                            SFX.Play(GetPath("SFX/ElectroMagnetPulse.wav"), 1f, 0f, 0f, false);
                            step1 = 0;
                        }
                        if(step2 > 90)
                        {
                            step2 = 0;
                        }
                        if(step3 > 90)
                        {
                            step3 = 0;
                        }
                        
                        _pulse.color = Color.Magenta;
                        for (int i = 0; i < 3; i++)
                        {
                            if(i == 0)
                            {
                                _pulse.angle = step1 * 0.03f;
                                _pulse.scale = new Vec2(0.9f + (step1 / 60), 0.9f + (step1 / 60)) * (range / 64);
                                _pulse.alpha = 0.3f;
                                if(step1 > 80)
                                {
                                    _pulse.alpha *= (90 - step1) / 10;
                                }
                            }
                            if (i == 1)
                            {
                                _pulse.angle = -step2 * 0.025f;
                                _pulse.scale = new Vec2(0.9f + (step2 / 60), 0.9f + (step2 / 60)) * (range / 64);
                                _pulse.alpha = 0.3f;
                                if (step2 > 80)
                                {
                                    _pulse.alpha *= (90 - step2) / 10;
                                }
                            }
                            if (i == 2)
                            {
                                _pulse.angle = step3 * 0.01f;
                                _pulse.scale = new Vec2(0.9f + (step3 / 60), 0.9f + (step3 / 60)) * (range / 64);
                                _pulse.alpha = 0.3f;
                                if (step3 > 80)
                                {
                                    _pulse.alpha *= (90 - step3) / 10;
                                }
                            }

                            if(time < 6)
                            {
                                _pulse.alpha *= time / 6;
                            }

                            Graphics.Draw(_pulse, position.x, position.y);
                        }
                    }
                    else
                    {
                        step1--;
                        step2--;
                        step3--;

                        if (step1 < 0)
                        {
                            if(time > 0)
                            SFX.Play(GetPath("SFX/ElectroMagnetPulse.wav"), 1f, 0f, 0f, false);
                            step1 = 90;
                        }
                        if (step2 < 0)
                        {
                            step2 = 90;
                        }
                        if (step3 < 0)
                        {
                            step3 = 90;
                        }

                        _pulse.color = Color.Blue;
                        for (int i = 0; i < 3; i++)
                        {
                            if (i == 0)
                            {
                                _pulse.angle = step1 * 0.03f;
                                _pulse.scale = new Vec2(0.9f + (step1 / 60), 0.9f + (step1 / 60)) * (range / 64);
                                _pulse.alpha = 0.3f;
                                if (step1 > 80)
                                {
                                    _pulse.alpha *= (90 - step1) / 10;
                                }
                            }
                            if (i == 1)
                            {
                                _pulse.angle = -step2 * 0.025f;
                                _pulse.scale = new Vec2(0.9f + (step2 / 60), 0.9f + (step2 / 60)) * (range / 64);
                                _pulse.alpha = 0.3f;
                                if (step2 > 80)
                                {
                                    _pulse.alpha *= (90 - step2) / 10;
                                }
                            }
                            if (i == 2)
                            {
                                _pulse.angle = step3 * 0.01f;
                                _pulse.scale = new Vec2(0.9f + (step3 / 60), 0.9f + (step3 / 60)) * (range / 64);
                                _pulse.alpha = 0.3f;
                                if (step3 > 80)
                                {
                                    _pulse.alpha *= (90 - step3) / 10;
                                }
                            }
                            if (time < 6)
                            {
                                _pulse.alpha *= time / 6;
                            }
                            Graphics.Draw(_pulse, position.x, position.y);
                        }
                    }
                }
            }
        }

        public override void Update()
        {
            if (placd)
            {
                if (time > 0)
                {
                    time -= 0.1f;
                    if(time <= 0)
                    {
                        _sprite.SetAnimation("dry");
                    }
                    foreach (PhysicsObject p in Level.CheckCircleAll<PhysicsObject>(position, range))
                    {
                        if (!(p is Duck) && !(p is Crate) && !(p is Flower) && !(p is Net) && (p.physicsMaterial == PhysicsMaterial.Metal))
                        {
                            if (repulse)
                            {
                                p.velocity += (p.position - position).normalized * power;
                            }
                            else
                            {
                                p.velocity -= (p.position - position).normalized * power;
                            }
                        }
                    }
                    foreach (Bullet b in Level.CheckCircleAll<Bullet>(position, range))
                    {
                        if (repulse)
                        {
                            Level.Remove(b);
                        }
                        else
                        {
                            Level.Remove(b);
                        }
                    }
                }
            }
            else
            {
                if (owner != null)
                {
                    if (owner is Duck)
                    {
                        if (repulse && _sprite.currentAnimation != "pulsor")
                        {
                            _sprite.SetAnimation("pulsor");
                        }
                        else if (!repulse && _sprite.currentAnimation != "repulsor")
                        {
                            _sprite.SetAnimation("repulsor");
                        }
                        if ((owner as Duck).profile.inputProfile.Pressed("SHOOT"))
                        {
                            (owner as Duck).doThrow = true;
                            placd = true;
                            canPickUp = false;
                            DuckNetwork.SendToEveryone(new NMMagnet(this, repulse));
                        }
                        if ((owner as Duck).profile.inputProfile.Pressed("QUACK"))
                        {
                            repulse = !repulse;
                        }
                    }
                }
            }
            if(owner == null && time > 0)
            {
                _sprite.SetAnimation("idle");
            }
            base.Update();
        }
    }
}
