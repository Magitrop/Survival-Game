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
	public enum WalkType
    {
		GroundOnly,
		WaterOnly,
		Everywhere
    }

	public abstract partial class GameObject : IBehaviour
	{
		public readonly Image sprite;
		public readonly int objectID;
		public readonly string objectName;
		public int x, y;
		public float visualX, visualY;
		public float visualMovementSpeed = 5;
		public bool isDespawnable;
		public WalkType canWalkOn;

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
				/*MapController.Instance.HasChunk(x, y) &&	*/
				MathOperations.Distance(coords, GameController.Instance.mainHero.coords) < 
					Math.Max(Constants.WINDOW_WIDTH, Constants.WINDOW_HEIGHT) / (2 * Constants.TILE_SIZE);
        }
		public (int x, int y) coords
        {
			get => (x, y);
        }
		public (float, float) visualCoords
		{
			get => (visualX, visualY);
		}
		public Rectangle destRect;
		public Rectangle srcRect;
		public byte[] objectAdditionalInformation;

		protected GameObject(int _x, int _y, int ID, string name, Image _sprite, byte[] additionalInformation = null)
		{
			visualX = x = _x;
			visualY = y = _y;
			objectID = ID;
			objectName = name;
			sprite = _sprite;
			objectAdditionalInformation = additionalInformation ?? new byte[] { (byte)objectID };
		}

		protected virtual void OnSpawn() { }
		protected virtual void OnDespawn() { }
		public abstract void Update();
		public abstract void Render();
		public bool CanStepOn(int tileType)
        {
			switch (canWalkOn)
            {
				case WalkType.Everywhere: return true;
				case WalkType.WaterOnly: return tileType <= 2;
				case WalkType.GroundOnly: return tileType > 2;
			}
			return true;
        }
		public static bool CanStepOn(WalkType canWalkOn, int tileType)
		{
			switch (canWalkOn)
			{
				case WalkType.Everywhere: return true;
				case WalkType.WaterOnly: return tileType <= 2;
				case WalkType.GroundOnly: return tileType > 2;
			}
			return true;
		}
		public bool MoveTo(int _x, int _y, bool byForce = false)
		{
			if (MapController.Instance.HasChunk(_x, _y))
            {
				Tile moveTo = MapController.Instance.GetTile(_x, _y);
				if ((moveTo.gameObject != null || !CanStepOn(moveTo.tileType)) && !byForce)
					return false;
				MapController.Instance.GetTile(x, y).SetGameObject(null);
				x = _x;
				y = _y;
				MapController.Instance.GetTile(x, y).SetGameObject(this);
			}
            else
            {
				if (!MapController.Instance.CheckTile(_x, _y, MapController.CheckIfTile.IsPassable) && !byForce)
					return false;
				MapController.Instance.GetTile(x, y, false)?.SetGameObject(null);
				x = _x;
				y = _y;
			}
			LightingController.Instance.GenerateLighting();
			return true;
		}

		public bool MoveToVisual(bool instantly = false)
        {
			bool resultX, resultY;
			if (isVisible && !instantly)
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

		public static GameObject Spawn(int _objectID, int atX, int atY, byte[] additionalInformation = null)
        {
			switch (_objectID)
            {
				case 1: return Spawn(new Hero(atX, atY, additionalInformation));
				case 2: return Spawn(new WolfCreature(atX, atY, additionalInformation));
				case 3: return Spawn(new BearCreature(atX, atY, additionalInformation));
				case 100: return Spawn(new PineTreeObject(atX, atY, additionalInformation));
				case 101: return Spawn(new WoodenFenceObject(atX, atY, additionalInformation));
				case 102: return Spawn(new WoodenFenceGateObject(atX, atY, additionalInformation));
				case 103: return Spawn(new BonfireObject(atX, atY, additionalInformation));
				case 104: return Spawn(new ChestObject(atX, atY, additionalInformation));
				case 105: return Spawn(new PalmTreeObject(atX, atY, additionalInformation));
				default:
					return null;
            }
		}

		public static GameObject Spawn(string _objectName, int atX, int atY, byte[] additionalInformation = null)
		{
			switch (_objectName)
			{
				case "creature_hero": return Spawn(new Hero(atX, atY, additionalInformation));
				case "creature_wolf": return Spawn(new WolfCreature(atX, atY, additionalInformation));
				case "creature_bear": return Spawn(new BearCreature(atX, atY, additionalInformation));
				case "obj_pine_tree": return Spawn(new PineTreeObject(atX, atY, additionalInformation));
				case "obj_wooden_fence": return Spawn(new WoodenFenceObject(atX, atY, additionalInformation));
				case "obj_wooden_fence_gate": return Spawn(new WoodenFenceGateObject(atX, atY, additionalInformation));
				case "obj_bonfire": return Spawn(new BonfireObject(atX, atY, additionalInformation));
				case "obj_chest": return Spawn(new ChestObject(atX, atY, additionalInformation));
				case "obj_palm_tree": return Spawn(new PalmTreeObject(atX, atY, additionalInformation));
				default:
					return null;
			}
		}

		public static string GetObjectNameByID(int _objectID)
        {
			switch (_objectID)
			{
				case 1: return "creature_hero";
				case 2: return "creature_wolf";
				case 3: return "creature_bear";
				case 100: return "obj_pine_tree";
				case 101: return "obj_wooden_fence";
				case 102: return "obj_wooden_fence_gate";
				case 103: return "obj_bonfire";
				case 104: return "obj_chest";
				case 105: return "obj_palm_tree";
				default:
					return "null";
			}
		}

		public static int GetObjectIDByName(string _objectName)
		{
			switch (_objectName)
			{
				case "creature_hero": return 1;
				case "creature_wolf": return 2;
				case "creature_bear": return 3;
				case "obj_pine_tree": return 100;
				case "obj_wooden_fence": return 101;
				case "obj_wooden_fence_gate": return 102;
				case "obj_bonfire": return 103;
				case "obj_chest": return 104;
				case "obj_palm_tree": return 105;
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