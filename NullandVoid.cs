using System.IO;
using Microsoft.Xna.Framework.Graphics;
using NullandVoid.Core;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace NullandVoid
{
	public class NullandVoid : Mod
	{
		public static Mod Instance => ModContent.GetInstance<NullandVoid>();

		public override void Load() {
			if (Main.netMode != NetmodeID.Server) {
				Asset<Effect> styleBonusEffect = Assets.Request<Effect>("Assets/Effects/StyleBonusEffect");
				Asset<Effect> spiritVignette = Assets.Request<Effect>("Assets/Effects/SpiritVignette");
				GameShaders.Misc["NullandVoid:StyleBonusEffect"] = new MiscShaderData(styleBonusEffect, "StyleBonusPass");
				Filters.Scene["NullandVoid:SpiritVignette"] = new Filter(new ScreenShaderData(spiritVignette, "VignettePass"), EffectPriority.Medium);
				Filters.Scene["NullandVoid:SpiritVignette"].Load();
			}
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI) {
			NullandVoidNetwork.HandlePacket(reader, whoAmI);
		}
	}
}