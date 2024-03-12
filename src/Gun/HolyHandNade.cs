using System;
using System.Collections.Generic;
using DuckGame.WMD; //Whoopsie, looks like I forgot to put .WMD into namespace, so now it just here, to not cause problems with item spawners, that is in existing levels

namespace DuckGame
{
    [EditorGroup("Faecterr's|Weapons")]
    public class HolyHandGrenade : Gun
    {
        private SpriteMap _sprite;

        public StateBinding _timerBinding = new StateBinding("_timer", -1, false, false);
        public StateBinding _pinBinding = new StateBinding("_pin", -1, false, false);

        private bool _pin = true;
        private float _timer = 1.8f;
        private float range = 64;

        private Duck _cookThrower;
        private float _cookTimeOnThrow;
       
        private bool _explosionCreated;
        private bool _localDidExplode;
        private static int grenade;

        public int gr;
        public int _explodeFrames = -1;
        
        public bool didfalled;

        public bool boosted;

        public Duck cookThrower
        {
            get
            {
                return _cookThrower;
            }
        }
        
        public float cookTimeOnThrow
        {
            get
            {
                return _cookTimeOnThrow;
            }
        }
        
        public HolyHandGrenade(float xval, float yval) : base(xval, yval)
        {
            ammo = 1;
            _ammoType = new ATShrapnel();
            _ammoType.penetration = 0.4f;
            _type = "gun";
            _sprite = new SpriteMap(GetPath("Sprites/HolyHandGrenade.png"), 16, 16, false);
            graphic = _sprite;
            center = new Vec2(7f, 8f);
            collisionOffset = new Vec2(-4f, -5f);
            collisionSize = new Vec2(8f, 10f);
            bouncy = 0.4f;
            friction = 0.05f;
            _fireRumble = RumbleIntensity.Kick;
            _editorName = "Holy Hand Nade";
            editorTooltip = "Fear number two for every worm. Number one is ducks.";
            _bio = "To cook grenade, pull pin and hold until feelings of bless run down your soul.";
        }
        
        public void Boost()
        {
            _sprite = new SpriteMap(GetPath("Sprites/HolyMineGrenade.png"), 16, 16);
            graphic = _sprite;
            boosted = true;
            range = 80;
        }
        public override void Initialize()
        {
            gr = grenade;
            grenade++;
        }
        
        public override void OnNetworkBulletsFired(Vec2 pos)
        {
            _pin = false;
            _localDidExplode = true;
            if (!_explosionCreated)
            {
                Graphics.FlashScreen();
            }
            CreateExplosion(pos);
        }

        public virtual void Explosion()
        {
            ATMissileShrapnel shrap = new ATMissileShrapnel();
            shrap.MakeNetEffect(this.position, false);
            Random rand = null;
            if (Network.isActive && isLocal)
            {
                rand = Rando.generator;
                Rando.generator = new Random(NetRand.currentSeed);
            }
            List<Bullet> firedBullets = new List<Bullet>();
            for (int i = 0; i < 12; i++)
            {
                float dir = i * 30f - 10f + Rando.Float(20f);
                shrap = new ATMissileShrapnel();
                shrap.range = range + Rando.Float(range * 0.2f);
                Vec2 shrapDir = new Vec2((float)Math.Cos((double)Maths.DegToRad(dir)), (float)Math.Sin((double)Maths.DegToRad(dir)));
                Bullet bullet = new Bullet(x + shrapDir.x * 8f, y - shrapDir.y * 8f, shrap, dir, null, false, -1f, false, true);
                bullet.firedFrom = this;
                firedBullets.Add(bullet);
                Level.Add(bullet);
                Level.Add(Spark.New(x + Rando.Float(-8f, 8f), y + Rando.Float(-8f, 8f), shrapDir + new Vec2(Rando.Float(-0.1f, 0.1f), Rando.Float(-0.1f, 0.1f)), 0.02f));
                Level.Add(SmallSmoke.New(x + shrapDir.x * 8f + Rando.Float(-8f, 8f), y + shrapDir.y * 8f + Rando.Float(-8f, 8f)));
            }
            if (boosted)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (isServerForObject)
                    {
                        //Replace with landmines
                        LandMine m = new LandMine(position.x, position.y);
                        //Cluster m = new Cluster(position.x, position.y);
                        m.hSpeed = 3 * (-1 + i);
                        m.vSpeed = 3 * (Math.Abs(i - 1) - 1);
                        //m._pin = false;
                        //m.UpdatePinState();
                        m.toggled = true;
                        Level.Add(m);
                    }
                }
            }
            if (Network.isActive && isLocal)
            {
                NMFireGun gunEvent = new NMFireGun(null, firedBullets, 0, false, 4, false);
                Send.Message(gunEvent, NetMessagePriority.ReliableOrdered);
                firedBullets.Clear();
            }
            if (Network.isActive && isLocal)
            {
                Rando.generator = rand;
            }
            IEnumerable<Window> windows = Level.CheckCircleAll<Window>(position, range);
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
            IEnumerable<PhysicsObject> phys = Level.CheckCircleAll<PhysicsObject>(position, range * 1.5f);
            foreach (PhysicsObject p in phys)
            {
                if (this.isLocal && this.owner == null)
                {
                    Fondle(p, DuckNetwork.localConnection);
                }
                if ((p.position - this.position).length < 30f)
                {
                    p.Destroy(new DTImpact(this));
                }
                p.sleeping = false;
                p.vSpeed = -2f;
            }

            HashSet<ushort> idx = new HashSet<ushort>();
            IEnumerable<BlockGroup> blokzGroup = Level.CheckCircleAll<BlockGroup>(position, range);
            foreach (BlockGroup block in blokzGroup)
            {
                if (block != null)
                {
                    BlockGroup group = block;
                    new List<Block>();
                    foreach (Block bl in group.blocks)
                    {
                        if (Collision.Circle(this.position, range, bl.rectangle))
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
            IEnumerable<Block> blokz = Level.CheckCircleAll<Block>(position, range);
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
            foreach (Platform p in Level.CheckCircleAll<Platform>(position, range))
            {
                Level.Remove(p);
            }
            foreach (AutoPlatform p in Level.CheckCircleAll<AutoPlatform>(position, range))
            {
                Level.Remove(p);
            }
        }

        public void CreateExplosion(Vec2 pos)
        {
            if (!_explosionCreated)
            {
                float cx = pos.x;
                float cy = pos.y - 2f;
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

                Explosion();
                _explosionCreated = true;

                SFX.Play(GetPath("SFX/HHGExpl.wav"), 1f, 0f, 0f, false);
                RumbleManager.AddRumbleEvent(pos, new RumbleEvent(RumbleIntensity.Heavy, RumbleDuration.Short, RumbleFalloff.Medium, RumbleType.Gameplay));
            }
        }
        
        public override void Update()
        {
            base.Update();
            if (!_pin && didfalled)
            {
                _timer -= 0.01f;
                holsterable = false;
            }

            if (grounded && owner == null && !_pin)
            {
                didfalled = true;
            }

            if(_timer > 1f && _timer < 1.02f)
            {
                SFX.Play(GetPath("SFX/HHGHallelujah.wav"), 1f, 0f, 0f, false);
            }

            if (!_localDidExplode && _timer < 0f)
            {
                if (_explodeFrames < 0)
                {
                    CreateExplosion(position);
                    _explodeFrames = 4;
                }
                else
                {
                    _explodeFrames--;
                    if (_explodeFrames == 0)
                    {
                        float cx = x;
                        float cy = y - 2f;
                        Graphics.FlashScreen();
                        if (isServerForObject)
                        {
                            for (int i = 0; i < 20; i++)
                            {
                                float dir = i * 18f - 5f + Rando.Float(10f);
                                ATShrapnel shrap = new ATShrapnel();
                                shrap.range = 60f + Rando.Float(18f);
                                Bullet bullet = new Bullet(cx + (float)(Math.Cos(Maths.DegToRad(dir)) * 6.0), cy - (float)(Math.Sin(Maths.DegToRad(dir)) * 6.0), shrap, dir, null, false, -1f, false, true);
                                bullet.firedFrom = this;
                                firedBullets.Add(bullet);
                                Level.Add(bullet);
                            }
                            foreach (Window w in Level.CheckCircleAll<Window>(position, 40f))
                            {
                                if (Level.CheckLine<Block>(position, w.position, w) == null)
                                {
                                    w.Destroy(new DTImpact(this));
                                }
                            }
                            bulletFireIndex += 20;
                            if (Network.isActive)
                            {
                                Send.Message(new NMFireGun(this, firedBullets, bulletFireIndex, false, 4, false), NetMessagePriority.ReliableOrdered);
                                firedBullets.Clear();
                            }
                        }
                        Level.Remove(this);
                        _destroyed = true;
                        _explodeFrames = -1;
                    }
                }
            }
            if (prevOwner != null && _cookThrower == null)
            {
                _cookThrower = prevOwner as Duck;
                _cookTimeOnThrow = _timer;
            }
            _sprite.frame = _pin ? 0 : 1;
        }
        
        public override void OnPressAction()
        {
            if (_pin)
            {
                _pin = false;
                Level.Add(new GrenadePin(x, y)
                {
                    hSpeed = -offDir * (1.5f + Rando.Float(0.5f)),
                    vSpeed = -2f
                });
                if (duck != null)
                {
                    RumbleManager.AddRumbleEvent(duck.profile, new RumbleEvent(_fireRumble, RumbleDuration.Pulse, RumbleFalloff.None, RumbleType.Gameplay));
                }
                SFX.Play(GetPath("SFX/HHGEpic.wav"), 1f, 0f, 0f, false);
            }
        }

        public override void Draw()
        {
            if (!_pin)
            {
                SpriteMap _pulse = new SpriteMap(GetPath("Sprites/HHGLight"), 64, 64);
                _pulse.CenterOrigin();
                _pulse.color = Color.Yellow;
                _pulse.alpha = (1.8f - _timer) / 1.8f * 0.5f;
                _pulse.angle = (float)Math.Pow(_timer, 0.5f) * 10;
                _pulse.scale = new Vec2(2 - ((1.8f - _timer) / 1.8f), 2 - ((1.8f - _timer) / 1.8f));
                Graphics.Draw(_pulse, position.x, position.y);
                _pulse.alpha *= 0.2f;
                _pulse.angle = -(float)Math.Pow(_timer, 0.5f) * 7;
                _pulse.scale = new Vec2(2 + ((1.8f - _timer) / 1.8f), 2 + ((1.8f - _timer) / 1.8f));
                Graphics.Draw(_pulse, position.x, position.y);
            }
            base.Draw();
        }
    }
}
