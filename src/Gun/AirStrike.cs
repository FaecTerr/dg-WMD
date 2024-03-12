using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckGame.WMD
{
    [BaggedProperty("isFatal", false)]
    [EditorGroup("Faecterr's|Weapons|Remote")]
    public class AirStrike : Holdable, IDrawToDifferentLayers
    {
        public SpriteMap _sprite;
        public SpriteMap _target;
        public Vec2 pos;
        public bool charging;
        public List<Bullet> firedBullets = new List<Bullet>();
        public int amount = 5;
        public float wide = 120;

        public bool boosted;

        public AirStrike(float xpos, float ypos) : base(xpos, ypos)
        {
            _sprite = new SpriteMap(GetPath("Sprites/AirStrikeController.png"), 10, 14, false);
            _target = new SpriteMap(GetPath("Sprites/AirStrikeAim.png"), 17, 17, false);
            _target.CenterOrigin();
            graphic = _sprite;
            center = new Vec2(5f, 7f);
            collisionOffset = new Vec2(-4f, -6f);
            collisionSize = new Vec2(8f, 12f);
            _target.frame = 2;
            dontCrush = true;

            _editorName = "Air Strike";
            editorTooltip = "Choose your target and see how bombs flying of sky.";
        }
        public virtual void Explosion()
        {
            if (isServerForObject && owner != null)
            {
                for (int i = 0; i < amount; i++)
                {
                    float spawnY = Math.Min(Level.current.camera.position.y - 580, Level.current.topLeft.y - 580) + 10 * i;
                    float spawnX = pos.x - (wide/2 - (wide/amount) * i) * (pos.x > owner.x ? 1 : -1);
                    Level.Add(new MissileBomb(spawnX, spawnY) { move = pos.x > owner.x ? 0.2f : -0.2f, boosted = boosted });
                    if(pos.x > owner.x)
                    {
                        SFX.Play(GetPath("SFX/AirplanePassBy_LR.wav"), 1f, 0f, 0f, false);
                    }
                    else
                    {
                        SFX.Play(GetPath("SFX/AirplanePassBy_RL.wav"), 1f, 0f, 0f, false);
                    }
                }
            }
            Level.Remove(this);
        }

        public virtual void Boost()
        {
            boosted = true;
            _sprite.frame = 1;
        }

        public override void Update()
        {
            if (prevOwner != null && owner == null)
            {
                if (_prevOwner is Duck)
                {
                    (_prevOwner as Duck).immobilized = false;
                }
                _prevOwner = null;
            }

            base.Update();
            if(owner != null)
            {
                Duck d = owner as Duck;
                if (d.profile.inputProfile.Released("SHOOT"))
                {
                    d.immobilized = false;
                    Explosion();
                }
                if (d.profile.inputProfile.Down("SHOOT"))
                {
                    d.immobilized = true;
                    if(charging == false)
                    {
                        charging = true;
                        pos = position;
                        SFX.Play(GetPath("SFX/AirstrikeAccept.wav"), 1f, 0f, 0f, false);
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
                        pos += move*2;
                        move = new Vec2(0f, 0f);
                    }
                }
            }
        }

        public void OnDrawLayer(Layer layer)
        {
            if (layer == Layer.Foreground)
            {
                if (charging == true)
                {
                    Graphics.Draw(_target, pos.x, pos.y);
                }
            }
        }
    }

    public class MissileBomb : PhysicsObject
    {
        public float delay = 9;
        public SpriteMap _sprite;
        public float move;

        public bool boosted;

        public MissileBomb(float xval, float yval) : base(xval, yval)
        {
            _sprite = new SpriteMap(GetPath("Sprites/Missiles.png"), 11, 21);
            _sprite.CenterOrigin();
            graphic = _sprite;
            center = _sprite.center;
            _sprite.frame = 0;
            collisionOffset = new Vec2(-3f, -5f);
            collisionSize = new Vec2(6f, 10f);
            _enablePhysics = false;
            physicsMaterial = PhysicsMaterial.Metal;
        }

        public override void Initialize()
        {
            if (boosted)
            {
                _sprite = new SpriteMap(GetPath("Sprites/MissilesBUP.png"), 11, 21);
                graphic = _sprite;
            }
            base.Initialize();
        }

        public override void Update()
        {
            if (grounded && delay <= 0)
            {
                Explode();
            }

            if (delay <= 0)
            {
                if (velocity.y > 6)
                {
                    velocity = new Vec2(velocity.x, 6);
                }
                if (Level.current.camera != null)
                {
                    FollowCam c = Level.current.camera as FollowCam;
                    if (!c.Contains(this) /*&& position.y < Level.current.topLeft.y - Level.current.camera.size.y*/)
                    {
                        c.Add(this);
                    }
                }
                if (hSpeed != 0)
                {
                    angle = MathHelper.Lerp(angle, (float)Math.Atan(vSpeed / hSpeed * offDir) + 1.57f * offDir, 0.1f);
                }
                else
                {
                    angle = 1.57f + 1.57f * offDir;
                }
            }
            else
            {
                if(Math.Round(delay, 1) == 1.5f)
                {
                    SFX.Play(GetPath("SFX/AirplaneHatch.wav"), 1f, 0f, 0f, false);
                }
                delay -= 0.1f;
                if(delay <= 0)
                {
                    _enablePhysics = true;
                    SFX.Play(GetPath("SFX/AirStrikeWhistle.wav"), 1f, 0f, 0f, false);
                }
            }
            _skipAutoPlatforms = true;
            _skipPlatforms = true;
            base.Update();
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
            foreach (Platform p in Level.CheckCircleAll<Platform>(position, 28f))
            {
                Level.Remove(p);
            }
            foreach (AutoPlatform p in Level.CheckCircleAll<AutoPlatform>(this.position, 28f))
            {
                Level.Remove(p);
            }
            if (Network.isActive && isLocal && idx.Count > 0)
            {
                Send.Message(new NMDestroyBlocks(idx));
            }

            if (boosted)
            {
                for (int i = 0; i < 10; i++)
                {
                    SmallFire s = SmallFire.New(position.x, position.y, (5 - i) * 0.75f, -2 - Math.Abs(5 - i) * 0.35f, false);
                    Level.Add(s);
                }
            }

            Level.Remove(this);
        }
        public override void Touch(MaterialThing with)
        {
            if (with != null)
            {
                if (!(with is Holdable) && !(with is MissileBomb))
                {
                    Explode();
                }
            }
            base.Touch(with);
        }
    }
}
