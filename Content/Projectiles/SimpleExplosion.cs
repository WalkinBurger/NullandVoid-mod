using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace NullandVoid.Content.Projectiles
{
	public class SimpleExplosion : ModProjectile
	{
		public override string Texture => "Terraria/Images/MagicPixel";

		public override void SetDefaults() {
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 3;
		}

		public override bool PreDraw(ref Color lightColor) {
			return false;
		}

		public override void OnSpawn(IEntitySource source) {
			Projectile.Resize((int)Projectile.ai[0], (int)Projectile.ai[0]);
		}
	}
}