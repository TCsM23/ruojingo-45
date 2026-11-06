using System.Collections.Generic;
using NUnit.Framework;

namespace RuoJingo.Tests
{
    public class SpacedRepetitionTests
    {
        private List<LeitnerEntry> store;
        private SpacedRepetition srs;

        [SetUp]
        public void SetUp()
        {
            store = new List<LeitnerEntry>();
            srs = new SpacedRepetition(store);
        }

        [Test]
        public void WrongAnswerGoesToBoxOne_DueNextLesson()
        {
            srs.RecordAnswer("u1_q1", 1, false, currentSession: 1);
            Assert.AreEqual(1, srs.GetBox("u1_q1"));
            Assert.IsEmpty(srs.GetDueReviews(currentUnitId: 2, currentSession: 1));
            var due = srs.GetDueReviews(currentUnitId: 2, currentSession: 2);
            Assert.AreEqual(1, due.Count);
            Assert.AreEqual("u1_q1", due[0].questionId);
        }

        [Test]
        public void CorrectAnswerOnUntrackedQuestionIsIgnored()
        {
            srs.RecordAnswer("u1_q1", 1, true, 1);
            Assert.AreEqual(0, srs.GetBox("u1_q1"));
            Assert.IsEmpty(store);
        }

        [Test]
        public void CorrectReviewMovesUpAndGraduatesAfterBoxThree()
        {
            srs.RecordAnswer("q", 1, false, 1);
            srs.RecordAnswer("q", 1, true, 2);
            Assert.AreEqual(2, srs.GetBox("q"));
            srs.RecordAnswer("q", 1, true, 4);
            Assert.AreEqual(3, srs.GetBox("q"));
            srs.RecordAnswer("q", 1, true, 8);
            Assert.AreEqual(0, srs.GetBox("q"), "box 3 + correct = graduated");
        }

        [Test]
        public void WrongAnswerFromHigherBoxDropsBackToBoxOne()
        {
            srs.RecordAnswer("q", 1, false, 1);
            srs.RecordAnswer("q", 1, true, 2);
            srs.RecordAnswer("q", 1, false, 3);
            Assert.AreEqual(1, srs.GetBox("q"));
        }

        [Test]
        public void HigherBoxesWaitLonger()
        {
            srs.RecordAnswer("q", 1, false, 1);
            srs.RecordAnswer("q", 1, true, 2); // box 2, interval 2 -> due at 4
            Assert.IsEmpty(srs.GetDueReviews(2, 3));
            Assert.AreEqual(1, srs.GetDueReviews(2, 4).Count);
        }

        [Test]
        public void OnlyReviewsEarlierUnits_AtMostTwo_WeakestFirst()
        {
            srs.RecordAnswer("u1_a", 1, false, 1);
            srs.RecordAnswer("u1_b", 1, false, 1);
            srs.RecordAnswer("u1_c", 1, false, 1);
            srs.RecordAnswer("u1_c", 1, true, 2);   // box 2, due 4
            srs.RecordAnswer("u2_a", 2, false, 1);  // same unit as the lesson -> excluded

            var due = srs.GetDueReviews(currentUnitId: 2, currentSession: 10);
            Assert.AreEqual(2, due.Count);
            CollectionAssert.AreEquivalent(new[] { "u1_a", "u1_b" }, new[] { due[0].questionId, due[1].questionId });
            Assert.IsEmpty(srs.GetDueReviews(currentUnitId: 1, currentSession: 10));
        }

        [Test]
        public void MissedQuestionReappearsInNextLessonViaLessonRunner()
        {
            var save = new SaveData();
            var q1 = new ExerciseData { id = "u1_q1", type = ExerciseTypes.NumericInput, prompt = "?", acceptedAnswers = new[] { "4" }, unitId = 1 };
            var q2 = new ExerciseData { id = "u1_q2", type = ExerciseTypes.NumericInput, prompt = "?", acceptedAnswers = new[] { "3" }, unitId = 1 };
            var u2 = new ExerciseData { id = "u2_q1", type = ExerciseTypes.NumericInput, prompt = "?", acceptedAnswers = new[] { "1" }, unitId = 2 };

            // Lesson 1 (Unit 1): miss q1.
            save.lessonCounter++;
            var srsA = new SpacedRepetition(save.leitner);
            var lesson1 = new LessonRunner(1, new[] { q1, q2 }, null, null, srsA, save.lessonCounter);
            lesson1.Submit(false); lesson1.Advance();
            lesson1.Submit(true); lesson1.Advance();
            Assert.IsTrue(lesson1.IsComplete);

            // Lesson 2 (Unit 2): q1 is due and gets mixed in as a review.
            save.lessonCounter++;
            var srsB = new SpacedRepetition(save.leitner);
            var due = srsB.GetDueReviews(2, save.lessonCounter);
            Assert.AreEqual(1, due.Count);
            var lesson2 = new LessonRunner(2, new[] { u2 }, new[] { q1 }, null, srsB, save.lessonCounter, new System.Random(1));
            Assert.AreEqual(2, lesson2.Items.Count);
            Assert.IsTrue(lesson2.Items[1].IsReview);
            Assert.AreEqual("u1_q1", lesson2.Items[1].Data.id);
        }
    }
}
