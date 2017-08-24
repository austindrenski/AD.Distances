using System;
using JetBrains.Annotations;

namespace AD.Distances
{
    [PublicAPI]
    public struct Location
    {
        /// <summary>
        /// The mean radius in kilometers of the Earth as defined by the Internation Union of Geodesy and Geophysics (IUGG).
        /// </summary>
        private const double EarthMeanRadius = 6371.0088;

        /// <summary>
        /// The sine of <see cref="Latitude"/>.
        /// </summary>
        private readonly double _sineLatitude;

        /// <summary>
        /// The cosine of <see cref="Latitude"/>.
        /// </summary>
        private readonly double _cosineLatitude;
            
        /// <summary>
        /// The latitude in radians.
        /// </summary>
        public double Latitude { get; }

        /// The longitude in radians.
        public double Longitude { get; }
            
        /// <summary>
        /// Constructs a location defined by the <paramref name="latitude"/> and <paramref name="longitude"/>.
        /// </summary>
        /// <param name="latitude">
        /// The latitude in radians.
        /// </param>
        /// <param name="longitude">
        /// The longitude in radians.
        /// </param>
        public Location(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
            _sineLatitude = Math.Sin(latitude);
            _cosineLatitude = Math.Cos(latitude);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        [Pure]
        [NotNull]
        public override string ToString()
        {
            return $"({Latitude}, {Longitude})";
        }

        /// <summary>
        /// Calculates the great-circle distance in kilometers between two locations.
        /// </summary>
        /// <param name="a">
        /// The first <see cref="Location"/>.
        /// </param>
        /// <param name="b">
        /// The second <see cref="Location"/>.
        /// </param>
        /// <returns>
        /// The great-circle distance in kilometers between the two locations.
        /// </returns>
        [Pure]
        public static double GreatCircleDistance(Location a, Location b)
        {
            double deltaLongitude = Math.Abs(a.Longitude - b.Longitude);
            double cosineDeltaLongitude = Math.Cos(deltaLongitude);

            double numerator0 = b._cosineLatitude * Math.Sin(deltaLongitude);
            double numerator1 = a._cosineLatitude * b._sineLatitude - a._sineLatitude * b._cosineLatitude * cosineDeltaLongitude;
            double numerator = numerator0 * numerator0 + numerator1 * numerator1;

            double denominator = a._sineLatitude * b._sineLatitude + a._cosineLatitude * b._cosineLatitude * cosineDeltaLongitude;

            return EarthMeanRadius * Math.Atan2(Math.Sqrt(numerator), denominator);
        }
    }
}