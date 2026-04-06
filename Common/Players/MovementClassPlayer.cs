using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NullandVoid.Common.Systems;
using NullandVoid.Content.Buffs;
using NullandVoid.Content.Projectiles;
using NullandVoid.Core;
using NullandVoid.Utils;
using ReLogic.Content;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.Graphics;
using Terraria.Graphics.CameraModifiers;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace NullandVoid.Common.Players
{
	public class MovementClassPlayer : ModPlayer
	{
		public static Asset<Texture2D> SpiritBody;
			
		public int StatStaminaMax;
		public int StatStamina;
		public int StaminaRegen;
		public int StaminaRegenCount;
		public int StaminaCost;
		public int StaminaCostAlt;

		public int ClassType;
		public int AbilityType;
		public bool UsingAbility;
		public bool UsingAltAbility;
		public Vector2 PreAbilityVel;
		public int AbilityDirection;
		public SlotId SoundSlot;
		public int AbilityFrame;
		public int MovementFrame;
		public int MovementFrameCount;
		public int Cooldown;
		public int CooldownAlt;
		public bool DrawAcc;
		public bool ChangedAcc;
		public Asset<Texture2D> AccTexture;
		public bool CanCancel;
		public bool CanCancelAlt;
		public bool oldJumped;


		public StatMovement StatMovement = new();

		public Vector2 ChargeAccel;
		public float ChargeSpeed;
		public bool Slamming;

		public Vector2 SpiritPosition;
		public float SpiritDistanceSq;
		public float SpiritDistance;

		public Vector2 GrappledPosition;
		public float GrappledLength;
		public float GrappleRotation;
		


		private void ResetMovement() {
			StatStaminaMax = 40;
			StaminaRegen = 8;

			ClassType = MovementClassID.None;
			ChangedAcc = false;
			DrawAcc = false;
			StatMovement.Reset();
		}

		public override void Load() {
			SpiritBody = ModContent.Request<Texture2D>("NullandVoid/Assets/Textures/SpiritBody");
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
			StaminaCostAlt = StatMovement.StaminaCostAlt.Get();
			if (Cooldown > 0) {
				Cooldown--;
			}
			if (CooldownAlt > 0) {
				CooldownAlt--;
			}

			if (UsingAltAbility) {
				return;
			}

			if (UsingAbility) {
				if (ClassType == MovementClassID.Spirit) {
					Player.moveSpeed += StatMovement.SpiritSpeed.Get();
				}
				return;
			}

			if (StatStamina < StatStaminaMax) {
				StaminaRegenCount += StaminaRegen;
				if (StaminaRegenCount >= 60) {
					StatStamina++;
					StaminaRegenCount -= 60;
					if (StatStamina % 20 == 0) {
						StatStamina = Math.Min(StatStamina, StatStaminaMax);
						SoundEngine.PlaySound(SoundID.Item53 with { Volume = 0.7f * ModContent.GetInstance<NullandVoidClientConfig>().StaminaSoundVolume, Pitch = (float)StatStamina / StatStaminaMax - 0.5f });
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
			if ((!KeybindSystem.MovementAbilityKeybind.JustPressed && !KeybindSystem.MovementAltAbilityKeybind.JustPressed) || ClassType == MovementClassID.None || Player.mount.Active) {
				return;
			}

			if (ClassType == MovementClassID.Grappler && !UsingAbility && !UsingAltAbility) {
				if (KeybindSystem.MovementAbilityKeybind.JustPressed && Player.GetModPlayer<MovementMiscPlayer>().Grounded) {
					return;
				}
				
				(NPC grappledNPC, Vector2? grappledPos) = FindGrapplePoint();
				if (grappledPos != null) {
					Projectile.NewProjectile(Player.GetSource_FromThis(), grappledPos.Value, Vector2.Zero, ModContent.ProjectileType<WoodenGrappleProjectile>(), 0, 0, Player.whoAmI, grappledNPC?.whoAmI ?? 0);
				}
				else {
					return;
				}
			}
			
			if (KeybindSystem.MovementAbilityKeybind.JustPressed) {
				if (Cooldown > 0 || (ClassType == MovementClassID.Dasher && StatStamina < StaminaCost) || (ClassType == MovementClassID.Spirit && !Player.GetModPlayer<MovementMiscPlayer>().Grounded && !UsingAbility)) {
					return;
				}
				
				if (Main.netMode != NetmodeID.SinglePlayer) {
					NullandVoidNetwork.SendMovementAbilityMessage(Player.whoAmI, (int)triggersSet.DirectionsRaw.X);
				}
				UsingAbility = true;
				UseAbility((int)triggersSet.DirectionsRaw.X, false);
			}
			else {
				if (CooldownAlt > 0) {
					return;
				}
				
				if (ClassType == MovementClassID.Dasher) {
					if (StatStamina < StaminaCostAlt) {
						return;
					}
				}
				else {
					switch (ClassType) {
						case MovementClassID.Charger when Player.GetModPlayer<MovementMiscPlayer>().Grounded:
						case MovementClassID.Spirit when !UsingAbility:
							return;
					}
				}
				
				if (Main.netMode != NetmodeID.SinglePlayer) {
					NullandVoidNetwork.SendMovementAltAbilityMessage(Player.whoAmI, (int)triggersSet.DirectionsRaw.X);
				}
				UsingAltAbility = true;
				UseAbility((int)triggersSet.DirectionsRaw.X, true);
			}
		}

		public void UseAbility(int direction, bool alt) {
			oldJumped = Player.controlJump;
			StaminaRegenCount = 0;
			PreAbilityVel = Player.velocity;
			AbilityDirection = direction;
			if (AbilityDirection == 0) {
				AbilityDirection = Player.direction;
			}

			switch (ClassType) {
				case MovementClassID.Dasher:
					AbilityFrame = StatMovement.DashTime.Get();
					Player.AddBuff(ModContent.BuffType<LungedBuff>(), 45);
					if (!alt) {
						Cooldown = 2;
						ChangeStatStamina(-StaminaCost);
					}
					else {
						CooldownAlt = 2;
						ChangeStatStamina(-StaminaCostAlt);
					}
					break;
				case MovementClassID.Charger:
					if (!alt) {
						PreAbilityVel /= 2;
						if (CanCancel) {
							CancelAbility(false, 30);
						}
						else if (CanCancelAlt) {
							CancelAbility(true, 30);
							ActivateAbility(false);
						}
						else {
							ActivateAbility(false);
						}
					}
					else {
						if (CanCancelAlt) {
							CancelAbility(true, 10);
						}
						else {
							SoundEngine.PlaySound(SoundID.DD2_BetsysWrathShot with { Volume = 0.2f * ModContent.GetInstance<NullandVoidClientConfig>().ChargeAbilitiesVolume, PitchVariance = 0.2f }, Player.Center);
							if (CanCancel) {
								CancelAbility(false, 30);
							}
							ActivateAbility(true);
						}
					}
					break;
				case MovementClassID.Spirit:
					if (CanCancel) {
						CancelAbility(false, 30);
						break;
					}

					if (Player.whoAmI == Main.myPlayer) {
						Filters.Scene.Activate("NullandVoid:SpiritVignette");
					}

					SpiritPosition = Player.Center;
					SoundEngine.PlaySound(SoundID.Item78 with { Volume = 0.6f * ModContent.GetInstance<NullandVoidClientConfig>().SpiritAbilitiesVolume, Pitch = 0.5f, PitchVariance = 0.2f }, Player.Center);
					ActivateAbility(false);
					int dustType = GetSpiritDustType();
					for (int i = 0; i < 27 * ModContent.GetInstance<NullandVoidClientConfig>().GeneralDustAmount; i++) {
						Dust dust = Dust.NewDustDirect(Player.Center, 10, 10, dustType, Scale: 2);
						dust.velocity *= 2f;
						dust.noGravity = true;
					}
					break;
				case MovementClassID.Grappler:
					AbilityFrame = 0;
					if (!alt) {
						if (CanCancel) {
							CancelAbility(false, 10);
						}
						else if (CanCancelAlt) {
							CancelAbility(true, 30);
						}
						else {
							ActivateAbility(false);
						}
					}
					else {
						if (CanCancel) {
							CancelAbility(false, 10);
							ActivateAbility(true);
						}
						else if (CanCancelAlt) {
							CancelAbility(true, 30);
						}
						else {
							ActivateAbility(true);
						}
					}
					break;
			}
		}

		public void CancelAbility(bool alt, int cooldown) {
			if (alt) {
				CooldownAlt = cooldown;
				CanCancelAlt = false;
			}
			else {
				Cooldown = cooldown;
				CanCancel = false;
			}
		}
		
		public void ActivateAbility(bool alt, int cooldown = 2) {
			if (alt) {
				CooldownAlt = cooldown;
				CanCancelAlt = true;
			}
			else {
				Cooldown = cooldown;
				CanCancel = true;
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
					if (UsingAbility) {
						ChargeAbility();
					}
					if (UsingAltAbility) {
						ChargeAltAbility();
					}
					break;
				case MovementClassID.Spirit:
					SpiritAbility();
					break;
				case MovementClassID.Grappler:
					if (UsingAbility) {
						GrappleAbility();
					}
					else {
						GrappleAltAbility();
					}
					break;
			}
		}

		public override void DrawPlayer(Camera camera) {
			if (UsingAbility) {
				Player.armorEffectDrawShadow = true;
				switch (ClassType) {
					case MovementClassID.Charger:
						Player.legFrame.Y = 392 + 56 * (MovementFrame % 12);
						Player.bodyFrame.Y = 392 + 56 * (MovementFrame % 12);
						Player.direction = Player.velocity.X switch {
							> 0 => 1,
							< 0 => -1,
							_ => Player.direction,
						};

						Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Quarter, -Player.direction * MathHelper.PiOver2);
						Player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Quarter, -Player.direction * MathHelper.PiOver2);
						DrawAcc = true;
						break;
					case MovementClassID.Dasher:
						Player.armorEffectDrawShadow = true;
						break;
					case MovementClassID.Spirit:
						Vector2 offPos = SpiritPosition - Player.Center;
						SpiritDistanceSq = offPos.X * offPos.X + offPos.Y * offPos.Y;
						if (Player.whoAmI == Main.myPlayer) {
							Vector2 screenCenter = NullandVoidUtils.ScreenCenter();
							int color = Math.Min(255, (int)SpiritDistanceSq / 256);
							Main.EntitySpriteDraw(SpiritBody.Value, SpiritPosition - Player.BottomRight + screenCenter + new Vector2(0, MathF.Sin((float)StatStamina * 10 / StatStaminaMax)), null, new Color(255, 255, 255, 0), 0, Vector2.Zero, 1, AbilityDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
							Main.EntitySpriteDraw(TextureAssets.MagicPixel.Value, screenCenter, new Rectangle(0, 0, (int)SpiritDistance, 2), new Color(color, color, color, 0), (SpiritPosition.X >= Player.Center.X ? 0 : MathHelper.Pi) + MathF.Atan(offPos.Y / offPos.X), Vector2.Zero, 1, SpriteEffects.None);
						}
						break;
					case MovementClassID.Grappler:
						Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Math.Sign(Player.direction) * -2.5f);
						Player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, Math.Sign(Player.direction) * -3);
						break;
				}
			}
			else if (UsingAltAbility) {
				Player.armorEffectDrawShadow = true;
				if (ClassType == MovementClassID.Grappler) {
					float angle = Player.AngleTo(GrappledPosition) - MathHelper.PiOver2;
					Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, angle);
					Player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.ThreeQuarters, angle);
				}
			}
		}

		public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright) {
			if (!UsingAbility || ClassType != MovementClassID.Spirit) {
				return;
			}

			Lighting.AddLight(Player.Center, 0.5f, 0.5f, 0.5f);
			switch (AbilityType) {
				case (int)SpiritType.Slime:
					r = 0.3f;
					g = 0.6f;
					b = 1;
					a = 0.5f;
					break;
			}
		}

		public bool CheckJumped() {
			bool jumped = !oldJumped && Player.controlJump;
			oldJumped = Player.controlJump;
			return jumped;
		}


		public void DashAbility() {
			int dashTime = StatMovement.DashTime.Get();
			float dashSpeed = StatMovement.DashSpeed.Get();
			if (dashTime == AbilityFrame) {
				// Start of Dash
				Player.SetImmuneTimeForAllTypes(StatMovement.IFrame.Get());
				Player.velocity.X = (8 + dashSpeed) * Player.direction;

				if (UsingAbility) {
					SoundEngine.PlaySound(SoundID.DD2_BetsysWrathShot with { Pitch = 0.6f, Volume = 0.3f * ModContent.GetInstance<NullandVoidClientConfig>().DashAbilitiesVolume, PitchVariance = 0.2f }, Player.Center);
				}
				else {
					Player.velocity.X *= 1.5f;
					Player.velocity.Y = -3;
					SoundEngine.PlaySound(SoundID.DD2_BetsysWrathShot with { Pitch = -0.2f, Volume = 0.4f * ModContent.GetInstance<NullandVoidClientConfig>().DashAbilitiesVolume }, Player.Center);
				}
			}

			if (AbilityFrame == 1) {
				// End of dash
				if (UsingAbility) {
					UsingAbility = false;
					Player.velocity.X = (Math.Max(Math.Abs(PreAbilityVel.X), 3.5f) + 2) * AbilityDirection;
				}
				else {
					UsingAltAbility = false;
					Player.velocity.X *= 0.8f;
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
			if (StatStamina <= 0 || !CanCancel || CheckJumped()) {
				CancelAbility(false, 30);
				UsingAbility = false;
				ChargeAccel = Vector2.Zero;
				Player.fallStart = Player.position.ToTileCoordinates().Y;
				MovementFrame = 0;
				MovementFrameCount = 0;
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
					PreAbilityVel.X *= 0.98f;
				}
			}

			Player.controlUseItem = false;
			StaminaRegenCount += StaminaCost;
			if (StaminaRegenCount >= 60) {
				StatStamina--;
				StaminaRegenCount -= 60;
			}

			if (!SoundEngine.TryGetActiveSound(SoundSlot, out ActiveSound _)) {
				SoundSlot = SoundEngine.PlaySound(SoundID.Run with { Volume = ModContent.GetInstance<NullandVoidClientConfig>().ChargeAbilitiesVolume * 0.9f, Pitch = ChargeSpeed - 0.25f, PitchVariance = 0.1f }, Player.Center);
			}

			MovementFrameCount += (int)((ChargeSpeed + 0.5f) * 10);
			if (MovementFrameCount > 12) {
				MovementFrameCount -= 12;
				MovementFrame++;
			}

			Dust dust = Dust.NewDustDirect(Player.Bottom + 2 * PreAbilityVel, 1, 1, DustID.GemDiamond, -PreAbilityVel.X, -PreAbilityVel.Y, Scale: ChargeSpeed * 2.67f * ModContent.GetInstance<NullandVoidClientConfig>().GeneralDustAmount, newColor: new Color(255, 255, 0));
			dust.noGravity = true;

			PreAbilityVel += ChargeAccel;
			Player.velocity = PreAbilityVel;

			if (Player.whoAmI != Main.myPlayer) {
				return;
			}

			foreach (NPC npc in Main.ActiveNPCs) {
				if (!(npc.DistanceSQ(Player.Center) < 200) || npc.immune[Player.whoAmI] != 0) {
					continue;
				}

				int damage = (int)(StatMovement.ImpactDamage.Get() * ChargeSpeed) + 3;
				npc.SimpleStrikeNPC(damage, Player.direction, false, 10 * (ChargeSpeed + 3));
				npc.immune[Player.whoAmI] = 15;
				Player.GetModPlayer<BloodPlayer>().BloodHeal(damage);
			}
		}

		public void ChargeAltAbility() {
			if (Player.GetModPlayer<MovementMiscPlayer>().Grounded || StatStamina <= 0 || !CanCancelAlt || Slamming || CheckJumped()) {
				if (Slamming) {
					Slamming = false;
					CooldownAlt = 60;
					Main.instance.CameraModifiers.Add(new PunchCameraModifier(Main.LocalPlayer.Center, (Main.rand.NextFloat() * MathHelper.Pi).ToRotationVector2(), 8.5f * ModContent.GetInstance<NullandVoidClientConfig>().CameraShakeIntensity, 8, 20, 1000f, FullName));
					SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundMiss with { Volume = ModContent.GetInstance<NullandVoidClientConfig>().ChargeAbilitiesVolume * 0.9f, PitchVariance = 0.2f, MaxInstances = 8 }, Player.Center);
					SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Volume = 0.8f * ModContent.GetInstance<NullandVoidClientConfig>().ChargeAbilitiesVolume, PitchVariance = 0.2f, MaxInstances = 8 }, Player.Center);
					for (int i = 0; i < 20 * ModContent.GetInstance<NullandVoidClientConfig>().GeneralDustAmount; i++) {
						Dust dust = Dust.NewDustDirect(Player.Bottom, 0, 0, DustID.Dirt, Alpha: 128, Scale: 3);
						dust.velocity.X *= 8;
						dust.noGravity = true;
					}

					if (Player.whoAmI == Main.myPlayer) {
						int damage = StatMovement.ImpactDamage.Get();
						int count = 0;
						foreach (NPC npc in Main.ActiveNPCs) {
							float distance = npc.DistanceSQ(Player.Center);
							if (!(distance < 20000)) {
								continue;
							}

							npc.SimpleStrikeNPC(damage, Player.Center.X > npc.Center.X ? -1 : 1, false, (20000 - distance) / 2048);
							count++;
						}
						
						Player.GetModPlayer<StylePlayer>().AddStyleBonus(StyleBonus.Slammed, count); 
						Player.GetModPlayer<BloodPlayer>().BloodHeal(damage * count);
					}
				}
				else {
					CooldownAlt = 10;
				}

				CanCancelAlt = false;
				UsingAltAbility = false;
				Player.velocity.Y = 0;
				return;
			}

			Player.velocity.Y = StatMovement.MaxSpeed.Get() + 2;
			Player.fallStart = Player.position.ToTileCoordinates().Y;

			StaminaRegenCount += StaminaCostAlt;
			if (StaminaRegenCount >= 60) {
				StatStamina--;
				StaminaRegenCount -= 60;
			}
			
			foreach (NPC npc in Main.ActiveNPCs) {
				if (npc.Top.Y - Player.Bottom.Y < 20 && npc.Top.Y - Player.Bottom.Y > -20 && npc.Left.X - Player.Right.X < 10 && npc.Right.X - Player.Left.X > -10) {
					Slamming = true;
					Player.GiveImmuneTimeForCollisionAttack(15);
				}
			}
		}

		public void SpiritAbility() {
			if (StatStamina <= 0 || !CanCancel) {
				for (int i = 0; i < 14 * ModContent.GetInstance<NullandVoidClientConfig>().GeneralDustAmount; i++) {
					Dust dust = Dust.NewDustDirect(SpiritPosition, 10, 20, DustID.GemDiamond, Scale: 2);
					dust.velocity *= 1.5f;
					dust.noGravity = true;
				}
				Player.fallStart = Player.position.ToTileCoordinates().Y;
				CanCancel = false;
				UsingAbility = false;
				float flingFactor = 0;
				if (UsingAltAbility) {
					UsingAltAbility = false;
					flingFactor = Math.Clamp(MathF.Cbrt(SpiritDistanceSq) * StatMovement.FlingSpeed.Get() / 32, StatMovement.FlingSpeed.Get(), 30);
					Player.velocity = (SpiritPosition - Player.Center).SafeNormalize(Vector2.Zero) * flingFactor;
					CooldownAlt = 30 + (int)flingFactor * 4;
				}
				else {
					Player.velocity = Vector2.Zero;
					Cooldown = 30;
				}
				Player.SetImmuneTimeForAllTypes(UsingAbility ? (int)(2 * flingFactor) : (int)(SpiritDistance / 16));
				Player.Center = SpiritPosition;

				if (Player.whoAmI != Main.myPlayer) {
					return;
				}
				
				Filters.Scene["NullandVoid:SpiritVignette"].GetShader().UseProgress(0);
				Filters.Scene["NullandVoid:SpiritVignette"].Deactivate();
				return;
			}

			switch (AbilityType) {
				case (int)SpiritType.Slime:
					if (Player.controlJump && Player.velocity.Y < PreAbilityVel.Y) {
						Player.velocity.Y *= 2.7f;
						Player.jump = 0;
					}
					break;
			}
			
			PreAbilityVel = Player.velocity;

			if (Player.whoAmI == Main.myPlayer) {
				Filters.Scene["NullandVoid:SpiritVignette"].GetShader().UseProgress((float)StatStamina / StatStaminaMax);
			}

			SpiritDistance = MathF.Sqrt(SpiritDistanceSq);
			StaminaRegenCount += StaminaCost + (int)(StatMovement.DistanceDecayRate.Get() * SpiritDistance / 256);
			if (StaminaRegenCount >= 60) {
				StatStamina--;
				StaminaRegenCount -= 60;
			}
		}

		public void GrappleAbility() {
			if (StatStamina <= 0 || !CanCancel || CheckJumped() || Player.GetModPlayer<MovementMiscPlayer>().Grounded) {
				UsingAbility = false;
				CancelAbility(false, 30);
				Player.fallStart = Player.position.ToTileCoordinates().Y;
				AbilityFrame = 0;
				if (UsingAltAbility) {
					return;
				}

				Player.velocity.X *= 1.3f;
				Player.velocity.Y *= 1.7f;
				Player.fullRotation = 0;
				return;
			}
			
			Player.controlUseItem = false;
			Player.fullRotationOrigin = new Vector2((float)Player.width / 2, (float)Player.height / 2);
			GrappleRotation = Player.Center.AngleTo(GrappledPosition) + MathHelper.PiOver2;
			Player.fullRotation = (GrappleRotation + Player.fullRotation * 3) / 4;

			StaminaRegenCount += StaminaCost;
			if (StaminaRegenCount >= 60) {
				StatStamina--;
				StaminaRegenCount -= 60;
			}

			float distSq = Player.Center.DistanceSQ(GrappledPosition);
			if (GrappledLength * GrappledLength > distSq || GrappledPosition == Vector2.Zero || AbilityFrame == 0) {
				AbilityFrame++;
				return;
			}

			if (distSq > GrappledLength * GrappledLength + 10000) {
				Player.Center = (Player.Center - GrappledPosition).SafeNormalize(Vector2.Zero) * GrappledLength + GrappledPosition;
			}
			float boostFactor = Math.Clamp((1.2f - (float)AbilityFrame / 64) * GrappledLength / 1024, 1, 1.2f);
			Player.velocity = (((Player.Center + new Vector2(Player.velocity.X, Player.velocity.Y * 1.05f) - GrappledPosition).SafeNormalize(Vector2.Zero) * GrappledLength + GrappledPosition) - Player.Center) * boostFactor;
			AbilityFrame++;
		}

		public void GrappleAltAbility() {
			if (StatStamina <= 0 || !CanCancelAlt || CheckJumped()) {
				UsingAltAbility = false;
				CancelAbility(true, 30);
				Player.velocity.X /= 2;
				Player.fullRotation = 0;
				return;
			}
			
			Player.direction = Player.Center.X > GrappledPosition.X ? -1 : 1;
			
			if (GrappledPosition.DistanceSQ(Player.Center) < 2000) {
				CanCancelAlt = false;
				Player.GiveImmuneTimeForCollisionAttack(30);
			}
			
			Player.fullRotation *= 0.75f;
			StaminaRegenCount += StaminaCostAlt;
			if (StaminaRegenCount >= 60) {
				StatStamina--;
				StaminaRegenCount -= 60;
			}
		}
			

		public (NPC?, Vector2?) FindGrapplePoint() {
			NPC? grappledNPC = null;
			Vector2? grappledPos = null;
			float minDistSq = MathF.Pow(StatMovement.Range.Get() * 16, 2);
			
			Vector2 direction = (Main.MouseWorld - Player.Center).SafeNormalize(Vector2.Zero) * 16;
			foreach (NPC npc in Main.ActiveNPCs) {
				float distSq = npc.DistanceSQ(Player.Center);
				if (distSq > minDistSq) {
					continue;
				}

				for (int i = 0; i < StatMovement.Range.Get(); i++) {
					Vector2 pos = Player.Center + direction * i;
					if (npc.Hitbox.Contains((int)pos.X, (int)pos.Y)) {
						grappledNPC = npc;
						grappledPos = pos;
						minDistSq = distSq;
					}
				}
			}
			
			for (int i = 0; i < MathF.Sqrt(minDistSq) / 16; i++) {
				Vector2 pos = Player.Center + direction * i;
				if (WorldGen.SolidTile2((int)(pos.X / 16), (int)(pos.Y / 16))) {
					grappledPos = new Vector2((int)(pos.X / 16) * 16 + 8, (int)(pos.Y / 16) * 16 + (WorldGen.SolidTile((int)(pos.X / 16), (int)(pos.Y / 16)) ? 8 : 4));
					return (grappledNPC, grappledPos);
				}
			}

			if (grappledNPC != null) {
				return (grappledNPC, grappledPos);
			}
			
			for (int i = 3; i < MathF.Sqrt(minDistSq) / 16; i += 4) {
				Vector2 pos = Player.Center + direction * i;
				for (int j = 0; j < 4; j++) {
					int x = (int)(pos.X / 16) + (j / 2 == 0 ? 1 + j : 1 - j);
					for (int k = 0; k < 4; k++) {
						int y = (int)(pos.Y / 16) + (k / 2 == 0 ? 1 + k : 1 - k);
						if (WorldGen.SolidTile2(x, y)) {
							grappledPos = new Vector2(x * 16 + 8, y * 16 + (WorldGen.SolidTile(x, y) ? 8 : 4));
							break;
						}
					}
				}
			}
			return (null, grappledPos);
		}


		public int GetSpiritDustType() {
			return AbilityType switch {
				(int)SpiritType.Slime => DustID.BlueFairy,
				_ => 0,
			};
		}
		
		
		public override void ModifyHurt(ref Player.HurtModifiers modifiers) {
			if (!UsingAbility) {
				return;
			}

			switch (ClassType) {
				case MovementClassID.Charger: {
					if (Main.netMode != NetmodeID.SinglePlayer) {
						NullandVoidNetwork.SendSoundMessage(Player.whoAmI, SoundsID.ChargeHurt, (int)(ChargeSpeed * 10));
					}
					else {
						SoundEngine.PlaySound(Sounds.GetSound(SoundsID.ChargeHurt, (int)(ChargeSpeed * 10)));
					}

					modifiers.FinalDamage *= 1 - StatMovement.DamageReduction.Get();
					break;
				}
			}
		}

		public override bool ConsumableDodge(Player.HurtInfo info) {
			if (!UsingAbility || ClassType != MovementClassID.Spirit || StatStamina <= 0) {
				return false;
			}

			if (Player.immuneTime > 0) {
				return true;
			}

			int damage = (int)(info.Damage * StatMovement.DamageAbsorption.Get());
			StatStamina -= damage;
			if (StatStamina < 0) {
				damage = Math.Abs(StatStamina);
				StatStamina = 0;
				Player.Hurt(info.DamageSource, damage, info.HitDirection);
			}
			else {
				SoundEngine.PlaySound(SoundID.NPCHit52);
				CombatText.NewText(Player.getRect(), new Color(128, 128, 255, 0.25f), damage);
			}

			Player.SetImmuneTimeForAllTypes(40);
			return true;
		}


		public MovementClassPlayer UpdateDasher(int staminaCost, int staminaCostAlt, int dashTime, float dashSpeed, int iFrame) {
			ClassType = MovementClassID.Dasher;
			StatMovement.StaminaCost.Set(staminaCost);
			StatMovement.StaminaCostAlt.Set(staminaCostAlt);
			StatMovement.DashTime.Set(dashTime);
			StatMovement.DashSpeed.Set(dashSpeed);
			StatMovement.IFrame.Set(iFrame);
			return this;
		}

		public MovementClassPlayer UpdateCharger(int staminaCost, int staminaCostAlt, float maxSpeed, float chargeAccel, float turnAccel, float damageReduction, int impactDamage) {
			ClassType = MovementClassID.Charger;
			StatMovement.StaminaCost.Set(staminaCost);
			StatMovement.StaminaCostAlt.Set(staminaCostAlt);
			StatMovement.MaxSpeed.Set(maxSpeed);
			StatMovement.ChargeAccel.Set(chargeAccel);
			StatMovement.TurnAccel.Set(turnAccel);
			StatMovement.DamageReduction.Set(damageReduction);
			StatMovement.ImpactDamage.Set(impactDamage);
			return this;
		}

		public MovementClassPlayer UpdateSpirit(int staminaCost, int staminaCostAlt, float distanceDecayRate, float damageAbsorption, float spiritSpeed, float flingSpeed) {
			ClassType = MovementClassID.Spirit;
			StatMovement.StaminaCost.Set(staminaCost);
			StatMovement.StaminaCostAlt.Set(staminaCostAlt);
			StatMovement.DistanceDecayRate.Set(distanceDecayRate);
			StatMovement.DamageAbsorption.Set(damageAbsorption);
			StatMovement.SpiritSpeed.Set(spiritSpeed);
			StatMovement.FlingSpeed.Set(flingSpeed);
			return this;
		}

		public MovementClassPlayer UpdateGrappler(int staminaCost, int staminaCostAlt, int range, float pullSpeed, float reelSpeed) {
			ClassType = MovementClassID.Grappler;
			StatMovement.StaminaCost.Set(staminaCost);
			StatMovement.StaminaCostAlt.Set(staminaCostAlt);
			StatMovement.Range.Set(range);
			StatMovement.PullSpeed.Set(pullSpeed);
			StatMovement.ReelSpeed.Set(reelSpeed);
			return this;
		}
	}
}