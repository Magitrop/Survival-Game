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
		Pickaxe,
		Hand
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
		public virtual void OnTurnStart() { }
		public virtual void OnTurnEnd() { }
		public virtual int WhenAttackWith(int playerStartDamageAmount) { return playerStartDamageAmount; }
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
				case 8: return ItemWoodenStick.Instance;
				case 9: return ItemCobblestone.Instance;
				case 10: return ItemStonePickaxe.Instance;
				case 11: return ItemStoneSpear.Instance;
				case 12: return ItemRawMeat.Instance;
				case 13: return ItemCoconut.Instance;
				case 14: return ItemWoodenPlanks.Instance;
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
				case "item_wooden_stick": return ItemWoodenStick.Instance;
				case "item_cobblestone": return ItemCobblestone.Instance;
				case "item_stone_pickaxe": return ItemStonePickaxe.Instance;
				case "item_stone_spear": return ItemStoneSpear.Instance;
				case "item_raw_meat": return ItemRawMeat.Instance;
				case "item_coconut": return ItemCoconut.Instance;
				case "item_wooden_planks": return ItemWoodenPlanks.Instance;
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

			public override int WhenAttackWith(int playerStartDamageAmount)
			{
				return playerStartDamageAmount + 2;
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
						new byte[] { (byte)GameObject.GetObjectIDByName("obj_wooden_fence_gate"), 3, (byte)(sender?.x != placeAt.globalX ? 1 : 0) }));
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
						placeAt.globalY));
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
						placeAt.globalY));
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

			public override int WhenAttackWith(int playerStartDamageAmount)
			{
				return playerStartDamageAmount + 5;
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
		public sealed class ItemWoodenStick : Item
		{
			// singleton
			private ItemWoodenStick()
			{
				itemID = 8;
				itemName = "item_wooden_stick";
				itemSrcRect = new Rectangle(7 * 16, 0, 16, 16);

				maxStackQuantity = 100;
			}
			private static ItemWoodenStick instance;
			public static ItemWoodenStick Instance
			{
				get
				{
					if (instance == null)
						instance = new ItemWoodenStick();
					return instance;
				}
			}

			public override int WhenAttackWith(int playerStartDamageAmount)
			{
				return playerStartDamageAmount + 1;
			}
		}
		public sealed class ItemCobblestone : Item
		{
			// singleton
			private ItemCobblestone()
			{
				itemID = 9;
				itemName = "item_cobblestone";
				itemSrcRect = new Rectangle(8 * 16, 0, 16, 16);

				maxStackQuantity = 100;
			}
			private static ItemCobblestone instance;
			public static ItemCobblestone Instance
			{
				get
				{
					if (instance == null)
						instance = new ItemCobblestone();
					return instance;
				}
			}

			public override int WhenAttackWith(int playerStartDamageAmount)
			{
				return playerStartDamageAmount + 1;
			}
		}
		public sealed class ItemStonePickaxe : ItemTool
		{
			// singleton
			private ItemStonePickaxe()
			{
				itemID = 10;
				itemName = "item_stone_pickaxe";
				itemSrcRect = new Rectangle(16 * 9, 0, 16, 16);
				toolType = ToolType.Pickaxe;
				efficiency = 2;

				maxStackQuantity = 1;
			}
			private static ItemStonePickaxe instance;
			public static ItemStonePickaxe Instance
			{
				get
				{
					if (instance == null)
						instance = new ItemStonePickaxe();
					return instance;
				}
			}

			public override int WhenAttackWith(int playerStartDamageAmount)
			{
				return playerStartDamageAmount + 3;
			}
		}
		public sealed class ItemStoneSpear : Item
		{
			// singleton
			private ItemStoneSpear()
			{
				itemID = 11;
				itemName = "item_stone_spear";
				itemSrcRect = new Rectangle(16 * 10, 0, 16, 16);

				maxStackQuantity = 1;
			}
			private static ItemStoneSpear instance;
			public static ItemStoneSpear Instance
			{
				get
				{
					if (instance == null)
						instance = new ItemStoneSpear();
					return instance;
				}
			}

			public override int WhenAttackWith(int playerStartDamageAmount)
			{
				return playerStartDamageAmount + 18;
			}
		}
		public sealed class ItemRawMeat : Item, Item.IUsable
		{
			// singleton
			private ItemRawMeat()
			{
				itemID = 12;
				itemName = "item_raw_meat";
				itemSrcRect = new Rectangle(16 * 11, 0, 16, 16);

				maxStackQuantity = 100;
			}
			private static ItemRawMeat instance;
			public static ItemRawMeat Instance
			{
				get
				{
					if (instance == null)
						instance = new ItemRawMeat();
					return instance;
				}
			}

			public int usesLeft { get; set; }

			public void OnItemUse()
			{
				GameObject.Hero h = (GameController.Instance.mainHero as GameObject.Hero);
				h.hunger = MathOperations.MoveTowards(h.hunger, 100, 5);
				h.thirst = MathOperations.MoveTowards(h.thirst, 0, 1);
			}
		}
		public sealed class ItemCoconut : Item, Item.IUsable
		{
			// singleton
			private ItemCoconut()
			{
				itemID = 13;
				itemName = "item_coconut";
				itemSrcRect = new Rectangle(16 * 13, 0, 16, 16);

				maxStackQuantity = 100;
			}
			private static ItemCoconut instance;
			public static ItemCoconut Instance
			{
				get
				{
					if (instance == null)
						instance = new ItemCoconut();
					return instance;
				}
			}

			public int usesLeft { get; set; }

			public void OnItemUse()
			{
				GameObject.Hero h = (GameController.Instance.mainHero as GameObject.Hero);
				h.hunger = MathOperations.MoveTowards(h.hunger, 100, 1);
				h.thirst = MathOperations.MoveTowards(h.thirst, 100, 5);
			}
		}
		public sealed class ItemWoodenPlanks : Item, Item.IPlaceable
		{
			// singleton
			private ItemWoodenPlanks()
			{
				itemID = 14;
				itemName = "item_wooden_planks";
				itemSrcRect = new Rectangle(16 * 14, 0, 16, 16);

				maxStackQuantity = 100;
			}
			private static ItemWoodenPlanks instance;
			public static ItemWoodenPlanks Instance
			{
				get
				{
					if (instance == null)
						instance = new ItemWoodenPlanks();
					return instance;
				}
			}

			public int usesLeft { get; set; }

			public bool OnItemPlacing(Tile placeAt, GameObject sender = null)
			{
				placeAt.tileType = 8;
				MapController.Instance.GetChunk(placeAt.globalX, placeAt.globalY).UpdateTile(placeAt.x, placeAt.y);
				return true;
			}
		}
	}
}