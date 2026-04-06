using NullandVoid.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NullandVoid.Content.Items.Accessories
{
	[AutoloadEquip(EquipType.Shield)]
	public class WoodenShield : DasherAcc
	{
		public override int AbilityType => (int)DasherType.Normal;
		public override int StaminaCost => 20;
		public override int StaminaCostAlt => 40;
		public override int DashTime => 9;
		public override float DashSpeed => 8;
		public override int IFrames => 6;

		public override void SetDefaults() {
			Item.width = 22;
			Item.height = 28;
			Item.accessory = true;
			Item.value = 30;
			Item.rare = ItemRarityID.White;
			Item.defense = 2;
		}
		
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddRecipeGroup(RecipeGroupID.Wood, 30);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
	}
}