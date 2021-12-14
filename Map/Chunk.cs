using Game.Controllers;
using Game.GameObjects;
using Game.GameObjects.Creatures;
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
		private int creaturesCount;

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
					Random r = new Random((tiles[_x, _y].globalX, tiles[_x, _y].globalY).GetHashCode());
					if (height < 0.15f)
						height = 0;
					else if (height < 0.3f)
						height = 1;
					else if (height < 0.42f)
						height = 2;
					else if (height < 0.55f)
					{
						height = 3;
						if (r.Next(100) < 3f)
							tiles[_x, _y].SetGameObject(GameObject.Spawn("obj_palm_tree", tiles[_x, _y].globalX, tiles[_x, _y].globalY));
					}
					else if (height < 0.67f)
					{
						height = 4;
						if (r.Next(100) < 1f)
							tiles[_x, _y].SetGameObject(GameObject.Spawn("obj_small_stone_lump", tiles[_x, _y].globalX, tiles[_x, _y].globalY));
						else
						if (r.Next(100) < 7f)
							tiles[_x, _y].SetGameObject(GameObject.Spawn("obj_pine_tree", tiles[_x, _y].globalX, tiles[_x, _y].globalY));
					}
					else if (height < 0.8f)
					{
						height = 5;
						if (r.Next(100) < 1f)
							tiles[_x, _y].SetGameObject(GameObject.Spawn("obj_stone_lump", tiles[_x, _y].globalX, tiles[_x, _y].globalY));
						else
						if (r.Next(100) < 4f)
							tiles[_x, _y].SetGameObject(GameObject.Spawn("obj_small_stone_lump", tiles[_x, _y].globalX, tiles[_x, _y].globalY));
						else 
						if (r.Next(100) < 25f)
							tiles[_x, _y].SetGameObject(GameObject.Spawn("obj_pine_tree", tiles[_x, _y].globalX, tiles[_x, _y].globalY));
					}
					else if (height < 0.94f)
                    {
						height = 6;
						if (r.Next(100) < 2f)
							tiles[_x, _y].SetGameObject(GameObject.Spawn("obj_pine_tree", tiles[_x, _y].globalX, tiles[_x, _y].globalY));
						else
						if (r.Next(100) < 8f)
							tiles[_x, _y].SetGameObject(GameObject.Spawn("obj_stone_lump", tiles[_x, _y].globalX, tiles[_x, _y].globalY));
						else
						if (r.Next(100) < 16f)
							tiles[_x, _y].SetGameObject(GameObject.Spawn("obj_small_stone_lump", tiles[_x, _y].globalX, tiles[_x, _y].globalY));
					}
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
			(int x, int y) t_coords;
			Tile tile;
			creaturesCount = 0;
			if ((info = GameController.Instance.savedChunks.Find(c => c.chunkX == x && c.chunkY == y)) != null)
			{
				int count = info.changedTiles.Count;
				SavedTileInfo t;
				for (int i = 0; i < count; i++)
				{
					t = info.changedTiles[i];
					t_coords = SavedChunkInfo.GetTile(t.coords);
					(tile = tiles[t_coords.x, t_coords.y]).tileType = t.type;
					// если на клетке есть объект и это не герой
					if (t.additionalInformation.Length == 0 || t.additionalInformation[0] == 0)
					{
						if (tile.gameObject?.objectID >= 100)
							tile.SetGameObject(null);
					}
					else if (t.additionalInformation[0] > 1)
						GameObject.Spawn(t.additionalInformation[0], tile.globalX, tile.globalY, t.additionalInformation);
				}
			}
		}

		public void SpawnCreatures()
        {
			foreach (var cr in GameController.Instance.objectsQueue.Where(o => o is Creature))
				if (MapController.Instance.IsInChunk(cr.x, cr.y, x, y))
					creaturesCount++;

			Tile[] darkTiles = tiles.Cast<Tile>().Where(t => t.lightingLevel < 100).ToArray();
			Random r = new Random((x, y).GetHashCode());
			for (int i = creaturesCount; darkTiles.Length > 0 && r.Next(0, 100) < 50 && i < r.Next(0, Constants.MAX_CREATURES_SPAWN_PER_CHUNK + 1); i++)
			{
				// spawn wolf or bear
				Tile tile = darkTiles[r.Next(0, Constants.CHUNK_SIZE)];
				if (tile.gameObject == null && tile.tileType >= 3)
				{
					GameController.Instance.Spawn((TurnBasedObject)GameObject.Spawn(r.Next(2, 4), tile.globalX, tile.globalY));
					creaturesCount++;
					break;
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

		public void RenderLighting()
		{
			for (int x = 0; x < Constants.CHUNK_SIZE; x++)
				for (int y = 0; y < Constants.CHUNK_SIZE; y++)
				{
					tiles[x, y].RenderLighting();
				}
		}

		public void ResetLighting()
		{
			for (int x = 0; x < Constants.CHUNK_SIZE; x++)
				for (int y = 0; y < Constants.CHUNK_SIZE; y++)
				{
					tiles[x, y].ResetLighting();
				}
		}

		public void RenderObjects()
		{
			for (int x = 0; x < Constants.CHUNK_SIZE; x++)
				for (int y = 0; y < Constants.CHUNK_SIZE; y++)
                {
					tiles[x, y].gameObject?.Render();
				}
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
				int index = info.changedTiles.FindIndex(t => ((t.coords & 7) == coordX) && (t.coords >> 3 == coordY));
				if (index != -1)
					info.changedTiles.RemoveAt(index);
				info.AddTile((byte)coordX, (byte)coordY, (byte)tile.tileType, tile.gameObject?.objectAdditionalInformation ?? new byte[]{ 0 });
			}
			LightingController.Instance.GenerateLighting();
		}
	}
}