using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Miscellaneous
{
    public abstract class Frame
    {
        public abstract string GetFrameType();
    }

    public sealed class ImageFrame : Frame
    {
        /// <summary>
        /// Изображение, требующее рендера.
        /// </summary>
        public Image imageToRender;
        /// <summary>
        /// Четырехугольник, определяющий координаты вырезаемой части. 
        /// </summary>
        public Rectangle srcRect;
        /// <summary>
        /// Четырехугольник, определяющий координаты отрисовки. 
        /// </summary>
        public Rectangle destRect;

        public ImageFrame(Image img, Rectangle dest, Rectangle src)
        {
            imageToRender = img;
            destRect = dest;
            srcRect = src;
        }

        public override string GetFrameType()
        {
            return "image";
        }
    }

    public sealed class TextFrame : Frame
    {
        /// <summary>
        /// Текст, требующий рендера.
        /// </summary>
        public string textToRender;
        /// <summary>
        /// Точка, определяющая координаты отрисовки. 
        /// </summary>
        public Point destPoint;
        public Brush brush;
        public Font font;

        public TextFrame(string text, Point dest, Font f, Brush br)
        {
            textToRender = text;
            destPoint = dest;
            font = f;
            brush = br;
        }

        public override string GetFrameType()
        {
            return "text";
        }
    }
}
