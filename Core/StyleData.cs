using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.Localization;

namespace NullandVoid.Core
{
	public class StyleBonus(string name, int points, int tier, StyleBonus? stackVarient = null, float stackPointsWeight = 1f)
	{
		public readonly LocalizedText Name = Language.GetText("Mods.NullandVoid.Style.Bonuses." + name);
		public readonly int Points = points;
		public readonly int Tier = tier;
		public readonly StyleBonus? StackVariant = stackVarient;
		public readonly float StackPointsWeight = stackPointsWeight;
		
		
		public static readonly StyleBonus FeatherFalling = new("FeatherFalling", 70, 2);
		public static readonly StyleBonus LongLunge = new("LongLunge", 50, 2);
		public static readonly StyleBonus MultiKill = new("MultiKill", 45, 2);
		public static readonly StyleBonus Parry = new("Parry", 50, 2);
			
		public static readonly StyleBonus Aerodynamic = new("Aerodynamic", 20, 1, null, 0.6f);
		public static readonly StyleBonus Airshot = new("Airshot", 15, 1, null, 0.75f);
		public static readonly StyleBonus FriendlyFire = new("FriendlyFire", 20, 1);
		public static readonly StyleBonus ProjectileBoost = new("ProjectileBoost", 20, 1,  null, 0.25f);
		public static readonly StyleBonus QuickDraw = new("QuickDraw", 5, 1,  null, 0);
		public static readonly StyleBonus RobinHood = new("RobinHood", 35, 1);
		public static readonly StyleBonus Uppercut = new("Uppercut", 30, 1,  null, 0.75f);
			
		public static readonly StyleBonus Kill = new("Kill", 30, 0, MultiKill);
		public static readonly StyleBonus Lunge = new("Lunge", 25, 0, null, 0.75f);
		public static readonly StyleBonus Overkill = new("Overkill", 10, 0, null, 0.5f);
		public static readonly StyleBonus Pogo = new("Pogo", 25, 0);
			
		public static readonly StyleBonus LameHealing = new("LameHealing", 100, -1);
		public static readonly StyleBonus Ouchie = new("Ouchie", 0, -1);
	}

	public readonly struct StyleTierColors()
	{
		public static readonly Dictionary<int, Color> Colors = new() {
			{ -1,  new Color(123, 37, 54) },
			{ 0,  new Color(218, 225, 229) },
			{ 1,  new Color(130, 216, 233) },
			{ 2,  new Color(85, 125, 211) },
		};
	}
	

	public class StyleRank(string name, int rank, int lowerBound, int loseThresholdFrame, int loseRate, int upperBound, Color color)
	{
		public readonly LocalizedText Name = Language.GetText("Mods.NullandVoid.Style.Ranks." + name);
		public readonly int Rank = rank;
		public readonly int LoseThresholdFrame = loseThresholdFrame;
		public readonly int LoseRate = loseRate;
		public readonly int LowerBound = lowerBound;
		public readonly int UpperBound = upperBound;
		public readonly Color Color = color;
		
		public static readonly StyleRank Null = new ("Null", 6, 1800, 2, 4, 2200, new Color(160, 160, 160, 32));
		public static readonly StyleRank Terraific = new ("Terraific", 5, 1250, 3, 4, Null.LowerBound, new Color(197, 245, 125, 64));
		public static readonly StyleRank Shattered = new ("Shattered", 4, 800, 4, 3, Terraific.LowerBound, new Color(28, 23, 41, 200));
		public static readonly StyleRank Abyssal = new ("Abyssal", 3, 450, 5, 3, Shattered.LowerBound, new Color(230, 46, 49));
		public static readonly StyleRank Ballistic = new ("Ballistic", 2, 200, 6, 2, Abyssal.LowerBound, new Color(225, 159, 62));
		public static readonly StyleRank Cool = new("Cool", 1, 50, 6, 1, Ballistic.LowerBound, new Color(42, 113, 72));
		public static readonly StyleRank Dull = new ("Dull", 0, 0, 10, 1, Cool.LowerBound, new Color(58, 73, 89));

		public static readonly StyleRank[] List = [Dull, Cool, Ballistic, Abyssal, Shattered, Terraific, Null];
	}
}