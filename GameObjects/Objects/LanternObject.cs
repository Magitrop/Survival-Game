using Game.Controllers;
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
        private sealed class LanternObject : LightingObject
        {
            public LanternObject(int _x, int _y, byte[] additionalInformation = null) : 
                base(_x, _y, 103, "obj_lantern", MapController.Instance.objectsSheet, additionalInformation)
            {
                destRect = new Rectangle(0, 0, (int)Constants.TILE_SIZE, (int)Constants.TILE_SIZE);
                srcRect = new Rectangle(0, 0, 16, 16);
                isDespawnable = true;

                Start();
            }

            public override void Render()
            {
                destRect.X = (int)(x * Constants.TILE_SIZE + MapController.Instance.camera.x);
                destRect.Y = (int)(y * Constants.TILE_SIZE + MapController.Instance.camera.y);
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

            }

            public override void PostUpdate()
            {

            }
        }
    }
}