using System;
using System.Collections.Generic;
using System.Linq;
using NullandVoid.Common.Players;
using NullandVoid.Common.Systems;
using NullandVoid.Core;
using Terraria;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.ModLoader;

namespace NullandVoid.Content.Items
{
	public abstract class MovementAcc : ModItem
	{
		public abstract int MovementType { get; }
		public abstract int AbilityType { get; }
		public abstract int StaminaCost { get; }
		public abstract int StaminaCostAlt { get; }
		public abstract bool HasLayer { get; }

		public float FramesToSeconds(int frames) => MathF.Round((float)frames / 60, 2, MidpointRounding.AwayFromZero);
		public int FloatToPercentage(float percent) => (int)(percent * 100);

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			if (PlayerInput.Triggers.Current.SmartSelect) {
				for (int i = 2; i < tooltips.Count; i++) {
					if (tooltips[i].Text.Length < 9) {
						continue;
					}

					if (tooltips[i].Text[new Range(0, 4)] == "[c/9") {
						tooltips[i].Text = tooltips[i].Text.Replace("<ability>", KeybindSystem.MovementAbilityKeybind.GetAssignedKeys().Count > 0 ? $"<{KeybindSystem.MovementAbilityKeybind.GetAssignedKeys()[0]}>" : Language.GetTextValue("Mods.NullandVoid.Common.KeybindNotSet"));
						tooltips[i].Text = tooltips[i].Text.Replace("<altAbility>", KeybindSystem.MovementAltAbilityKeybind.GetAssignedKeys().Count > 0 ? $"<{KeybindSystem.MovementAltAbilityKeybind.GetAssignedKeys()[0]}>" : Language.GetTextValue("Mods.NullandVoid.Common.KeybindNotSet"));
						continue;
					}
					if (tooltips[i].Text[new Range(0, 4)] == "[c/8") {
						tooltips[i].Hide();
					}
				}
			}
			else {
				for (int i = 2; i < tooltips.Count; i++) {
					if (tooltips[i].Text.Length < 9) {
						continue;
					}

					if (tooltips[i].Text[new Range(0, 4)] == "[c/9") {
						tooltips[i].Hide();
						continue;
					}
					if (tooltips[i].Text[new Range(0, 4)] == "[c/8") {
						tooltips[i].Text = tooltips[i].Text.Replace("<shift>", $"<{PlayerInput.CurrentProfile.InputModes[InputMode.Keyboard].KeyStatus["SmartSelect"].First()}>");
					}
				}
			}
		}
	}

	public abstract class DasherAcc : MovementAcc
	{
		public override int MovementType => MovementClassID.Dasher;
		public override bool HasLayer => false;
		public abstract int DashTime { get; }
		public abstract float DashSpeed { get; }
		public abstract int IFrames { get; }

		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(StaminaCost, StaminaCostAlt, FramesToSeconds(DashTime), DashSpeed, FramesToSeconds(IFrames));

		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.GetModPlayer<MovementClassPlayer>().UpdateDasher(StaminaCost, StaminaCostAlt, DashTime, DashSpeed, IFrames).AbilityType = AbilityType;
		}
	}

	public abstract class ChargerAcc : MovementAcc
	{
		public override int MovementType => MovementClassID.Charger;
		public override bool HasLayer => false;
		public abstract float MaxSpeed { get; }
		public abstract float ChargeAccel { get; }
		public abstract float TurnAccel { get; }
		public abstract float DamageReduction { get; }
		public abstract int ImpactDamage { get; }

		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(StaminaCost, StaminaCostAlt, MaxSpeed, ChargeAccel, TurnAccel, FloatToPercentage(DamageReduction), ImpactDamage);

		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.GetModPlayer<MovementClassPlayer>().UpdateCharger(StaminaCost, StaminaCostAlt, MaxSpeed, ChargeAccel, TurnAccel, DamageReduction, ImpactDamage).AbilityType = AbilityType;
		}
	}

	public abstract class SpiritAcc : MovementAcc
	{
		public override int MovementType => MovementClassID.Spirit;
		public override int StaminaCostAlt => 0;
		public override bool HasLayer => false;
		public abstract float DistanceDecayRate { get; }
		public abstract float DamageAbsorption { get; }
		public abstract float SpiritSpeed { get; }
		public abstract float FlingSpeed { get; }

		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(StaminaCost, DistanceDecayRate, FloatToPercentage(DamageAbsorption), FloatToPercentage(SpiritSpeed), FlingSpeed);

		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.GetModPlayer<MovementClassPlayer>().UpdateSpirit(StaminaCost, StaminaCostAlt, DistanceDecayRate, DamageAbsorption, SpiritSpeed, FlingSpeed).AbilityType = AbilityType;
		}
	}

	public abstract class GrapplerAcc : MovementAcc
	{
		public override int MovementType => MovementClassID.Grappler;
		public override bool HasLayer => false;
		public abstract int Range { get; }
		public abstract float PullSpeed { get; }
		public abstract float ReelSpeed { get; }

		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(StaminaCost, StaminaCostAlt, Range, PullSpeed, ReelSpeed);

		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.GetModPlayer<MovementClassPlayer>().UpdateGrappler(StaminaCost, StaminaCostAlt, Range, PullSpeed, ReelSpeed).AbilityType = AbilityType;
		}
	}
}