using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NullandVoid.Common.Systems;
using NullandVoid.Content.Buffs;
using NullandVoid.Core;
using ReLogic.Content;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace NullandVoid.Common.Players
{
	public class MovementClassPlayer : ModPlayer
	{
		public int StatStaminaMax;
		public int StatStamina;
		public int StaminaRegen;
		public int StaminaRegenCount;
		public int StaminaCost;
		
		public int ClassType;
		public bool UsingAbility;
		public bool UsingAltAbility;
		public Vector2 PreAbilityVel;
		public int AbilityDirection;
		public bool StopMovement;
		public SlotId SoundSlot;
		public int AbilityFrame;
		public int MovementFrame;
		public int MovementFrameCount;
		public int Cooldown;
		public bool DrawAcc;
		public bool ChangedAcc;
		public Asset<Texture2D> AccTexture;

		
		public StatMovement StatMovement = new();

		public Vector2 ChargeAccel;
		public float ChargeSpeed;

		
		private void ResetMovement() {
			StatStaminaMax = 40;
			StaminaRegen = 8;
			
			ClassType = MovementClassID.None;
			ChangedAcc = false;
			DrawAcc = false;
			StatMovement.Reset();
		}

		public override void Load() {
			ResetMovement();
		}

		public override void ResetEffects() {
			ResetMovement();
		}

		public override void UpdateDead() {
			ResetMovement();
		}

		public override void PostUpdateMiscEffects() {
			StaminaCost = StatMovement.StaminaCost.Get();
			if (Cooldown > 0) {
				Cooldown--;
			}
			
			if (UsingAbility || UsingAltAbility) {
				return;
			}

			if (StatStamina < StatStaminaMax) {
				StaminaRegenCount += StaminaRegen;
				if (StaminaRegenCount >= 60) {
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
			if ((!KeybindSystem.MovementAbilityKeybind.JustPressed && !KeybindSystem.MovementAltAbilityKeybind.JustPressed) || ClassType == MovementClassID.None || (ClassType == MovementClassID.Dasher && StatStamina < StaminaCost) || Player.mount.Active || Cooldown > 0) {
				return;
			}
			
			if (KeybindSystem.MovementAbilityKeybind.JustPressed) {
				UsingAbility = true;
				if (Main.netMode != NetmodeID.SinglePlayer) {
					NullandVoidNetwork.SendMovementAbilityMessage(Player.whoAmI);
				}
			}
			else {
				UsingAltAbility = true;
				if (Main.netMode != NetmodeID.SinglePlayer) {
					NullandVoidNetwork.SendMovementAltAbilityMessage(Player.whoAmI);
				}
			}
			
			UseAbility();
		}

		public void UseAbility() {
			PreAbilityVel = Player.velocity;
			AbilityDirection = (int)PlayerInput.Triggers.Current.DirectionsRaw.X;
			if (AbilityDirection == 0) {
				AbilityDirection = Player.direction;
			}
			switch (ClassType) { 
				case MovementClassID.Dasher:
					AbilityFrame = StatMovement.DashTime.Get();
					Player.AddBuff(ModContent.BuffType<LungedBuff>(), 45);
					Cooldown = 2;
					if (UsingAbility) {
						ChangeStatStamina(-StaminaCost);
					}
					else {
						ChangeStatStamina(2 * -StaminaCost);
					}
					break;
				case MovementClassID.Charger:
					PreAbilityVel /= 2;
					if (StopMovement) {
						StopMovement = false;
						Cooldown = 30;
					}
					else {
						StopMovement = true;
						Cooldown = 2;
					}
					StaminaRegenCount = 0;
					UsingAltAbility = false;
					break;
				case MovementClassID.Spirit:
					break;
				case MovementClassID.Grappler:
					break;
			}
		}

		public override void PreUpdateMovement() {
			if ((!UsingAbility && !UsingAltAbility) || ClassType == MovementClassID.None) {
				return;
			}

			switch (ClassType) {
				case MovementClassID.Dasher:
					DashAbility();
					break;
				case MovementClassID.Charger:
					ChargeAbility();
					break;
				case MovementClassID.Spirit:
					break;
				case MovementClassID.Grappler:
					break;
			}
		}
		
		public override void DrawPlayer(Camera camera) {
			if (UsingAbility) {
				switch (ClassType) {
					case MovementClassID.Charger:
						Player.armorEffectDrawShadow =  true;
						Player.legFrame.Y = 392 + 56 * (MovementFrame % 12);
						Player.bodyFrame. Y = 392 + 56 * (MovementFrame % 12);
						Player.direction = Player.velocity.X switch {
							> 0 => 1,
							< 0 => -1,
							_ => Player.direction
						};

						Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Quarter, -Player.direction * MathHelper.PiOver2);
						Player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Quarter, -Player.direction * MathHelper.PiOver2);
						DrawAcc = true;
						break;
					case MovementClassID.Dasher:
						Player.armorEffectDrawShadow =  true;
						break;
				}
			}
			else if (UsingAltAbility) {
				switch (ClassType) {
					case MovementClassID.Dasher:
						Player.armorEffectDrawShadow = true;
						break;
				}
			}
		}
		

		public void DashAbility() {
			int dashTime = StatMovement.DashTime.Get();
			float dashSpeed = StatMovement.DashSpeed.Get();
			if (dashTime == AbilityFrame) {
				// Start of Dash
				Player.SetImmuneTimeForAllTypes(StatMovement.IFrame.Get());
				Player.velocity.X = (8 + dashSpeed) * Player.direction;
				
				if (UsingAbility) {
					SoundEngine.PlaySound(SoundID.DD2_BetsysWrathShot with { Pitch = 0.6f, Volume = 0.3f * ModContent.GetInstance<NullandVoidClientConfig>().StaminaSoundVolume, PitchVariance = 0.2f });
				}
				else {
					Player.velocity.X *= 1.5f;
					Player.velocity.Y = -3;
					SoundEngine.PlaySound(SoundID.DD2_BetsysWrathShot with { Pitch = -0.2f, Volume = 0.4f * ModContent.GetInstance<NullandVoidClientConfig>().StaminaSoundVolume });
				}
			}
			
			if (AbilityFrame == 1) {
				// End of dash
				if (UsingAbility) {
					Player.velocity.X = (Math.Max(Math.Abs(PreAbilityVel.X), 3.5f) + 2) * AbilityDirection;
					UsingAbility = false;
				}
				else {
					UsingAltAbility = false;
				}
				PreAbilityVel = Vector2.Zero;
				Player.fallStart = Player.position.ToTileCoordinates().Y;
			}
			else {
				if (UsingAbility) {
					Player.velocity.X = AbilityDirection * Math.Clamp(Math.Abs(Player.velocity.X) - 1f, dashSpeed, 8 + dashSpeed);
					Player.velocity.Y = Math.Clamp(Player.velocity.Y, -5f, 0.5f);
				}
				else {
					Player.velocity.X = AbilityDirection * Math.Clamp(Math.Abs(Player.velocity.X) - 1.6f, dashSpeed, 28 + dashSpeed);
					Player.velocity.Y -= 0.5f;
				}
			}

			Player.noKnockback = true;
			Player.immuneAlpha = 1;
			AbilityFrame--;
		}

		public void ChargeAbility() {
			if (StatStamina == 0 || !StopMovement) {
				StopMovement = false;
				UsingAbility = false;
				ChargeAccel = Vector2.Zero;
				Player.fallStart = Player.position.ToTileCoordinates().Y;
				AbilityFrame = 0;
				MovementFrame = 0;
				MovementFrameCount = 0;
				Cooldown = 30;
				return;
			}
			
			float maxSpeed = StatMovement.MaxSpeed.Get();
			ChargeSpeed = (Math.Abs(PreAbilityVel.X) * Math.Abs(PreAbilityVel.X) + Math.Abs(PreAbilityVel.Y) * Math.Abs(PreAbilityVel.Y)) / (maxSpeed * maxSpeed);
			float accelFactor = (StatMovement.ChargeAccel.Get() * (1 - ChargeSpeed) + StatMovement.TurnAccel.Get() * ChargeSpeed) / 128;
			
			if ((Player.direction == -1 && !Player.controlRight) || Player.controlLeft) {
				ChargeAccel.X = (-maxSpeed - PreAbilityVel.X) * accelFactor;
				if (!Player.controlUp && !Player.controlDown) {
					PreAbilityVel.Y *= 0.98f;
				}
			}
			else if ((Player.direction == 1 && !Player.controlLeft) || Player.controlRight) {
				ChargeAccel.X = (maxSpeed - PreAbilityVel.X) * accelFactor;
				if (!Player.controlUp && !Player.controlDown) {
					PreAbilityVel.Y *= 0.98f;
				}
			}

			if (Player.controlUp) {
				ChargeAccel.Y = (-maxSpeed - PreAbilityVel.Y) * accelFactor;
				if (!Player.controlLeft && !Player.controlRight) {
					PreAbilityVel.X *= 0.98f;
				}
			}
			else if (Player.controlDown) {
				ChargeAccel.Y = (maxSpeed - PreAbilityVel.Y) * accelFactor;
				if (!Player.controlLeft && !Player.controlRight) {
					PreAbilityVel.X  *= 0.98f;
				}
			}

			Player.controlUseItem = false;
			StaminaRegenCount += StaminaCost;
			if (StaminaRegenCount >= 60) {
				StatStamina--;
				StaminaRegenCount -= 60;
			}

			if (!SoundEngine.TryGetActiveSound(SoundSlot, out ActiveSound _)) {
				SoundSlot = SoundEngine.PlaySound(SoundID.Run with { Volume = 0.7f, Pitch = ChargeSpeed - 0.25f, PitchVariance = 0.1f }, Player.Center);
			}
			
			MovementFrameCount += (int)((ChargeSpeed + 0.5f) * 10);
			if (MovementFrameCount > 12) {
				MovementFrameCount -= 12;
				MovementFrame++;
			}

			Dust dust = Dust.NewDustDirect(Player.Bottom + 2 * PreAbilityVel, 1, 1, DustID.GemDiamond, -PreAbilityVel.X, -PreAbilityVel.Y, Scale: ChargeSpeed * 2, newColor: new Color(255, 255, 0));
			dust.noGravity = true;
			
			PreAbilityVel += ChargeAccel;
			Player.velocity = PreAbilityVel;

			if (Player.whoAmI != Main.myPlayer) {
				return;
			}

			foreach (NPC npc in Main.ActiveNPCs) {
				if (!(npc.DistanceSQ(Player.Center) < 100) || npc.immune[Player.whoAmI] != 0) {
					continue;
				}

				int damage = (int)(StatMovement.ImpactDamage.Get() * ChargeSpeed) + 3;
				npc.SimpleStrikeNPC(damage, Player.direction, false, 10 * (ChargeSpeed + 3));
				npc.immune[Player.whoAmI] = 15;
				Player.GetModPlayer<BloodPlayer>().BloodHeal(damage);
			}
		}


		public override void ModifyHurt(ref Player.HurtModifiers modifiers) {
			if (UsingAbility && ClassType == MovementClassID.Charger) {
				if (Main.netMode != NetmodeID.SinglePlayer) {
					NullandVoidNetwork.SendSoundMessage(Player.whoAmI, SoundsID.ChargeHurt, (int)(ChargeSpeed * 10));
				}
				else {
					SoundEngine.PlaySound(Sounds.GetSound(SoundsID.ChargeHurt, (int)(ChargeSpeed * 10)));
				}
				modifiers.FinalDamage *= (1 - StatMovement.DamageReduction.Get());
			}
		}


		public void UpdateDasher(int staminaCost, int dashTime, float dashSpeed, int iFrame) {
			ClassType = MovementClassID.Dasher;
			StatMovement.StaminaCost.Set(staminaCost);
			StatMovement.DashTime.Set(dashTime);
			StatMovement.DashSpeed.Set(dashSpeed);
			StatMovement.IFrame.Set(iFrame);
		}
		
		public void UpdateCharger(int staminaCost, float maxSpeed, float chargeAccel, float turnAccel, float damageReduction, int impactDamage) {
			ClassType = MovementClassID.Charger;
			StatMovement.StaminaCost.Set(staminaCost);
			StatMovement.MaxSpeed.Set(maxSpeed);
			StatMovement.ChargeAccel.Set(chargeAccel);
			StatMovement.TurnAccel.Set(turnAccel);
			StatMovement.DamageReduction.Set(damageReduction);
			StatMovement.ImpactDamage.Set(impactDamage);
		}

		public void UpdateSpirit(int staminaCost, float distanceDecayRate, int damageCost, float spiritSpeed, float flingSpeed) {
			ClassType = MovementClassID.Spirit;
			StatMovement.StaminaCost.Set(staminaCost);
			StatMovement.DistanceDecayRate.Set(distanceDecayRate);
			StatMovement.DamageCost.Set(damageCost);
			StatMovement.SpiritSpeed.Set(spiritSpeed);
			StatMovement.FlingSpeed.Set(flingSpeed);
		}

		public void UpdateGrappler(int staminaCost, int range, float pullSpeed, float reelSpeed) {
			ClassType = MovementClassID.Grappler;
			StatMovement.StaminaCost.Set(staminaCost);
			StatMovement.Range.Set(range);
			StatMovement.PullSpeed.Set(pullSpeed);
			StatMovement.ReelSpeed.Set(reelSpeed);
		}
	}
}