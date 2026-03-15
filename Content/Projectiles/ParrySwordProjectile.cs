using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NullandVoid.Common.Globals.Items;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace NullandVoid.Content.Projectiles
{
	public class ParrySwordProjectile : ModProjectile
	{
		private static Texture2D slashTexture = ModContent.Request<Texture2D>("NullandVoid/Assets/Textures/Slash").Value;
		private Asset<Texture2D> swordTexture;
		private int parrySword = -1;

		public override string Texture => "NullandVoid/Assets/Textures/Slash";

		public override void SetDefaults() {
			Projectile.timeLeft = 20;
			Projectile.width = 0;
			Projectile.height = 0;
		}
		
		public override bool PreDraw(ref Color lightColor) {
			Player player = Main.player[Projectile.owner];

			if (parrySword == -1) {
				for (int i = 0; i < 11; i++) {
					Item item = player.inventory[i];
					if (item.IsAir || (item.useStyle != SwordGlobalItem.SwordUseStyle && item.useStyle != ItemUseStyleID.Rapier)) {
						if (i == 10) {
							return false;
						}
						continue;
					}

					parrySword = item.type;
					break;
				}
			}

			
			if (swordTexture == null) {
				Main.instance.LoadItem(parrySword);
				swordTexture = TextureAssets.Item[parrySword];
			}
			float swordAngle = player.compositeBackArm.rotation + MathHelper.PiOver2 * player.direction;
			if (player.direction == -1) {
				swordAngle += MathHelper.Pi * 1.5f;
			}

			Vector2 armPosition = player.GetBackHandPosition(player.compositeBackArm.stretch, player.compositeBackArm.rotation) - player.MountedCenter;
			if (Main.myPlayer != Projectile.owner) {
				armPosition -= Main.LocalPlayer.Center - player.MountedCenter;
			}

			Vector2 screenCenter = new(Main.screenWidth / 2, Main.screenHeight / 2);

			Main.EntitySpriteDraw(
				swordTexture.Value,
				armPosition + screenCenter,
				new Rectangle(0, 0, swordTexture.Width(), swordTexture.Height()),
				lightColor,
				swordAngle,
				new Vector2(0, swordTexture.Height()),
				1f,
				SpriteEffects.None
			);

			float t = Math.Clamp(MathF.Pow((float)(Projectile.timeLeft - 5) / 15, 3), 0, 1) - 0.15f;
			float slashAngle = swordAngle - 0.65f * player.direction;
			Main.EntitySpriteDraw(
				slashTexture,
				armPosition + screenCenter,
				new Rectangle(0, 0, slashTexture.Width, slashTexture.Height),
				new Color(t, t, t, 0f),
				slashAngle,
				new Vector2(0, slashTexture.Height),
				swordTexture.Size().Length() * 2 / slashTexture.Size().Length(),
				SpriteEffects.None
			);

			return false;
		}

		public override void AI() {
			Player player = Main.player[Projectile.owner];
			float parryFrame = (float)(20 - Projectile.timeLeft) / 20;
			player.ChangeDir((int)Projectile.ai[0]);
			
			float t = MathF.Pow(parryFrame, 3) - 0.75f * parryFrame - 1.25f * MathF.Pow(2, parryFrame * -20) - 0.5f;
			float armAngle = t * player.direction + Projectile.knockBack;
			if (player.direction == -1) {
				armAngle -= MathHelper.Pi;
			}
			player.SetCompositeArmBack(true, Projectile.timeLeft > 15 ? Player.CompositeArmStretchAmount.Quarter : Player.CompositeArmStretchAmount.Full, armAngle);
		}
	}
}