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
	public abstract class BreakableObject : GameObject
	{
		public byte durability;
		public byte maxDurability;
		public ToolType shouldBeBrokenWith;

		protected BreakableObject(
			int _x,
			int _y,
			int ID,
			string name,
			Image _sprite,
			byte[] additionalInformation = null) : base(_x, _y, ID, name, _sprite, additionalInformation) { }
		public bool Break(GameObject breaker, byte force, ToolType wasBrokenWith)
		{
			if (force > 1 && wasBrokenWith != shouldBeBrokenWith)
				force = 1;
			if ((durability = MathOperations.MoveTowards(durability, (byte)0, force)) == 0)
			{
				OnBreak(breaker, wasBrokenWith);
				return true;
			}
			return false;
		}
		protected virtual void OnBreak(GameObject breaker, ToolType wasBrokenWith) { }
	}
}