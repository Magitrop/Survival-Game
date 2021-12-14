using Game.Controllers;
using Game.Interfaces;
using Game.Map;
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
		private sealed class WoodenFenceObject : BreakableObject
		{
			public WoodenFenceObject(int _x, int _y, byte[] additionalInformation = null) : 
				base(_x, _y, 101, "obj_wooden_fence", Constants.objectsSheet, additionalInformation)
			{
				destRect = new Rectangle(0, 0, (int)Constants.TILE_SIZE, (int)Constants.TILE_SIZE);
				srcRect = new Rectangle(16, 0, 16, 16);
				isDespawnable = true;
				maxDurability = 3;
                shouldBeBrokenWith = ToolType.Axe;
				if (additionalInformation != null && additionalInformation.Length > 1)
					durability = additionalInformation[1];
				else
					durability = maxDurability;

				Start();
			}

			protected override void OnBreak(GameObject breaker, ToolType wasBrokenWith)
			{
				base.OnBreak(breaker, wasBrokenWith);
				if (breaker as Hero != null)
				{
					if (wasBrokenWith == shouldBeBrokenWith)
						InventoryController.Instance.ReceiveItems(Items.ItemWoodenFence.Instance, 1);
					else
						InventoryController.Instance.ReceiveItems(Items.ItemWoodenLog.Instance, new Random().Next(1, 3));
				}
			}

			protected override void OnSpawn()
			{
				base.OnSpawn();
				RecalculateAdjacent();
			}

			public void RecalculateAdjacent()
			{
				GetSpriteSrc();
				GameObject wall;
				if ((wall = MapController.Instance.GetTile(x - 1, y, false)?.gameObject)?.objectName == "obj_wooden_fence")
					((WoodenFenceObject)wall).GetSpriteSrc();
				if ((wall = MapController.Instance.GetTile(x, y + 1, false)?.gameObject)?.objectName == "obj_wooden_fence")
					((WoodenFenceObject)wall).GetSpriteSrc();
				if ((wall = MapController.Instance.GetTile(x + 1, y, false)?.gameObject)?.objectName == "obj_wooden_fence")
					((WoodenFenceObject)wall).GetSpriteSrc();
				if ((wall = MapController.Instance.GetTile(x, y - 1, false)?.gameObject)?.objectName == "obj_wooden_fence")
					((WoodenFenceObject)wall).GetSpriteSrc();
			}

			private void GetSpriteSrc()
			{
				byte tilesAround = 0;
				if (MapController.Instance.GetTile(x - 1, y, false)?.gameObject?.objectName == "obj_wooden_fence")
					tilesAround |= 1;
				if (MapController.Instance.GetTile(x, y + 1, false)?.gameObject?.objectName == "obj_wooden_fence")
					tilesAround |= 2;
				if (MapController.Instance.GetTile(x + 1, y, false)?.gameObject?.objectName == "obj_wooden_fence")
					tilesAround |= 4;
				if (MapController.Instance.GetTile(x, y - 1, false)?.gameObject?.objectName == "obj_wooden_fence")
					tilesAround |= 8;

				/*
				t r d l
				0 0 0 0
				0 0 0 1
				0 0 1 0
				. . . .
				*/
				switch (tilesAround)
				{
					case 0:
						srcRect.X = 16 * 3;
						srcRect.Y = 16 * 3;
						break; // нигде
					case 1:
						srcRect.X = 16 * 2;
						srcRect.Y = 0;
						break; // слева
					case 2:
						srcRect.X = 16 * 3;
						srcRect.Y = 0;
						break; // снизу
					case 3:
						srcRect.X = 16 * 2;
						srcRect.Y = 16 * 1;
						break; // снизу и слева
					case 4:
						srcRect.X = 0;
						srcRect.Y = 0;
						break; // справа
					case 5:
						srcRect.X = 16 * 1;
						srcRect.Y = 0;
						break; // справа и слева
					case 6:
						srcRect.X = 0;
						srcRect.Y = 16 * 1;
						break; // справа и снизу
					case 7:
						srcRect.X = 16 * 1;
						srcRect.Y = 16 * 1;
						break; // справа и снизу и слева
					case 8:
						srcRect.X = 16 * 3;
						srcRect.Y = 16 * 2;
						break; // сверху
					case 9:
						srcRect.X = 16 * 2;
						srcRect.Y = 16 * 3;
						break; // сверху и слева
					case 10:
						srcRect.X = 16 * 3;
						srcRect.Y = 16 * 1;
						break; // сверху и снизу
					case 11:
						srcRect.X = 16 * 2;
						srcRect.Y = 16 * 2;
						break; // сверху и снизу и слева
					case 12:
						srcRect.X = 0;
						srcRect.Y = 16 * 3;
						break; // сверху и справа
					case 13:
						srcRect.X = 16 * 1;
						srcRect.Y = 16 * 3;
						break; // сверху и справа и слева
					case 14:
						srcRect.X = 0;
						srcRect.Y = 16 * 2;
						break; // сверху и справа и снизу
					case 15:
						srcRect.X = 16 * 1;
						srcRect.Y = 16 * 2;
						break; // со всех сторон
					default:
						srcRect.X = 16 * 3;
						srcRect.Y = 16 * 3;
						break;
				}
			}

			public override void Render()
			{
				destRect.X = (int)(x * Constants.TILE_SIZE + MapController.Instance.camera.x);
				destRect.Y = (int)(y * Constants.TILE_SIZE + MapController.Instance.camera.y);
				GameController.Instance.Render(sprite, destRect, srcRect);
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