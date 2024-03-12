using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame.WMD
{
    [EditorGroup("Faecterr's|Special")]
    public class AirDrop : Thing, IDrawToDifferentLayers
    {
        public SpriteMap _sprite;
        public bool init;
        //public EditorProperty<float> wind; 
        public float nextBox;

        public AirDrop(float xpos, float ypos) : base(xpos, ypos)
        {
            _sprite = new SpriteMap(Mod.GetPath<WMD>("Sprites/airdelivery.png"), 16, 16, false);
            _sprite.frame = 0;
            graphic = _sprite;
            _canFlip = true;
            _visibleInGame = false;
            center = new Vec2(8f, 8f);
            collisionSize = new Vec2(16f, 16f);
            collisionOffset = new Vec2(-8f, -8f);
            depth = -0.7f;
            _editorName = "Air drop delivery";
        }

        public void OnDrawLayer(Layer pLayer)
        {
            if (pLayer == Layer.Foreground)
            {
                
            }
        }

        public override void Update()
        {
            base.Update();

            if (init == false)
            {
                init = true;
            }
            else
            {
                nextBox -= 0.1f;
                if(nextBox <= 0)
                {
                    nextBox = Rando.Float(0.1f * 60 * 8, 0.1f * 60 * 20);
                    if (isServerForObject)
                    {
                        float posx = Rando.Float(Level.current.topLeft.x, Level.current.bottomRight.x);
                        float posy = Math.Min(Level.current.topLeft.y - 300, Level.current.camera.position.y - 100);

                        bool boosted = Rando.Int(0, 2) == 0;

                        if (isServerForObject)
                        {
                            Level.Add(new Airdrop(posx, posy) { boost = boosted });
                        }
                    }
                }
            }
        }
    }
}
