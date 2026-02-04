using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace NullandVoid
{
	public class NullandVoidClientConfig : ModConfig
	{
		public override ConfigScope Mode {
			get { return ConfigScope.ClientSide; }
		}

		[Header("Parrying")]
		[DefaultValue(12)]
		[Increment(1)]
		[Range(0, 120)]
		public int ParryingFrameFreezing { get; set; }

		[DefaultValue(0.15f)]
		[Increment(0.01f)]
		[Range(0f, 0.5f)]
		public float ParryingFlashIntensity { get; set; }

		[DefaultValue(0.7f)]
		[Increment(0.1f)]
		[Range(0f, 10f)]
		public float ParryingShakeIntensity { get; set; }

		[DefaultValue(1f)]
		[Increment(0.1f)]
		[Range(0f, 1f)]
		public float ParryingSoundVolume { get; set; }
	}

	public class NullandVoidServerConfig : ModConfig
	{
		public override ConfigScope Mode {
			get { return ConfigScope.ServerSide; }
		}
	}
}