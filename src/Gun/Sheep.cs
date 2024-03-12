using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckGame.WMD
{
    //[EditorGroup("Faecterr's|Weapons")]
    public class Sheep : Holdable, IDrawToDifferentLayers
    {
        public float time = 0.1f * 60 * 30; // magnet will be active 30 seconds
        public float range = 80;
        public float power = 5;
        public bool placd;

        public SpriteMap _sprite;

        public Sheep(float xval, float yval) : base(xval, yval)
        {
            _sprite = new SpriteMap(GetPath("Sprites/Sheep.png"), 16, 16);
            graphic = _sprite;
            center = new Vec2(8f, 8f);
            _holdOffset = new Vec2(0, 3);
            _sprite.CenterOrigin();
            collisionOffset = new Vec2(-7f, -7f);
            collisionSize = new Vec2(14f, 14f);
            _editorName = "Magnet";

            friction = 1;
        }

        public void OnDrawLayer(Layer pLayer)
        {
            if (pLayer == Layer.Foreground)
            {
                if (placd)
                {
                    
                }
            }
        }

        public override void Update()
        {
            if (placd)
            {
                if (time > 0)
                {
                    time -= 0.1f;
                    
                }
            }
            else
            {
                if (owner != null)
                {
                    if (owner is Duck)
                    {
                        if ((owner as Duck).profile.inputProfile.Pressed("SHOOT"))
                        {
                            (owner as Duck).doThrow = true;
                            placd = true;
                            canPickUp = false;
                        }
                    }
                }
            }
            base.Update();
        }
    }
}
