using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Xunit;

namespace AD.Distances.Tests
{
    [PublicAPI]
    public class GreatCircleDistance
    {
        public static readonly Coordinates NewYorkCity = new Coordinates(40.7143528, -74.0059731);

        public static readonly Coordinates Chicago = new Coordinates(41.8781136, -87.6297982);

        public static IEnumerable<object[]> TestData
        {
            get
            {
                yield return new object[] { NewYorkCity, NewYorkCity, 0.0 };
                yield return new object[] { NewYorkCity, Chicago, 1144.0 };
            }
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public void GreatCircleDistanceTest(Coordinates a, Coordinates b, double expected)
        {
            double distance = Coordinates.GreatCircleDistance(a, b);

            Assert.True(Math.Abs(expected - distance) < 0.5);
        }
    }
}