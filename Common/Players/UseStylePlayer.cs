using System;
using Terraria;
using Terraria.ModLoader;

namespace NullandVoid.Common.Players
{
	public class UseStylePlayer : ModPlayer
	{
		public int HitStyle;
		public float[] HitAngleRange =  new float[2];
		public int HitResetTimer;
		public int HitDirection;

		public float ShootAngle;
		public float OldShootAngle;

		public override void PostUpdateMiscEffects() {
			if (HitResetTimer <= 0) {
				return;
			}
			HitResetTimer--;
			if (HitResetTimer == 0) {
				HitStyle = 0;
			}
		}
		
		public void SetHit(int whoAmI, float hitAngle, int style) {
			Player player = Main.player[whoAmI];
			UseStylePlayer useStylePlayer = player.GetModPlayer<UseStylePlayer>();
			
			useStylePlayer.HitStyle = style;
			useStylePlayer.HitDirection = -Math.Sign(hitAngle);
			if (useStylePlayer.HitDirection == 0) {
				useStylePlayer.HitDirection = player.direction;
			}
			float offsetAngle = 1.4f * useStylePlayer.HitDirection;
			switch (style) {
				case 0:
					player.itemAnimationMax = (int)(player.itemAnimationMax * 1.5f);
					player.itemAnimation = player.itemAnimationMax;
					useStylePlayer.HitAngleRange = [hitAngle - offsetAngle, hitAngle + offsetAngle * 1.25f];
					break;
				case 1:
					useStylePlayer.HitAngleRange = [hitAngle - offsetAngle, hitAngle + offsetAngle];
					break;
				case 2:
					useStylePlayer.HitAngleRange = [hitAngle + offsetAngle, hitAngle - offsetAngle];
					break;
			}
			useStylePlayer.HitAngleRange[0] = useStylePlayer.HitDirection == 1 ? Math.Clamp(useStylePlayer.HitAngleRange[0], -4, 1) : Math.Clamp(useStylePlayer.HitAngleRange[0], -1, 4);
			useStylePlayer.HitAngleRange[1] = useStylePlayer.HitDirection == 1 ? Math.Clamp(useStylePlayer.HitAngleRange[1], -4, 1) : Math.Clamp(useStylePlayer.HitAngleRange[1], -1, 4);
			
		}
	}
}