using Game.Controllers;
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
		public sealed class ChestObject : BreakableObject
		{
			public List<(InventoryController.Slot slot, Item item, int count)> container = new List<(InventoryController.Slot slot, Item item, int count)>();

			public ChestObject(int _x, int _y, byte[] additionalInformation = null) :
				base(_x, _y, 104, "obj_chest", MapController.Instance.objectsSheet, additionalInformation)
			{
				destRect = new Rectangle(0, 0, (int)Constants.TILE_SIZE, (int)Constants.TILE_SIZE);
				srcRect = new Rectangle(4 * 16, 0, 16, 16);
				isDespawnable = true;
				maxDurability = 3;
                shouldBeBrokenWith = ToolType.Axe;
				if (additionalInformation != null && additionalInformation.Length > 1)
					durability = additionalInformation[1];
				else
					durability = maxDurability;

				if (additionalInformation != null)
					for (int i = 2; i < additionalInformation.Length; i += 4)
						container.Add(
							(
								InventoryController.Instance.GetSlotByRowAndColumn((additionalInformation[i], additionalInformation[i + 1])), 
								Items.GetItemByID(additionalInformation[i + 2]), 
								additionalInformation[i + 3]
							));

				Start();
			}

			protected override void OnBreak(GameObject breaker, ToolType wasBrokenWith)
			{
				base.OnBreak(breaker, wasBrokenWith);
				if (breaker as Hero != null)
				{
					if (wasBrokenWith == shouldBeBrokenWith)
						InventoryController.Instance.ReceiveItems(Items.ItemLantern.Instance, 1);
					else
						InventoryController.Instance.ReceiveItems(Items.ItemWoodenLog.Instance, new Random().Next(1, 5));
					InventoryController.Instance.ReceiveItems(Items.ItemEmberPiece.Instance, new Random().Next(0, 2));
				}
			}

			public void OpenChest()
            {
				GameController.Instance.currentChest = this;
				foreach (var i in container)
					i.slot.SetItem(i.item, i.count, false);
			}

			public void CloseChest()
			{
				InventoryController.Instance.SaveChest(this);
				List<byte> info = new List<byte>()
				{
					(byte)objectID,
					durability
				};
				container.RemoveAll(s => s.slot.currentItem == null);
				foreach (var s in container)
                {
					info.Add((byte)InventoryController.Instance.GetRowAndColumnFromSlot(s.slot).row);
					info.Add((byte)InventoryController.Instance.GetRowAndColumnFromSlot(s.slot).column);
					info.Add((byte)s.item.itemID);
					info.Add((byte)s.slot.itemsCount);
				}
				objectAdditionalInformation = info.ToArray();

				Tile t = MapController.Instance.GetTile(x, y);
				MapController.Instance.GetChunk(x, y).UpdateTile(t.x, t.y);

				InventoryController.Instance.ClearChestSlots();
				GameController.Instance.currentChest = null;
			}

			public override void PostUpdate()
			{
			}

			public override void PreUpdate()
			{
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

			public override void Update()
			{
			}
		}
	}
}
