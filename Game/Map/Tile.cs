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

		private Rectangle destRect;

		private static Rectangle[] srcRects = 
			new Rectangle[]
			{
				new Rectangle(12 * Constants.TEXTURE_RESOLUTION, 2 * Constants.TEXTURE_RESOLUTION, Constants.TEXTURE_RESOLUTION, Constants.TEXTURE_RESOLUTION),
				new Rectangle(13 * Constants.TEXTURE_RESOLUTION, 2 * Constants.TEXTURE_RESOLUTION, Constants.TEXTURE_RESOLUTION, Constants.TEXTURE_RESOLUTION),
				new Rectangle(14 * Constants.TEXTURE_RESOLUTION, 2 * Constants.TEXTURE_RESOLUTION, Constants.TEXTURE_RESOLUTION, Constants.TEXTURE_RESOLUTION),
				new Rectangle(15 * Constants.TEXTURE_RESOLUTION, 2 * Constants.TEXTURE_RESOLUTION, Constants.TEXTURE_RESOLUTION, Constants.TEXTURE_RESOLUTION),
				new Rectangle(16 * Constants.TEXTURE_RESOLUTION, 2 * Constants.TEXTURE_RESOLUTION, Constants.TEXTURE_RESOLUTION, Constants.TEXTURE_RESOLUTION),
				new Rectangle(17 * Constants.TEXTURE_RESOLUTION, 2 * Constants.TEXTURE_RESOLUTION, Constants.TEXTURE_RESOLUTION, Constants.TEXTURE_RESOLUTION),
				new Rectangle(18 * Constants.TEXTURE_RESOLUTION, 2 * Constants.TEXTURE_RESOLUTION, Constants.TEXTURE_RESOLUTION, Constants.TEXTURE_RESOLUTION),
				new Rectangle(19 * Constants.TEXTURE_RESOLUTION, 2 * Constants.TEXTURE_RESOLUTION, Constants.TEXTURE_RESOLUTION, Constants.TEXTURE_RESOLUTION),
				new Rectangle(12 * Constants.TEXTURE_RESOLUTION, 3 * Constants.TEXTURE_RESOLUTION, Constants.TEXTURE_RESOLUTION, Constants.TEXTURE_RESOLUTION)
			};

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

		private float curTimer;
		private int curAnimationFrame = 0;
		private Rectangle[] srcAnimRects = 
			new Rectangle[3]
			{
				new Rectangle(14 * Constants.TEXTURE_RESOLUTION, 2 * Constants.TEXTURE_RESOLUTION, Constants.TEXTURE_RESOLUTION, Constants.TEXTURE_RESOLUTION),
				new Rectangle(14 * Constants.TEXTURE_RESOLUTION, 3 * Constants.TEXTURE_RESOLUTION, Constants.TEXTURE_RESOLUTION, Constants.TEXTURE_RESOLUTION),
				new Rectangle(14 * Constants.TEXTURE_RESOLUTION, 4 * Constants.TEXTURE_RESOLUTION, Constants.TEXTURE_RESOLUTION, Constants.TEXTURE_RESOLUTION),
			};
		public void Render()
		{
			curTimer += Time.deltaTime;
			if (curTimer > 0.5f)
			{
				curTimer = 0;
				if (++curAnimationFrame > 2)
					curAnimationFrame = 0;
			}

			destRect.X = (int)((chunk.x * Constants.CHUNK_SIZE + x) * Constants.TILE_SIZE + MapController.Instance.camera.x);
			destRect.Y = (int)((chunk.y * Constants.CHUNK_SIZE + y) * Constants.TILE_SIZE + MapController.Instance.camera.y);
			//if (tileType != 2)
				GameController.Instance.Render(MapController.Instance.tilesSheet, destRect, srcRects[tileType]);
			//else GameController.Instance.Render(MapController.Instance.tilesSheet, destRect, srcAnimRects[curAnimationFrame]);

			if (gameObject != null)
				gameObject.Render();
		}
	}
}