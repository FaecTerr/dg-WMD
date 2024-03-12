using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckGame.WMD
{
	[EditorGroup("Faecterr's|Weapons")]
	public class Limonka : Gun
    {
		private SpriteMap _sprite;

		public StateBinding _timerBinding = new StateBinding("_timer", -1, false, false);
		public StateBinding _pinBinding = new StateBinding("_pin", -1, false, false);
		public float Timer = 1.2f;
		private float clusterDelay = 0;
		private int _explodeFrames = -1;

		private bool _pin = true;
		public int pieces = 4;
		public float range = 48; 

		private Duck _cookThrower;
		public bool pullOnImpact;

		private bool _explosionCreated;
		private bool _localDidExplode;
		public int gr;

        public bool boosted;
        public bool powered;


		public Limonka(float xval, float yval) : base(xval, yval)
        {
            dontCrush = true;
            ammo = 1;
			_ammoType = new ATShrapnel();
			_ammoType.penetration = 0.4f;
			_type = "gun";
			_sprite = new SpriteMap(GetPath("Sprites/Limonka.png"), 16, 16);
			graphic = _sprite;
			center = new Vec2(7f, 8f);
			collisionOffset = new Vec2(-4f, -5f);
			collisionSize = new Vec2(8f, 10f);
			bouncy = 0.4f;
			friction = 0.05f;
            physicsMaterial = PhysicsMaterial.Metal;
            _fireRumble = RumbleIntensity.Kick;
			_editorName = "Cluster bomb";
			editorTooltip = "#1 Pull pin. #2 Throw grenade. #3 Dodge clusters. The last one important, if you want to live";
			_bio = "To cook grenade, pull pin and hold until feelings of terror run down your spine. Serves as many ducks as you can fit into a 3 meter radius.";

            weight = 0.5f;
		}

        public virtual void PowerUp()
        {
            boosted = false;
            powered = true;
            pieces = 8;
            _sprite = new SpriteMap(GetPath("Sprites/LimonkaPUP.png"), 16, 16);
            _sprite.CenterOrigin();
            graphic = _sprite;
        }

        public virtual void Boost()
        {
            powered = false;
            boosted = true;
            pieces = 6;
            clusterDelay = 0.1f * 60 * 7;
            _sprite = new SpriteMap(GetPath("Sprites/LimonkaBUP.png"), 16, 16);
            _sprite.CenterOrigin();
            graphic = _sprite;
        }

		public override void OnNetworkBulletsFired(Vec2 pos)
		{
			_pin = false;
			_localDidExplode = true;
			if (!_explosionCreated)
			{
				Graphics.FlashScreen();
			}
			CreateExplosion(pos);
		}

		public void CreateExplosion(Vec2 pos)
		{
			if (!_explosionCreated)
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
					float dir = i * 60f + Rando.Float(-10f, 10f);
					float dist = Rando.Float(12f, 20f);
					Level.Add(new ExplosionPart(cx + (float)(Math.Cos(Maths.DegToRad(dir)) * dist), cy - (float)(Math.Sin(Maths.DegToRad(dir)) * dist), true));
				}
                for (int i = 0; i < pieces; i++)
                {
                    float dir = 360 / pieces + Rando.Float(10f);
                    Cluster c = new Cluster(position.x, position.y);
                    c.velocity = new Vec2((float)Math.Cos(dir), (float)Math.Sin(dir)) * 5;
                    c.delay = clusterDelay;

                    if (boosted)
                    {
                        c.scale *= 2;
                        c.range *= 2;
                    }

                    if (isServerForObject)
                    {
                        Level.Add(c);
                    }
				}
				_explosionCreated = true;
                int sound = Rando.Int(4);
                if (sound == 0)
                {
                    SFX.Play(GetPath("SFX/ClusterBombExplosion_01.wav"), 1f, 0f, 0f, false);
                }
                if(sound == 1)
                {
                    SFX.Play(GetPath("SFX/ClusterBombExplosion_02.wav"), 1f, 0f, 0f, false);
                }
                if (sound == 2)
                {
                    SFX.Play(GetPath("SFX/ClusterBombExplosion_03.wav"), 1f, 0f, 0f, false);
                }
                if (sound == 3)
                {
                    SFX.Play(GetPath("SFX/ClusterBombExplosion_04.wav"), 1f, 0f, 0f, false);
                }
                RumbleManager.AddRumbleEvent(pos, new RumbleEvent(RumbleIntensity.Heavy, RumbleDuration.Short, RumbleFalloff.Medium, RumbleType.Gameplay));
			}
		}

		public override void Update()
		{
			base.Update();
			if (!_pin)
			{
				Timer -= 0.01f;
				holsterable = false;
			}
			if (!_localDidExplode && Timer < 0f)
			{
				if (_explodeFrames < 0)
				{
					CreateExplosion(position);
					_explodeFrames = 4;
				}
				else
				{
					_explodeFrames--;
					if (_explodeFrames == 0)
					{
						ExplosionTargetedAtBlocks();
						Level.Remove(this);
						_destroyed = true;
						_explodeFrames = -1;
					}
				}
			}
			if (prevOwner != null && _cookThrower == null)
			{
				_cookThrower = (prevOwner as Duck);
			}
			_sprite.frame = (_pin ? 0 : 1);
		}

		void ExplosionTargetedAtBlocks()
        {
			float cx = x;
			float cy = y;
			Graphics.FlashScreen();
			if (isServerForObject)
			{
				for (int i = 0; i < 20; i++)
				{
					float dir = i * 18f - 5f + Rando.Float(10f);
					ATShrapnel shrap = new ATShrapnel();
					shrap.range = range + Rando.Float(range * 0.2f);
					Bullet bullet = new Bullet(cx + (float)(Math.Cos((double)Maths.DegToRad(dir)) * 6.0), cy - (float)(Math.Sin((double)Maths.DegToRad(dir)) * 6.0), shrap, dir, null, false, -1f, false, true);
					bullet.firedFrom = this;
					firedBullets.Add(bullet);
					Level.Add(bullet);
				}
				foreach (Window w in Level.CheckCircleAll<Window>(this.position, 40f))
				{
					if (Level.CheckLine<Block>(position, w.position, w) == null)
					{
						w.Destroy(new DTImpact(this));
					}
				}
				bulletFireIndex += 20;
				if (Network.isActive)
				{
					Send.Message(new NMFireGun(this, firedBullets, bulletFireIndex, false, 4, false), NetMessagePriority.ReliableOrdered);
					firedBullets.Clear();
				}
				HashSet<ushort> idx = new HashSet<ushort>();
				IEnumerable<BlockGroup> blokzGroup = Level.CheckCircleAll<BlockGroup>(this.position, 50f);
				foreach (BlockGroup block in blokzGroup)
				{
					if (block != null)
					{
						BlockGroup group = block;
						new List<Block>();
						foreach (Block bl in group.blocks)
						{
							if (Collision.Circle(this.position, range, bl.rectangle))
							{
								bl.shouldWreck = true;
								if (bl is AutoBlock)
								{
									idx.Add((bl as AutoBlock).blockIndex);
								}
							}
						}
						group.Wreck();
					}
				}
				IEnumerable<Block> blokz = Level.CheckCircleAll<Block>(position, range * 0.5f);
				foreach (Block block2 in blokz)
				{
					if (block2 is AutoBlock)
					{
						block2.skipWreck = true;
						block2.shouldWreck = true;
						if (block2 is AutoBlock)
						{
							idx.Add((block2 as AutoBlock).blockIndex);
						}
					}
					else if (block2 is Door || block2 is VerticalDoor)
					{
						Level.Remove(block2);
						block2.Destroy(new DTRocketExplosion(null));
					}
				}
				if (Network.isActive && isLocal && idx.Count > 0)
				{
					Send.Message(new NMDestroyBlocks(idx));
				}
			}
		}

		public override void OnSolidImpact(MaterialThing with, ImpactedFrom from)
		{
			if (pullOnImpact)
			{
				OnPressAction();
			}
			base.OnSolidImpact(with, from);
		}
		public override void OnPressAction()
		{
			if (_pin)
			{
				_pin = false;
				Level.Add(new GrenadePin(x, y)
				{
					hSpeed = (float)(-(float)offDir) * (1.5f + Rando.Float(0.5f)),
					vSpeed = -2f
				});
				if (duck != null)
				{
					RumbleManager.AddRumbleEvent(base.duck.profile, new RumbleEvent(this._fireRumble, RumbleDuration.Pulse, RumbleFalloff.None, RumbleType.Gameplay));
				}
				SFX.Play("pullPin", 1f, 0f, 0f, false);
			}
        }
    }

    public class Cluster : PhysicsObject
	{
		private SpriteMap _sprite;

		public float range = 16;
        public float cooldown = 3f;
        public float delay = 0;

		public Cluster(float xval, float yval) : base(xval, yval)
		{
			_sprite = new SpriteMap(GetPath("Sprites/Cluster.png"), 4, 4);
			_sprite.CenterOrigin();
			graphic = _sprite;
			center = _sprite.center;
			_sprite.frame = Rando.Int(3);
			collisionOffset = new Vec2(-1.5f, -1.5f);
			collisionSize = new Vec2(3f, 3f);
            bouncy = 0.5f;
            _sprite.scale = new Vec2(2, 2);
            physicsMaterial = PhysicsMaterial.Metal;

            weight = 0.2f;
        }

        public override void Update()
        {
            if(cooldown > 0)
            {
                cooldown -= 0.1f;
            }
            if(delay > 0)
            {
                delay -= 0.1f;
            }
            base.Update();
        }

        public override void Touch(MaterialThing with)
		{
			if (with != null)
			{
				if (!(with is Holdable) && cooldown <= 0 && delay <= 0)
				{
					ATMissileShrapnel shrap = new ATMissileShrapnel();
					shrap.MakeNetEffect(this.position, false);
					Random rand = null;
					if (Network.isActive && isLocal)
					{
						rand = Rando.generator;
						Rando.generator = new Random(NetRand.currentSeed);
					}
					List<Bullet> firedBullets = new List<Bullet>();
					for (int i = 0; i < 12; i++)
					{
						float dir = i * 30f - 10f + Rando.Float(20f);
						shrap = new ATMissileShrapnel();
						shrap.range = range + Rando.Float(range * 0.2f);
						Vec2 shrapDir = new Vec2((float)Math.Cos((double)Maths.DegToRad(dir)), (float)Math.Sin((double)Maths.DegToRad(dir)));
						Bullet bullet = new Bullet(x + shrapDir.x * 8f, y - shrapDir.y * 8f, shrap, dir, null, false, -1f, false, true);
						bullet.firedFrom = this;
						firedBullets.Add(bullet);
						Level.Add(bullet);
						Level.Add(Spark.New(x + Rando.Float(-8f, 8f), y + Rando.Float(-8f, 8f), shrapDir + new Vec2(Rando.Float(-0.1f, 0.1f), Rando.Float(-0.1f, 0.1f)), 0.02f));
						Level.Add(SmallSmoke.New(x + shrapDir.x * 8f + Rando.Float(-8f, 8f), y + shrapDir.y * 8f + Rando.Float(-8f, 8f)));
					}
					if (Network.isActive && isLocal)
					{
						NMFireGun gunEvent = new NMFireGun(null, firedBullets, 0, false, 4, false);
						Send.Message(gunEvent, NetMessagePriority.ReliableOrdered);
						firedBullets.Clear();
					}
					if (Network.isActive && isLocal)
					{
						Rando.generator = rand;
					}
					IEnumerable<Window> windows = Level.CheckCircleAll<Window>(this.position, 30f);
					foreach (Window w in windows)
					{
						if (this.isLocal)
						{
							Thing.Fondle(w, DuckNetwork.localConnection);
						}
						if (Level.CheckLine<Block>(this.position, w.position, w) == null)
						{
							w.Destroy(new DTImpact(this));
						}
					}
					IEnumerable<PhysicsObject> phys = Level.CheckCircleAll<PhysicsObject>(this.position, 70f);
					foreach (PhysicsObject p in phys)
					{
						if (this.isLocal && this.owner == null)
						{
							Fondle(p, DuckNetwork.localConnection);
						}
						if ((p.position - this.position).length < 30f)
						{
							p.Destroy(new DTImpact(this));
						}
						p.sleeping = false;
						p.vSpeed = -2f;
                    }
                    int sound = Rando.Int(4);
                    if (sound == 0)
                    {
                        SFX.Play(GetPath("SFX/ClusterBombExplosion_01.wav"), 1f, 0f, 0f, false);
                    }
                    if (sound == 1)
                    {
                        SFX.Play(GetPath("SFX/ClusterBombExplosion_02.wav"), 1f, 0f, 0f, false);
                    }
                    if (sound == 2)
                    {
                        SFX.Play(GetPath("SFX/ClusterBombExplosion_03.wav"), 1f, 0f, 0f, false);
                    }
                    if (sound == 3)
                    {
                        SFX.Play(GetPath("SFX/ClusterBombExplosion_04.wav"), 1f, 0f, 0f, false);
                    }
                    Level.Remove(this);
				}
			}
			base.Touch(with);
		}
	}
}
