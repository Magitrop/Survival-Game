using Game.Interfaces;
using Game.Miscellaneous;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Game.Controllers
{
    public sealed class MainMenuController : IBehaviour
    {
		// singleton
		private MainMenuController() { }
		private static MainMenuController instance;
		/// <summary>
		/// Осуществляет доступ к единственному экземпляру MainMenuController.
		/// </summary>
		/// <returns></returns>
		public static MainMenuController Instance
		{
			get
			{
				if (instance == null)
					instance = new MainMenuController();
				return instance;
			}
		}

		/// <summary>
		/// Очередь изображений, требующих рендера в текущем кадре.
		/// </summary>
		public Queue<Frame> renderQueue;

		private Rectangle buttonDest;
		private Rectangle buttonSrc;
		private Rectangle buttonSelectedSrc;
		private Rectangle buttonDisabledSrc;
		private Rectangle backgroundDest;
		private Point buttonTextDest;

		private Font buttonTextFont;
		private Brush buttonTextBrush;
		private Brush backgroundBrush;

		private Size buttonSize = new Size(400, 80);

		public void Start()
        {
			renderQueue = new Queue<Frame>();

			buttonDest = new Rectangle();
			backgroundDest = new Rectangle(0, 0, Constants.WINDOW_WIDTH, Constants.WINDOW_HEIGHT);
			buttonDest.Size = buttonSize;

			buttonSrc = new Rectangle(128, 0, 32, 32);
			buttonSelectedSrc = new Rectangle(160, 0, 32, 32);
			buttonDisabledSrc = new Rectangle(192, 0, 32, 32);
			buttonTextDest = new Point();

			buttonTextFont = new Font(Fonts.fonts.Families[0], 25.0F, FontStyle.Bold);
			buttonTextBrush = new SolidBrush(Color.White);
			backgroundBrush = new SolidBrush(Color.Black);
		}

		public void PreUpdate()
        {

        }

        public void Update()
        {
			Render(backgroundBrush, backgroundDest);

			buttonDest.X = (Constants.WINDOW_WIDTH - buttonSize.Width) / 2;
			buttonDest.Y = (Constants.WINDOW_HEIGHT - 4 * (buttonSize.Height - 10)) / 2;
			buttonTextDest.X = (Constants.WINDOW_WIDTH - buttonSize.Width / 3) / 2;
			buttonTextDest.Y = (Constants.WINDOW_HEIGHT - 4 * (buttonSize.Height - 10) + buttonSize.Height / 2) / 2;
			if (buttonDest.Contains(MainWindow.mousePosition))
			{
				Render(Constants.uiSheet, buttonDest, buttonSelectedSrc);
				if (MainWindow.leftMouseButton)
				{
					MainWindow.leftMouseButton = false;
					if (File.Exists(Directory.GetCurrentDirectory() + "\\Saves\\save.dat"))
						File.Delete(Directory.GetCurrentDirectory() + "\\Saves\\save.dat");
					MainWindow.SwitchGameState(GameState.InGame);
				}
			}
			else
				Render(Constants.uiSheet, buttonDest, buttonSrc);
			Render("Новая игра", buttonTextDest, buttonTextFont, buttonTextBrush);

			buttonDest.X = (Constants.WINDOW_WIDTH - buttonSize.Width) / 2;
			buttonDest.Y = (Constants.WINDOW_HEIGHT - buttonSize.Height) / 2;
			buttonTextDest.X = (Constants.WINDOW_WIDTH - buttonSize.Width / 3) / 2;
			buttonTextDest.Y = (Constants.WINDOW_HEIGHT - buttonSize.Height / 2) / 2;
			if (File.Exists(Directory.GetCurrentDirectory() + "\\Saves\\save.dat"))
			{
				if (buttonDest.Contains(MainWindow.mousePosition))
				{
					Render(Constants.uiSheet, buttonDest, buttonSelectedSrc);
					if (MainWindow.leftMouseButton)
					{
						MainWindow.leftMouseButton = false;
						MainWindow.SwitchGameState(GameState.InGame);
					}
				}
				else
					Render(Constants.uiSheet, buttonDest, buttonSrc);
			}
			else
				Render(Constants.uiSheet, buttonDest, buttonDisabledSrc);
			Render("Продолжить", buttonTextDest, buttonTextFont, buttonTextBrush);

			buttonDest.X = (Constants.WINDOW_WIDTH - buttonSize.Width) / 2;
			buttonDest.Y = (Constants.WINDOW_HEIGHT + (buttonSize.Height + 10) * 3 / 2) / 2;
			buttonTextDest.X = (Constants.WINDOW_WIDTH - buttonSize.Width * 2 / 5) / 2;
			buttonTextDest.Y = (Constants.WINDOW_HEIGHT + (buttonSize.Height + 10) * 3 / 2 + buttonSize.Height / 2) / 2;
			if (buttonDest.Contains(MainWindow.mousePosition))
			{
				Render(Constants.uiSheet, buttonDest, buttonSelectedSrc);
				if (MainWindow.leftMouseButton)
				{
					MainWindow.leftMouseButton = false;
					Application.Exit();
				}
			}
			else
				Render(Constants.uiSheet, buttonDest, buttonSrc);
			Render("Выйти из игры", buttonTextDest, buttonTextFont, buttonTextBrush);
		}

		public void OnPress(object sender, KeyEventArgs e)
        {

        }

		public void PostUpdate()
        {

        }

		/// <summary>
		/// Добавляет изображение в очередь для рендера.
		/// </summary>
		public void Render(Image img, Rectangle dest, Rectangle src)
		{
			renderQueue.Enqueue(new ImageFrame(img, dest, src));
		}

		/// <summary>
		/// Добавляет текст в очередь для рендера.
		/// </summary>
		public void Render(string text, Point dest, Font font, Brush br)
		{
			renderQueue.Enqueue(new TextFrame(text, dest, font, br));
		}

		/// <summary>
		/// Добавляет прямоугольник в очередь для рендера.
		/// </summary>
		public void Render(Brush br, Rectangle dest)
		{
			renderQueue.Enqueue(new RectFrame(br, dest));
		}

		public void OnPaint(object sender, PaintEventArgs e)
        {
			Graphics graphics = e.Graphics;

			graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
			graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
			foreach (var frame in renderQueue)
			{
				if (frame.GetFrameType() == "rect")
				{
					RectFrame request = (RectFrame)frame;
					graphics.FillRectangle(request.brush, request.destRect);
				}
				else if (frame.GetFrameType() == "image")
				{
					ImageFrame request = (ImageFrame)frame;
					graphics.DrawImage(request.imageToRender, request.destRect, request.srcRect, GraphicsUnit.Pixel);
				}
				else if (frame.GetFrameType() == "text")
				{
					TextFrame request = (TextFrame)frame;
					graphics.DrawString(request.textToRender, request.font, request.brush, request.destPoint);
				}
			}
			renderQueue.Clear();
		}
	}
}