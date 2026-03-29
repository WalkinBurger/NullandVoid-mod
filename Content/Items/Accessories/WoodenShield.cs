using NullandVoid.Core;
using Terraria.ID;

namespace NullandVoid.Content.Items.Accessories
{
	public class WoodenShield : DasherAcc
	{
		public override int MovementType => MovementClassID.Dasher;
		public override int StaminaCost => 20;
		public override bool HasLayer => false;
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
	}
}