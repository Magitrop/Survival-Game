using Game.GameObjects;
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

namespace Game.Controllers
{
	public sealed class LightingController
	{
		// singleton
		private LightingController() { }
		private static LightingController instance;
		public static LightingController Instance
		{
			get
			{
				if (instance == null)
                {
					instance = new LightingController();
					instance.lightingBrushes = new SolidBrush[256];
					for (int i = 0; i < instance.lightingBrushes.Length; i++)
						instance.lightingBrushes[i] = new SolidBrush(Color.FromArgb(i, 0, 0, 0));
				}
				return instance;
			}
		}

		/// <summary>
		/// Общий уровень освещенности (солнце и т.д.).
		/// </summary>
		public byte ambientLightingLevel { get; private set; }
		public Color lightingColor { get; private set; }
		/// <summary>
		/// Текущее время суток (в часах).
		/// </summary>
		public byte currentDaytime { get; private set; } = 12;

		public List<LightingObject> lightingObjects = new List<LightingObject>();
		public Brush[] lightingBrushes;

		public void SetCurrentDaytime(byte newDaytime)
        {
			currentDaytime = (byte)(newDaytime % 24);
			GenerateLighting();
        }

		public void GenerateLighting()
        {
			Task.Run(async () =>
			{
				for (int i = 0; i < MapController.Instance.visibleChunks.Count; i++)
					await Task.Run(() => { MapController.Instance.visibleChunks[i].ResetLighting(); });
			}).Wait();

			for (int i = 0; i < lightingObjects.Count; i++)
				if (GameController.Instance.mainHero != null && 
					MathOperations.Distance(lightingObjects[i].coords, GameController.Instance.mainHero.coords) < 20)
					lightingObjects[i].GenerateLighting();
		}

		/// <summary>
		/// Вызывает наступление следующей части дня.
		/// </summary>
		public void NextDayPart()
        {
			currentDaytime++;
			if (currentDaytime >= 24)
				currentDaytime = 0;

			switch (currentDaytime)
            {
				case 0:
					ambientLightingLevel = 30;
					lightingColor = Color.FromArgb(ambientLightingLevel, 0, 0, 30);
					break;
				case 1:
				case 23:
					ambientLightingLevel = 40;
					lightingColor = Color.FromArgb(ambientLightingLevel, 0, 0, 40);
					break;
				case 2:
				case 22:
					ambientLightingLevel = 50;
					lightingColor = Color.FromArgb(ambientLightingLevel, 0, 0, 50);
					break;
				case 3:
					ambientLightingLevel = 60;
					lightingColor = Color.FromArgb(ambientLightingLevel, 0, 10, 60);
					break;
				case 4:
				case 21:
					ambientLightingLevel = 75;
					lightingColor = Color.FromArgb(ambientLightingLevel, 0, 20, 70);
					break;
				case 5:
					ambientLightingLevel = 90;
					lightingColor = Color.FromArgb(ambientLightingLevel, 0, 20, 70);
					break;
				case 6:
				case 20:
					ambientLightingLevel = 110;
					lightingColor = Color.FromArgb(ambientLightingLevel, 40, 20, 30);
					break;
				case 7:
					ambientLightingLevel = 130;
					lightingColor = Color.FromArgb(ambientLightingLevel, 60, 30, 30);
					break;

				case 8:
				case 19:
					ambientLightingLevel = 155;
					lightingColor = Color.FromArgb(ambientLightingLevel, 60, 30, 30);
					break;
				case 9:
				case 18:
					ambientLightingLevel = 180;
					lightingColor = Color.FromArgb(ambientLightingLevel, 60, 30, 30);
					break;
				case 10:
				case 17:
					ambientLightingLevel = 210;
					lightingColor = Color.FromArgb(ambientLightingLevel, 60, 30, 30);
					break;
				case 11:
				case 16:
					ambientLightingLevel = 240;
					lightingColor = Color.FromArgb(ambientLightingLevel, 60, 30, 30);
					break;
				case 12:
				case 13:
				case 14:
				case 15:
					ambientLightingLevel = 255;
					lightingColor = Color.FromArgb(ambientLightingLevel, 60, 30, 30);
					break;
			}
			GenerateLighting();
		}
    }
}