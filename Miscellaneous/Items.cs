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
	public enum ToolType
	{
		Axe,
		Spade,
		Pickaxe
	}

	public abstract class Item
	{
		public interface IUsable
        {
			int usesLeft { get; set; }
			void OnItemUse();
		}

		public interface IPlaceable
        {
			bool OnItemPlacing(Tile placeAt, GameObject sender = null);
		}

		public int itemID;
		public string itemName;
		public int maxStackQuantity;

		public Rectangle itemSrcRect;

		public virtual void OnItemReceive() { }
	}
    public abstract class ItemTool : Item
    {
		public byte efficiency;
		public ToolType toolType;
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
				case 4: return ItemBonfire.Instance;
				case 5: return ItemChest.Instance;
				case 6: return ItemStoneAxe.Instance;
				case 7: return ItemEmberPiece.Instance;
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
				case "item_lantern": return ItemBonfire.Instance;
				case "item_chest": return ItemChest.Instance;
				case "item_stone_axe": return ItemStoneAxe.Instance;
				case "item_ember_piece": return ItemEmberPiece.Instance;
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
		public sealed class ItemWoodenFence : Item, Item.IPlaceable
		{
			// singleton
			private ItemWoodenFence()
			{
				itemID = 2;
				itemName = "item_wooden_fence";
				itemSrcRect = new Rectangle(16, 0, 16, 16);

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

            public bool OnItemPlacing(Tile placeAt, GameObject sender = null)
            {
				// нельзя ставить в воде
				if (placeAt.tileType < 2)
					return false;

				placeAt.SetGameObject(
					GameObject.Spawn(
						"obj_wooden_fence", 
						placeAt.globalX, 
						placeAt.globalY));
				MapController.Instance.GetChunk(placeAt.globalX, placeAt.globalY).UpdateTile(placeAt.x, placeAt.y);
				return true;
			}
		}
		public sealed class ItemWoodenFenceGate : Item, Item.IPlaceable
		{
			// singleton
			private ItemWoodenFenceGate()
			{
				itemID = 3;
				itemName = "item_wooden_fence_gate";
				itemSrcRect = new Rectangle(32, 0, 16, 16);

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

			public bool OnItemPlacing(Tile placeAt, GameObject sender = null)
			{
				// нельзя ставить в воде
				if (placeAt.tileType < 2)
					return false;

				placeAt.SetGameObject(
					GameObject.Spawn(
						"obj_wooden_fence_gate", 
						placeAt.globalX, 
						placeAt.globalY, 
						new byte[] { (byte)GameObject.GetObjectIDByName("obj_wooden_fence_gate"), 1, (byte)(sender?.x != placeAt.globalX ? 1 : 0) }));
				MapController.Instance.GetChunk(placeAt.globalX, placeAt.globalY).UpdateTile(placeAt.x, placeAt.y);
				return true;
			}
		}
		public sealed class ItemBonfire : Item, Item.IPlaceable
		{
			// singleton
			private ItemBonfire()
			{
				itemID = 4;
				itemName = "item_bonfire";
				itemSrcRect = new Rectangle(5 * 16, 0, 16, 16);

				maxStackQuantity = 100;
			}
			private static ItemBonfire instance;
			public static ItemBonfire Instance
			{
				get
				{
					if (instance == null)
						instance = new ItemBonfire();
					return instance;
				}
			}

			public bool OnItemPlacing(Tile placeAt, GameObject sender = null)
			{
				// нельзя ставить в воде
				if (placeAt.tileType < 2)
					return false;

				GameObject obj;
				placeAt.SetGameObject(
					obj = GameObject.Spawn(
						"obj_bonfire",
						placeAt.globalX,
						placeAt.globalY,
						new byte[] { (byte)GameObject.GetObjectIDByName("obj_bonfire"), 1 }));
				MapController.Instance.GetChunk(placeAt.globalX, placeAt.globalY).UpdateTile(placeAt.x, placeAt.y);
				return true;
			}
		}
		public sealed class ItemChest : Item, Item.IPlaceable
		{
			// singleton
			private ItemChest()
			{
				itemID = 5;
				itemName = "item_chest";
				itemSrcRect = new Rectangle(3 * 16, 0, 16, 16);

				maxStackQuantity = 100;
			}
			private static ItemChest instance;
			public static ItemChest Instance
			{
				get
				{
					if (instance == null)
						instance = new ItemChest();
					return instance;
				}
			}

			public bool OnItemPlacing(Tile placeAt, GameObject sender = null)
			{
				// нельзя ставить в воде
				if (placeAt.tileType < 2)
					return false;

				GameObject obj;
				placeAt.SetGameObject(
					obj = GameObject.Spawn(
						"obj_chest",
						placeAt.globalX,
						placeAt.globalY,
						new byte[] { (byte)GameObject.GetObjectIDByName("obj_chest"), 1 }));
				MapController.Instance.GetChunk(placeAt.globalX, placeAt.globalY).UpdateTile(placeAt.x, placeAt.y);
				return true;
			}
		}
		public sealed class ItemStoneAxe : ItemTool
		{
			// singleton
			private ItemStoneAxe()
			{
				itemID = 6;
				itemName = "item_stone_axe";
				itemSrcRect = new Rectangle(16 * 4, 0, 16, 16);
				toolType = ToolType.Axe;
				efficiency = 2;

				maxStackQuantity = 1;
			}
			private static ItemStoneAxe instance;
			public static ItemStoneAxe Instance
			{
				get
				{
					if (instance == null)
						instance = new ItemStoneAxe();
					return instance;
				}
			}
		}
		public sealed class ItemEmberPiece : Item
		{
			// singleton
			private ItemEmberPiece()
			{
				itemID = 7;
				itemName = "item_ember_piece";
				itemSrcRect = new Rectangle(6 * 16, 0, 16, 16);

				maxStackQuantity = 100;
			}
			private static ItemEmberPiece instance;
			public static ItemEmberPiece Instance
			{
				get
				{
					if (instance == null)
						instance = new ItemEmberPiece();
					return instance;
				}
			}
		}
	}
}