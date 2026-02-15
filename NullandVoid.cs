using System.IO;
using NullandVoid.Common.Players;
using Terraria.ModLoader;

namespace NullandVoid
{
	public class NullandVoid : Mod
	{
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