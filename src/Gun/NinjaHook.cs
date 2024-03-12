using System;

namespace DuckGame.WMD
{
    [EditorGroup("Faecterr's|Equipment")]
    [BaggedProperty("previewPriority", true)]
    public class NinjaGrapple : Holdable, ISwing
    {
        public StateBinding _ropeDataBinding = new DataBinding("ropeData");
        public BitBuffer ropeData = new BitBuffer(true);
        protected SpriteMap _sprite;
        public Harpoon _harpoon;
        public Rope _rope;
        protected Vec2 _barrelOffsetTL;
        private float _grappleLength = 160f;
        private Tex2D _laserTex;
        public Sprite _ropeSprite;
        protected Vec2 _wallPoint;
        private Vec2 _lastHit = Vec2.Zero;
        protected Vec2 _grappleTravel;
        protected Sprite _sightHit;
        private float _grappleDist;
        private bool _canGrab;
        private int _lagFrames;

        public Duck duck;

        public int uses = 5;

        public Vec2 barrelPosition { get { return Offset(barrelOffset);}}
        
        public Vec2 barrelOffset {get { return _barrelOffsetTL - center;}}
        
        public bool hookInGun { get { return _harpoon.inGun;}}
        
        public NinjaGrapple(float xpos, float ypos) : base(xpos, ypos)
        {
            _sprite = new SpriteMap(GetPath("Sprites/grappleArm.png"), 16, 16, false);
            graphic = _sprite;
            center = new Vec2(8f, 8f);
            collisionOffset = new Vec2(-5f, -4f);
            collisionSize = new Vec2(11f, 7f);
            _equippedDepth = 12;
            _barrelOffsetTL = new Vec2(10f, 4f);
            thickness = 0.1f;
            dontCrush = true;
        }
        
        public override void OnTeleport()
        {
            Degrapple();
        }
        
        public override void OnPressAction()
        {
        }
        
        public override void OnReleaseAction()
        {
        }
        
        public override void Initialize()
        {
            base.Initialize();
            _harpoon = new Harpoon(this);
            Level.Add(_harpoon);
            _sightHit = new Sprite("laserSightHit", 0f, 0f);
            _sightHit.CenterOrigin();
            _ropeSprite = new Sprite(GetPath("Sprites/grappleWire.png"), 0f, 0f);
            _ropeSprite.center = new Vec2(8f, 0f);
        }
        
        public Rope GetRopeParent(Thing child)
        {
            for (Rope t = _rope; t != null; t = (t.attach2 as Rope))
            {
                if (t.attach2 == child)
                {
                    return t;
                }
            }
            return null;
        }
        
        public void SerializeRope(Rope r)
        {
            if (r != null)
            {
                ropeData.Write(true);
                ropeData.Write(CompressedVec2Binding.GetCompressedVec2(r.attach2Point, int.MaxValue));
                SerializeRope(r.attach2 as Rope);
                return;
            }
            ropeData.Write(false);
        }
        
        public void DeserializeRope(Rope r)
        {
            if (ropeData.ReadBool())
            {
                if (r == null)
                {
                    _rope = new Rope(0f, 0f, r, null, null, false, _ropeSprite, this);
                    r = _rope;
                }
                r.attach1 = r;
                r._thing = null;
                Level.Add(r);
                Vec2 pos = CompressedVec2Binding.GetUncompressedVec2(ropeData.ReadInt(), int.MaxValue);
                if (r == _rope)
                {
                    r.attach1 = r;
                    if (duck != null)
                    {
                        r.position = duck.position;
                    }
                    else
                    {
                        r.position = position;
                    }
                    r._thing = duck;
                }
                if (r.attach2 == null || !(r.attach2 is Rope) || r.attach2 == r)
                {
                    Rope nextRope = new Rope(pos.x, pos.y, r, null, null, false, _ropeSprite, this);
                    r.attach2 = nextRope;
                }
                if (r.attach2 != null)
                {
                    r.attach2.position = pos;
                    (r.attach2 as Rope).attach1 = r;
                }
                DeserializeRope(r.attach2 as Rope);
                return;
            }
            if (r == _rope)
            {
                Degrapple();
                return;
            }
            if (r != null)
            {
                Rope rope = r.attach1 as Rope;
                rope.TerminateLaterRopes();
                _harpoon.Latch(r.position);
                rope.attach2 = _harpoon;
            }
        }
        
        public void Degrapple()
        {
            _harpoon.Return();
            if (_rope != null)
            {
                _rope.RemoveRope();
            }
            if (_rope != null && duck != null)
            {
                base.duck.frictionMult = 1f;
                base.duck.gravMultiplier = 1f;
                base.duck._double = false;
                if (base.duck.vSpeed < 0f && base.duck.framesSinceJump > 3)
                {
                    base.duck.vSpeed *= 1.75f;
                }
            }
            _rope = null;
            frictionMult = 1f;
            gravMultiplier = 1f;
        }
        
        public Vec2 wallPoint
        {
            get
            {
                return _wallPoint;
            }
        }
        
        public Vec2 grappelTravel
        {
            get
            {
                return _grappleTravel;
            }
        }
        
        public override void Update()
        {
            if(owner == null && _prevOwner != null)
            {
                (_prevOwner as Duck).frictionMult = 1f;
                (_prevOwner as Duck).gravMultiplier = 1f;
                (_prevOwner as Duck)._double = false;
                if ((_prevOwner as Duck).vSpeed < 0f && (_prevOwner as Duck).framesSinceJump > 3)
                {
                    (_prevOwner as Duck).vSpeed *= 1.75f;
                }
                (_prevOwner as Duck).hMax = 3.1f;
                _prevOwner = null;
            }
            if(uses <= 0 && duck == null)
            {
                Degrapple();
                Level.Remove(this);
            }
            if (_harpoon == null)
            {
                return;
            }
            if (isServerForObject)
            {
                ropeData.Clear();
                SerializeRope(_rope);
            }
            else
            {
                ropeData.SeekToStart();
                DeserializeRope(_rope);
            }

            if(owner != null)
            {
                if(owner is Duck)
                {
                    duck = owner as Duck;
                }
                else
                {
                    duck = null;
                }
            }
            else
            {
                duck = null;
            }

            if (_rope != null)
            {
                _rope.SetServer(isServerForObject);
            }
            if (isServerForObject && duck != null)
            {
                if (duck._trapped != null)
                {
                    Degrapple();
                }
                ATTracer tracer = new ATTracer();
                float dist = tracer.range = _grappleLength;
                tracer.penetration = 1f;
                float a = -angleDegrees;
                if (offDir < 0)
                {
                    a = 180 - angleDegrees;
                }
                if (_harpoon.inGun)
                {
                    Vec2 pos = Offset(barrelOffset);
                    if (_lagFrames > 0)
                    {
                        _lagFrames--;
                        if (_lagFrames == 0)
                        {
                            _canGrab = false;
                        }
                        else
                        {
                            a = Maths.PointDirection(pos, _lastHit);
                        }
                    }
                    tracer.penetration = 9f;
                    Bullet b = new Bullet(pos.x, pos.y, tracer, a, owner, false, -1f, true, true);
                    _wallPoint = b.end;
                    _grappleTravel = b.travelDirNormalized;
                    dist = (pos - _wallPoint).length;
                }
                if (dist < _grappleLength - 2f && dist <= _grappleDist + 16f)
                {
                    _lastHit = _wallPoint;
                    _canGrab = true;
                }
                else if (_canGrab && _lagFrames == 0)
                {
                    _lagFrames = 6;
                    _wallPoint = _lastHit;
                }
                else
                {
                    _canGrab = false;
                }
                _grappleDist = dist;
                if (duck.inputProfile.Pressed("SHOOT", false) && base.duck._trapped == null)
                {
                    if (_harpoon.inGun && uses > 0)
                    {
                        if (!base.duck.grounded && base.duck.framesSinceJump > 6 && _canGrab)
                        {
                            uses--;
                            RumbleManager.AddRumbleEvent(base.duck.profile, new RumbleEvent(RumbleIntensity.Kick, RumbleDuration.Pulse, RumbleFalloff.Short, RumbleType.Gameplay));
                            _harpoon.Fire(wallPoint, grappelTravel);
                            _rope = new Rope(barrelPosition.x, barrelPosition.y, null, _harpoon, base.duck, false, _ropeSprite, this);
                            Level.Add(_rope);
                            SFX.Play(GetPath("SFX/NinjaRopeFire.wav"), 1f, 0f, 0f, false);
                            SFX.Play(GetPath("SFX/NinjaRopeImpact_01.wav"), 1f, 0f, 0f, false);
                            DuckNetwork.SendToEveryone(new NMNinjaHookCount(this, uses));
                        }
                    }
                    else
                    {
                        Degrapple();
                        _lagFrames = 0;
                        _canGrab = false;
                    }
                }
            }
            base.Update();
            if (owner != null)
            {
                offDir = owner.offDir;
            }
            if (base.duck != null)
            {
                base.duck.grappleMul = false;
            }
            if (isServerForObject && _rope != null)
            {
                if (owner != null)
                {
                    _rope.position = owner.position;
                }
                else
                {
                    _rope.position = position;
                    if (prevOwner != null)
                    {
                        PhysicsObject physicsObject = prevOwner as PhysicsObject;
                        physicsObject.frictionMult = 1f;
                        physicsObject.gravMultiplier = 1f;
                        _prevOwner = null;
                        frictionMult = 1f;
                        gravMultiplier = 1f;
                        if (prevOwner is Duck)
                        {
                            (prevOwner as Duck).grappleMul = false;
                        }
                    }
                }
                if (_harpoon.stuck)
                {
                    if (base.duck != null)
                    {
                        if (!base.duck.grounded)
                        {
                            base.duck.frictionMult = 0f;
                        }
                        else
                        {
                            base.duck.frictionMult = 1f;
                            base.duck.gravMultiplier = 1f;
                        }
                        if (_rope.properLength > 0f)
                        {
                            if (duck.inputProfile.Down("UP") && _rope.properLength >= 16f)
                            {
                                _rope.properLength -= 4f;
                            }
                            if (base.duck.inputProfile.Down("DOWN") && _rope.properLength <= _grappleLength)
                            {
                                _rope.properLength += 4f;
                            }
                            _rope.properLength = Maths.Clamp(_rope.properLength, 16f, _grappleLength);
                        }

                        if(Rando.Float(1) > 0.99f)
                        {
                            int i = Rando.Int(2);
                            if(i == 0)
                            {
                                SFX.Play(GetPath("SFX/NinjaRopeStress_01.wav"), 1f, 0f, 0f, false);
                            }
                            else
                            {
                                SFX.Play(GetPath("SFX/NinjaRopeStress_02.wav"), 1f, 0f, 0f, false);
                            }
                        }
                    }
                    else if (!grounded)
                    {
                        frictionMult = 0f;
                    }
                    else
                    {
                        frictionMult = 1f;
                        gravMultiplier = 1f;
                    }
                    Vec2 travel = _rope.attach1.position - _rope.attach2.position;
                    if (_rope.properLength < 0f)
                    {
                        _rope.properLength = (_rope.startLength = travel.length);
                    }
                    if (travel.length > _rope.properLength)
                    {
                        travel = travel.normalized;
                        if (base.duck != null)
                        {
                            base.duck.grappleMul = true;
                            PhysicsObject attach = base.duck;
                            duck.framesSinceJump = 2;
                            duck.grounded = false;
                            if (base.duck.ragdoll != null)
                            {
                                Degrapple();
                                return;
                            }
                            Vec2 position = attach.position;
                            attach.position = _rope.attach2.position + travel * _rope.properLength;
                            Vec2 dif = attach.position - attach.lastPosition;
                            attach.hSpeed = dif.x;
                            attach.vSpeed = dif.y;
                            return;
                        }
                        else
                        {
                            Vec2 position2 = position;
                            position = _rope.attach2.position + travel * _rope.properLength;
                            Vec2 dif2 = position - lastPosition;
                            hSpeed = dif2.x;
                            vSpeed = dif2.y;
                        }
                    }
                }
            }
        }
        
        public override void Draw()
        {
            /*string text = "";
            float a = -angleDegrees;
            if (offDir < 0)
            {
                a = 180 - angleDegrees;
            }
            text += Convert.ToString(a);
            Graphics.DrawString(text, position, Color.White, 1f, null, 2f);*/
            if(duck != null)
            {
                Graphics.DrawStringOutline(Convert.ToString(uses), duck.position + new Vec2(-3, -14), Color.LightBlue, Color.Black, 1f, null, 1f);
            }
            base.Draw();
        }
    }
}
