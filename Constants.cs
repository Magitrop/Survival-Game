using System;
using System.Collections.Generic;
using System.Drawing;
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
        public const float OBJECTS_DESPAWN_RANGE = CHUNK_SIZE * RENDER_DISTANCE * 20f;
    }
}