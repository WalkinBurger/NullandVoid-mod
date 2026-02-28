using System;
using Microsoft.Xna.Framework;
using Terraria;

namespace NullandVoid.Utils
{
	public static partial class NullandVoidUtils
	{
		public static float MouseAngle(Vector2 mouseScreen, Point screenSize, bool fromNegY = false)
		{
			float x = fromNegY? mouseScreen.Y - screenSize.Y / 2 : mouseScreen.X - screenSize.X / 2;
			float y = fromNegY? (screenSize.X / 2) - mouseScreen.X : (screenSize.Y / 2) - mouseScreen.Y;

			return x switch {
				> 0 => MathF.Atan((y / x)),
				< 0 when y >= 0 => MathF.Atan((y / x)) + MathHelper.Pi,
				< 0 => MathF.Atan((y / x)) - MathHelper.Pi,
				0 when y > 0 => MathHelper.PiOver2,
				0 when y < 0 => -MathHelper.PiOver2,
				_ => 0,
			};
		}
	}
}