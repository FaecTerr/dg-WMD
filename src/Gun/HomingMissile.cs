using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckGame.WMD
{
    [EditorGroup("Faecterr's|Weapons|Remote")]
    public class HominMissileStrike : Gun, IDrawToDifferentLayers
    {
        public SpriteMap _sprite;
        public SpriteMap _target;
        public Vec2 pos = new Vec2();
        public bool charging;
        public float power = 0.4f;

        public bool selected;
        public Sound sound;

        public bool boosted;
        public float moveMod = 3;

        public HominMissileStrike(float xpos, float ypos) : base(xpos, ypos)
        {
            _fireWait = 1f;

            _sprite = new SpriteMap(GetPath("Sprites/HomingStrike.png"), 14, 14, false);
            _target = new SpriteMap(GetPath("Sprites/AirStrikeAim.png"), 17, 17, false);
            _target.CenterOrigin();
            _target.frame = 1;
            graphic = _sprite;
            center = new Vec2(7f, 7f);
            collisionOffset = new Vec2(-4f, -6f);
            collisionSize = new Vec2(8f, 12f);
            _fullAuto = false;

            _fireRumble = RumbleIntensity.Kick;
            _editorName = "Homing missile";
            editorTooltip = "Select target with quack and fire";
            _bio = "To select target quack and then shoot homing rocket";

            isFatal = false;
            ammo = 1;
        }

        public virtual void Boost()
        {
            boosted = true;
            _sprite = new SpriteMap(GetPath("Sprites/HomingStrikeBUP.png"), 14, 14, false);
            graphic = _sprite;
            moveMod = 4.5f;
        }

        public override void Initialize()
        {
            _fullAuto = false;
            charging = false;
            power = 0.4f;
            pos = position;
            selected = false;
            base.Initialize();
        }

        public override void Update()
        {
            base.Update();
            if (owner != null)
            {
                Duck d = owner as Duck;
                if (!selected)
                {
                    if (d.profile.inputProfile.Pressed("SHOOT"))
                    {
                        pos = position;
                    }
                    if (d.profile.inputProfile.Released("SHOOT"))
                    {
                        d.immobilized = false;
                        SFX.Play(GetPath("SFX/HomingMissileLockOn.wav"), 1f, 0f, 0f, false);
                        selected = true;
                        _sprite.frame = 1;
                    }
                    if (d.profile.inputProfile.Down("SHOOT"))
                    {
                        d.immobilized = true;
                        if (charging == false)
                        {
                            charging = true;
                            pos = position;
                        }
                        else
                        {
                            Vec2 move = new Vec2();
                            if (d.profile.inputProfile.Down("UP") || d.profile.inputProfile.Down("JUMP"))
                            {
                                move += new Vec2(0f, -1f);
                            }
                            if (d.profile.inputProfile.Down("DOWN"))
                            {
                                move += new Vec2(0f, 1f);
                            }
                            if (d.profile.inputProfile.Down("LEFT"))
                            {
                                move += new Vec2(-1f, 0f);
                            }
                            if (d.profile.inputProfile.Down("RIGHT"))
                            {
                                move += new Vec2(1f, 0f);
                            }
                            pos += move * moveMod;
                            move = new Vec2(0f, 0f);
                        }
                    }
                }
            }
            if (prevOwner != null && owner == null)
            {
                if (_prevOwner is Duck)
                {
                    (_prevOwner as Duck).immobilized = false;
                }
                _prevOwner = null;
            }
        }

        public override void OnHoldAction()
        {
            if (pos != null && ammo > 0 && selected)
            {
                if (power < 0.44f)
                {
                    if (sound == null)
                    {
                        sound = SFX.Play(GetPath("SFX/PowerUpBar.wav"), 1, 0, 0, false);
                    }
                }
                power += 0.04f;
                if (power >= 4)
                {
                    OnReleaseAction();
                }
            }
        }

        public override void OnReleaseAction()
        {
            if (ammo > 0 && pos != null && selected)
            {
                if (sound != null)
                {
                    sound.Kill();
                }
                ammo--;
                if (duck != null)
                {
                    RumbleManager.AddRumbleEvent(duck.profile, new RumbleEvent(_fireRumble, RumbleDuration.Pulse, RumbleFalloff.None, RumbleType.Gameplay));
                }
                SFX.Play(GetPath("SFX/HomingMissileFire.wav"), 1f, 0f, 0f, false);

                ApplyKick();
                if (!receivingPress)
                {
                    HominMissile i = new HominMissile(position.x, position.y) { target = pos, boosted = boosted };
                    Level.Add(i);
                    Fondle(i);
                    if (owner != null)
                    {
                        i.responsibleProfile = owner.responsibleProfile;
                    }
                    i.clip.Add(owner as MaterialThing);
                    i.clip.Add(this);
                    i.hSpeed = barrelVector.x * 6f *  power;
                    i.vSpeed = barrelVector.y * 6f * 0.7f * power;
                    return;
                }
            }
            else
            {
                DoAmmoClick();                
            }
        }

        public override void Fire()
        {
        }

        public void OnDrawLayer(Layer layer)
        {
            if (layer == Layer.Foreground)
            {
                if (charging == true && ammo > 0)
                {
                    Graphics.Draw(_target, pos.x, pos.y);
                }
                /*if(power > 0)
                {
                    for (int i = 0; i < (int)(power / 0.4f); i++)
                    {
                        Color c = new Vec3(255, 255 - 155 * (power * (i / 10) / 4), 0).ToColor();
                        Graphics.DrawCircle(barrelPosition + new Vec2((float)Math.Cos(offDir > 0 ? angle : -(3.14f - angle)) * 8 * (i / 10), 
                            (float)Math.Sin(offDir > 0 ? angle : 3.14f - angle) * 8 * (i / 10)), 
                            7 + power * (i / 8),
                            c, 
                            1f, 
                            0.3f + 0.01f * power * (i/10), 
                            32);
                    }
                }*/
            }
        }
    }

    public class HominMissile : PhysicsObject, IDrawToDifferentLayers
    {
        public SpriteMap _target;
        public SpriteMap _sprite;
        public Vec2 target;
        public float notHome = 0.3f;
        public float speed = 3;
        public float KillTIme = 0.1f * 60 * 12; //12 seconds until forced explosion]

        public float targetAngle;

        public Vec2 moveVector;

        public bool boosted;

        public HominMissile(float xval, float yval) : base(xval, yval)
        {
            dontCrush = true;
            _target = new SpriteMap(GetPath("Sprites/AirStrikeAim.png"), 17, 17, false);
            _sprite = new SpriteMap(GetPath("Sprites/Missiles.png"), 11, 21);
            _sprite.CenterOrigin();
            graphic = _sprite;
            center = _sprite.center;
            _sprite.frame = 1;
            _target.frame = 1;
            collisionOffset = new Vec2(-3f, -5f);
            collisionSize = new Vec2(6f, 10f);
            friction = 0.2f;
            thickness = 0;
            physicsMaterial = PhysicsMaterial.Metal;

            gravMultiplier = 0;
        }

        public override void Initialize()
        {
            if (boosted)
            {
                _sprite = new SpriteMap(GetPath("Sprites/MissilesBUP.png"), 11, 21);
                _sprite.frame = 1;
                speed *= 2f;
                graphic = _sprite;
            }
            base.Initialize();
        }

        public override void Update()
        {
            _skipAutoPlatforms = true;
            _skipPlatforms = true;
            base.Update();
            offDir = 1;
            if (notHome <= 0)
            {
                if (Math.Abs(hSpeed) + Math.Abs(vSpeed) > 0.1f)
                {
                    angleDegrees = -Maths.PointDirection(Vec2.Zero, new Vec2(hSpeed, vSpeed)) + 90;
                }

                //hSpeed -= 0.098f;

                //float dir = 0;
                //graphic.angle = (float)Maths.PointDirection(position, target);

                Vec2 motion = (position - target).normalized;

                hSpeed += -motion.x;
                vSpeed += -motion.y - 0.098f;
                //hSpeed += -moveVector.normalized.x;
                //vSpeed += -moveVector.normalized.y - 0.098f;
                //hSpeed += (float)Math.Cos(targetAngle) * speed;
                //vSpeed += (float)Math.Sin(targetAngle) * speed;

                if ((target - position).length < 3)
                {
                    Explode();
                }
                else
                {
                    KillTIme -= 0.1f;

                    if(KillTIme <= 0)
                    {
                        Explode();
                    }
                }
                if(friction < 0.5f)
                {
                    friction += 0.01f;
                }
            }
            else
            {
                angleDegrees += 16;
                notHome -= 0.01f;
                if(notHome <= 0)
                {
                    if (hSpeed != 0)
                    {
                        targetAngle = MathHelper.Lerp(targetAngle, (float)Math.Atan((_target.y - position.y) / (_target.x - position.x) * offDir) + 1.57f * offDir, 0.1f);
                    }
                    else
                    {
                        targetAngle = 1.57f + 1.57f * offDir;
                    }
                    velocity = new Vec2(0, 0);
                    angle = targetAngle;
                    Vec2 motion = (position - target);
                    moveVector = motion;
                }
            }
        }

        public virtual void Explode()
        {
            ATMissileShrapnel shrap = new ATMissileShrapnel();
            shrap.MakeNetEffect(this.position, false);
            Random rand = null;
            if (Network.isActive && this.isLocal)
            {
                rand = Rando.generator;
                Rando.generator = new Random(NetRand.currentSeed);
            }
            List<Bullet> firedBullets = new List<Bullet>();
            for (int i = 0; i < 12; i++)
            {
                float dir = i * 30f - 10f + Rando.Float(20f);
                shrap = new ATMissileShrapnel();
                shrap.range = 15f + Rando.Float(5f);
                Vec2 shrapDir = new Vec2((float)Math.Cos((double)Maths.DegToRad(dir)), (float)Math.Sin((double)Maths.DegToRad(dir)));
                Bullet bullet = new Bullet(this.x + shrapDir.x * 8f, this.y - shrapDir.y * 8f, shrap, dir, null, false, -1f, false, true);
                bullet.firedFrom = this;
                firedBullets.Add(bullet);
                Level.Add(bullet);
                Level.Add(Spark.New(this.x + Rando.Float(-8f, 8f), this.y + Rando.Float(-8f, 8f), shrapDir + new Vec2(Rando.Float(-0.1f, 0.1f), Rando.Float(-0.1f, 0.1f)), 0.02f));
                Level.Add(SmallSmoke.New(this.x + shrapDir.x * 8f + Rando.Float(-8f, 8f), this.y + shrapDir.y * 8f + Rando.Float(-8f, 8f)));
            }
            if (Network.isActive && this.isLocal)
            {
                NMFireGun gunEvent = new NMFireGun(null, firedBullets, 0, false, 4, false);
                Send.Message(gunEvent, NetMessagePriority.ReliableOrdered);
                firedBullets.Clear();
            }
            if (Network.isActive && this.isLocal)
            {
                Rando.generator = rand;
            }
            IEnumerable<Window> windows = Level.CheckCircleAll<Window>(this.position, 30f);
            foreach (Window w in windows)
            {
                if (this.isLocal)
                {
                    Thing.Fondle(w, DuckNetwork.localConnection);
                }
                if (Level.CheckLine<Block>(this.position, w.position, w) == null)
                {
                    w.Destroy(new DTImpact(this));
                }
            }
            IEnumerable<PhysicsObject> phys = Level.CheckCircleAll<PhysicsObject>(this.position, 70f);
            foreach (PhysicsObject p in phys)
            {
                if (this.isLocal && this.owner == null)
                {
                    Thing.Fondle(p, DuckNetwork.localConnection);
                }
                if ((p.position - this.position).length < 30f)
                {
                    p.Destroy(new DTImpact(this));
                }
                p.sleeping = false;
                p.vSpeed = -2f;
            }
            HashSet<ushort> idx = new HashSet<ushort>();
            IEnumerable<BlockGroup> blokzGroup = Level.CheckCircleAll<BlockGroup>(this.position, 50f);
            foreach (BlockGroup block in blokzGroup)
            {
                if (block != null)
                {
                    BlockGroup group = block;
                    new List<Block>();
                    foreach (Block bl in group.blocks)
                    {
                        if (Collision.Circle(this.position, 28f, bl.rectangle))
                        {
                            bl.shouldWreck = true;
                            if (bl is AutoBlock)
                            {
                                idx.Add((bl as AutoBlock).blockIndex);
                            }
                        }
                    }
                    group.Wreck();
                }
            }
            IEnumerable<Block> blokz = Level.CheckCircleAll<Block>(this.position, 28f);
            foreach (Block block2 in blokz)
            {
                if (block2 is AutoBlock)
                {
                    block2.skipWreck = true;
                    block2.shouldWreck = true;
                    if (block2 is AutoBlock)
                    {
                        idx.Add((block2 as AutoBlock).blockIndex);
                    }
                }
                else if (block2 is Door || block2 is VerticalDoor)
                {
                    Level.Remove(block2);
                    block2.Destroy(new DTRocketExplosion(null));
                }
            }
            if (Network.isActive && isLocal && idx.Count > 0)
            {
                Send.Message(new NMDestroyBlocks(idx));
            }
            SFX.Play(GetPath("SFX/HomingMissileExplosion.wav"), 1f, 0f, 0f, false);

            if (boosted)
            {
                int pieces = 4;
                for (int i = 0; i < pieces; i++)
                {
                    float dir = 360 / pieces + Rando.Float(10f);
                    Cluster c = new Cluster(position.x, position.y);
                    c.velocity = new Vec2((float)Math.Cos(dir), (float)Math.Sin(dir)) * 5;

                    Level.Add(c);
                }
                for (int i = 0; i < 10; i++)
                {
                    SmallFire s = SmallFire.New(position.x, position.y, (5 - i) * 0.55f, -2 - Math.Abs(5 - i) * 0.15f, false);
                    Level.Add(s);
                }
            }

            Level.Remove(this);
        }

        public override void Touch(MaterialThing with)
        {
            if (with != null)
            {
                if (with is Block)
                {
                    if (with is Duck)
                    {
                        if ((with as Duck).profile == responsibleProfile)
                        {
                            return;
                        }
                    }
                    Explode();
                }
            }
            base.Touch(with);
        }

        public void OnDrawLayer(Layer layer)
        {
            if (layer == Layer.Foreground)
            {
                if (target != null)
                {
                    Graphics.Draw(_target, target.x, target.y);

                    //Graphics.DrawLine(position, target, Color.White, 1f);
                }
            }
        }
    }
}
