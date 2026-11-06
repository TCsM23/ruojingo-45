using NUnit.Framework;

namespace RuoJingo.Tests
{
    public class HeartSystemTests
    {
        [Test]
        public void StartsWithFiveHearts()
        {
            var hearts = new HeartSystem();
            Assert.AreEqual(5, hearts.Current);
            Assert.AreEqual(5, hearts.MaxHearts);
            Assert.IsFalse(hearts.IsEmpty);
        }

        [Test]
        public void LosesOneHeartPerWrongAnswer()
        {
            var hearts = new HeartSystem();
            hearts.LoseHeart();
            hearts.LoseHeart();
            Assert.AreEqual(3, hearts.Current);
        }

        [Test]
        public void IsEmptyAfterFiveLosses_AndNeverNegative()
        {
            var hearts = new HeartSystem();
            for (int i = 0; i < 4; i++) Assert.IsFalse(hearts.LoseHeart());
            Assert.IsTrue(hearts.LoseHeart());
            Assert.IsTrue(hearts.IsEmpty);
            hearts.LoseHeart();
            Assert.AreEqual(0, hearts.Current);
        }

        [Test]
        public void RefillRestoresMax()
        {
            var hearts = new HeartSystem();
            hearts.LoseHeart();
            hearts.Refill();
            Assert.AreEqual(5, hearts.Current);
        }

        [Test]
        public void ChangedEventReportsNewValue()
        {
            var hearts = new HeartSystem();
            int reported = -1;
            hearts.Changed += v => reported = v;
            hearts.LoseHeart();
            Assert.AreEqual(4, reported);
        }
    }
}
