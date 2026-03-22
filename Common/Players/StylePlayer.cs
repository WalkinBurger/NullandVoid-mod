using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using NullandVoid.Common.Globals.Items;
using NullandVoid.Core;
using NullandVoid.Utils;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace NullandVoid.Common.Players
{
	public class PlayerStyleBonus(StyleBonus bonusType, int count = 1)
	{
		public StyleBonus BonusType { get; } = bonusType;
		public int Count { get; set; } = count;
		public int TimeAlive { get; set; }
	}
	
	
	public class StylePlayer : ModPlayer
	{
		public int StylePoints;
		public StyleRank PlayerStyleRank = StyleRank.Dull;
		public List<PlayerStyleBonus> PlayerStyleBonuses = [];
		private int styleTimer;
		private int styleLoseThreshold = 7;
		private int styleLoseRate = 1;

		public int ScorePoints;

		public float WeaponFreshness = 1f;
		public bool ResetFreshnessNext;
		public float FreshnessDecayRate;
		private int freshnessTimer;
		
		public bool Lunging;
		public int QuickDrawWindow;
		

		public override void ResetEffects() {
			Lunging = false;
			FreshnessDecayRate = 1f;
		}

		public override void UpdateDead() {
			StylePoints = 0;
			if (Main.CurrentFrameFlags.AnyActiveBossNPC) {
				ScorePoints /= 2;
			}
		}


		public void UpdateStyleRank() {
			if (StylePoints < PlayerStyleRank.LowerBound && PlayerStyleRank != StyleRank.Dull) {
				PlayerStyleRank = StyleRank.List[PlayerStyleRank.Rank - 1];
				styleLoseThreshold = PlayerStyleRank.LoseThresholdFrame;
				styleLoseRate = PlayerStyleRank.LoseRate;
			}
			else if (PlayerStyleRank != StyleRank.Null && StylePoints >= PlayerStyleRank.UpperBound) {
				PlayerStyleRank = StyleRank.List[PlayerStyleRank.Rank + 1];
				styleLoseThreshold = PlayerStyleRank.LoseThresholdFrame;
				styleLoseRate = PlayerStyleRank.LoseRate;
			}
		}

		public void UpdateStyleBonuses() {
			for (int i = 0; i < PlayerStyleBonuses.Count; i++) {
				PlayerStyleBonus styleBonus = PlayerStyleBonuses[i];
				styleBonus.TimeAlive++;
				if (styleBonus.TimeAlive > ModContent.GetInstance<NullandVoidClientConfig>().StyleBonusFadeTime * 60) {
					PlayerStyleBonuses.RemoveAt(i--);
					i++;
				}
			}
		}

		public void CalcAddPoints(int rawPoints, int count, float weight) {
			int calcPoints = (int)(rawPoints * (WeaponFreshness + 0.25f) * (1 + MathF.Log10(count) * weight));
			StylePoints += calcPoints;
			if (ResetFreshnessNext) {
				WeaponFreshness += 0.25f;
				ResetFreshnessNext = false;
			}
			else if (WeaponFreshness > 0) {
				WeaponFreshness -= MathHelper.Clamp(rawPoints * count * FreshnessDecayRate / 2048, 0, 0.2f);
				WeaponFreshness = Math.Max(WeaponFreshness, 0);
				freshnessTimer = 0;
			}
			if (Main.CurrentFrameFlags.AnyActiveBossNPC) {
				ScorePoints += calcPoints;
			}
		}

		public void CalcMinusPoints(int points, int count) {
			StylePoints = Math.Max(0, StylePoints - points * count);
			UpdateStyleRank();
			if (Main.CurrentFrameFlags.AnyActiveBossNPC) {
				ScorePoints -= points * count;
			}
		}
		
		public void AddStyleBonus(StyleBonus bonusType, int count = 1) {
			if (bonusType.Tier == -1) {
				if (bonusType.Points != 0) {
					CalcMinusPoints(bonusType.Points, count);
				}
				foreach (PlayerStyleBonus styleBonus in PlayerStyleBonuses.Where(styleBonus => styleBonus.BonusType == bonusType)) {
					styleBonus.Count += count;
					return;
				}
				PlayerStyleBonuses.Add(new PlayerStyleBonus(bonusType, count));
				return;
			}
			if (bonusType.StackVariant == null) {
				int bonusStackCount = count;
				foreach (PlayerStyleBonus styleBonus in PlayerStyleBonuses.Where(styleBonus => styleBonus.BonusType == bonusType)) {
					bonusStackCount += styleBonus.Count;
					if (styleBonus.TimeAlive > 30) {
						continue;
					}
					styleBonus.Count += count;
					CalcAddPoints(bonusType.Points, bonusStackCount, bonusType.StackPointsWeight);
					return;
				}
				PlayerStyleBonuses.Add(new PlayerStyleBonus(bonusType, count));
				CalcAddPoints(bonusType.Points, bonusStackCount, bonusType.StackPointsWeight);
				return;
			}
			for (int i = 0; i < PlayerStyleBonuses.Count; i++) {
				PlayerStyleBonus styleBonus = PlayerStyleBonuses[i];
				if (styleBonus.TimeAlive > 30) {
					continue;
				}
				if (styleBonus.BonusType == bonusType) {
					PlayerStyleBonuses.RemoveAt(i);
					PlayerStyleBonuses.Add(new PlayerStyleBonus(bonusType.StackVariant, count + 1));
					CalcAddPoints(bonusType.StackVariant.Points, count + 1, bonusType.StackVariant.StackPointsWeight);
					return;
				}
				if (styleBonus.BonusType == bonusType.StackVariant) {
					styleBonus.Count += count;
					CalcAddPoints(bonusType.StackVariant.Points, styleBonus.Count, bonusType.StackVariant.StackPointsWeight);
					return;
				}
			}
			if (count == 1) {
				PlayerStyleBonuses.Add(new PlayerStyleBonus(bonusType));
				CalcAddPoints(bonusType.Points, 1, bonusType.StackPointsWeight);
				return;
			}
			PlayerStyleBonuses.Add(new PlayerStyleBonus(bonusType.StackVariant, count));
			CalcAddPoints(bonusType.StackVariant.Points, count, bonusType.StackVariant.StackPointsWeight);
		}
		

		public override void UpdateLifeRegen() {
			Player.lifeRegen += PlayerStyleRank.Rank * 2;
		}


		public override void PostUpdateMiscEffects() {
			Player.GetDamage(DamageClass.Generic) += (float)(PlayerStyleRank.Rank - 2) / 8;
			freshnessTimer++;
			styleTimer++;
			if (styleTimer >= styleLoseThreshold && StylePoints != 0) {
				StylePoints = Math.Max(StylePoints - styleLoseRate, 0);
				styleTimer = 0;
				UpdateStyleRank();
			}
			UpdateStyleBonuses();

			if (freshnessTimer >= 15) {
				WeaponFreshness = Math.Min(1f, WeaponFreshness + 0.01f);
				freshnessTimer = 0;
			}

			if (QuickDrawWindow != 0) {
				QuickDrawWindow--;
			}
		}

		public override void OnHurt(Player.HurtInfo info) {
			AddStyleBonus(StyleBonus.Ouchie);
			CalcMinusPoints(info.Damage / 2, 1);
		}

		public void CheckQuickDraw(NPC.HitInfo hit) {
			if (QuickDrawWindow != 0 && hit.DamageType != DamageClass.Summon) {
				AddStyleBonus(StyleBonus.QuickDraw);
				QuickDrawWindow = 0;
			}
		}
		
		public void CheckQuickDraw() {
			if (QuickDrawWindow != 0) {
				AddStyleBonus(StyleBonus.QuickDraw);
				QuickDrawWindow = 0;
			}
		}
		
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (!target.active) {
				bool airKill = true;
				for (int i = 0; i < 2; i++) {
					if (WorldGen.SolidTile2( Framing.GetTileSafely(target.Bottom.ToTileCoordinates() + new Point(0, i)))) {
						airKill = false;
						break;
					}
				}
				if (airKill) {
					if (Player.HeldItem.useStyle == SwordGlobalItem.SwordUseStyle) {
						AddStyleBonus(StyleBonus.Uppercut);
					}
					else if (hit.DamageType == DamageClass.Ranged) {
						AddStyleBonus(StyleBonus.Airshot);
					}
				}

				if (damageDone > target.lifeMax) {
					AddStyleBonus(StyleBonus.Overkill);
				}
				AddStyleBonus(StyleBonus.Kill);
			}
			
			if (Lunging && hit.DamageType == DamageClass.Melee) {
				Lunging = false;
				SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundMiss with { Volume = 0.75f, Pitch = 0.5f, MaxInstances = 8 }, Player.Center);
				Player.GiveImmuneTimeForCollisionAttack(30);
				AddStyleBonus(Player.GetModPlayer<MovementClassPlayer>().DashJump ? StyleBonus.LongLunge : StyleBonus.Lunge);
			}

			CheckQuickDraw(hit);
		}
	}
}