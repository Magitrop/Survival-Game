using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.GameObjects;
using Game.Interfaces;
using Game.Map;
using Game.Miscellaneous;

namespace Game.Controllers
{
	public sealed class MapController : IBehaviour
	{
		// singleton
		private MapController() { }
		private static MapController instance;
		/// <summary>
		/// Осуществляет доступ к единственному экземпляру MapController.
		/// </summary>
		/// <returns></returns>
		public static MapController Instance
		{
            get
            {
				if (instance == null)
					instance = new MapController();
				return instance;
			}
		}

		public List<Chunk> visibleChunks;
		public Camera camera;

		public FastNoise noise { get; } = new FastNoise().SetNoiseType(FastNoise.NoiseType.SimplexFractal).SetFrequency(0.005f);

		/// <summary>
		/// Вызывается при создании карты (в начале игры).
		/// </summary>
		public void Start()
		{
			visibleChunks = new List<Chunk>();
			/*for (int x = -Constants.RENDER_DISTANCE; x <= Constants.RENDER_DISTANCE; x++)
				for (int y = -Constants.RENDER_DISTANCE; y <= Constants.RENDER_DISTANCE; y++)
				{
					visibleChunks.Add(new Chunk(x, y));
				}*/
		}
		/// <summary>
		/// Метод отрисовки карты и ее частей, выполняющийся в первую очередь.
		/// </summary>
		public void PreUpdate()
		{
			foreach (var chunk in visibleChunks)
			{
				chunk.Render();
			}
		}
		/// <summary>
		/// Метод отрисовки карты и ее частей, выполняющийся в последнюю очередь.
		/// </summary>
		public void PostUpdate()
		{
			foreach (var chunk in visibleChunks)
			{
				chunk.RenderLighting();
			}
		}

		public void RecalculateChunks(int chunkPosX, int chunkPosY)
		{
			for (int i = 0; i < visibleChunks.Count;)
			{
				if ((visibleChunks[i].x > chunkPosX + Constants.RENDER_DISTANCE) || 
					(visibleChunks[i].x < chunkPosX - Constants.RENDER_DISTANCE) ||
					(visibleChunks[i].y > chunkPosY + Constants.RENDER_DISTANCE) ||
					(visibleChunks[i].y < chunkPosY - Constants.RENDER_DISTANCE))
				{
					visibleChunks[i].UnloadChunk();
					visibleChunks.RemoveAt(i);
					continue;
				}
				i++;
			}

			for (int x = chunkPosX - Constants.RENDER_DISTANCE; x <= chunkPosX + Constants.RENDER_DISTANCE; x++)
				for (int y = chunkPosY - Constants.RENDER_DISTANCE; y <= chunkPosY + Constants.RENDER_DISTANCE; y++)
				{
					if (!visibleChunks.Any(c => c.x == x && c.y == y))
                    {
						int index = visibleChunks.FindIndex(c => c.x == x && c.y > y);
						if (index < 0)
							index = visibleChunks.Count - 1;
						Chunk chunk = new Chunk(x, y);
						visibleChunks.Insert(index < 0 ? 0 : index, chunk);
						chunk.Initialize();
						LightingController.Instance.GenerateLighting();
					}
				}

			for (int x = chunkPosX - Constants.RENDER_DISTANCE; x <= chunkPosX + Constants.RENDER_DISTANCE; x++)
				for (int y = chunkPosY - Constants.RENDER_DISTANCE; y <= chunkPosY + Constants.RENDER_DISTANCE; y++)
					visibleChunks.Find(c => c.x == x && c.y == y).SpawnCreatures();
		}

		public void Update() { }

		/// <summary>
		/// Рендерится ли в настоящий момент чанк, содержащий клетку с такими координатами?
		/// </summary>
		/// <param name="coordX"></param>
		/// <param name="coordY"></param>
		/// <returns></returns>
		public bool HasChunk(int coordX, int coordY)
		{
			if (coordX >= 0)
				coordX /= Constants.CHUNK_SIZE;
			else coordX = (coordX + 1) / Constants.CHUNK_SIZE - 1;

			if (coordY >= 0)
				coordY /= Constants.CHUNK_SIZE;
			else coordY = (coordY + 1) / Constants.CHUNK_SIZE - 1;

			return visibleChunks.Find(c => c.x == coordX && c.y == coordY) != null;
		}

		public Chunk GetChunk(int coordX, int coordY, bool createNewChunkIfNotFound = true)
		{
			if (coordX >= 0)
				coordX /= Constants.CHUNK_SIZE;
			else coordX = (coordX + 1) / Constants.CHUNK_SIZE - 1;

			if (coordY >= 0)
				coordY /= Constants.CHUNK_SIZE;
			else coordY = (coordY + 1) / Constants.CHUNK_SIZE - 1;

			Chunk result = visibleChunks.Find(c => c.x == coordX && c.y == coordY);
			if (result == null && createNewChunkIfNotFound)
			{
				int index = visibleChunks.FindIndex(c => c.x == coordX && c.y > coordY);
				if (index < 0)
					index = visibleChunks.Count - 1;
				visibleChunks.Insert(index < 0 ? 0 : index, result = new Chunk(coordX, coordY));
				result.Initialize();
			}
			return result;
		}
		
		public bool IsInChunk(int coordX, int coordY, int chunkX, int chunkY)
        {
			if (coordX >= 0)
				coordX /= Constants.CHUNK_SIZE;
			else coordX = (coordX + 1) / Constants.CHUNK_SIZE - 1;

			if (coordY >= 0)
				coordY /= Constants.CHUNK_SIZE;
			else coordY = (coordY + 1) / Constants.CHUNK_SIZE - 1;

			return coordX == chunkX && coordY == chunkY;
		}

		public Tile GetTile(int coordX, int coordY, bool createNewChunkIfNotFound = true)
		{
			return GetChunk(coordX, coordY, createNewChunkIfNotFound)?.GetTile(
				coordX >= 0 ? (coordX % Constants.CHUNK_SIZE) : (Constants.CHUNK_SIZE + (coordX % Constants.CHUNK_SIZE)) % Constants.CHUNK_SIZE,
				coordY >= 0 ? (coordY % Constants.CHUNK_SIZE) : (Constants.CHUNK_SIZE + (coordY % Constants.CHUNK_SIZE)) % Constants.CHUNK_SIZE);
		}

		public int GetTileType(int coordX, int coordY)
        {
			float height = noise.GetNoise(coordX, coordY) + 0.5f;
			if (height < 0.15f)
				return 0;
			else if (height < 0.3f)
				return 1;
			else if (height < 0.42f)
				return 2;
			else if (height < 0.55f)
				return 3;
			else if (height < 0.67f)
				return 4;
			else if (height < 0.8f)
				return 5;
			else if (height < 0.94f)
				return 6;
			else return 7;
		}

		public enum CheckIfTile
        {
			Exist = 1,
			IsPassable = 2
        }
		public bool CheckTile(int coordX, int coordY, CheckIfTile checkFor)
        {
			if ((checkFor & CheckIfTile.Exist) == CheckIfTile.Exist)
				if (!HasChunk(coordX, coordY))
					return false;
			if ((checkFor & CheckIfTile.IsPassable) == CheckIfTile.IsPassable)
            {
				foreach (var obj in GameController.Instance.objectsQueue)
					if (obj.x == coordX && obj.y == coordY)
						return false;

				SavedChunkInfo info;
				int _x = coordX >= 0 ? (coordX % Constants.CHUNK_SIZE) : (Constants.CHUNK_SIZE + (coordX % Constants.CHUNK_SIZE)) % Constants.CHUNK_SIZE;
				int _y = coordY >= 0 ? (coordY % Constants.CHUNK_SIZE) : (Constants.CHUNK_SIZE + (coordY % Constants.CHUNK_SIZE)) % Constants.CHUNK_SIZE;
				if ((info = GameController.Instance.savedChunks.Find(c => c.changedTiles.Any(t => t.coords == (byte)((_y << 3) + _x)))) != null)
				{
					var t = info.changedTiles.Find(c => c.coords == (byte)((_y << 3) + _x));
					// если на клетке есть объект
					if (t.additionalInformation.Length > 0 && t.additionalInformation[0] > 0)
						return false;
				}

				/*float height = noise.GetNoise(coordX, coordY) + 0.5f;
				Random r = new Random((coordX, coordY).GetHashCode());
				if (height >= 0.42f && height < 0.55f)
				{
					if (r.Next(100) < 3f)
						return false;
				}
				else if (height >= 0.55f && height < 0.67f)
                {
					if (r.Next(100) < 1f)
						return false;
					else
					if (r.Next(100) < 7f)
						return false;
				}
				else if (height >= 0.67f && height < 0.8f)
				{
					if (r.Next(100) < 1f)
						return false;
					else if (r.Next(100) < 4f)
						return false;
					else if (r.Next(100) < 25f)
						return false;
				}
				else if (height >= 0.8f && height < 0.94f)
				{
					if (r.Next(100) < 2f)
						return false;
					else if (r.Next(100) < 8f)
						return false;
					else if (r.Next(100) < 16f)
						return false;
				}*/
			}
			return true;
        }

		public List<(int, int)> FindPath((int x, int y) from, (int x, int y) to, WalkType canWalkOn, int maxDistance = 25)
        {
			if (MathOperations.Distance(from, to) > maxDistance)
				return null;

			List<(int, int)> path = new List<(int, int)>();

			// сама клетка - количество шагов
			var tiles = new Queue<(Tile tile, int stepsCount)>();
			var checkedTiles = new Dictionary<string, (Tile, int)>();
			tiles.Enqueue((GetTile(from.x, from.y), 0));
			while (tiles.Count > 0)
			{
				var t = tiles.Dequeue();
				if (!checkedTiles.ContainsKey(t.tile.globalX + ";" + t.tile.globalY) && 
					(t.tile?.gameObject == null || 
					t.tile?.gameObject == GetTile(to.x, to.y).gameObject ||
					t.tile?.gameObject == GetTile(from.x, from.y).gameObject) && 
					t.stepsCount <= maxDistance &&
					GameObject.CanStepOn(canWalkOn, t.tile.tileType))
				{
					checkedTiles.Add(t.tile.globalX + ";" + t.tile.globalY, (t.tile, t.stepsCount));
					tiles.Enqueue((GetTile(t.tile.globalX - 1, t.tile.globalY), t.stepsCount + 1));
					tiles.Enqueue((GetTile(t.tile.globalX + 1, t.tile.globalY), t.stepsCount + 1));
					tiles.Enqueue((GetTile(t.tile.globalX, t.tile.globalY - 1), t.stepsCount + 1));
					tiles.Enqueue((GetTile(t.tile.globalX, t.tile.globalY + 1), t.stepsCount + 1));
				}
			}

			(Tile, int)[] sideTiles = new (Tile, int)[4];
			if (!checkedTiles.TryGetValue(to.x + ";" + to.y, out (Tile, int) cur))
				return null;
			while (cur.Item2 > 0)
			{
				path.Add((cur.Item1.globalX, cur.Item1.globalY));
				GetChunk(cur.Item1.globalX, cur.Item1.globalY).UpdateTile(cur.Item1.x, cur.Item1.y);

				if (!checkedTiles.TryGetValue((cur.Item1.globalX - 1) + ";" + cur.Item1.globalY, out sideTiles[0])) sideTiles[0].Item2 = -1;
				if (!checkedTiles.TryGetValue((cur.Item1.globalX + 1) + ";" + cur.Item1.globalY, out sideTiles[1])) sideTiles[1].Item2 = -1;
				if (!checkedTiles.TryGetValue(cur.Item1.globalX + ";" + (cur.Item1.globalY - 1), out sideTiles[2])) sideTiles[2].Item2 = -1;
				if (!checkedTiles.TryGetValue(cur.Item1.globalX + ";" + (cur.Item1.globalY + 1), out sideTiles[3])) sideTiles[3].Item2 = -1;
				for (int i = 0; i < 4; i++)
					if (sideTiles[i].Item2 >= 0 && sideTiles[i].Item2 < cur.Item2)
						cur = sideTiles[i];
			}

			path.Reverse();
			return path;
		}
	}
}