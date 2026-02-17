using Microsoft.Xna.Framework;
using Terraria.Localization;

namespace NullandVoid.Common
{
	public class StyleBonus(string name, int points, int tier, StyleBonus? stackVarient = null, float stackPointsWeight = 1f)
	{
		public readonly LocalizedText Name = Language.GetText("Mods.NullandVoid.Style.Bonuses." + name);
		public readonly int Points = points;
		public readonly int Tier = tier;
		public readonly StyleBonus? StackVariant = stackVarient;
		public readonly float stackPointsWeight = stackPointsWeight;
	}
	
	internal static class StyleBonusesList
	{
		internal static readonly StyleBonus MultiKill = new ("MultiKill", 100, 2, null, 1.2f);
		
		internal static readonly StyleBonus Kill = new("Kill", 50, 0, MultiKill, 1.15f);
		internal static readonly StyleBonus Parry = new ("Parry", 75, 2, null, 1.25f);
		internal static readonly StyleBonus ProjectileBoost = new ("ProjectileBoost", 20, 1,  null, 0.5f);
		internal static readonly StyleBonus Lunge = new ("Lunge", 25, 0);
	}


	public class StyleRank(string name, int rank, int lowerBound, int upperBound, Color color)
	{
		public readonly LocalizedText Name = Language.GetText("Mods.NullandVoid.Style.Ranks." + name);
		public readonly int Rank = rank;
		public readonly int LowerBound = lowerBound;
		public readonly int UpperBound = upperBound;
		public readonly Color Color = color;
	}

	internal static class StyleRanksList
	{
		internal static readonly StyleRank Null = new ("Null", 6, 1800, 2200, new Color(42, 33, 52));
		internal static readonly StyleRank Terraific = new ("Terraific", 5, 1250, Null.LowerBound, new Color(44, 159, 76));
		internal static readonly StyleRank Shattered = new ("Shattered", 4, 800, Terraific.LowerBound, new Color(101, 51, 95));
		internal static readonly StyleRank Abyssal = new ("Abyssal", 3, 450, Shattered.LowerBound, new Color(230, 46, 49));
		internal static readonly StyleRank Ballistic = new ("Ballistic", 2, 200, Abyssal.LowerBound, new Color(225, 159, 62));
		internal static readonly StyleRank Cool = new("Cool", 1, 50, Ballistic.LowerBound, new Color(42, 113, 72));
		internal static readonly StyleRank Dull = new ("Dull", 0, 0, Cool.LowerBound, new Color(58, 73, 89));

		internal static readonly StyleRank[] List = [Dull, Cool, Ballistic, Abyssal, Shattered, Terraific, Null];
	}
}