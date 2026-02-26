using System;
using Terraria;

namespace NullandVoid.Utils
{
	public static partial class NullandVoidUtils
	{
		public static int EstimateDamage(Projectile projectile) {
			Player player = Main.LocalPlayer;
			return (int)(Math.Max(1, (projectile.damage * (Main.GameMode + 1) * 2 - player.statDefense * player.DefenseEffectiveness.Value) * (1 - player.endurance)) * (1 - 0.15f * player.luck));
		}
		
		public static int EstimateDamage(NPC npc) {
			Player player = Main.LocalPlayer;
			return (int)(Math.Max(1, (npc.damage - player.statDefense * player.DefenseEffectiveness.Value) * (1 - player.endurance)) * (1 - 0.15f * player.luck));
		}
	}
}