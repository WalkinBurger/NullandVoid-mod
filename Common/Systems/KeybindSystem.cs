using Terraria.ModLoader;

namespace NullandVoid.Common.Systems
{
	public class KeybindSystem : ModSystem
	{
		public static ModKeybind ParryKeybind { get; private set; }
		public static ModKeybind DashKeybind { get; private set; }

		public override void Load() {
			ParryKeybind = KeybindLoader.RegisterKeybind(Mod, "Parry", "F");
			DashKeybind = KeybindLoader.RegisterKeybind(Mod, "Dash", "Q");
		}

		public override void Unload() {
			ParryKeybind = null;
			DashKeybind = null;
		}
	}
}