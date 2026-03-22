using Microsoft.Xna.Framework;
using NullandVoid.Content.Items;
using NullandVoid.Content.Items.Accessories;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace NullandVoid.Common.Players
{
	public class MovementClassAccessorySlot : ModAccessorySlot
	{
		public LocalizedText MovementClassAccessoryText;
		
		public override void SetupContent() {
			MovementClassAccessoryText = Language.GetText("Mods.NullandVoid.UI.MovementClassAccessory");
		}
		
		public override string FunctionalBackgroundTexture => "Terraria/Images/Inventory_Back19";
		public override string DyeBackgroundTexture => "Terraria/Images/Inventory_Back5";
		public override string FunctionalTexture => "Terraria/Images/Item_" + ItemID.AmethystHook;
		public override bool DrawVanitySlot => false;

		public override bool CanAcceptItem(Item checkItem, AccessorySlotType context) {
			return checkItem.ModItem is IMovementAcc movementAcc;
		}

		public override void OnMouseHover(AccessorySlotType context) {
			if (context == AccessorySlotType.FunctionalSlot && FunctionalItem != null) {
				Main.hoverItemName = MovementClassAccessoryText.Value;
			}
		}
	}
}