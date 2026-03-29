using System;
using System.Collections.Generic;
using NullandVoid.Common.Players;
using NullandVoid.Core;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace NullandVoid.Content.Items.Accessories
{
	public class WoodenGauntlets : Charger
	{
		public override int MovementType => MovementClassID.Charger;
		public override int StaminaCost => 20;
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
	}
}