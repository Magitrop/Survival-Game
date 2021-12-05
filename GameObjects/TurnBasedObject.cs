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
    public abstract class TurnBasedObject : GameObject
    {
        public int maxActionsCount;
        public int actionsLeft;

        protected TurnBasedObject(int _x, int _y, int ID, string name, Image _sprite) : base(_x, _y, ID, name, _sprite) { }
        public virtual void OnTurnStart() { actionsLeft = maxActionsCount; }
        public virtual void OnTurnEnd() { }
    }
}