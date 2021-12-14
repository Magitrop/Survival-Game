using Game.Controllers;
using Game.GameObjects.Creatures;
using Game.Interfaces;
using Game.Map;
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
        private sealed class WolfCreature : Creature
        {
			public WolfCreature(
				int _x,
				int _y,
				byte[] additionalInformation = null) : 
				base(_x, _y, 2, "creature_wolf", Constants.creatureSheets["creature_wolf"], additionalInformation)
			{
				destRect = new Rectangle(0, 0, (int)(Constants.TILE_SIZE * 1.5f), (int)Constants.TILE_SIZE);
				srcRect = new Rectangle(0, 0, 24, 16);
				isDespawnable = true;
				canWalkOn = WalkType.GroundOnly;

				maxActionsCount = 15;
				maxHealth = currentHealth = 120;
				isAlive = true;
				damageAmount = 15;
				sightDistance = 8;

				dropsItems.Add((Items.ItemRawMeat.Instance, new Random().Next(1, 4)));

				Start();
			}

			public Creature currentTarget;
			private List<(int x, int y)> currentPath;
			private int currentPathIndex;

			public override void Render()
			{
				if (isFacingRight)
					srcRect.X = 24;
				else
					srcRect.X = 0;

				if (isAlive)
					srcRect.Y = 0;
				else
					srcRect.Y = 16;

				destRect.X = (int)((visualX - 0.25f) * Constants.TILE_SIZE + MapController.Instance.camera.x);
				destRect.Y = (int)((visualY - 0.25f) * Constants.TILE_SIZE + MapController.Instance.camera.y);
				GameController.Instance.Render(sprite, destRect, srcRect);
				DrawHealthbar();
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
				currentTarget = GameController.Instance.mainHero;
				currentPath = MapController.Instance.FindPath(coords, currentTarget.coords, canWalkOn, sightDistance);
				currentPathIndex = 0;
			}

			public override void Update()
			{
				if (GameController.Instance.currentObject == this)
				{
					if (MoveToVisual())
					{
						if (actionsLeft <= 0 || !isAlive)
						{
							GameController.Instance.NextTurn();
							return;
						}
						if (currentPath != null && currentPath.Count > 0)
                        {
							if (currentPath[currentPathIndex].x > x)
								isFacingRight = true;
							else if (currentPath[currentPathIndex].x < x)
								isFacingRight = false;
							if (MapController.Instance.GetTile(
								currentPath[currentPathIndex].x,
								currentPath[currentPathIndex].y).gameObject != currentTarget)
							{
								if (MoveTo(currentPath[currentPathIndex].x, currentPath[currentPathIndex].y))
								{
									if (isVisible)
										GameController.Instance.SetPause(0.25f);
									currentPathIndex++;
									actionsLeft -= Tile.GetTileTypePathPrice(MapController.Instance.GetTile(x, y).tileType);
								}
							}
							else
							{
								DealDamage(currentTarget, this, damageAmount);
								actionsLeft = 0;
							}
						}
						else actionsLeft = 0;
					}
				}
			}

			public override void PostUpdate()
			{

			}

			public override void PostRender()
			{
				if (isAlive)
					DrawHealthbar();
			}

			protected override void DrawHealthbar()
			{
				healthBarDest.X = healthCount.X = (int)(visualX * Constants.TILE_SIZE + MapController.Instance.camera.x);
				healthBarDest.Y = healthCount.Y = (int)((visualY - 0.6f) * Constants.TILE_SIZE + MapController.Instance.camera.y);
				healthCount.Width = (int)(healthBarDest.Width / 100f * Math.Min(currentHealth * 100f / maxHealth, 100));
				GameController.Instance.Render(
					Constants.uiSheet,
					healthBarDest,
					backgroundBarSrc);
				GameController.Instance.Render(
					Constants.uiSheet,
					healthCount,
					healthBarSrc);
			}
		}
    }
}