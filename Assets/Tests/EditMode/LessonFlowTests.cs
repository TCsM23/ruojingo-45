using System;
using System.Linq;
using NUnit.Framework;

namespace RuoJingo.Tests
{
    /// <summary>LessonRunner, XpSystem, ProgressService, wiring rules, grading and the mock leaderboard.</summary>
    public class LessonFlowTests
    {
        private static ExerciseData Q(string id, int unit = 1) => new ExerciseData
        {
            id = id, type = ExerciseTypes.NumericInput, prompt = "?", acceptedAnswers = new[] { "1" }, unitId = unit
        };

        private static LessonRunner Runner(int count, int unit = 1) =>
            new LessonRunner(unit, Enumerable.Range(1, count).Select(i => Q($"u{unit}_q{i}", unit)).ToList(), null);

        [Test]
        public void XpRules()
        {
            Assert.AreEqual(80 + 20, XpSystem.CalculateLessonXp(8, 0, true), "perfect bonus");
            Assert.AreEqual(60, XpSystem.CalculateLessonXp(6, 2, true));
            Assert.AreEqual(0, XpSystem.CalculateLessonXp(3, 5, false), "no XP for a failed lesson");
        }

        [Test]
        public void PassingUnitUnlocksNextAndAwardsXpAndStreak()
        {
            var save = new SaveData();
            var runner = Runner(6);
            for (int i = 0; i < 6; i++) { runner.Submit(i != 0); runner.Advance(); }
            Assert.IsTrue(runner.IsComplete);

            var today = new DateTime(2026, 11, 6);
            var result = ProgressService.ApplyLessonResult(save, runner, today, totalUnits: 3);
            Assert.IsTrue(result.Passed);
            Assert.AreEqual(50, result.XpEarned);
            Assert.AreEqual(50, save.totalXp);
            Assert.AreEqual(4, save.lastHearts);
            Assert.AreEqual(2, save.highestUnlockedUnit);
            Assert.AreEqual(2, result.UnlockedUnitId);
            Assert.AreEqual(1, save.streak);
            Assert.AreEqual("2026-11-06", save.lastPlayedDate);
        }

        [Test]
        public void FiveWrongAnswersFailTheLesson_NoUnlockNoXp()
        {
            var save = new SaveData();
            var runner = Runner(8);
            for (int i = 0; i < 5; i++)
            {
                bool failed = runner.Submit(false);
                Assert.AreEqual(i == 4, failed);
                runner.Advance();
            }
            Assert.IsTrue(runner.IsFailed);
            Assert.IsFalse(runner.IsComplete);
            Assert.IsNull(runner.Current);

            var result = ProgressService.ApplyLessonResult(save, runner, new DateTime(2026, 11, 6), 3);
            Assert.IsFalse(result.Passed);
            Assert.AreEqual(0, result.XpEarned);
            Assert.AreEqual(1, save.highestUnlockedUnit);
            Assert.AreEqual(0, save.streak);
            Assert.AreEqual(5, result.Missed.Count);
        }

        [Test]
        public void LessonIsCappedAtTenExercisesIncludingReviews()
        {
            var regular = Enumerable.Range(1, 12).Select(i => Q($"u2_q{i}", 2)).ToList();
            var reviews = new[] { Q("u1_a"), Q("u1_b"), Q("u1_c") };
            var runner = new LessonRunner(2, regular, reviews, rng: new Random(3));
            Assert.AreEqual(10, runner.Items.Count);
            Assert.AreEqual(2, runner.Items.Count(i => i.IsReview));
            Assert.IsFalse(runner.Items[0].IsReview, "a lesson never opens with a review");
        }

        [Test]
        public void CableTypeRule()
        {
            Assert.AreEqual(WireStandards.StraightThrough, WireStandards.GetCableType("T568A", "T568A"));
            Assert.AreEqual(WireStandards.StraightThrough, WireStandards.GetCableType("T568B", "T568B"));
            Assert.AreEqual(WireStandards.Crossover, WireStandards.GetCableType("T568A", "T568B"));
            Assert.AreEqual(WireStandards.Crossover, WireStandards.GetCableType("T568B", "T568A"));
        }

        [Test]
        public void WireOrdersMatchStandard()
        {
            CollectionAssert.AreEqual(
                new[] { "white/green", "green", "white/orange", "blue", "white/blue", "orange", "white/brown", "brown" },
                WireStandards.GetOrder("T568A"));
            CollectionAssert.AreEqual(
                new[] { "white/orange", "orange", "white/green", "blue", "white/blue", "green", "white/brown", "brown" },
                WireStandards.GetOrder("T568B"));
        }

        [Test]
        public void NumericAnswersAreNormalised()
        {
            var q = new ExerciseData { type = ExerciseTypes.NumericInput, acceptedAnswers = new[] { "255.255.255.192" } };
            Assert.IsTrue(AnswerChecker.CheckNumeric(q, " 255.255.255.192 "));
            Assert.IsFalse(AnswerChecker.CheckNumeric(q, "255.255.255.128"));
            var n = new ExerciseData { type = ExerciseTypes.NumericInput, acceptedAnswers = new[] { "62" } };
            Assert.IsTrue(AnswerChecker.CheckNumeric(n, "062"));
            Assert.IsFalse(AnswerChecker.CheckNumeric(n, ""));
        }

        [Test]
        public void LeaderboardIncludesPlayerAndFiveMockNames_SortedByXp()
        {
            var board = LeaderboardMock.GetWeekly("You", 100);
            Assert.AreEqual(6, board.Count);
            Assert.AreEqual(1, board.Count(e => e.IsPlayer));
            for (int i = 1; i < board.Count; i++) Assert.GreaterOrEqual(board[i - 1].Xp, board[i].Xp);
            Assert.AreEqual(2, board.First(e => e.IsPlayer).Rank);
        }
    }
}
