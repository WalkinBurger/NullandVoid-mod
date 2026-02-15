using System;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace NullandVoid.Common.Players
{
	public class QuickSwapPlayer : ModPlayer
	{
		public int[] HotbarUseDelay = new int[10];
		
		public override void ProcessTriggers(TriggersSet triggersSet) {

			int selecting = -1;
			if (triggersSet.Hotbar1) {
				selecting = 0;
			}
			else if (triggersSet.Hotbar2) {
				selecting = 1;
			}
			else if (triggersSet.Hotbar3) {
				selecting = 2;
			}
			else if (triggersSet.Hotbar4) {
				selecting = 3;
			}
			else if (triggersSet.Hotbar5) {
				selecting = 4;
			}
			else if (triggersSet.Hotbar6) {
				selecting = 5;
			}
			else if (triggersSet.Hotbar7) {
				selecting = 6;
			}
			else if (triggersSet.Hotbar8) {
				selecting = 7;
			}
			else if (triggersSet.Hotbar9) {
				selecting = 8;
			}
			else if (triggersSet.Hotbar10) {
				selecting = 9;
			}

			if (selecting == -1) {
				return;
			}

			
			if (selecting != Player.selectedItem) {
				HotbarUseDelay[Player.selectedItem] = Player.itemTime;
				Player.selectedItem = selecting;
				Player.controlUseItem = false;
				int newDelay = HotbarUseDelay[selecting];
				Player.itemTime = newDelay;
				Player.itemAnimation = newDelay;
			}
		}

		public override void PostUpdateMiscEffects() {
			for (int i = 0; i < 10; i++) {
				if (HotbarUseDelay[i] != 0) {
					HotbarUseDelay[i]--;
					if (i == Player.selectedItem) {
						Player.controlUseItem = false;
					}
					Main.NewText(string.Join(", ", HotbarUseDelay));
				}
			}
		}
	}
}