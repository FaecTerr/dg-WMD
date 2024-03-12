using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckGame.WMD
{
    [EditorGroup("Faecterr's|Weapons")]
    public class BananaBomb : Gun
    {
        public bool _pin = true;
        public int pieces = 5;
        public float range = 48;
        public SpriteMap _sprite;

        public StateBinding _timerBinding = new StateBinding("_timer", -1, false, false);
        public StateBinding _pinBinding = new StateBinding("_pin", -1, false, false);
        public float _timer = 1.6f;

        private Duck _cookThrower;
        private float _cookTimeOnThrow;
        public bool pullOnImpact;

        private bool _explosionCreated;
        private bool _localDidExplode;
        private bool _didBonus;
        private static int grenade;
        public int gr;

        public int _explodeFrames = -1;

        public BananaBomb(float xval, float yval) : base(xval, yval)
        {
            dontCrush = true;
            ammo = 1;
            _ammoType = new ATShrapnel();
            _ammoType.penetration = 0.4f;
            _type = "gun";
            _sprite = new SpriteMap(GetPath("Sprites/BananaBomb.png"), 16, 16);
            graphic = _sprite;
            center = new Vec2(7f, 8f);
            collisionOffset = new Vec2(-4f, -5f);
            collisionSize = new Vec2(8f, 10f);
            bouncy = 0.6f;
            friction = 0.05f;
            _fireRumble = RumbleIntensity.Kick;
            _editorName = "Banana bomb";
            editorTooltip = "The only weapon that could be identified as mass destruction";
            _bio = "To cook grenade, pull pin and hold until feelings of terror run down your spine. Could serve an entire hotel";
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
                    float dir = (float)i * 60f + Rando.Float(-10f, 10f);
                    float dist = Rando.Float(12f, 20f);
                    Level.Add(new ExplosionPart(cx + (float)(Math.Cos(Maths.DegToRad(dir)) * dist), cy - (float)(Math.Sin(Maths.DegToRad(dir)) * dist), true));
                }
                _explosionCreated = true;
                SFX.Play("explode", 1f, 0f, 0f, false);
                RumbleManager.AddRumbleEvent(pos, new RumbleEvent(RumbleIntensity.Heavy, RumbleDuration.Short, RumbleFalloff.Medium, RumbleType.Gameplay));
                Level.Remove(this);
            }
        }
        public void CreateBlockExplosion()
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
                    shrap.range = range + Rando.Float(range * 0.2f);
                    Bullet bullet = new Bullet(cx + (float)(Math.Cos(Maths.DegToRad(dir)) * 6.0), cy - (float)(Math.Sin(Maths.DegToRad(dir)) * 6.0), shrap, dir, null, false, -1f, false, true);
                    bullet.firedFrom = this;
                    firedBullets.Add(bullet);
                    Level.Add(bullet);
                }
                foreach (Window w in Level.CheckCircleAll<Window>(this.position, 40f))
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
                IEnumerable<Block> blokz = Level.CheckCircleAll<Block>(position, range * 0.5f);
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
            }
        }
        public override void Update()
        {
            base.Update();
            if (!_pin)
            {
                _timer -= 0.01f;
                holsterable = false;
            }
            if (!_localDidExplode && _timer < 0f)
            {
                if (_explodeFrames < 0)
                {
                    CreateExplosion(position);
                    for (int i = 0; i < pieces; i++)
                    {
                        if (isServerForObject)
                        {
                            float dir = 360 / pieces + Rando.Float(10f);
                            BananaCluster c = new BananaCluster(position.x, position.y);
                            c.velocity = new Vec2((float)Math.Cos(dir), (float)Math.Sin(dir)) * 7;
                            Level.Add(c);
                        }
                    }
                    _explodeFrames = 4;
                }
                else
                {
                    _explodeFrames--;
                    if (_explodeFrames == 0)
                    {
                        CreateBlockExplosion();
                        _destroyed = true;
                        _explodeFrames = -1;
                        Level.Remove(this);
                    }
                }
            }
            if (prevOwner != null && _cookThrower == null)
            {
                _cookThrower = (prevOwner as Duck);
                _cookTimeOnThrow = _timer;
            }
            _sprite.frame = (_pin ? 0 : 1);
        }

        public override void OnSolidImpact(MaterialThing with, ImpactedFrom from)
        {
            SFX.Play(GetPath("SFX/BananaBombImpact_02.wav"), 1f, 0f, 0f, false);
            if (pullOnImpact)
            {
                OnPressAction();
            }
            base.OnSolidImpact(with, from);
        }
        public override void OnPressAction()
        {
            if (_pin)
            {
                _pin = false;
                Level.Add(new GrenadePin(x, y)
                {
                    hSpeed = (float)(-(float)offDir) * (1.5f + Rando.Float(0.5f)),
                    vSpeed = -2f
                });
                if (duck != null)
                {
                    RumbleManager.AddRumbleEvent(base.duck.profile, new RumbleEvent(this._fireRumble, RumbleDuration.Pulse, RumbleFalloff.None, RumbleType.Gameplay));
                }
                SFX.Play(GetPath("SFX/Vsegovno.wav"), 1f, 0f, 0f, false);
            }
        }
    }

    public class BananaCluster : PhysicsObject
    {
        public int lvl = 2;
        public SpriteMap _sprite;
        public float Range = 16;
        public float cooldown = 3f;
        public int pieces = 5;
        public float range = 16;

        public int bounces;

        public BananaCluster(float xval, float yval) : base(xval, yval)
        {
            _sprite = new SpriteMap(GetPath("Sprites/BananaBomb.png"), 16, 16);
            _sprite.CenterOrigin();
            graphic = _sprite;
            center = _sprite.center;
            _sprite.frame = Rando.Int(3);
            center = new Vec2(7f, 8f);
            collisionOffset = new Vec2(-4f, -5f);
            collisionSize = new Vec2(8f, 10f);
            bouncy = 0.7f;
            _sprite.scale = new Vec2(2, 2);
            physicsMaterial = PhysicsMaterial.Metal;
        }

        public override void Update()
        {
            if (cooldown > 0)
            {
                cooldown -= 0.1f;
            }
            base.Update();

            if(bounces > 60)
            {
                Explode();
            }
        }

        public void Explode()
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
                shrap.range = Range + Rando.Float(Range * 0.2f);
                Vec2 shrapDir = new Vec2((float)Math.Cos(Maths.DegToRad(dir)), (float)Math.Sin(Maths.DegToRad(dir)));
                Bullet bullet = new Bullet(x + shrapDir.x * 8f, y - shrapDir.y * 8f, shrap, dir, null, false, -1f, false, true);
                bullet.firedFrom = this;
                firedBullets.Add(bullet);
                Level.Add(bullet);
                Level.Add(Spark.New(x + Rando.Float(-8f, 8f), y + Rando.Float(-8f, 8f), shrapDir + new Vec2(Rando.Float(-0.1f, 0.1f), Rando.Float(-0.1f, 0.1f)), 0.02f));
                Level.Add(SmallSmoke.New(x + shrapDir.x * 8f + Rando.Float(-8f, 8f), y + shrapDir.y * 8f + Rando.Float(-8f, 8f)));
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
                    Fondle(w, DuckNetwork.localConnection);
                }
                if (Level.CheckLine<Block>(position, w.position, w) == null)
                {
                    w.Destroy(new DTImpact(this));
                }
            }
            IEnumerable<PhysicsObject> phys = Level.CheckCircleAll<PhysicsObject>(position, range);
            foreach (PhysicsObject p in phys)
            {
                if (isLocal && owner == null)
                {
                    Fondle(p, DuckNetwork.localConnection);
                }
                if ((p.position - position).length < 30f)
                {
                    p.Destroy(new DTImpact(this));
                }
                p.sleeping = false;
                p.vSpeed = -2f;
            }
            if (lvl >= 0)
            {
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
            }


            if (lvl > 0)
            {
                for (int i = 0; i < pieces; i++)
                {

                    if (isServerForObject)
                    {
                        float dir = 360 / pieces + Rando.Float(10f);
                        BananaCluster c = new BananaCluster(position.x, position.y);
                        c.velocity = new Vec2((float)Math.Cos(dir), (float)Math.Sin(dir)) * 7;
                        c.lvl = lvl - 1;
                        Level.Add(c);
                    }
                }
            }

            Level.Remove(this);
        }

        public override void Touch(MaterialThing with)
        {
            if (with != null)
            {
                bounces++;
                SFX.Play(GetPath("SFX/BananaBombImpact_02.wav"), 1f, 0f, 0f, false);
                if (!(with is Holdable) && cooldown <= 0)
                {
                    Explode();
                }
            }
            base.Touch(with);
        }
    }
}
