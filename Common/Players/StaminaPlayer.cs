using System;
using Microsoft.Xna.Framework;
using NullandVoid.Common.Systems;
using NullandVoid.Content.Buffs;
using Terraria;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace NullandVoid.Common.Players
{
	public class StaminaPlayer : ModPlayer
	{
		// Stamina stats fields
		private int staminaTimer;

		public int StaminaMax;
		public int StaminaResource;
		public int StaminaUsage;
		public float StaminaRegenRate;
		public int DashFrame;
		public int DashTime;
		private Vector2 preDashVelocity;
		private int dashDirection;
		private bool longDash;
		private bool canLongDash;

		
		// Reset stamina stats
		private void ResetStamina() {
			StaminaMax = 40;
			StaminaRegenRate = 1.5f;
			StaminaUsage = 20;
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

		// Updatruetes stamina
		public override void PostUpdateMiscEffects() {
			// Stamina resource regen
			staminaTimer++;
			if (staminaTimer >= 10 / StaminaRegenRate && StaminaResource < StaminaMax) {
				StaminaResource++;
				if (StaminaResource % 20 == 0) {
					SoundEngine.PlaySound(SoundID.Item53 with {Volume = 0.5f, Pitch = ((float)StaminaResource / StaminaMax) - 0.5f});
				}

				staminaTimer = 0;
			}

			StaminaResource = Utils.Clamp(StaminaResource, 0, StaminaMax);

			if (DashFrame > 0) {
				StaminaDash();
			}

			// Disable vanilla dash
			Player.dashType = 0;
		}

		public void AddStaminaResource(int addAmount) {
			StaminaResource = Math.Clamp(StaminaResource + addAmount, 0, StaminaMax);
		}

		public override void ProcessTriggers(TriggersSet triggersSet) {
			if (KeybindSystem.DashKeybind.JustPressed && StaminaResource >= StaminaUsage) {
				if (!Player.mount.Active && Player.grapCount == 0) {
					AddStaminaResource(-StaminaUsage);
					DashFrame = DashTime;
					dashDirection = (int)triggersSet.DirectionsRaw.X;
					if (dashDirection == 0) {
						dashDirection = Player.direction;
					}
					
				}
			}
		}

		public void StaminaDash() {
			if ((DashTime - DashFrame) <= 5) {
				if (DashFrame == DashTime) {
					// Start of Dash
					canLongDash = longDash = false;
					
					if (preDashVelocity == Vector2.Zero) {
						preDashVelocity = Player.velocity;
						Player.velocity.X = 20 * Player.direction;
					}
					
					Player.SetImmuneTimeForAllTypes(DashTime);
				}

				// Check for long dash
				if (!longDash && !canLongDash && (((Player.velocity.Y == 0f || Player.sliding) && Player.releaseJump) || (Player.autoJump && Player.justJumped))) {
					canLongDash = true;
				}
				else if (canLongDash && Player.velocity.Y != 0f && StaminaResource >= StaminaUsage) {
					// Is long dash
					SoundEngine.PlaySound(SoundID.DD2_BetsysWrathShot with {Pitch = -0.2f, Volume = 0.3f});
					AddStaminaResource(-StaminaUsage);
					longDash = true;
					canLongDash = false;
					Player.velocity.X *= 1.5f;
				}
			}

			if (DashFrame == 1) {
				// End of dash
				if (!longDash) {
					Player.velocity.X = (Math.Max(Math.Abs(preDashVelocity.X), 3.5f) + 2) * dashDirection;
					Player.AddBuff(ModContent.BuffType<LungedBuff>(), 60);
				}

				preDashVelocity = Vector2.Zero;
			}
			else {
				if (!longDash) {
					Player.velocity.X = dashDirection * Math.Clamp(Math.Abs(Player.velocity.X) - 1f, 12, 20);
					Player.velocity.Y = Math.Clamp(Player.velocity.Y, -5f, 0.5f);
				}
				else {
					Player.velocity.X = dashDirection * Math.Clamp(Math.Abs(Player.velocity.X) - 1.6f, 12, 40);
					Player.velocity.Y -= 1f;
				}
			}
			
			Player.noKnockback = true;
			Player.immuneAlpha = 1;
			NetMessage.SendData(MessageID.PlayerControls, number: Player.whoAmI);
			DashFrame--;
		}
		
		public override void DrawPlayer(Camera camera) {
			if (DashFrame > 0) {
				Player.armorEffectDrawShadow =  true;
			}
		}
	}
}