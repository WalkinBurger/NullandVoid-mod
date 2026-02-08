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
	public class StaminaBar : UIState
	{
		private UIElement area;
		private Asset<Texture2D> barEmpty = ModContent.Request<Texture2D>("NullandVoid/Common/UIs/StaminaBar");
		private Asset<Texture2D> barFull = ModContent.Request<Texture2D>("NullandVoid/Common/UIs/StaminaBarFull");
		
		public override void OnInitialize() {
			area = new UIElement();
			area.Left.Set(-475, 1f);
			area.Top.Set(15, 0f);
			Append(area);
		}

		public override void Draw(SpriteBatch spriteBatch) {
			base.Draw(spriteBatch);
			
			StaminaPlayer modPlayer = Main.LocalPlayer.GetModPlayer<StaminaPlayer>();
			float staminaRatio = (float)modPlayer.StaminaResource / 20;
			int staminaBars = modPlayer.StaminaMax / 20;
			Rectangle areaRect = area.GetInnerDimensions().ToRectangle();
			for (int i = 0; i < staminaBars; i++) {
				Vector2 barOrigin = new Vector2(areaRect.Left - i * 55, areaRect.Top);
				spriteBatch.Draw(barEmpty.Value, barOrigin, Color.White);

				if (staminaRatio <= i) {
					continue;
				}
				
				int barOffset = (int)(barFull.Width() * (1 - (staminaRatio - i)));
				Color barColor = Color.White;
				barColor.A = barColor.R = barColor.G = barColor.B = staminaRatio - i >= 1f ? (byte)255 : (byte)128;

				spriteBatch.Draw(barFull.Value, new Vector2(barOrigin.X + barOffset, barOrigin.Y), new Rectangle(barOffset, 0, barFull.Width() - barOffset, barFull.Height()), barColor);
			}
		}
	}

	[Autoload(Side = ModSide.Client)]
	internal class StaminaBarSystem : ModSystem
	{
		internal StaminaBar StaminaBar;
		private UserInterface StaminaBarUserInterface;

		public override void Load() {
			StaminaBar = new StaminaBar();
			StaminaBarUserInterface = new UserInterface();
			StaminaBarUserInterface.SetState(StaminaBar);
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