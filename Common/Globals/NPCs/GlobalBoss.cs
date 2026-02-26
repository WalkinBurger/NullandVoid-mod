using NullandVoid.Common.Players;
using Terraria;
using Terraria.ModLoader;

namespace NullandVoid.Common.Globals.NPCs
{
	public class GlobalBoss : GlobalNPC
	{
		public override bool AppliesToEntity(NPC entity, bool lateInstantiation) {
			return entity.boss;
		}

		public override void OnKill(NPC npc) {
			Main.LocalPlayer.GetModPlayer<StylePlayer>().ScorePoints = 0;
		}
	}
}