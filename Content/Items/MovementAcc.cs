using System;
using System.Collections.Generic;
using System.Linq;
using NullandVoid.Common.Players;
using Terraria;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.ModLoader;

namespace NullandVoid.Content.Items
{
	public abstract class MovementAcc : ModItem
	{
		public abstract int MovementType { get; }
		public abstract int StaminaCost { get; }
		public abstract bool HasLayer { get; }

		internal string ShiftKey = PlayerInput.CurrentProfile.InputModes[InputMode.Keyboard].KeyStatus["SmartSelect"].First();

		public float FramesToSeconds(int frames) => MathF.Round((float)frames / 60, 2, MidpointRounding.AwayFromZero);
		public int FloatToPercentage(float percent) => (int)(percent * 100);

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			if (PlayerInput.Triggers.Current.SmartSelect) {
				tooltips.Find(line => line.Text[new Range(0, 4)] == "[c/8").Hide();
			}
			else {
				foreach (TooltipLine line in tooltips) {
					if (line.Text[new Range(0, 4)] == "[c/9") {
						line.Hide();
					}
				}
			}
		}
	}

	public abstract class DasherAcc : MovementAcc
	{
		public abstract int DashTime { get; }
		public abstract float DashSpeed { get; }
		public abstract int IFrames { get; }

		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(ShiftKey, StaminaCost, FramesToSeconds(DashTime), DashSpeed, FramesToSeconds(IFrames));
		
		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.GetModPlayer<MovementClassPlayer>().UpdateDasher(StaminaCost, DashTime, DashSpeed, IFrames);
		}
	}

	public abstract class Charger : MovementAcc
	{
		public abstract float MaxSpeed { get; }
		public abstract float ChargeAccel { get; }
		public abstract float TurnAccel { get; }
		public abstract float DamageReduction { get; }
		public abstract int ImpactDamage { get; }
		
		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(ShiftKey, StaminaCost, MaxSpeed, ChargeAccel, TurnAccel, FloatToPercentage(DamageReduction), ImpactDamage);
		
		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.GetModPlayer<MovementClassPlayer>().UpdateCharger(StaminaCost, MaxSpeed, ChargeAccel, TurnAccel, DamageReduction, ImpactDamage);
		}
	}

	public abstract class Spirit : MovementAcc
	{
		public abstract float DistanceDecayRate { get; }
		public abstract int DamageCost { get; }
		public abstract float SpiritSpeed { get; }
		public abstract float FlingSpeed { get; }
		
		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(ShiftKey, StaminaCost, DistanceDecayRate, DamageCost, SpiritSpeed, FlingSpeed);
		
		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.GetModPlayer<MovementClassPlayer>().UpdateSpirit(StaminaCost, DistanceDecayRate, DamageCost, SpiritSpeed, FlingSpeed);
		}
	}

	public abstract class Grappler : MovementAcc
	{
		public abstract int Range { get; }
		public abstract float PullSpeed { get; }
		public abstract float ReelSpeed { get; }
		
		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(ShiftKey, StaminaCost, Range, PullSpeed, ReelSpeed);
		
		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.GetModPlayer<MovementClassPlayer>().UpdateGrappler(StaminaCost, Range, PullSpeed, ReelSpeed);
		}
	}
}