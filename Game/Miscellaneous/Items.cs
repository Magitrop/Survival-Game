using Game.Controllers;
using Game.GameObjects;
using Game.Map;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Miscellaneous
{
	public abstract class Item
	{
		public int itemID;
		public string itemName;

		public bool isConsumable;
		public bool isPlaceable;
		public int maxStackQuantity;
		public int usesLeft;

		public Rectangle itemSrcRect;

		public virtual void OnItemReceive() { }
		public virtual void OnItemUse() { }
		public virtual bool OnItemPlacing(Tile placeAt) { return true; }
	}

	public static class Items
    {
		public static Item GetItemByID(int itemID)
        {
			switch (itemID)
			{
				case 1: return ItemWoodenLog.Instance;
				case 2: return ItemWoodenFence.Instance;
				case 3: return ItemWoodenFenceGate.Instance;
				default: return null;
			};
		}

		public static Item GetItemByName(string itemName)
		{
			switch (itemName)
			{
				case "item_wooden_log": return ItemWoodenLog.Instance;
				case "item_wooden_fence": return ItemWoodenFence.Instance;
				case "item_wooden_fence_gate": return ItemWoodenFenceGate.Instance;
				default: return null;
			};
		}

		public sealed class ItemWoodenLog : Item
        {
			// singleton
			private ItemWoodenLog() 
			{
				itemID = 1;
				itemName = "item_wooden_log";
				itemSrcRect = new Rectangle(0, 0, 16, 16);

				isPlaceable = false;
				isConsumable = false;
				maxStackQuantity = 100;
			}
			private static ItemWoodenLog instance;
			public static ItemWoodenLog Instance
			{
				get
				{
					if (instance == null)
						instance = new ItemWoodenLog();
					return instance;
				}
			}
		}
		public sealed class ItemWoodenFence : Item
		{
			// singleton
			private ItemWoodenFence()
			{
				itemID = 2;
				itemName = "item_wooden_fence";
				itemSrcRect = new Rectangle(16, 0, 16, 16);

				isPlaceable = true;
				isConsumable = false;
				maxStackQuantity = 100;
			}
			private static ItemWoodenFence instance;
			public static ItemWoodenFence Instance
			{
				get
				{
					if (instance == null)
						instance = new ItemWoodenFence();
					return instance;
				}
			}

            public override bool OnItemPlacing(Tile placeAt)
            {
				// нельзя ставить в воде
				if (placeAt.tileType < 2)
					return false;

				placeAt.SetGameObject(GameObject.Spawn("obj_wall", placeAt.globalX, placeAt.globalY));
				MapController.Instance.GetChunk(placeAt.globalX, placeAt.globalY).UpdateTile(placeAt.x, placeAt.y);
				return true;
			}
		}
		public sealed class ItemWoodenFenceGate : Item
		{
			// singleton
			private ItemWoodenFenceGate()
			{
				itemID = 3;
				itemName = "item_wooden_fence_gate";
				itemSrcRect = new Rectangle(32, 0, 16, 16);

				isPlaceable = true;
				isConsumable = false;
				maxStackQuantity = 100;
			}
			private static ItemWoodenFenceGate instance;
			public static ItemWoodenFenceGate Instance
			{
				get
				{
					if (instance == null)
						instance = new ItemWoodenFenceGate();
					return instance;
				}
			}

			public override bool OnItemPlacing(Tile placeAt)
			{
				// нельзя ставить в воде
				if (placeAt.tileType < 2)
					return false;

				placeAt.SetGameObject(GameObject.Spawn("obj_fence_gate", placeAt.globalX, placeAt.globalY));
				MapController.Instance.GetChunk(placeAt.globalX, placeAt.globalY).UpdateTile(placeAt.x, placeAt.y);
				return true;
			}
		}
	}
}