using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NullandVoid.Common.Players
{
	public class BloodPlayer : ModPlayer
	{
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (target.type == NPCID.TargetDummy) {
				return;
			}
			
			Vector2 closestPoint = Player.Center;
			if (Player.Center.X < target.Hitbox.Left) {
				closestPoint.X = target.Hitbox.Left;
			} 
			else if (Player.Center.X > target.Hitbox.Right) {
				closestPoint.X = target.Hitbox.Right;
			}

			if (Player.Center.Y < target.Hitbox.Top) {
				closestPoint.Y = target.Hitbox.Top;
			} 
			else if (Player.Center.Y > target.Hitbox.Bottom) {
				closestPoint.Y = target.Hitbox.Bottom;
			}

			if (closestPoint.DistanceSQ(Player.Center) < (hit.DamageType == DamageClass.Melee? 5000 : 20000)) {
				Player.statLife = Math.Min(Player.statLifeMax2, Player.statLife + (int)(damageDone * (1 + ((float)Player.GetModPlayer<StylePlayer>().PlayerStyleRank.Rank / 4))));
				NetMessage.SendData(MessageID.PlayerLifeMana, number: Player.whoAmI);
			}
		}
	}
}