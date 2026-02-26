using System;
using Microsoft.Xna.Framework;
using NullandVoid.Common.Players;
using NullandVoid.Content.Projectiles;
using NullandVoid.Utils;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NullandVoid.Common.Globals.Items
{
	public class SwordGlobalItem : GlobalItem
	{
		public static int SwordUseStyle;

		public override void Load() {
			SwordUseStyle = ItemLoader.RegisterUseStyle(Mod, "SwordUseStyle");
		}

		public override bool AppliesToEntity(Item entity, bool lateInstantiation) {
			return entity is { melee: true, pick: 0, axe: 0, hammer: 0, useStyle: ItemUseStyleID.Swing };
		}

		public override void SetDefaults(Item item) {
			item.useStyle = SwordUseStyle;
		}

		public override void UseStyle(Item item, Player player, Rectangle heldItemFrame) {
			SwordPlayer swordPlayer = player.GetModPlayer<SwordPlayer>();

			if (player.ItemAnimationJustStarted) {
				swordPlayer.HitResetTimer = player.itemAnimationMax * 2;
				swordPlayer.HitStyle = (swordPlayer.HitStyle + 1) % 3;
				swordPlayer.HitDirection = (int)MathF.Round(Math.Clamp(Main.MouseScreen.X - Main.screenWidth / 2, -1, 1));
				if (swordPlayer.HitDirection == 0) {
					swordPlayer.HitDirection = player.direction;
				}
				float hitAngle = MathF.Atan((Main.MouseScreen.Y - Main.screenHeight / 2) / (Math.Abs(Main.MouseScreen.X - Main.screenWidth / 2)) * swordPlayer.HitDirection) -  MathHelper.PiOver2 * swordPlayer.HitDirection;
				float offsetAngle = 1.4f * swordPlayer.HitDirection;
				switch (swordPlayer.HitStyle) {
					case 0:
						player.itemAnimationMax = (int)(player.itemAnimationMax * 1.5f);
						player.itemAnimation = player.itemAnimationMax;
						swordPlayer.HitAngleRange = [hitAngle - offsetAngle, hitAngle + offsetAngle * 1.25f];
						break;
					case 1:
						swordPlayer.HitAngleRange = [hitAngle - offsetAngle, hitAngle + offsetAngle];
						break;
					case 2:
						swordPlayer.HitAngleRange = [hitAngle + offsetAngle, hitAngle - offsetAngle];
						break;
				}
				swordPlayer.HitAngleRange[0] = swordPlayer.HitDirection == 1 ? Math.Clamp(swordPlayer.HitAngleRange[0], -4, 1) : Math.Clamp(swordPlayer.HitAngleRange[0], -1, 4);
				swordPlayer.HitAngleRange[1] = swordPlayer.HitDirection == 1 ? Math.Clamp(swordPlayer.HitAngleRange[1], -4, 1) : Math.Clamp(swordPlayer.HitAngleRange[1], -1, 4);
				
				if (player.whoAmI != Main.myPlayer) {
					return;
				}

				Projectile.NewProjectile(player.GetSource_FromThis(), player.MountedCenter, Vector2.Zero, ModContent.ProjectileType<SwordSlashProjectile>(), 0, 0, Main.myPlayer, player.itemAnimationMax);
				player.GetModPlayer<ParryPlayer>().DoParry(true, 6);
					
				MovementMiscPlayer movementMiscPlayer = player.GetModPlayer<MovementMiscPlayer>();
				if (movementMiscPlayer.CanPogo(PogoTypes.Sword)) {
					movementMiscPlayer.Pogo(swordPlayer.HitStyle == 0? 16 : 12);
				}
			}

			
			player.ChangeDir(swordPlayer.HitDirection);
			player.itemTime = player.itemAnimation;

			float hitProgress = 1 - ((float)player.itemAnimation / player.itemAnimationMax);
			float swingAngle = MathHelper.Lerp(swordPlayer.HitAngleRange[0], swordPlayer.HitAngleRange[1], NullandVoidUtils.OutElastic(hitProgress));
			player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, swingAngle);
			player.itemLocation = player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, player.compositeFrontArm.rotation);
			player.itemRotation = swingAngle + 2f * swordPlayer.HitDirection;
			player.FlipItemLocationAndRotationForGravity();
		}

		public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage) {
			SwordPlayer swordPlayer = player.GetModPlayer<SwordPlayer>();
			if (swordPlayer.HitStyle == 0 && swordPlayer.HitResetTimer != 0) {
				damage.Multiplicative = 1.5f;
			}
		}

		public override void ModifyWeaponKnockback(Item item, Player player, ref StatModifier knockback) {
			SwordPlayer swordPlayer = player.GetModPlayer<SwordPlayer>();
			if (swordPlayer.HitStyle == 0 && swordPlayer.HitResetTimer != 0) {
				knockback.Multiplicative = 1.5f;
			}
		}

		public override void UseItemFrame(Item item, Player player) {
			player.bodyFrame.Y = player.bodyFrame.Height;
		}

		public override void UseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox) {
			SwordPlayer swordPlayer = player.GetModPlayer<SwordPlayer>();
			
			Vector2 handPostion = player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, player.compositeFrontArm.rotation);
			float x = NullandVoidUtils.OutElastic(1 - ((float)player.itemAnimation / player.itemAnimationMax));
			float size = MathF.Sqrt(hitbox.Width * hitbox.Width + hitbox.Height * hitbox.Height) * (MathF.Sin(x * MathHelper.Pi) * 1.3f + (float)(((swordPlayer.HitStyle + 2) % 3) + 1) / 4);
			Vector2 tipPosition = handPostion - new Vector2(size * MathF.Sin(player.compositeFrontArm.rotation), size * -MathF.Cos(player.compositeFrontArm.rotation));
			hitbox = Terraria.Utils.CornerRectangle(handPostion, tipPosition);
			
			/* funny dust spam hitbox debug
			for (int i = 0; i < 10; i++) {
				Dust dust = Dust.NewDustDirect(new Vector2(MathHelper.Lerp(hitbox.BottomLeft().X, hitbox.BottomRight().X, (float)i / 10), MathHelper.Lerp(hitbox.BottomLeft().Y, hitbox.BottomRight().Y, (float)i / 10)), 1 ,1, DustID.Flare, 0f, 0f);
				dust.noGravity = true;
				dust = Dust.NewDustDirect(new Vector2(MathHelper.Lerp(hitbox.BottomRight().X, hitbox.TopRight().X, (float)i / 10), MathHelper.Lerp(hitbox.BottomRight().Y, hitbox.TopRight().Y, (float)i / 10)), 1 ,1, DustID.Flare, 0f, 0f);
				dust.noGravity = true;
				dust = Dust.NewDustDirect(new Vector2(MathHelper.Lerp(hitbox.TopRight().X, hitbox.TopLeft().X, (float)i / 10), MathHelper.Lerp(hitbox.TopRight().Y, hitbox.TopLeft().Y, (float)i / 10)), 1 ,1, DustID.Flare, 0f, 0f);
				dust.noGravity = true;
				dust = Dust.NewDustDirect(new Vector2(MathHelper.Lerp(hitbox.TopLeft().X, hitbox.BottomLeft().X, (float)i / 10), MathHelper.Lerp(hitbox.TopLeft().Y, hitbox.BottomLeft().Y, (float)i / 10)), 1 ,1, DustID.Flare, 0f, 0f);
				dust.noGravity = true;
			}
			*/
		}
	}
}