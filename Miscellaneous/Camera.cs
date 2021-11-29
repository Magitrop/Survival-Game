using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Controllers;
using Game.GameObjects;
using Game.Map;

namespace Game.Miscellaneous
{
	public sealed class Camera
	{
		// визуальные координаты
		public float x, y;
		// визуальные координаты для свободной камеры
		public float freeX, freeY;
		// скорость перемещения камеры
		public float movementSpeed;
		// находится ли камера в свободном режиме?
		public bool freeCamera;
		public GameObject target;

		public bool cameraFinishedMovement { get; private set; }
		
		public Camera(int startX = 0, int startY = 0, float speed = 400)
		{
			x = -startX * Constants.TILE_SIZE + Constants.WINDOW_WIDTH / 2 - Constants.TILE_SIZE / 2;
			y = -startY * Constants.TILE_SIZE + Constants.WINDOW_HEIGHT / 2 - Constants.TILE_SIZE / 2;
			movementSpeed = speed;
			target = null;
		}

		/// <summary>
		/// Возвращает true, если камера достигла цели или если камера находится в свободном режиме.
		/// </summary>
		/// <returns></returns>
		public bool CameraFollow()
		{
			if (freeCamera || target == null)
				return false;

			float speedDelta = movementSpeed * Time.deltaTime;
			float targetX = -target.visualX * Constants.TILE_SIZE + Constants.WINDOW_WIDTH / 2 - Constants.TILE_SIZE / 2;
			float targetY = -target.visualY * Constants.TILE_SIZE + Constants.WINDOW_HEIGHT / 2 - Constants.TILE_SIZE / 2;

			x = MathOperations.MoveTowards(x, targetX, speedDelta, out bool reachX);
			y = MathOperations.MoveTowards(y, targetY, speedDelta, out bool reachY);
			cameraFinishedMovement = reachX && reachY;
			return cameraFinishedMovement;
		}

		public void TeleportCamera()
        {
			if (target == null)
				return;
			x = -target.x * Constants.TILE_SIZE + Constants.WINDOW_WIDTH / 2 - Constants.TILE_SIZE / 2;
			y = -target.y * Constants.TILE_SIZE + Constants.WINDOW_HEIGHT / 2 - Constants.TILE_SIZE / 2;
			cameraFinishedMovement = true;
		}
	}
}