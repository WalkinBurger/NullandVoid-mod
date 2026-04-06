using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace NullandVoid
{
	public class NullandVoidClientConfig : ModConfig
	{
		public override ConfigScope Mode {
			get { return ConfigScope.ClientSide; }
		}

		[Header("Misc")]
		[DefaultValue(0.75f)]
		[Increment(0.05f)]
		[Range(0f, 1f)]
		public float CameraShakeIntensity { get; set; }

		[DefaultValue(0.75f)]
		[Increment(0.05f)]
		[Range(0f, 1f)]
		public float GeneralDustAmount { get; set; }
		
		[Header("StyleMeter")]
		[DefaultValue(true)]
		public bool ShowStyleMeterUI { get; set; }

		[DefaultValue(5)]
		[Increment(1)]
		[Range(0, 10)]
		[Slider]
		public int StyleMeterHideTime { get; set; }

		[DefaultValue(10)]
		[Increment(1)]
		[Range(0, 30)]
		[Slider]
		public int MaxStyleBonuses { get; set; }

		[DefaultValue(5)]
		[Increment(1)]
		[Range(1, 10)]
		[Slider]
		public int StyleBonusFadeTime { get; set; }

		[DefaultValue(true)]
		public bool StyleMeterEase { get; set; }

		[Header("Parrying")]
		[DefaultValue(true)]
		public bool ShowParryUI { get; set; }

		[DefaultValue(0.75f)]
		[Increment(0.05f)]
		[Range(0f, 1f)]
		public float ParryFlashIntensity { get; set; }

		[DefaultValue(0.75f)]
		[Increment(0.05f)]
		[Range(0f, 1f)]
		public float ParryShakeIntensity { get; set; }

		[DefaultValue(0.75f)]
		[Increment(0.05f)]
		[Range(0f, 1f)]
		public float ParrySoundVolume { get; set; }

		[Header("Movement")]
		[DefaultValue(true)]
		public bool ShowStaminaUI { get; set; }

		[DefaultValue(0.75f)]
		[Increment(0.05f)]
		[Range(0f, 1f)]
		public float StaminaSoundVolume { get; set; }

		[DefaultValue(0.75f)]
		[Increment(0.05f)]
		[Range(0f, 1f)]
		public float PogoVolume { get; set; }
		
		[DefaultValue(0.75f)]
		[Increment(0.05f)]
		[Range(0f, 1f)]
		public float DashAbilitiesVolume { get; set; }
		
		[DefaultValue(0.75f)]
		[Increment(0.05f)]
		[Range(0f, 1f)]
		public float ChargeAbilitiesVolume { get; set; }
		
		[DefaultValue(0.75f)]
		[Increment(0.05f)]
		[Range(0f, 1f)]
		public float SpiritAbilitiesVolume { get; set; }
		
		[DefaultValue(0.75f)]
		[Increment(0.05f)]
		[Range(0f, 1f)]
		public float GrappleAbilitiesVolume { get; set; }
		
		[Header("Blood")]
		[DefaultValue(true)]
		public bool ShowBloodSpill { get; set; }

		[DefaultValue(true)]
		public bool ShowBloodTrail { get; set; }

		[DefaultValue(4)]
		[Increment(1)]
		[Range(1, 10)]
		public int BloodAmount { get; set; }

		[DefaultValue(0.65f)]
		[Increment(0.05f)]
		[Range(0.3f, 2f)]
		public float BloodSize { get; set; }
	}

	public class NullandVoidServerConfig : ModConfig
	{
		public override ConfigScope Mode {
			get { return ConfigScope.ServerSide; }
		}
	}
}