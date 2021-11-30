using Game.Controllers;
using Game.Map;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.GameObjects
{
	public abstract class LightingObject : BreakableObject
	{
		protected LightingObject(
			int _x,
			int _y,
			int ID,
			string name,
			Image _sprite,
			byte[] additionalInformation = null) : base(_x, _y, ID, name, _sprite, additionalInformation)
		{
			if (additionalInformation != null && additionalInformation.Length > 1)
				durability = additionalInformation[1];
			else
				durability = 1;

			LightingController.Instance.lightingObjects.Add(this);
		}

		// сама клетка - количество шагов
		Queue<(Tile tile, int stepsCount)> tiles = new Queue<(Tile tile, int stepsCount)>();
		Dictionary<string, (Tile, int)> checkedTiles = new Dictionary<string, (Tile, int)>();
		public void GenerateLighting()
		{
			tiles.Clear();
			checkedTiles.Clear();

			tiles.Enqueue((MapController.Instance.GetTile(x, y), 0));
			while (tiles.Count > 0)
			{
				var t = tiles.Dequeue();
				if (!checkedTiles.ContainsKey(t.tile.globalX + ";" + t.tile.globalY) &&
					t.stepsCount <= 4)
				{
					MapController.Instance.GetTile(t.tile.globalX, t.tile.globalY).lightingLevel = 
						new List<byte>() 
						{
							MapController.Instance.GetTile(t.tile.globalX, t.tile.globalY).lightingLevel,
							(byte)((5 - t.stepsCount) * 51),
							LightingController.Instance.ambientLightingLevel 
						}.Max();

					checkedTiles.Add(t.tile.globalX + ";" + t.tile.globalY, (t.tile, t.stepsCount));
					if ((t.tile?.gameObject as BreakableObject) == null || t.tile == MapController.Instance.GetTile(x, y))
					{
						tiles.Enqueue((MapController.Instance.GetTile(t.tile.globalX - 1, t.tile.globalY), t.stepsCount + 1));
						tiles.Enqueue((MapController.Instance.GetTile(t.tile.globalX + 1, t.tile.globalY), t.stepsCount + 1));
						tiles.Enqueue((MapController.Instance.GetTile(t.tile.globalX, t.tile.globalY - 1), t.stepsCount + 1));
						tiles.Enqueue((MapController.Instance.GetTile(t.tile.globalX, t.tile.globalY + 1), t.stepsCount + 1));
						tiles.Enqueue((MapController.Instance.GetTile(t.tile.globalX - 1, t.tile.globalY - 1), t.stepsCount + 1));
						tiles.Enqueue((MapController.Instance.GetTile(t.tile.globalX + 1, t.tile.globalY + 1), t.stepsCount + 1));
						tiles.Enqueue((MapController.Instance.GetTile(t.tile.globalX + 1, t.tile.globalY - 1), t.stepsCount + 1));
						tiles.Enqueue((MapController.Instance.GetTile(t.tile.globalX - 1, t.tile.globalY + 1), t.stepsCount + 1));
					}
				}
			}
		}
	}
}