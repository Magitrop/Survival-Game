using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Game
{
    public static class KeyBindings
    {
        public static Keys MoveUp = Keys.Up;
        public static Keys MoveRight = Keys.Right;
        public static Keys MoveDown = Keys.Down;
        public static Keys MoveLeft = Keys.Left;
        public static Keys ToggleGridDrawing = Keys.ControlKey;
        public static Keys SaveGame = Keys.S;
        public static Keys PlaceBlock = Keys.Space;
    }
}