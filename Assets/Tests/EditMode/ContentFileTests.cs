using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace RuoJingo.Tests
{
    /// <summary>Validates the shipped lesson JSON (structure and key technical facts).</summary>
    public class ContentFileTests
    {
        private static string LessonsDir => Path.Combine(Application.streamingAssetsPath, ContentRepository.LessonsFolder);

        private static List<UnitData> LoadUnits()
        {
            var index = JsonUtility.FromJson<LessonIndex>(File.ReadAllText(Path.Combine(LessonsDir, ContentRepository.IndexFile)));
            return index.files
                .Select(f => JsonUtility.FromJson<UnitData>(File.ReadAllText(Path.Combine(LessonsDir, f))))
                .ToList();
        }

        private static ExerciseData Find(List<UnitData> units, string id) =>
            units.SelectMany(u => u.exercises).Single(e => e.id == id);

        [Test]
        public void ThreeUnitsWithAtLeastSixValidExercisesEach()
        {
            var units = LoadUnits();
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, units.Select(u => u.unitId).ToArray());
            foreach (var u in units)
            {
                Assert.GreaterOrEqual(u.exercises.Length, 6, $"unit {u.unitId}");
                Assert.LessOrEqual(u.exercises.Length, LessonRunner.MaxExercises - SpacedRepetition.DefaultMaxReviewsPerLesson, $"unit {u.unitId}");
                var errors = ContentValidator.Validate(u);
                Assert.IsEmpty(errors, string.Join("\n", errors));
            }
            var ids = units.SelectMany(u => u.exercises).Select(e => e.id).ToList();
            Assert.AreEqual(ids.Count, ids.Distinct().Count(), "exercise ids must be globally unique");
        }

        [Test]
        public void Unit2HasWireOrderExercises()
        {
            var unit2 = LoadUnits().Single(u => u.unitId == 2);
            Assert.IsTrue(unit2.exercises.Any(e => e.type == ExerciseTypes.WireOrder));
        }

        [Test]
        public void SubnettingAnswersAgreeWithFormula()
        {
            var units = LoadUnits();
            Assert.AreEqual(SubnetMath.MaskForPrefix(26), Find(units, "u3_q1").acceptedAnswers[0]);
            Assert.AreEqual(SubnetMath.UsableHosts(27).ToString(), Find(units, "u3_q2").acceptedAnswers[0]);
            var q3 = Find(units, "u3_q3");
            Assert.AreEqual(SubnetMath.MaskForPrefix(25), q3.options[q3.correctIndex]);
            Assert.AreEqual(SubnetMath.UsableHosts(24).ToString(), Find(units, "u3_q4").acceptedAnswers[0]);
            var q5 = Find(units, "u3_q5");
            Assert.AreEqual("/" + SubnetMath.SmallestPrefixForHosts(50), q5.options[q5.correctIndex]);
            var q6 = Find(units, "u3_q6");
            Assert.AreEqual(SubnetMath.MaskForPrefix(SubnetMath.SmallestPrefixForHosts(25)), q6.options[q6.correctIndex]);
            Assert.AreEqual(SubnetMath.SubnetCount(24, 26).ToString(), Find(units, "u3_q7").acceptedAnswers[0]);
        }
    }
}
