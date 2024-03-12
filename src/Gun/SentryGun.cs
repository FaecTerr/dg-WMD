using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckGame.WMD
{
    [EditorGroup("Faecterr's|Weapons")]
    public class SentryGun : Holdable, IDrawToDifferentLayers
    {
        public Duck user;
        public float range = 112;

        public int ammo = 50;
        public int health = 100;

        private float unableToFireAfterLanding = 0.3f; //0.01f per frame
        private float cantFire;

        public SpriteMap _head;
        public SpriteMap _body;

        public PhysicsObject _target;
        public float gunAngle;
        public float _cooldown;
        public int _shotsFired;
        public bool _ramped;
        public bool Placed;

        public SinWave _waving = 0.05f;
    
        public List<Bullet> firedBullets = new List<Bullet>();

        private AmmoType ammoType;
        private Vec2 firePosition
        {
            get
            {
                return position + new Vec2(offDir * 2f, -6f) + (3f * new Vec2(offDir * 1f, 0f).Rotate(gunAngle, Vec2.Zero));
            }
        }

        public SentryGun(float xpos, float ypos) : base(xpos, ypos)
        {
            _head = new SpriteMap(GetPath("Sprites/SentryHead.png"), 16, 12);
            _body = new SpriteMap(GetPath("Sprites/SentryBase.png"), 16, 12);

            _head.center = new Vec2(4, 8);
            _body.CenterOrigin();

            center = new Vec2(8, 10);
            collisionOffset = new Vec2(-8, -10);
            collisionSize = new Vec2(16, 20);
            ammoType = new AT9mm();
            physicsMaterial = PhysicsMaterial.Metal;

            thickness = 0.1f;
            dontCrush = true;
        }

        PhysicsObject ValidateTarget()
        {
            PhysicsObject value = null;
            foreach (PhysicsObject physicsObject in Level.CheckCircleAll<PhysicsObject>(position, range))
            {
                if (physicsObject.velocity.length > 0.2f)
                {
                    IList<Block> blockers = new List<Block>();
                    foreach (Block b in Level.CheckLineAll<Block>(position, physicsObject.position))
                    {
                        if (!(b is Window))
                            blockers.Add(b);
                    }
                    if (physicsObject != this && physicsObject != user &&
                        (physicsObject is Duck ||
                        (physicsObject is RagdollPart && ((RagdollPart)physicsObject)._doll != null
                        && ((RagdollPart)physicsObject)._doll._duck != null && ((RagdollPart)physicsObject)._doll._duck != user && !((RagdollPart)physicsObject)._doll._duck.dead)
                        || physicsObject is TrappedDuck)
                        && blockers.Count == 0
                        && (_target == null || (position - physicsObject.position).length < (position - _target.position).length))
                    {
                        SFX.Play(GetPath("SFX/SentryActivate.wav"), 1f, 0f, 0f, false);
                        return physicsObject;
                    }
                }
            }
            return value;
        }

        public override void Update()
        {
            if (!grounded)
            {
                cantFire = unableToFireAfterLanding;
            }
            if(cantFire > 0)
            {
                cantFire -= 0.01f;
            }
            if (!Placed)
            {
                if(owner != null)
                {
                    if(owner is Duck)
                    {
                        if((owner as Duck).profile.inputProfile.Pressed("SHOOT"))
                        {
                            user = (owner as Duck);
                            (owner as Duck).doThrow = true;
                            canPickUp = false;
                            Placed = true;
                            DuckNetwork.SendToEveryone(new NMTurretReady(this, user));
                        }
                    }
                }
            }
            else if(user != null && owner == null && cantFire <= 0 && (Math.Abs(hSpeed) + Math.Abs(vSpeed)) < 0.1f)
            {
                float ang = angle + (offDir < 0 ? (float)Math.PI : 0f);

                if (_target == null && cantFire <= 0)
                {
                    _target = ValidateTarget();
                }
                if (_target != null && cantFire <= 0 && (Math.Abs(hSpeed) + Math.Abs(vSpeed)) < 0.1f)
                {
                    IList<Block> blockers = new List<Block>();
                    foreach (Block b in Level.CheckLineAll<Block>(position, _target.position))
                    {
                        if (!(b is Window))
                            blockers.Add(b);
                    }
                    if (!Level.CheckCircleAll<PhysicsObject>(position, range).Contains(_target) || blockers.Count > 0)
                    {
                        if(_target != null)
                        {
                            SFX.Play(GetPath("SFX/SentryDeactivate.wav"), 1f, 0f, 0f, false);
                        }
                        _target = null;

                    }
                    else
                    {
                        gunAngle = MathHelper.Lerp(gunAngle, (float)Math.Atan((_target.y - firePosition.y) / (_target.x - firePosition.x)), 0.1f);
                        
                        if (ammo > 0 && _cooldown <= 0)
                        {
                            _ramped = true;
                            if (isServerForObject)
                            {
                                if (_target.x < position.x)
                                {
                                    offDir = -1;
                                }
                                else
                                {
                                    offDir = 1;
                                }
                                Fire();
                                Send.Message(new NMFireGun(null, firedBullets, 0, false, 4, true), NetMessagePriority.Urgent);
                                firedBullets.Clear();
                            }
                            ammo--;
                            _cooldown = 0.8f;
                            if (ammo <= 0)
                            {
                                _cooldown = 9f; //0.01f per frame
                            }
                        }
                        else
                        {
                            if (_cooldown > 0)
                            {
                                _cooldown -= 0.1f;
                                if (ammo <= 0 && _cooldown <= 0)
                                {
                                    ammo = 50;
                                }
                            }
                        }
                    }
                }
                else
                {
                    angle = _waving * 1f;
                    gunAngle = MathHelper.Lerp(gunAngle, angle, 0.1f);
                    _cooldown = 0f;
                    _shotsFired = 0;
                    _ramped = false;
                }
            }

            base.Update();
        }

        private void Fire()
        {
            firedBullets.Clear();
            float fireAngleDegrees = Maths.RadToDeg(gunAngle);
            float fireAngle = offDir >= 0 ? fireAngleDegrees + ammoType.barrelAngleDegrees : fireAngleDegrees + 180f - ammoType.barrelAngleDegrees;
            Bullet bullet = ammoType.FireBullet(firePosition, owner, fireAngle, this);
            if (Network.isActive && isServerForObject)
            {
                firedBullets.Add(bullet);
            }
            int sound = Rando.Int(3);
            if (sound == 0)
            {
                SFX.Play(GetPath("SFX/SentryFireLoop_01.wav"), 1f, 0f, 0f, false);
            }
            if (sound == 1)
            {
                SFX.Play(GetPath("SFX/SentryFireLoop_02.wav"), 1f, 0f, 0f, false);
            }
            if (sound == 2)
            {
                SFX.Play(GetPath("SFX/SentryFireLoop_03.wav"), 1f, 0f, 0f, false);
            }
        }

        public void OnDrawLayer(Layer pLayer)
        {
            if(pLayer == Layer.Game)
            {
                _head.angle = gunAngle;

                _head.flipH = offDir == 1 ? false : true;
                _body.flipH = offDir == 1 ? false : true;


                if(user != null)
                {
                    int ind = user.profile.currentColor;
                    Color c = Color.White;
                    if(ind == 1)
                    {
                        c = Color.Gray;
                    }
                    if(ind == 2)
                    {
                        c = Color.Yellow;
                    }
                    if(ind == 3)
                    {
                        c = Color.Orange;
                    }
                    if(ind == 4)
                    {
                        c = Color.Turquoise;
                    }
                    if(ind == 5)
                    {
                        c = Color.Pink;
                    }
                    if(ind == 6)
                    {
                        c = Color.LightBlue;
                    }
                    if(ind == 7)
                    {
                        c = Color.Magenta;
                    }


                    _head.color = c;
                }
                else
                {
                    _head.color = Color.Black;
                }

                Graphics.Draw(_body, position.x, bottom - 5, 0.7f);
                Graphics.Draw(_head, position.x, bottom - 12, 0.71f);
            }
        }
    }
}
