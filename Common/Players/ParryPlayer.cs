using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using NullandVoid.Common.Globals.Items;
using NullandVoid.Common.Systems;
using NullandVoid.Content.Projectiles;
using NullandVoid.Core;
using NullandVoid.Utils;
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
		public const int StatParryMax = 50;
		public const int ParryWindowMax = 10;
		public int StatParry;
		public int ParryCost;
		public int ParryRegen;
		public int ParryRegenCount;
		public int ParryFrame;
		public List<int> ParriedProjectiles = [];
		public List<int> ParriedNPCs = [];
		public bool SwordParry;
		public int SwordParryIFrame;
		public float ParryAngle;
		public int ParryDirection;
		public int QuickProjectileBoostWindow;
		public int ParryExplosionRange;
		private int parryRange;
		private float parryScope;
		private int parryWindow;
		private int projectileBoostCount;
		
		
		// Reset parrying stats
		private void ResetParry() {
			ParryRegen = 30;
			ParryCost = 50;
			parryRange = 50;
			parryScope = MathHelper.PiOver2;
			ParryExplosionRange = 150;
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
		

		
		public void ChangeStatParry(int addAmount) {
			StatParry = Math.Clamp(StatParry + addAmount, 0, StatParryMax);
		}
		
		public override void PostUpdateMiscEffects() {
			if (StatParry < StatParryMax) {
				ParryRegenCount += ParryRegen;
				if (ParryRegenCount > 60) {
					StatParry++;
					ParryRegenCount -= 60;
					if (StatParry >= StatParryMax) {
						StatParry = Math.Min(StatParry, StatParryMax);
						SoundEngine.PlaySound(new SoundStyle("NullandVoid/Assets/Sounds/ParryFilled") with {Volume = 0.4f * ModContent.GetInstance<NullandVoidClientConfig>().ParrySoundVolume, PitchVariance = 0.5f, Pitch = 0.3f, MaxInstances = 8 });
					}
				}
			}
			else {
				ParryRegenCount = 0;
			}

			SwordParry = Player.HeldItem.useStyle == SwordGlobalItem.SwordUseStyle;
			if (SwordParry) { 
				parryScope = MathHelper.PiOver4;
			}
			
			if (parryWindow != 0) {
				parryWindow--;
				(List<int> parryingProjectiles, List<int> parryingNPCs) = GetParried();
				int parryCount = parryingProjectiles.Count + parryingNPCs.Count;
				if (parryCount != 0) {
					ParryReflect(parryingProjectiles, parryingNPCs);
					if (parryCount - projectileBoostCount != 0) {
						Main.LocalPlayer.GetModPlayer<MovementClassPlayer>().ChangeStatStamina(5 * parryCount - projectileBoostCount);
					}
				}
			}

			if (ParryFrame != 0) {
				ParryFrame--;
			}

			if (SwordParryIFrame != 0) {
				SwordParryIFrame--;
			}

			if (QuickProjectileBoostWindow != 0) {
				QuickProjectileBoostWindow--;
			}
		}

		public void DoParry(bool swordParry = false, int window = StatParryMax) {
			if (!swordParry) {
				parryWindow = ParryWindowMax;
				ParryDirection = (int)MathF.Round(Math.Clamp(Main.MouseScreen.X - Main.screenWidth / 2, -1, 1));
				if (ParryDirection == 0) {
					ParryDirection = Player.direction;
				}
				ChangeStatParry(-ParryCost);
			}
			else {
				parryWindow = window;
				ParryDirection = Player.direction;
			}

			ParriedNPCs.Clear();
			ParriedProjectiles.Clear();
			projectileBoostCount = 0;
			ParryAngle = ((Main.MouseWorld - Player.MountedCenter) * new Vector2(1, Player.gravDir)).ToRotation();
			ParryEffects(Player.whoAmI, 0, swordParry);
		}

		public override void ProcessTriggers(TriggersSet triggersSet) {
			if (KeybindSystem.ParryKeybind.JustPressed && StatParry >= ParryCost && !SwordParry) {
				DoParry();
			}
		}

		public void ParryEffects(int whoAmI, int parryCount, bool swordParry) {
			Player player = Main.player[whoAmI];

			if (parryCount == 0 && !swordParry) {
				SoundEngine.PlaySound(SoundID.DD2_MonkStaffSwing with { Volume = ModContent.GetInstance<NullandVoidClientConfig>().ParrySoundVolume, Pitch = 0.2f }, player.Center);
			}
			else if (parryCount != 0) {
				SoundEngine.PlaySound(new SoundStyle("NullandVoid/Assets/Sounds/ParryHit") with { Volume = ModContent.GetInstance<NullandVoidClientConfig>().ParrySoundVolume, PitchVariance = 0.2f, Pitch = (parryCount - 1) * 0.1f, MaxInstances = 8 }, player.Center);
				
				for (int i = 0; i < 8; i++) {
					Dust dust = Dust.NewDustDirect(player.Center, 10, 10, DustID.Firework_Yellow, Scale: 0.6f);
					dust.noGravity = true;
				}
			}
			
			
			if (Player.whoAmI != Main.myPlayer) {
				return;
			}

			if (parryCount != 0) {
				Main.instance.CameraModifiers.Add(new PunchCameraModifier(Player.Center, (Main.rand.NextFloat() * MathHelper.Pi).ToRotationVector2(), 8f * ModContent.GetInstance<NullandVoidClientConfig>().ParryShakeIntensity, 7.5f, 15, 1000f, FullName));
			}

			if (!swordParry) {
				Projectile.NewProjectile(Player.GetSource_FromThis(), Player.MountedCenter, Vector2.Zero, ModContent.ProjectileType<ParrySwordProjectile>(), 0, ParryAngle, Main.myPlayer, ParryDirection);
			}

			if (Main.netMode != NetmodeID.SinglePlayer && (parryCount != 0 || !swordParry)) {
				NullandVoidNetwork.SendParryMessage(Player.whoAmI, parryCount, swordParry);
			}
		}
		
		public override bool ConsumableDodge(Player.HurtInfo info) {
			if (SwordParryIFrame != 0) {
				return true;
			}
			
			if (parryWindow <= 0) {
				return false;
			}


			if (info.DamageSource.SourceOtherIndex == 0) {
				ParryFrame = 20;
				ParryAngle = MathHelper.PiOver4;
				ParryDirection = Player.direction;
				ParryEffects(Player.whoAmI, 1, SwordParry);
				Player.velocity = new Vector2(Player.velocity.X * 2f, -10);
				NetMessage.SendData(MessageID.PlayerControls, number: Player.whoAmI);		
				return true;
			}
			return ParriedProjectiles.Contains(info.DamageSource.SourceProjectileLocalIndex) || ParriedNPCs.Contains(info.DamageSource.SourceNPCIndex);
		}

		public (List<int> parryingProjectiles, List<int> parryingNPCs) GetParried() {
			List<int> parryingProjectiles = [];
			List<int> parryingNPCs = [];
			int modifiedRange = SwordParry? (int)(parryRange / 1.5f) : parryRange;
			float modifiedScope = SwordParry? (parryScope / 1.5f) : parryScope;
			int quickBoostRange = (SwordParry && QuickProjectileBoostWindow != 0)? 200 : 0;
			StylePlayer stylePlayer = Player.GetModPlayer<StylePlayer>();
			
			foreach (Projectile projectile in Main.ActiveProjectiles) {
				if (ParriedProjectiles.Contains(projectile.whoAmI)) {
					continue;
				}
				if ((projectile.DamageType == DamageClass.Ranged || projectile.DamageType == DamageClass.Magic || projectile.aiStyle == ProjAIStyleID.Boomerang) && projectile.owner == Player.whoAmI) {
					if (projectile.type != ModContent.ProjectileType<ParryExplosion>() && 
						(projectile.Center.X - Player.Center.X) * Player.direction > 0 &&
					    projectile.Center.DistanceSQ(Player.Center) <= (parryRange + quickBoostRange) * (parryRange + quickBoostRange) &&
					    projectile.Hitbox.IntersectsConeFastInaccurate(Player.Center, parryRange + quickBoostRange, ParryAngle, modifiedScope)
					    ) {
						parryingProjectiles.Add(projectile.whoAmI);
						stylePlayer.CheckQuickDraw();
					}
				}
				else if (projectile.hostile) {
					if ((projectile.Center.X - Player.Center.X) * Player.direction > 0 &&
					    projectile.Center.DistanceSQ(Player.Center) <= parryRange * parryRange &&
					    projectile.Hitbox.IntersectsConeFastInaccurate(Player.Center, parryRange, ParryAngle, modifiedScope)
					    ) {
						parryingProjectiles.Add(projectile.whoAmI);
						stylePlayer.CheckQuickDraw();
					}
				}
			}
			
			foreach (NPC npc in Main.ActiveNPCs) {
				if (
					!npc.friendly &&
					npc.damage != 0 &&
					!ParriedNPCs.Contains(npc.whoAmI) &&
				    (
					    (npc.Hitbox.ClosestPointInRect(Player.Center).DistanceSQ(Player.Center) <= modifiedRange * modifiedRange &&
						npc.Hitbox.IntersectsConeFastInaccurate(Player.Center, modifiedRange, ParryAngle, parryScope * 0.1f)) ||
					    npc.Hitbox.Contains(Player.Center.ToPoint())
					    )
				    ) {
					parryingNPCs.Add(npc.whoAmI);
				}
			}

			return (parryingProjectiles, parryingNPCs);
		}

		public void ParryReflect(List<int> parryingProjectiles, List<int> parryingNPCs) {
			Vector2 knockbackVelocity = Vector2.Zero;
			int parriedDamage = 0;
			int tempProjectileBoostCount = 0;
			int specialProjectileCount = 0;
				
			foreach (int i in parryingProjectiles) {
				ParriedProjectiles.Add(i);
				Projectile projectile =  Main.projectile[i];
				bool isBoost = false;
				if (projectile.hostile) {
					projectile.hostile = false;
					projectile.friendly = true;
					projectile.velocity *= -1;
					parriedDamage += NullandVoidUtils.EstimateDamage(projectile);
				}
				else {
					projectileBoostCount++;
					tempProjectileBoostCount++;
					isBoost = true;
					if (projectile.aiStyle == ProjAIStyleID.Boomerang) {
						Player.GetModPlayer<StylePlayer>().AddStyleBonus(StyleBonus.Aerodynamic);
						specialProjectileCount++;
						knockbackVelocity = projectile.velocity.SafeNormalize(Vector2.Zero);
						NPC closetNPC = NullandVoidUtils.FindClosestNPC(1000, projectile.Center);
						if (closetNPC != null) {
							Projectile.NewProjectile(projectile.GetSource_FromThis(), closetNPC.Center - new Vector2(20, 0) * closetNPC.direction, Vector2.Zero, ModContent.ProjectileType<SpecialParryProjectile>(), 0, 0, 0, projectile.identity, (int)SpecialParryAIStyle.Boomerang, Player.whoAmI);
						}
						else {
							projectile.velocity = (projectile.Center - Player.Center).SafeNormalize(Vector2.One) * new Vector2(12, 12);
						}
						continue;
					}
				}
				
				projectile.knockBack *= 1.5f;
				if (Math.Abs(projectile.Center.X - Player.Center.X / 8) < 3) {
					projectile.velocity *= Math.Clamp((16 / projectile.velocity.Length()), 1f, 1.75f);
				}
				else {
					projectile.velocity.X += Math.Clamp((projectile.Center.X - Player.Center.X) / 8, -3, 3);
				}

				if (Math.Abs(projectile.velocity.Length()) < Math.Abs(projectile.Center.Y - Player.Center.Y - projectile.velocity.Y)) {
					projectile.velocity.Y = MathHelper.Clamp(projectile.Center.Y - Player.Center.Y, -5, 5);
				}
				projectile.netUpdate = true;
				Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.position, Vector2.Zero, ModContent.ProjectileType<ParryExplosion>(), projectile.damage, projectile.knockBack, 0, projectile.identity,  ParryExplosionRange, isBoost? -Player.whoAmI : Player.whoAmI);

				knockbackVelocity = projectile.velocity.SafeNormalize(Vector2.Zero);
				Player.velocity.X -= knockbackVelocity.X * 5 / ParriedProjectiles.Count;
				Player.velocity.Y = -knockbackVelocity.Y * 8 * MathF.Sqrt(1 + ParriedProjectiles.Count * 0.4f);
				Player.controlJump = false;
			}

			foreach (int i in parryingNPCs) {
				ParriedNPCs.Add(i);
				NPC npc = Main.npc[i];
				Vector2 approachVelocity = Player.velocity - npc.velocity;
				npc.PlayerInteraction(Player.whoAmI);
				npc.SimpleStrikeNPC(SwordParry? npc.damage / 2 : npc.damage, Player.direction, false, MathF.Sqrt(approachVelocity.Length() + 10) + 5, DamageClass.Melee);
				if (!npc.active) {
					Player.GetModPlayer<StylePlayer>().AddStyleBonus(StyleBonus.Kill);
				}
				
				parriedDamage += NullandVoidUtils.EstimateDamage(npc);
				knockbackVelocity = approachVelocity.SafeNormalize(Vector2.Zero);
				Player.velocity -= knockbackVelocity * approachVelocity.Length() * (1 - npc.knockBackResist);
				
				if (npc.boss) {
					SwordParryIFrame = 20;
				}
			}

			ParryAngle = MathF.Atan(knockbackVelocity.Y / knockbackVelocity.X);
			if (ParryDirection == -1) {
				ParryAngle -= MathHelper.Pi;
			}
			ParryFrame = 20;
			int parryCount = ParriedProjectiles.Count + ParriedNPCs.Count;
			
			int parryHeal = (int)((parriedDamage / parryCount) * (0.5f + ((float)Player.GetModPlayer<StylePlayer>().PlayerStyleRank.Rank / 8)));
			if (SwordParry) {
				parryHeal /= 2;
			}
			if (parryHeal != 0) {
				Player.Heal(parryHeal);
			}
			
			Player.fallStart = Player.position.ToTileCoordinates().Y;
			
			int tempParryCount = parryingProjectiles.Count + parryingNPCs.Count - tempProjectileBoostCount;
			StylePlayer stylePlayer = Player.GetModPlayer<StylePlayer>();
			if (tempParryCount != 0) {
				stylePlayer.AddStyleBonus(StyleBonus.Parry, tempParryCount);
			}
			if (projectileBoostCount - specialProjectileCount > 0) {
				stylePlayer.AddStyleBonus(StyleBonus.ProjectileBoost, tempProjectileBoostCount - specialProjectileCount);
			}

			ParryEffects(Player.whoAmI, parryCount, SwordParry);
			NetMessage.SendData(MessageID.PlayerControls, number: Player.whoAmI);			
		}
	}
}