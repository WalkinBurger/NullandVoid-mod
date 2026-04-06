using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace NullandVoid.Core
{
	public enum SoundsID : byte
	{
		Pogo,
		ChargeHurt,
	}

	public static class Sounds
	{
		public static SoundStyle GetSound(int soundsID, int wildVar) {
			switch (soundsID) {
				case (int)SoundsID.Pogo:
					return SoundID.DrumClosedHiHat with { Volume = ModContent.GetInstance<NullandVoidClientConfig>().PogoVolume, Pitch = wildVar == 5 ? -1f : 0, PitchVariance = 0.2f };
				case (int)SoundsID.ChargeHurt:
					return SoundID.Item53 with { Pitch = (float)wildVar / 10, PitchVariance = 0.2f };
				default:
					return SoundID.Item1;
			}
		}

		public static SoundStyle GetSound(SoundsID soundsID, int wildVar = 1) => GetSound((int)soundsID, wildVar);
	}
}