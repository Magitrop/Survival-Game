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
        public class Hero : Creature
        {
            public Hero(
                int _x, 
                int _y, 
                byte[] additionalInformation = null) : 
                base(_x, _y, 1, "creature_hero", Constants.creatureSheets["creature_hero"], additionalInformation)
            {
                destRect = new Rectangle(0, 0, (int)Constants.TILE_SIZE, (int)Constants.TILE_SIZE);
                srcRect = new Rectangle(0, 0, 16, 16);
                visualMovementSpeed = 10;
                isDespawnable = true;
                canWalkOn = WalkType.GroundOnly;

				maxActionsCount = 20;
                maxHealth = currentHealth = 100;
				isAlive = true;
                damageAmount = 10;

                hunger = 100;
                thirst = 100;

                Start();
            }

            public float hunger;
            public float thirst;

            public override void OnTurnStart()
            {
                base.OnTurnStart();
                foreach (var item in InventoryController.Instance.GetNonEmptySlots())
                    item.currentItem.OnTurnStart();
                hunger = MathOperations.MoveTowards(hunger, 0, 0.35f);
                thirst = MathOperations.MoveTowards(thirst, 0, 0.75f);
                if (hunger < 15)
                    DealDamage(this, null, (int)(15 - hunger));
                if (thirst < 15)
                    DealDamage(this, null, (int)(15 - thirst));
            }

            public override void OnTurnEnd()
            {
                base.OnTurnEnd();
                foreach (var item in InventoryController.Instance.GetNonEmptySlots())
                    item.currentItem.OnTurnEnd();
            }

            public override void Render()
            {
                if (isFacingRight)
                    srcRect.X = 0;
                else
                    srcRect.X = 16;
                destRect.X = (int)(visualX * Constants.TILE_SIZE + MapController.Instance.camera.x);
                destRect.Y = (int)((visualY - 0.25f) * Constants.TILE_SIZE + MapController.Instance.camera.y);
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

            protected override void DrawHealthbar() { }
        }
    }
}