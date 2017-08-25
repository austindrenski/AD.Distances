using System;
using System.Reflection;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AD.Distances
{
    [PublicAPI]
    [JsonConverter(typeof(CityJsonConverter))]
    public class City
    {
        [NotNull]
        public string Name { get; }

        public double Population { get; }

        public Coordinates Coordinates { get; }

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

            return Coordinates.GreatCircleDistance(a.Coordinates, b.Coordinates);
        }

        /// <summary>
        /// Custom JSON converter for the <see cref="City"/> class.
        /// </summary>
        private sealed class CityJsonConverter : JsonConverter
        {
            /// <summary>
            /// True if the type implements <see cref="City"/>; otherwise false.
            /// </summary>
            /// <param name="objectType">
            /// The type to compare.
            /// </param>
            public override bool CanConvert(Type objectType)
            {
                return typeof(City).GetTypeInfo().IsAssignableFrom(objectType);
            }

            /// <summary>
            /// Reads the JSON representation of the object.
            /// </summary>
            /// <param name="reader">
            /// The <see cref="JsonReader"/> to read from.
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
                    new City(
                        jObject.Value<string>(nameof(Name)),
                        jObject.Value<double>(nameof(Population)),
                        jObject.Value<Coordinates>(nameof(Coordinates)));
            }

            /// <summary>
            /// Writes the JSON representation of the object.
            /// </summary>
            /// <param name="writer">
            /// The <see cref="JsonWriter"/> to write to.
            /// </param>
            /// <param name="value">
            /// The value.
            /// </param>
            /// <param name="serializer">
            /// The calling serializer.
            /// </param>
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                City city = (City)value;

                JToken token =
                    new JObject(
                        new JProperty(nameof(Name), city.Name),
                        new JProperty(nameof(Population), city.Population),
                        new JProperty(nameof(Coordinates),
                            JToken.FromObject(city.Coordinates)));

                token.WriteTo(writer);
            }
        }
    }
}