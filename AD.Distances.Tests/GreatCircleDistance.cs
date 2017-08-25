using System;
using Xunit;

namespace AD.Distances.Tests
{
    public class GreatCircleDistance
    {
        private const double Tolerance = 1e-15;
        
        private static readonly Coordinates NewYorkCity = new Coordinates(40.7143528, -74.0059731);

        private static readonly Coordinates Chicago = new Coordinates(41.8781136, -87.6297982);

        [Theory]
        [MemberData(nameof(NewYorkCity), nameof(NewYorkCity), 0.0)]
        [MemberData(nameof(NewYorkCity), nameof(Chicago), 1144.0)]
        public void GreatCircleDistanceTest(Coordinates a, Coordinates b, double expected)
        {
            double distance = Coordinates.GreatCircleDistance(a, b);

            Assert.True(Math.Abs(expected - distance) < Tolerance);
        }
    }
}
