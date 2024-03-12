using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame.WMD
{
    [EditorGroup("Faecterr's|Special")]
    public class Flood : Thing, IDrawToDifferentLayers
    {
        public SpriteMap _sprite;
        public bool init;
        //public float speed = 1f;
        public EditorProperty<float> speed;
        public EditorProperty<float> delay;
        public EditorProperty<int> startOffset;

        public float delayed;
        public float globalWaterLevel;
        //public Sound s;

        public DateTime lastUpdate = DateTime.Now;

        public int frames;

        public bool lagging;

        public Flood(float xpos, float ypos) : base(xpos, ypos)
        {
            _sprite = new SpriteMap(Mod.GetPath<WMD>("Sprites/flood.png"), 16, 16, false);
            _sprite.frame = 0;
            graphic = _sprite;
            _canFlip = true;
            _visibleInGame = false;
            center = new Vec2(8f, 8f);
            collisionSize = new Vec2(16f, 16f);
            collisionOffset = new Vec2(-8f, -8f);
            depth = -0.7f;
            _editorName = "Flooding";

            delay = new EditorProperty<float>(5f, null, 0f, 60f, 1f);
            speed = new EditorProperty<float>(1f, null, 0.2f, 10f, 0.4f);
            startOffset = new EditorProperty<int>(0, null, -100, 100, 4);
        }

        public void OnDrawLayer(Layer pLayer)
        {
            float LposX = Level.current.camera.left - 16;
            float TposY = Math.Min(globalWaterLevel, Level.current.camera.bottom + 16);
            float RposX = Level.current.camera.right + 16;
            float BposY = Math.Max(Level.current.camera.bottom + 16, globalWaterLevel);
            int globalFrame = (frames / 15);
            if (frames >= 60)
            {
                frames = 0;
            }

            SpriteMap _water = new SpriteMap(GetPath("Sprites/water.png"), 16, 16);
            _water.CenterOrigin();
            _water.alpha = 0.6f;

            
            if (pLayer == Layer.Game && !(Level.current is Editor))
            {
                Graphics.DrawRect(new Vec2(LposX, TposY), new Vec2(RposX, BposY), Color.LightBlue * 0.6f, 1f);
                               
                for (float i = LposX; i < RposX + 16; i += 16)
                {
                    _water.frame = ((int)Math.Abs((i / 16)) + globalFrame) % 4;
                    Graphics.Draw(_water, i, TposY - 8, 1f);
                }
            }
            if (!lagging)
            {
                if (pLayer == Layer.Foreground && !(Level.current is Editor))
                {
                    Graphics.DrawRect(new Vec2(LposX, TposY + 20), new Vec2(RposX, BposY), Color.LightBlue * 0.6f, 1f);

                    _water.scale = new Vec2(1.4f, 1.4f);

                    for (float i = LposX; i < RposX + 16 * 1.4f; i += 16 * 1.4f)
                    {
                        _water.frame = ((int)Math.Abs((i / (16 * 1.4f))) + globalFrame) % 4;
                        Graphics.Draw(_water, i, TposY - 8 * 1.4f + 20, 1f);
                    }
                }
                if (pLayer == Layer.Blocks && !(Level.current is Editor))
                {
                    Graphics.DrawRect(new Vec2(LposX, TposY + 10), new Vec2(RposX, BposY), Color.LightBlue * 0.6f, 1f);

                    _water.scale = new Vec2(1.2f, 1.2f);

                    for (float i = LposX; i < RposX + 16 * 1.2f; i += 16 * 1.2f)
                    {
                        _water.frame = ((int)Math.Abs((i / (16 * 1.2f))) + globalFrame) % 4;
                        Graphics.Draw(_water, i, TposY - 8 * 1.2f + 10, 1f);
                    }
                }
                if (pLayer == Layer.Background && !(Level.current is Editor))
                {
                    Graphics.DrawRect(new Vec2(LposX, TposY - 8), new Vec2(RposX, BposY), Color.LightBlue * 0.6f, -1f);

                    _water.scale = new Vec2(0.8f, 0.8f);

                    for (float i = LposX; i < RposX + 16 * 0.8f; i += 16 * 0.8f)
                    {
                        _water.frame = ((int)Math.Abs((i / (16 * 0.8f))) + globalFrame) % 4;
                        Graphics.Draw(_water, i, TposY - 8 * 0.8f - 8, -1f);
                    }
                }
            }
            
        }

        public override void Update()
        {
            base.Update();
            frames++;
            if (init == false)
            {
                /*if (s == null)
                {
                    s = SFX.Play(GetPath("SFX/WaterLappingLoop.wav"), 1, 0, 0, true);
                }*/
                delayed = 0.1f * 60 * delay;
                init = true;
                globalWaterLevel = Level.current.bottomRight.y + startOffset;
            }
            else
            {
                lagging = false;
                if (Math.Abs((lastUpdate - DateTime.Now).Milliseconds) > 30)
                {
                    lagging = true;
                }
                lastUpdate = DateTime.Now;
                if (delayed <= 0)
                {
                    /*if (s != null)
                    {
                        s.Kill();
                        //sound = null;
                    }*/
                    globalWaterLevel -= speed * 0.1f;
                }
                else
                {
                    delayed -= 0.1f;
                    if(delayed <= 0)
                    {
                        SFX.Play(GetPath("SFX/SuddenDeathTurnStart.wav"), 1f, 0f, 0f, false);
                    }
                }
            }

            foreach (MaterialThing m in Level.current.things[typeof(MaterialThing)])
            {
                if (m.position.y >= globalWaterLevel - 8)
                {                    
                    if (m.heat > 0 || m.onFire)
                    {
                        m.heat = 0;
                        m.onFire = false;
                        m.Extinquish();
                    }
                    if (m is PhysicsObject)
                    {
                        (m as PhysicsObject).sleeping = false;
                        (m as PhysicsObject).DoFloat();
                        (m as PhysicsObject).vSpeed -= (m as PhysicsObject).currentGravity * (m as PhysicsObject).gravMultiplier * (m.vSpeed > 3 ? 2 : 1.2f);
                        
                    }
                    //m.Hurt(10);
                    if (m is Duck)
                    {
                        if (!(m as Duck).dead)
                        {
                            int sound = Rando.Int(4);
                            if (sound == 0)
                            {
                                SFX.Play(GetPath("SFX/WormSplash_01.wav"), 1f, 0f, 0f, false);
                            }
                            else if (sound == 1)
                            {
                                SFX.Play(GetPath("SFX/WormSplash_02.wav"), 1f, 0f, 0f, false);
                            }
                            else
                            {
                                SFX.Play(GetPath("SFX/WormSplash_03.wav"), 1f, 0f, 0f, false);
                            }
                            (m as Duck).Kill(new DTFall());
                        }
                    }
                    if (m is RagdollPart)
                    {
                        if ((m as RagdollPart)._doll != null && (m as RagdollPart)._doll._duck != null && !(m as RagdollPart)._doll._duck.dead)
                        {
                            int sound = Rando.Int(4);
                            if (sound == 0)
                            {
                                SFX.Play(GetPath("SFX/WormSplash_01.wav"), 1f, 0f, 0f, false);
                            }
                            else if (sound == 1)
                            {
                                SFX.Play(GetPath("SFX/WormSplash_02.wav"), 1f, 0f, 0f, false);
                            }
                            else
                            {
                                SFX.Play(GetPath("SFX/WormSplash_03.wav"), 1f, 0f, 0f, false);
                            }
                            (m as RagdollPart)._doll._duck.Kill(new DTFall());
                        }
                    }
                }
            }
        }
    }
}
