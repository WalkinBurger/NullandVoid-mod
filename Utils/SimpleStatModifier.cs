namespace NullandVoid.Utils
{
	public struct SimpleStatModifier(float flat, float additive = 1)
	{
		public static readonly SimpleStatModifier Default = new(0);
		public float Flat = flat;
		public float Additive = additive;

		public int ApplyTo(int value) => (int)(value * Additive + Flat);

		public float ApplyTo(float value) => value * Additive + Flat;
	}
}