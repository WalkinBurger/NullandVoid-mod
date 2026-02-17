using System.IO;
using Microsoft.Xna.Framework.Graphics;
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
		/*
		public override void Load() {
			if (Main.netMode != NetmodeID.Server) {
				Asset<Effect> styleBonusEffect = Assets.Request<Effect>("Assets/Effects/StyleBonusEffect");
				
				GameShaders.Misc["NullandVoid:StyleBonusEffect"] = new MiscShaderData(styleBonusEffect, "StyleBonusPass");
			}
		}
		*/
		
		public override void HandlePacket(BinaryReader reader, int whoAmI) {
			MessageType msgType = (MessageType)reader.ReadByte();

			switch (msgType) {
				case MessageType.ParryEffects:
					ParryPlayer.HandleParryEffectsMessage(reader, whoAmI);
					break;
				default:
					Logger.WarnFormat("Null and Void: Unknown Message type: {0}", msgType);
					break;
			}
		}

		internal enum MessageType : byte
		{
			ParryEffects,
		}
	}
}