using Game.Controllers;
using Game.GameObjects;
using Game.Miscellaneous;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Map
{
	public sealed class Chunk
	{
		public int x { get; private set; }
		public int y { get; private set; }
		public Tile[,] tiles { get; private set; }

		public Chunk(int chunkX, int chunkY)
		{
			x = chunkX;
			y = chunkY;
			tiles = new Tile[Constants.CHUNK_SIZE, Constants.CHUNK_SIZE];
		}

		public void Initialize()
        {
			float height;
			for (int _x = 0; _x < Constants.CHUNK_SIZE; _x++)
				for (int _y = 0; _y < Constants.CHUNK_SIZE; _y++)
				{
					tiles[_x, _y] = new Tile(_x, _y, this);
					height = MapController.Instance.noise.GetNoise(tiles[_x, _y].globalX, tiles[_x, _y].globalY) + 0.5f;
					if (height < 0.15f)
						height = 0;
					else if (height < 0.3f)
						height = 1;
					else if (height < 0.42f)
						height = 2;
					else if (height < 0.55f)
						height = 3;
					else if (height < 0.67f)
					{
						height = 4;
						if (MapController.Instance.noise.GetNoise(tiles[_x, _y].globalX * 10, tiles[_x, _y].globalY * 10) + 0.5f < 0.25f)
							tiles[_x, _y].SetGameObject(GameObject.Spawn("obj_tree", tiles[_x, _y].globalX, tiles[_x, _y].globalY));
					}
					else if (height < 0.8f)
						height = 5;
					else if (height < 0.94f)
						height = 6;
					else height = 7;
					tiles[_x, _y].tileType = (int)height;
					foreach (var obj in GameController.Instance.objectsQueue)
					{
						if (obj.x == tiles[_x, _y].globalX && obj.y == tiles[_x, _y].globalY)
							tiles[_x, _y].SetGameObject(obj);
					}
				}

			// загрузка сохраненных клеток
			SavedChunkInfo info;
			(int, int) t_coords;
			Tile tile;
			if ((info = GameController.Instance.savedChunks.Find(c => c.chunkX == x && c.chunkY == y)) != null)
			{
				int count = info.changedTiles.Count;
				(byte, byte, byte) t;
				for (int i = 0; i < count; i++)
				{
					t = info.changedTiles[i];
					t_coords = info.GetTile(t.Item1);
					(tile = tiles[t_coords.Item1, t_coords.Item2]).tileType = t.Item2;
					// если на клетке есть объект и это не герой
					if (t.Item3 == 0)
						tile.SetGameObject(null);
					else if (t.Item3 > 1)
						GameObject.Spawn(t.Item3, tile.globalX, tile.globalY);
				}
			}
		}

		public void UnloadChunk()
		{
			foreach (Tile t in tiles)
				t.chunk = null;
		}

		public void Render()
		{
			for (int x = 0; x < Constants.CHUNK_SIZE; x++)
				for (int y = 0; y < Constants.CHUNK_SIZE; y++)
				{
					tiles[x, y].Render();
				}
		}

		public void RenderObjects()
		{
			for (int x = 0; x < Constants.CHUNK_SIZE; x++)
				for (int y = 0; y < Constants.CHUNK_SIZE; y++)
					if (tiles[x, y].gameObject != null)
						tiles[x, y].gameObject.Render();
		}

		public Tile GetTile(int coordX, int coordY)
		{
			if (coordX < 0 || coordX >= Constants.CHUNK_SIZE ||
				coordY < 0 || coordY >= Constants.CHUNK_SIZE)
				return null;
			return tiles[coordX, coordY];
		}

		public void UpdateTile(int coordX, int coordY)
		{
			Tile tile;
			if ((tile = GetTile(coordX, coordY)) != null)
			{
				SavedChunkInfo info;
				if ((info = GameController.Instance.savedChunks.Find(c => c.chunkX == x && c.chunkY == y)) == null)
					GameController.Instance.savedChunks.Add(info = new SavedChunkInfo(x, y));
				// если среди измененных клеток пока что нет клетки с такими координатами
				int index = info.changedTiles.FindIndex(t => ((t.Item1 & 7) == coordX) && (t.Item1 >> 3 == coordY));
				if (index != -1)
					info.changedTiles.RemoveAt(index);
				info.AddTile((byte)coordX, (byte)coordY, (byte)tile.tileType, (byte)(tile.gameObject?.objectID ?? 0));
			}
		}
	}
}