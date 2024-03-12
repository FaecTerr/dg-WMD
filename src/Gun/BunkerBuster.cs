using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckGame.WMD
{
    [BaggedProperty("isFatal", false)]
    [EditorGroup("Faecterr's|Weapons|Remote")]
    public class BunkerBuster : Holdable, IDrawToDifferentLayers
    {
        public SpriteMap _sprite;
        public SpriteMap _target;
        public Vec2 pos;
        public bool charging;
        public List<Bullet> firedBullets = new List<Bullet>();

        public int hold;

        public bool boosted;
        public bool powered;

        public BunkerBuster(float xpos, float ypos) : base(xpos, ypos)
        {
            dontCrush = true;
            _sprite = new SpriteMap(GetPath("Sprites/BunkerBuster.png"), 10, 14, false);
            _target = new SpriteMap(GetPath("Sprites/AirStrikeAim.png"), 17, 17, false);
            _target.CenterOrigin();
            graphic = _sprite;
            center = new Vec2(5f, 7f);
            collisionOffset = new Vec2(-4f, -6f);
            collisionSize = new Vec2(8f, 12f);
            
            _editorName = "Bunker buster";
            editorTooltip = "Reach your enemies even when they are underground.";
        }
        public override void Terminate()
        {
            if (owner != null && owner is Duck)
            {
                (owner as Duck).immobilized = false;
            }
            if(prevOwner != null && prevOwner is Duck && owner == null)
            {
                (prevOwner as Duck).immobilized = true;
            }
            base.Terminate();
        }
        public virtual void Explosion()
        {
            if (isServerForObject && owner != null)
            {
                float spawnY = Math.Min(Level.current.camera.position.y - 80, Level.current.topLeft.y - 80);
                float spawnX = pos.x;
                Level.Add(new BBBomb(spawnX, spawnY) { boosted = boosted, powered = powered});

                if (boosted)
                {
                    Level.Add(new BBBomb(spawnX + 32, spawnY) { boosted = boosted, powered = powered });
                    Level.Add(new BBBomb(spawnX - 32, spawnY) { boosted = boosted, powered = powered });
                }

                if (pos.x > owner.x)
                {
                    SFX.Play(GetPath("SFX/AirplanePassBy_LR.wav"), 1f, 0f, 0f, false);
                }
                else
                {
                    SFX.Play(GetPath("SFX/AirplanePassBy_RL.wav"), 1f, 0f, 0f, false);
                }
            }
            if(owner is Duck)
            {
                (owner as Duck).immobilized = false;
            }
            Level.Remove(this);
        }

        public virtual void Boost()
        {
            boosted = true;
            powered = false;
            _sprite.frame = 1;
            //_sprite = new SpriteMap(GetPath("Sprites/MissilesBUP.png"), 9, 25);
            //graphic = _sprite;
        }

        public virtual void PowerUp()
        {
            powered = true;
            boosted = false;
            _sprite.frame = 2;
            //_sprite = new SpriteMap(GetPath("Sprites/MissilesPUP.png"), 9, 25);
            //graphic = _sprite;
        }


        public override void Update()
        {
            base.Update();
            if (owner != null)
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
                    if (charging == false)
                    {
                        charging = true;
                        pos = position;
                    }
                    else
                    {
                        Vec2 move = new Vec2();
                        hold += 1;
                        if (hold > 15 * 6)
                        {
                            hold = 0;
                        }
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
                        pos += move * 1.2f * (Level.current.camera.size.x / 320);
                        move = new Vec2(0f, 0f);
                    }
                }
            }
            else if (prevOwner != null)
            {
                Duck d = prevOwner as Duck;
                if (d.holdObject == null)
                {
                    d.immobilized = false;
                }
            }

            if(pos.x > Level.current.camera.position.x + Level.current.camera.size.x)
            {
                pos.x = Level.current.camera.position.x + Level.current.camera.size.x;
            }
            if (pos.x < Level.current.camera.position.x)
            {
                pos.x = Level.current.camera.position.x;
            }
        }

        public void OnDrawLayer(Layer layer)
        {
            if (layer == Layer.Foreground)
            {
                if (charging == true)
                {
                    for (int i = (int)Level.current.camera.position.y; i < (int)(Level.current.camera.position.y + Level.current.camera.size.y) + 12; i += 12)
                    {
                        SpriteMap _beam = new SpriteMap(GetPath("Sprites/BunkerBusterBeam.png"), 10, 12);
                        _beam.frame = (int)(hold / 15);
                        _beam.alpha = 0.5f;
                        _beam.CenterOrigin();
                        Graphics.Draw(_beam, pos.x, i);
                    }
                    if (boosted)
                    {
                        for (int i = (int)Level.current.camera.position.y; i < (int)(Level.current.camera.position.y + Level.current.camera.size.y) + 12; i += 12)
                        {
                            SpriteMap _beam = new SpriteMap(GetPath("Sprites/BunkerBusterBeam.png"), 10, 12);
                            _beam.frame = (int)(hold / 15);
                            _beam.alpha = 0.5f;
                            _beam.CenterOrigin();
                            Graphics.Draw(_beam, pos.x - 32, i);
                        }

                        for (int i = (int)Level.current.camera.position.y; i < (int)(Level.current.camera.position.y + Level.current.camera.size.y) + 12; i += 12)
                        {
                            SpriteMap _beam = new SpriteMap(GetPath("Sprites/BunkerBusterBeam.png"), 10, 12);
                            _beam.frame = (int)(hold / 15);
                            _beam.alpha = 0.5f;
                            _beam.CenterOrigin();
                            Graphics.Draw(_beam, pos.x + 32, i);
                        }
                    }

                    Graphics.Draw(_target, pos.x, pos.y);
                    _target.angleDegrees += 1;
                }
            }
        }
    }

    public class BBBomb : PhysicsObject
    {
        public SpriteMap _sprite;
        public int deepness = 200;
        public float KillTIme = 0.1f * 60 * 8; //8 seconds until forced explosion
        public float delay = 9;
        public float digTime = 0.1f * 60 * 1;

        public bool boosted;
        public bool powered;

        public bool startDig;

        public float rangeMod = 1;

        public BBBomb(float xval, float yval) : base(xval, yval)
        {
            _sprite = new SpriteMap(GetPath("Sprites/Missiles.png"), 11, 21);
            _sprite.CenterOrigin();
            graphic = _sprite;
            center = _sprite.center;
            _sprite.frame = 2;

            collisionOffset = new Vec2(0f, 0f);
            collisionSize = new Vec2(0f, 0f);
            
            enablePhysics = false;

            weight = 10;
            physicsMaterial = PhysicsMaterial.Metal;
        }

        public override void Initialize()
        {
            base.Initialize();
            if (boosted)
            {
                _sprite = new SpriteMap(GetPath("Sprites/MissilesBUP.png"), 11, 21);
                _sprite.frame = 2;
            }
            if (powered)
            {
                rangeMod = 3f;
                digTime = 0.1f * 60 * 2;
                _sprite = new SpriteMap(GetPath("Sprites/MissilesPUP.png"), 11, 21);
                _sprite.frame = 2;
            }
            graphic = _sprite;
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

            if (delay <= 0)
            {
                foreach (MaterialThing t in Level.CheckRectAll<MaterialThing>(topLeft + new Vec2(-16, -16), bottomRight + new Vec2(16, 16)))
                {
                    clip.Add(t);
                }
                alpha = 1;
                _sleeping = false;
                if (Level.current.camera != null)
                {
                    FollowCam c = Level.current.camera as FollowCam;
                    if (!c.Contains(this) /*&& position.y < Level.current.topLeft.y - Level.current.camera.size.y*/)
                    {
                        c.Add(this);
                    }
                }
                if (grounded)
                {
                    Dig();
                }

                if(position.y > Level.current.bottomRight.y - 32)
                {
                    Explode();
                }

                KillTIme -= 0.1f;

                if (KillTIme <= 0)
                {
                    Explode();
                }
                Dig();

                if (startDig)
                {
                    digTime -= 0.1f;
                }
                if(digTime <= 0)
                {
                    Explode();
                }

                deepness = 9999;

                if (hSpeed != 0)
                {
                    angle = MathHelper.Lerp(angle, (float)Math.Atan(vSpeed / hSpeed), 0.1f);
                }
                else
                {
                    angle = 1.57f + 1.57f * offDir;
                }
                angle %= 3.14f * 2;
                
                if (position.y > Level.current.bottomRight.y + 200)
                {
                    Level.Remove(this);
                }
            }
            else
            {
                if (Math.Round(delay, 1) == 1.5f)
                {
                    SFX.Play(GetPath("SFX/AirplaneHatch.wav"), 1f, 0f, 0f, false);
                }
                delay -= 0.1f;
                if (delay <= 0)
                {
                    if (powered)
                    {
                        rangeMod = 2;
                        digTime = 0.1f * 60 * 2;
                    }
                    SFX.Play(GetPath("SFX/BunkerBusterDrillLoop.wav"), 1f, 0f, 0f, false);
                    _enablePhysics = true;
                    SFX.Play(GetPath("SFX/BunkerBusterWhistle.wav"), 1f, 0f, 0f, false);
                }
            }
            _skipAutoPlatforms = true;
            _skipPlatforms = true;
            base.Update();
        }

        public virtual void Explode()
        {
            ATMissileShrapnel shrap = new ATMissileShrapnel();
            shrap.MakeNetEffect(position, false);
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
                shrap.range = 15f + Rando.Float(5f);
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
            IEnumerable<Window> windows = Level.CheckCircleAll<Window>(position, 30f * rangeMod);
            foreach (Window w in windows)
            {
                if (isLocal)
                {
                    Fondle(w, DuckNetwork.localConnection);
                }
                if (Level.CheckLine<Block>(position, w.position, w) == null)
                {
                    w.Destroy(new DTImpact(this));
                }
            }
            IEnumerable<PhysicsObject> phys = Level.CheckCircleAll<PhysicsObject>(position, 70f * rangeMod);
            foreach (PhysicsObject p in phys)
            {
                if (isLocal && owner == null && p != this)
                {
                    Fondle(p, DuckNetwork.localConnection);
                }
                if ((p.position - position).length < 30f * rangeMod)
                {
                    p.Destroy(new DTImpact(this));
                }
                p.sleeping = false;
                p.vSpeed = -2f;
            }
            HashSet<ushort> idx = new HashSet<ushort>();
            IEnumerable<BlockGroup> blokzGroup = Level.CheckCircleAll<BlockGroup>(position, 50f * rangeMod);
            foreach (BlockGroup block in blokzGroup)
            {
                if (block != null)
                {
                    BlockGroup group = block;
                    new List<Block>();
                    foreach (Block bl in group.blocks)
                    {
                        if (Collision.Circle(position, 28f * rangeMod, bl.rectangle))
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
            IEnumerable<Block> blokz = Level.CheckCircleAll<Block>(position, 28f * rangeMod);
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
            SFX.Play(GetPath("SFX/BunkerBusterExpl.wav"), 1f, 0f, 0f, false);
            Level.Remove(this);
        }

        public void Dig()
        {            
            HashSet<ushort> idx = new HashSet<ushort>();
            foreach (MaterialThing m in Level.CheckRectAll<MaterialThing>(topLeft, bottomRight + new Vec2(0, 6)))
            {
                if (m != this && !(m is Magnet))
                {
                    if (m is Duck)
                    {
                        Explode();
                    }
                    else
                    {
                        startDig = true;
                        if(m is Block)
                        {
                            //(m as Block).shouldWreck = true;
                            if(m is AutoBlock)
                            {
                                //idx.Add((m as AutoBlock).blockIndex);
                            }
                        }
                        if(m is BlockGroup)
                        {
                            //(m as BlockGroup).Wreck();
                        }
                        if(m is Platform)
                        {
                            //Level.Remove(m);
                        }
                        if(m is AutoPlatform)
                        {
                            //Level.Remove(m);
                        }
                        //digTime -= 0.1f;
                        //Level.Remove(m);
                    }
                }
            }
            /*HashSet<ushort> idx = new HashSet<ushort>();
            IEnumerable<BlockGroup> blokzGroup = Level.CheckCircleAll<BlockGroup>(position, 8f);
            foreach (BlockGroup block in blokzGroup)
            {
                if (block != null)
                {
                    BlockGroup group = block;
                    new List<Block>();
                    foreach (Block bl in group.blocks)
                    {
                        if (Collision.Circle(position, 3f, bl.rectangle) && deepness > 0)
                        {
                            deepness -= (int)bl.collisionSize.x;
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
            IEnumerable<Block> blokz = Level.CheckCircleAll<Block>(position, 8f);
            foreach (Block block2 in blokz)
            {
                if (block2 is AutoBlock && deepness > 0)
                {
                    deepness -= (int)block2.collisionSize.x;
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

            foreach (Platform p in Level.CheckCircleAll<Platform>(position, 8))
            {
                if (deepness > 0)
                {
                    deepness -= (int)p.collisionSize.x;
                    Level.Remove(p);
                }
            }
            foreach (AutoPlatform p in Level.CheckCircleAll<AutoPlatform>(position, 8))
            {
                if (deepness > 0)
                {
                    deepness -= (int)p.collisionSize.x;
                    Level.Remove(p);
                }
            }

            if (Network.isActive && isLocal && idx.Count > 0)
            {
                Send.Message(new NMDestroyBlocks(idx));
            }
            if (deepness <= 0)
            {
                //Explode();
            }*/
        }
    }
}
