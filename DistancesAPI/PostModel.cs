using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AD.Distances;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

namespace DistancesAPI
{
    public class PostModel
    {
        public string Year { get; set; }

        public string Country { get; set; }

        public double Population { get; set; }

        public string City { get; set; }

        public double CityPopulation { get; set; }

        public double CityLatitude { get; set; }

        public double CityLongitude { get; set; }

        public static IEnumerable<Country> Convert(IEnumerable<PostModel> model)
        {
            return
                model.GroupBy(
                         x => new
                         {
                             x.Year,
                             x.Country,
                             x.Population
                         })
                     .Where(x => x.Key.Year == "2015")
                     .OrderBy(x => x.Key.Year)
                     .ThenBy(x => x.Key.Country)
                     .Select(
                         x =>
                             new Country(
                                 x.Key.Country,
                                 x.Key.Population,
                                 x.Select(
                                     y =>
                                         new City(
                                             y.City,
                                             y.CityPopulation,
                                             new Location(
                                                 y.CityLatitude,
                                                 y.CityLongitude)))))
                     .ToArray();
        }
    }
}
