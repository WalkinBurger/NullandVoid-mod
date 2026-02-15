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

		[DefaultValue(0.7f)]
		[Increment(0.05f)]
		[Range(0f, 1f)]
		public float ParryingFlashIntensity { get; set; }

		[DefaultValue(0.7f)]
		[Increment(0.05f)]
		[Range(0f, 1f)]
		public float ParryingShakeIntensity { get; set; }

		[DefaultValue(1f)]
		[Increment(0.05f)]
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