using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NullandVoid.Common.Players;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace NullandVoid.Common
{
	public class BloomLayer : PlayerDrawLayer
	{
		private float parryFlashIntensity = ModContent.GetInstance<NullandVoidClientConfig>().ParryFlashIntensity;

		public void ChangeConfig() {
			parryFlashIntensity = ModContent.GetInstance<NullandVoidClientConfig>().ParryFlashIntensity;
		}
		
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			return drawInfo.drawPlayer.GetModPlayer<ParryPlayer>().ParryFrame != 0;
		}

		public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.LastVanillaLayer);

		protected override void Draw(ref PlayerDrawSet drawInfo) {
			Player player = drawInfo.drawPlayer;
			ParryPlayer parryPlayer = player.GetModPlayer<ParryPlayer>();
			StaminaPlayer staminaPlayer =  player.GetModPlayer<StaminaPlayer>();
			
			float t = MathF.Pow((float)parryPlayer.ParryFrame / 20, 4) * parryFlashIntensity;
			if (staminaPlayer.DashFrame != 0) {
				t -= 0.15f;
			}

			if (t <= 0.05f) {
				return;
			}
			
			Texture2D bloom = ModContent.Request<Texture2D>("NullandVoid/Assets/Textures/Bloom").Value;
			Vector2 screenCenter = new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);
			Vector2 bloomPosition = 2 * (player.GetBackHandPosition(player.compositeBackArm.stretch, player.compositeBackArm.rotation - 1 * player.direction) - player.Center) + screenCenter;
			
			drawInfo.DrawDataCache.Add(new DrawData(
				bloom, 
				bloomPosition,
				new Rectangle(0, 0, bloom.Width, bloom.Height),
				new Color(t + 0.05f, t + 0.05f, t, 0f),
				player.compositeBackArm.rotation,
				new Vector2(bloom.Width / 2, bloom.Height / 2),
				1 + 0.25f * (parryPlayer.ParriedNPCs.Count + parryPlayer.ParriedProjectiles.Count),
				SpriteEffects.None, 0)
			);
		}
	}
}