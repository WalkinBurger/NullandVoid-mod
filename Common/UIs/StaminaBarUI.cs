using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NullandVoid.Common.Players;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace NullandVoid.Common.UIs
{
	internal class StaminaBarUI : UIState
	{
		private UIElement area;
		private static Texture2D barEmpty;
		private static Texture2D barFull;
		private static int barWidth;
		private static int barHeight;
		private Rectangle areaRect;

		private bool showStaminaUI;

		public void ChangeConfig() {
			showStaminaUI = ModContent.GetInstance<NullandVoidClientConfig>().ShowStaminaUI;
		}
		
		public override void OnInitialize() {
			area = new UIElement();
			area.Left.Set(-450, 1);
			area.Top.Set(15, 0);

			barEmpty = ModContent.Request<Texture2D>("NullandVoid/Common/UIs/StaminaBar", AssetRequestMode.ImmediateLoad).Value;
			barFull = ModContent.Request<Texture2D>("NullandVoid/Common/UIs/StaminaBarFull", AssetRequestMode.ImmediateLoad).Value;
			
			barWidth = barEmpty.Width;
			barHeight = barEmpty.Height;
			
			Append(area);
			
			ChangeConfig();
		}

		public override void Draw(SpriteBatch spriteBatch) {
			if (!showStaminaUI) {
				return;
			}
			
			base.Draw(spriteBatch);
			
			StaminaPlayer staminaPlayer = Main.LocalPlayer.GetModPlayer<StaminaPlayer>();
			areaRect = area.GetInnerDimensions().ToRectangle();
			float staminaRatio = (float)staminaPlayer.StaminaResource / 20;
			int staminaBars = staminaPlayer.StaminaMax / 20;
			for (int i = 0; i < staminaBars; i++) {
				Vector2 barOrigin = new Vector2(areaRect.Left - i * 50, areaRect.Top);
				spriteBatch.Draw(barEmpty, barOrigin, Color.White);

				if (staminaRatio <= i) {
					continue;
				}
				
				int barOffset = (int)(barWidth * (1 - (staminaRatio - i)));
				Color barColor = Color.White;
				barColor.A = barColor.R = barColor.G = barColor.B = staminaRatio - i >= 1f ? (byte)255 : (byte)128;

				spriteBatch.Draw(
					barFull,
					new Vector2(barOrigin.X + barOffset, barOrigin.Y),
					new Rectangle(barOffset, 0, barWidth - barOffset, barHeight),
					barColor
				);
			}
		}
	}

	[Autoload(Side = ModSide.Client)]
	internal class StaminaBarSystem : ModSystem
	{
		internal StaminaBarUI StaminaBarUI;
		private UserInterface StaminaBarUserInterface;

		public override void Load() {
			StaminaBarUI = new StaminaBarUI();
			StaminaBarUserInterface = new UserInterface();
			StaminaBarUserInterface.SetState(StaminaBarUI);
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
			if (mouseTextIndex != -1) {
				layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer("NullandVoid: Stamina Bar", delegate {
					StaminaBarUserInterface.Draw(Main.spriteBatch, new GameTime());
					return true;
				}, InterfaceScaleType.UI));
			}
		}
	}
}