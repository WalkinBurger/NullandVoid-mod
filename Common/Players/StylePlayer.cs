using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using NullandVoid.Common.Globals.Items;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
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
		private int styleLoseThreshold = 7;
		private int styleLoseRate = 1;

		public int ScorePoints;

		public float WeaponFreshness;
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
			if (StylePoints < PlayerStyleRank.LowerBound) {
				PlayerStyleRank = StyleRanksList.List[PlayerStyleRank.Rank - 1];
				styleLoseThreshold = PlayerStyleRank.LoseThresholdFrame;
				styleLoseRate = PlayerStyleRank.LoseRate;
			}
			else if (PlayerStyleRank != StyleRanksList.Null && StylePoints >= PlayerStyleRank.UpperBound) {
				PlayerStyleRank = StyleRanksList.List[PlayerStyleRank.Rank + 1];
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
				WeaponFreshness = 1;
				ResetFreshnessNext = false;
			}
			else if (WeaponFreshness > 0) {
				WeaponFreshness -= MathHelper.Clamp(rawPoints * count * FreshnessDecayRate / 2048, 0, 0.05f);
				WeaponFreshness = Math.Max(WeaponFreshness, 0);
				freshnessTimer = 0;
			}
			if (Main.CurrentFrameFlags.AnyActiveBossNPC) {
				ScorePoints += calcPoints;
			}
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
			freshnessTimer++;
			styleTimer++;
			if (styleTimer >= styleLoseThreshold && StylePoints != 0) {
				StylePoints = Math.Max(StylePoints - styleLoseRate, 0);
				styleTimer = 0;
				UpdateStyleRank();
			}
			UpdateStyleBonuses();

			if (freshnessTimer >= 12) {
				WeaponFreshness = Math.Min(1f, WeaponFreshness + 0.01f);
				freshnessTimer = 0;
			}
			/*
			List<string> bonuses = [];
			foreach (PlayerStyleBonus styleBonus in PlayerStyleBonuses.ToArray()) {
				bonuses.Add(string.Join(" x ", (styleBonus.BonusType.Name, styleBonus.Count, styleBonus.TimeAlive)));
			}
			Main.NewText((StylePoints, styleLoseThreshold, styleLoseRate, PlayerStyleRank.Name, string.Join(" | ",  bonuses)));
			*/

			if (QuickDrawWindow != 0) {
				QuickDrawWindow--;
			}
		}

		public override void OnHurt(Player.HurtInfo info) {
			StylePoints -= info.Damage / 2;
			if (Main.CurrentFrameFlags.AnyActiveBossNPC) {
				ScorePoints -= info.Damage / 2;
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
						AddStyleBonus(StyleBonusesList.Uppercut);
					}
					else if (hit.DamageType == DamageClass.Ranged) {
						AddStyleBonus(StyleBonusesList.Airshot);
					}
				}

				if (Main.netMode == NetmodeID.MultiplayerClient && Main.CurrentFrameFlags.ActivePlayersCount > 1) {
					bool selfInteraction = false;
					for (int i = 0; i < Main.CurrentFrameFlags.ActivePlayersCount; i++) {
						if (!target.playerInteraction[i]) {
							continue;
						}
						if (selfInteraction) {
							AddStyleBonus(StyleBonusesList.Assist);
							break;
						}
						selfInteraction = true;
					}
				}

				if (damageDone > target.lifeMax * 3) {
					AddStyleBonus(StyleBonusesList.Overkill);
				}
				AddStyleBonus(StyleBonusesList.Kill);
			}
			if (Lunging && hit.DamageType == DamageClass.Melee) {
				Lunging = false;
				SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundMiss with { Volume = 0.75f, Pitch = 0.5f }, Player.Center);
				Player.GiveImmuneTimeForCollisionAttack(30);
				AddStyleBonus(Player.GetModPlayer<StaminaPlayer>().DashJump ? StyleBonusesList.LongLunge : StyleBonusesList.Lunge);
			}

			if (QuickDrawWindow != 0 && hit.DamageType != DamageClass.Summon) {
				AddStyleBonus(StyleBonusesList.QuickDraw);
				QuickDrawWindow = 0;
			}
		}
	}
}