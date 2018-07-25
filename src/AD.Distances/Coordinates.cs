using System;
using System.Reflection;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AD.Distances
{
    /// <summary>
    /// Represents a coordinate set with support for geospatial distance calculations.
    /// </summary>
    [PublicAPI]
    [JsonConverter(typeof(CoordinatesJsonConverter))]
    public readonly struct Coordinates
    {
        /// <summary>
        /// The mean radius in kilometers of the Earth as defined by the Internation Union of Geodesy and Geophysics (IUGG).
        /// </summary>
        const double EarthMeanRadius = 6371.0088;

        /// <summary>
        /// The scalar by which degrees can be multiplied to return radians.
        /// </summary>
        const double DegreesToRadians = Math.PI / 180.0;

        /// <summary>
        /// The sine of <see cref="Latitude"/>.
        /// </summary>
        readonly double _sineLatitude;

        /// <summary>
        /// The cosine of <see cref="Latitude"/>.
        /// </summary>
        readonly double _cosineLatitude;

        /// <summary>
        /// The latitude in radians.
        /// </summary>
        public double Latitude { get; }

        /// <summary>
        /// The longitude in radians.
        /// </summary>
        public double Longitude { get; }

        /// <summary>
        /// Constructs a location defined by the <paramref name="latitude"/> and <paramref name="longitude"/> in degrees.
        /// </summary>
        /// <param name="latitude">
        /// The latitude in degrees.
        /// </param>
        /// <param name="longitude">
        /// The longitude in degrees.
        /// </param>
        public Coordinates(double latitude, double longitude)
        {
            if (latitude < -90.0 || latitude > 90.0)
                throw new ArgumentException(nameof(latitude));

            if (longitude < -180.0 || longitude > 180.0)
                throw new ArgumentException(nameof(longitude));

            Latitude = latitude * DegreesToRadians;
            Longitude = longitude * DegreesToRadians;
            _sineLatitude = Math.Sin(Latitude);
            _cosineLatitude = Math.Cos(Latitude);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        [Pure]
        public override string ToString() => $"({Latitude}, {Longitude})";

        /// <summary>
        /// Calculates the great-circle distance in kilometers between two locations.
        /// </summary>
        /// <param name="a">
        /// The first <see cref="Coordinates"/>.
        /// </param>
        /// <param name="b">
        /// The second <see cref="Coordinates"/>.
        /// </param>
        /// <returns>
        /// The great-circle distance in kilometers between the two locations.
        /// </returns>
        [Pure]
        public static double GreatCircleDistance(Coordinates a, Coordinates b)
        {
            double deltaLongitude = Math.Abs(a.Longitude - b.Longitude);
            double cosineDeltaLongitude = Math.Cos(deltaLongitude);

            double numerator0 = b._cosineLatitude * Math.Sin(deltaLongitude);
            double numerator1 = a._cosineLatitude * b._sineLatitude - a._sineLatitude * b._cosineLatitude * cosineDeltaLongitude;
            double numerator = numerator0 * numerator0 + numerator1 * numerator1;

            double denominator = a._sineLatitude * b._sineLatitude + a._cosineLatitude * b._cosineLatitude * cosineDeltaLongitude;

            return EarthMeanRadius * Math.Atan2(Math.Sqrt(numerator), denominator);
        }

        /// <inheritdoc />
        /// <summary>
        /// Custom JSON converter for the <see cref="T:AD.Distances.Coordinates" /> class.
        /// </summary>
        sealed class CoordinatesJsonConverter : JsonConverter
        {
            /// <inheritdoc />
            [Pure]
            public override bool CanConvert([NotNull] Type objectType)
            {
                if (objectType is null)
                    throw new ArgumentNullException(nameof(objectType));

                return typeof(Coordinates).GetTypeInfo().IsAssignableFrom(objectType);
            }

            /// <inheritdoc />
            [Pure]
            [NotNull]
            public override object ReadJson([NotNull] JsonReader reader, [NotNull] Type objectType, [CanBeNull] object existingValue, [CanBeNull] JsonSerializer serializer)
            {
                if (reader is null)
                    throw new ArgumentNullException(nameof(reader));

                if (objectType is null)
                    throw new ArgumentNullException(nameof(objectType));

                JObject jObject = JObject.Load(reader);

                return
                    new Coordinates(
                        jObject.Value<double>(nameof(Latitude).ToLower()),
                        jObject.Value<double>(nameof(Longitude).ToLower()));
            }

            /// <inheritdoc />
            public override void WriteJson([NotNull] JsonWriter writer, [NotNull] object value, [CanBeNull] JsonSerializer serializer)
            {
                if (writer is null)
                    throw new ArgumentNullException(nameof(writer));

                if (value is null)
                    throw new ArgumentNullException(nameof(value));

                Coordinates coordinates = (Coordinates) value;

                JToken token =
                    new JObject(
                        new JProperty(nameof(Latitude).ToLower(), coordinates.Latitude),
                        new JProperty(nameof(Longitude).ToLower(), coordinates.Longitude));

                token.WriteTo(writer);
            }
        }
    }
}