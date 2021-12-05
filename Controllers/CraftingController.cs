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

		private Rectangle itemDestRect;
		private Rectangle slotDestRect;
		private Rectangle slotSrcRect;
		private Rectangle resultSlotSrcRect;
		private Rectangle resultSelectedSlotSrcRect;
		private Rectangle backgroundDestRect;
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

			slotDestRect = new Rectangle();
			slotDestRect.Width = InventoryController.Instance.slotSize;
			slotDestRect.Height = InventoryController.Instance.slotSize;

			slotSrcRect = new Rectangle(0, 0, 32, 32);
			resultSlotSrcRect = new Rectangle(32, 0, 32, 32);
			resultSelectedSlotSrcRect = new Rectangle(64, 0, 32, 32);

			itemDestRect = new Rectangle();
			itemDestRect.Width = InventoryController.Instance.slotSize - 20;
			itemDestRect.Height = InventoryController.Instance.slotSize - 20;

			itemsCountFont = new Font(Fonts.fonts.Families[0], 25.0F, FontStyle.Bold);
			itemsCountBrush = new SolidBrush(Color.White);
		}
		public void PreUpdate() { }
		public void Update() { }
		public void PostUpdate()
		{
			if (displayMode == CraftingDisplayMode.DoNotDisplay || currentRecipe == null)
				return;

			int x = (Constants.WINDOW_WIDTH - InventoryController.Instance.slotSize * 3 - 16) / 2;
			int y = Constants.WINDOW_HEIGHT / 2 - InventoryController.Instance.slotSize;
			for (int i = 0; i < currentRecipe.ingredients.Length; i++)
			{
				slotDestRect.X = x;
				slotDestRect.Y = y;
				GameController.Instance.Render(
					MapController.Instance.uiSheet, 
					slotDestRect, 
					slotSrcRect);

				itemDestRect.X = x + 10;
				itemDestRect.Y = y + 10;
				GameController.Instance.Render(
					MapController.Instance.itemsSheet, 
					itemDestRect, 
					currentRecipe.ingredients[i].item.itemSrcRect);

				x -= InventoryController.Instance.slotSize;
			}

			x = (Constants.WINDOW_WIDTH + InventoryController.Instance.slotSize - 16) / 2;
			slotDestRect.X = x;
			slotDestRect.Y = y;
			if (MathOperations.IsPointInside(GameController.Instance.mousePosition, slotDestRect))
			{
				GameController.Instance.Render(
					MapController.Instance.uiSheet,
					slotDestRect,
					resultSelectedSlotSrcRect);
				if (GameController.Instance.mouseIsDown)
				{
					Craft(currentRecipe, displayMode == CraftingDisplayMode.WithCraftingTable);
					GameController.Instance.mouseIsDown = false;
				}
			}
			else
				GameController.Instance.Render(
					MapController.Instance.uiSheet,
					slotDestRect,
					resultSlotSrcRect);

			itemDestRect.X = x + 10;
			itemDestRect.Y = y + 10;
			GameController.Instance.Render(
				MapController.Instance.itemsSheet,
				itemDestRect,
				currentRecipe.result.item.itemSrcRect);
		}
	}

	public static class Recipes
	{
		private static List<CraftingController.Recipe> allRecipes;
		public static void InitializeRecipes()
		{
			allRecipes = new List<CraftingController.Recipe>();

			// деревянный забор
			allRecipes.Add(
				new CraftingController.Recipe
				(
					new (Item, int)[]
					{ 
						(Items.ItemWoodenLog.Instance, 3),
						(Items.ItemStoneAxe.Instance, 3),
						(Items.ItemBonfire.Instance, 3),
					}, 
					(Items.ItemWoodenFence.Instance, 1),
					false)
				);
		}

		public static List<CraftingController.Recipe> GetAvailableRecipes() => 
			allRecipes.Where(
				r => r.ingredients.ToList().TrueForAll(
					i => InventoryController.Instance.HasEnoughItems(i.item, i.count))).ToList();
	}
}