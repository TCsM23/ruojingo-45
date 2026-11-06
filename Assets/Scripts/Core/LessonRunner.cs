using System;
using System.Collections.Generic;
using System.Linq;

namespace RuoJingo
{
    public class LessonItem
    {
        public ExerciseData Data;
        public bool IsReview;     // mixed in by spaced repetition from an earlier unit
        public bool? AnsweredCorrectly;
    }

    /// <summary>
    /// Runs one lesson: serves exercises one at a time, tracks score and hearts and reports answers to the
    /// spaced-repetition system. No Unity dependencies; the LessonScreen just drives it.
    /// Flow per exercise: Current -> Submit(correct) -> (show feedback) -> Advance().
    /// </summary>
    public class LessonRunner
    {
        public const int MinExercises = 5;
        public const int MaxExercises = 10;

        private readonly List<LessonItem> items = new List<LessonItem>();
        private readonly SpacedRepetition srs;
        private readonly int session;
        private bool currentSubmitted;

        public int UnitId { get; }
        public HeartSystem Hearts { get; }
        public IReadOnlyList<LessonItem> Items => items;
        public int CurrentIndex { get; private set; }
        public int CorrectCount { get; private set; }
        public int WrongCount { get; private set; }
        public int AnsweredCount => CorrectCount + WrongCount;
        public List<ExerciseData> Missed { get; } = new List<ExerciseData>();

        public bool IsFailed => Hearts.IsEmpty;
        public bool IsComplete => !IsFailed && CurrentIndex >= items.Count;
        public bool IsOver => IsFailed || IsComplete;
        public LessonItem Current => !IsOver && CurrentIndex < items.Count ? items[CurrentIndex] : null;
        public bool CurrentSubmitted => currentSubmitted;

        /// <summary>Fraction of exercises finished (0..1), for the progress bar.</summary>
        public float Progress => items.Count == 0 ? 1f : (float)(CurrentIndex + (currentSubmitted ? 1 : 0)) / items.Count;

        public float Accuracy => AnsweredCount == 0 ? 0f : (float)CorrectCount / AnsweredCount;

        public event Action<LessonItem, bool> AnswerSubmitted;

        /// <param name="regular">The unit's own exercises (in order).</param>
        /// <param name="reviews">Due review questions from earlier units (at most 2 are used).</param>
        /// <param name="srs">Optional; every answer is recorded with it.</param>
        /// <param name="session">Leitner clock (SaveData.lessonCounter) for this lesson.</param>
        /// <param name="rng">Used to place reviews at random positions; pass a seeded Random in tests.</param>
        public LessonRunner(int unitId, IList<ExerciseData> regular, IList<ExerciseData> reviews,
            HeartSystem hearts = null, SpacedRepetition srs = null, int session = 0, Random rng = null)
        {
            UnitId = unitId;
            Hearts = hearts ?? new HeartSystem();
            this.srs = srs;
            this.session = session;
            rng = rng ?? new Random();

            var reviewList = (reviews ?? new List<ExerciseData>())
                .Where(r => r != null)
                .Take(SpacedRepetition.DefaultMaxReviewsPerLesson)
                .ToList();
            var regularList = (regular ?? new List<ExerciseData>())
                .Where(r => r != null && reviewList.All(v => v.id != r.id))
                .Take(MaxExercises - reviewList.Count)
                .ToList();

            items.AddRange(regularList.Select(d => new LessonItem { Data = d }));
            // Never open the lesson with a review: insert each at a random position after the first item.
            foreach (var r in reviewList)
            {
                int pos = items.Count == 0 ? 0 : rng.Next(1, items.Count + 1);
                items.Insert(pos, new LessonItem { Data = r, IsReview = true });
            }
        }

        /// <summary>Records the result for the current exercise. Returns true if this answer emptied the hearts.</summary>
        public bool Submit(bool correct)
        {
            var item = Current ?? throw new InvalidOperationException("Lesson is over.");
            if (currentSubmitted) throw new InvalidOperationException("Current exercise already submitted.");
            currentSubmitted = true;
            item.AnsweredCorrectly = correct;

            if (correct) CorrectCount++;
            else
            {
                WrongCount++;
                Missed.Add(item.Data);
                Hearts.LoseHeart();
            }
            srs?.RecordAnswer(item.Data.id, item.Data.unitId, correct, session);
            AnswerSubmitted?.Invoke(item, correct);
            return IsFailed;
        }

        /// <summary>Moves to the next exercise after feedback was shown.</summary>
        public void Advance()
        {
            if (!currentSubmitted) throw new InvalidOperationException("Submit before advancing.");
            if (IsFailed) return;
            currentSubmitted = false;
            CurrentIndex++;
        }

        /// <summary>Skips an exercise that cannot be shown (e.g. missing prefab) without scoring it.</summary>
        public void Skip()
        {
            if (IsOver) return;
            currentSubmitted = false;
            CurrentIndex++;
        }
    }
}
