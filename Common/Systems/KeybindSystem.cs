using Terraria.ModLoader;

namespace NullandVoid.Common.Systems
{
	public class KeybindSystem : ModSystem
	{
		public static ModKeybind ParryKeybind { get; private set; }
		public static ModKeybind MovementAbilityKeybind { get; private set; }
		public static ModKeybind MovementAltAbilityKeybind { get; private set; }

		public override void Load() {
			ParryKeybind = KeybindLoader.RegisterKeybind(Mod, "Parry", "F");
			MovementAbilityKeybind = KeybindLoader.RegisterKeybind(Mod, "MovementAbility", "Q");
			MovementAltAbilityKeybind = KeybindLoader.RegisterKeybind(Mod, "MovementAltAility", "E");
		}

		public override void Unload() {
			ParryKeybind = null;
			MovementAbilityKeybind = null;
			MovementAltAbilityKeybind = null;
		}
	}
}