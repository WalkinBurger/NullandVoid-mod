using NullandVoid.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NullandVoid.Content.Items.Accessories
{
	[AutoloadEquip(EquipType.Waist)]
	public class SlimeAtomizer : SpiritAcc
	{
		public override int AbilityType => (int)SpiritType.Slime;
		public override int StaminaCost => 10;
		public override float DistanceDecayRate => 5;
		public override float DamageAbsorption => 0.25f;
		public override float SpiritSpeed => 0.5f;
		public override float FlingSpeed => 11;

		public override void SetDefaults() {
			Item.width = 26;
			Item.height = 38;
			Item.accessory = true;
			Item.value = 30;
			Item.rare = ItemRarityID.White;
		}
		
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.Gel, 10);
			recipe.AddRecipeGroup(RecipeGroupID.Wood, 10);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
	}
}