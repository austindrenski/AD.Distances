using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AD.Distances.Tests
{
    [TestClass]
    public class GreatCircleDistance
    {
        private static Coordinates NewYorkCity { get; } = new Coordinates(40.7143528, -74.0059731);

        private static Coordinates Chicago { get; } = new Coordinates(41.8781136, -87.6297982);

        [TestMethod]
        public void TestMethod0()
        {
            double distance = Coordinates.GreatCircleDistance(NewYorkCity, NewYorkCity);

            Assert.AreEqual(0.0, distance);
        }

        [TestMethod]
        public void TestMethod1()
        {
            double distance = Coordinates.GreatCircleDistance(NewYorkCity, Chicago);

            Assert.AreEqual(1144, Math.Round(distance));
        }
    }
}
