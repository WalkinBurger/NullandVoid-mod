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
		
		[Header("Stamina")]
		
		[DefaultValue(true)]
		public bool ShowStaminaUI { get; set; }
		
		[DefaultValue(0.75f)]
		[Increment(0.05f)]
		[Range(0f, 1f)]
		public float StaminaSoundVolume { get; set; }
		
		[Header("StyleMeter")]
		
		[DefaultValue(true)]
		public bool ShowStyleMeterUI { get; set; }
		
		[DefaultValue(10)]
		[Increment(1)]
		[Range(0, 30)]
		public int MaxStyleBonuses { get; set; }
		
		[DefaultValue(5)]
		[Increment(1)]
		[Range(1, 10)]
		public int StyleBonusFadeTime { get; set; }
		
		[DefaultValue(5)]
		[Increment(1)]
		[Range(0, 10)]
		public int StyleMeterHideTime { get; set; }
		
		[DefaultValue(0.5f)]
		[Increment(0.1f)]
		[Range(0, 1f)]
		public float StyleMeterEaseSpeed { get; set; }
		
		public override void OnChanged() {
			Player player = Main.LocalPlayer;
			if (player == null) { 
				return;
			}

			try {
				player.GetModPlayer<ParryPlayer>().ChangeConfig();
				ModContent.GetInstance<StylePlayer>().ChangeConfig();
				ModContent.GetInstance<ParryBarUI>().ChangeConfig();
				ModContent.GetInstance<StaminaBarUI>().ChangeConfig();
				ModContent.GetInstance<StyleMeterUI>().ChangeConfig();
				ModContent.GetInstance<BloomLayer>().ChangeConfig();
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