using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Game.Controllers;
using Game.Interfaces;
using Game.Map;
using Game.Miscellaneous;

namespace Game.GameObjects
{
	public abstract partial class GameObject : IBehaviour
	{
		public readonly Image sprite;
		public readonly int objectID;
		public readonly string objectName;
		public int x, y;
		public float visualX, visualY;
		public float visualMovementSpeed = 5;
		public bool isDespawnable;

		public bool isMoving
        {
			get
            {
				bool resultX, resultY;
				float speed = visualMovementSpeed * Time.deltaTime;
				MathOperations.MoveTowards(visualX, x, speed, out resultX);
				MathOperations.MoveTowards(visualY, y, speed, out resultY);
				return resultX && resultY;
			}
        }
		public bool isVisible
        {
            get => 
				MapController.Instance.HasChunk(x, y) &&	
				MathOperations.Distance(coords, GameController.Instance.mainHero.coords) < 
					Math.Max(Constants.WINDOW_WIDTH, Constants.WINDOW_HEIGHT) / (2 * Constants.TILE_SIZE);
        }
		public (int, int) coords
        {
			get => (x, y);
        }
		public (float, float) visualCoords
		{
			get => (visualX, visualY);
		}
		public Rectangle destRect;
		public Rectangle srcRect;

		protected GameObject(int _x, int _y, int ID, string name, Image _sprite)
		{
			visualX = x = _x;
			visualY = y = _y;
			objectID = ID;
			objectName = name;
			sprite = _sprite;
		}

		public virtual void OnSpawn() { }
		public virtual void OnDespawn() { }
		public abstract void Update();
		public abstract void Render();
		public bool MoveTo(int _x, int _y)
		{
			if (MapController.Instance.HasChunk(_x, _y))
            {
				Tile moveTo = MapController.Instance.GetTile(_x, _y);
				if (moveTo.gameObject != null)
					return false;
				MapController.Instance.GetTile(x, y).SetGameObject(null);
				x = _x;
				y = _y;
				MapController.Instance.GetTile(x, y).SetGameObject(this);
			}
            else
            {
				if (!MapController.Instance.CheckTile(_x, _y, MapController.CheckIfTile.IsPassable))
					return false;
				MapController.Instance.GetTile(x, y, false)?.SetGameObject(null);
				x = _x;
				y = _y;
			}
			return true;
		}

		public bool MoveToVisual()
        {
			bool resultX, resultY;
			if (isVisible)
			{
				float speed = visualMovementSpeed * Time.deltaTime;
				visualX = MathOperations.MoveTowards(visualX, x, speed, out resultX);
				visualY = MathOperations.MoveTowards(visualY, y, speed, out resultY);
				return resultX && resultY;
			}
			else
			{
				visualX = x;
				visualY = y;
				return true;
			}
		}

		public static GameObject Spawn(GameObject objectToSpawn)
        {
			if (objectToSpawn == null)
				return null;
			MapController.Instance.GetTile(objectToSpawn.x, objectToSpawn.y).SetGameObject(objectToSpawn);
			objectToSpawn.OnSpawn();
			return objectToSpawn;
        }

		public static TurnBasedObject Spawn(TurnBasedObject objectToSpawn)
		{
			if (objectToSpawn == null)
				return null;
			MapController.Instance.GetTile(objectToSpawn.x, objectToSpawn.y).SetGameObject(objectToSpawn);
			objectToSpawn.OnSpawn();
			return objectToSpawn;
		}

		public static GameObject Spawn(int _objectID, int atX, int atY)
        {
			switch (_objectID)
            {
				case 1: return Spawn(new Hero(atX, atY));
				case 2: return Spawn(new TestCreature(atX, atY));
				case 100: return Spawn(new Tree(atX, atY));
				case 101: return Spawn(new WallObject(atX, atY));
				case 102: return Spawn(new FenceGateObject(atX, atY));
				default:
					return null;
            }
		}

		public static GameObject Spawn(string _objectName, int atX, int atY)
		{
			switch (_objectName)
			{
				case "creature_hero": return Spawn(new Hero(atX, atY));
				case "creature_test": return Spawn(new TestCreature(atX, atY));
				case "obj_tree": return Spawn(new Tree(atX, atY));
				case "obj_wall": return Spawn(new WallObject(atX, atY));
				case "obj_fence_gate": return Spawn(new FenceGateObject(atX, atY));
				default:
					return null;
			}
		}

		public static string GetObjectNameByID(int _objectID)
        {
			switch (_objectID)
			{
				case 1: return "creature_hero";
				case 2: return "creature_test";
				case 100: return "obj_tree";
				case 101: return "obj_wall";
				case 102: return "obj_fence_gate";
				default:
					return "null";
			}
		}

		public static int GetObjectIDByName(string _objectName)
		{
			switch (_objectName)
			{
				case "creature_hero": return 1;
				case "creature_test": return 2;
				case "obj_tree": return 100;
				case "obj_wall": return 101;
				case "obj_fence_gate": return 102;
				default:
					return 0;
			}
		}

		public abstract void Start();
		public abstract void PreUpdate();
		public abstract void PostUpdate();

        ~GameObject()
        {
			OnDespawn();
        }
	}
}