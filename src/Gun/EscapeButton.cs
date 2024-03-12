using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckGame.WMD
{
    [BaggedProperty("isFatal", false)]
    [EditorGroup("Faecterr's|Weapons|Remote")]
    public class EscapeButton : Holdable, IDrawToDifferentLayers
    {
        public SpriteMap _sprite;
        public SpriteMap _target;
        public Vec2 pos;

        public bool startTP;
        public float chargeTP = 60 * 0.1f * 1.3f; // last num for seconds before tp
        public bool charging;

        public bool powered;

        public float moveMod = 3;

        public EscapeButton(float xpos, float ypos) : base(xpos, ypos)
        {
            _sprite = new SpriteMap(GetPath("Sprites/Teleport.png"), 13, 17, false);
            _target = new SpriteMap(GetPath("Sprites/AirStrikeAim.png"), 17, 17, false);
            _target.CenterOrigin();
            graphic = _sprite;
            center = new Vec2(6.5f, 8.5f);
            collisionOffset = new Vec2(-6.5f, -5.5f);
            collisionSize = new Vec2(13f, 11f);
            _target.frame = 1;
            dontCrush = true;

            _editorName = "Escape button";
            editorTooltip = "Where did he go?";
        }
        
        public virtual void PowerUp()
        {
            powered = true;
            chargeTP = 60 * 0.1f * 0.1f;
            moveMod = 4.5f;
            _sprite = new SpriteMap(GetPath("Sprites/TeleportPUP.png"), 13, 17, false);
            graphic = _sprite;
        }

        public virtual void PrepTeleport()
        {
            bool acceptable = Level.CheckRect<Block>(pos + new Vec2(-7, -12), pos + new Vec2(7, 12)) == null;
            if(pos.x > Level.current.bottomRight.x || pos.y > Level.current.bottomRight.y || pos.x < Level.current.topLeft.x || pos.y < Level.current.topLeft.y)
            {
                acceptable = false;
            }
            if (acceptable)
            {
                if(owner != null)
                {
                    startTP = true;
                    SFX.Play(GetPath("SFX/TeleportOut.wav"), 1f, 0f, 0f, false);
                }
            }
            else
            {
                charging = false;
            }
        }

        public virtual void Teleport()
        {
            if(owner != null)
            {
                if (powered)
                {
                    foreach (Duck d in Level.CheckRectAll<Duck>(pos + new Vec2(-7, -12), pos + new Vec2(7, 12)))
                    {
                        d.position = owner.position;
                    }
                }
                SFX.Play(GetPath("SFX/TeleportIn.wav"), 1f, 0f, 0f, false);
                owner.position = pos;
                if(owner is Duck)
                {
                    (owner as Duck)._sleeping = false;
                }
                Level.Remove(this);
            }
        }

        public override void Update()
        {
            base.Update();

            if(prevOwner != null && owner == null)
            {
                if (_prevOwner is Duck)
                {
                    (_prevOwner as Duck).immobilized = false;
                }
                _prevOwner = null;
            }
            /*if (!powered)
            {
                PowerUp();
            }*/
            if(owner == null)
            {
                _sprite.frame = 0;
            }
            else
            {
                _sprite.frame = 1;
            }

            if(startTP && owner == null)
            {
                Level.Remove(this);
            }
            if (owner != null)
            {
                bool acceptable = true; 
                if (pos.x > Level.current.bottomRight.x)
                {
                    pos.x = Level.current.bottomRight.x;
                }
                if(pos.x < Level.current.topLeft.x)
                {
                    pos.x = Level.current.topLeft.x;
                }
                if (pos.y < Level.current.topLeft.y - 32)
                {
                    pos.y = Level.current.topLeft.y - 32;
                }

                acceptable = Level.CheckRect<Block>(pos + new Vec2(-7, -12), pos + new Vec2(7, 12)) == null;
                if (pos.y > Level.current.bottomRight.y)
                {
                    acceptable = false;
                }

                if (startTP)
                {
                    chargeTP -= 0.1f;
                    if(chargeTP <= 0)
                    {
                        Teleport();
                    }
                }

                if (acceptable)
                {
                    _target.alpha = 1f;
                }
                else
                {
                    _target.alpha = 0.3f;
                }


                Duck d = owner as Duck;
                if (d.profile.inputProfile.Released("SHOOT"))
                {
                    d.immobilized = false;

                    PrepTeleport();       
                }
                if (d.profile.inputProfile.Down("SHOOT") && !startTP)
                {
                    d.immobilized = true;
                    if (charging == false)
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
                        pos += move * moveMod;
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
}