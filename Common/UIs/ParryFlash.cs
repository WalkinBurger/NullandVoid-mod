using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NullandVoid.Common.Players;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace NullandVoid.Common.UIs
{
	internal class ParryFlash : UIState
	{
		public override void Draw(SpriteBatch spriteBatch) {
			Texture2D freezeImage = Main.LocalPlayer.GetModPlayer<ParryPlayer>().FreezeImage;
			Rectangle freezeRect = new(-15, -15, freezeImage.Width + 30, freezeImage.Height + 30);
			spriteBatch.Draw(freezeImage, freezeRect, Color.White);
		}
	}

	[Autoload(Side = ModSide.Client)]
	internal class ParryFlashSystem : ModSystem
	{
		internal ParryFlash ParryFlash;
		private UserInterface ParryFlashUserInterface;

		public override void Load() {
			ParryFlash = new ParryFlash();
			ParryFlashUserInterface = new UserInterface();
		}

		public void Show() {
			ParryFlashUserInterface.SetState(ParryFlash);
		}

		public void Hide() {
			ParryFlashUserInterface.SetState(null);
		}

		public override void UpdateUI(GameTime gameTime) {
			ParryFlashUserInterface?.Update(gameTime);
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
			if (mouseTextIndex != -1) {
				layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer("NullandVoid: Parry Flash", delegate {
					ParryFlashUserInterface.Draw(Main.spriteBatch, new GameTime());
					return true;
				}, InterfaceScaleType.UI));
			}
		}
	}
}