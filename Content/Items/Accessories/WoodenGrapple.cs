using NullandVoid.Core;
using Terraria;
using Terraria.ID;

namespace NullandVoid.Content.Items.Accessories
{
	public class WoodenGrapple : GrapplerAcc
	{
		public override int AbilityType => (int)GrapplerType.NormalWooden;
		public override int StaminaCost => 20;
		public override int StaminaCostAlt => 40;
		public override int Range => 16;
		public override float PullSpeed => 14;
		public override float ReelSpeed => 10;
		
		public override void SetDefaults() {
			Item.width = 24;
			Item.height = 32;
			Item.accessory = true;
			Item.value = 30;
			Item.rare = ItemRarityID.White;
		}
		
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddRecipeGroup(RecipeGroupID.Wood, 25);
			recipe.AddIngredient(ItemID.Gel, 5);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
	}
}