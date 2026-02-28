using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NullandVoid.Common.Players;
using NullandVoid.Utils;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace NullandVoid.Content.Projectiles
{
	public class SwordSlashProjectile : ModProjectile
	{
		public Texture2D SlashTexture = ModContent.Request<Texture2D>("NullandVoid/Assets/Textures/Slash", AssetRequestMode.ImmediateLoad).Value;
		public float Scale;
		
		public override void SetDefaults() {
			Projectile.height = 0;
			Projectile.width = 0;
		}

		public override string Texture {
			get { return "NullandVoid/Assets/Textures/Slash"; }
		}

		public override bool PreDraw(ref Color lightColor) {
			Player player = Main.player[Projectile.owner];
			UseStylePlayer useStylePlayer = player.GetModPlayer<UseStylePlayer>();
			Vector2 screenCenter = new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);
			Vector2 armPosition = player.GetFrontHandPosition(player.compositeFrontArm.stretch, player.compositeFrontArm.rotation) - player.MountedCenter;
			float hitProgress = 1 - (Projectile.timeLeft / Projectile.ai[0]);
			float fade = MathF.Pow(Projectile.timeLeft / Projectile.ai[0], 3) - 0.15f;
			float slashAngle = MathHelper.Lerp(useStylePlayer.HitAngleRange[0], useStylePlayer.HitAngleRange[1], NullandVoidUtils.OutElastic(hitProgress));

			if (Main.myPlayer != Projectile.owner) {
				armPosition -= Main.LocalPlayer.Center - player.Center;
			}
			
			if (player.direction == -1) {
				slashAngle -= MathHelper.PiOver2;
			}

			if (player.GetModPlayer<UseStylePlayer>().HitStyle == 2) {
				slashAngle += 2.3f * player.direction;
			}

			Main.EntitySpriteDraw(
				SlashTexture,
				armPosition + screenCenter,
				new Rectangle(0, 0, SlashTexture.Width, SlashTexture.Height),
				new Color(fade, fade, fade, 0f),
				slashAngle,
				new Vector2(0, SlashTexture.Height),
				Scale,
				SpriteEffects.None
			);
			
			return false;
		}

		public override void AI() {
			if (Projectile.timeLeft <= Projectile.ai[0]) {
				return;
			}
			
			Projectile.timeLeft = (int)Projectile.ai[0];
			Scale = Main.player[Projectile.owner].HeldItem.Size.Length() * 3 / SlashTexture.Size().Length();
		}
	}
}