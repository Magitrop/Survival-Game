using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Game.GameObjects;
using Game.GameObjects.Creatures;
using Game.Interfaces;
using Game.Map;
using Game.Miscellaneous;

namespace Game.Controllers
{
	public sealed class GameController
	{
		// singleton
		private GameController() { }
		private static GameController instance;
		/// <summary>
		/// Осуществляет доступ к единственному экземпляру GameController.
		/// </summary>
		/// <returns></returns>
		public static GameController Instance
		{
			get
			{
				if (instance == null)
					instance = new GameController();
				return instance;
			}
		}

		/// <summary>
		/// Очередь изображений, требующих рендера в текущем кадре.
		/// </summary>
		public Queue<Frame> renderQueue;
		/// <summary>
		/// Очередность ходов объектов.
		/// </summary>
		public List<TurnBasedObject> objectsQueue;
		/// <summary>
		/// Объект, который сейчас совершает свой ход.
		/// </summary>
		public TurnBasedObject currentObject
		{
			get
			{
				return 
					(objectsQueue.Count > 0 && 
					currentObjectIndex >= 0 && 
					currentObjectIndex < objectsQueue.Count) 
					? objectsQueue[currentObjectIndex] 
					: null;
			}
		}
		public int currentObjectIndex;
		public GameObject mainHero;

		public string currentSavePath;
		public List<SavedChunkInfo> savedChunks;
		public float pause { get; private set; }

		public int currentFPS;
		private float screenCounterFPS = 0;

		public Point mousePosition;
		public Point currentTileSelection = new Point();
		public bool mouseIsDown;

		/// <summary>
		/// Метод, вызываемый при запуске приложения.
		/// </summary>
		public void Start()
		{
			renderQueue = new Queue<Frame>();
			savedChunks = new List<SavedChunkInfo>();
			objectsQueue = new List<TurnBasedObject>();

			MapController.Instance.Start();
			InventoryController.Instance.Start();

			if (!LoadGame("save"))
			{
				int playerX = 0;
				int playerY = 0;
				mainHero = GameObject.Spawn("creature_hero", playerX, playerY);
				objectsQueue.Add((TurnBasedObject)mainHero);
			}
			Chunk chunk = MapController.Instance.GetChunk(mainHero.x, mainHero.y);
			MapController.Instance.GetTile(mainHero.x, mainHero.y).SetGameObject(GameObject.Spawn(mainHero));
			MapController.Instance.RecalculateChunks(chunk.x, chunk.y);
			(MapController.Instance.camera = new Camera(mainHero.x, mainHero.y)).target = mainHero;

			currentObjectIndex = 0;
		}

		public void NextTurn()
		{
			currentObject?.OnTurnEnd();
			currentObjectIndex = currentObjectIndex < objectsQueue.Count - 1 ? currentObjectIndex + 1 : 0;
			currentObject?.OnTurnStart();
		}

		public void SetPause(float duration)
		{
			pause = duration;
		}

		/// <summary>
		/// Вызывает в игровых объектах все методы, которые должны выполняться каждый кадр.
		/// </summary>
		public void Update()
		{
			//MainWindow.curAngle = (MainWindow.curAngle += Time.deltaTime) % (float)(2 * Math.PI);
			//MainWindow.radius = (float)(Math.Sin(Time.timeSinceStart * 2) + 2) * 150 + 70;
			if (MainWindow.DrawWindowClosingAnimation)
			{
				MainWindow.radius = MathOperations.MoveTowards(MainWindow.radius, 1, Time.deltaTime * 1500);
				if (MainWindow.radius <= 2)
					Application.Exit();
			}
			MapController.Instance.PreUpdate();
			InventoryController.Instance.PreUpdate();

			var chunks = MapController.Instance.visibleChunks;
			for (int i = 0; i < chunks.Count; i++)
				chunks[i].RenderObjects();

			if (!MainWindow.DrawWindowClosingAnimation &&
				!(!MapController.Instance.camera.cameraFinishedMovement ||
				currentObject != mainHero ||
				!mainHero.isMoving) &&
				InventoryController.Instance.selectedSlot == null &&
				!InventoryController.Instance.isDraggingItem)
			{
				double _x = (mousePosition.X - MapController.Instance.camera.x) / Constants.TILE_SIZE;
				if (_x < 0)
					currentTileSelection.X = (int)Math.Truncate(_x - 1);
				else currentTileSelection.X = (int)Math.Truncate(_x);
				double _y = (mousePosition.Y - MapController.Instance.camera.y) / Constants.TILE_SIZE;
				if (_y < 0)
					currentTileSelection.Y = (int)Math.Truncate(_y - 1);
				else currentTileSelection.Y = (int)Math.Truncate(_y);

				Rectangle rect = new Rectangle
				(
					(int)(currentTileSelection.X * Constants.TILE_SIZE + MapController.Instance.camera.x)
						/*(int)(Math.Truncate((mousePosition.X - Constants.TEXTURE_RESOLUTION / 2) / Constants.TILE_SIZE +
						(MapController.Instance.camera.x < 0 ? 1f : 0)) * Constants.TILE_SIZE +
							MapController.Instance.camera.x % Constants.TILE_SIZE)*/,
					(int)(currentTileSelection.Y * Constants.TILE_SIZE + MapController.Instance.camera.y)
						/*(int)(Math.Truncate((mousePosition.Y - Constants.TEXTURE_RESOLUTION / 2) / Constants.TILE_SIZE +
						(MapController.Instance.camera.y < 0 ? 1f : 0)) * Constants.TILE_SIZE +
							MapController.Instance.camera.y % Constants.TILE_SIZE)*/,
					(int)Constants.TILE_SIZE,
					(int)Constants.TILE_SIZE
				);
				Rectangle src;
				Tile selectedTile = MapController.Instance.GetTile(currentTileSelection.X, currentTileSelection.Y);
				if (MathOperations.Distance((currentTileSelection.X, currentTileSelection.Y), mainHero.coords) < 2)
				{
					if (mouseIsDown && 
						InventoryController.Instance.currentSlot.currentItem != null && 
						InventoryController.Instance.currentSlot.currentItem.isPlaceable)
					{
						if (selectedTile.gameObject == null)
							if (InventoryController.Instance.currentSlot.currentItem.OnItemPlacing(selectedTile))
								InventoryController.Instance.WithdrawItemsFromSlot(
									InventoryController.Instance.currentSlot, 
									InventoryController.Instance.currentSlot.currentItem, 1);
					}
					src = new Rectangle(
						7 * Constants.TEXTURE_RESOLUTION, 
						2 * Constants.TEXTURE_RESOLUTION, 
						Constants.TEXTURE_RESOLUTION, 
						Constants.TEXTURE_RESOLUTION);
				}
				else src = new Rectangle(
					8 * Constants.TEXTURE_RESOLUTION, 
					2 * Constants.TEXTURE_RESOLUTION, 
					Constants.TEXTURE_RESOLUTION, 
					Constants.TEXTURE_RESOLUTION);
                if (InventoryController.Instance.currentSlot.currentItem != null || (selectedTile.gameObject as Creature) != null)
					Render(MapController.Instance.tilesSheet, rect, src);
			}

			if (pause <= 0)
				for (int i = 0; i < objectsQueue.Count;)
				{
					objectsQueue[i].PreUpdate();
					objectsQueue[i].Update();
					objectsQueue[i].PostUpdate();
					if (MathOperations.Distance(objectsQueue[i].coords, mainHero.coords) > Constants.OBJECTS_DESPAWN_RANGE)
					{
						if (MapController.Instance.HasChunk(objectsQueue[i].x, objectsQueue[i].y))
							MapController.Instance.GetTile(objectsQueue[i].x, objectsQueue[i].y)?.SetGameObject(null);
						if (currentObject != objectsQueue[i])
							objectsQueue.RemoveAt(i);
						if (currentObjectIndex >= objectsQueue.Count)
							currentObjectIndex = 0;
					}
					else i++;
				}
			else pause -= Time.deltaTime;

			MapController.Instance.PostUpdate();
			InventoryController.Instance.PostUpdate();
			MapController.Instance.camera.CameraFollow();
		}

		public void Spawn(TurnBasedObject objectToSpawn)
		{
			objectsQueue.Add(GameObject.Spawn(objectToSpawn));
			if (MapController.Instance.HasChunk(objectToSpawn.x, objectToSpawn.y))
				MapController.Instance.GetTile(objectToSpawn.x, objectToSpawn.y).SetGameObject(objectToSpawn);
		}

		/// <summary>
		/// Возвращает false, если файла сохранения не существует.
		/// </summary>
		/// <param name="saveName"></param>
		/// <returns></returns>
		public bool LoadGame(string saveName)
		{
			Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\Saves");
			if (File.Exists(currentSavePath = Directory.GetCurrentDirectory() + "\\Saves\\" + saveName + ".dat"))
			{
				var lines = File.ReadAllLines(Directory.GetCurrentDirectory() + "\\Saves\\" + saveName + ".dat");

				int creaturesCount = int.Parse(lines[0]);
				for (int i = lines.Length - 1; i > 0; i--)
				{
					if (i >= creaturesCount + 1)
						savedChunks.Add(new SavedChunkInfo(lines[i]));
					else
					{
						string[] info = lines[i].Split(',');
						if (i == 1)
						{
							int playerX = int.Parse(info[1]);
							int playerY = int.Parse(info[2]);
							mainHero = GameObject.Spawn("creature_hero", playerX, playerY);
							objectsQueue.Add((TurnBasedObject)mainHero);
							for (int j = 3; j < info.Length; j += 4)
								InventoryController.Instance.AddItemsToSlot(
									InventoryController.Instance.GetSlotByRowAndColumn((int.Parse(info[j]), int.Parse(info[j + 1]))),
									Items.GetItemByID(int.Parse(info[j + 2])),
									int.Parse(info[j + 3]));
						}
						else
							Spawn((TurnBasedObject)GameObject.Spawn(int.Parse(info[0]), int.Parse(info[1]), int.Parse(info[2])));
					}
				}
				return true;
			}
			return false;
		}

		public void SaveGame(string saveName)
		{
			Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\Saves");
			List<string> saves = new List<string>();

			// кол-во существ на карте и их ID и координаты
			saves.Add(objectsQueue.Count.ToString());
			for (int i = 0; i < objectsQueue.Count; i++)
				if (objectsQueue[i].objectID == 1)
				{
					string str = objectsQueue[i].objectID + "," + objectsQueue[i].x + "," + objectsQueue[i].y;
					foreach (var slotColumn in InventoryController.Instance.slots)
						foreach (var slot in slotColumn)
							if (slot.currentItem != null)
							{
								(int row, int column) s = InventoryController.Instance.GetRowAndColumnFromSlot(slot);
								str += "," +
									s.row + "," +
									s.column + "," +
									slot.currentItem.itemID + "," +
									slot.itemsCount;
							}
					saves.Add(str);
				}
				else
					saves.Add(objectsQueue[i].objectID + "," + objectsQueue[i].x + "," + objectsQueue[i].y);

			foreach (var save in savedChunks)
				saves.Add(save.ToString());
			File.WriteAllLines(currentSavePath = Directory.GetCurrentDirectory() + "\\Saves\\" + saveName + ".dat", saves);
		}

		public void OnPress(object sender, KeyEventArgs e)
		{
			if (MainWindow.DrawWindowClosingAnimation)
				return;
			/*if (e.KeyCode == Keys.Escape)
				MainWindow.DrawWindowClosingAnimation = true;*/
			if (e.KeyCode == KeyBindings.ToggleGridDrawing)
				shouldDrawGrid = !shouldDrawGrid;

			if (e.KeyCode >= Keys.D1 && e.KeyCode <= Keys.D9)
				InventoryController.Instance.currentSlotIndex = (int)e.KeyCode - 49;
			else if (e.KeyCode == Keys.D0)
				InventoryController.Instance.currentSlotIndex = 9;

			if (e.KeyCode == Keys.I)
				if (InventoryController.Instance.displayMode != InventoryController.InventoryDisplayMode.HotbarOnly)
					InventoryController.Instance.displayMode = InventoryController.InventoryDisplayMode.HotbarOnly;
				else
					InventoryController.Instance.displayMode = InventoryController.InventoryDisplayMode.WholeCenterAligned;

			if (e.KeyCode == Keys.T)
			{
				InventoryController.Instance.ReceiveItems(Items.ItemWoodenFence.Instance);
				InventoryController.Instance.ReceiveItems(Items.ItemWoodenFenceGate.Instance);
			}

			if (!MapController.Instance.camera.cameraFinishedMovement || currentObject != mainHero || !mainHero.isMoving)
				return;

			if (e.KeyCode == KeyBindings.MoveUp)
				MoveHero(0, -1);
			else if (e.KeyCode == KeyBindings.MoveRight)
				MoveHero(1, 0);
			else if (e.KeyCode == KeyBindings.MoveDown)
				MoveHero(0, 1);
			else if (e.KeyCode == KeyBindings.MoveLeft)
				MoveHero(-1, 0);
			else if (e.KeyCode == KeyBindings.PlaceBlock)
			{
				Tile t;
				//(t = MapController.Instance.GetTile(mainHero.x, mainHero.y - 1)).tileType = 1;
				//MapController.Instance.GetChunk(mainHero.x, mainHero.y - 1).UpdateTile(t.x, t.y);
				if ((t = MapController.Instance.GetTile(mainHero.x, mainHero.y - 1)).gameObject == null)
				{
					t.SetGameObject(GameObject.Spawn("obj_wall", mainHero.x, mainHero.y - 1));
					MapController.Instance.GetChunk(mainHero.x, mainHero.y - 1).UpdateTile(t.x, t.y);
					/*if (t.gameObject.objectName == "obj_wall")
						t.SetGameObject(null);*/
				}
			}

			if (e.KeyCode == Keys.Return)
			{
				/*Tile t = MapController.Instance.GetTile(mainHero.x, mainHero.y - 1);
				t?.SetGameObject(null);
				MapController.Instance.GetChunk(mainHero.x, mainHero.y - 1).UpdateTile(t.x, t.y);*/
				NextTurn();
			}

			if (e.KeyCode == KeyBindings.SaveGame)
				SaveGame("save");
			if (e.KeyCode == Keys.O)
			{
				/*Tile t;
				if ((t = MapController.Instance.GetTile(mainHero.x, mainHero.y - 1)).gameObject == null)
				{
					t.SetGameObject(GameObject.Spawn("obj_fence_gate", mainHero.x, mainHero.y - 1));
					MapController.Instance.GetChunk(mainHero.x, mainHero.y - 1).UpdateTile(t.x, t.y);
				}*/
				Spawn((TurnBasedObject)GameObject.Spawn("creature_test", mainHero.x, mainHero.y + 1));
			}

			void MoveHero(int xOffset, int yOffset)
			{
				Chunk previousChunk = MapController.Instance.GetChunk(mainHero.x, mainHero.y);
				mainHero.MoveTo(mainHero.x + xOffset, mainHero.y + yOffset);
				Chunk chunk = MapController.Instance.GetChunk(mainHero.x, mainHero.y);
				if (previousChunk != chunk)
					MapController.Instance.RecalculateChunks(chunk.x, chunk.y);
				MapController.Instance.camera.CameraFollow();
			}
		}

		public void OnMouseDown(object sender, MouseEventArgs e)
		{
			mouseIsDown = true;
		}

		public void OnMouseUp(object sender, MouseEventArgs e)
		{
			mouseIsDown = false;
		}

		/// <summary>
		/// Добавляет изображение в очередь для рендера.
		/// </summary>
		public void Render(Image img, Rectangle dest, Rectangle src)
		{
			renderQueue.Enqueue(new ImageFrame(img, dest, src));
		}

		/// <summary>
		/// Добавляет текст в очередь для рендера.
		/// </summary>
		public void Render(string text, Point dest, Font font, Brush br)
		{
			renderQueue.Enqueue(new TextFrame(text, dest, font, br));
		}

		public bool shouldDrawGrid = false;
		private Pen gridPen = new Pen(Color.Red);
		private Brush brush = new SolidBrush(Color.White);
		/// <summary>
		/// Осуществляет отрисовку игрового окна.
		/// </summary>
		public void OnPaint(object sender, PaintEventArgs e)
		{
			Graphics graphics = e.Graphics;

			graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
			graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
			foreach (var frame in renderQueue)
			{
				if (frame.GetFrameType() == "image")
				{
					ImageFrame request = (ImageFrame)frame;
					graphics.DrawImage(request.imageToRender, request.destRect, request.srcRect, GraphicsUnit.Pixel);
				}
				else if (frame.GetFrameType() == "text")
				{
					TextFrame request = (TextFrame)frame;
					graphics.DrawString(request.textToRender, request.font, request.brush, request.destPoint);
				}
			}
			renderQueue.Clear();

			if (shouldDrawGrid)
			{
				int xOffset = (int)MapController.Instance.camera.x % (int)Constants.TILE_SIZE;
				int yOffset = (int)MapController.Instance.camera.y % (int)Constants.TILE_SIZE;
				for (int x = 0; x < Constants.WINDOW_WIDTH; x += (int)Constants.TILE_SIZE)
					for (int y = 0; y < Constants.WINDOW_HEIGHT; y += (int)Constants.TILE_SIZE)
					{
						graphics.DrawLine(gridPen, x + xOffset, 0, x + xOffset, Constants.WINDOW_HEIGHT);
						graphics.DrawLine(gridPen, 0, y + yOffset, Constants.WINDOW_WIDTH, y + yOffset);
					}
				xOffset = (int)MapController.Instance.camera.x % (int)(Constants.TILE_SIZE * Constants.CHUNK_SIZE);
				yOffset = (int)MapController.Instance.camera.y % (int)(Constants.TILE_SIZE * Constants.CHUNK_SIZE);
				for (int x = 0; x < Constants.WINDOW_WIDTH + (int)(Constants.TILE_SIZE * Constants.CHUNK_SIZE); x += (int)(Constants.TILE_SIZE * Constants.CHUNK_SIZE))
					for (int y = 0; y < Constants.WINDOW_HEIGHT + (int)(Constants.TILE_SIZE * Constants.CHUNK_SIZE); y += (int)(Constants.TILE_SIZE * Constants.CHUNK_SIZE))
					{
						graphics.DrawLine(new Pen(Color.Blue), x + xOffset, 0, x + xOffset, Constants.WINDOW_HEIGHT);
						graphics.DrawLine(new Pen(Color.Blue), 0, y + yOffset, Constants.WINDOW_WIDTH, y + yOffset);
					}
			}

			screenCounterFPS += Time.deltaTime;
			if (screenCounterFPS >= 0.5)
			{
				currentFPS = (int)Math.Truncate(1 / Time.deltaTime);
				screenCounterFPS = 0;
			}
			graphics.DrawString("FPS: " + currentFPS, Fonts.font_1, brush, 0, 0);
		}
	}
}