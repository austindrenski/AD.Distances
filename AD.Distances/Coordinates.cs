using System;
using System.Reflection;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AD.Distances
{
    [PublicAPI]
    [JsonConverter(typeof(CoordinatesJsonConverter))]
    public struct Coordinates
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
        public Coordinates(double latitude, double longitude)
        {
            Latitude = latitude * Math.PI / 180.0;
            Longitude = longitude * Math.PI / 180.0;
            _sineLatitude = Math.Sin(Latitude);
            _cosineLatitude = Math.Cos(Latitude);
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

        /// <summary>
        /// Custom JSON converter for the <see cref="T:AD.Distances.Coordinates" /> class.
        /// </summary>
        private sealed class CoordinatesJsonConverter : JsonConverter
        {
            /// <summary>
            /// True if the type implements <see cref="T:AD.Distances.Coordinates" />; otherwise false.
            /// </summary>
            /// <param name="objectType">
            /// The type to compare.
            /// </param>
            public override bool CanConvert(Type objectType)
            {
                return typeof(Coordinates).GetTypeInfo().IsAssignableFrom(objectType);
            }

            /// <summary>
            /// Reads the JSON representation of the object.
            /// </summary>
            /// <param name="reader">
            /// The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.
            /// </param>
            /// <param name="objectType">
            /// Type of the object.
            /// </param>
            /// <param name="existingValue">
            /// The existing value of object being read.
            /// </param>
            /// <param name="serializer">
            /// The calling serializer.
            /// </param>
            /// <returns>
            /// The object value.
            /// </returns>
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                JObject jObject = JObject.Load(reader);

                return
                    new Coordinates(
                        jObject.Value<double>(nameof(Latitude)),
                        jObject.Value<double>(nameof(Longitude)));
            }

            /// <summary>
            /// Writes the JSON representation of the object.
            /// </summary>
            /// <param name="writer">
            /// The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.
            /// </param>
            /// <param name="value">
            /// The value.
            /// </param>
            /// <param name="serializer">
            /// The calling serializer.
            /// </param>
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                Coordinates coordinates = (Coordinates)value;

                JToken token =
                    new JObject(
                        new JProperty(nameof(Latitude), coordinates.Latitude),
                        new JProperty(nameof(Longitude), coordinates.Longitude));

                token.WriteTo(writer);
            }
        }
    }
}