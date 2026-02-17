using System;
using System.ComponentModel;
using NullandVoid.Common;
using NullandVoid.Common.Players;
using NullandVoid.Common.UIs;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace NullandVoid
{
	public class NullandVoidClientConfig : ModConfig
	{
		public override ConfigScope Mode {
			get { return ConfigScope.ClientSide; }
		}

		[Header("StyleMeter")]
		
		[BackgroundColor(157, 58, 120)]
		[DefaultValue(true)]
		public bool ShowStyleMeterUI { get; set; }
		
		[BackgroundColor(198, 71, 130)]
		[SliderColor(216, 106, 159)]
		[DefaultValue(10)]
		[Increment(1)]
		[Range(0, 30)]
		[Slider()]
		public int MaxStyleBonuses { get; set; }
		
		[BackgroundColor(198, 71, 130)]
		[SliderColor(216, 106, 159)]
		[DefaultValue(5)]
		[Increment(1)]
		[Range(1, 10)]
		[Slider()]
		public int StyleBonusFadeTime { get; set; }
		
		[BackgroundColor(198, 71, 130)]
		[SliderColor(216, 106, 159)]
		[DefaultValue(5)]
		[Increment(1)]
		[Range(0, 10)]
		[Slider()]
		public int StyleMeterHideTime { get; set; }
		
		[BackgroundColor(198, 71, 130)]
		[SliderColor(216, 106, 159)]
		[DefaultValue(0.5f)]
		[Increment(0.1f)]
		[Range(0, 1f)]
		public float StyleMeterEaseSpeed { get; set; }
		
		[Header("Parrying")]
		
		[BackgroundColor(187, 136, 90)]
		[DefaultValue(true)]
		public bool ShowParryUI { get; set; }

		[BackgroundColor(223, 187, 137)]
		[SliderColor(240, 224, 184)]
		[DefaultValue(0.75f)]
		[Increment(0.05f)]
		[Range(0f, 1f)]
		public float ParryFlashIntensity { get; set; }

		[BackgroundColor(223, 187, 137)]
		[SliderColor(240, 224, 184)]
		[DefaultValue(0.75f)]
		[Increment(0.05f)]
		[Range(0f, 1f)]
		public float ParryShakeIntensity { get; set; }

		[BackgroundColor(223, 187, 137)]
		[SliderColor(240, 224, 184)]
		[DefaultValue(0.75f)]
		[Increment(0.05f)]
		[Range(0f, 1f)]
		public float ParrySoundVolume { get; set; }
		
		[Header("Stamina")]
		
		[BackgroundColor(44, 159, 76)]
		[DefaultValue(true)]
		public bool ShowStaminaUI { get; set; }
		
		[BackgroundColor(104, 209, 80)]
		[SliderColor(197, 245, 125)]
		[DefaultValue(0.75f)]
		[Increment(0.05f)]
		[Range(0f, 1f)]
		public float StaminaSoundVolume { get; set; }
		
		public override void OnChanged() {
			Player player = Main.LocalPlayer;
			if (player == null) { 
				return;
			}

			try {
				player.GetModPlayer<ParryPlayer>().ChangeConfig();
				player.GetModPlayer<StylePlayer>().ChangeConfig();
				
				ModContent.GetInstance<ParryBarSystem>().ChangeConfig();
				ModContent.GetInstance<StaminaBarSystem>().ChangeConfig();
				ModContent.GetInstance<StyleMeterSystem>().ChangeConfig();
				
				ModContent.GetInstance<GlowLayer>().ChangeConfig();
			}
			catch (Exception e) {
				// first load?
			}
		}
	}

	public class NullandVoidServerConfig : ModConfig
	{
		public override ConfigScope Mode {
			get { return ConfigScope.ServerSide; }
		}
	}
}