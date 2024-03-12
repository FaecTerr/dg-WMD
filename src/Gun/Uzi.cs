using System;

namespace DuckGame.WMD
{
    [EditorGroup("Faecterr's|Guns")]
    public class Uzi : Gun
    {
        public StateBinding _burstingBinding = new StateBinding("_bursting", -1, false, false);
        public StateBinding _burstNumBinding = new StateBinding("_burstNum", -1, false, false);
        public float _burstWait;
        public bool _bursting;
        public int _burstNum;
        public static bool inFire;

        public Uzi(float xval, float yval) : base(xval, yval)
        {
            ammo = 25;
            _ammoType = new ATPewPew();
            _type = "gun";
            graphic = new Sprite(GetPath("Sprites/Uzi.png"), 0f, 0f);
            center = new Vec2(8f, 8f);
            collisionOffset = new Vec2(-8f, -3f);
            collisionSize = new Vec2(16f, 7f);
            _barrelOffsetTL = new Vec2(16f, 7f);
            _fireSound = "";
            _fullAuto = true;
            _fireWait = 1.5f;
            _kickForce = 1f;
            _fireRumble = RumbleIntensity.Kick;
            _holdOffset = new Vec2(0f, 0f);
            _flare = new SpriteMap("laserFlare", 16, 16, false);
            _flare.center = new Vec2(0f, 8f);
            editorTooltip = "Quick-fire semi-machine gun of ULTIMATE DESTRUCTION... with an adorable wittle name.";
        }
        
        public override void Update()
        {
            if (_bursting)
            {
                _burstWait = Maths.CountDown(_burstWait, 0.16f, 0f);
                if (_burstWait <= 0f)
                {
                    if (ammo % 8 == 0 && ammo > 0)
                    {
                        SFX.Play(GetPath("SFX/UziFireAlt.wav"), 1f, 0f, 0f, false);
                    }
                    _burstWait = 1.6f;
                    if (isServerForObject)
                    {
                        inFire = true;
                        Fire();
                        inFire = false;
                        if (Network.isActive)
                        {
                            Send.Message(new NMFireGun(this, firedBullets, bulletFireIndex, false, (duck != null) ? duck.netProfileIndex : (byte)4, true), NetMessagePriority.Urgent);
                        }
                        firedBullets.Clear();
                    }
                    _wait = 0f;
                    _burstNum++;
                }
                if(owner == null)
                {
                    Level.Remove(this);
                }
            }
            if(ammo <= 0)
            {
                _bursting = false;
            }
            base.Update();
        }
        
        public override void OnPressAction()
        {
            if (receivingPress && hasFireEvents && onlyFireAction)
            {
                PewPewLaser.inFire = true;
                Fire();
                PewPewLaser.inFire = false;
            }
            if (!_bursting && _wait == 0f)
            {
                _bursting = true;
            }
 
        }
        
        public override void OnHoldAction()
        {
        }
    }
}
