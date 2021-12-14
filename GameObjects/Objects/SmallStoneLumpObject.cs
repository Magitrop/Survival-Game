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
        private sealed class SmallStoneLumpObject : BreakableObject
        {
            public SmallStoneLumpObject(int _x, int _y, byte[] additionalInformation = null) :
                base(_x, _y, 107, "obj_small_stone_lump", Constants.objectsSheet, additionalInformation)
            {
                destRect = new Rectangle(0, 0, (int)Constants.TILE_SIZE, (int)Constants.TILE_SIZE);
                srcRect = new Rectangle(64, 16, 16, 16);
                isDespawnable = true;
                maxDurability = 12;
                shouldBeBrokenWith = ToolType.Pickaxe;
                if (additionalInformation != null && additionalInformation.Length > 1)
                    durability = additionalInformation[1];
                else
                    durability = maxDurability;

                Start();
            }

            public override void Render()
            {
                destRect.X = (int)(x * Constants.TILE_SIZE + MapController.Instance.camera.x);
                destRect.Y = (int)(y * Constants.TILE_SIZE + MapController.Instance.camera.y - 0.25f * Constants.TILE_SIZE);
                GameController.Instance.Render(sprite, destRect, srcRect);
            }

            protected override void OnBreak(GameObject breaker, ToolType wasBrokenWith)
            {
                base.OnBreak(breaker, wasBrokenWith);
                if (breaker as Hero != null)
                {
                    if (wasBrokenWith == shouldBeBrokenWith)
                        InventoryController.Instance.ReceiveItems(Items.ItemCobblestone.Instance, new Random().Next(5, 9));
                    else
                        InventoryController.Instance.ReceiveItems(Items.ItemCobblestone.Instance, new Random().Next(2, 5));
                }
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