using Terraria.ModLoader;

namespace NullandVoid.Common.Players
{
	public class SwordPlayer : ModPlayer
	{
		public int HitStyle;
		public float[] HitAngleRange =  new float[2];
		public int HitResetTimer;
		public int HitDirection;

		public override void PostUpdateMiscEffects() {
			if (HitResetTimer <= 0) {
				return;
			}
			HitResetTimer--;
			if (HitResetTimer == 0) {
				HitStyle = 0;
			}
		}
	}
}