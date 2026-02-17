using NullandVoid.Common.Players;
using Terraria;
using Terraria.ModLoader;

namespace NullandVoid.Content.Buffs
{
	internal class LungedBuff : ModBuff
	{
		public override void SetStaticDefaults() {
			Main.buffNoSave[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex) {
			player.GetDamage(DamageClass.Melee) *= 3;
			player.GetModPlayer<StylePlayer>().Lunging = true;
			
		}
	}
}