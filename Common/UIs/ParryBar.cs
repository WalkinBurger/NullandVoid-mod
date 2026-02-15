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
		private static Texture2D barEmpty;
		private UIImage barEmptyUI;
		private static Texture2D barFull;
		private static int  barWidth;
		private static int  barHeight;
		private int parryBarFrame = 0;
		private Rectangle barFrame;
		Color barColor = Color.White;

		public override void OnInitialize() {
			area = new UIElement();
			area.Left.Set(-360, 1f);
			area.Top.Set(15, 0f);
			area.Width.Set(128, 0f);
			area.Height.Set(40, 0f);

			barEmpty = ModContent.Request<Texture2D>("NullandVoid/Common/UIs/ParryBar", AssetRequestMode.ImmediateLoad).Value;
			barFull = ModContent.Request<Texture2D>("NullandVoid/Common/UIs/ParryBarFullAnim", AssetRequestMode.ImmediateLoad).Value;
			barWidth = barEmpty.Width;
			barHeight = barEmpty.Height;

			barEmptyUI = new UIImage(barEmpty);
			barEmptyUI.Left.Set(0, 0f);
			barEmptyUI.Top.Set(0, 0f);
			barEmptyUI.Color.A = 255;
			
			
			area.Append(barEmptyUI);
			Append(area);
		}

		public override void Draw(SpriteBatch spriteBatch) {
			base.Draw(spriteBatch);
			
			barFrame = barEmptyUI.GetInnerDimensions().ToRectangle();
			ParryPlayer parryPlayer = Main.LocalPlayer.GetModPlayer<ParryPlayer>();
			float parryRatio = (float)parryPlayer.ParryResource / ParryPlayer.ParryResourceMax;
			
			if (parryRatio == 1) {
				if (parryBarFrame == 0) {
					parryBarFrame = 6;
				}
				else if (parryBarFrame != 1) {
					parryBarFrame--;
				}
			}
			else {
				parryBarFrame = 0;
			}
			
			int barOffset = (int)((((float)barFull.Height / 6) - 1) * (1f - parryRatio));
			barColor.A = barColor.R = barColor.G = barColor.B = parryRatio == 1f ? (byte)255 : (byte)128;
			spriteBatch.Draw(
				barFull,
				new Vector2(barFrame.Left, barFrame.Top + barOffset),
				parryRatio == 1f? new Rectangle(0, (barHeight + 2) * (parryBarFrame - 1), barWidth, barHeight) : new Rectangle(0, barOffset, barWidth, barHeight - barOffset),
				barColor
			);
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