using System;
using System.Collections.Generic;
using System.Linq;

namespace RuoJingo
{
    public class LeaderboardEntry
    {
        public int Rank;
        public string Name;
        public int Xp;
        public bool IsPlayer;
    }

    /// <summary>
    /// MOCK DATA. A fake weekly leaderboard for one class section: the player plus 5 hard-coded classmates.
    /// There is no backend; these names and numbers are invented placeholders.
    /// </summary>
    public static class LeaderboardMock
    {
        public const string SectionName = "BSIT 2-A";
        public const string Disclaimer = "MOCK DATA - placeholder classmates, not real students";

        private static readonly (string name, int xp)[] Classmates =
        {
            ("Andrea (mock)", 140),
            ("Miguel (mock)", 95),
            ("Bea (mock)", 60),
            ("Paolo (mock)", 30),
            ("Jessa (mock)", 15),
        };

        public static List<LeaderboardEntry> GetWeekly(string playerName, int playerWeeklyXp)
        {
            var all = Classmates
                .Select(c => new LeaderboardEntry { Name = c.name, Xp = c.xp })
                .Append(new LeaderboardEntry
                {
                    Name = string.IsNullOrEmpty(playerName) ? "You" : playerName,
                    Xp = Math.Max(0, playerWeeklyXp),
                    IsPlayer = true
                })
                // Highest XP first; on a tie the player ranks higher (friendlier for a prototype).
                .OrderByDescending(e => e.Xp)
                .ThenByDescending(e => e.IsPlayer)
                .ToList();

            for (int i = 0; i < all.Count; i++) all[i].Rank = i + 1;
            return all;
        }
    }
}
