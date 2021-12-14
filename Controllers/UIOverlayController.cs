using Game.GameObjects;
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
		private Rectangle hungerBarSrc;
		private Rectangle thirstBarSrc;
		private Rectangle backgroundBarSrc;
		private Rectangle healthBarTipSrc;
		private Rectangle staminaBarTipSrc;
		private Rectangle hungerBarTipSrc;
		private Rectangle thirstBarTipSrc;

		private Rectangle barDest;
		private Rectangle healthCount;
		private Rectangle staminaCount;
		private Rectangle hungerCount;
		private Rectangle thirstCount;
		private Rectangle barTipDest;

		public void Start()
		{
			barSrc = new Rectangle(0, 144, 128, 16);
			healthBarSrc = new Rectangle(0, 144 + 16, 128, 16);
			staminaBarSrc = new Rectangle(0, 144 + 16 * 2, 128, 16);
			backgroundBarSrc = new Rectangle(0, 144 + 16 * 3, 128, 16);
			hungerBarSrc = new Rectangle(0, 144 + 16 * 4, 128, 16);
			thirstBarSrc = new Rectangle(0, 144 + 16 * 5, 128, 16);

			healthBarTipSrc = new Rectangle(128, 144, 16, 16);
			staminaBarTipSrc = new Rectangle(144, 144, 16, 16);
			hungerBarTipSrc = new Rectangle(160, 144, 16, 16);
			thirstBarTipSrc = new Rectangle(176, 144, 16, 16);

			barDest = 
				new Rectangle
				(
					16, 
					16, 
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
			hungerCount =
				new Rectangle
				(
					16, 16 + 2 * InventoryController.Instance.slotSize / 2, 0, InventoryController.Instance.slotSize / 2
				);
			thirstCount =
				new Rectangle
				(
					16, 16 + 3 * InventoryController.Instance.slotSize / 2, 0, InventoryController.Instance.slotSize / 2
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
			barDest.Y = 16;

			// полоска здоровья
			healthCount.Width =
				(int)(barDest.Width
				/ 100f
				* Math.Min(
					GameController.Instance.mainHero.currentHealth 
					* 100f 
					/ GameController.Instance.mainHero.maxHealth, 100));
			barTipDest.X = barDest.X + barDest.Width;
			barTipDest.Y = barDest.Y;
			GameController.Instance.Render(
				Constants.uiSheet,
				barDest,
				backgroundBarSrc);
			GameController.Instance.Render(
				Constants.uiSheet,
				healthCount,
				healthBarSrc);
			GameController.Instance.Render(
				Constants.uiSheet,
				barDest,
				barSrc);
			GameController.Instance.Render(
				Constants.uiSheet,
				barTipDest,
				healthBarTipSrc);

			barDest.Y += InventoryController.Instance.slotSize / 2;
			// полоска выносливости
			barTipDest.X = barDest.X + barDest.Width;
			barTipDest.Y = barDest.Y;
			staminaCount.Width =
				(int)(barDest.Width 
				/ 100f
				* Math.Min(
					GameController.Instance.mainHero.actionsLeft 
					* 100f 
					/ GameController.Instance.mainHero.maxActionsCount, 100));
			GameController.Instance.Render(
				Constants.uiSheet,
				barDest,
				backgroundBarSrc);
			GameController.Instance.Render(
				Constants.uiSheet,
				staminaCount,
				staminaBarSrc);
			GameController.Instance.Render(
				Constants.uiSheet,
				barDest,
				barSrc);
			GameController.Instance.Render(
				Constants.uiSheet,
				barTipDest,
				staminaBarTipSrc);

			barDest.Y += InventoryController.Instance.slotSize / 2;
			// полоска голода
			barTipDest.X = barDest.X + barDest.Width;
			barTipDest.Y = barDest.Y;
			hungerCount.Width = (int)(barDest.Width / 100f * Math.Min((GameController.Instance.mainHero as GameObject.Hero).hunger, 100));
			GameController.Instance.Render(
				Constants.uiSheet,
				barDest,
				backgroundBarSrc);
			GameController.Instance.Render(
				Constants.uiSheet,
				hungerCount,
				hungerBarSrc);
			GameController.Instance.Render(
				Constants.uiSheet,
				barDest,
				barSrc);
			GameController.Instance.Render(
				Constants.uiSheet,
				barTipDest,
				hungerBarTipSrc);

			barDest.Y += InventoryController.Instance.slotSize / 2;
			// полоска жажды
			barTipDest.X = barDest.X + barDest.Width;
			barTipDest.Y = barDest.Y;
			thirstCount.Width = (int)(barDest.Width / 100f * Math.Min((GameController.Instance.mainHero as GameObject.Hero).thirst, 100));
			GameController.Instance.Render(
				Constants.uiSheet,
				barDest,
				backgroundBarSrc);
			GameController.Instance.Render(
				Constants.uiSheet,
				thirstCount,
				thirstBarSrc);
			GameController.Instance.Render(
				Constants.uiSheet,
				barDest,
				barSrc);
			GameController.Instance.Render(
				Constants.uiSheet,
				barTipDest,
				thirstBarTipSrc);
		}
	}
}
