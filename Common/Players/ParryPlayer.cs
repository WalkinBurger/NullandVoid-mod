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

		// Parrying freeze frame texture
		public Texture2D FreezeImage;

		// Parrying configs
		private int parryEffectTimer = -1;
		public int ParryIFrame;
		public float ParryRegenRate;
		public int ParryResource;
		private int parryTimer;
		public int ParryUsage;
		public int ParryWindow;


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
					SoundEngine.PlaySound(SoundID.MaxMana);
				}

				parryTimer = 0;
			}

			ParryResource = Utils.Clamp(ParryResource, 0, ParryResourceMax);

			// Post-parrying client sided effects
			if (parryEffectTimer > 0) {
				parryEffectTimer--;
			}
			else if (parryEffectTimer == 0) {
				ModContent.GetInstance<ParryFlashSystem>().Hide();
				parryEffectTimer = -1;

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
				ParryEffects();
				ParryDamage(info);
				return true;
			}

			return false;
		}

		// Visual & sound effects for a successful parry
		private void ParryEffects() {
			float parrySoundVolume = ModContent.GetInstance<NullandVoidClientConfig>().ParryingSoundVolume;
			if (parrySoundVolume != 0) {
				SoundEngine.PlaySound(SoundID.Research with { Volume = 0.7f * parrySoundVolume });
				SoundEngine.PlaySound(SoundID.Item52 with { Volume = 3f * parrySoundVolume, Pitch = 1.3f });
				SoundEngine.PlaySound(SoundID.CoinPickup with { Volume = 3f * parrySoundVolume });
			}

			if (Player.whoAmI != Main.myPlayer) {
				return;
			}

			// Client sided effects
			Player.SetImmuneTimeForAllTypes(ParryIFrame);
			parryEffectTimer = ModContent.GetInstance<NullandVoidClientConfig>().ParryingFrameFreezing;
			if (parryEffectTimer != 0) {
				int freezeWidth = Main.instance.GraphicsDevice.PresentationParameters.BackBufferWidth;
				int freezeHeight = Main.instance.GraphicsDevice.PresentationParameters.BackBufferHeight;
				Color[] freezeColors = new Color[freezeWidth * freezeHeight];
				Main.instance.GraphicsDevice.GetBackBufferData(freezeColors);
				float parryFlashIntensity =
					ModContent.GetInstance<NullandVoidClientConfig>().ParryingFlashIntensity;
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
				SendParryDodgeMessage(Player.whoAmI);
			}
		}

		// Parry the damage
		private void ParryDamage(Player.HurtInfo info) {
			Vector2 selfKnockBack = new();
			int damageReflected = 0;
			float parryPercision;

			// NPC contact damage
			if (info.DamageSource.SourceNPCIndex != -1) {
				NPC parriedNPC = Main.npc[info.DamageSource.SourceNPCIndex];
				Vector2 appoarchVelocity = Player.velocity - parriedNPC.velocity;
				parryPercision = (float)Math.Sqrt(Math.Pow(appoarchVelocity.X, 2) + Math.Pow(appoarchVelocity.Y, 2) * 2);
				damageReflected = (int)(info.Damage * (0.5 + parryPercision / 8));
				Main.NewText(("Approch velocity: ", parryPercision, "Damage reflected/healed: ", damageReflected));
				parriedNPC.SimpleStrikeNPC(damageReflected, -info.HitDirection, false, 4 + parryPercision / 2);
				selfKnockBack = appoarchVelocity * (1f - parriedNPC.knockBackResist);
			}
			// Projectiles damage
			else if (info.DamageSource.SourceProjectileLocalIndex != -1) {
				Projectile parriedProjectile = Main.projectile[info.DamageSource.SourceProjectileLocalIndex];
				parryPercision = GetParryBuffRemainingTime();
				(selfKnockBack, damageReflected) = ReflectProjectile(parriedProjectile, (parryPercision / 2) + 1);
				
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
				Player.Heal(damageReflected);
			}
			if (selfKnockBack != Vector2.Zero) {
				Player.velocity -= selfKnockBack;
			}
		}
		
		// Reflects parried projectile
		private (Vector2 selfKnockBack, int damageReflected) ReflectProjectile(Projectile parriedProjectile, float parryPercision) {
			if (!parriedProjectile.friendly) {
				parriedProjectile.friendly = true;
				parriedProjectile.hostile = false;
				Vector2 newVelocity = parriedProjectile.oldPosition - parriedProjectile.position;
				newVelocity.Normalize();
				parriedProjectile.velocity = newVelocity * parriedProjectile.velocity.Length() * (float)Math.Sqrt(parryPercision);
			}
			else {
				parriedProjectile.velocity *= parryPercision;
			}
			Main.NewText(parryPercision);
			parriedProjectile.damage = (int)(parryPercision * 2 * parriedProjectile.damage);
			
			Vector2 selfKnockBack = -0.1f * parriedProjectile.oldVelocity * parryPercision;
			int damageReflected = (int)(parriedProjectile.oldVelocity.Length() * parryPercision);
			
			return (selfKnockBack, damageReflected);
		}
		
		// Parry your own projectiles--projectile boosting
		
		
		private int GetParryBuffRemainingTime() {
			for (int i = 0; i < Player.buffType.Length; i++) {
				if (Player.buffType[i] == ModContent.BuffType<ParryBuff>()) {
					return Player.buffTime[i];
				}
			}
			return 0;
		}

		// Packets sending and handling methods
		private static void SendParryDodgeMessage(int whoAmI) {
			ModPacket packet = ModContent.GetInstance<NullandVoid>().GetPacket();
			packet.Write((byte)NullandVoid.MessageType.ParryDodge);
			packet.Write((byte)whoAmI);
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
			if (Main.netMode == NetmodeID.Server) {
				player = whoAmI;
				SendParryDodgeMessage(player);
			}
			Main.player[player].GetModPlayer<ParryPlayer>().ParryEffects();
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