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
	public sealed class CraftingController : IBehaviour
	{
		// singleton
		private CraftingController() { }
		private static CraftingController instance;
		/// <summary>
		/// Осуществляет доступ к единственному экземпляру CraftingController.
		/// </summary>
		/// <returns></returns>
		public static CraftingController Instance
		{
			get
			{
				if (instance == null)
					instance = new CraftingController();
				return instance;
			}
		}

		public enum CraftingDisplayMode
		{
			DoNotDisplay,           // не отображать вообще
			WithoutCraftingTable,   // без использования верстака
			WithCraftingTable,      // с использованием верстака
		}

		public sealed class Recipe
		{
			public (Item item, int count)[] ingredients { get; private set; }
			public (Item item, int count) result { get; private set; }
			public bool needsCraftingTable { get; private set; }

			public Recipe(
				(Item item, int count)[] _ingredients, 
				(Item item, int count) _result, 
				bool needsTable)
			{
				ingredients = _ingredients;
				result = _result;
				needsCraftingTable = needsTable;
			}
		}

		/// <summary>
		/// Рецепты, по которым игрок может создать предметы при текущем наборе предметов в инвентаре.
		/// </summary>
		public List<Recipe> availableRecipes { get; private set; }
		public int currentRecipeIndex;
		public Recipe currentRecipe
		{
			get
			{
				if (availableRecipes.Count == 0)
					return null;
				else if (currentRecipeIndex < 0)
					currentRecipeIndex = 0;
				else if (currentRecipeIndex >= availableRecipes.Count)
					currentRecipeIndex = availableRecipes.Count - 1;
				return availableRecipes[currentRecipeIndex];
			}
		}

		private Rectangle slotSrc;
		private Rectangle backgroundSrc;
		private Rectangle resultSlotSrc;
		private Rectangle resultSelectedSlotSrc;
		private Rectangle prevRecipeSrc;
		private Rectangle nextRecipeSrc;
		private Rectangle prevRecipeSelectedSrc;
		private Rectangle nextRecipeSelectedSrc;

		private Rectangle backgroundDest;
		private Rectangle itemDest;
		private Rectangle slotDest;
		private Rectangle prevRecipeDest;
		private Rectangle nextRecipeDest;

		private Font itemsCountFont;
		private Brush itemsCountBrush;

		public CraftingDisplayMode displayMode;

		/// <summary>
		/// Создает предмет по данному рецепту. Возвращает true в случае успешного создания.
		/// </summary>
		/// <param name="recipe"></param>
		/// <param name="withCraftingTable"></param>
		/// <returns></returns>
		public bool Craft(Recipe recipe, bool withCraftingTable)
		{
			if (recipe.needsCraftingTable && !withCraftingTable)
				return false;

			foreach (var i in recipe.ingredients)
				if (!InventoryController.Instance.HasEnoughItems(i.item, i.count))
					return false;

			foreach (var i in recipe.ingredients)
				InventoryController.Instance.WithdrawItems(i.item, i.count);
			InventoryController.Instance.ReceiveItems(recipe.result.item, recipe.result.count);
			return true;
		}
		public void GetAvailableRecipes() => availableRecipes = Recipes.GetAvailableRecipes();

		public void Start()
		{
			Recipes.InitializeRecipes();

			slotDest = new Rectangle();
			slotDest.Width = InventoryController.Instance.slotSize;
			slotDest.Height = InventoryController.Instance.slotSize;

			slotSrc = new Rectangle(0, 0, 32, 32);
			resultSlotSrc = new Rectangle(32, 0, 32, 32);
			resultSelectedSlotSrc = new Rectangle(64, 0, 32, 32);
			backgroundSrc = new Rectangle(0, 48, 256, 64);
			prevRecipeSrc = new Rectangle(0, 112, 16, 16);
			nextRecipeSrc = new Rectangle(16, 112, 16, 16);
			prevRecipeSelectedSrc = new Rectangle(0, 128, 16, 16);
			nextRecipeSelectedSrc = new Rectangle(16, 128, 16, 16);

			itemDest = new Rectangle();
			itemDest.Width = InventoryController.Instance.slotSize - 20;
			itemDest.Height = InventoryController.Instance.slotSize - 20;
			backgroundDest = 
				new Rectangle
				(
					(Constants.WINDOW_WIDTH - InventoryController.Instance.slotSize * 8 - 16) / 2,
					(Constants.WINDOW_HEIGHT - InventoryController.Instance.slotSize * 4) / 2, 
					8 * InventoryController.Instance.slotSize, 
					2 * InventoryController.Instance.slotSize
				);
			prevRecipeDest =
				new Rectangle
				(
					(Constants.WINDOW_WIDTH - InventoryController.Instance.slotSize * 8 - 16) / 2,
					(Constants.WINDOW_HEIGHT - InventoryController.Instance.slotSize) / 2,
					InventoryController.Instance.slotSize / 2,
					InventoryController.Instance.slotSize / 2
				);
			nextRecipeDest =
				new Rectangle
				(
					(Constants.WINDOW_WIDTH + InventoryController.Instance.slotSize * 7 - 16) / 2,
					(Constants.WINDOW_HEIGHT - InventoryController.Instance.slotSize) / 2,
					InventoryController.Instance.slotSize / 2,
					InventoryController.Instance.slotSize / 2
				);

			itemsCountFont = new Font(Fonts.fonts.Families[0], 25.0F, FontStyle.Bold);
			itemsCountBrush = new SolidBrush(Color.White);
		}
		public void PreUpdate() { }
		public void Update() { }
		public void PostUpdate()
		{
			if (displayMode == CraftingDisplayMode.DoNotDisplay || currentRecipe == null)
				return;

			GameController.Instance.Render(
				MapController.Instance.uiSheet,
				backgroundDest,
				backgroundSrc);

			if (currentRecipeIndex > 0)
            {
				if (MathOperations.IsPointInside(GameController.Instance.mousePosition, prevRecipeDest))
				{
					GameController.Instance.Render(
					MapController.Instance.uiSheet,
					prevRecipeDest,
					prevRecipeSelectedSrc);

					if (GameController.Instance.leftMouseButton)
					{
						currentRecipeIndex--;
						GameController.Instance.leftMouseButton = false;
					}
				}
				else
					GameController.Instance.Render(
					MapController.Instance.uiSheet,
					prevRecipeDest,
					prevRecipeSrc);
			}

			if (currentRecipeIndex < availableRecipes.Count - 1)
			{
				if (MathOperations.IsPointInside(GameController.Instance.mousePosition, nextRecipeDest))
				{
					GameController.Instance.Render(
					MapController.Instance.uiSheet,
					nextRecipeDest,
					nextRecipeSelectedSrc);

					if (GameController.Instance.leftMouseButton)
					{
						currentRecipeIndex++;
						GameController.Instance.leftMouseButton = false;
					}
				}
				else
					GameController.Instance.Render(
					MapController.Instance.uiSheet,
					nextRecipeDest,
					nextRecipeSrc);
			}

			int x = (Constants.WINDOW_WIDTH + InventoryController.Instance.slotSize - 16) / 2;
			int y = Constants.WINDOW_HEIGHT / 2 - InventoryController.Instance.slotSize * 3 / 2;
			for (int i = 0; i < currentRecipe.ingredients.Length; i++)
			{
				slotDest.X = x;
				slotDest.Y = y;
				GameController.Instance.Render(
					MapController.Instance.uiSheet, 
					slotDest, 
					slotSrc);

				itemDest.X = slotDest.X + 10;
				itemDest.Y = y + 10;
				GameController.Instance.Render(
					MapController.Instance.itemsSheet, 
					itemDest, 
					currentRecipe.ingredients[i].item.itemSrcRect);

				GameController.Instance.Render(
					currentRecipe.ingredients[i].count.ToString(),
					new Point(
						itemDest.X + (int)(55 - 10f * currentRecipe.ingredients[i].count.ToString().Length),
						itemDest.Y + 35),
					itemsCountFont,
					itemsCountBrush);

				x -= InventoryController.Instance.slotSize;
			}

			x = (Constants.WINDOW_WIDTH + InventoryController.Instance.slotSize * 5 - 16) / 2;
			slotDest.X = x;
			slotDest.Y = y;

			bool crafted = false;
			if (MathOperations.IsPointInside(GameController.Instance.mousePosition, slotDest))
			{
				GameController.Instance.Render(
					MapController.Instance.uiSheet,
					slotDest,
					resultSelectedSlotSrc);

				if (GameController.Instance.leftMouseButton)
				{
					crafted = Craft(currentRecipe, displayMode == CraftingDisplayMode.WithCraftingTable);
					GameController.Instance.leftMouseButton = false;
				}
			}
			else
				GameController.Instance.Render(
					MapController.Instance.uiSheet,
					slotDest,
					resultSlotSrc);

			itemDest.X = x + 10;
			itemDest.Y = y + 10;
			GameController.Instance.Render(
				MapController.Instance.itemsSheet,
				itemDest,
				currentRecipe.result.item.itemSrcRect);

			GameController.Instance.Render(
					currentRecipe.result.count.ToString(),
					new Point(
						itemDest.X + (int)(55 - 10f * currentRecipe.result.count.ToString().Length),
						itemDest.Y + 35),
					itemsCountFont,
					itemsCountBrush);

			if (crafted)
            {
				GetAvailableRecipes();
				if (availableRecipes.Count == 0)
					displayMode = CraftingDisplayMode.DoNotDisplay;
			}
		}
	}

	public static class Recipes
	{
		private static List<CraftingController.Recipe> allRecipes;
		public static void InitializeRecipes()
		{
			allRecipes = new List<CraftingController.Recipe>();

			// деревянная палка
			allRecipes.Add(
				new CraftingController.Recipe
				(
					new (Item, int)[]
					{
						(Items.ItemWoodenLog.Instance, 1),
					},
					(Items.ItemWoodenStick.Instance, 3),
					false)
				);

			// деревянный забор
			allRecipes.Add(
				new CraftingController.Recipe
				(
					new (Item, int)[]
					{ 
						(Items.ItemWoodenLog.Instance, 4),
					}, 
					(Items.ItemWoodenFence.Instance, 1),
					false)
				);

			// деревянная калитка
			allRecipes.Add(
				new CraftingController.Recipe
				(
					new (Item, int)[]
					{
						(Items.ItemWoodenLog.Instance, 5),
					},
					(Items.ItemWoodenFenceGate.Instance, 1),
					false)
				);

			// сундук
			allRecipes.Add(
				new CraftingController.Recipe
				(
					new (Item, int)[]
					{
						(Items.ItemWoodenLog.Instance, 20),
					},
					(Items.ItemChest.Instance, 1),
					false)
				);

			// каменный топор
			allRecipes.Add(
				new CraftingController.Recipe
				(
					new (Item, int)[]
					{
						(Items.ItemWoodenStick.Instance, 5),
						(Items.ItemCobblestone.Instance, 5),
					},
					(Items.ItemStoneAxe.Instance, 1),
					false)
				);
		}

		public static List<CraftingController.Recipe> GetAvailableRecipes() => 
			allRecipes.Where(
				r => r.ingredients.ToList().TrueForAll(
					i => InventoryController.Instance.HasEnoughItems(i.item, i.count))).ToList();
	}
}