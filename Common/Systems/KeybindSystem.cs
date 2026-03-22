using Terraria.ModLoader;

namespace NullandVoid.Common.Systems
{
	public class KeybindSystem : ModSystem
	{
		public static ModKeybind ParryKeybind { get; private set; }
		public static ModKeybind MovementAbilityKeybind { get; private set; }

		public override void Load() {
			ParryKeybind = KeybindLoader.RegisterKeybind(Mod, "Parry", "F");
			MovementAbilityKeybind = KeybindLoader.RegisterKeybind(Mod, "MovementAbility", "Q");
		}

		public override void Unload() {
			ParryKeybind = null;
			MovementAbilityKeybind = null;
		}
	}
}