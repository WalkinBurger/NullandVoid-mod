using Terraria.ModLoader;

namespace NullandVoid.Common.Systems
{
	public class KeybindSystem : ModSystem
	{
		public static ModKeybind ParryKeybind { get; private set; }

		public override void Load() {
			ParryKeybind = KeybindLoader.RegisterKeybind(Mod, "Parry", "F");
		}

		public override void Unload() {
			ParryKeybind = null;
		}
	}
}