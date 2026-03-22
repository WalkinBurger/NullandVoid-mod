using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace NullandVoid.Common.Systems
{
	public class NoVanillaMovement : ModSystem
	{
		private static LocalizedText noGrappleText;
		
		public override void Load() {
			noGrappleText = Language.GetText("Mods.NullandVoid.UI.NoVanillaGrapple");
			On_ItemSlot.MouseHover_ItemArray_int_int += DetourNoGrappleText;
			On_Player.QuickGrapple += DetourNoGrapple;
			On_Player.DashMovement += DetourNoDash;
		}

		public override void Unload() {
			On_ItemSlot.MouseHover_ItemArray_int_int -= DetourNoGrappleText;
			On_Player.QuickGrapple -= DetourNoGrapple;
			On_Player.DashMovement -= DetourNoDash;
		}


		private static void DetourNoGrappleText(On_ItemSlot.orig_MouseHover_ItemArray_int_int orig, Item[] inv, int context, int slot) {
			orig(inv, context, slot);
			if (context == 16)
			{
				Main.hoverItemName = Lang.inter[90].Value + " (" + noGrappleText.Value + ")";
			}
		}
		
		private static void DetourNoGrapple(On_Player.orig_QuickGrapple orig, Player self) {
			
		}
		
		private static void DetourNoDash(On_Player.orig_DashMovement orig, Player self) {
			
		}
	}
}