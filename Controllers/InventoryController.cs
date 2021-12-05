using Game.Interfaces;
using Game.Miscellaneous;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Controllers
{
	public sealed class InventoryController : IBehaviour
	{
		// singleton
		private InventoryController() { }
		private static InventoryController instance;
		public static InventoryController Instance
		{
			get
			{
				if (instance == null)
					instance = new InventoryController();
				return instance;
			}
		}

		public enum InventoryDisplayMode
		{
			DoNotDisplay,			// не отображать вообще
			HotbarOnly,				// только слоты на горячие клавиши
			PlayerOnly,				// обычное отображение инвентаря
			Whole					// для сундуков
		}

		public sealed class Slot
		{
			public Item currentItem { get; private set; } = null;
			public int itemsCount;

			public Rectangle screenRect;

			public void SetItem(Item itemToSet, int count, bool triggerReceiveEvent = true)
			{
				currentItem = itemToSet;
				itemsCount = count;
				if (itemToSet != null && triggerReceiveEvent)
					itemToSet.OnItemReceive();
			}

			public void UseItem()
			{
				if ((currentItem as Item.IUsable) != null)
				{
					(currentItem as Item.IUsable).OnItemUse();
					if ((currentItem as Item.IUsable).usesLeft-- < 0)
						if (--itemsCount <= 0)
							SetItem(null, 0);
				}
			}

			public void TrashItem()
			{
				if (currentItem != null)
				{
					//currentItem.OnItemTrash();
					SetItem(null, 0);
				}
			}

			public static void SwapSlots(Slot from, Slot to)
			{
				Item tempItem = to.currentItem;
				int tempCount = to.itemsCount;

				to.currentItem = from.currentItem;
				to.itemsCount = from.itemsCount;

				from.currentItem = tempItem;
				from.itemsCount = tempCount;
			}
		}

		private int columns = 10;
		private int playerRows = 3;
		private int chestRows = 3;
		private int rows
		{
			get => playerRows + chestRows;
		}
		public int slotsCount
		{
			get => columns * (rows + chestRows);
		}
		public int slotSize
		{
			get => 80;
		}
		public List<Slot[]> slots = new List<Slot[]>();
		public int currentSlotIndex = 0;
		public Slot currentSlot
		{
			get => slots[currentSlotIndex][rows - 1];
		}
		public Slot selectedSlot { get; private set; }

		private Rectangle slotDestRect;
		private Rectangle slotSrcRect;
		private Rectangle currentSlotSrcRect;	// который выбран в нижней панели слотов (hotbar)
		private Rectangle selectedSlotSrcRect;	// на который наведен курсор мыши
		private Rectangle itemDestRect;
		private Rectangle itemTrashIcon;
		private Font itemsCountFont;
		private Brush itemsCountBrush;

		public InventoryDisplayMode displayMode;
		private Slot draggingItemFromSlot = null;
		public bool isDraggingItem { get; private set; }

		public void Start()
		{
			for (int i = 0; i < columns; i++)
			{
				slots.Add(new Slot[rows]);
				for (int j = 0; j < rows; j++)
				{
					(slots[i][j] = new Slot()).screenRect = new Rectangle
						(
							i * slotSize + (Constants.WINDOW_WIDTH - columns * slotSize - 16) / 2,
							j * slotSize + (Constants.WINDOW_HEIGHT - (rows + 1) * slotSize),
							slotSize,
							slotSize
						);
				}
			}

			slotDestRect = new Rectangle();
			slotDestRect.Width = slotSize;
			slotDestRect.Height = slotSize;

			slotSrcRect = new Rectangle(0, 0, 32, 32);
			selectedSlotSrcRect = new Rectangle(64, 0, 32, 32);
			currentSlotSrcRect = new Rectangle(32, 0, 32, 32);

			itemTrashIcon = new Rectangle(96, 0, 16, 16);

			itemDestRect = new Rectangle();
			itemDestRect.Width = slotSize - 20;
			itemDestRect.Height = slotSize - 20;

			displayMode = InventoryDisplayMode.HotbarOnly;

			itemsCountFont = new Font(Fonts.fonts.Families[0], 25.0F, FontStyle.Bold);
			itemsCountBrush = new SolidBrush(Color.White);
		}
		public bool CheckIfPlayerSlot(Slot s)
		{
			return GetRowAndColumnFromSlot(s).row >= rows - playerRows;
		}
		public void ClearChestSlots()
        {
			for (int j = 0; j < chestRows; j++)
				for (int i = 0; i < columns; i++)
					slots[i][j].SetItem(null, 0);
		}
		public void SaveChest(GameObjects.GameObject.ChestObject chest)
        {
			for (int j = 0; j < chestRows; j++)
				for (int i = 0; i < columns; i++)
					if (slots[i][j].currentItem != null)
						chest.container.Add((slots[i][j], slots[i][j].currentItem, slots[i][j].itemsCount));
		}
		public void ReceiveItems(Item item, int count = 1)
		{
			if (count < 1)
				return;
			for (int j = rows - 1; j >= playerRows; j--)
				for (int i = 0; i < columns; i++)
					if (slots[i][j].currentItem == null)
					{
						if (HasEnoughItems(item, 1))
							continue;
						slots[i][j].SetItem(item, count);
						return;
					}
					else if (slots[i][j].currentItem.itemName == item.itemName &&
						slots[i][j].itemsCount < slots[i][j].currentItem.maxStackQuantity)
					{
						if (slots[i][j].itemsCount + count <= slots[i][j].currentItem.maxStackQuantity)
							slots[i][j].itemsCount += count;
						else
						{
							int remaining = slots[i][j].currentItem.maxStackQuantity - slots[i][j].itemsCount;
							slots[i][j].itemsCount = slots[i][j].currentItem.maxStackQuantity;
							ReceiveItems(item, remaining);
						}
						return;
					}

		}
		public void AddItemsToSlot(Slot slot, Item item, int count = 1)
		{
			if (count < 1)
				return;
			if (slot.currentItem == null)
				slot.SetItem(item, count);
			else if (slot.currentItem.itemName == item.itemName && slot.itemsCount < slot.currentItem.maxStackQuantity)
			{
				if (slot.itemsCount + count <= slot.currentItem.maxStackQuantity)
					slot.itemsCount += count;
				else
				{
					int remaining = slot.currentItem.maxStackQuantity - slot.itemsCount;
					slot.itemsCount = slot.currentItem.maxStackQuantity;
					ReceiveItems(item, remaining);
				}
			}
		}
		public bool HasEnoughItems(Item item, int count)
		{
			for (int j = playerRows; j < rows; j++)
				for (int i = 0; i < columns; i++)
					if (slots[i][j].currentItem != null && 
						slots[i][j].currentItem.itemName == item.itemName)
						if ((count -= slots[i][j].itemsCount) <= 0)
							return true;
							
			return false;
		}
		public bool WithdrawItems(Item item, int count)
		{
			if (count < 1)
				return true;
			if (!HasEnoughItems(item, count))
				return false;
			for (int j = playerRows; j < rows; j++)
				for (int i = 0; i < columns; i++)
					if (slots[i][j].currentItem != null && 
						slots[i][j].currentItem.itemName == item.itemName)
					{
						int canWithdrawFromhere = slots[i][j].itemsCount - MathOperations.MoveTowards(slots[i][j].itemsCount, 0, count);
						count -= canWithdrawFromhere;
						if ((slots[i][j].itemsCount -= canWithdrawFromhere) <= 0)
							slots[i][j].SetItem(null, 0);
					}
							
			return true;
		}
		public bool WithdrawItemsFromSlot(Slot slot, Item item, int count)
		{
			if (count < 1)
				return true;
			if (slot.currentItem == null ||
				slot.currentItem.itemName != item.itemName ||
				slot.itemsCount < count)
				return false;

			if ((slot.itemsCount -= slot.itemsCount - MathOperations.MoveTowards(slot.itemsCount, 0, count)) <= 0)
			{
				slot.SetItem(null, 0);
				return true;
			}
			
			return false;
		}
		public Slot GetSlotByRowAndColumn((int row, int column) slot) => slots[slot.column][slot.row];
		public (int row, int column) GetRowAndColumnFromSlot(Slot slot)
		{
			for (int j = 0; j < rows; j++)
				for (int i = 0; i < columns; i++)
					if (slots[i][j] == slot)
						return (j, i);
			return (-1, -1);
		}

		public void PreUpdate() { }
		public void Update() { }
		public void PostUpdate() 
		{
			selectedSlot = null;
			bool mouseWasInsideSlot = false;

			switch (displayMode)
			{
				case InventoryDisplayMode.DoNotDisplay:
					break;
				case InventoryDisplayMode.HotbarOnly:
					for (int i = 0; i < columns; i++)
					{
						int j = rows - 1;
						slotDestRect = slots[i][j].screenRect;

						if (MathOperations.IsPointInside(GameController.Instance.mousePosition, slotDestRect))
						{
							mouseWasInsideSlot = true;
							GameController.Instance.Render(MapController.Instance.uiSheet, slotDestRect, selectedSlotSrcRect);
							if (draggingItemFromSlot == null)
							{
								selectedSlot = slots[i][j];
								if (GameController.Instance.mouseIsDown)
								{
									draggingItemFromSlot = selectedSlot;
									isDraggingItem = true;
								}
							}
							else if (!GameController.Instance.mouseIsDown)
							{
								if (draggingItemFromSlot.currentItem != null && draggingItemFromSlot != slots[i][j])
								{
									if (draggingItemFromSlot.currentItem?.itemName != slots[i][j].currentItem?.itemName)
										Slot.SwapSlots(draggingItemFromSlot, slots[i][j]);
									else
									{
										if (slots[i][j].itemsCount + draggingItemFromSlot.itemsCount <= slots[i][j].currentItem.maxStackQuantity)
										{
											slots[i][j].itemsCount += draggingItemFromSlot.itemsCount;
											draggingItemFromSlot.SetItem(null, 0);
										}
										else
										{
											int remaining = slots[i][j].currentItem.maxStackQuantity - slots[i][j].itemsCount;
											slots[i][j].itemsCount = slots[i][j].currentItem.maxStackQuantity;
											if ((draggingItemFromSlot.itemsCount -= remaining) <= 0)
											{
												draggingItemFromSlot.SetItem(null, 0);
											}
										}
									}
								}
								draggingItemFromSlot = null;
								isDraggingItem = false;
							}
						}
						else if (slots[i][j] == currentSlot/*currentSlotIndex == i && j == rows - 1*/)
							GameController.Instance.Render(MapController.Instance.uiSheet, slotDestRect, currentSlotSrcRect);
						else
							GameController.Instance.Render(MapController.Instance.uiSheet, slotDestRect, slotSrcRect);

						if (slots[i][j].currentItem != null && slots[i][j] != draggingItemFromSlot)
						{
							itemDestRect.X = slotDestRect.X + 10;
							itemDestRect.Y = slotDestRect.Y + 10;
							GameController.Instance.Render(
								MapController.Instance.itemsSheet,
								itemDestRect,
								slots[i][j].currentItem.itemSrcRect);
							GameController.Instance.Render(
								slots[i][j].itemsCount.ToString(),
								new Point(
									itemDestRect.X + (int)(55 - 10f * slots[i][j].itemsCount.ToString().Length),
									itemDestRect.Y + 35),
								itemsCountFont,
								itemsCountBrush);
						}

						if (draggingItemFromSlot != null && draggingItemFromSlot.currentItem != null)
						{
							itemDestRect.X = GameController.Instance.mousePosition.X - 16;
							itemDestRect.Y = GameController.Instance.mousePosition.Y - 16;
							GameController.Instance.Render(
								MapController.Instance.itemsSheet,
								itemDestRect,
								draggingItemFromSlot.currentItem.itemSrcRect);
						}
					}
					break;
				case InventoryDisplayMode.PlayerOnly:
					for (int i = 0; i < columns; i++)
						for (int j = playerRows; j < rows; j++)
						{
							slotDestRect = slots[i][j].screenRect;

							if (MathOperations.IsPointInside(GameController.Instance.mousePosition, slotDestRect))
							{
								mouseWasInsideSlot = true;
								GameController.Instance.Render(MapController.Instance.uiSheet, slotDestRect, selectedSlotSrcRect);
								if (draggingItemFromSlot == null)
								{
									selectedSlot = slots[i][j];
									if (GameController.Instance.mouseIsDown)
									{
										draggingItemFromSlot = selectedSlot;
										isDraggingItem = true;
									}
								}
								else if (!GameController.Instance.mouseIsDown)
								{
									if (draggingItemFromSlot.currentItem != null && draggingItemFromSlot != slots[i][j])
									{
										if (draggingItemFromSlot.currentItem?.itemName != slots[i][j].currentItem?.itemName)
											Slot.SwapSlots(draggingItemFromSlot, slots[i][j]);
										else
										{
											if (slots[i][j].itemsCount + draggingItemFromSlot.itemsCount <= slots[i][j].currentItem.maxStackQuantity)
											{
												slots[i][j].itemsCount += draggingItemFromSlot.itemsCount;
												draggingItemFromSlot.SetItem(null, 0);
											}
											else
											{
												int remaining = slots[i][j].currentItem.maxStackQuantity - slots[i][j].itemsCount;
												slots[i][j].itemsCount = slots[i][j].currentItem.maxStackQuantity;
												if ((draggingItemFromSlot.itemsCount -= remaining) <= 0)
												{
													draggingItemFromSlot.SetItem(null, 0);
												}
											}
										}
									}
									draggingItemFromSlot = null;
									isDraggingItem = false;
								}
							}
							else if (slots[i][j] == currentSlot/*currentSlotIndex == i && j == rows - 1*/)
								GameController.Instance.Render(MapController.Instance.uiSheet, slotDestRect, currentSlotSrcRect);
							else
								GameController.Instance.Render(MapController.Instance.uiSheet, slotDestRect, slotSrcRect);

							if (slots[i][j].currentItem != null && slots[i][j] != draggingItemFromSlot)
							{
								itemDestRect.X = slotDestRect.X + 10;
								itemDestRect.Y = slotDestRect.Y + 10;
								GameController.Instance.Render(
									MapController.Instance.itemsSheet,
									itemDestRect,
									slots[i][j].currentItem.itemSrcRect);
								GameController.Instance.Render(
									slots[i][j].itemsCount.ToString(),
									new Point(
										itemDestRect.X + (int)(55 - 10f * slots[i][j].itemsCount.ToString().Length),
										itemDestRect.Y + 35),
									itemsCountFont,
									itemsCountBrush);
							}

							if (draggingItemFromSlot != null && draggingItemFromSlot.currentItem != null)
							{
								itemDestRect.X = GameController.Instance.mousePosition.X - 16;
								itemDestRect.Y = GameController.Instance.mousePosition.Y - 16;
								GameController.Instance.Render(
									MapController.Instance.itemsSheet,
									itemDestRect,
									draggingItemFromSlot.currentItem.itemSrcRect);
							}
						}
					break;
				case InventoryDisplayMode.Whole:
					for (int i = 0; i < columns; i++)
						for (int j = 0; j < rows; j++)
						{
							slotDestRect = slots[i][j].screenRect;
							if (j < chestRows)
								slotDestRect.Y -= 15;

							if (MathOperations.IsPointInside(GameController.Instance.mousePosition, slotDestRect))
							{
								mouseWasInsideSlot = true;
								GameController.Instance.Render(MapController.Instance.uiSheet, slotDestRect, selectedSlotSrcRect);
								if (draggingItemFromSlot == null)
								{
									selectedSlot = slots[i][j];
									if (GameController.Instance.mouseIsDown)
									{
										draggingItemFromSlot = selectedSlot;
										isDraggingItem = true;
									}
								}
								else if (!GameController.Instance.mouseIsDown)
								{
									if (draggingItemFromSlot.currentItem != null && draggingItemFromSlot != slots[i][j])
									{
										if (draggingItemFromSlot.currentItem?.itemName != slots[i][j].currentItem?.itemName)
											Slot.SwapSlots(draggingItemFromSlot, slots[i][j]);
										else
										{
											if (slots[i][j].itemsCount + draggingItemFromSlot.itemsCount <= slots[i][j].currentItem.maxStackQuantity)
											{
												slots[i][j].itemsCount += draggingItemFromSlot.itemsCount;
												draggingItemFromSlot.SetItem(null, 0);
											}
											else
											{
												int remaining = slots[i][j].currentItem.maxStackQuantity - slots[i][j].itemsCount;
												slots[i][j].itemsCount = slots[i][j].currentItem.maxStackQuantity;
												if ((draggingItemFromSlot.itemsCount -= remaining) <= 0)
												{
													draggingItemFromSlot.SetItem(null, 0);
												}
											}
										}
									}
									draggingItemFromSlot = null;
									isDraggingItem = false;
								}
							}
							else if (slots[i][j] == currentSlot/*currentSlotIndex == i && j == rows - 1*/)
								GameController.Instance.Render(MapController.Instance.uiSheet, slotDestRect, currentSlotSrcRect);
							else
								GameController.Instance.Render(MapController.Instance.uiSheet, slotDestRect, slotSrcRect);

							if (slots[i][j].currentItem != null && slots[i][j] != draggingItemFromSlot)
							{
								itemDestRect.X = slotDestRect.X + 10;
								itemDestRect.Y = slotDestRect.Y + 10;
								GameController.Instance.Render(
									MapController.Instance.itemsSheet,
									itemDestRect,
									slots[i][j].currentItem.itemSrcRect);
								GameController.Instance.Render(
									slots[i][j].itemsCount.ToString(),
									new Point(
										itemDestRect.X + (int)(55 - 10f * slots[i][j].itemsCount.ToString().Length),
										itemDestRect.Y + 35),
									itemsCountFont,
									itemsCountBrush);
							}

							if (draggingItemFromSlot != null && draggingItemFromSlot.currentItem != null)
							{
								itemDestRect.X = GameController.Instance.mousePosition.X - 16;
								itemDestRect.Y = GameController.Instance.mousePosition.Y - 16;
								GameController.Instance.Render(
									MapController.Instance.itemsSheet,
									itemDestRect,
									draggingItemFromSlot.currentItem.itemSrcRect);
							}
						}
					break;
			}

			if (draggingItemFromSlot != null && 
				draggingItemFromSlot.currentItem != null && 
				!mouseWasInsideSlot)
			{
				if (GameController.Instance.mouseIsDown)
				{
					itemDestRect.X = GameController.Instance.mousePosition.X - 16;
					itemDestRect.Y = GameController.Instance.mousePosition.Y - 16;
					GameController.Instance.Render(
						MapController.Instance.uiSheet,
						itemDestRect,
						itemTrashIcon);
				}
				else
				{
					draggingItemFromSlot.TrashItem();
					draggingItemFromSlot = null;
					isDraggingItem = false;
				}
			}
		}
	}
}