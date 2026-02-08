using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace NullandVoid.Common.Players
{
	public class QuickSwapPlayer : ModPlayer
	{
		public override void ProcessTriggers(TriggersSet triggersSet) {
			if (triggersSet.Hotbar1 || triggersSet.Hotbar2 || triggersSet.Hotbar3 || triggersSet.Hotbar4 ||
			    triggersSet.Hotbar5 || triggersSet.Hotbar6 || triggersSet.Hotbar7 || triggersSet.Hotbar8 ||
			    triggersSet.Hotbar9 || triggersSet.Hotbar10 || triggersSet.HotbarScrollCD != 0) {
				Player.reuseDelay = 0;
				// Main.NewText(Player.itemTime);
			}
		}
	}
}