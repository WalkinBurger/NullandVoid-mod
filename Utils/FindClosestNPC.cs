using System;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Terraria;

namespace NullandVoid.Utils
{
	public static partial class NullandVoidUtils
	{
		[CanBeNull]
		public static NPC FindClosestNPC(float range, Vector2 origin) {
			NPC closestNPC = null;
			range = range * range;

			foreach (NPC npc in Main.ActiveNPCs) {
				if (npc.CanBeChasedBy() && Collision.CanHit(origin, 1, 1, npc.position, npc.width, npc.height)) {
					float distance = Vector2.DistanceSquared(npc.Center, origin);
					if (distance < range) {
						range = distance;
						closestNPC = npc;
					}
				}
			}
			return closestNPC;
		}	
	}
}