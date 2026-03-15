using Terraria;
using Terraria.ModLoader;

namespace NullandVoid.Common.Globals.NPCs
{
	public class AdjustStatsNPC : GlobalNPC
	{
		public override void ApplyDifficultyAndPlayerScaling(NPC npc, int numPlayers, float balance, float bossAdjustment) {
			npc.damage *= 2;
			npc.lifeMax = (int)(npc.lifeMax * 1.5f);
		}
	}
}