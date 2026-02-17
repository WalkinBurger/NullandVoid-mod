using System;

namespace NullandVoid.Utils
{
	public static class Easing
	{
		public static float EaseInPow(float x, int n) {
			return MathF.Pow(x, n);
		}
		
		public static float EaseOutPow(float x, int n) {
			return 1 - MathF.Pow(1 - x, n);
		}

		public static float EaseInOutQuad(float x) {
			return x < 0.5
				? x * x 
				: 1 - MathF.Pow(-2 * x + 2, 2) / 2;	
		}
		
		public static float EaseInOutCubic(float x) {
			return x < 0.5f
				? 4 * x * x * x 
				: 1 - MathF.Pow(-2 * x + 2, 3) / 2;
		}
	}
}