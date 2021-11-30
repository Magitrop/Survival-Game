using Game.Controllers;
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
        private sealed class PineTreeObject : BreakableObject
        {
            public PineTreeObject(int _x, int _y, byte[] additionalInformation = null) : 
                base(_x, _y, 100, "obj_pine_tree", MapController.Instance.objectsSheet, additionalInformation)
            {
                destRect = new Rectangle(0, 0, (int)(Constants.TILE_SIZE * 1.5f), (int)(Constants.TILE_SIZE * 2f));
                srcRect = new Rectangle(103, 0, 24, 32);
                isDespawnable = true;

                Start();
            }

            public override void Render()
            {
                destRect.X = (int)(x * Constants.TILE_SIZE + MapController.Instance.camera.x - 0.5f * Constants.TILE_SIZE);
                destRect.Y = (int)(y * Constants.TILE_SIZE + MapController.Instance.camera.y - 1.25f * Constants.TILE_SIZE);
                GameController.Instance.Render(sprite, destRect, srcRect);
            }

            protected override void OnBreak(GameObject breaker)
            {
                base.OnBreak(breaker);
                if (breaker as Hero != null)
                    InventoryController.Instance.ReceiveItems(Items.ItemWoodenLog.Instance, new Random().Next(3, 7));
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