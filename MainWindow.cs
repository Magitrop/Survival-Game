using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Game.Controllers;

namespace Game
{
	public enum GameState
    {
		MainMenu,
		InGame
    }

	public sealed partial class MainWindow : Form
	{
		public bool gameIsRunning { get; private set; }
		private bool continueUpdate;
		public static GameState gameState { get; private set; }
		public static Point mousePosition;
		public static bool leftMouseButton;
		public static bool rightMouseButton;

		public MainWindow()
		{
			InitializeComponent();
			Size = new Size(Constants.WINDOW_WIDTH, Constants.WINDOW_HEIGHT);

			KeyDown += new KeyEventHandler(OnPress);
			MouseDown += new MouseEventHandler(OnMouseDown);
			MouseUp += new MouseEventHandler(OnMouseUp);

			Initialize();

			gameIsRunning = true;
			SwitchGameState(GameState.MainMenu);
			while (gameIsRunning)
			{
				Time.SetFrameBeginning(DateTime.Now.Ticks);
				continueUpdate = false;
				mousePosition = PointToClient(Cursor.Position);

				Refresh();
				Application.DoEvents();

				switch (gameState)
				{
					case GameState.MainMenu:
						if (continueUpdate)
							MainMenuController.Instance.Update();
						break;
					case GameState.InGame:
						if (continueUpdate)
							GameController.Instance.Update();
						break;
				}
				if (gameIsRunning)
					Show();

				Time.CalculateDeltaTime(DateTime.Now.Ticks);
				Time.CalculateTimeSinceStart();
			}
			Application.Exit();
		}

		public static void SwitchGameState(GameState toSet)
        {
			gameState = toSet;
			switch (gameState)
			{
				case GameState.MainMenu:
					GameController.Instance.Exit();
					MainMenuController.Instance.Start();
					break;
				case GameState.InGame:
					GameController.Instance.Start();
					break;
			}
		}

		private void Initialize()
		{
			byte[] fontData = Properties.Resources.font_1;
			IntPtr fontPtr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(fontData.Length);
			System.Runtime.InteropServices.Marshal.Copy(fontData, 0, fontPtr, fontData.Length);
			uint dummy = 0;
			Fonts.fonts.AddMemoryFont(fontPtr, Properties.Resources.font_1.Length);
			Fonts.AddFontMemResourceEx(fontPtr, (uint)Properties.Resources.font_1.Length, IntPtr.Zero, ref dummy);
			System.Runtime.InteropServices.Marshal.FreeCoTaskMem(fontPtr);
			Fonts.font_1 = new Font(Fonts.fonts.Families[0], 32.0F, FontStyle.Bold);
		}

		private void OnPress(object sender, KeyEventArgs e)
		{
			switch (gameState)
			{
				case GameState.MainMenu:
					break;
				case GameState.InGame:
					GameController.Instance.OnPress(sender, e);
					break;
			}
		}

		private void OnMouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
				leftMouseButton = true;
			else if (e.Button == MouseButtons.Right)
				rightMouseButton = true;
		}

		private void OnMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
				leftMouseButton = false;
			else if (e.Button == MouseButtons.Right)
				rightMouseButton = false;
		}

		List<PointF> points = new List<PointF>();
		public static int n = 5;
		public static float curAngle;
		public static float radius = 1200;
		public static bool DrawWindowClosingAnimation = false;
		private void OnPaint(object sender, PaintEventArgs e)
		{
			/*GraphicsPath g = new GraphicsPath();
			points = new List<PointF>();
			for (int i = 0; i < n; i++)
				points.Add(new PointF
					(
						(float)Math.Cos(curAngle + i * 2 * Math.PI / n) * radius + 500,
						(float)Math.Sin(curAngle + i * 2 * Math.PI / n) * radius + 500
					));
			g.AddPolygon(points.ToArray());
			Region = new Region(g);*/
			if (DrawWindowClosingAnimation)
			{
				GraphicsPath g = new GraphicsPath();
				g.AddEllipse(new RectangleF(
					Constants.WINDOW_WIDTH / 2 - radius / 2,
					Constants.WINDOW_HEIGHT / 2 - radius / 2,
					radius,
					radius));
				Region = new Region(g);
			}

			switch (gameState)
			{
				case GameState.MainMenu:
					MainMenuController.Instance.OnPaint(sender, e);
					break;
				case GameState.InGame:
					GameController.Instance.OnPaint(sender, e);
					break;
			}
			continueUpdate = true;
		}
	}
}