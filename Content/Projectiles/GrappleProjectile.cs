using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NullandVoid.Common.Players;
using NullandVoid.Core;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace NullandVoid.Content.Projectiles
{
	public abstract class GrappleProjectile : ModProjectile
	{
		public static Asset<Texture2D> ChainTexture { get; set; }
		public abstract string TextureName { get; }
		public Verlet GrappleVerlet { get; set; }

		public override void SetDefaults() {
			Projectile.width = Projectile.height = 12;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
		}

		public override bool PreDraw(ref Color lightColor) {
			Vector2 textureCenter = new(ChainTexture.Width() / 2, ChainTexture.Height() / 2);
			for (int i = 0; i < GrappleVerlet.Vertices.Length - 1; i++) {
				Main.EntitySpriteDraw(
					ChainTexture.Value,
					(GrappleVerlet.Vertices[i].Position + GrappleVerlet.Vertices[i + 1].Position) / 2 - Main.screenPosition,
					null,
					Lighting.GetColor(GrappleVerlet.Vertices[i].Position.ToTileCoordinates()),
					GrappleVerlet.Vertices[i].Position.AngleTo(GrappleVerlet.Vertices[i + 1].Position) + MathHelper.PiOver2,
					textureCenter,
					1,
					SpriteEffects.None
				);
			}
			return true;
		}

		public override void AI() {
			Player player = Main.player[Projectile.owner];
			MovementClassPlayer movementClassPlayer = player.GetModPlayer<MovementClassPlayer>();
			NPC? npc = null;
			
			if (Projectile.ai[1] == 0) {
				Projectile.ai[1] = 1;
				float distance = player.Center.Distance(Projectile.Center);
				movementClassPlayer.GrappledLength = distance;
				GrappleVerlet = new Verlet(Math.Max(3, (int)(Math.Ceiling(distance / 16))), player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, player.compositeFrontArm.rotation), Projectile.Center);
				ChainTexture = ModContent.Request<Texture2D>($"NullandVoid/Content/Projectiles/{TextureName}");
			}

			if (Projectile.ai[0] != 0) {
				npc = Main.npc[(int)Projectile.ai[0]];
				Projectile.position += npc.position - npc.oldPosition;
			}
			
			Projectile.rotation = player.Center.AngleTo(Projectile.Center);
			movementClassPlayer.GrappledPosition = Projectile.Center;
			Projectile.timeLeft = 60;
			
			if (!movementClassPlayer.CanCancel && !movementClassPlayer.CanCancelAlt) {
				if (npc != null) {
					npc.velocity /= 2;
				}
				Projectile.Kill();
				return;
			}

			if (Main.dedServ) {
				return;
			}
			
			GrappleVerlet.Update(player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, player.compositeFrontArm.rotation), Projectile.Center);

			if (!movementClassPlayer.UsingAltAbility) {
				return;
			}

			Vector2 direction = movementClassPlayer.StatMovement.PullSpeed.Get() * (Projectile.Center - player.Center).SafeNormalize(Vector2.Zero);
			float accel = 1 - (1 / Projectile.ai[1]);
			if (npc != null) {
				player.velocity = (1 - npc.knockBackResist) * direction * accel;
				npc.velocity = npc.knockBackResist * -direction * accel;
				Projectile.ai[1] += 1;
			}
			else {
				player.velocity = movementClassPlayer.StatMovement.ReelSpeed.Get() * (Projectile.Center - player.Center).SafeNormalize(Vector2.Zero) * accel;
				Projectile.ai[1] += 0.5f;
			}

		}
	}
}