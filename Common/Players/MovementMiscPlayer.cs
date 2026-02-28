using System;
using System.IO;
using Microsoft.Xna.Framework;
using NullandVoid.Content.Projectiles;
using NullandVoid.Core;
using Terraria;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace NullandVoid.Common.Players
{
	public enum PogoTypes {
		Sword,
		Ranged,
	}
	
	public class MovementMiscPlayer : ModPlayer
	{
		public bool Grounded;
		private bool canNextPogo;
		private int[] pogoCounts = new int[Enum.GetNames(typeof(PogoTypes)).Length];
		private int pogoCoolDown;
		private bool pogoing;

		public bool MaintainVelocity;


		public override void ResetEffects() {
			if (Main.mouseLeftRelease) {
				canNextPogo = true;
			}
		}
		
		public override void PostUpdateMiscEffects() {
			if (pogoCoolDown != 0) {
				pogoCoolDown--;
			}

			Grounded = ((Player.velocity.Y == 0f || Player.sliding) && Player.releaseJump) || (Player.autoJump && Player.justJumped);
		}
		
		public override void ProcessTriggers(TriggersSet triggersSet) {
			if (triggersSet.DirectionsRaw.X * Player.velocity.X > 0 && Player.velocity.Y != 0) {
				if (Main.netMode != NetmodeID.SinglePlayer && !MaintainVelocity) {
					NullandVoidNetwork.SendMaintainVelMessage(Player.whoAmI, true);
				}
				MaintainVelocity = true;
			}
			else {
				if (Main.netMode != NetmodeID.SinglePlayer && MaintainVelocity) {
					NullandVoidNetwork.SendMaintainVelMessage(Player.whoAmI, false);
				}
				MaintainVelocity = false;

				if (Player.velocity.Y != 0) {
					return;
				}

				if (Grounded && pogoing) {
					pogoing = false;
					Array.Clear(pogoCounts);
					pogoCoolDown = 30;
				}
				Player.runSlowdown = 0.2f + Math.Max(0, Math.Abs(Player.velocity.X) - 6) * 0.2f;
			}
		}

		public override void PostUpdateRunSpeeds() {
			if (MaintainVelocity) {
				Player.runSlowdown = 0.03f;
			}
		}


		public bool CanPogo(PogoTypes pogoType) {
			Vector2 aimPosition = (Main.MouseScreen - new Vector2 (Main.screenWidth / 2, Main.screenHeight / 2));
			if (!canNextPogo || pogoCounts[(int)pogoType] > 4 || pogoCoolDown != 0 || Math.Abs(aimPosition.X) > 35f || aimPosition.Y > 70f || aimPosition.Y < 0 ) {
				return false;
			}

			if (Main.mouseLeft) {
				canNextPogo = false;
			}
			
			pogoCounts[(int)pogoType]++;
			
			switch (pogoType) {
				case PogoTypes.Sword:
					for (int i = 0; i < 3; i++) {
						if (WorldGen.SolidTile2(Framing.GetTileSafely(Player.Bottom.ToTileCoordinates() + new Point(0, i)))) {
							return true;
						}
					}
					return false;
				case PogoTypes.Ranged:
					return Player.velocity.Y is >= 0 and < 0.5f;
				default:
					return true;
			}
		}
		
		public void Pogo(int boostVelocity, int count = -1) {
			if (count == -1) {
				count = pogoCounts[(int)PogoTypes.Sword];
			}
			Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Bottom, Vector2.Zero, ModContent.ProjectileType<GlowStarProjectile>(), 0, 0, Main.myPlayer, 15f);
			Player.GetModPlayer<StylePlayer>().AddStyleBonus(StyleBonusesList.Pogo);
			pogoing = true;
			Player.velocity.Y = -boostVelocity;
			Player.fallStart = Player.position.ToTileCoordinates().Y;

			
			if (Main.netMode != NetmodeID.SinglePlayer) {
				NullandVoidNetwork.SendSoundMessage(Player.whoAmI, NullandVoidNetwork.Sounds.Pogo, count);
				NetMessage.SendData(MessageID.PlayerControls, number: Player.whoAmI);
			}
			else {
				SoundEngine.PlaySound(SoundID.DrumClosedHiHat with { Pitch = count == 5? -1f : 0, PitchVariance = 0.2f });
			}
		}

		public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone) {
			if (!Grounded) {
				Player.velocity.Y = -6;
				NetMessage.SendData(MessageID.PlayerControls, number: Player.whoAmI);
			}
		}
	}
}