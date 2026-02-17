using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NullandVoid.Common.Players;
using NullandVoid.Utils;
using ReLogic.Content;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using Terraria.UI;

namespace NullandVoid.Common.UIs
{
	internal class StyleMeterUI : UIState
	{
		internal UIElement area;
		private UIText styleRankText;
		private UIText styleRankShadow;
		private int textShake;
		private int styleVisualFill;
		private int idleTime;
		private int hidFrame;
		private int styleBonusOffset;
		private Texture2D glowBar;
		
		internal bool showStyleMeterUI;
		internal int maxStyleBonuses;
		internal int styleMeterHideTime;
		internal float styleMeterEaseSpeed;


		public override void OnInitialize() {
			styleMeterHideTime = ModContent.GetInstance<NullandVoidClientConfig>().StyleMeterHideTime;
			idleTime = styleMeterHideTime;

			glowBar = ModContent.Request<Texture2D>("NullandVoid/Assets/Textures/GlowBar", AssetRequestMode.ImmediateLoad).Value;
			
			area = new UIElement();
			area.Width.Set(280,0);
			area.Height.Set(200,0);
			area.Left.Set(-295, 1);
			area.Top.Set(0, 1);

			styleRankText = new UIText("");
			styleRankText.Left.Set(0, 0);
			styleRankText.Top.Set(-35, 1);
			
			styleRankShadow = new UIText("");
			styleRankShadow.Left.Set(5, 0);
			styleRankShadow.Top.Set(-33, 1);
			styleRankShadow.ShadowColor = new Color(0, 0, 0, 0.8f);
			
			area.Append(styleRankShadow);
			area.Append(styleRankText);
			Append(area);
		}

		public override void Draw(SpriteBatch spriteBatch) {
			if (Main.playerInventory || !showStyleMeterUI) {
				return;
			}
			
			Rectangle areaRect = area.GetInnerDimensions().ToRectangle();
			StylePlayer stylePlayer = Main.LocalPlayer.GetModPlayer<StylePlayer>();
			StyleRank styleRank = stylePlayer.PlayerStyleRank;

			if (styleMeterHideTime != 0) {
				if (stylePlayer.StylePoints == 0) {
					if (idleTime == styleMeterHideTime) {
						return;
					}
					idleTime++;
					if (idleTime > styleMeterHideTime - 40) {
						area.Top.Set((int)MathHelper.Lerp(-215, 0, Easing.EaseInPow((float)(idleTime - styleMeterHideTime + 40) / 40, 3)), 1);
					}
				}
				else if (idleTime > 0) {
					if (idleTime > styleMeterHideTime - 39 || idleTime < 21) {
						idleTime = Math.Min(idleTime - 1, 20);
						area.Top.Set((int)MathHelper.Lerp(-215, 0, Easing.EaseInPow((float)idleTime / 20, 3)), 1);
					}
					else {
						idleTime = 0;
					}
				}
			}
			
			if (styleRank.Rank > 1) {
				area.Left.Set( (int)(MathF.Sin(Main.rand.NextFloat()) * (styleRank.Rank - 1) * 2) - 295, 1);
				area.Top.Set((int)(MathF.Sin(Main.rand.NextFloat()) * (styleRank.Rank - 1) * 2) - 215, 1);
			}
			
			styleRankText.SetText(styleRank.Name, 0.6f + styleRank.Rank * 0.03f, true);
			styleRankText.TextColor = styleRank.Color;
			styleRankText.ShadowColor = styleRank.Color.MultiplyRGBA(new Color(0.2f, 0.2f, 0.2f, 1 - ((float)styleRank.Rank / 6)));

			styleRankShadow.SetText(styleRank.Name, 0.6f + styleRank.Rank * 0.03f, true);
			styleRankShadow.TextColor = new Color(0, 0, 0, (float)(styleRank.Rank + 6) / 16);
			
			float styleFill = (int)(areaRect.Width * (float)(Math.Min(stylePlayer.StylePoints, styleRank.UpperBound) - styleRank.LowerBound) / (styleRank.UpperBound - styleRank.LowerBound));
			styleVisualFill = (int)((styleVisualFill + styleFill * 0.5f) / 1.5f);
			spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(areaRect.Left, areaRect.Bottom - 50, areaRect.Width, 3), styleRank.Color);
			spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(areaRect.Left - 1, areaRect.Bottom - 51, areaRect.Width + 2, 5), Color.Black);
			spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(areaRect.Left, areaRect.Bottom - 50, styleVisualFill, 3), styleRank.Color);
			base.Draw(spriteBatch);

			styleBonusOffset = areaRect.Bottom - 55;
			for (int i = Math.Max(0, stylePlayer.PlayerStyleBonuses.Count - maxStyleBonuses); i < stylePlayer.PlayerStyleBonuses.Count; i++) {
				PlayerStyleBonus styleBonus = stylePlayer.PlayerStyleBonuses[i];
				string text = styleBonus.Count == 1
					? styleBonus.BonusType.Name.Value
					: $"{styleBonus.BonusType.Name.Value} [x{styleBonus.Count}]";
				styleBonusOffset -= styleBonus.BonusType.Tier * 4 + 17;
				Color textColor;
				switch (styleBonus.BonusType.Tier) {
					case 0:
					default:
						textColor = new Color(218, 225, 229);
						break;
					case 1:
						textColor = new Color(130, 216, 233);
						break;
					case 2:
						textColor = new Color(85, 125, 211);
						spriteBatch.Draw(glowBar, new Rectangle(areaRect.Left, styleBonusOffset - 2, areaRect.Width, 27), new Color(0.1f, 0.15f, 0.3f, 0));
						// spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.UIScaleMatrix);
						// GameShaders.Misc["NullandVoid:StyleBonusEffect"].UseImage1(ModContent.Request<Texture2D>("NullandVoid/Assets/Textures/Slash"));
						// GameShaders.Misc["NullandVoid:StyleBonusEffect"].Apply();
						break;
				}

				Color shadowColor = textColor.MultiplyRGBA(new Color(0.25f, 0.25f, 0.25f, 0.75f));
				spriteBatch.DrawString(FontAssets.MouseText.Value, text, new Vector2(areaRect.Left + 1, styleBonusOffset - 1), shadowColor, 0f, Vector2.Zero, 0.8f + 0.15f * styleBonus.BonusType.Tier, SpriteEffects.None, 0f);
				spriteBatch.DrawString(FontAssets.MouseText.Value, text, new Vector2(areaRect.Left + 2, styleBonusOffset + 1), shadowColor, 0f, Vector2.Zero, 0.8f + 0.15f * styleBonus.BonusType.Tier, SpriteEffects.None, 0f);
				spriteBatch.DrawString(FontAssets.MouseText.Value, text, new Vector2(areaRect.Left, styleBonusOffset), textColor, 0f, Vector2.Zero, 0.8f + 0.15f * styleBonus.BonusType.Tier, SpriteEffects.None, 0f);
				// spriteBatch.End();
				// spriteBatch.Begin(0, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix /*Main.GameViewMatrix.TransformationMatrix*/);
			}
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
			ChangeConfig();
		}
		
		public void ChangeConfig() {
			StyleMeterUI.showStyleMeterUI = ModContent.GetInstance<NullandVoidClientConfig>().ShowStyleMeterUI;
			StyleMeterUI.maxStyleBonuses = ModContent.GetInstance<NullandVoidClientConfig>().MaxStyleBonuses;
			StyleMeterUI.styleMeterHideTime = ModContent.GetInstance<NullandVoidClientConfig>().StyleMeterHideTime * 60;
			StyleMeterUI.styleMeterEaseSpeed = ModContent.GetInstance<NullandVoidClientConfig>().StyleMeterEaseSpeed;
			
			StyleMeterUI.area.Top.Set(-215, 1);
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