using System;
using System.Linq;
using Microsoft.Xna.Framework;
using NullandVoid.Common.Players;
using NullandVoid.Core;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace NullandVoid.Content.Projectiles
{
	public class ParryExplosion : ModProjectile
	{
		public override string Texture => "Terraria/Images/MagicPixel";

		public override void SetDefaults() {
			Projectile.penetrate = -1;
			Projectile.friendly = true;
			Projectile.alpha = 255;
		}

		public override bool PreDraw(ref Color lightColor) {
			return false;
		}

		public override void OnSpawn(IEntitySource source) {
			Projectile parent = Main.projectile.FirstOrDefault(x => x.identity == (int)Projectile.ai[0]);
			Projectile.Size = new Vector2(parent.width + Math.Abs(parent.velocity.X), parent.height + Math.Abs(parent.velocity.Y));
		}

		public override void AI() {
			if (Projectile.timeLeft < 10) {
				return;
			}

			Projectile parent = Main.projectile.FirstOrDefault(x => x.identity == (int)Projectile.ai[0]);
			if (parent.numHits == 0 && parent.timeLeft > 1) {
				Projectile.timeLeft = parent.timeLeft;
				Projectile.Center = parent.Center - parent.velocity;
			}
			else {
				Explode();
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Projectile.Resize((int)Projectile.ai[1], (int)Projectile.ai[1]);
			Player player = Main.player[Math.Abs((int)Projectile.ai[2])];
			Explode();
			if (player != Main.LocalPlayer) {
				return;
			}

			if (!target.active && Math.Sign(Projectile.ai[2]) == 1) {
				player.GetModPlayer<StylePlayer>().AddStyleBonus(StyleBonus.FriendlyFire);
			}
		}

		public void Explode() {
			Projectile.Resize((int)Projectile.ai[1], (int)Projectile.ai[1]);
			Projectile.timeLeft = 10;
			if (Main.dedServ) {
				return;
			}

			SoundEngine.PlaySound(SoundID.Item38 with { Volume = 0.3f, PitchVariance = 0.2f, MaxInstances = 8 }, Projectile.position);
			SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.3f, PitchVariance = 0.2f, MaxInstances = 8 }, Projectile.position);
			int dustRange = (int)(Projectile.ai[1] / 2);
			Vector2 dustPosition = Projectile.Center - new Vector2(dustRange / 2, dustRange / 2);
			for (int i = 0; i < 14 * ModContent.GetInstance<NullandVoidClientConfig>().GeneralDustAmount; i++) {
				Dust dust = Dust.NewDustDirect(dustPosition, dustRange, dustRange, DustID.GemTopaz, 0f, 0f, 192);
				dust.noGravity = true;

				dust = Dust.NewDustDirect(dustPosition, dustRange, dustRange, DustID.Torch, 0f, 0f, 0, default, 3f);
				dust.noGravity = true;
				dust.velocity *= 5f;
			}
		}
	}
}