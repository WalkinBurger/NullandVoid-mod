using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NullandVoid.Common.Players;
using ReLogic.Content;
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

			Vector2 position = new(Main.screenWidth / 2, (Main.screenHeight / 2) - 10);
			if (player.bodyFrame.Y / 56 >= 17 || (player.bodyFrame.Y / 56 < 14 && player.bodyFrame.Y / 56 >= 10)) {
				position.Y += 2;
			}
			Color lightColor = Lighting.GetColor(player.Center.ToTileCoordinates());
			
			position.X -= 8 - (6 * player.direction);
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