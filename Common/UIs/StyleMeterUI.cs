using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NullandVoid.Common.Players;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace NullandVoid.Common.UIs
{
	internal class StyleMeterUI : UIState
	{
		private UIElement area;
		private UIText styleRankText;
		private int textShake;
		private int styleVisualFill;
		private int idleTime;
		private int styleBonusOffset;
		
		private bool showStyleMeterUI;
		private int maxStyleBonuses;
		private int styleBonusFadeTime;
		private int styleMeterHideTime;
		private float styleMeterEaseSpeed;

		public void ChangeConfig() {
			showStyleMeterUI = ModContent.GetInstance<NullandVoidClientConfig>().ShowStyleMeterUI;
			maxStyleBonuses = ModContent.GetInstance<NullandVoidClientConfig>().MaxStyleBonuses;
			styleBonusFadeTime = ModContent.GetInstance<NullandVoidClientConfig>().StyleBonusFadeTime * 60;
			styleMeterHideTime = ModContent.GetInstance<NullandVoidClientConfig>().StyleMeterHideTime * 60;
			styleMeterEaseSpeed = ModContent.GetInstance<NullandVoidClientConfig>().StyleMeterEaseSpeed;
		}

		public override void OnInitialize() {
			area = new UIElement();
			area.Width.Set(280,0);
			area.Height.Set(200,0);
			area.Left.Set(-295, 1);
			area.Top.Set(-215, 1);

			styleRankText = new UIText("");
			styleRankText.Left.Set(0, 0);
			styleRankText.Top.Set(-35, 1);
			
			area.Append(styleRankText);
			Append(area);
			
			ChangeConfig();
			idleTime = 300;
		}

		public override void Draw(SpriteBatch spriteBatch) {
			if (Main.playerInventory || !showStyleMeterUI) {
				return;
			}
			
			StylePlayer stylePlayer = Main.LocalPlayer.GetModPlayer<StylePlayer>();
			StyleRank styleRank = stylePlayer.PlayerStyleRank;
			Rectangle areaRect = area.GetInnerDimensions().ToRectangle();
			
			/*
			if (stylePlayer.StylePoints == 0) {
				if (idleTime == 300) {
					return;
				}
				idleTime++;
				area.Top.Set(areaVisual, 1);
			} else {
				idleTime = 0;
				area.Top.Set(areaVisual, 1);
			}
			
			Main.NewText((idleTime, areaRect.Top, areaVisual));
			*/

			if (styleRank.Rank > 1) {
				area.Left.Set( (int)(MathF.Sin(Main.rand.NextFloat()) * (styleRank.Rank - 1) * 2) - 295, 1);
				area.Top.Set((int)(MathF.Sin(Main.rand.NextFloat()) * (styleRank.Rank - 1) * 2) - 215, 1);
			}
			
			styleRankText.SetText(styleRank.Name, 0.6f + styleRank.Rank * 0.03f, true);
			styleRankText.TextColor = styleRank.Color;
			float shadowColor = (float)styleRank.Rank / 6;
			styleRankText.ShadowColor = styleRank.Color.MultiplyRGBA(new Color(shadowColor, shadowColor, shadowColor, 1 - shadowColor));

			float styleFill = (int)(areaRect.Width * (float)(Math.Min(stylePlayer.StylePoints, styleRank.UpperBound) - styleRank.LowerBound) / (styleRank.UpperBound - styleRank.LowerBound));
			styleVisualFill = (int)((styleVisualFill + styleFill * 0.5f) / 1.5f);
			spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(areaRect.Left, areaRect.Bottom - 50, areaRect.Width, 3), styleRank.Color);
			spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(areaRect.Left - 1, areaRect.Bottom - 51, areaRect.Width + 2, 5), Color.Black);
			spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(areaRect.Left, areaRect.Bottom - 50, styleVisualFill, 3), styleRank.Color);

			styleBonusOffset = areaRect.Bottom - 55;
			foreach (PlayerStyleBonus styleBonus in stylePlayer.PlayerStyleBonuses) {
				string text = styleBonus.Count == 1
					? styleBonus.BonusType.Name.Value
					: $"{styleBonus.BonusType.Name.Value} [x{styleBonus.Count}]";
				styleBonusOffset -= styleBonus.BonusType.Tier * 3 + 17;
				spriteBatch.DrawString(FontAssets.MouseText.Value, text, new Vector2(areaRect.Left, styleBonusOffset), Color.White, 0f, Vector2.Zero, 0.8f + 0.1f * styleBonus.BonusType.Tier, SpriteEffects.None, 0f);
			}

			base.Draw(spriteBatch);
		}
	}
	
	[Autoload(Side = ModSide.Client)]
	internal class StyleMeterSystem: ModSystem
	{
		internal StyleMeterUI StyleMeterUI;
		private UserInterface StyleMeterUserInterface;

		public override void Load() {
			StyleMeterUI = new StyleMeterUI();
			StyleMeterUserInterface = new UserInterface();
			StyleMeterUserInterface.SetState(StyleMeterUI);
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
			if (mouseTextIndex != -1) {
				layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer("NullandVoid: Style Meter", delegate {
					StyleMeterUserInterface.Draw(Main.spriteBatch, new GameTime());
					return true;
				}, InterfaceScaleType.UI));
			}
		}
	}
}