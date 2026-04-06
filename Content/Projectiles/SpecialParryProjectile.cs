using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace NullandVoid.Content.Projectiles
{
	public enum SpecialParryAIStyle
	{
		Boomerang,
	}

	public class SpecialParryProjectile : ModProjectile
	{
		public override string Texture => "Terraria/Images/MagicPixel";
		private Projectile parent;

		public override void SetDefaults() {
			Projectile.penetrate = -1;
			Projectile.friendly = true;
			Projectile.alpha = 255;
			Projectile.timeLeft = 10;
		}

		public override bool PreDraw(ref Color lightColor) {
			return false;
		}

		public override void AI() {
			if (Projectile.timeLeft == 10) {
				parent = Main.projectile.FirstOrDefault(x => x.identity == (int)Projectile.ai[0]);
			}

			switch (Projectile.ai[1]) {
				case 0:
					if (Projectile.timeLeft == 10) {
						parent.knockBack = Math.Min(20, parent.knockBack * 2 + 5);
					}

					parent.Center = (parent.Center + Projectile.Center) / 2;
					parent.velocity = Vector2.Zero;
					break;
			}
		}
	}
}