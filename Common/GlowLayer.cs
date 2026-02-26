using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NullandVoid.Common.Players;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace NullandVoid.Common
{
	public class GlowLayer : PlayerDrawLayer
	{
		private Texture2D glowStar;
		
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			return drawInfo.drawPlayer.GetModPlayer<ParryPlayer>().ParryFrame != 0;
		}

		public override void Load() {
			glowStar = ModContent.Request<Texture2D>("NullandVoid/Assets/Textures/GlowStar", AssetRequestMode.ImmediateLoad).Value;
		}

		public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.LastVanillaLayer);

		protected override void Draw(ref PlayerDrawSet drawInfo) {
			Player player = drawInfo.drawPlayer;
			ParryPlayer parryPlayer = player.GetModPlayer<ParryPlayer>();
			StaminaPlayer staminaPlayer =  player.GetModPlayer<StaminaPlayer>();
			
			float t = MathF.Pow((float)(parryPlayer.ParryFrame + 1) / 20, 4) * ModContent.GetInstance<NullandVoidClientConfig>().ParryFlashIntensity;
			if (staminaPlayer.DashFrame != 0) {
				t -= 0.15f;
			}

			if (t <= 0.05f) {
				return;
			}
			
			
			Vector2 screenCenter = new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);
			Vector2 glowPosition = 2 * (player.GetBackHandPosition(player.compositeBackArm.stretch, player.compositeBackArm.rotation - 1 * player.direction) - player.MountedCenter) + screenCenter;
			
			drawInfo.DrawDataCache.Add(new DrawData(
				glowStar, 
				glowPosition,
				new Rectangle(0, 0, glowStar.Width, glowStar.Height),
				new Color(t + 0.05f, t + 0.05f, t, 0f),
				player.compositeBackArm.rotation,
				new Vector2(glowStar.Width / 2, glowStar.Height / 2),
				0.85f + 0.15f * (parryPlayer.ParriedNPCs.Count + parryPlayer.ParriedProjectiles.Count),
				SpriteEffects.None, 0)
			);
		}
	}
}