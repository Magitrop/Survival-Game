using Game.Controllers;
using Game.Interfaces;
using Game.Miscellaneous;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.GameObjects
{
	public abstract partial class GameObject
	{
		private sealed class BonfireObject : LightingObject, IAnimatable
		{
			public List<Animation> animations { get; set; }
			private int curAnimation;
			private int curAnimatonFrame;
			private float curAnimationTimer;

			public BonfireObject(int _x, int _y, byte[] additionalInformation = null) : 
				base(_x, _y, 103, "obj_bonfire", Constants.objectsSheet, additionalInformation)
			{
				destRect = new Rectangle(0, 0, (int)Constants.TILE_SIZE, (int)Constants.TILE_SIZE);
				srcRect = new Rectangle(4 * 16, 16, 16, 16);
				isDespawnable = true;
				maxDurability = 1;
                shouldBeBrokenWith = ToolType.Axe;
				if (additionalInformation != null && additionalInformation.Length > 1)
					durability = additionalInformation[1];
				else
					durability = maxDurability;

				curAnimation = 0;
				InitializeAnimation();

				Start();
			}

			protected override void OnBreak(GameObject breaker, ToolType wasBrokenWith)
			{
				base.OnBreak(breaker, wasBrokenWith);
				if (breaker as Hero != null)
				{
					if (wasBrokenWith == shouldBeBrokenWith)
						InventoryController.Instance.ReceiveItems(Items.ItemBonfire.Instance, 1);
					else
						InventoryController.Instance.ReceiveItems(Items.ItemWoodenLog.Instance, new Random().Next(1, 5));
					InventoryController.Instance.ReceiveItems(Items.ItemEmberPiece.Instance, new Random().Next(0, 2));
				}
			}

			public override void Render()
			{
				PlayAnimation();
				destRect.X = (int)(x * Constants.TILE_SIZE + MapController.Instance.camera.x);
				destRect.Y = (int)(y * Constants.TILE_SIZE + MapController.Instance.camera.y);
				GameController.Instance.Render(sprite, destRect, srcRect);
			}

			public void PlayAnimation()
			{
				if ((curAnimationTimer += Time.deltaTime) > animations[curAnimation].frames[curAnimatonFrame].frameDuration)
                {
					curAnimationTimer = 0;
					if (++curAnimatonFrame >= animations[curAnimation].frames.Count)
						curAnimatonFrame = 0;
				}
				
				srcRect = animations[curAnimation].frames[curAnimatonFrame].frame.srcRect;
				destRect = animations[curAnimation].frames[curAnimatonFrame].frame.destRect;
			}

			public void InitializeAnimation()
			{
				Image img = Constants.objectsSheet;
				float duration = 0.14f;
				animations = new List<Animation>()
				{
					new Animation("idle")
					{
						frames = new List<(ImageFrame frame, float frameDuration)>()
						{
							(new ImageFrame(img, new Rectangle(0, 0, (int)Constants.TILE_SIZE, (int)Constants.TILE_SIZE), new Rectangle(8 * 16, 0 * 16, 16, 16)), duration),
							(new ImageFrame(img, new Rectangle(0, 0, (int)Constants.TILE_SIZE, (int)Constants.TILE_SIZE), new Rectangle(8 * 16, 1 * 16, 16, 16)), duration),
							(new ImageFrame(img, new Rectangle(0, 0, (int)Constants.TILE_SIZE, (int)Constants.TILE_SIZE), new Rectangle(8 * 16, 2 * 16, 16, 16)), duration),
							(new ImageFrame(img, new Rectangle(0, 0, (int)Constants.TILE_SIZE, (int)Constants.TILE_SIZE), new Rectangle(8 * 16, 3 * 16, 16, 16)), duration),
						}
					}
				};
				curAnimatonFrame = new Random((x, y).GetHashCode()).Next(0, animations[curAnimation].frames.Count);
			}

			public override void Start()
			{

			}

			public override void PreUpdate()
			{

			}

			public override void Update()
			{
			}

			public override void PostUpdate()
			{

			}
		}
	}
}