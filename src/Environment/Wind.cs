using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame.WMD
{
    //[EditorGroup("Faecterr's|Special")]
    public class Wind : Thing, IDrawToDifferentLayers
    {
        public SpriteMap _sprite;
        public bool init;
        //public EditorProperty<float> wind; 
        public int direction;
        public float power = 2;

        public Wind(float xpos, float ypos) : base(xpos, ypos)
        {
            _sprite = new SpriteMap(Mod.GetPath<WMD>("Sprites/wind.png"), 16, 16, false);
            _sprite.frame = 0;
            graphic = _sprite;
            _canFlip = true;
            _visibleInGame = false;
            center = new Vec2(8f, 8f);
            collisionSize = new Vec2(16f, 16f);
            collisionOffset = new Vec2(-8f, -8f);
            depth = -0.7f;
            //wind = new EditorProperty<float>(0, null, -1, 1, 0.2f);
        }

        public void OnDrawLayer(Layer pLayer)
        {
            if(pLayer == Layer.Foreground)
            {
                if(Math.Abs(direction) >= 1)
                {
                    Vec2 pos = new Vec2(Rando.Float(Level.current.topLeft.x, Level.current.bottomRight.x), Rando.Float(Level.current.topLeft.y, Level.current.bottomRight.y));

                    SmallSmoke s = SmallSmoke.New(pos.x, pos.y, 0.8f, 1f);
                    s.hSpeed = direction * power;
                    Level.Add(s);
                }
            }
        }

        public override void Update()
        {
            base.Update();

            if (init == false)
            {
                init = true;
                direction = Rando.Int(-3, 3);
            }
            else
            {
                /*foreach (PhysicsObject p in Level.current.things[typeof(PhysicsObject)])
                {
                    if(p is HominMissileStrike)
                    {
                        p.hSpeed += direction * power * 0.1f;
                    }                    
                }
                */
            }
        }
    }
}
