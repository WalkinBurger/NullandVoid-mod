using Terraria;
using Terraria.ModLoader;

namespace NullandVoid.Content.Buffs
{
	internal class ParryBuff : ModBuff
	{
		public override void SetStaticDefaults() {
			Main.buffNoSave[Type] = true;
		}
	}
}