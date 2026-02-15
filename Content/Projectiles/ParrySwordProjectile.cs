using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace NullandVoid.Content.Projectiles
{
	public class ParrySwordProjectile : ModProjectile
	{
		public int ParrySword = -1;
		Texture2D slashTexture = ModContent.Request<Texture2D>("NullandVoid/Assets/Textures/Slash").Value;


		public override void SetDefaults() {
			Projectile.timeLeft = 20;
			Projectile.width = 0;
			Projectile.height = 0;
		}
		
		public override bool PreDraw(ref Color lightColor) {
			Player player = Main.player[Projectile.owner];

			if (ParrySword == -1) {
				Item item = player.HeldItem;
				if (!item.IsAir && item is { melee: true, pick: 0, axe: 0, useStyle: ItemUseStyleID.Swing or ItemUseStyleID.Rapier }) {
					ParrySword = item.type;
				}
				else {
					for (int i = 0; i < 10; i++) {
						item = player.inventory[i];
						if (!item.IsAir && item is { melee: true, pick: 0, axe: 0, useStyle: ItemUseStyleID.Swing or ItemUseStyleID.Rapier }) {
							ParrySword = item.type;
							break;
						}
					}
				}
			}

			if (ParrySword == -1) {
				return false;
			}
			
			Main.instance.LoadItem(ParrySword);
			Texture2D swordTexture = TextureAssets.Item[ParrySword].Value;
			float swordAngle = player.compositeBackArm.rotation + 1.2f * player.direction;
			if (player.direction == -1) {
				swordAngle += MathHelper.Pi * 1.5f;
			}

			Vector2 armPosition = player.GetBackHandPosition(player.compositeBackArm.stretch, player.compositeBackArm.rotation) - player.Center;
			if (Main.myPlayer != Projectile.owner) {
				armPosition -= Main.LocalPlayer.Center - player.Center;
			}

			Vector2 screenCenter = new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);

			Main.EntitySpriteDraw(
				swordTexture,
				armPosition + screenCenter,
				new Rectangle(0, 0, swordTexture.Width, swordTexture.Height),
				lightColor,
				swordAngle,
				new Vector2(0, swordTexture.Height),
				1f,
				SpriteEffects.None
			);

			float t = Math.Clamp((float)Math.Pow((float)(Projectile.timeLeft - 5) / 15, 3), 0, 1) - 0.15f;
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

			float t = (float)Math.Pow(parryFrame, 3) - 0.75f * parryFrame - 1.25f * (float)Math.Pow(2, parryFrame * -20);
			float armAngle = t * player.direction * MathHelper.PiOver2 + Projectile.knockBack;
			player.SetCompositeArmBack(true, Projectile.timeLeft > 15 ? Player.CompositeArmStretchAmount.Quarter : Player.CompositeArmStretchAmount.Full, armAngle);
		}
	}
}