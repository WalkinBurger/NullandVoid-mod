using NullandVoid.Common.Players;
using NullandVoid.Core;
using Terraria;
using Terraria.ModLoader;

namespace NullandVoid.Common.Globals.Items
{
	public class HealingPotions : GlobalItem
	{
		public override bool AppliesToEntity(Item entity, bool lateInstantiation) {
			return entity.potion;
		}

		public override bool? UseItem(Item item, Player player) {
			player.GetModPlayer<StylePlayer>().AddStyleBonus(StyleBonus.LameHealing);
			return true;
		}
	}
}