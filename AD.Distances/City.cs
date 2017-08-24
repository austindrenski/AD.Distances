using System;
using JetBrains.Annotations;

namespace AD.Distances
{
    [PublicAPI]
    public class City
    {
        [NotNull]
        public string Name { get; }

        public double Population { get; }

        public Location Location { get; }

        public City([NotNull] string name, double population, Location location)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (population < 0)
            {
                //throw new ArgumentOutOfRangeException(nameof(population));
            }

            Name = name;
            Population = population;
            Location = location;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        [Pure]
        [NotNull]
        public override string ToString()
        {
            return $"({Name}, {Population})";
        }

        [Pure]
        public static double Distance([NotNull] City a, [NotNull] City b)
        {
            if (a is null)
            {
                throw new ArgumentNullException(nameof(a));
            }
            if (b is null)
            {
                throw new ArgumentNullException(nameof(b));
            }

            return Location.GreatCircleDistance(a.Location, b.Location);
        }
    }
}