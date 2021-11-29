using Game.Controllers;
using Game.Miscellaneous;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.GameObjects.Creatures
{
	public abstract class Creature : TurnBasedObject
	{
		public int maxHealth;
		public int currentHealth;
		public int damageAmount;
		public bool isAlive;

		protected Creature(
			int _x, 
			int _y, 
			int ID, 
			string name, 
			Image _sprite, 
			byte[] additionalInformation = null) : base(_x, _y, ID, name, _sprite, additionalInformation) { }

		public static bool DealDamage(Creature target, Creature sender, int amount)
        {
			sender?.OnDealingDamage(amount, target);
			return target.GetDamage(amount, sender);
		}
		public static void Heal(Creature target, Creature sender, int amount)
		{
			sender?.OnHealing(amount, target);
			target.GetHealing(amount, sender);
		}

		/// <summary>
		/// Возвращает true, если урон убил существо.
		/// </summary>
		/// <param name="amount"></param>
		/// <param name="sender"></param>
		/// <returns></returns>
		protected bool GetDamage(int amount, Creature sender = null)
		{
			OnGettingDamage(amount, sender);
			if ((currentHealth = MathOperations.MoveTowards(currentHealth, 0, amount)) <= 0)
			{
				sender?.OnKilling(this);
				isAlive = false;
				return true;
			}
			return false;
		}

		protected void GetHealing(int amount, Creature sender = null)
		{
			OnGettingHealing(amount, sender);
			currentHealth = MathOperations.MoveTowards(currentHealth, maxHealth, amount);
		}

		protected virtual void OnKilling(Creature target) { }
		protected virtual void OnDealingDamage(int amount, Creature target) { }
		protected virtual void OnHealing(int amount, Creature target) { }
		protected virtual void OnGettingDamage(int amount, Creature sender) { }
		protected virtual void OnGettingHealing(int amount, Creature sender) { }
	}
}