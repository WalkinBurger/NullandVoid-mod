using System;
using Microsoft.Xna.Framework;

namespace NullandVoid.Utils
{
	public static partial class NullandVoidUtils
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

		public static float FadeInOut(float x, float cutoff, int pow) {
			if (x <= cutoff) {
				return 1 - MathF.Pow(1 - (x / cutoff), pow);
			}
			if (1 - x <= cutoff) {
				return 1 - MathF.Pow(1 - ((1 - x) / cutoff), pow);
			}
			return 1;
		}
		
		public static float OutElastic(float x) {
			return x switch {
				0 => 0,
				1 => 1,
				_ => MathF.Pow(2, -7 * (x - 0.15f)) * MathF.Sin(x * 3 - 0.75f) + 1,
			};
		}
	}
}