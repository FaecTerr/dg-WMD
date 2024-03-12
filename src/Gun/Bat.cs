using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckGame.WMD
{
    [EditorGroup("Faecterr's|Weapons")]
    public class Bat : Holdable
    {
        public float power = 0.4f;
        public float range = 24;
        public float rangeMod = 1f;

        public SpriteMap _sprite;
        public int animation;

        public Duck hitted;

        public bool powered; //powered can add some ability
        public bool boosted; //boosted making thing twice more effective

        public Bat(float xpos, float ypos) : base(xpos, ypos)
        {
            _sprite = new SpriteMap(GetPath("Sprites/Bat.png"), 9, 25);
            graphic = _sprite;
            center = new Vec2(4.5f, 12.5f);
            _holdOffset = new Vec2(-3, 7);
            _sprite.CenterOrigin();
            collisionOffset = new Vec2(-3.5f, -11.5f);
            collisionSize = new Vec2(7f, 23f);
            _editorName = "Baseball Bat";
            _sprite.frame = 1;
            physicsMaterial = PhysicsMaterial.Wood;
        }

        public virtual void Boost()
        {
            boosted = true;
            powered = false;
            _sprite = new SpriteMap(GetPath("Sprites/BatBUP.png"), 9, 25);
            graphic = _sprite;
            rangeMod = 1.5f;
            power = 0.4f;
        }

        public virtual void PowerUp()
        {
            powered = true;
            boosted = false;
            _sprite = new SpriteMap(GetPath("Sprites/BatPUP.png"), 9, 25);
            graphic = _sprite;
            rangeMod = 1f;
            power = 0.6f;
        }

        public override void Update()
        {
            if(hitted != null)
            {
                if(hitted.ragdoll != null)
                {
                    if(hitted.ragdoll.part1.x > Level.current.bottomRight.x || hitted.ragdoll.part1.x < Level.current.topLeft.x)
                    {
                        hitted = null;
                        SFX.Play(GetPath("SFX/BaseballBatHomeRun.wav"), 1f, 0f, 0f, false);
                    }
                }
            }

            if(_prevOwner != null && owner == null)
            {
                if(_prevOwner is Duck)
                {
                    if((prevOwner as Duck).runMax == 4.25f)
                    {
                        (prevOwner as Duck).runMax = 3.25f;
                    }
                    _prevOwner = null;
                }
            }
            if (animation <= 0)
            {
                _sprite.frame = 1;
            }
            else
            {
                animation--;
            }
            if (owner != null)
            {
                if(owner is Duck)
                {
                    (owner as Duck).tilt = owner.hSpeed * -0.1f * power;
                    (owner as Duck).doThrow = false;
                    (owner as Duck).runMax = 4.25f;
                    angle = offDir + power * 0.5f * offDir;
                    if ((owner as Duck).profile.inputProfile.Down("SHOOT"))
                    {
                        power += 0.04f;
                        if(power < 1.6f)
                        {
                            _sprite.frame = 0;
                        }
                        if(power > 3f)
                        {
                            _sprite.frame = 2;
                        }
                        if(power > 4)
                        {
                            _sprite.frame = 3;
                            power = 4;
                        }
                    }
                    if((owner as Duck).profile.inputProfile.Released("SHOOT"))
                    {
                        if (powered)
                        {
                            power *= 2;
                        }
                        Punch();
                    }
                    
                    (owner as Duck).holdAngleOff = angle;
                    if(animation > 0)
                    {
                        (owner as Duck).holdAngleOff = offDir - 3f * offDir;
                        angle = offDir - 3f * offDir;
                    }
                }
            }
            else
            {
                angle = 0;
            }

            base.Update();
        }

        public void Punch()
        {
            if (power >= 1.6f && owner != null)
            {
                if (owner != this)
                {
                    owner.hSpeed = (power * 1.5f + 0.5f) * offDir;
                    owner.vSpeed = -0.5f - power * 0.6f;
                }
                foreach (Duck d in Level.CheckCircleAll<Duck>(position + new Vec2(range * rangeMod * offDir, 0), range))
                {
                    if (d != owner && (d.position.x - owner.position.x) * offDir > 0)
                    {
                        if (boosted)
                        {
                            d.onFire = true;
                        }
                        d.GoRagdoll();
                        if (d.ragdoll != null)
                        {
                            d.ragdoll.part1.velocity = new Vec2((power * 2 + 3) * offDir, -0.5f - power * 0.5f);
                            d.ragdoll.part2.velocity = new Vec2((power * 2 + 3) * offDir, -0.5f - power * 0.5f);
                            d.ragdoll.part3.velocity = new Vec2((power * 2 + 3) * offDir - 0.5f - power * 0.5f);
                            d.ragdoll.part1.clip.Add(owner as Duck);
                            d.ragdoll.part2.clip.Add(owner as Duck);
                            d.ragdoll.part3.clip.Add(owner as Duck);

                            if (boosted)
                            {
                                d.ragdoll.part1.onFire = true; 
                                d.ragdoll.part2.onFire = true; 
                                d.ragdoll.part3.onFire = true;
                            }
                        }
                        SFX.Play(GetPath("SFX/BaseballBatImpact.wav"), 1f, 0f, 0f, false);
                        hitted = d;
                        if (power >= 4)
                        {
                            //SFX.Play(GetPath("SFX/BaseballBatHomeRun.wav"), 1f, 0f, 0f, false);
                        }
                        power = 0.4f;
                        animation = 45;
                        return;
                    }
                }
                foreach (Ragdoll r in Level.CheckCircleAll<Ragdoll>(position + new Vec2(range * rangeMod * offDir, 0), range))
                {
                    if ((r.position.x - owner.position.x) * offDir > 0)
                    {
                        r.hSpeed = (power * 2 + 0.5f) * offDir;
                        r.vSpeed = -0.5f - power * 0.75f;
                        //p.velocity = new Vec2((power * 2 + 3 ) * offDir, -0.5f - power * 0.5f);
                        SFX.Play(GetPath("SFX/BaseballBatImpact.wav"), 1f, 0f, 0f, false);
                        power = 0.4f;
                        animation = 45;
                        if (r._duck != null)
                        {
                            hitted = r._duck;
                        }
                        r.part1.hSpeed = (power * 2 + 0.5f) * offDir * 10;
                        r.part2.hSpeed = (power * 2 + 0.5f) * offDir * 10;
                        r.part3.hSpeed = (power * 2 + 0.5f) * offDir * 10;
                        r.part1.vSpeed = -0.5f - power * 0.75f * 10;
                        r.part2.vSpeed = -0.5f - power * 0.75f * 10;
                        r.part3.vSpeed = -0.5f - power * 0.75f * 10;
                        r.part1.clip.Add(owner as Duck);
                        r.part2.clip.Add(owner as Duck);
                        r.part3.clip.Add(owner as Duck);
                        if (boosted)
                        {
                            r.part1.onFire = true;
                            r.part2.onFire = true;
                            r.part3.onFire = true;
                        }
                    }
                }
                foreach (PhysicsObject p in Level.CheckCircleAll<PhysicsObject>(position + new Vec2(range * rangeMod * offDir, 0), range))
                {
                    if (p != this && p != owner && (p.position.x - owner.position.x) * offDir > 0)
                    {
                        if (boosted)
                        {
                            p.onFire = true;
                        }
                        p.hSpeed = (power * 2 + 0.5f) * offDir;
                        p.vSpeed = -0.5f - power * 0.75f;
                        //p.velocity = new Vec2((power * 2 + 3 ) * offDir, -0.5f - power * 0.5f);
                        SFX.Play(GetPath("SFX/BaseballBatImpact.wav"), 1f, 0f, 0f, false);
                        power = 0.4f;
                        animation = 45;
                        p.clip.Add(owner as Duck);
                        return;
                    }
                }
            }
            power = 0.4f;
        }

        public override void Draw()
        {
            if(animation > 35)
            {
                _sprite.frame = 0;
                if(animation <= 0)
                {
                    _sprite.frame = 1;
                }

                int anim = animation - 35;
                Graphics.DrawCircle(position + new Vec2(range * rangeMod * offDir, 0), range - (10 - anim) * 2, Color.White);
                Graphics.DrawCircle(position + new Vec2(range * rangeMod * offDir, 0), anim * 2, Color.White);
            }
            /*alpha = 1;
            if (power >= 1.6f && power < 2f)
            {
                alpha = (2f - power) * 2.5f;
            }
            if (power >= 3.6f && power < 4f)
            {
                alpha = (4f - power) * 2.5f;
            }*/
            base.Draw();
        }
    }
}
