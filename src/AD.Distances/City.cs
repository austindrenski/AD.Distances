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
        public readonly Coordinates Coordinates;

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
        public City([NotNull] string name, double population, Coordinates location)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (population < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(population));
            }

            Name = name;
            Population = population;
            Coordinates = location;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        [Pure]
        public override string ToString()
        {
            return $"({Name}, {Population})";
        }

        /// <summary>
        /// Calculates the great-circle distance in kilometers between two cities.
        /// </summary>
        /// <param name="a">
        /// The first <see cref="City"/>.
        /// </param>
        /// <param name="b">
        /// The second <see cref="City"/>.
        /// </param>
        /// <returns>
        /// The great-circle distance in kilometers.
        /// </returns>
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

            return Coordinates.GreatCircleDistance(a.Coordinates, b.Coordinates);
        }

        /// <inheritdoc />
        /// <summary>
        /// Custom JSON converter for the <see cref="T:AD.Distances.City" /> class.
        /// </summary>
        private sealed class CityJsonConverter : JsonConverter
        {
            /// <inheritdoc />
            [Pure]
            public override bool CanConvert([NotNull] Type objectType)
            {
                if (objectType is null)
                {
                    throw new ArgumentNullException(nameof(objectType));
                }

                return typeof(City).GetTypeInfo().IsAssignableFrom(objectType);
            }

            /// <inheritdoc />
            [Pure]
            [NotNull]
            public override object ReadJson([NotNull] JsonReader reader, [NotNull] Type objectType, [NotNull] object existingValue, [NotNull] JsonSerializer serializer)
            {
                if (reader is null)
                {
                    throw new ArgumentNullException(nameof(reader));
                }

                if (objectType is null)
                {
                    throw new ArgumentNullException(nameof(objectType));
                }

                if (existingValue is null)
                {
                    throw new ArgumentNullException(nameof(existingValue));
                }

                if (serializer is null)
                {
                    throw new ArgumentNullException(nameof(serializer));
                }

                JObject jObject = JObject.Load(reader);

                return
                    new City(
                        jObject.Value<string>(nameof(Name)),
                        jObject.Value<double>(nameof(Population)),
                        jObject.Value<Coordinates>(nameof(Coordinates)));
            }

            /// <inheritdoc />
            public override void WriteJson([NotNull] JsonWriter writer, [NotNull] object value, [NotNull] JsonSerializer serializer)
            {
                if (writer is null)
                {
                    throw new ArgumentNullException(nameof(writer));
                }

                if (value is null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                if (serializer is null)
                {
                    throw new ArgumentNullException(nameof(serializer));
                }

                City city = (City) value;

                JToken token =
                    new JObject(
                        new JProperty(nameof(Name), city.Name),
                        new JProperty(nameof(Population), city.Population),
                        new JProperty(nameof(Coordinates), JToken.FromObject(city.Coordinates)));

                token.WriteTo(writer);
            }
        }
    }
}