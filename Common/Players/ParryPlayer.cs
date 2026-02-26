using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using NullandVoid.Common.Globals.Items;
using NullandVoid.Common.Systems;
using NullandVoid.Content.Projectiles;
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
		public const int ParryResourceMax = 50;
		public const int ParryWindowMax = 10;
		public int ParryResource;
		public int ParryUsage;
		public int ParryRegenRate;
		public int ParryFrame;
		public List<int> ParriedProjectiles = [];
		public List<int> ParriedNPCs = [];
		public bool SwordParry;
		public int SwordParryIFrame;
		public float ParryAngle;
		private int parryRange;
		private float parryScope;
		public int ParryDirection;
		private int parryTimer;
		private int parryWindow;
		private int projectileBoostCount;
		
		
		// Reset parrying stats
		private void ResetParry() {
			ParryRegenRate = 3;
			ParryUsage = 50;
			parryRange = 50;
			parryScope = MathHelper.PiOver2;
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
					SoundEngine.PlaySound(new SoundStyle("NullandVoid/Assets/Sounds/ParryFilled") with {Volume = 0.4f * ModContent.GetInstance<NullandVoidClientConfig>().ParrySoundVolume, PitchVariance = 0.5f, Pitch = 0.3f});
				}
				parryTimer = 0;
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
						Main.LocalPlayer.GetModPlayer<StaminaPlayer>().AddStaminaResource(5 * parryCount - projectileBoostCount);
					}
				}
			}

			if (ParryFrame != 0) {
				ParryFrame--;
			}

			if (SwordParryIFrame != 0) {
				SwordParryIFrame--;
			}
		}

		public void DoParry(bool swordParry = false, int window = ParryResourceMax) {
			if (!swordParry) {
				parryWindow = ParryWindowMax;
				ParryDirection = (int)MathF.Round(Math.Clamp(Main.MouseScreen.X - Main.screenWidth / 2, -1, 1));
				if (ParryDirection == 0) {
					ParryDirection = Player.direction;
				}
				AddParryResource(-ParryUsage);
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
			if (KeybindSystem.ParryKeybind.JustPressed && ParryResource >= ParryUsage && !SwordParry) {
				DoParry();
			}
		}

		public void ParryEffects(int whoAmI, int parryCount, bool swordParry) {
			Player player = Main.player[whoAmI];

			if (parryCount == 0 && !swordParry) {
				SoundEngine.PlaySound(SoundID.DD2_MonkStaffSwing with {Volume = ModContent.GetInstance<NullandVoidClientConfig>().ParrySoundVolume, Pitch = 0.2f}, player.Center);
			}
			else if (parryCount != 0) {
				SoundEngine.PlaySound(new SoundStyle("NullandVoid/Assets/Sounds/ParryHit") with {Volume = ModContent.GetInstance<NullandVoidClientConfig>().ParrySoundVolume, PitchVariance = 0.2f, Pitch = (parryCount - 1) * 0.1f}, player.Center);
				
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
				NetHandler.SendParryMessage(Player.whoAmI, parryCount, swordParry);
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
			
			foreach (Projectile projectile in Main.ActiveProjectiles) {
				if (
					(((projectile.DamageType == DamageClass.Ranged || projectile.DamageType == DamageClass.Magic) && projectile.owner == Player.whoAmI) || projectile.hostile) &&
					!ParriedProjectiles.Contains(projectile.whoAmI) &&
					(projectile.Center.X - Player.Center.X) * Player.direction > 0 &&
					projectile.Center.DistanceSQ(Player.Center) <= parryRange * parryRange &&
					projectile.Hitbox.IntersectsConeFastInaccurate(Player.Center, parryRange, ParryAngle, modifiedScope)
				    ) {
					parryingProjectiles.Add(projectile.whoAmI);
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
				
			foreach (int i in parryingProjectiles) {
				ParriedProjectiles.Add(i);
				Projectile projectile =  Main.projectile[i];
				if (projectile.hostile) {
					projectile.hostile = false;
					projectile.friendly = true;
					projectile.velocity *= -1;
					parriedDamage += NullandVoidUtils.EstimateDamage(projectile);
				}
				else {
					projectileBoostCount++;
					tempProjectileBoostCount++;
				}

				projectile.localNPCHitCooldown = 20;
				projectile.usesLocalNPCImmunity = true;
				projectile.penetrate = projectile.penetrate == -1? -1 : projectile.penetrate + 3;
				projectile.damage *= 2;
				projectile.knockBack *= 1.5f;
				projectile.velocity *= Math.Clamp((16 / projectile.velocity.Length()), 1f, 1.75f);
				projectile.netUpdate = true;

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
				npc.SimpleStrikeNPC(SwordParry? npc.damage : npc.damage * 2, Player.direction, false, MathF.Sqrt(approachVelocity.Length() + 10) + 5, DamageClass.Melee);
				if (!npc.active) {
					Player.GetModPlayer<StylePlayer>().AddStyleBonus(StyleBonusesList.Kill);
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
			
			int parryHeal = parriedDamage / parryCount;
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
				stylePlayer.AddStyleBonus(StyleBonusesList.Parry, tempParryCount);
			}
			if (projectileBoostCount != 0) {
				stylePlayer.AddStyleBonus(StyleBonusesList.ProjectileBoost, tempProjectileBoostCount);
			}

			ParryEffects(Player.whoAmI, parryCount, SwordParry);
			NetMessage.SendData(MessageID.PlayerControls, number: Player.whoAmI);			
		}
	}
}