using Game.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Controllers
{
	public sealed class UIOverlayController : IBehaviour
	{
		// singleton
		private UIOverlayController() { }
		private static UIOverlayController instance;
		public static UIOverlayController Instance
		{
			get
			{
				if (instance == null)
					instance = new UIOverlayController();
				return instance;
			}
		}

		private Rectangle barSrc;
		private Rectangle healthBarSrc;
		private Rectangle staminaBarSrc;
		private Rectangle backgroundBarSrc;
		private Rectangle healthBarTipSrc;
		private Rectangle staminaBarTipSrc;

		private Rectangle healthBarDest;
		private Rectangle staminaBarDest;
		private Rectangle healthCount;
		private Rectangle staminaCount;
		private Rectangle barTipDest;

		public void Start()
		{
			barSrc = new Rectangle(0, 144, 128, 16);
			healthBarSrc = new Rectangle(0, 160, 128, 16);
			staminaBarSrc = new Rectangle(0, 176, 128, 16);
			backgroundBarSrc = new Rectangle(0, 192, 128, 16);
			healthBarTipSrc = new Rectangle(128, 144, 16, 16);
			staminaBarTipSrc = new Rectangle(144, 144, 16, 16);

			healthBarDest = 
				new Rectangle
				(
					16, 
					16, 
					InventoryController.Instance.slotSize * 4, 
					InventoryController.Instance.slotSize / 2
				);
			staminaBarDest = 
				new Rectangle
				(
					16, 
					16 + InventoryController.Instance.slotSize / 2, 
					InventoryController.Instance.slotSize * 4, 
					InventoryController.Instance.slotSize / 2
				);
			barTipDest = new Rectangle(0, 0, InventoryController.Instance.slotSize / 2, InventoryController.Instance.slotSize / 2);

			healthCount =
				new Rectangle
				(
					16, 16, 0, InventoryController.Instance.slotSize / 2
				);
			staminaCount =
				new Rectangle
				(
					16, 16 + InventoryController.Instance.slotSize / 2, 0, InventoryController.Instance.slotSize / 2
				);
		}

		public void PreUpdate()
		{
		}

		public void Update()
		{
		}

		public void PostUpdate()
		{
			// полоска здоровья
			healthCount.Width =
				(int)(healthBarDest.Width
				/ 100f
				* Math.Min(GameController.Instance.mainHero.currentHealth * 100f / GameController.Instance.mainHero.maxHealth, 100));
			barTipDest.X = healthBarDest.X + healthBarDest.Width;
			barTipDest.Y = healthBarDest.Y;
			GameController.Instance.Render(
				MapController.Instance.uiSheet,
				healthBarDest,
				backgroundBarSrc);
			GameController.Instance.Render(
				MapController.Instance.uiSheet,
				healthCount,
				healthBarSrc);
			GameController.Instance.Render(
				MapController.Instance.uiSheet,
				healthBarDest,
				barSrc);
			GameController.Instance.Render(
				MapController.Instance.uiSheet,
				barTipDest,
				healthBarTipSrc);

			// полоска выносливости
			barTipDest.X = staminaBarDest.X + staminaBarDest.Width;
			barTipDest.Y = staminaBarDest.Y;
			staminaCount.Width =
				(int)(staminaBarDest.Width 
				/ 100f
				* Math.Min(GameController.Instance.mainHero.actionsLeft * 100f / GameController.Instance.mainHero.maxActionsCount, 100));
			GameController.Instance.Render(
				MapController.Instance.uiSheet,
				staminaBarDest,
				backgroundBarSrc);
			GameController.Instance.Render(
				MapController.Instance.uiSheet,
				staminaCount,
				staminaBarSrc);
			GameController.Instance.Render(
				MapController.Instance.uiSheet,
				staminaBarDest,
				barSrc);
			GameController.Instance.Render(
				MapController.Instance.uiSheet,
				barTipDest,
				staminaBarTipSrc);
		}
	}
}
