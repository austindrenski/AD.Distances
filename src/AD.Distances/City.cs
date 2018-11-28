using System;
using System.Reflection;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AD.Distances
{
    /// <summary>
    /// Represents a city with location and characteristics.
    /// </summary>
    [PublicAPI]
    [JsonConverter(typeof(CityJsonConverter))]
    public sealed class City
    {
        /// <summary>
        /// The name of the <see cref="City"/>.
        /// </summary>
        [NotNull]
        public string Name { get; }

        /// <summary>
        /// The population of the <see cref="City"/>.
        /// </summary>
        public double Population { get; }

        /// <summary>
        /// The latitude and longitude of the <see cref="City"/>.
        /// </summary>
        public Coordinates Coordinates { get; }

        /// <summary>
        /// Constructs a <see cref="City"/> with the given location and characteristics.
        /// </summary>
        /// <param name="name">
        /// The name of the city.
        /// </param>
        /// <param name="population">
        /// The population of the city.
        /// </param>
        /// <param name="location">
        /// The location of the city.
        /// </param>
        public City([NotNull] string name, double population, in Coordinates location)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (population < 0)
                throw new ArgumentOutOfRangeException(nameof(population));

            Name = name;
            Population = population;
            Coordinates = location;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        [Pure]
        [NotNull]
        public override string ToString() => $"({Name}, {Population})";

        /// <summary>
        /// Calculates the great-circle distance in kilometers between two cities.
        /// </summary>
        /// <param name="a">The first <see cref="City"/>.</param>
        /// <param name="b">The second <see cref="City"/>.</param>
        /// <returns>
        /// The great-circle distance in kilometers.
        /// </returns>
        [Pure]
        public static double Distance([NotNull] City a, [NotNull] City b)
        {
            if (a == null)
                throw new ArgumentNullException(nameof(a));

            if (b == null)
                throw new ArgumentNullException(nameof(b));

            return Coordinates.GreatCircleDistance(a.Coordinates, b.Coordinates);
        }

        /// <summary>
        /// Custom JSON converter for the <see cref="T:AD.Distances.City" /> class.
        /// </summary>
        /// <inheritdoc />
        sealed class CityJsonConverter : JsonConverter
        {
            /// <inheritdoc />
            [Pure]
            public override bool CanConvert([CanBeNull] Type objectType) => typeof(City).GetTypeInfo().IsAssignableFrom(objectType);

            /// <inheritdoc />
            [Pure]
            [NotNull]
            public override object ReadJson([NotNull] JsonReader reader, [NotNull] Type objectType, [CanBeNull] object existingValue, [CanBeNull] JsonSerializer serializer)
            {
                if (reader == null)
                    throw new ArgumentNullException(nameof(reader));

                if (objectType == null)
                    throw new ArgumentNullException(nameof(objectType));

                JObject jObject = JObject.Load(reader);

                return
                    new City(
                        jObject.Value<string>(nameof(Name).ToLower()),
                        jObject.Value<double>(nameof(Population).ToLower()),
                        jObject.Value<JObject>(nameof(Coordinates).ToLower())
                               .ToObject<Coordinates>());
            }

            /// <inheritdoc />
            public override void WriteJson([NotNull] JsonWriter writer, [NotNull] object value, [CanBeNull] JsonSerializer serializer)
            {
                if (writer == null)
                    throw new ArgumentNullException(nameof(writer));

                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                City city = (City) value;

                JToken token =
                    new JObject(
                        new JProperty(nameof(Name).ToLower(), city.Name),
                        new JProperty(nameof(Population).ToLower(), city.Population),
                        new JProperty(nameof(Coordinates).ToLower(), JToken.FromObject(city.Coordinates)));

                token.WriteTo(writer);
            }
        }
    }
}