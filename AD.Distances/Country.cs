using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AD.Distances
{
    [PublicAPI]
    [JsonConverter(typeof(CountryJsonConverter))]
    public class Country
    {
        [NotNull]
        public string Name { get; }

        [NotNull]
        public string Year { get; }

        public double Population { get; }
        
        [NotNull]
        [ItemNotNull]
        public IReadOnlyList<City> Cities { get; }

        public Country([NotNull] string name, [NotNull] string year, double population, [NotNull][ItemNotNull] IEnumerable<City> cities)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (year is null)
            {
                throw new ArgumentNullException(nameof(year));
            }
            if (population < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(population));
            }
            if (cities is null)
            {
                throw new ArgumentNullException(nameof(cities));
            }

            Name = name;
            Year = year;
            Population = population;
            Cities = cities as City[] ?? cities.ToArray();
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        [Pure]
        [NotNull]
        public override string ToString()
        {
            return $"({Name}, {Population}, {Cities.Count})";
        }

        [Pure]
        public static (Country A, Country B, double Distance) Distance([NotNull] Country a, [NotNull] Country b)
        {
            if (a is null)
            {
                throw new ArgumentNullException(nameof(a));
            }
            if (b is null)
            {
                throw new ArgumentNullException(nameof(b));
            }

            double result = 0;

            double countryPopulationProduct = a.Population * b.Population;

            for (int i = 0; i < a.Cities.Count; i++)
            {
                for (int j = 0; j < b.Cities.Count; j++)
                {
                    double distance = City.Distance(a.Cities[i], b.Cities[j]);
                    
                    double cityPopulationProduct = a.Cities[i].Population * b.Cities[j].Population;

                    result += cityPopulationProduct * distance / countryPopulationProduct;
                }
            }

            return (a, b, result);
        }

        [Pure]
        [NotNull]
        public static IEnumerable<(Country A, Country B, double Distance)> Distance([NotNull][ItemNotNull] IEnumerable<Country> countries)
        {
            if (countries is null)
            {
                throw new ArgumentNullException(nameof(countries));
            }

            return countries.GroupBy(x => x.Year).SelectMany(Distance);
        }

        [Pure]
        [NotNull]
        private static IEnumerable<(Country A, Country B, double Distance)> Distance([NotNull][ItemNotNull] IGrouping<string, Country> countries)
        {
            if (countries is null)
            {
                throw new ArgumentNullException(nameof(countries));
            }

            Country[] countryArray = countries.ToArray();

            (Country A, Country B, double Distance)[] results = new (Country A, Country B, double Distance)[countryArray.Length * countryArray.Length];

            Parallel.For(0, countryArray.Length, i =>
            {
                for (int j = 0; j < countryArray.Length; j++)
                {
                    results[countryArray.Length * i + j] = Distance(countryArray[i], countryArray[j]);
                }
            });

            return results;
        }

        /// <summary>
        /// Custom JSON converter for the <see cref="Country" /> class.
        /// </summary>
        private sealed class CountryJsonConverter : JsonConverter
        {
            /// <summary>
            /// True if the type implements <see cref="T:AD.Distances.Country" />; otherwise false.
            /// </summary>
            /// <param name="objectType">
            /// The type to compare.
            /// </param>
            public override bool CanConvert(Type objectType)
            {
                return typeof(Country).GetTypeInfo().IsAssignableFrom(objectType);
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
                    new Country(
                        jObject.Value<string>(nameof(Name)),
                        jObject.Value<string>(nameof(Year)),
                        jObject.Value<double>(nameof(Population)),
                        jObject.Values<City>(nameof(Cities)));
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
                Country country = (Country)value;

                JToken token =
                    new JObject(
                        new JProperty(nameof(Name), country.Name),
                        new JProperty(nameof(Year), country.Year),
                        new JProperty(nameof(Population), country.Population),
                        new JProperty(nameof(Cities), 
                            new JArray(country.Cities.Select(JToken.FromObject))));

                token.WriteTo(writer);
            }
        }
    }
}