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

		protected BreakableObject(
			int _x,
			int _y,
			int ID,
			string name,
			Image _sprite,
			byte[] additionalInformation = null) : base(_x, _y, ID, name, _sprite, additionalInformation) 
		{
			//Debug.WriteLine(additionalInformation?.Length);
			if (additionalInformation != null && additionalInformation.Length > 1)
				durability = additionalInformation[1];
			else
				durability = 5;
		}
		public bool Break(GameObject breaker, byte force)
		{
			if ((durability = MathOperations.MoveTowards(durability, (byte)0, force)) == 0)
			{
				OnBreak(breaker);
				return true;
			}
			return false;
		}
		protected virtual void OnBreak(GameObject breaker) { }
	}
}