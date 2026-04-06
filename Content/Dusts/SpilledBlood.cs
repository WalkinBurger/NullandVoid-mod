using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace NullandVoid.Content.Dusts
{
	public class SpilledBlood : ModDust
	{
		public override string Texture => "Terraria/Images/MagicPixel";

		public override void SetStaticDefaults() {
			ChildSafety.SafeDust[Type] = false;
		}

		public override void OnSpawn(Dust dust) {
			if (ModContent.GetInstance<NullandVoidClientConfig>().ShowBloodTrail) {
				dust.customData = new Queue<Vector2>(4);
			}

			dust.noLight = true;
		}

		public override bool PreDraw(Dust dust) {
			if (dust.customData is not Queue<Vector2> trail) {
				return true;
			}

			Color lightColor = Lighting.GetColor(dust.position.ToTileCoordinates(), dust.color);
			int i = 3;
			foreach (Vector2 pos in trail) {
				Main.EntitySpriteDraw(TextureAssets.MagicPixel.Value, pos - Main.screenPosition, new Rectangle(0, 0, 8, 8), lightColor, dust.rotation + i, new Vector2(4f, 4f), dust.scale - (float)i / 16, SpriteEffects.None);
				i--;
			}

			return true;
		}

		public override bool Update(Dust dust) {
			if (dust.customData is not Queue<Vector2> trail) {
				return true;
			}

			if (trail.Count == 4) {
				trail.Dequeue();
			}

			trail.Enqueue(dust.position - dust.velocity);
			return true;
		}
	}
}