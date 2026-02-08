using System;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NullandVoid.Common.Systems;
using NullandVoid.Common.UIs;
using NullandVoid.Content.Buffs;
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
		public int ParryResource;
		public int ParryUsage;
		private int parryTimer;
		public int ParryWindow;
		public int ParryIFrame;
		public float ParryRegenRate;

		// Parrying freeze frame texture
		public Texture2D FreezeImage;

		// Parrying others
		private int parryEffectTimer = -1;
		private Vector2 knockBackVelocity;
		private Vector2 totalKnockBack = Vector2.One;
		
		// Reset parrying stats
		private void ResetParry() {
			ParryRegenRate = 10f;
			ParryUsage = 50;
			ParryWindow = 10;
			ParryIFrame = 30;
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

		// Updates parry cooldown, post-parry visual & sound effects client sided
		public override void PostUpdateMiscEffects() {
			// Parry resource regen
			parryTimer++;
			if (parryTimer >= 10 / ParryRegenRate && ParryResource < ParryResourceMax) {
				ParryResource++;
				if (ParryResource == ParryResourceMax) {
					SoundEngine.PlaySound(new SoundStyle("NullandVoid/Assets/Sounds/ParryFilled") with {Volume = 0.5f * ModContent.GetInstance<NullandVoidClientConfig>().ParryingSoundVolume, PitchVariance = 0.5f, Pitch = 0.75f});
				}
				parryTimer = 0;
			}
			ParryResource = Utils.Clamp(ParryResource, 0, ParryResourceMax);
			
			if (Player.HasBuff(ModContent.BuffType<ParryBuff>())) {
				GetSurroundingProjectiles();
			}

			// Post-parrying client sided effects
			if (parryEffectTimer > 0) {
				parryEffectTimer--;
			}
			else if (parryEffectTimer == 0) {
				ModContent.GetInstance<ParryFlashSystem>().Hide();
				parryEffectTimer = -1;
				totalKnockBack = Vector2.One;
				
				float parrySoundVolume = ModContent.GetInstance<NullandVoidClientConfig>().ParryingSoundVolume;
				if (parrySoundVolume != 0) {
					SoundEngine.PlaySound(SoundID.NPCHit16 with { Volume = 1f * parrySoundVolume });
					SoundEngine.PlaySound(SoundID.Shatter with { Volume = 0.3f * parrySoundVolume });
				}

				float parryShakeIntensity = ModContent.GetInstance<NullandVoidClientConfig>().ParryingShakeIntensity;
				if (parryShakeIntensity != 0f) {
					Main.instance.CameraModifiers.Add(new PunchCameraModifier(Player.Center,
						(Main.rand.NextFloat() * (float)Math.PI).ToRotationVector2(), 6.5f * parryShakeIntensity, 10f,
						20, 1000f, FullName));
				}
				ParryVisual(Player.position, knockBackVelocity);
			}
		}

		public void AddParryResource(int addAmount) {
			ParryResource = Math.Clamp(ParryResource + addAmount, 0, ParryResourceMax);
		}

		public override void ProcessTriggers(TriggersSet triggersSet) {
			if (KeybindSystem.ParryKeybind.JustPressed && ParryResource >= ParryUsage) {
				AddParryResource(-ParryUsage);
				Player.AddBuff(ModContent.BuffType<ParryBuff>(), ParryWindow, false);
			}
		}

		// A dodge with parry buff indicates a successful parry
		public override bool ConsumableDodge(Player.HurtInfo info) {
			if (Player.HasBuff(ModContent.BuffType<ParryBuff>())) {
				(float parryPercision, Vector2 selfKnockBack) = ParryDamage(info);
				ParryEffects(Player.whoAmI, parryPercision, selfKnockBack);
				Main.LocalPlayer.GetModPlayer<StaminaPlayer>().AddStaminaResource((int)Math.Sqrt(parryPercision));
				return true;
			}

			return false;
		}

		private void ParryVisual(Vector2 parryPos, Vector2 knockBackVector) {
			Projectile parryVisual = Projectile.NewProjectileDirect(Player.GetSource_FromThis(), parryPos, knockBackVector, ProjectileID.FinalFractal, 0, 0);
			parryVisual.timeLeft = 15;
			parryVisual.light = 1f;
			parryVisual.soundDelay = 16;

			for (int i = 0; i < 10; i++) {
				Dust.NewDustDirect(parryPos, 10, 100, DustID.Firefly, Scale: 5f);
			}
		}

		// Visual & sound effects for a successful parry
		private void ParryEffects(int whoAmI, float parryPercision, Vector2 knockBackVector) {
			float parrySoundVolume = ModContent.GetInstance<NullandVoidClientConfig>().ParryingSoundVolume;
			Player parryPlayer = Main.player[whoAmI];
			Vector2 parryPos = parryPlayer.Center;
			
			if (parrySoundVolume != 0) {
				SoundEngine.PlaySound(SoundID.Research with { Volume = 0.3f + 0.05f * parrySoundVolume * parryPercision}, parryPos);
				SoundEngine.PlaySound(SoundID.Item52 with { Volume = 3f * parrySoundVolume, Pitch = 0.9f + 0.05f * parryPercision}, parryPos);
				SoundEngine.PlaySound(SoundID.CoinPickup with { Volume = 3f * parrySoundVolume}, parryPos);
			}
			

			
			if (Player.whoAmI != Main.myPlayer) {
				return;
			}

			Player.SetImmuneTimeForAllTypes(ParryIFrame);

			// Client sided effects
			if (parryEffectTimer > -1) {
				// Don't apply effects if already in effect
				return;
			}

			knockBackVelocity = knockBackVector;
			
			parryEffectTimer = ModContent.GetInstance<NullandVoidClientConfig>().ParryingFrameFreezing;
			if (parryEffectTimer != 0) {
				int freezeWidth = Main.instance.GraphicsDevice.PresentationParameters.BackBufferWidth;
				int freezeHeight = Main.instance.GraphicsDevice.PresentationParameters.BackBufferHeight;
				Color[] freezeColors = new Color[freezeWidth * freezeHeight];
				Main.instance.GraphicsDevice.GetBackBufferData(freezeColors);
				float parryFlashIntensity = ModContent.GetInstance<NullandVoidClientConfig>().ParryingFlashIntensity;
				if (parryFlashIntensity != 0f) {
					for (int i = 0; i < freezeColors.Length; i++) {
						freezeColors[i].R = (byte)Utils.Clamp(freezeColors[i].R + (int)(255 * parryFlashIntensity),
							0, 255);
						freezeColors[i].G = (byte)Utils.Clamp(freezeColors[i].G + (int)(255 * parryFlashIntensity),
							0, 255);
						freezeColors[i].B = (byte)Utils.Clamp(freezeColors[i].B + (int)(255 * parryFlashIntensity),
							0, 255);
					}
				}

				FreezeImage = new Texture2D(Main.instance.GraphicsDevice, freezeWidth, freezeHeight);
				FreezeImage.SetData(freezeColors);

				ModContent.GetInstance<ParryFlashSystem>().Show();
			}

			if (Main.netMode != NetmodeID.SinglePlayer) {
				SendParryDodgeMessage(Player.whoAmI, parryPercision, knockBackVector);
			}
		}

		// Parry the damage
		private (float parryPercision, Vector2 selfKnockBack) ParryDamage(Player.HurtInfo info) {
			Vector2 selfKnockBack = new();
			int damageReflected = 0;
			float parryPercision = 0;

			// NPC contact damage
			if (info.DamageSource.SourceNPCIndex != -1) {
				NPC parriedNPC = Main.npc[info.DamageSource.SourceNPCIndex];
				Vector2 appoarchVelocity = Player.velocity - parriedNPC.velocity;
				parryPercision = (float)Math.Sqrt(Math.Pow(appoarchVelocity.X, 2) + Math.Pow(appoarchVelocity.Y, 2) * 2);
				damageReflected = (int)(info.Damage * (0.5 + parryPercision / 10));
				Main.NewText(("Approch velocity: ", parryPercision, "Damage reflected/healed: ", damageReflected));
				parriedNPC.SimpleStrikeNPC(damageReflected, -info.HitDirection, false, 4 + parryPercision / 2);
				selfKnockBack = appoarchVelocity;
				selfKnockBack.Normalize();
				selfKnockBack *= (1f - parriedNPC.knockBackResist) * (float)Math.Sqrt(appoarchVelocity.Length()) * 4;
			}
			// Projectiles damage
			else if (info.DamageSource.SourceProjectileLocalIndex != -1) {
				Projectile parriedProjectile = Main.projectile[info.DamageSource.SourceProjectileLocalIndex];
				parryPercision = ((float)GetParryBuffRemainingTime() / 2) + 1;
				(selfKnockBack, damageReflected) = ReflectProjectile(parriedProjectile, parryPercision);
				
				// Sync parried projectiles
				if (Main.netMode != NetmodeID.SinglePlayer) {
					SendParryProjectileMessage(Player.whoAmI, Main.projectile[info.DamageSource.SourceProjectileLocalIndex].identity, parryPercision);
				}
			}
			// Fall damage
			else if (info.DamageSource.SourceOtherIndex == 0) {
				selfKnockBack = new Vector2(Player.direction * -6, 12f);
			}
			
			if (damageReflected != 0) {
				Player.Heal(damageReflected / 4);
			}
			if (selfKnockBack != Vector2.Zero) {
				Player.velocity -= selfKnockBack / (Utils.Clamp(Math.Abs(Player.velocity.Length()) * 0.5f, 1f, 10f));
				NetMessage.SendData(MessageID.PlayerControls, number: Player.whoAmI);
			}

			return (parryPercision, selfKnockBack);
		}
		
		// Reflects parried projectile
		private (Vector2 selfKnockBack, int damageReflected) ReflectProjectile(Projectile parriedProjectile, float parryPercision) {
			Vector2 selfKnockBack = parriedProjectile.velocity;
			if (selfKnockBack != Vector2.Zero)
			{
				selfKnockBack.Normalize();
				selfKnockBack *= -1.5f * ((float)Math.Sqrt(parryPercision + 10) - 1);
			}
			
			if (parriedProjectile.hostile) {
				parriedProjectile.friendly = true;
				parriedProjectile.hostile = false;
				Vector2 newVelocity = parriedProjectile.oldPosition - parriedProjectile.position;
				newVelocity.Normalize();
				parriedProjectile.velocity = newVelocity * parriedProjectile.velocity.Length() * (float)Math.Sqrt(parryPercision);
				parriedProjectile.damage = (int)(Math.Sqrt(parryPercision) * 2 * parriedProjectile.damage);
			}
			else {
				parriedProjectile.velocity *= Utils.Clamp((float)Math.Sqrt(parryPercision) / (parriedProjectile.velocity.Length() * 0.1f + 0.5f), 1f, 5f);
				parriedProjectile.damage = (int)(Math.Sqrt(parryPercision) * 2 * parriedProjectile.damage);
				selfKnockBack *= -1.5f;
			}

			parriedProjectile.knockBack *= 2;
			parriedProjectile.reflected = true;
			Main.NewText(parryPercision);
			
			int damageReflected = (int)(parriedProjectile.oldVelocity.Length() * Math.Sqrt(parryPercision) * Math.Sqrt(parriedProjectile.damage) * 0.5f);
			
			return (selfKnockBack, damageReflected);
		}
		
		// Parry surrounding projectiles as well
		private void GetSurroundingProjectiles() {
			bool multiplayerClient = Main.netMode == NetmodeID.MultiplayerClient;
			
			foreach (Projectile projectile in Main.ActiveProjectiles) {
				float distance = Vector2.Distance(Player.Center, projectile.position);
				bool ownProjectile = projectile.friendly;
				if (projectile.reflected || projectile.damage == 0 || distance > 80 || projectile.minion || projectile.DamageType == DamageClass.Melee) {
					continue;
				}
				if (multiplayerClient && projectile.owner != Player.whoAmI && projectile.owner != 255) {
					continue;
				}

				float parryPercision = Utils.Clamp(GetParryBuffRemainingTime() - distance / 8, 1f, 10f);
				Main.NewText(parryPercision);
				(Vector2 selfKnockBack, int damageReflected) = ReflectProjectile(projectile, parryPercision);
				ParryEffects(Player.whoAmI, parryPercision, selfKnockBack);
				if (damageReflected != 0 && !ownProjectile) {
					Player.Heal(damageReflected / 4);
				}
				if (selfKnockBack != Vector2.Zero) {
					Player.velocity -= selfKnockBack / (totalKnockBack * 0.75f);
					totalKnockBack +=  selfKnockBack;
					NetMessage.SendData(MessageID.PlayerControls, number: Player.whoAmI);
				}

				if (Main.netMode != NetmodeID.SinglePlayer) {
					SendParryProjectileMessage(Player.whoAmI, projectile.identity, parryPercision);
				}
			}
		}

		private int GetParryBuffRemainingTime() {
			for (int i = 0; i < Player.buffType.Length; i++) {
				if (Player.buffType[i] == ModContent.BuffType<ParryBuff>()) {
					return Player.buffTime[i];
				}
			}
			return 0;
		}

		// Packets sending and handling methods
		private static void SendParryDodgeMessage(int whoAmI, float parryPercision, Vector2 knockBackVector) {
			ModPacket packet = ModContent.GetInstance<NullandVoid>().GetPacket();
			packet.Write((byte)NullandVoid.MessageType.ParryDodge);
			packet.Write((byte)whoAmI);
			packet.Write(parryPercision);
			packet.WriteVector2(knockBackVector);
			packet.Send(ignoreClient: whoAmI);
		}

		private static void SendParryProjectileMessage(int whoAmI, int projectileID, float parryPercision) {
			ModPacket packet = ModContent.GetInstance<NullandVoid>().GetPacket();
			packet.Write((byte)NullandVoid.MessageType.ParryProjectile);
			packet.Write((byte)whoAmI);
			packet.Write(projectileID);
			packet.Write(parryPercision);
			packet.Send(ignoreClient: whoAmI);
		}
		
		public static void HandleParryDodgeMessage(BinaryReader reader, int whoAmI) {
			int player = reader.ReadByte();
			float parryPercision = reader.ReadSingle();
			Vector2 knockBackVector = reader.ReadVector2();
			if (Main.netMode == NetmodeID.Server) {
				player = whoAmI;
				SendParryDodgeMessage(player,  parryPercision, knockBackVector);
			}
			Main.player[player].GetModPlayer<ParryPlayer>().ParryEffects(player, parryPercision, knockBackVector);
		}
		
		public static void HandleParryProjectileMessage(BinaryReader reader, int whoAmI) {
			int player = reader.ReadByte();
			int projectileID = reader.ReadInt32();
			float parryPercision = reader.ReadSingle();
			if (Main.netMode == NetmodeID.Server) {
				player = whoAmI;
				SendParryProjectileMessage(player, projectileID,  parryPercision);
			}
			Main.player[player].GetModPlayer<ParryPlayer>().ReflectProjectile(Main.projectile.FirstOrDefault(x => x.identity == projectileID), parryPercision);
		}
	}
}