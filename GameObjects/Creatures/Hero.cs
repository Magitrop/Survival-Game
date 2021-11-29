using Game.Controllers;
using Game.GameObjects.Creatures;
using Game.Interfaces;
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
        private class Hero : Creature
        {
            public Hero(
                int _x, 
                int _y, 
                byte[] additionalInformation = null) : base(_x, _y, 1, "creature_hero", MapController.Instance.tilesSheet, additionalInformation)
            {
                destRect = new Rectangle(0, 0, (int)Constants.TILE_SIZE, (int)Constants.TILE_SIZE);
                srcRect = new Rectangle(32, 32, 16, 16);
                visualMovementSpeed = 10;
                isDespawnable = true;

                maxHealth = currentHealth = 100;
                damageAmount = 25;

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

            public override void Update()
            {
                MoveToVisual();
            }

            public override void PostUpdate()
            {

            }
        }
    }
}