using Microsoft.Xna.Framework.Graphics;
using NullandVoid.Content.Items;
using NullandVoid.Core;
using Terraria;
using Terraria.Graphics.Effects;
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
		public override bool DrawDyeSlot => false;

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
			if (LastItem != FunctionalItem ) {
				MovementClassPlayer movementClassPlayer = Player.GetModPlayer<MovementClassPlayer>();
				movementClassPlayer.ChangedAcc = true;
				if (FunctionalItem.ModItem is MovementAcc { HasLayer: true }) {
					movementClassPlayer.AccTexture = ModContent.Request<Texture2D>($"NullandVoid/Content/Items/Accessories/{FunctionalItem.ModItem.Name}_Acc");
				}
				movementClassPlayer.CancelAbility(false, 60);
				movementClassPlayer.CancelAbility(true, 60);
				movementClassPlayer.AbilityFrame = 1;
				movementClassPlayer.SpiritPosition = Player.Center;
				if (Player.whoAmI == Main.myPlayer && LastItem is { ModItem: MovementAcc { MovementType: MovementClassID.Spirit } }) {
					Filters.Scene["NullandVoid:SpiritVignette"].GetShader().UseProgress(0);
					Filters.Scene["NullandVoid:SpiritVignette"].Deactivate();
				}
			}

			LastItem = FunctionalItem;
			base.ApplyEquipEffects();
		}
	}
}