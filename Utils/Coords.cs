using Microsoft.Xna.Framework;
using Terraria;

namespace NullandVoid.Utils
{
	public partial class NullandVoidUtils
	{
		public static Vector2 ScreenCenter() => new((float)Main.screenWidth / 2, (float)Main.screenHeight / 2);
		
		public static Vector2 ToScreenCoords(Vector2 worldCoords) => new(worldCoords.X - Main.screenPosition.X, worldCoords.Y - Main.screenPosition.Y);
	}
}