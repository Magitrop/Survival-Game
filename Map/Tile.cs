using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Controllers;
using Game.GameObjects;
using Game.Miscellaneous;

namespace Game.Map
{
	public sealed class Tile
	{
		// [0 .. chunk size)
		public int x { get; private set; }
		// [0 .. chunk size)
		public int y { get; private set; }
		public int globalX
		{
			get => chunk.x * Constants.CHUNK_SIZE + x;
		}
		public int globalY
		{
			get => chunk.y * Constants.CHUNK_SIZE + y;
		}
		public Rectangle spriteSrc { get; private set; }
		public Chunk chunk;
		public GameObject gameObject { get; private set; }
		public int tileType;
		/// <summary>
		/// Уровень освещенности клетки.
		/// </summary>
		public byte lightingLevel;

		private Rectangle destRect;

		private static Rectangle[] srcRects = 
			new Rectangle[]
			{
				// 0 - глубокое море
				new Rectangle(12 * Constants.TEXTURE_RESOLUTION, 2 * Constants.TEXTURE_RESOLUTION, Constants.TEXTURE_RESOLUTION, Constants.TEXTURE_RESOLUTION),
				// 1 - море
				new Rectangle(13 * Constants.TEXTURE_RESOLUTION, 2 * Constants.TEXTURE_RESOLUTION, Constants.TEXTURE_RESOLUTION, Constants.TEXTURE_RESOLUTION),
				// 2 - мелководье
				new Rectangle(14 * Constants.TEXTURE_RESOLUTION, 2 * Constants.TEXTURE_RESOLUTION, Constants.TEXTURE_RESOLUTION, Constants.TEXTURE_RESOLUTION),
				// 3 - пляж
				new Rectangle(15 * Constants.TEXTURE_RESOLUTION, 2 * Constants.TEXTURE_RESOLUTION, Constants.TEXTURE_RESOLUTION, Constants.TEXTURE_RESOLUTION),
				// 4 - луг
				new Rectangle(16 * Constants.TEXTURE_RESOLUTION, 2 * Constants.TEXTURE_RESOLUTION, Constants.TEXTURE_RESOLUTION, Constants.TEXTURE_RESOLUTION),
				// 5 - холм
				new Rectangle(17 * Constants.TEXTURE_RESOLUTION, 2 * Constants.TEXTURE_RESOLUTION, Constants.TEXTURE_RESOLUTION, Constants.TEXTURE_RESOLUTION),
				// 6 - гора
				new Rectangle(18 * Constants.TEXTURE_RESOLUTION, 2 * Constants.TEXTURE_RESOLUTION, Constants.TEXTURE_RESOLUTION, Constants.TEXTURE_RESOLUTION),
				// 7 - заснеженная гора
				new Rectangle(19 * Constants.TEXTURE_RESOLUTION, 2 * Constants.TEXTURE_RESOLUTION, Constants.TEXTURE_RESOLUTION, Constants.TEXTURE_RESOLUTION),
				// 8 - деревянный пол
				new Rectangle(12 * Constants.TEXTURE_RESOLUTION, 3 * Constants.TEXTURE_RESOLUTION, Constants.TEXTURE_RESOLUTION, Constants.TEXTURE_RESOLUTION),
			};

		public static int GetTileTypePathPrice(int tileType)
        {
			switch (tileType)
            {
				case 0:
				case 1:
				case 2: return 1;
				case 3: return 3;
				case 4: return 3;
				case 5: return 4;
				case 6: return 5;
				case 7: return 7;
				case 8: return 1;
			}
			return 1;
        }

		public Tile(int _x, int _y, Chunk _chunk)
		{
			x = _x;
			y = _y;
			destRect = new Rectangle();
			chunk = _chunk;
			gameObject = null;
			destRect.Width = (int)Constants.TILE_SIZE;
			destRect.Height = (int)Constants.TILE_SIZE;
			tileType = 0;
		}

		public void SetGameObject(GameObject objectToSet)
		{
			/*if (gameObject != null)
				chunk.objects.Remove(gameObject);*/

			gameObject = objectToSet;

			/*if (gameObject != null)
				chunk.objects.Add(gameObject);*/
		}

		public void Render()
		{
			destRect.X = (int)((chunk.x * Constants.CHUNK_SIZE + x) * Constants.TILE_SIZE + MapController.Instance.camera.x);
			destRect.Y = (int)((chunk.y * Constants.CHUNK_SIZE + y) * Constants.TILE_SIZE + MapController.Instance.camera.y);

			GameController.Instance.Render(MapController.Instance.tilesSheet, destRect, srcRects[tileType]);
		}

		public void RenderLighting()
		{
			destRect.X = (int)((chunk.x * Constants.CHUNK_SIZE + x) * Constants.TILE_SIZE + MapController.Instance.camera.x);
			destRect.Y = (int)((chunk.y * Constants.CHUNK_SIZE + y) * Constants.TILE_SIZE + MapController.Instance.camera.y);

			GameController.Instance.Render(LightingController.Instance.lightingBrushes[255 - lightingLevel], destRect);
		}

		public void ResetLighting()
		{
			lightingLevel = LightingController.Instance.ambientLightingLevel;
		}
	}
}