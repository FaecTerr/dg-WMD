using System;

namespace DuckGame.WMD
{
    [EditorGroup("Faecterr's|Equipment")]
    public class JetpackWMD : Holdable, IDrawToDifferentLayers
    {
        protected SpriteMap _sprite;
        public bool _on;
        public float _fuel = 5f * 0.1f * 60;
        public Duck ownDuck;

        public JetpackWMD(float xpos, float ypos) : base(xpos, ypos)
        {
            _editorName = "Jet Pack";
            _sprite = new SpriteMap(GetPath("Sprites/jetpack.png"), 16, 16, false);
            graphic = _sprite;
            center = new Vec2(8f, 8f);
            collisionOffset = new Vec2(-5f, -5f);
            collisionSize = new Vec2(11f, 12f);
            thickness = 0.1f;
            _canRaise = false;

            dontCrush = true;
        }               

        public virtual void PowerUp()
        {
            _sprite.frame = 3;
            _fuel = 10f * 0.1f * 60;
        }

        public override void Update()
        {
            if (owner != null)
            {
                if (owner is Duck)
                {
                    ownDuck = owner as Duck;
                }
                else
                {
                    ownDuck = null;
                }
            }
            else
            {
                ownDuck = null;
                center = new Vec2(8, 8);
            }

            if (duck != null)
            {
                center = new Vec2(14, 5);
                layer = duck.layer;
                _depth = duck._sprite.depth.value - 0.5f;
                if (duck.inputProfile.Pressed("SHOOT") && !duck.grounded)
                {
                    _on = true;
                }
                if (duck.inputProfile.Released("SHOOT"))
                {
                    _on = false;
                }

                float smokeOff = 0f;
                angle = 0f;
                if (duck.sliding)
                {
                    if (duck.offDir > 0)
                    {
                        angle = -1.57079637f;
                    }
                    else
                    {
                        angle = 1.57079637f;
                    }
                    smokeOff -= 6f;
                }
                if (duck.crouch && !duck.sliding)
                {

                }

                solid = false;
                PhysicsObject propel = duck;
                _sprite.flipH = duck._sprite.flipH;
                if (_on && _fuel > 0f && propel != null)
                {
                    if (duck._trapped == null && duck.crouch)
                    {
                        duck.sliding = true;
                    }
                    _fuel -= 0.4f;

                    Level.Add(new JetpackSmoke(x, y + 8f + smokeOff));
                    if (angle > 0f)
                    {
                        if (propel.hSpeed < 6f)
                        {
                            propel.hSpeed += 0.9f;
                        }
                    }
                    else if (angle < 0f)
                    {
                        if (propel.hSpeed > -6f)
                        {
                            propel.hSpeed -= 0.9f;
                        }
                    }
                    else if (propel.vSpeed > -4.5f)
                    {
                        propel.vSpeed -= 0.38f;
                    }
                    if (_fuel <= 0f)
                    {
                        _on = false;
                    }
                }
            }
            else
            {
                if (_sprite != null)
                {
                    _sprite.flipH = false;
                }
                collisionOffset = new Vec2(-5f, -5f);
                collisionSize = new Vec2(11f, 12f);
                solid = true;
            }
            if (_fuel <= 0)
            {
                Level.Remove(this);
            }
            base.Update();
        }

        public void OnDrawLayer(Layer pLayer)
        {
            if(pLayer == Layer.Foreground)
            {
                if (duck != null)
                {
                    Graphics.DrawStringOutline(Convert.ToString((int)((_fuel * 10 / 6) + 0.99f)), duck.position + new Vec2(-6, -14f), Color.LightBlue, Color.Black, 1f, null, 1f);
                }
            }
        }
    }
}
