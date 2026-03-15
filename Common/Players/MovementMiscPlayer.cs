using System;
using Microsoft.Xna.Framework;
using NullandVoid.Content.Projectiles;
using NullandVoid.Core;
using Terraria;
using Terraria.Audio;
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


		public override void ResetEffects() {
			if (Main.mouseLeftRelease) {
				canNextPogo = true;
			}
		}
		
		public override void PostUpdateMiscEffects() {
			Grounded = Player.velocity.Y == 0f;
			
			if (Grounded && pogoing) {
				pogoing = false;
				Array.Clear(pogoCounts);
				pogoCoolDown = 30;
			} 
			else if (pogoCoolDown != 0) {
				pogoCoolDown--;
			}
		}
		
		public override void Load() {
			On_Player.HorizontalMovement += On_PlayerOnHorizontalMovement;
		}

		public override void Unload() {
			On_Player.HorizontalMovement -= On_PlayerOnHorizontalMovement;
		}

		private static void On_PlayerOnHorizontalMovement(On_Player.orig_HorizontalMovement orig, Player self) {
			if (((self.controlLeft && self.velocity.X < 0) || (self.controlRight && self.velocity.X > 0)) && !self.GetModPlayer<MovementMiscPlayer>().Grounded) {
				self.runSlowdown = 0;
			}
			else if (self.GetModPlayer<MovementMiscPlayer>().Grounded && Math.Abs(self.velocity.X) > self.accRunSpeed + 1) {
				self.runSlowdown = Math.Abs(self.velocity.X / 6);
			}
			orig(self);
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
			Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Bottom, Vector2.Zero, ModContent.ProjectileType<GlowStarProjectile>(), 0, 0, Main.myPlayer, 10f);
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