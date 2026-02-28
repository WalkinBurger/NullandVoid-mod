using System;
using Microsoft.Xna.Framework;
using NullandVoid.Common.Players;
using NullandVoid.Content.Projectiles;
using NullandVoid.Core;
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
			UseStylePlayer useStylePlayer = player.GetModPlayer<UseStylePlayer>();

			if (player.ItemAnimationJustStarted && player.whoAmI == Main.myPlayer) {
				useStylePlayer.HitResetTimer = player.itemAnimationMax * 2;
				int style = (useStylePlayer.HitStyle + 1) % 3;
				float hitAngle = NullandVoidUtils.MouseAngle(Main.MouseScreen, Main.ScreenSize, true);
				
				Projectile.NewProjectile(player.GetSource_FromThis(), player.MountedCenter, Vector2.Zero, ModContent.ProjectileType<SwordSlashProjectile>(), 0, 0, Main.myPlayer, player.itemAnimationMax);
				player.GetModPlayer<ParryPlayer>().DoParry(true, 6);
					
				MovementMiscPlayer movementMiscPlayer = player.GetModPlayer<MovementMiscPlayer>();
				useStylePlayer.SetHit(player.whoAmI, hitAngle, style);
				if (movementMiscPlayer.CanPogo(PogoTypes.Sword)) {
					movementMiscPlayer.Pogo(useStylePlayer.HitStyle == 0? 16 : 12);
				}
				if (Main.netMode != NetmodeID.SinglePlayer) {
					NullandVoidNetwork.SendSwordMessage(player.whoAmI, hitAngle, style);
				}
			}

			
			player.ChangeDir(useStylePlayer.HitDirection);
			player.itemTime = player.itemAnimation;

			float hitProgress = 1 - ((float)player.itemAnimation / player.itemAnimationMax);
			float swingAngle = MathHelper.Lerp(useStylePlayer.HitAngleRange[0], useStylePlayer.HitAngleRange[1], NullandVoidUtils.OutElastic(hitProgress));
			player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, swingAngle);
			player.itemLocation = player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, player.compositeFrontArm.rotation);
			player.itemRotation = swingAngle + 2f * useStylePlayer.HitDirection;
			player.FlipItemLocationAndRotationForGravity();
		}

		public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage) {
			UseStylePlayer useStylePlayer = player.GetModPlayer<UseStylePlayer>();
			if (useStylePlayer.HitStyle == 0 && useStylePlayer.HitResetTimer != 0) {
				damage.Multiplicative = 1.5f;
			}
		}

		public override void ModifyWeaponKnockback(Item item, Player player, ref StatModifier knockback) {
			UseStylePlayer useStylePlayer = player.GetModPlayer<UseStylePlayer>();
			if (useStylePlayer.HitStyle == 0 && useStylePlayer.HitResetTimer != 0) {
				knockback.Multiplicative = 1.5f;
			}
		}

		public override void UseItemFrame(Item item, Player player) {
			player.bodyFrame.Y = player.bodyFrame.Height;
		}

		public override void UseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox) {
			UseStylePlayer useStylePlayer = player.GetModPlayer<UseStylePlayer>();
			
			Vector2 handPostion = player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, player.compositeFrontArm.rotation);
			float x = NullandVoidUtils.OutElastic(1 - ((float)player.itemAnimation / player.itemAnimationMax));
			float size = MathF.Sqrt(hitbox.Width * hitbox.Width + hitbox.Height * hitbox.Height) * (MathF.Sin(x * MathHelper.Pi) * 1.3f + (float)(((useStylePlayer.HitStyle + 2) % 3) + 1) / 4);
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