using System.Collections.Immutable;
using System.IO;
using NullandVoid.Common.Players;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace NullandVoid.Common
{
	public class NetHandler
	{
		public static void HandlePacket(BinaryReader reader, int whoAmI) {
			MessageType msgType = (MessageType)reader.ReadByte();

			switch (msgType) {
				case MessageType.Sound:
					HandleSoundMessage(reader, whoAmI);
					break;
				case MessageType.Parry:
					HandleParryMessage(reader, whoAmI);
					break;
				default:
					NullandVoid.Instance.Logger.WarnFormat("Null and Void: Unknown Message type: {0}", msgType);
					break;
			}
		}

		private enum MessageType : byte
		{
			Sound,
			Parry,
		}
		

		public enum Sounds : byte
		{
			Pogo,
			Dash,
			DashJump,
		}
		
		public static SoundStyle SoundList(int sounds, int wildVar) {
			float staminaSoundVolume = ModContent.GetInstance<NullandVoidClientConfig>().StaminaSoundVolume;
			float parrySoundVolume = ModContent.GetInstance<NullandVoidClientConfig>().ParrySoundVolume;
		
			ImmutableList<SoundStyle> soundList = [
				SoundID.DrumClosedHiHat with { Pitch = wildVar == 5? -1f : 0, PitchVariance = 0.2f },
				SoundID.DD2_BetsysWrathShot with { Pitch = 0.6f, Volume = 0.3f * staminaSoundVolume, PitchVariance = 0.2f },
				SoundID.DD2_BetsysWrathShot with { Pitch = -0.2f, Volume = 0.4f * staminaSoundVolume },
			];

			return soundList[sounds];
		}
		
		public static void SendSoundMessage(int whoAmI, Sounds sounds, int wildVar = 1, bool fromSender = true) {
			ModPacket packet = ModContent.GetInstance<NullandVoid>().GetPacket();
			packet.Write((byte)MessageType.Sound);
			packet.Write((byte)whoAmI);
			packet.Write((byte)sounds);
			packet.Write((byte)wildVar);
			packet.Write(fromSender);
			packet.Send();
		}
		
		public static void SendSoundMessage(int whoAmI, int sounds, int wildVar = -1, bool fromSender = true) {
			ModPacket packet = ModContent.GetInstance<NullandVoid>().GetPacket();
			packet.Write((byte)MessageType.Sound);
			packet.Write((byte)whoAmI);
			packet.Write((byte)sounds);
			packet.Write((byte)wildVar);
			packet.Write(fromSender);
			packet.Send();
		}
		
		public static void HandleSoundMessage(BinaryReader reader, int whoAmI) {
			int player = reader.ReadByte();
			int sounds = reader.ReadByte();
			int wildVar = reader.ReadByte();
			bool fromSender = reader.ReadBoolean();
			if (Main.netMode == NetmodeID.Server) {
				player = whoAmI;
				SendSoundMessage(player, sounds, wildVar, fromSender);
			}
			else {
				SoundEngine.PlaySound(SoundList(sounds, wildVar), fromSender? Main.player[player].Center : null);
			}
		}
		

		public static void SendParryMessage(int whoAmI, int parryCount, bool swordParry) {
			ModPacket packet = ModContent.GetInstance<NullandVoid>().GetPacket();
			packet.Write((byte)MessageType.Parry);
			packet.Write((byte)whoAmI);
			packet.Write((byte)parryCount);
			packet.Write(swordParry);
			packet.Send(ignoreClient: whoAmI);
		}
		
		public static void HandleParryMessage(BinaryReader reader, int whoAmI) {
			int player = reader.ReadByte();
			int parryCount = reader.ReadByte();
			bool swordParry = reader.ReadBoolean();
			if (Main.netMode == NetmodeID.Server) {
				player = whoAmI;
				SendParryMessage(player, parryCount, swordParry);
			}
			Main.player[player].GetModPlayer<ParryPlayer>().ParryEffects(player, parryCount, swordParry);
		}
	}
}