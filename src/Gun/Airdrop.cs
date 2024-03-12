using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckGame.WMD
{
    [EditorGroup("Faecterr's|Stuff")]
    public class Airdrop : PhysicsObject
    {
        //public bool boost = Rando.Int(2) == 0 ? true : false;
        public SpriteMap _sprite;
        public bool boost;

        public SinWave _pulse = 0.3f;
        public bool init;
        public EditorProperty<bool> Blue;

        public Airdrop(float xpos, float ypos) : base(xpos, ypos)
        {
            _sprite = new SpriteMap(GetPath("Sprites/Airdrop.png"), 16, 16);
            graphic = _sprite;
            center = new Vec2(7.5f, 9.5f);
            _sprite.CenterOrigin();
            collisionOffset = new Vec2(-6.5f, -5.5f);
            collisionSize = new Vec2(13f, 11f);
            _editorName = "Airdrop";

            bouncy = 0.3f;
            dontCrush = true;

            Blue = new EditorProperty<bool>(false);

            _sprite.frame = boost == true ? 0 : 1;
        }

        public override void Initialize()
        {
            if(Network.isActive && Network.isServer)
            {
                DuckNetwork.SendToEveryone(new NMDefineDrop(this, boost));
            }
            base.Initialize();
        }

        public override void EditorUpdate()
        {
            _sprite.frame = Blue != true ? 1 : 0;
            base.EditorUpdate();
        }

        public override void Update()
        {
            if (!init)
            {
                init = true;
                boost = Blue;
            }


            _sprite.frame = boost == true ? 0 : 1;
            if (_pulse != null)
            {
                scale = new Vec2(1.1f - _pulse * 0.1f, 1.1f + _pulse * 0.15f);
            }

            foreach (Duck d in Level.CheckRectAll<Duck>(topLeft, bottomRight))
            {
                if(d.holdObject != null)
                {
                    bool used = false;
                    Holdable h = d.holdObject;
                    if(h is Bat)
                    {
                        if (boost && !(h as Bat).boosted)
                        {
                            (h as Bat).Boost();
                            used = true;
                        }
                        else if (!(h as Bat).powered)
                        {
                            (h as Bat).PowerUp();
                            used = true;
                        }
                    }
                    if(h is EscapeButton && !(h as EscapeButton).powered)
                    {
                        (h as EscapeButton).PowerUp();
                        used = true;
                    }
                    if(h is AirStrike && !(h as AirStrike).boosted)
                    {
                        (h as AirStrike).Boost();
                        used = true;
                    }
                    if(h is BunkerBuster)
                    {
                        if (boost && !(h as BunkerBuster).boosted)
                        {
                            (h as BunkerBuster).Boost();
                            used = true;
                        }
                        else if(!(h as BunkerBuster).powered)
                        {
                            (h as BunkerBuster).PowerUp();
                            used = true;
                        }
                    }
                    if(h is JetpackWMD)
                    {
                        (h as JetpackWMD).PowerUp();
                        used = true;
                    }
                    if(h is Magnet)
                    {
                        (h as Magnet).PowerUp();
                        used = true;
                    }
                    if(h is HominMissileStrike && !(h as HominMissileStrike).boosted)
                    {
                        (h as HominMissileStrike).Boost();
                        used = true;
                    }
                    if(h is Limonka)
                    {
                        if (boost && !(h as Limonka).boosted)
                        {
                            (h as Limonka).Boost();
                            used = true;
                        }
                        else if(!(h as Limonka).powered)
                        {
                            (h as Limonka).PowerUp();
                            used = true;
                        }
                    }
                    if(h is Dynamite && !(h as Dynamite).powered)
                    {
                        (h as Dynamite).PowerUp(); 
                        used = true;
                    }
                    if (h is HolyHandGrenade && !(h as HolyHandGrenade).boosted)
                    {
                        (h as HolyHandGrenade).Boost();
                        used = true;
                    }
                    if (h is DodgyPhoneBattery && !(h as DodgyPhoneBattery).boosted)
                    {
                        (h as DodgyPhoneBattery).Boost();
                        used = true;
                    }
                    if (used)
                    {
                        SFX.Play(GetPath("SFX/CratePickUpUtil.wav"), 1f, 0f, 0f, false);
                        Level.Remove(this);
                    }
                }
            }
            base.Update();
        }
    }
}
