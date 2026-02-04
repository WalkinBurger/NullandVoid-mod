using Terraria;
using Terraria.ModLoader;

namespace NullandVoid.Content.Buffs
{
	internal class ParryBuff : ModBuff
	{
		public static int TypeIndex {
			get { return ModContent.BuffType<ParryBuff>(); }
		}

		public override void SetStaticDefaults() {
			Main.buffNoSave[Type] = true;
		}
	}
}