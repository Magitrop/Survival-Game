using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Miscellaneous
{
	public static class MathOperations
	{
		public static float Clamp(float current, float min, float max)
		{
			if (current < min) return min;
			else if (current > max) return max;
			else return current;
		}
		public static int Clamp(int current, int min, int max)
		{
			if (current < min) return min;
			else if (current > max) return max;
			else return current;
		}

		public static float MoveTowards(float current, float target, float maxDelta)
		{
			if (Math.Abs(target - current) <= maxDelta)
				return target;
			return current + (target - current > 0 ? 1 : -1) * maxDelta;
		}
		public static int MoveTowards(int current, int target, int maxDelta)
		{
			if (Math.Abs(target - current) <= maxDelta)
				return target;
			return current + (target - current > 0 ? 1 : -1) * maxDelta;
		}
		public static byte MoveTowards(byte current, byte target, byte maxDelta)
		{
			if (Math.Abs(target - current) <= maxDelta)
				return target;
			return (byte)(current + (target - current > 0 ? 1 : -1) * maxDelta);
		}
		public static float MoveTowards(float current, float target, float maxDelta, out bool result)
		{
			if (Math.Abs(target - current) <= maxDelta)
			{
				result = true;
				return target;
			}
			result = false;
			return current + (target - current > 0 ? 1 : -1) * maxDelta;
		}
		public static int MoveTowards(int current, int target, int maxDelta, out bool result)
		{
			if (Math.Abs(target - current) <= maxDelta)
			{
				result = true;
				return target;
			}
			result = false;
			return current + (target - current > 0 ? 1 : -1) * maxDelta;
		}

		public static float Distance((int, int) from, (int, int) to)
		{
			return (float)Math.Sqrt(Math.Pow(from.Item1 - to.Item1, 2) + Math.Pow(from.Item2 - to.Item2, 2));
		}

		public static float Distance((float, float) from, (float, float) to)
		{
			return (float)Math.Sqrt(Math.Pow(from.Item1 - to.Item1, 2) + Math.Pow(from.Item2 - to.Item2, 2));
		}

		public static bool IsPointInside(Point point, Rectangle rect, bool includeBorders = false)
		{
			if (includeBorders)
				return
					point.X <= rect.X + rect.Width &&
					point.X >= rect.X &&
					point.Y <= rect.Y + rect.Height &&
					point.Y >= rect.Y;
			else 
				return 
					point.X <= rect.X + rect.Width &&
					point.X >= rect.X &&
					point.Y <= rect.Y + rect.Height &&
					point.Y >= rect.Y;
		}
	}
}