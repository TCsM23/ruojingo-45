using System;
using NUnit.Framework;

namespace RuoJingo.Tests
{
    public class StreakSystemTests
    {
        private static readonly DateTime Day1 = new DateTime(2026, 11, 2);

        [Test]
        public void FirstLessonEverStartsStreakAtOne()
        {
            Assert.AreEqual(1, StreakSystem.RegisterLessonCompleted(0, null, Day1));
        }

        [Test]
        public void SecondLessonSameDayDoesNotIncrement()
        {
            Assert.AreEqual(3, StreakSystem.RegisterLessonCompleted(3, Day1, Day1.AddHours(5)));
        }

        [Test]
        public void ConsecutiveDayIncrements()
        {
            Assert.AreEqual(4, StreakSystem.RegisterLessonCompleted(3, Day1, Day1.AddDays(1)));
        }

        [Test]
        public void MissedDayResetsToOneOnNextLesson()
        {
            Assert.AreEqual(1, StreakSystem.RegisterLessonCompleted(7, Day1, Day1.AddDays(2)));
        }

        [Test]
        public void DisplayedStreakIsZeroAfterAMissedDay()
        {
            Assert.AreEqual(5, StreakSystem.GetCurrentStreak(5, Day1, Day1));
            Assert.AreEqual(5, StreakSystem.GetCurrentStreak(5, Day1, Day1.AddDays(1)));
            Assert.AreEqual(0, StreakSystem.GetCurrentStreak(5, Day1, Day1.AddDays(2)));
            Assert.AreEqual(0, StreakSystem.GetCurrentStreak(0, null, Day1));
        }

        [Test]
        public void MonthBoundaryCountsAsConsecutive()
        {
            var oct31 = new DateTime(2026, 10, 31);
            Assert.AreEqual(2, StreakSystem.RegisterLessonCompleted(1, oct31, new DateTime(2026, 11, 1)));
        }
    }
}
