using NullandVoid.Core;
using Terraria;
using Terraria.ID;

namespace NullandVoid.Content.Items.Accessories
{
	public class WoodenGauntlets : ChargerAcc
	{
		public override int AbilityType => (int)ChargerType.Normal;
		public override int StaminaCost => 20;
		public override int StaminaCostAlt => 20;
		public override bool HasLayer => true;
		public override float MaxSpeed => 10;
		public override float ChargeAccel => 2;
		public override float TurnAccel => 1;
		public override float DamageReduction => 0.5f;
		public override int ImpactDamage => 30;

		public override void SetDefaults() {
			Item.width = 28;
			Item.height = 22;
			Item.accessory = true;
			Item.value = 30;
			Item.rare = ItemRarityID.White;
			Item.defense = 2;
		}

		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddRecipeGroup(RecipeGroupID.Wood, 20);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
	}
}