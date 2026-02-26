using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NullandVoid.Utils;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace NullandVoid.Content.Projectiles
{
	public class GlowStarProjectile : ModProjectile
	{
		public override string Texture {
			get { return "NullandVoid/Assets/Textures/GlowStar"; }
		}

		public override void SetDefaults() {
			Projectile.width = Projectile.height = 128;
		}
		
		public override void OnSpawn(IEntitySource source) {
			Projectile.timeLeft = (int)Projectile.ai[0];
			Projectile.netUpdate = true;
		}

		public override void AI() {
			if (Projectile.timeLeft >= Projectile.ai[0]) {
				Projectile.timeLeft = (int)Projectile.ai[0];
			}
			
			Player player = Main.player[Projectile.owner];
			
			Projectile.Center = new Vector2(player.Bottom.X, player.Bottom.Y + 2);
			float t = NullandVoidUtils.EaseInPow(Projectile.timeLeft / Projectile.ai[0], 3);
			Projectile.rotation = t;
		}

		public override bool PreDraw(ref Color lightColor) {
			float t = NullandVoidUtils.EaseInPow(Projectile.timeLeft / Projectile.ai[0], 3);
			lightColor = new Color(t, t, t, 0);
			return true;
		}
	}
}