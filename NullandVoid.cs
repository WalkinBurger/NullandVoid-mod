using System.IO;
using Microsoft.Xna.Framework.Graphics;
using NullandVoid.Common;
using NullandVoid.Common.Players;
using ReLogic.Content;
using Terraria;
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
				GameShaders.Misc["NullandVoid:StyleBonusEffect"] = new MiscShaderData(styleBonusEffect, "StyleBonusPass");
			}
		}
		
		public override void HandlePacket(BinaryReader reader, int whoAmI) {
			NetHandler.HandlePacket(reader, whoAmI);
		}
	}
}