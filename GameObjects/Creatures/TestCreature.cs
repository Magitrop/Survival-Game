using Game.Controllers;
using Game.GameObjects.Creatures;
using Game.Interfaces;
using Game.Miscellaneous;
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
		private sealed class TestCreature : Creature
		{
			public Creature currentTarget;
			private List<(int x, int y)> currentPath;
			private int currentPathIndex;

			public TestCreature(
				int _x, 
				int _y, 
				byte[] additionalInformation = null) : base(_x, _y, 2, "creature_test", MapController.Instance.tilesSheet, additionalInformation)
			{
				destRect = new Rectangle(0, 0, (int)Constants.TILE_SIZE, (int)Constants.TILE_SIZE);
				srcRect = new Rectangle(32, 32, 16, 16);
				isDespawnable = true;

				maxActionsCount = 20;
				maxHealth = currentHealth = 100;
				damageAmount = 10;

				Start();
			}

			public override void Render()
			{
				destRect.X = (int)(visualX * Constants.TILE_SIZE + MapController.Instance.camera.x);
				destRect.Y = (int)(visualY * Constants.TILE_SIZE + MapController.Instance.camera.y);
				GameController.Instance.Render(sprite, destRect, srcRect);
			}

			public override void Start()
			{

			}

			public override void PreUpdate()
			{

			}

			public override void OnTurnStart()
			{
				base.OnTurnStart();
				currentTarget = GameController.Instance.mainHero as Creature;
				currentPath = MapController.Instance.FindPath(coords, currentTarget.coords);
				currentPathIndex = 0;
			}

			public override void Update()
			{
				if (GameController.Instance.currentObject == this)
				{
					if (MoveToVisual())
					{
						if (actionsLeft <= 0)
						{
							GameController.Instance.NextTurn();
							return;
						}
						actionsLeft--;
						if (currentPath != null && currentPath.Count > 0)
							if (MapController.Instance.GetTile(
								currentPath[currentPathIndex].x, 
								currentPath[currentPathIndex].y).gameObject != currentTarget)
							{
								if (MoveTo(currentPath[currentPathIndex].x, currentPath[currentPathIndex].y))
								{
									if (isVisible)
										GameController.Instance.SetPause(0.25f);
									currentPathIndex++;
								}
							}
							else
							{
								DealDamage(currentTarget, this, damageAmount);
                                actionsLeft = 0;
							}
					}
				}
			}

			public override void PostUpdate()
			{

			}
		}
	}
}