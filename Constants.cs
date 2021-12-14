using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public static class Constants
    {
        public const float ZOOM = 5f;
        public const int TEXTURE_RESOLUTION = 16;
        public const int CHUNK_SIZE = 8;
        public const int RENDER_DISTANCE = 2;
        public const int WINDOW_WIDTH = 1200;
        public const int WINDOW_HEIGHT = 700;
        public const float TILE_SIZE = ZOOM * TEXTURE_RESOLUTION;
        public const float OBJECTS_DESPAWN_RANGE = CHUNK_SIZE * RENDER_DISTANCE * 1.5f;
        public const int MAX_CREATURES_SPAWN_PER_CHUNK = 2;

        public static readonly Dictionary<string, Image> creatureSheets;
        public static readonly Image itemsSheet;
        public static readonly Image uiSheet;
        public static readonly Image tilesSheet;
        public static readonly Image floorsSheet;
        public static readonly Image objectsSheet;

        static Constants()
        {
            creatureSheets = new Dictionary<string, Image>();
            creatureSheets["creature_hero"] = new Bitmap(new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "\\Sprites\\hero.png");
            creatureSheets["creature_wolf"] = new Bitmap(new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "\\Sprites\\wolf.png");
            creatureSheets["creature_bear"] = new Bitmap(new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "\\Sprites\\bear.png");

            itemsSheet = new Bitmap(new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "\\Sprites\\items.png");
            uiSheet = new Bitmap(new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "\\Sprites\\ui.png");
            tilesSheet = new Bitmap(new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "\\Sprites\\tiles_floor.png");
            floorsSheet = new Bitmap(new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "\\Sprites\\tiles.png");
            objectsSheet = new Bitmap(new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "\\Sprites\\objects.png");
        }
    }
}