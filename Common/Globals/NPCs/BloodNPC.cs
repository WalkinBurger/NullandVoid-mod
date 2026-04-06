using System;
using Microsoft.Xna.Framework;
using NullandVoid.Common.Systems;
using NullandVoid.Content.Dusts;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace NullandVoid.Common.Globals.NPCs
{
	[Autoload(Side = ModSide.Client)]
	internal class BloodNPC : GlobalNPC
	{
		public override void HitEffect(NPC npc, NPC.HitInfo hit) {
			if (Main.dedServ || !ModContent.GetInstance<NullandVoidClientConfig>().ShowBloodSpill || !ChildSafety.Disabled) {
				return;
			}

			Color color;
			if (npc.color == Color.Transparent) {
				color = BloodSets.GetBloodColor(npc.type);
			}
			else {
				color = npc.color * 0.5f;
				color.A = 255;
			}

			int amount = Math.Min(6, (int)(ModContent.GetInstance<NullandVoidClientConfig>().BloodAmount * ((float)hit.Damage * (npc.boss ? 128 : 2) / npc.lifeMax + 0.5f)));
			float scale = ModContent.GetInstance<NullandVoidClientConfig>().BloodSize * ((float)(npc.width + npc.height) / 512 + 0.7f);
			for (int i = 0; i < amount; i++) {
				Dust blood = Dust.NewDustDirect((npc.Center + npc.position) / 2 + npc.velocity * 2, npc.frame.Width / 2, npc.frame.Height / 2, ModContent.DustType<SpilledBlood>(), hit.HitDirection * npc.velocity.X / 2, npc.velocity.Y / 2, Scale: scale);
				blood.color = color;
			}
		}
	}
}