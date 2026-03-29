using NullandVoid.Utils;

namespace NullandVoid.Core
{
	public static class MovementClassID
	{
		public const int None = 0;
		public const int Dasher = 1;
		public const int Charger = 2;
		public const int Spirit = 3;
		public const int Grappler = 4;
	}

	public class MovementStatI
	{
		private int stat;
		private SimpleStatModifier modifier;

		public void Set(int newStat) {
			stat = newStat;
		}

		public void Set(SimpleStatModifier newModifier) {
			modifier = newModifier;
		}
		
		public void Set(int newStat, SimpleStatModifier newModifier) {
			stat = newStat;
			modifier = newModifier;
		}

		public MovementStatI(int stat = 0) {
			Set(stat, SimpleStatModifier.Default);
		}
		
		public MovementStatI(SimpleStatModifier modifier, int stat) {
			Set(stat, modifier);
		}
		
		public int Get() => modifier.ApplyTo(stat);
	}

	public class MovementStatF
	{
		private float stat;
		private SimpleStatModifier modifier;

		public void Set(float newStat) {
			stat = newStat;
		}

		public void Set(SimpleStatModifier newModifier) {
			modifier = newModifier;
		}
		
		public void Set(float newStat, SimpleStatModifier newModifier) {
			stat = newStat;
			modifier = newModifier;
		}

		public MovementStatF(float stat = 0) {
			Set(stat, SimpleStatModifier.Default);
		}

		public MovementStatF(SimpleStatModifier modifier, float stat = 0) {
			Set(stat, modifier);
		}
		
		public float Get() => modifier.ApplyTo(stat);
	}
	
	public class StatMovement
	{
		public MovementStatI StaminaCost = new();
		
		public MovementStatI DashTime = new();
		public MovementStatF DashSpeed = new();
		public MovementStatI IFrame = new();

		public MovementStatF MaxSpeed = new();
		public MovementStatF ChargeAccel = new();
		public MovementStatF TurnAccel = new();
		public MovementStatF DamageReduction = new();
		public MovementStatI ImpactDamage = new();

		public MovementStatF DistanceDecayRate = new();
		public MovementStatI DamageCost = new();
		public MovementStatF SpiritSpeed = new();
		public MovementStatF FlingSpeed = new();

		public MovementStatI Range = new();
		public MovementStatF PullSpeed = new();
		public MovementStatF ReelSpeed = new();

		public void Reset() {
			StaminaCost.Set(SimpleStatModifier.Default);
			DashTime.Set(SimpleStatModifier.Default);
			DashSpeed.Set(SimpleStatModifier.Default);
			IFrame.Set(SimpleStatModifier.Default);
			MaxSpeed.Set(SimpleStatModifier.Default);
			ChargeAccel.Set(SimpleStatModifier.Default);
			TurnAccel.Set(SimpleStatModifier.Default);
			DamageReduction.Set(SimpleStatModifier.Default);
			ImpactDamage.Set(SimpleStatModifier.Default);
			DistanceDecayRate.Set(SimpleStatModifier.Default);
			DamageCost.Set(SimpleStatModifier.Default);
			SpiritSpeed.Set(SimpleStatModifier.Default);
			FlingSpeed.Set(SimpleStatModifier.Default);
			Range.Set(SimpleStatModifier.Default);
			PullSpeed.Set(SimpleStatModifier.Default);
			ReelSpeed.Set(SimpleStatModifier.Default);
		}
	}
}