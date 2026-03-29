using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
		public Item LastItem;
		
		public override void SetupContent() {
			MovementClassAccessoryText = Language.GetText("Mods.NullandVoid.UI.MovementClassAccessory");
		}
		
		public override string FunctionalBackgroundTexture => "Terraria/Images/Inventory_Back19";
		public override string DyeBackgroundTexture => "Terraria/Images/Inventory_Back5";
		public override string FunctionalTexture => "Terraria/Images/Item_" + ItemID.AmethystHook;
		public override bool DrawVanitySlot => false;

		public override bool CanAcceptItem(Item checkItem, AccessorySlotType context) {
			return checkItem.ModItem is MovementAcc;
		}

		public override bool ModifyDefaultSwapSlot(Item item, int accSlotToSwapTo) {
			return item.ModItem is MovementAcc;
		}
		
		public override void OnMouseHover(AccessorySlotType context) {
			if (context == AccessorySlotType.FunctionalSlot && FunctionalItem.IsAir) {
				Main.hoverItemName = MovementClassAccessoryText.Value;
			}
		}

		public override void ApplyEquipEffects() {
			if (LastItem != FunctionalItem) {
				MovementClassPlayer movementClassPlayer = Player.GetModPlayer<MovementClassPlayer>();
				movementClassPlayer.ChangedAcc = true;
				if (FunctionalItem.ModItem is MovementAcc { HasLayer: true }) {
					movementClassPlayer.AccTexture = ModContent.Request<Texture2D>($"NullandVoid/Content/Items/Accessories/{FunctionalItem.ModItem.Name}_Acc");
				}
			}
			LastItem = FunctionalItem;
			base.ApplyEquipEffects();
		}
	}
}