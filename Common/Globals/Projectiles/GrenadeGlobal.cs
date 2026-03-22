using System;
using Microsoft.Xna.Framework;
using NullandVoid.Common.Players;
using NullandVoid.Content.Projectiles;
using NullandVoid.Core;
using Terraria;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;

namespace NullandVoid.Common.Globals.Projectiles
{
	public class GrenadeGlobal : GlobalProjectile
	{
		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) {
			return entity.type is ProjectileID.Grenade or ProjectileID.Beenade or ProjectileID.BouncyGrenade or ProjectileID.PartyGirlGrenade;
		}

		public override void AI(Projectile projectile) {
			if (projectile.timeLeft <= 2) {
				return;
			}
			foreach (Projectile proj in Main.ActiveProjectiles) {
				if (proj.aiStyle != ProjAIStyleID.Arrow || !(proj.Center.DistanceSQ(projectile.Center) < 1000 - proj.velocity.X * proj.velocity.Y)) {
					continue;
				}
				projectile.timeLeft = 2;
				projectile.damage *= 2;
				Vector2 normal = (projectile.Center - proj.Center).SafeNormalize(Vector2.One);
				proj.Kill();
				Projectile.NewProjectile(projectile.GetSource_FromThis(), proj.Center, Vector2.Zero, ModContent.ProjectileType<SimpleExplosion>(), projectile.damage, projectile.knockBack, -1, 400);
				
				if (Main.dedServ) {
					return;
				}

				Main.instance.CameraModifiers.Add(new PunchCameraModifier(Main.LocalPlayer.Center, (Main.rand.NextFloat() * MathHelper.Pi).ToRotationVector2(), 8f * ModContent.GetInstance<NullandVoidClientConfig>().CameraShakeIntensity, 8, 20, 1000f, FullName));
				for (int i = 0; i < 20; i++) {
					Dust dust = Dust.NewDustDirect(projectile.Center, 1, 1, DustID.GemDiamond, (i - 10) * 2 * normal.Y, (i - 10) * 2 * normal.X, 0, new Color(255, 255, 255, 32), (float)(20 - Math.Abs(10 - i)) / 6);
					dust.noGravity = true;
					dust.noLight = true;
				}
				for (int i = 0; i < 10; i++) {
					Dust dust = Dust.NewDustDirect(projectile.Center, 20, 20, DustID.Torch, Scale: 8f);
					dust.velocity.X *= 5;
					dust.velocity.Y *= 5;
					dust.noGravity = true;
				}
				SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, projectile.Center);
						
				Player owner = Main.player[projectile.owner];
				if (Main.LocalPlayer != owner) {
					return;
				}

				owner.GetModPlayer<StylePlayer>().AddStyleBonus(StyleBonus.RobinHood);
				owner.GetModPlayer<StylePlayer>().CheckQuickDraw();
				return;
			}
		}
	}
}