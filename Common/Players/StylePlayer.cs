using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
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
		public StyleRank PlayerStyleRank = StyleRanksList.Dull;
		public List<PlayerStyleBonus> PlayerStyleBonuses = [];
		private int styleTimer;
		private int styleLoseThreshold = 5;
		private int styleLoseRate = 1;
		
		private int styleBonusFadeTime = 60 * ModContent.GetInstance<NullandVoidClientConfig>().StyleBonusFadeTime;

		public bool Lunging;


		public override void ResetEffects() {
			Lunging = false;
		}


		public void ChangeConfig() {
			styleBonusFadeTime = 60 * ModContent.GetInstance<NullandVoidClientConfig>().StyleBonusFadeTime;
		}
		
		public void UpdateStyleRank() {
			if (StylePoints < PlayerStyleRank.LowerBound) {
				PlayerStyleRank = StyleRanksList.List[PlayerStyleRank.Rank - 1];
				styleLoseThreshold++;
				styleLoseRate = Math.Min(PlayerStyleRank.Rank + 1, 3);
			}
			else if (PlayerStyleRank != StyleRanksList.Null && StylePoints >= PlayerStyleRank.UpperBound) {
				PlayerStyleRank = StyleRanksList.List[PlayerStyleRank.Rank + 1];
				styleLoseThreshold--;
				styleLoseRate = Math.Min(PlayerStyleRank.Rank + 1, 3);
			}
		}

		public void UpdateStyleBonuses() {
			for (int i = 0; i < PlayerStyleBonuses.Count; i++) {
				PlayerStyleBonus styleBonus = PlayerStyleBonuses[i];
				styleBonus.TimeAlive++;
				if (styleBonus.TimeAlive > styleBonusFadeTime) {
					PlayerStyleBonuses.RemoveAt(i--);
					i++;
				}
			}
		}

		public void CalcAddPoints(int rawPoints, int count, float weight) {
			StylePoints += (int)(rawPoints * (1 + Math.Log10(count) * weight));
		}
		
		public void AddStyleBonus(StyleBonus bonusType, int count = 1) {
			if (bonusType.StackVariant == null) {
				int bonusStackCount = count;
				foreach (PlayerStyleBonus styleBonus in PlayerStyleBonuses.Where(styleBonus => styleBonus.BonusType == bonusType)) {
					bonusStackCount += styleBonus.Count;
					if (styleBonus.TimeAlive > 30) {
						continue;
					}
					styleBonus.Count += count;
					CalcAddPoints(bonusType.Points, bonusStackCount, bonusType.stackPointsWeight);
					return;
				}
				PlayerStyleBonuses.Add(new PlayerStyleBonus(bonusType, count));
				CalcAddPoints(bonusType.Points, bonusStackCount, bonusType.stackPointsWeight);
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
					CalcAddPoints(bonusType.StackVariant.Points, count + 1, bonusType.StackVariant.stackPointsWeight);
					return;
				}
				if (styleBonus.BonusType == bonusType.StackVariant) {
					styleBonus.Count += count;
					CalcAddPoints(bonusType.StackVariant.Points, styleBonus.Count, bonusType.StackVariant.stackPointsWeight);
					return;
				}
			}
			if (count == 1) {
				PlayerStyleBonuses.Add(new PlayerStyleBonus(bonusType));
				CalcAddPoints(bonusType.Points, 1, bonusType.stackPointsWeight);
				return;
			}
			PlayerStyleBonuses.Add(new PlayerStyleBonus(bonusType.StackVariant, count));
			CalcAddPoints(bonusType.StackVariant.Points, count, bonusType.StackVariant.stackPointsWeight);
		}
		
		
		public override void PostUpdateMiscEffects() {
			styleTimer++;
			if (styleTimer >= styleLoseThreshold && StylePoints != 0) {
				StylePoints = Math.Max(StylePoints - styleLoseRate, 0);
				styleTimer = 0;
				UpdateStyleRank();
			}
			UpdateStyleBonuses();

			/*
			List<string> bonuses = [];
			foreach (PlayerStyleBonus styleBonus in PlayerStyleBonuses.ToArray()) {
				bonuses.Add(string.Join(" x ", (styleBonus.BonusType.Name, styleBonus.Count, styleBonus.TimeAlive)));
			}
			Main.NewText((StylePoints, styleLoseThreshold, styleLoseRate, PlayerStyleRank.Name, string.Join(" | ",  bonuses)));
			*/
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (!target.active) {
				AddStyleBonus(StyleBonusesList.Kill);
			}
			if (Lunging && hit.DamageType == DamageClass.Melee) {
				Lunging = false;
				AddStyleBonus(StyleBonusesList.Lunge);
			}
		}
	}
}