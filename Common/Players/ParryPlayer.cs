using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using NullandVoid.Common.Systems;
using NullandVoid.Content.Projectiles;
using Terraria;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;

namespace NullandVoid.Common.Players
{
	public class ParryPlayer : ModPlayer
	{
		// Parrying stats fields
		public const int ParryResourceMax = 50;
		public const int ParryWindowMax = 10;
		public int ParryResource;
		public int ParryUsage;
		public int ParryRegenRate;
		public int ParryFrame;
		public List<int> ParriedProjectiles = [];
		public List<int> ParriedNPCs = [];
		public float ParryAngle;
		public int ParryDirection;
		private int parryTimer;
		private int parryWindow;
		private int parryRange;
		private int projectileBoostCount;
		
		private float parrySoundVolume = ModContent.GetInstance<NullandVoidClientConfig>().ParrySoundVolume;
		private float parryShakeIntensity = ModContent.GetInstance<NullandVoidClientConfig>().ParryShakeIntensity;


		public void ChangeConfig() {
			parrySoundVolume = ModContent.GetInstance<NullandVoidClientConfig>().ParrySoundVolume;
			parryShakeIntensity = ModContent.GetInstance<NullandVoidClientConfig>().ParryShakeIntensity;
			Main.NewText(parrySoundVolume);
		}
		
		// Reset parrying stats
		private void ResetParry() {
			ParryRegenRate = 3;
			ParryUsage = 50;
			parryRange = 50;
		}

		public override void Load() {
			ResetParry();
		}

		public override void ResetEffects() {
			ResetParry();
		}

		public override void UpdateDead() {
			ResetParry();
		}
		

		
		public void AddParryResource(int addAmount) {
			ParryResource = Math.Clamp(ParryResource + addAmount, 0, ParryResourceMax);
		}
		
		public override void PostUpdateMiscEffects() {
			parryTimer++;

			if (parryTimer >= 5 && ParryResource < ParryResourceMax) {
				ParryResource += ParryRegenRate;
				if (ParryResource >= ParryResourceMax) {
					ParryResource = Math.Clamp(ParryResource, 0, ParryResourceMax);
					SoundEngine.PlaySound(new SoundStyle("NullandVoid/Assets/Sounds/ParryFilled") with {Volume = 0.4f * parrySoundVolume, PitchVariance = 0.5f, Pitch = 0.3f});
				}
				parryTimer = 0;
			}

			if (parryWindow != 0) {
				parryWindow--;
				(List<int> parryingProjectiles, List<int> parryingNPCs) = GetParried();
				int parryCount = parryingProjectiles.Count + parryingNPCs.Count;
				if (parryCount != 0) {
					ParryReflect(parryingProjectiles, parryingNPCs);
					if (parryCount - projectileBoostCount != 0) {
						Main.LocalPlayer.GetModPlayer<StaminaPlayer>().AddStaminaResource(5 * parryCount - projectileBoostCount);
					}
				}
				Player.ChangeDir(ParryDirection);
			}

			if (ParryFrame != 0) {
				ParryFrame--;
			}
		}

		public override void ProcessTriggers(TriggersSet triggersSet) {
			if (KeybindSystem.ParryKeybind.JustPressed && ParryResource >= ParryUsage) {
				parryWindow = ParryWindowMax;
				ParriedNPCs.Clear();
				ParriedProjectiles.Clear();
				projectileBoostCount = 0;
				ParryAngle = MathF.Atan((Main.MouseScreen.Y - Main.screenHeight / 2) / (Math.Abs(Main.MouseScreen.X - Main.screenWidth / 2) * Player.direction));
				ParryDirection = (int)Math.Clamp(Main.MouseScreen.X - Main.screenWidth / 2, -1, 1);
				
				AddParryResource(-ParryUsage);
				ParryEffects(Player.whoAmI, 0, Player.Center);
			}
		}

		public void ParryEffects(int whoAmI, int parryCount, Vector2 parryPosition) {
			Player player = Main.player[whoAmI];

			if (parryCount == 0) {
				SoundEngine.PlaySound(SoundID.DD2_MonkStaffSwing with {Volume = parrySoundVolume, Pitch = 0.2f}, player.Center);
			}
			else {
				SoundEngine.PlaySound(new SoundStyle("NullandVoid/Assets/Sounds/ParryHit") with {Volume = parrySoundVolume, PitchVariance = 0.2f, Pitch = (parryCount - 1) * 0.1f}, player.Center);
				
				for (int i = 0; i < 8; i++) {
					Dust dust = Dust.NewDustDirect(parryPosition, 10, 10, DustID.Firework_Yellow, Scale: 0.6f);
					dust.noGravity = true;
				}
			}
			
			
			if (Player.whoAmI != Main.myPlayer) {
				return;
			}

			if (parryCount != 0) {
				Main.instance.CameraModifiers.Add(new PunchCameraModifier(Player.Center, (Main.rand.NextFloat() * MathHelper.Pi).ToRotationVector2(), 8f * parryShakeIntensity, 7.5f, 15, 1000f, FullName));
			}
			else if (!(!Player.HeldItem.IsAir && Player.HeldItem is { melee: true, pick: 0, axe: 0, useStyle: ItemUseStyleID.Swing or ItemUseStyleID.Rapier })) {
				Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.Zero, ModContent.ProjectileType<ParrySwordProjectile>(), 0, ParryAngle, Main.myPlayer);
			}

			if (Main.netMode != NetmodeID.SinglePlayer) {
				SendParryEffectsMessage(Player.whoAmI, parryCount, parryPosition);
			}
		}
		
		public override bool ConsumableDodge(Player.HurtInfo info) {
			if (parryWindow > 0) {
				if (info.DamageSource.SourceOtherIndex == 0) {
					ParryFrame = 20;
					ParryAngle = 0;
					ParryEffects(Player.whoAmI, 1, Player.Bottom);
					Player.velocity = new Vector2(Player.velocity.X * 2f, -10);
					NetMessage.SendData(MessageID.PlayerControls, number: Player.whoAmI);		
				}
				return true;
			}
			return false;
		}

		public (List<int> parryingProjectiles, List<int> parryingNPCs) GetParried() {
			List<int> parryingProjectiles = [];
			List<int> parryingNPCs = [];
			
			foreach (Projectile projectile in Main.ActiveProjectiles) {
				if (!ParriedProjectiles.Contains(projectile.whoAmI) && (projectile.Center.X - Player.Center.X) * Player.direction > 0  && Player.DistanceSQ(projectile.Center) <= parryRange * parryRange && (((projectile.DamageType == DamageClass.Ranged || projectile.DamageType == DamageClass.Magic) && projectile.owner == Player.whoAmI) || projectile.hostile)) {
					parryingProjectiles.Add(projectile.whoAmI);
				}
			}
			
			foreach (NPC npc in Main.ActiveNPCs) {
				if (!ParriedNPCs.Contains(npc.whoAmI) && (npc.Center.X - Player.Center.X) * Player.direction > 0  && Player.DistanceSQ(npc.Center) <= parryRange * parryRange && npc.damage != 0 && !npc.friendly) {
					parryingNPCs.Add(npc.whoAmI);
				}
			}

			return (parryingProjectiles, parryingNPCs);
		}

		public void ParryReflect(List<int> parryingProjectiles, List<int> parryingNPCs) {
			Vector2 knockbackVelocity = Vector2.Zero;
			int parriedDamage = 0;
			int tempProjectileBoostCount = 0;
				
			foreach (int i in parryingProjectiles) {
				ParriedProjectiles.Add(i);
				Projectile projectile =  Main.projectile[i];
				if (projectile.hostile) {
					projectile.hostile = false;
					projectile.friendly = true;
					projectile.velocity *= -1;
					parriedDamage += (int)(projectile.damage * 1.5f);
				}
				else {
					projectileBoostCount++;
					tempProjectileBoostCount++;
				}

				projectile.damage *= 2;
				projectile.knockBack *= 1.5f;
				projectile.velocity *= Math.Clamp((16 / projectile.velocity.Length()), 1f, 1.75f);
				projectile.netUpdate = true;

				knockbackVelocity = projectile.velocity;
				knockbackVelocity.Normalize();
				Player.velocity -= knockbackVelocity * 5 / ParriedProjectiles.Count;
			}

			foreach (int i in parryingNPCs) {
				ParriedNPCs.Add(i);
				NPC npc = Main.npc[i];
				Vector2 approachVelocity = Player.velocity - npc.velocity;
				npc.SimpleStrikeNPC(npc.damage * 2, Player.direction, false, MathF.Sqrt(approachVelocity.Length() + 10) + 5);
				
				parriedDamage += npc.damage / 2;
				knockbackVelocity = approachVelocity;
				knockbackVelocity.Normalize();
				Player.velocity -= knockbackVelocity * approachVelocity.Length() * (1 - npc.knockBackResist);
				
			}

			ParryAngle = MathF.Atan(knockbackVelocity.Y / knockbackVelocity.X);
			ParryFrame = 20;
			int parryCount = ParriedProjectiles.Count + ParriedNPCs.Count;
			
			int parryHeal = parriedDamage / parryCount;
			if (parryHeal != 0) {
				Player.Heal(parryHeal);
			}
			
			Player.fallStart = Player.position.ToTileCoordinates().Y;
			
			int tempParryCount = parryingProjectiles.Count + parryingNPCs.Count - tempProjectileBoostCount;
			StylePlayer stylePlayer = Player.GetModPlayer<StylePlayer>();
			if (tempParryCount != 0) {
				stylePlayer.AddStyleBonus(StyleBonusesList.Parry, tempParryCount);
			}
			if (projectileBoostCount != 0) {
				stylePlayer.AddStyleBonus(StyleBonusesList.ProjectileBoost, tempProjectileBoostCount);
			}

			ParryEffects(Player.whoAmI, parryCount, Player.Center + knockbackVelocity * 16);
			NetMessage.SendData(MessageID.PlayerControls, number: Player.whoAmI);			
		}

		

		public static void SendParryEffectsMessage(int whoAmI, int parryCount, Vector2 parryPosition) {
			ModPacket packet = ModContent.GetInstance<NullandVoid>().GetPacket();
			packet.Write((byte)NullandVoid.MessageType.ParryEffects);
			packet.Write((byte)whoAmI);
			packet.Write(parryCount);
			packet.WriteVector2(parryPosition);
			packet.Send(ignoreClient: whoAmI);
		}
		
		public static void HandleParryEffectsMessage(BinaryReader reader, int whoAmI) {
			int player = reader.ReadByte();
			int parryCount = reader.ReadInt32();
			Vector2 parryPosition = reader.ReadVector2();
			if (Main.netMode == NetmodeID.Server) {
				player = whoAmI;
				SendParryEffectsMessage(player, parryCount, parryPosition);
			}
			Main.player[player].GetModPlayer<ParryPlayer>().ParryEffects(player, parryCount, parryPosition);
		}
	}
}