using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using NullandVoid.Common.Globals.Items;
using NullandVoid.Core;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace NullandVoid.Common.Players
{
	public class QuickDrawPlayer : ModPlayer
	{
		public Dictionary<int, int> HotbarDict = new(10);

		public override void ProcessTriggers(TriggersSet triggersSet) {
			if (triggersSet.Hotbar1 && Player.selectedItem != 0) {
				QuickDraw(0);
			}
			if (triggersSet.Hotbar2 && Player.selectedItem != 1) {
				QuickDraw(1);
			}
			if (triggersSet.Hotbar3 && Player.selectedItem != 2) {
				QuickDraw(2);
			}
			if (triggersSet.Hotbar4 && Player.selectedItem != 3) {
				QuickDraw(3);
			}
			if (triggersSet.Hotbar5 && Player.selectedItem != 4) {
				QuickDraw(4);
			}
			if (triggersSet.Hotbar6 && Player.selectedItem != 5) {
				QuickDraw(5);
			}
			if (triggersSet.Hotbar7 && Player.selectedItem != 6) {
				QuickDraw(6);
			}
			if (triggersSet.Hotbar8 && Player.selectedItem != 7) {
				QuickDraw(7);
			}
			if (triggersSet.Hotbar9 && Player.selectedItem != 8) {
				QuickDraw(8);
			}
			if (triggersSet.Hotbar10 && Player.selectedItem != 9) {
				QuickDraw(9);
			}
		}

		public override void PostUpdateMiscEffects() {
			Player.selectItemOnNextUse = false;

			foreach (var key in HotbarDict.ToArray()) {
				if (HotbarDict[key.Key] == 0) {
					HotbarDict.Remove(key.Key);
				}
				else {
					HotbarDict[key.Key]--;
				}
			}
		}

		public void QuickDraw(int slot) {
			HotbarDict[Player.selectedItem] = Player.itemTime;
			Player.GetModPlayer<StylePlayer>().ResetFreshnessNext = true;
			Player.GetModPlayer<StylePlayer>().QuickDrawWindow = Math.Min(10, Player.itemTime) + 10;
			Player.GetModPlayer<UseStylePlayer>().HitStyle = 0;
			Player.selectedItem = slot;
			HotbarDict.TryGetValue(slot, out int cooldown);
			Player.itemAnimation = cooldown;
			Player.itemTime = cooldown;
			if (Player.HeldItem.useStyle == SwordGlobalItem.SwordUseStyle) {
				Player.GetModPlayer<ParryPlayer>().QuickProjectileBoostWindow = 30;
			}

			if (Player.whoAmI == Main.myPlayer && Main.netMode == NetmodeID.MultiplayerClient) {
				NullandVoidNetwork.SendQuickDrawMessage(Player.whoAmI, slot);
			}
		}
	}
}