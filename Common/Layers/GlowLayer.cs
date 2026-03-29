using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NullandVoid.Common.Players;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace NullandVoid.Common.Layers
{
	public class GlowLayer : PlayerDrawLayer
	{
		private static Asset<Texture2D> glowStar;
		
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			return drawInfo.drawPlayer.GetModPlayer<ParryPlayer>().ParryFrame != 0;
		}

		public override void Load() {
			glowStar = ModContent.Request<Texture2D>("NullandVoid/Assets/Textures/GlowStar");
		}

		public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.LastVanillaLayer);

		protected override void Draw(ref PlayerDrawSet drawInfo) {
			Player player = drawInfo.drawPlayer;
			ParryPlayer parryPlayer = player.GetModPlayer<ParryPlayer>();
			
			float t = MathF.Pow((float)(parryPlayer.ParryFrame + 1) / 20, 4) * ModContent.GetInstance<NullandVoidClientConfig>().ParryFlashIntensity;
			if (player.GetModPlayer<MovementClassPlayer>().AbilityFrame != 0) {
				t -= 0.15f;
			}

			if (t <= 0.05f) {
				return;
			}
			
			
			Vector2 screenCenter = new(Main.screenWidth / 2, Main.screenHeight / 2);
			Vector2 glowPosition;
			if (parryPlayer.SwordParry) {
				glowPosition = 3 * (player.GetFrontHandPosition(player.compositeFrontArm.stretch, player.compositeFrontArm.rotation - 1 * player.direction) - player.MountedCenter) + screenCenter;
			}
			else {
				glowPosition = 2 * (player.GetBackHandPosition(player.compositeBackArm.stretch, player.compositeBackArm.rotation - 1 * player.direction) - player.MountedCenter) + screenCenter;
			}
			
			drawInfo.DrawDataCache.Add(new DrawData(
				glowStar.Value, 
				glowPosition,
				new Rectangle(0, 0, glowStar.Width(), glowStar.Height()),
				new Color(t + 0.05f, t + 0.05f, t, 0f),
				parryPlayer.SwordParry? player.compositeFrontArm.rotation : player.compositeBackArm.rotation,
				new Vector2(glowStar.Width() / 2, glowStar.Height() / 2),
				0.9f + 0.05f * (parryPlayer.ParriedNPCs.Count + parryPlayer.ParriedProjectiles.Count),
				SpriteEffects.None, 0)
			);
		}
	}
}