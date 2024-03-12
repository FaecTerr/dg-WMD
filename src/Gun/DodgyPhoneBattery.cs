using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckGame.WMD
{
    [EditorGroup("Faecterr's|Weapons")]
    public class DodgyPhoneBattery : Gun, IDrawToDifferentLayers
    {
        public SpriteMap _sprite;
        private SpriteMap _line = new SpriteMap(Mod.GetPath<WMD>("Sprites/HeatWave.png"), 16, 16);

        private Duck usedBy;

        private bool toggled = false;
        private bool triggered = false;

        private int chainLength = 5; //Amount of objects in chain
        private int currentStep = 0; //defines which current chain link now active
        private float range = 64;
        private float delayActivation = 1.2f; //0.01f per frame
        private float stepTime = 0.35f; //0.01f per frame
        private float stepSubTime = 0f; //Timer for step time
        private float stayTime = 1.2f; //0.01f per frame

        public bool boosted;

        private List<PhysicsObject> chainReactionObjects = new List<PhysicsObject>();

        public DodgyPhoneBattery(float xval, float yval) : base(xval, yval)
        {
            ammo = 1;

            _sprite = new SpriteMap(GetPath("Sprites/DodgyPhoneBattery.png"), 13, 16);
            _sprite.CenterOrigin();
            _graphic = _sprite;

            center = new Vec2(6.5f, 8f);

            collisionSize = new Vec2(11, 14);
            collisionOffset = new Vec2(-5.5f, -7f);

            bouncy = 0.6f;
            chainReactionObjects.Add(this);

            _line.AddAnimation("idle", 0.25f, true, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15});
            _line.SetAnimation("idle");
            
            _editorName = "DPB";
            editorTooltip = "Let's call your friends and ask them what they think about this thing";
            _bio = "It's dodgy, It's Battery, But It's not Phone.";
        }
        public override void Fire()
        {
        }
        public void Boost()
        {
            _sprite = new SpriteMap(GetPath("Sprites/ChargedPhoneBattery.png"), 13, 16);
            _sprite.CenterOrigin();
            _graphic = _sprite;
            boosted = true;
            range = 96f;
            chainLength = 7;
        }

        void AddToChain()
        {
            if(chainReactionObjects.Count <= 0 || chainReactionObjects.Count >= chainLength)
            {
                return;
            }
            float MinDistance = -1;
            PhysicsObject obj = null;

            Vec2 positionOfLastLinkInChain = chainReactionObjects[chainReactionObjects.Count - 1].position;

            foreach (PhysicsObject p in Level.CheckCircleAll<PhysicsObject>(positionOfLastLinkInChain, range))
            {
                if(p != usedBy && 
                    (usedBy.ragdoll == null || (usedBy.ragdoll != null && p != usedBy.ragdoll.part1 && p != usedBy.ragdoll.part2 && p != usedBy.ragdoll.part3)) 
                    && !chainReactionObjects.Contains(p) && 
                    (p.physicsMaterial == PhysicsMaterial.Metal || p.physicsMaterial == PhysicsMaterial.Duck || p.physicsMaterial == PhysicsMaterial.Default))
                {
                    if(MinDistance < 0)
                    {
                        MinDistance = (p.position - positionOfLastLinkInChain).length; 
                        obj = p;
                    }
                    else
                    {
                        if((p.position - positionOfLastLinkInChain).length < MinDistance)
                        {
                            MinDistance = (p.position - positionOfLastLinkInChain).length;
                            obj = p;
                        }
                    }
                }
            }
            if(obj != null && MinDistance > 0)
            {
                chainReactionObjects.Add(obj);
            }
        }

        public override void Update()
        {
            base.Update();
            if (!toggled)
            {
                if(owner == null && prevOwner != null && (prevOwner is Duck))
                {
                    toggled = true;
                    usedBy = prevOwner as Duck;
                    canPickUp = false;
                }
            }
            else
            {
                if (triggered && usedBy != null)
                {
                    if(delayActivation > 0)
                    {
                        delayActivation -= 0.01f;
                    }
                    else
                    {
                        int tries = 0;
                        while(chainReactionObjects.Count < chainLength && tries < chainReactionObjects.Count)
                        {
                            AddToChain();
                            tries++;
                        }
                        if (currentStep >= chainLength)
                        {
                            if(stayTime > 0)
                            {
                                stayTime -= 0.01f;
                            }
                            else
                            {
                                Explosion();
                            }
                        }
                        else
                        {
                            ElectrifyChain();
                            if (stepSubTime > 0)
                            {
                                stepSubTime -= 0.01f;
                            }
                            else
                            {
                                currentStep++;
                                if (currentStep < chainLength)
                                {
                                    stepSubTime = stepTime;
                                    if (currentStep % 4 == 0)
                                    {
                                        SFX.Play(GetPath("SFX/DPB_ChainShot_01.wav"), 1f, 0f, 0f, false);
                                    }
                                    if (currentStep % 4 == 1)
                                    {
                                        SFX.Play(GetPath("SFX/DPB_ChainShot_02.wav"), 1f, 0f, 0f, false);
                                    }
                                    if (currentStep % 4 == 2)
                                    {
                                        SFX.Play(GetPath("SFX/DPB_ChainShot_03.wav"), 1f, 0f, 0f, false);
                                    }
                                    if (currentStep % 4 == 3)
                                    {
                                        SFX.Play(GetPath("SFX/DPB_ChainShot_04.wav"), 1f, 0f, 0f, false);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (grounded)
                    {
                        triggered = true;
                    }
                }
            }
        }

        public override void OnImpact(MaterialThing with, ImpactedFrom from)
        {
            if(with is Block)
            {
                triggered = true;
            }
            base.OnImpact(with, from);
        }
        public void ElectrifyChain()
        {
            if (chainReactionObjects.Count > 1)
            {
                Vec2 startPos = chainReactionObjects[0].position;
                Vec2 endPos = new Vec2(0, 0);
                for (int i = 0; i < Math.Min(chainReactionObjects.Count, currentStep); i++)
                {
                    endPos = chainReactionObjects[i].position;
                    Electrify(startPos, endPos);
                    startPos = endPos;
                }
            }
        }
        public void Electrify(Vec2 start, Vec2 end)
        {
            foreach (PhysicsObject p in Level.CheckLineAll<PhysicsObject>(start, end))
            {
                p.Hurt(1f);
                p.HeatUp(p.position);
                if (p is Duck)
                {
                    (p as Duck).Kill(new DTCrush(this));
                    if ((p as Duck).ragdoll != null)
                    {
                        Ragdoll d = (p as Duck).ragdoll;
                        if(d._duck != null)
                        {
                            d._duck.Kill(new DTCrush(this));
                        }
                        if (d.part1.duck != null)
                        {
                            d.part1.duck.Kill(new DTCrush(this));
                        }
                        if (d.part2.duck != null)
                        {
                            d.part2.duck.Kill(new DTCrush(this));
                        }
                        if (d.part3.duck != null)
                        {
                            d.part3.duck.Kill(new DTCrush(this));
                        }
                    }
                }
            }
        }
        public void Explosion()
        {
            /*if (chainReactionObjects.Count > 0)
            {
                CreateExplosion(chainReactionObjects[chainReactionObjects.Count - 1].position);
            }*/
            CreateExplosion(position);
            Level.Remove(this);
        }
        public void CreateExplosion(Vec2 pos)
        {
            float cx = pos.x;
            float cy = pos.y;
            Level.Add(new ExplosionPart(cx, cy, true));
            int num = 6;
            if (Graphics.effectsLevel < 2)
            {
                num = 3;
            }
            for (int i = 0; i < num; i++)
            {
                float dir = (float)i * 60f + Rando.Float(-10f, 10f);
                float dist = Rando.Float(12f, 20f);
                Level.Add(new ExplosionPart(cx + (float)(Math.Cos(Maths.DegToRad(dir)) * dist), cy - (float)(Math.Sin(Maths.DegToRad(dir)) * dist), true));
            }
            SFX.Play(GetPath("SFX/DPB_EndExpl.wav"), 1f, 0f, 0f, false);
            RumbleManager.AddRumbleEvent(pos, new RumbleEvent(RumbleIntensity.Heavy, RumbleDuration.Short, RumbleFalloff.Medium, RumbleType.Gameplay));
        }
        public void OnDrawLayer(Layer pLayer)
        {
            if(pLayer == Layer.Foreground)
            {
                if(chainReactionObjects.Count > 1)
                {
                    for (int i = 0; i < Math.Min(chainReactionObjects.Count - 2, currentStep); i++)
                    {
                        Vec2 start = chainReactionObjects[i].position;
                        Vec2 end = chainReactionObjects[i + 1].position;
                        float len = (start - end).length;
                        int lines = (int)(len / 16f);

                        //for (int j = 0; j < lines; j++)
                        //{
                        //    //Graphics.DrawTexturedLine(_line.texture, start, start + end * (j / len), Color.Blue, (len % 16 / 16), 0.5f);

                        //}
                        Graphics.DrawLine(start, end, Color.Blue, 3f, 0.5f);
                    }
                }
            }
        }
    }
}
