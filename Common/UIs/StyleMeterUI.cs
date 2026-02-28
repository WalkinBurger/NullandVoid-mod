using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NullandVoid.Common.Players;
using NullandVoid.Core;
using NullandVoid.Utils;
using ReLogic.Content;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.Graphics.Shaders;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace NullandVoid.Common.UIs
{
	internal class StyleMeterUI : UIState
	{
		private UIElement area;
		private UIText styleRankText;
		private UIText styleRankShadow;
		private UIText freshnessText;
		private int textShake;
		private int styleVisualFill;
		private int idleTime;
		private int hidFrame;
		private int styleBonusOffset;
		private Texture2D glowBar;
		private Texture2D bgBottom;
		private Texture2D bgMiddle;
		private Texture2D bgTop;
		private Texture2D freshnessBar;
		private Asset<Texture2D>[] effectUImages;
		

		public override void OnInitialize() {
			idleTime = 600;
			effectUImages = [
				ModContent.Request<Texture2D>("NullandVoid/Assets/Textures/Crack", AssetRequestMode.ImmediateLoad),
				ModContent.Request<Texture2D>("NullandVoid/Assets/Textures/TerraGradient", AssetRequestMode.ImmediateLoad),
				ModContent.Request<Texture2D>("NullandVoid/Assets/Textures/Turbulence", AssetRequestMode.ImmediateLoad),
			];

			glowBar = ModContent.Request<Texture2D>("NullandVoid/Assets/Textures/GlowBar", AssetRequestMode.ImmediateLoad).Value;

			bgBottom = ModContent.Request<Texture2D>("NullandVoid/Common/UIs/StyleBackgroundBottom", AssetRequestMode.ImmediateLoad).Value;
			bgMiddle = ModContent.Request<Texture2D>("NullandVoid/Common/UIs/StyleBackgroundMiddle", AssetRequestMode.ImmediateLoad).Value;
			bgTop = ModContent.Request<Texture2D>("NullandVoid/Common/UIs/StyleBackgroundTop", AssetRequestMode.ImmediateLoad).Value;
			
			freshnessBar = ModContent.Request<Texture2D>("NullandVoid/Common/UIs/WeaponFreshnessBar", AssetRequestMode.ImmediateLoad).Value;
			
			area = new UIElement();
			area.Width.Set(280,0);
			area.Height.Set(200,0);
			area.Left.Set(-315, 1);
			area.Top.Set(0, 1);

			styleRankText = new UIText("");
			styleRankText.Left.Set(0, 0);
			styleRankText.Top.Set(-35, 1);
			
			styleRankShadow = new UIText("");
			styleRankShadow.Left.Set(5, 0);
			styleRankShadow.Top.Set(-33, 1);
			styleRankShadow.TextColor = new Color(0, 0, 0, 0.5f);
			styleRankShadow.ShadowColor = new Color(0, 0, 0, 0.8f);

			freshnessText = new UIText("");
			freshnessText.Left.Set(-65, 0);
			freshnessText.Top.Set(-30, 1);
			freshnessText.TextColor = new Color(240, 142, 57);
			freshnessText.ShadowColor = new Color(82, 30, 58);
			
			area.Append(freshnessText);
			area.Append(styleRankShadow);
			area.Append(styleRankText);
			Append(area);
		}

		public override void Draw(SpriteBatch spriteBatch) {
			if (Main.playerInventory || !ModContent.GetInstance<NullandVoidClientConfig>().ShowStyleMeterUI) {
				return;
			}
			styleRankText.Top.Set(-40, 1);
			styleRankShadow.Top.Set(-39, 1);
			
			area.Left.Set(-315, 1);
			Rectangle areaRect = area.GetInnerDimensions().ToRectangle();
			StylePlayer stylePlayer = Main.LocalPlayer.GetModPlayer<StylePlayer>();
			StyleRank styleRank = stylePlayer.PlayerStyleRank;

			int styleMeterHideTime = ModContent.GetInstance<NullandVoidClientConfig>().StyleMeterHideTime * 60;

			if (styleMeterHideTime != 0) {
				if (stylePlayer.StylePoints == 0) {
					if (idleTime >= styleMeterHideTime) {
						return;
					}
					idleTime++;
					if (idleTime > styleMeterHideTime - 40) {
						area.Top.Set((int)MathHelper.Lerp(-215, 0, NullandVoidUtils.EaseInPow((float)(idleTime - styleMeterHideTime + 40) / 40, 3)), 1);
					}
				}
				else if (idleTime > 0) {
					if ((idleTime > styleMeterHideTime - 39 || idleTime < 21)) {
						idleTime = Math.Min(idleTime - 1, 20);
						area.Top.Set((int)MathHelper.Lerp(-215, 0, NullandVoidUtils.EaseInPow((float)idleTime / 20, 3)), 1);
					}
					else {
						idleTime = 0;
					}
				}
			}
			
			if (styleRank.Rank > 1) {
				area.Left.Set( (int)(MathF.Sin(Main.rand.NextFloat()) * (styleRank.Rank - 1) * 2) - 315, 1);
				area.Top.Set((int)(MathF.Sin(Main.rand.NextFloat()) * (styleRank.Rank - 1) * 2) - 215, 1);
			}
			
			spriteBatch.Draw(bgBottom, new Vector2(areaRect.Left - 31, areaRect.Bottom - bgBottom.Height + 1), Color.White);

			int freshness = (int)(stylePlayer.WeaponFreshness * 56);
			spriteBatch.Draw(freshnessBar, new Rectangle(areaRect.Left - 23, areaRect.Bottom - 9 - freshness, 4, freshness), new Rectangle(0, 56 - freshness, 4, freshness), Color.White);

			freshnessText.SetText($"x{MathF.Round(stylePlayer.WeaponFreshness + 0.25f, 2):0.00}");
			freshnessText.Left.Set(-78, 0);
			freshnessText.Top.Set(-20, 1);
			
			styleBonusOffset = areaRect.Bottom - 65;
			for (int i = Math.Max(0, stylePlayer.PlayerStyleBonuses.Count - ModContent.GetInstance<NullandVoidClientConfig>().MaxStyleBonuses); i < stylePlayer.PlayerStyleBonuses.Count; i++) {
				PlayerStyleBonus styleBonus = stylePlayer.PlayerStyleBonuses[i];
				Color textColor = StyleTierColors.Colors[styleBonus.BonusType.Tier];
				float styleBonusFading = 1;
				if (ModContent.GetInstance<NullandVoidClientConfig>().StyleMeterEase) {
					styleBonusFading = NullandVoidUtils.FadeInOut((float)styleBonus.TimeAlive / (ModContent.GetInstance<NullandVoidClientConfig>().StyleBonusFadeTime * 60), 0.05f, 2);
					styleBonusOffset -= (int)((styleBonus.BonusType.Tier * 4 + 17) * styleBonusFading);
					if (styleBonusFading != 1) {
						textColor = textColor.MultiplyRGB(new Color(styleBonusFading, styleBonusFading, styleBonusFading));
						textColor.A = (byte)Math.Min(255, (styleBonusFading) * 256);
					}
				}
				else {
					styleBonusOffset -= styleBonus.BonusType.Tier * 4 + 20;
				}
				
				string text = styleBonus.Count == 1
					? styleBonus.BonusType.Name.Value
					: $"{styleBonus.BonusType.Name.Value} [x{styleBonus.Count}]";
				
				if (styleBonus.BonusType.Tier > 1) {
					spriteBatch.Draw(glowBar, new Rectangle(areaRect.Left, styleBonusOffset - 2, areaRect.Width, 27), textColor.MultiplyRGBA(new Color(0.3f, 0.3f, 0.3f, 0)));
				}

				spriteBatch.Draw(bgMiddle, new Rectangle(areaRect.Left - 13, styleBonusOffset, bgMiddle.Width, (int)((17 + styleBonus.BonusType.Tier * 4) * styleBonusFading)), Color.White);
				Color shadowColor = textColor.MultiplyRGB(new Color(0.25f, 0.25f, 0.25f, 0.9f));
				spriteBatch.DrawString(FontAssets.MouseText.Value, text, new Vector2(areaRect.Left + 2 - styleBonus.BonusType.Tier * 2, styleBonusOffset + 2), shadowColor, 0f, Vector2.Zero, 0.8f + 0.15f * styleBonus.BonusType.Tier, SpriteEffects.None, 0f);
				spriteBatch.DrawString(FontAssets.MouseText.Value, text, new Vector2(areaRect.Left - styleBonus.BonusType.Tier * 2, styleBonusOffset), textColor, 0f, Vector2.Zero, 0.8f + 0.15f * styleBonus.BonusType.Tier, SpriteEffects.None, 0f);
			}
			
			styleRankText.TextColor = styleRank.Color;
			styleRankText.SetText(styleRank.Name, 0.6f + styleRank.Rank * 0.03f, true);
			spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(areaRect.Left - 1, areaRect.Bottom - 61, areaRect.Width + 2, 6), Color.Black);
			
			if (styleRank.Rank > 3) { 
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.UIScaleMatrix);
				GameShaders.Misc["NullandVoid:StyleBonusEffect"].UseImage1(effectUImages[styleRank.Rank - 4]);
				GameShaders.Misc["NullandVoid:StyleBonusEffect"].Shader.Parameters["uUIPosition"].SetValue(new Vector2(areaRect.Left - Main.LocalPlayer.velocity.X * 10,  areaRect.Top - 35 - Main.LocalPlayer.velocity.Y * 10));
				GameShaders.Misc["NullandVoid:StyleBonusEffect"].Apply();
				 
				spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(areaRect.Left, areaRect.Bottom - 60, areaRect.Width, 4), Color.Black);
				styleRankText.Draw(spriteBatch);
				 
				spriteBatch.End();
				spriteBatch.Begin(0, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
				freshnessText.Draw(spriteBatch);
				
				spriteBatch.DrawString(FontAssets.DeathText.Value, styleRank.Name.Value, new Vector2(areaRect.Left - 5, areaRect.Bottom - 50), styleRank.Color, 0f, Vector2.Zero, 0.6f + styleRank.Rank * 0.03f, SpriteEffects.None, 0f);
			}
			else {
				styleRankText.TextColor = styleRank.Color;
				styleRankShadow.SetText(styleRank.Name, 0.6f + styleRank.Rank * 0.03f, true);
				spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(areaRect.Left, areaRect.Bottom - 60, areaRect.Width, 4), styleRank.Color.MultiplyRGB(new Color(0.5f, 0.5f, 0.5f)));
				base.Draw(spriteBatch);
			}
			
			float styleFill = (int)(areaRect.Width * (float)(Math.Min(stylePlayer.StylePoints, styleRank.UpperBound) - styleRank.LowerBound) / (styleRank.UpperBound - styleRank.LowerBound));
			styleVisualFill = (int)((styleVisualFill + styleFill * 0.5f) / 1.5f);
			Color barColor = styleRank.Color;
			barColor.A = 200;
			spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(areaRect.Left, areaRect.Bottom - 60, styleVisualFill, 4), barColor);
			
			spriteBatch.Draw(bgTop, new Vector2(areaRect.Left - 13, styleBonusOffset - bgTop.Height), Color.White);
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