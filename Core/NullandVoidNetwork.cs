using System;
using System.IO;
using NullandVoid.Common.Players;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace NullandVoid.Core
{
	public class NullandVoidNetwork
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
				case MessageType.Sword:
					HandleSwordMessage(reader, whoAmI);
					break;
				case MessageType.Shoot:
					HandleShootMessage(reader, whoAmI);
					break;
				case MessageType.MovementAbility:
					HandleMovementAbilityMessage(reader, whoAmI);
					break;
				case MessageType.MovementAltAbility:
					HandleMovementAltAbilityMessage(reader, whoAmI);
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
			Sword,
			Shoot,
			MovementAbility,
			MovementAltAbility,
		}
		

		
		public static void SendSoundMessage(int whoAmI, SoundsID soundsID, int wildVar = 1, bool fromSender = true) {
			ModPacket packet = ModContent.GetInstance<NullandVoid>().GetPacket();
			packet.Write((byte)MessageType.Sound);
			packet.Write((byte)whoAmI);
			packet.Write((byte)soundsID);
			packet.Write((byte)wildVar);
			packet.Write(fromSender);
			packet.Send();
		}
		
		public static void SendSoundMessage(int whoAmI, int soundsID, int wildVar = -1, bool fromSender = true) {
			ModPacket packet = ModContent.GetInstance<NullandVoid>().GetPacket();
			packet.Write((byte)MessageType.Sound);
			packet.Write((byte)whoAmI);
			packet.Write((sbyte)((soundsID + 1) * (fromSender ? 1 : -1)));
			packet.Write((byte)wildVar);
			packet.Send();
		}
		
		public static void HandleSoundMessage(BinaryReader reader, int whoAmI) {
			int player = reader.ReadByte();
			int sounds = reader.ReadSByte();
			int wildVar = reader.ReadByte();
			bool fromSender = sounds > 0;
			sounds = Math.Abs(sounds) - 1;
			if (Main.dedServ) {
				player = whoAmI;
				SendSoundMessage(player, sounds, wildVar, fromSender);
			}
			else {
				SoundEngine.PlaySound(Sounds.GetSound(sounds, wildVar), fromSender? Main.player[player].Center : null);
			}
		}
		

		public static void SendParryMessage(int whoAmI, int parryCount, bool swordParry) {
			ModPacket packet = ModContent.GetInstance<NullandVoid>().GetPacket();
			packet.Write((byte)MessageType.Parry);
			packet.Write((byte)whoAmI);
			if (swordParry) {
				packet.Write((sbyte)(parryCount + 1));
			}
			else {
				packet.Write((sbyte)(parryCount + 1) * -1);
			}
			packet.Send(ignoreClient: whoAmI);
		}
		
		public static void HandleParryMessage(BinaryReader reader, int whoAmI) {
			int player = reader.ReadByte();
			int parryCount = reader.ReadSByte();
			bool swordParry = parryCount > 0;
			parryCount = Math.Abs(parryCount) - 1;
			if (Main.dedServ) {
				player = whoAmI;
				SendParryMessage(player, parryCount, swordParry);
			}
			else {
				Main.player[player].GetModPlayer<ParryPlayer>().ParryEffects(player, parryCount, swordParry);
			}
		}
		
		
		public static void SendSwordMessage(int whoAmI, float angle, int style) {
			ModPacket packet = ModContent.GetInstance<NullandVoid>().GetPacket();
			packet.Write((byte)MessageType.Sword);
			packet.Write((byte)whoAmI);
			packet.Write((Half)(Math.Sign(angle) * style * 4 + angle));
			packet.Send(ignoreClient: whoAmI);
		}
		
		public static void HandleSwordMessage(BinaryReader reader, int whoAmI) {
			int player = reader.ReadByte();
			float angle = (float)reader.ReadHalf();
			int style = Math.Abs((int)angle / 4);
			angle %= 4;
			if (Main.dedServ) {
				player = whoAmI;
				SendSwordMessage(player, angle, style);
			}
			else {
				Main.player[player].GetModPlayer<UseStylePlayer>().SetHit(player, angle, style);
			}
		}
		
		
		public static void SendShootMessage(int whoAmI, float angle) {
			ModPacket packet = ModContent.GetInstance<NullandVoid>().GetPacket();
			packet.Write((byte)MessageType.Shoot);
			packet.Write((byte)whoAmI);
			packet.Write((Half)angle);
			packet.Send(ignoreClient: whoAmI);
		}
		
		public static void HandleShootMessage(BinaryReader reader, int whoAmI) {
			int player = reader.ReadByte();
			float angle = (float)reader.ReadHalf();
			if (Main.dedServ) {
				player = whoAmI;
				SendShootMessage(player, angle);
			}
			else {
				Main.player[player].GetModPlayer<UseStylePlayer>().ShootAngle = angle;
			}
		}
		
		
		public static void SendMovementAbilityMessage(int whoAmI) {
			ModPacket packet = ModContent.GetInstance<NullandVoid>().GetPacket();
			packet.Write((byte)MessageType.MovementAbility);
			packet.Write((byte)whoAmI);
			packet.Send(ignoreClient: whoAmI);
		}
		
		public static void HandleMovementAbilityMessage(BinaryReader reader, int whoAmI) {
			int player = reader.ReadByte();
			if (Main.dedServ) {
				player = whoAmI;
				SendMovementAbilityMessage(player);
			}
			else {
				MovementClassPlayer movementClassPlayer = Main.player[player].GetModPlayer<MovementClassPlayer>();
				movementClassPlayer.UsingAbility = true;
				movementClassPlayer.UseAbility();
			}
		}
		
		
		public static void SendMovementAltAbilityMessage(int whoAmI) {
			ModPacket packet = ModContent.GetInstance<NullandVoid>().GetPacket();
			packet.Write((byte)MessageType.MovementAltAbility);
			packet.Write((byte)whoAmI);
			packet.Send(ignoreClient: whoAmI);
		}
		
		public static void HandleMovementAltAbilityMessage(BinaryReader reader, int whoAmI) {
			int player = reader.ReadByte();
			if (Main.dedServ) {
				player = whoAmI;
				SendMovementAltAbilityMessage(player);
			}
			else {
				MovementClassPlayer movementClassPlayer = Main.player[player].GetModPlayer<MovementClassPlayer>();
				movementClassPlayer.UsingAltAbility = true;
				movementClassPlayer.UseAbility();
			}
		}
	}
}