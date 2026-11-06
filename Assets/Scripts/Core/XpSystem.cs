namespace RuoJingo
{
    /// <summary>+10 XP per correct answer, +20 bonus for a perfect lesson. XP is only awarded for a passed lesson.</summary>
    public static class XpSystem
    {
        public const int XpPerCorrect = 10;
        public const int PerfectBonus = 20;

        public static int CalculateLessonXp(int correctCount, int wrongCount, bool passed)
        {
            if (!passed || correctCount <= 0) return 0;
            int xp = correctCount * XpPerCorrect;
            if (wrongCount == 0) xp += PerfectBonus;
            return xp;
        }
    }
}
