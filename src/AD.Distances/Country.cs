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
    /// <summary>
    /// Represents a <see cref="Country"/> with characteristics and cities.
    /// </summary>
    [PublicAPI]
    [JsonConverter(typeof(CountryJsonConverter))]
    public class Country
    {
        /// <summary>
        /// The name of the <see cref="Country"/>.
        /// </summary>
        [NotNull]
        public string Name { get; }

        /// <summary>
        /// The year in which the characteristics were observed.
        /// </summary>
        [NotNull]
        public string Year { get; }

        /// <summary>
        /// The population of the <see cref="Country"/>.
        /// </summary>
        public double Population { get; }

        /// <summary>
        /// The collection of cities observed for this <see cref="Country"/>.
        /// </summary>
        [NotNull]
        [ItemNotNull]
        public IReadOnlyList<City> Cities { get; }

        /// <summary>
        /// Constructs a <see cref="Country"/> for a given year with the given characteristics.
        /// </summary>
        /// <param name="name">
        /// The name of the <see cref="Country"/>.
        /// </param>
        /// <param name="year">
        /// The year in which the characteristics were observed.
        /// </param>
        /// <param name="population">
        /// The population of the <see cref="Country"/>
        /// </param>
        /// <param name="cities">
        /// The collection of cities observed for this <see cref="Country"/>.
        /// </param>
        public Country([NotNull] string name, [NotNull] string year, double population, [NotNull] [ItemNotNull] IEnumerable<City> cities)
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
        public override string ToString()
        {
            return $"({Name}, {Population}, {Cities.Count})";
        }

        /// <summary>
        /// Calculates the population-weighted distance between the collection of countries grouped by year based on the great-circle distance between constituent cities.
        /// </summary>
        /// <param name="countries">
        /// The collection of countries over which to calculate distances.
        /// </param>
        /// <returns>
        /// A collection of tuples containing the input countries and the calculated distance.
        /// </returns>
        [Pure]
        [NotNull]
        public static IEnumerable<(Country A, Country B, double Distance)> Distance([NotNull] [ItemNotNull] IEnumerable<Country> countries)
        {
            if (countries is null)
            {
                throw new ArgumentNullException(nameof(countries));
            }

            return countries.GroupBy(x => x.Year).SelectMany(Distance);
        }

        /// <summary>
        /// Calculates the population-weighted distance between the group (e.g. by year) of countries based on the great-circle distance between constituent cities.
        /// </summary>
        /// <param name="countries">
        /// The group of countries over which to calculate distances.
        /// </param>
        /// <returns>
        /// A collection of tuples containing the input countries and the calculated distance.
        /// </returns>
        [Pure]
        [NotNull]
        public static IEnumerable<(Country A, Country B, double Distance)> Distance([NotNull] [ItemNotNull] IGrouping<string, Country> countries)
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
        /// Calculates the population-weighted distance between two countries based on the great-circle distance between constituent cities.
        /// </summary>
        /// <param name="a">
        /// The first <see cref="Country"/>.
        /// </param>
        /// <param name="b">
        /// The second <see cref="Country"/>.
        /// </param>
        /// <returns>
        /// A tuple containing the input countries and the calculated distance.
        /// </returns>
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

        /// <inheritdoc />
        /// <summary>
        /// Custom JSON converter for the <see cref="T:AD.Distances.Country" /> class.
        /// </summary>
        private sealed class CountryJsonConverter : JsonConverter
        {
            /// <inheritdoc />
            [Pure]
            public override bool CanConvert(Type objectType)
            {
                return typeof(Country).GetTypeInfo().IsAssignableFrom(objectType);
            }

            /// <inheritdoc />
            [Pure]
            [NotNull]
            public override object ReadJson([NotNull] JsonReader reader, Type objectType, [NotNull] object existingValue, [NotNull] JsonSerializer serializer)
            {
                if (reader is null)
                {
                    throw new ArgumentNullException(nameof(reader));
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
                    new Country(
                        jObject.Value<string>(nameof(Name)),
                        jObject.Value<string>(nameof(Year)),
                        jObject.Value<double>(nameof(Population)),
                        jObject.Values<City>(nameof(Cities)));
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

                Country country = (Country) value;

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