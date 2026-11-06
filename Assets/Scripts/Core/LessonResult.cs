using System.Collections.Generic;

namespace RuoJingo
{
    /// <summary>Summary handed from the Lesson scene to the Results scene.</summary>
    public class LessonResult
    {
        public int UnitId;
        public bool Passed;
        public int CorrectCount;
        public int WrongCount;
        public int TotalExercises;
        public int HeartsLeft;
        public int XpEarned;
        public bool Perfect;
        public float Accuracy;      // 0..1
        public int Streak;
        public int TotalXp;
        public int UnlockedUnitId;  // 0 if nothing new was unlocked
        public int ReviewCount;
        public List<ExerciseData> Missed = new List<ExerciseData>();
    }
}
