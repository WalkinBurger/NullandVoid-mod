using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using NullandVoid.Common.Systems;
using NullandVoid.Content.Buffs;
using NullandVoid.Core;
using Terraria;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace NullandVoid.Common.Players
{
	public struct MovementClass(string name, int type)
	{
		public readonly LocalizedText Name = Language.GetText("Mods.NullandVoid.MovementClass." + name);
		public readonly int Type = type;

		public static readonly MovementClass Dasher = new("Dasher", 0);
		public static readonly MovementClass Charger = new("Charger", 1);
		public static readonly MovementClass Spirit = new("Spirit", 2);
		public static readonly MovementClass Grappler = new("Grappler", 3);
	}
	
	public class MovementClassPlayer : ModPlayer
	{
		public int StatStaminaMax;
		public int StatStamina;
		public int StaminaRegen;
		public int StaminaRegenCount;
		
		public MovementClass? ClassType;
		public bool UsingAbility;
		public Vector2 PreAbilityVel;
		public int AbilityCost;
		public int AbilityDirection;

		public int DashFrame;
		public int DashTime;
		public bool DashJump;
		public bool CanDashJump;

		
		private void ResetStamina() {
			StatStaminaMax = 40;
			StaminaRegen = 10;
			
			DashTime = 12;
		}

		public override void Load() {
			ResetStamina();
		}

		public override void ResetEffects() {
			ResetStamina();
		}

		public override void UpdateDead() {
			ResetStamina();
		}

		public override void PostUpdateMiscEffects() {
			if (DashFrame != 0) {
				return;
			}

			if (StatStamina < StatStaminaMax) {
				StaminaRegenCount += StaminaRegen;
				if (StaminaRegenCount > 60) {
					StatStamina++;
					StaminaRegenCount -= 60;
					if (StatStamina % 20 == 0) {
						StatStamina = Math.Min(StatStamina, StatStaminaMax);
						SoundEngine.PlaySound(SoundID.Item53 with {Volume = 0.7f * ModContent.GetInstance<NullandVoidClientConfig>().StaminaSoundVolume, Pitch = ((float)StatStamina / StatStaminaMax) - 0.5f});
					}
				}
			}
			else {
				StaminaRegenCount = 0;
			}
		}


		public void ChangeStatStamina(int addAmount) {
			StatStamina = Math.Clamp(StatStamina + addAmount, 0, StatStaminaMax);
		}

		public override void ProcessTriggers(TriggersSet triggersSet) {
			if (!KeybindSystem.MovementAbilityKeybind.JustPressed || ClassType == null || StatStamina < AbilityCost || Player.mount.Active || DashTime > 2) {
				return;
			}

			AbilityDirection = (int)triggersSet.DirectionsRaw.X;
			if (AbilityDirection == 0) {
				AbilityDirection = Player.direction;
			}

			switch (ClassType.Value.Type) { 
				case 0:
					ChangeStatStamina(-AbilityCost);
					DashFrame = DashTime;
					Player.AddBuff(ModContent.BuffType<LungedBuff>(), 45);
					if (Main.netMode != NetmodeID.SinglePlayer) {
						NullandVoidNetwork.SendDashMessage(Player.whoAmI, DashTime, AbilityDirection);
					}
					break;
				case 1:
					break;
				case 2:
					break;
				case 3:
					break;
			}
		}

		public override void PreUpdateMovement() {
			if (!UsingAbility || ClassType == null) {
				return;
			}

			switch (ClassType.Value.Type) {
				case 0:
					DashAbility();
					break;
				case 1:
					break;
				case 2:
					break;
				case 3:
					break;
			}
		}
		
		public override void DrawPlayer(Camera camera) {
			if (DashFrame > 0) {
				Player.armorEffectDrawShadow =  true;
			}
		}

		public void DashAbility() {
			if ((DashTime - DashFrame) <= 5) {
				if (DashFrame == DashTime) {
					// Start of Dash
					SoundEngine.PlaySound(SoundID.DD2_BetsysWrathShot with { Pitch = 0.6f, Volume = 0.3f * ModContent.GetInstance<NullandVoidClientConfig>().StaminaSoundVolume, PitchVariance = 0.2f});
					CanDashJump = DashJump = false;
					
					if (PreAbilityVel == Vector2.Zero) {
						PreAbilityVel = Player.velocity;
						Player.velocity.X = 20 * Player.direction;
					}
					
					Player.SetImmuneTimeForAllTypes(DashTime);
				}

				// Check for dash jump
				if (!DashJump && !CanDashJump && Player.GetModPlayer<MovementMiscPlayer>().Grounded) {
					CanDashJump = true;
				}
				else if (CanDashJump && Player.justJumped && StatStamina >= AbilityCost) {
					// Is long dash jump
					ChangeStatStamina(-AbilityCost);
					DashJump = true;
					CanDashJump = false;
					Player.velocity.X *= 1.5f;
					SoundEngine.PlaySound(SoundID.DD2_BetsysWrathShot with {Pitch = -0.2f, Volume = 0.4f * ModContent.GetInstance<NullandVoidClientConfig>().StaminaSoundVolume});
				}
			}

			if (DashFrame == 1) {
				// End of dash
				if (!DashJump) {
					Player.velocity.X = (Math.Max(Math.Abs(PreAbilityVel.X), 3.5f) + 2) * AbilityDirection;
				}

				PreAbilityVel = Vector2.Zero;
				UsingAbility = false;
			}
			else {
				if (!DashJump) {
					Player.velocity.X = AbilityDirection * Math.Clamp(Math.Abs(Player.velocity.X) - 1f, 12, 20);
					Player.velocity.Y = Math.Clamp(Player.velocity.Y, -5f, 0.5f);
				}
				else {
					Player.velocity.X = AbilityDirection * Math.Clamp(Math.Abs(Player.velocity.X) - 1.6f, 12, 40);
					Player.velocity.Y -= 1f;
				}
			}
			
			Player.noKnockback = true;
			Player.immuneAlpha = 1;
			DashFrame--;
		}
	}
}