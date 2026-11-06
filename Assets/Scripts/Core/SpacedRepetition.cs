using System;
using System.Collections.Generic;
using System.Linq;

namespace RuoJingo
{
    [Serializable]
    public class LeitnerEntry
    {
        public string questionId;
        public int unitId;
        public int box;         // 1..3
        public int dueSession;  // lessonCounter value at which it is due again
    }

    /// <summary>
    /// Leitner system with 3 boxes. The "clock" is the lesson counter (lessons started), not wall time,
    /// so a missed question is guaranteed to come back in the very next lesson.
    ///  - Wrong answer: question goes to box 1, due next lesson.
    ///  - Correct answer on a tracked question: moves up one box (due after 2 / 4 lessons).
    ///  - Correct answer while already in box 3: graduated (removed from the system).
    /// Operates directly on the list stored in SaveData.
    /// </summary>
    public class SpacedRepetition
    {
        public const int BoxCount = 3;
        public const int DefaultMaxReviewsPerLesson = 2;
        private static readonly int[] IntervalByBox = { 1, 2, 4 }; // lessons until due, for box 1..3

        private readonly List<LeitnerEntry> entries;

        public SpacedRepetition(List<LeitnerEntry> entries)
        {
            this.entries = entries ?? throw new ArgumentNullException(nameof(entries));
        }

        public IReadOnlyList<LeitnerEntry> Entries => entries;

        public static int IntervalFor(int box) => IntervalByBox[Math.Max(1, Math.Min(BoxCount, box)) - 1];

        public LeitnerEntry Find(string questionId) => entries.FirstOrDefault(e => e.questionId == questionId);

        /// <summary>Box 1..3, or 0 if the question is not tracked.</summary>
        public int GetBox(string questionId) => Find(questionId)?.box ?? 0;

        public void RecordAnswer(string questionId, int unitId, bool correct, int currentSession)
        {
            if (string.IsNullOrEmpty(questionId)) return;
            var entry = Find(questionId);

            if (!correct)
            {
                if (entry == null)
                {
                    entry = new LeitnerEntry { questionId = questionId, unitId = unitId };
                    entries.Add(entry);
                }
                entry.box = 1;
                entry.dueSession = currentSession + IntervalFor(1);
                return;
            }

            if (entry == null) return; // correct on an untracked question: nothing to do

            if (entry.box >= BoxCount)
            {
                entries.Remove(entry); // graduated
                return;
            }
            entry.box++;
            entry.dueSession = currentSession + IntervalFor(entry.box);
        }

        /// <summary>
        /// Up to <paramref name="max"/> due questions from units *earlier* than <paramref name="currentUnitId"/>,
        /// lowest box first (weakest knowledge first), then most overdue first.
        /// </summary>
        public List<LeitnerEntry> GetDueReviews(int currentUnitId, int currentSession, int max = DefaultMaxReviewsPerLesson)
        {
            return entries
                .Where(e => e.unitId < currentUnitId && e.dueSession <= currentSession)
                .OrderBy(e => e.box)
                .ThenBy(e => e.dueSession)
                .Take(Math.Max(0, max))
                .ToList();
        }
    }
}
