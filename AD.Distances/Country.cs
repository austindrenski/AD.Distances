using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace AD.Distances
{
    [PublicAPI]
    public class Country
    {
        [NotNull]
        public string Name { get; }

        public double Population { get; }

        [NotNull]
        [ItemNotNull]
        public IReadOnlyList<City> Cities { get; }

        public Country([NotNull] string name, double population, [NotNull][ItemNotNull] IEnumerable<City> cities)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
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

            Country[] countryArray = countries as Country[] ?? countries.ToArray();

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
    }
}