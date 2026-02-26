using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace NullandVoid.Common.Players
{
	public class QuickDrawPlayer : ModPlayer
	{
		public Dictionary<int, int> HotbarDict = new(10);
		
		public override void PostUpdateMiscEffects() {
			// Main.NewText((Player.itemTime, string.Join(", ", HotbarDict.ToArray())));
			Player.selectItemOnNextUse = false;
			foreach (Keys key in PlayerInput.GetPressedKeys().Where(key => (int)key >= 48 && (int)key <= 57)) {
				int slot = (int)key - 49;
				if (slot == -1) {
					slot = 10;
				}

				if (slot == Player.selectedItem) {
					continue;
				}

				HotbarDict[Player.selectedItem] = Player.itemTime;
				Player.GetModPlayer<StylePlayer>().ResetFreshnessNext = true;
				Player.GetModPlayer<StylePlayer>().QuickDrawWindow = Math.Min(10, Player.itemTime) + 10;
				Player.GetModPlayer<SwordPlayer>().HitStyle = 0;
				Player.selectedItem = slot;
				HotbarDict.TryGetValue(slot, out int cooldown);
				Player.itemAnimation = cooldown;
				Player.itemTime = cooldown;
			};

			foreach (var key in HotbarDict.ToArray()) {
				if (HotbarDict[key.Key] == 0) {
					HotbarDict.Remove(key.Key);
				}
				else {
					HotbarDict[key.Key]--;
				}
			}
		}
	}
}