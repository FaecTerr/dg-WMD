using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckGame.WMD
{
    [EditorGroup("Faecterr's|Weapons")]
    public class Dynamite : Gun
	{
		public SpriteMap _sprite;
		private Duck _cookThrower;

		private bool _sticky; 
		private bool _pin = true;
		private bool _touchedGround;
		private float range = 48; 
		private float bulletRange = 48 * 5;
		public float Timer = 2.4f; //0.01f per frame, 60 frames per second - 4 seconds
		public bool powered;

		private bool _explosionCreated;
		private bool _localDidExplode;
		private int _explodeFrames = -1;

		private Sound FuseSound = SFX.Get(Mod.GetPath<WMD>("SFX/DynamiteFuse.wav"), 1f, 0f, 0f, false);
        public Dynamite(float xval, float yval) : base(xval, yval)
		{
			dontCrush = true;
			ammo = 1;
			_ammoType = new ATShrapnel();
			_ammoType.penetration = 0.4f;
			_type = "gun";
			_sprite = new SpriteMap(GetPath("Sprites/Dynamite.png"), 9, 24); 
			
			_sprite.AddAnimation("idle", 1f, false, new int[] { 0 });
			_sprite.AddAnimation("fuse", 8f, false, new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 });
			_sprite.AddAnimation("ignite", 5f, true, new int[] { 1, 2 });

			graphic = _sprite;
			center = new Vec2(4.5f, 12f);
			collisionOffset = new Vec2(-4f, -8f);
			collisionSize = new Vec2(8f, 16f);

			physicsMaterial = PhysicsMaterial.Metal;
			_fireRumble = RumbleIntensity.Kick;
			_editorName = "Dynamite";
			editorTooltip = "Easy to light up, hard to run";
			_bio = "That old dynamite found in worms stock";

			_sprite.SetAnimation("idle");

			weight = 0.5f; 
			bouncy = 0.1f;
		}
		public override void Fire()
		{
		}
		public virtual void PowerUp()
		{
			_sticky = true;
			powered = true;
			_sprite = new SpriteMap(GetPath("Sprites/DynamiteSticky.png"), 9, 24);
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
		public override void OnPressAction()
		{
			if (_pin)
			{
				_pin = false; 
				SFX.Play(GetPath("SFX/DynamiteIgnite.wav"), 1f, 0f, 0f, false);
				if (duck != null)
				{
					RumbleManager.AddRumbleEvent(duck.profile, new RumbleEvent(this._fireRumble, RumbleDuration.Pulse, RumbleFalloff.None, RumbleType.Gameplay));
				}
				if(owner != null && owner is Duck)
                {
					(owner as Duck).doThrow = true;
                }
			}
		}
        public override void Touch(MaterialThing with)
		{
			if (_sticky && with is Block && !_pin && prevOwner != null)
			{
				gravMultiplier = 0;
				velocity = new Vec2(0, 0);
				enablePhysics = false;
				_touchedGround = true;
				//When collision happens, sticky dynamite would stay in place and countinue explosion
			}
			base.Touch(with);
        }
        public override void Update()
		{
			base.Update();
			if (!_pin)
			{
				if (_touchedGround)
				{
					if(_sprite.currentAnimation != "fuse")
                    {
						_sprite.SetAnimation("fuse");
					}
					Timer -= 0.01f;
					holsterable = false;
				}
                else
                {
                    if (grounded)
                    {
						_sprite.SetAnimation("fuse");
						FuseSound.Play();
						_touchedGround = true;
                    }
                }
				canPickUp = false;
			}
            else
            {
				if(owner != null)
                {
					_sprite.SetAnimation("ignite");
				}
                else
                {
					_sprite.SetAnimation("idle");
				}
            }
			if (!_localDidExplode && Timer < 0f)
			{
				if(!FuseSound.IsDisposed)
                {
					FuseSound.Stop();
                }
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
						Graphics.FlashScreen();
						if (isServerForObject)
						{
							BlockTargetedExplosion();
						}
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
		void BlockTargetedExplosion()
		{
			float cx = x;
			float cy = y - 2f;
			for (int i = 0; i < 20; i++)
			{
				float dir = i * 18f - 5f + Rando.Float(10f);
				ATShrapnel shrap = new ATShrapnel();
				shrap.range = bulletRange + Rando.Float(bulletRange * 0.2f);
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
		} //Targeted at block*
		public void CreateExplosion(Vec2 pos)
		{
			if (!_explosionCreated)
			{
				float cx = pos.x;
				float cy = pos.y - 2f;
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
				_explosionCreated = true;
				SFX.Play(GetPath("SFX/DynamiteExplosion.wav"), 1f, 0f, 0f, false);			
				RumbleManager.AddRumbleEvent(pos, new RumbleEvent(RumbleIntensity.Heavy, RumbleDuration.Short, RumbleFalloff.Medium, RumbleType.Gameplay));
			}
		}
        public override void Draw()
        {
            base.Draw();
			if (_touchedGround) 
			{
				float time = (float)Math.Round(Timer / 0.6f, 1);
				string text = Convert.ToString(time);
				float scale = 0.8f;
				Graphics.DrawStringOutline(text, position + new Vec2(-text.Length * 4 * scale, -8), Color.White, Color.Black, 0.5f, null, scale);			
			}
        }
    }
}
