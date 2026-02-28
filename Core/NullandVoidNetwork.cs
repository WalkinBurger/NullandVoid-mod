using System;
using System.Collections.Immutable;
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
				case MessageType.Dash:
					HandleDashMessage(reader, whoAmI);
					break;
				case MessageType.MaintainVel:
					HandleMaintainVelMessage(reader, whoAmI);
					break;
				case MessageType.Sword:
					HandleSwordMessage(reader, whoAmI);
					break;
				case MessageType.Shoot:
					HandleShootMessage(reader, whoAmI);
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
			Dash,
			MaintainVel,
			Sword,
			Shoot,
		}
		

		public enum Sounds : byte
		{
			Pogo,
		}
		
		public static SoundStyle SoundList(int sounds, int wildVar) {
			float staminaSoundVolume = ModContent.GetInstance<NullandVoidClientConfig>().StaminaSoundVolume;
			float parrySoundVolume = ModContent.GetInstance<NullandVoidClientConfig>().ParrySoundVolume;
		
			ImmutableList<SoundStyle> soundList = [
				SoundID.DrumClosedHiHat with { Pitch = wildVar == 5? -1f : 0, PitchVariance = 0.2f },
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
			if (Main.netMode == NetmodeID.Server) {
				player = whoAmI;
				SendParryMessage(player, parryCount, swordParry);
			}
			else {
				Main.player[player].GetModPlayer<ParryPlayer>().ParryEffects(player, parryCount, swordParry);
			}
		}
		
		
		public static void SendDashMessage(int whoAmI, int dashTime, int dashDirection) {
			ModPacket packet = ModContent.GetInstance<NullandVoid>().GetPacket();
			packet.Write((byte)MessageType.Dash);
			packet.Write((byte)whoAmI);
			packet.Write((sbyte)(dashTime * dashDirection));
			packet.Send(ignoreClient: whoAmI);
		}
		
		public static void HandleDashMessage(BinaryReader reader, int whoAmI) {
			int player = reader.ReadByte();
			int dashTime = reader.ReadSByte();
			int dashDirection = Math.Sign(dashTime);
			dashTime = Math.Abs(dashTime);
			if (Main.netMode == NetmodeID.Server) {
				player = whoAmI;
				SendDashMessage(player, dashTime, dashDirection);
			}
			else {
				StaminaPlayer staminaPlayer = Main.player[player].GetModPlayer<StaminaPlayer>();
				staminaPlayer.DashFrame = staminaPlayer.DashTime = dashTime;
				staminaPlayer.DashDirection = dashDirection;
			}
		}
		
		
		public static void SendMaintainVelMessage(int whoAmI, bool maintainVel) {
			ModPacket packet = ModContent.GetInstance<NullandVoid>().GetPacket();
			packet.Write((byte)MessageType.MaintainVel);
			if (maintainVel) {
				packet.Write((sbyte)(whoAmI + 1));
			}
			else {
				packet.Write((sbyte)-(whoAmI + 1));
			}
			packet.Send(ignoreClient: whoAmI);
		}
		
		public static void HandleMaintainVelMessage(BinaryReader reader, int whoAmI) {
			int player = reader.ReadSByte();
			bool maintainVel = player > 0;
			player = Math.Abs(player) - 1;
			if (Main.netMode == NetmodeID.Server) {
				player = whoAmI;
				SendMaintainVelMessage(player, maintainVel);
			}
			else {
				Main.player[player].GetModPlayer<MovementMiscPlayer>().MaintainVelocity = maintainVel;
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
			if (Main.netMode == NetmodeID.Server) {
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
			if (Main.netMode == NetmodeID.Server) {
				player = whoAmI;
				SendShootMessage(player, angle);
			}
			else {
				Main.player[player].GetModPlayer<UseStylePlayer>().ShootAngle = angle;
			}
		}
	}
}