using NUnit.Framework;

namespace RuoJingo.Tests
{
    public class SubnetMathTests
    {
        [TestCase(24, 254)]
        [TestCase(25, 126)]
        [TestCase(26, 62)]
        [TestCase(27, 30)]
        [TestCase(28, 14)]
        [TestCase(30, 2)]
        public void UsableHostsFollowsFormula(int prefix, long expected)
        {
            Assert.AreEqual(expected, SubnetMath.UsableHosts(prefix));
        }

        [TestCase(24, "255.255.255.0")]
        [TestCase(25, "255.255.255.128")]
        [TestCase(26, "255.255.255.192")]
        [TestCase(27, "255.255.255.224")]
        [TestCase(28, "255.255.255.240")]
        [TestCase(16, "255.255.0.0")]
        [TestCase(0, "0.0.0.0")]
        public void MaskForPrefix(int prefix, string expected)
        {
            Assert.AreEqual(expected, SubnetMath.MaskForPrefix(prefix));
        }

        [TestCase(50, 26)]
        [TestCase(25, 27)]
        [TestCase(30, 27)]
        [TestCase(31, 26)]
        [TestCase(100, 25)]
        [TestCase(200, 24)]
        public void SmallestPrefixForHosts(long hosts, int expectedPrefix)
        {
            Assert.AreEqual(expectedPrefix, SubnetMath.SmallestPrefixForHosts(hosts));
        }

        [Test]
        public void SubnetCount()
        {
            Assert.AreEqual(4, SubnetMath.SubnetCount(24, 26));
            Assert.AreEqual(8, SubnetMath.SubnetCount(24, 27));
        }

        [Test]
        public void RejectsOutOfRangePrefix()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => SubnetMath.UsableHosts(31));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => SubnetMath.UsableHosts(-1));
        }
    }
}
