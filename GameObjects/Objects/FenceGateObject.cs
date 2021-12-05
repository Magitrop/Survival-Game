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
	public abstract partial class GameObject
	{
		private sealed class FenceGateObject : BreakableObject
		{
			public FenceGateObject(
				int _x, 
				int _y, 
				byte[] additionalInformation = null) : base(_x, _y, 102, "obj_fence_gate", MapController.Instance.objectsSheet, additionalInformation)
			{
				destRect = new Rectangle(0, 0, (int)Constants.TILE_SIZE, (int)Constants.TILE_SIZE);

				// калитка повернута боком
				if (additionalInformation != null && additionalInformation.Length > 2 && additionalInformation[2] == 1)
					srcRect = new Rectangle(16 * 4, 16 * 3, 16, 16);
				else 
					srcRect = new Rectangle(16 * 4, 16 * 2, 16, 16);

				isDespawnable = true;

				Start();
			}

            protected override void OnSpawn()
            {
                base.OnSpawn();
				FindRoom();
			}

			private int fillTileType = 8;
            public void FindRoom()
			{
				/*
				t r d l
				0 0 0 1
				0 0 1 0
				0 1 0 0
				1 0 0 0
				. . . .
				*/
				byte walls = 0;
				if (MapController.Instance.GetTile(x, y - 1)?.gameObject?.objectName == "obj_wall")
					walls |= 8;
				if (MapController.Instance.GetTile(x + 1, y)?.gameObject?.objectName == "obj_wall")
					walls |= 4;
				if (MapController.Instance.GetTile(x, y + 1)?.gameObject?.objectName == "obj_wall")
					walls |= 2;
				if (MapController.Instance.GetTile(x - 1, y)?.gameObject?.objectName == "obj_wall")
					walls |= 1;

				void FloodFill(Point from, List<Tile> perimeter, Point min, Point max)
				{
					int targetColor = fillTileType;
					int tempColor = -1;
					Stack<Point> pixels = new Stack<Point>();
					pixels.Push(new Point(from.X, from.Y));

					List<GameObject> perimeterObjects = new List<GameObject>();
					foreach (var t in perimeter)
						if (t.gameObject != null)
							perimeterObjects.Add(t.gameObject);

					List<Tile> coloredTiles = new List<Tile>();
					while (pixels.Count > 0)
					{
						Point a = pixels.Pop();
						if (a.X < min.X || a.X > max.X ||
							a.Y < min.Y || a.Y > max.Y)
							continue;
						Tile t = MapController.Instance.GetTile(a.X, a.Y);
						if (t != null && t.tileType != tempColor && (t.gameObject == null || !perimeterObjects.Contains(t.gameObject)))
						{
							t.tileType = tempColor;
							coloredTiles.Add(t);
							pixels.Push(new Point(a.X - 1, a.Y));
							pixels.Push(new Point(a.X + 1, a.Y));
							pixels.Push(new Point(a.X, a.Y - 1));
							pixels.Push(new Point(a.X, a.Y + 1));
						}
					}

					int count = coloredTiles.Count;
					Tile tile;
					for (int i = 0; i < count; i++)
                    {
						tile = coloredTiles[i];
						tile.tileType = targetColor;
						MapController.Instance.GetChunk(tile.globalX, tile.globalY).UpdateTile(tile.x, tile.y);
					}
				}
				bool CheckTileInsideRoom(Point at, bool isPlacedVertical, List<Tile> perimeter, Point min, Point max)
				{
					/*if (MapController.Instance.GetTile(at.X, at.Y)?.gameObject != null)
						return false;*/

					if (at.X <= min.X || at.X >= max.X ||
						at.Y <= min.Y || at.Y >= max.Y)
						return false;

					bool result = false;
					if (isPlacedVertical)
					{
						for (int _x = at.X; _x <= max.X; _x++)
						{
							if (MapController.Instance.GetTile(_x, at.Y)?.gameObject?.objectName == "obj_wall" &&
								perimeter.Any(t => t.globalX == _x && t.globalY == at.Y))
							{
								if (!(MapController.Instance.GetTile(_x - 1, at.Y)?.gameObject?.objectName == "obj_wall" &&
									perimeter.Any(t => t.globalX == _x - 1 && t.globalY == at.Y)))
									result = !result;
							}
						}
						if (!result)
							for (int _x = at.X; _x >= min.X; _x--)
							{
								if (MapController.Instance.GetTile(_x, at.Y)?.gameObject?.objectName == "obj_wall" &&
									perimeter.Any(t => t.globalX == _x && t.globalY == at.Y))
								{
									if (!(MapController.Instance.GetTile(_x + 1, at.Y)?.gameObject?.objectName == "obj_wall" &&
										perimeter.Any(t => t.globalX == _x + 1 && t.globalY == at.Y)))
										result = !result;
								}
							}
					}
					else
					{
						if (!result)
							for (int _y = at.Y; _y <= max.Y; _y++)
							{
								if (MapController.Instance.GetTile(at.X, _y)?.gameObject?.objectName == "obj_wall" &&
									perimeter.Any(t => t.globalX == at.X && t.globalY == _y))
								{
									if (!(MapController.Instance.GetTile(at.X, _y - 1)?.gameObject?.objectName == "obj_wall" &&
										perimeter.Any(t => t.globalX == at.X && t.globalY == _y - 1)))
										result = !result;
								}
							}
						if (!result)
							for (int _y = at.Y; _y >= min.Y; _y--)
							{
								if (MapController.Instance.GetTile(at.X, _y)?.gameObject?.objectName == "obj_wall" &&
									perimeter.Any(t => t.globalX == at.X && t.globalY == _y))
								{
									if (!(MapController.Instance.GetTile(at.X, _y + 1)?.gameObject?.objectName == "obj_wall" &&
										perimeter.Any(t => t.globalX == at.X && t.globalY == _y + 1)))
										result = !result;
								}
							}
					}

					return result;
				}

				Tile to;
				// сама клетка - количество шагов
				var tiles = new Queue<(Tile, int)>();
				var checkedTiles = new Dictionary<string, (Tile, int)>();
				if (walls == 10)
				{
					// стены сверху и снизу
					// конечная точка - стена сверху
					to = MapController.Instance.GetTile(x, y - 1);
					// начало со стены снизу
					tiles.Enqueue((MapController.Instance.GetTile(x, y + 1), 0));
					while (tiles.Count > 0)
					{
						var t = tiles.Dequeue();
						if (!checkedTiles.ContainsKey(t.Item1.globalX + ";" + t.Item1.globalY) && t.Item1?.gameObject?.objectName == "obj_wall")
						{
							checkedTiles.Add(t.Item1.globalX + ";" + t.Item1.globalY, (t.Item1, t.Item2));
							tiles.Enqueue((MapController.Instance.GetTile(t.Item1.globalX - 1, t.Item1.globalY), t.Item2 + 1));
							tiles.Enqueue((MapController.Instance.GetTile(t.Item1.globalX + 1, t.Item1.globalY), t.Item2 + 1));
							tiles.Enqueue((MapController.Instance.GetTile(t.Item1.globalX, t.Item1.globalY - 1), t.Item2 + 1));
							tiles.Enqueue((MapController.Instance.GetTile(t.Item1.globalX, t.Item1.globalY + 1), t.Item2 + 1));
						}
					}

					Point _min = new Point(int.MaxValue - 1, int.MaxValue - 1);
					Point _max = new Point(int.MinValue + 1, int.MinValue + 1);
					List<Tile> _perimeter = new List<Tile>();
					(Tile, int)[] sideTiles = new (Tile, int)[4];
					checkedTiles.TryGetValue(to.globalX + ";" + to.globalY, out (Tile, int) cur);
					while (cur.Item2 > 0)
					{
						if (cur.Item1.globalX > _max.X) _max.X = cur.Item1.globalX;
						if (cur.Item1.globalY > _max.Y) _max.Y = cur.Item1.globalY;
						if (cur.Item1.globalX < _min.X) _min.X = cur.Item1.globalX;
						if (cur.Item1.globalY < _min.Y) _min.Y = cur.Item1.globalY;

						_perimeter.Add(cur.Item1);
						cur.Item1.tileType = 8;
						MapController.Instance.GetChunk(cur.Item1.globalX, cur.Item1.globalY).UpdateTile(cur.Item1.x, cur.Item1.y);

						if (!checkedTiles.TryGetValue((cur.Item1.globalX - 1) + ";" + cur.Item1.globalY, out sideTiles[0])) sideTiles[0].Item2 = -1;
						if (!checkedTiles.TryGetValue((cur.Item1.globalX + 1) + ";" + cur.Item1.globalY, out sideTiles[1])) sideTiles[1].Item2 = -1;
						if (!checkedTiles.TryGetValue(cur.Item1.globalX + ";" + (cur.Item1.globalY - 1), out sideTiles[2])) sideTiles[2].Item2 = -1;
						if (!checkedTiles.TryGetValue(cur.Item1.globalX + ";" + (cur.Item1.globalY + 1), out sideTiles[3])) sideTiles[3].Item2 = -1;
						for (int i = 0; i < 4; i++)
							if (sideTiles[i].Item1?.gameObject?.objectName == "obj_wall" && sideTiles[i].Item2 < cur.Item2)
								cur = sideTiles[i];
					}

					if (cur.Item1 != null)
					{
						_perimeter.Add(cur.Item1);
						cur.Item1.tileType = 8;
						MapController.Instance.GetChunk(cur.Item1.globalX, cur.Item1.globalY).UpdateTile(cur.Item1.x, cur.Item1.y);
						_perimeter.Add(MapController.Instance.GetTile(x, y));

						if (CheckTileInsideRoom(new Point(x + 1, y), false, _perimeter, _min, _max))
							FloodFill(new Point(x + 1, y), _perimeter, _min, _max);
						else //if (CheckTileInsideRoom(new Point(x - 1, y), false, _perimeter, _min, _max))
							FloodFill(new Point(x - 1, y), _perimeter, _min, _max);
						Tile t = MapController.Instance.GetTile(x, y);
						t.tileType = fillTileType;
						MapController.Instance.GetChunk(t.globalX, t.globalY).UpdateTile(t.x, t.y);
					}
				}
				// стены справа и слева
				else if (walls == 5)
				{
					// стены справа и слева
					// конечная точка - стена слева
					to = MapController.Instance.GetTile(x - 1, y);
					// начало со стены справа
					tiles.Enqueue((MapController.Instance.GetTile(x + 1, y), 0));
					while (tiles.Count > 0)
					{
						var t = tiles.Dequeue();
						if (!checkedTiles.ContainsKey(t.Item1.globalX + ";" + t.Item1.globalY) && t.Item1?.gameObject?.objectName == "obj_wall")
						{
							checkedTiles.Add(t.Item1.globalX + ";" + t.Item1.globalY, (t.Item1, t.Item2));
							tiles.Enqueue((MapController.Instance.GetTile(t.Item1.globalX - 1, t.Item1.globalY), t.Item2 + 1));
							tiles.Enqueue((MapController.Instance.GetTile(t.Item1.globalX + 1, t.Item1.globalY), t.Item2 + 1));
							tiles.Enqueue((MapController.Instance.GetTile(t.Item1.globalX, t.Item1.globalY - 1), t.Item2 + 1));
							tiles.Enqueue((MapController.Instance.GetTile(t.Item1.globalX, t.Item1.globalY + 1), t.Item2 + 1));
						}
					}

					Point _min = new Point(int.MaxValue - 1, int.MaxValue - 1);
					Point _max = new Point(int.MinValue + 1, int.MinValue + 1);
					List<Tile> _perimeter = new List<Tile>();
					(Tile, int)[] sideTiles = new (Tile, int)[4];
					checkedTiles.TryGetValue(to.globalX + ";" + to.globalY, out (Tile, int) cur);
					while (cur.Item2 > 0)
					{
						if (cur.Item1.globalX > _max.X) _max.X = cur.Item1.globalX;
						if (cur.Item1.globalY > _max.Y) _max.Y = cur.Item1.globalY;
						if (cur.Item1.globalX < _min.X) _min.X = cur.Item1.globalX;
						if (cur.Item1.globalY < _min.Y) _min.Y = cur.Item1.globalY;

						_perimeter.Add(cur.Item1);
						cur.Item1.tileType = 8;
						MapController.Instance.GetChunk(cur.Item1.globalX, cur.Item1.globalY).UpdateTile(cur.Item1.x, cur.Item1.y);

						if (!checkedTiles.TryGetValue((cur.Item1.globalX - 1) + ";" + cur.Item1.globalY, out sideTiles[0])) sideTiles[0].Item2 = -1;
						if (!checkedTiles.TryGetValue((cur.Item1.globalX + 1) + ";" + cur.Item1.globalY, out sideTiles[1])) sideTiles[1].Item2 = -1;
						if (!checkedTiles.TryGetValue(cur.Item1.globalX + ";" + (cur.Item1.globalY - 1), out sideTiles[2])) sideTiles[2].Item2 = -1;
						if (!checkedTiles.TryGetValue(cur.Item1.globalX + ";" + (cur.Item1.globalY + 1), out sideTiles[3])) sideTiles[3].Item2 = -1;
						for (int i = 0; i < 4; i++)
							if (sideTiles[i].Item1?.gameObject?.objectName == "obj_wall" && sideTiles[i].Item2 < cur.Item2)
								cur = sideTiles[i];
					}

					if (cur.Item1 != null)
					{
						_perimeter.Add(cur.Item1);
						cur.Item1.tileType = 8;
						MapController.Instance.GetChunk(cur.Item1.globalX, cur.Item1.globalY).UpdateTile(cur.Item1.x, cur.Item1.y);
						_perimeter.Add(MapController.Instance.GetTile(x, y));

						if (CheckTileInsideRoom(new Point(x, y + 1), true, _perimeter, _min, _max))
							FloodFill(new Point(x, y + 1), _perimeter, _min, _max);
						else //if (CheckTileInsideRoom(new Point(x, y - 1), true, _perimeter, _min, _max))
							FloodFill(new Point(x, y - 1), _perimeter, _min, _max);
						Tile t = MapController.Instance.GetTile(x, y);
						t.tileType = fillTileType;
						MapController.Instance.GetChunk(t.globalX, t.globalY).UpdateTile(t.x, t.y);
					}
				}
			}

			public override void Render()
			{
				destRect.X = (int)(x * Constants.TILE_SIZE + MapController.Instance.camera.x);
				destRect.Y = (int)(y * Constants.TILE_SIZE + MapController.Instance.camera.y);
				GameController.Instance.Render(sprite, destRect, srcRect);
			}

			public override void Start()
			{

			}

			public override void PreUpdate()
			{

			}

			public override void Update()
			{

			}

			public override void PostUpdate()
			{

			}
		}
	}
}