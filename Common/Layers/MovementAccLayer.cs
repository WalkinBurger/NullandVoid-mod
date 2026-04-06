using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NullandVoid.Common.Players;
using NullandVoid.Utils;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace NullandVoid.Common.Layers
{
	public class MovementAccLayer : PlayerDrawLayer
	{
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			return drawInfo.drawPlayer.GetModPlayer<MovementClassPlayer>().DrawAcc;
		}

		public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.LastVanillaLayer);

		protected override void Draw(ref PlayerDrawSet drawInfo) {
			Player player = drawInfo.drawPlayer;
			MovementClassPlayer movementClassPlayer = player.GetModPlayer<MovementClassPlayer>();

			Vector2 position = NullandVoidUtils.ScreenCenter();
			position.Y -= player.bodyFrame.Y / 56 >= 17 || (player.bodyFrame.Y / 56 < 14 && player.bodyFrame.Y / 56 >= 10) ? 8 : 10;
			position.X -= 8 - 6 * player.direction;
			if (player.whoAmI != Main.myPlayer) {
				position += player.Center - Main.LocalPlayer.Center;
			}

			Color lightColor = Lighting.GetColor(player.Center.ToTileCoordinates());

			drawInfo.DrawDataCache.Add(new DrawData(
				movementClassPlayer.AccTexture.Value,
				position,
				null,
				new Color((int)MathHelper.Lerp(lightColor.R, 255, movementClassPlayer.ChargeSpeed), (int)MathHelper.Lerp(lightColor.G, 255, movementClassPlayer.ChargeSpeed), (int)MathHelper.Lerp(lightColor.B, 255, movementClassPlayer.ChargeSpeed)),
				0,
				Vector2.Zero,
				1,
				player.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally
			));
		}
	}
}