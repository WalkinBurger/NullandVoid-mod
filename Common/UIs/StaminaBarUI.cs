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
		private static Asset<Texture2D> bar;
		private static Asset<Texture2D> barFull;
		private static Asset<Texture2D> barUsing;
		private static Asset<Texture2D> barUsingFull;
		private Rectangle areaRect;
		private UIElement area;

		public override void OnInitialize() {
			area = new UIElement();
			area.Left.Set(-450, 1);
			area.Top.Set(15, 0);

			bar = ModContent.Request<Texture2D>("NullandVoid/Common/UIs/StaminaBar", AssetRequestMode.ImmediateLoad);
			barFull = ModContent.Request<Texture2D>("NullandVoid/Common/UIs/StaminaBarFull", AssetRequestMode.ImmediateLoad);
			barUsing = ModContent.Request<Texture2D>("NullandVoid/Common/UIs/StaminaBarUsing", AssetRequestMode.ImmediateLoad);
			barUsingFull = ModContent.Request<Texture2D>("NullandVoid/Common/UIs/StaminaBarUsingFull", AssetRequestMode.ImmediateLoad);

			Append(area);
		}

		public override void Draw(SpriteBatch spriteBatch) {
			if (!ModContent.GetInstance<NullandVoidClientConfig>().ShowStaminaUI) {
				return;
			}

			base.Draw(spriteBatch);

			areaRect = area.GetInnerDimensions().ToRectangle();
			MovementClassPlayer movementClassPlayer = Main.LocalPlayer.GetModPlayer<MovementClassPlayer>();
			float staminaRatio = (float)movementClassPlayer.StatStamina / 20;
			bool ability = movementClassPlayer.UsingAbility || movementClassPlayer.Cooldown > 0;
			bool altAbility = movementClassPlayer.UsingAltAbility || movementClassPlayer.CooldownAlt > 0;
			int halfHeight = (bar.Height() / 2) + 1;
			for (int i = 0; i < movementClassPlayer.StatStaminaMax / 20; i++) {
				Vector2 origin = new(areaRect.Left - i * 42, areaRect.Top);
				if (staminaRatio >= i + 1) {
					spriteBatch.Draw(altAbility ? barUsingFull.Value : barFull.Value, origin, Color.White);
					if (ability != altAbility) {
						spriteBatch.Draw(ability ? barUsingFull.Value : barFull.Value, origin, new Rectangle(0, 0, bar.Width(), halfHeight), Color.White);
					}
				}
				else {
					spriteBatch.Draw(altAbility ? barUsing.Value : bar.Value, origin, new Color(128, 128, 128));
					if (ability != altAbility) {
						spriteBatch.Draw(ability ? barUsing.Value : bar.Value, origin, new Rectangle(0, 0, bar.Width(), halfHeight), new Color(128, 128, 128));
					}

					if (!(staminaRatio >= i)) {
						continue;
					}

					int fillRatio = (int)(bar.Width() * (1 - (staminaRatio - i)));
					spriteBatch.Draw(altAbility ? barUsingFull.Value : barFull.Value, origin + new Vector2(fillRatio, 0), new Rectangle(fillRatio, 0, bar.Width() - fillRatio, bar.Height()), new Color(196, 196, 196));
					if (ability != altAbility) {
						spriteBatch.Draw(ability ? barUsingFull.Value : barFull.Value, origin + new Vector2(fillRatio, 0), new Rectangle(fillRatio, 0, bar.Width() - fillRatio, halfHeight), new Color(196, 196, 196));
					}
				}
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

		public override void Unload() {
			StaminaBarUI = null;
			StaminaBarUserInterface = null;
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