using System;
using Microsoft.Xna.Framework;
using NullandVoid.Common.Players;
using NullandVoid.Core;
using NullandVoid.Utils;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NullandVoid.Common.Globals.Items
{
	public class ShootGlobalItem : GlobalItem
	{
		public override bool AppliesToEntity(Item entity, bool lateInstantiation) {
			return entity.useStyle == ItemUseStyleID.Shoot && entity.DamageType != DamageClass.Melee;
		}
		
		public override void UseStyle(Item item, Player player, Rectangle heldItemFrame) {
			UseStylePlayer useStylePlayer = player.GetModPlayer<UseStylePlayer>();
			
			float shootAngle;
			if (player.whoAmI == Main.myPlayer) {
				shootAngle = Math.Abs(NullandVoidUtils.MouseAngle(Main.MouseScreen, Main.ScreenSize, true)) * player.direction * -1;
				useStylePlayer.ShootAngle = shootAngle;
				if (Main.netMode != NetmodeID.SinglePlayer && Math.Abs(shootAngle - useStylePlayer.OldShootAngle) > 0.4f) {
					useStylePlayer.OldShootAngle = shootAngle;
					NullandVoidNetwork.SendShootMessage(player.whoAmI, shootAngle);
				}
			}
			else {
				shootAngle = useStylePlayer.ShootAngle;
			}
			
			int t = item.useTime - player.itemTime;
			if (t is > 0 and <= 6 && (player.reuseDelay == 0 || item.reuseDelay == 0)) {
				player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, (shootAngle + player.compositeFrontArm.rotation) / 2);
				if (item.useAmmo == AmmoID.Bullet || item.width > item.height) {
					player.itemRotation = player.compositeFrontArm.rotation + (MathHelper.PiOver2 - MathF.Pow(MathF.Sin(1 / MathF.Pow(0.3f * t + 0.57f, 2)), 2) * (Math.Max(item.knockBack , 1) / 16)) * player.direction;
				}
			}
			else {
				player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, (shootAngle + player.compositeFrontArm.rotation) / 2);
				player.itemRotation = player.compositeFrontArm.rotation + MathHelper.PiOver2 * player.direction;
			}
			player.FlipItemLocationAndRotationForGravity();
		}
	}
}