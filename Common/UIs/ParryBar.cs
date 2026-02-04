using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NullandVoid.Common.Players;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace NullandVoid.Common.UIs
{
	internal class ParryBar : UIState
	{
		private UIElement area;
		private UIImage barEmpty;
		private Asset<Texture2D> barFullTexture;

		public override void OnInitialize() {
			area = new UIElement();
			area.Left.Set(-350, 1f);
			area.Top.Set(30, 0f);
			area.Width.Set(128, 0f);
			area.Height.Set(40, 0f);

			barEmpty = new UIImage(ModContent.Request<Texture2D>("NullandVoid/Common/UIs/ParryBar"));
			barEmpty.Left.Set(0, 0f);
			barEmpty.Top.Set(0, 0f);
			barEmpty.Color.A = 255;

			area.Append(barEmpty);
			Append(area);
		}

		public override void Draw(SpriteBatch spriteBatch) {
			base.Draw(spriteBatch);

			ParryPlayer modPlayer = Main.LocalPlayer.GetModPlayer<ParryPlayer>();
			float parryRatio = (float)modPlayer.ParryResource / ParryPlayer.ParryResourceMax;
			Rectangle barFrame = barEmpty.GetInnerDimensions().ToRectangle();
			barFullTexture = ModContent.Request<Texture2D>("NullandVoid/Common/UIs/ParryBarFull");
			int barOffset = (int)(barFullTexture.Height() * (1 - parryRatio));
			Color barColor = Color.White;
			barColor.A = barColor.R = barColor.G = barColor.B = parryRatio == 1f ? (byte)255 : (byte)128;

			spriteBatch.Draw(barFullTexture.Value, new Vector2(barFrame.Left, barFrame.Top + barOffset),
				new Rectangle(0, barOffset, barFullTexture.Width(), barFullTexture.Height() - barOffset), barColor);
		}
	}

	[Autoload(Side = ModSide.Client)]
	internal class ParryBarSystem : ModSystem
	{
		internal ParryBar ParryBar;
		private UserInterface ParryBarUserInterface;

		public override void Load() {
			ParryBar = new ParryBar();
			ParryBarUserInterface = new UserInterface();
			ParryBarUserInterface.SetState(ParryBar);
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
			if (mouseTextIndex != -1) {
				layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer("NullandVoid: Parry Bar", delegate {
					ParryBarUserInterface.Draw(Main.spriteBatch, new GameTime());
					return true;
				}, InterfaceScaleType.UI));
			}
		}
	}
}