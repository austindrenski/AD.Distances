using System;
using System.Collections.Generic;
using System.Linq;
using AD.Distances;
using AD.IO;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace DistancesAPI
{
    [PublicAPI]
    [Route("")]
    public class DistancesController : Controller
    {
        [HttpGet("")]
        [HttpGet("json")]
        public IActionResult PopulationWeightedDistanceFromJson()
            => Json(new { Message = "Submit a JSON array of Country objects." });

        [HttpGet("csv")]
        public IActionResult PopulationWeightedDistanceFromCsv()
            => Json(new { Message = "Submit a CSV string of country-city data." });

        [HttpPost("")]
        [HttpPost("json")]
        public IActionResult PopulationWeightedDistanceFromJson([NotNull] [FromBody] IEnumerable<Country> countries)
        {
            if (countries is null)
                throw new ArgumentNullException(nameof(countries));

            return Json(Country.Distance(countries).Select(x => new { x.A.Year, A = x.A.Name, B = x.B.Name, x.Distance }));
        }

        [HttpPost("csv")]
        public IActionResult PopulationWeightedDistanceFromDelimited([NotNull] [FromBody] string countries)
        {
            if (countries is null)
                throw new ArgumentNullException(nameof(countries));

            IEnumerable<Country> countryData =
                countries.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                         .Skip(1)
                         .Select(x => x.SplitDelimitedLine(','))
                         .Select(x => x.Select(y => y.Trim()).ToArray())
                         .Select(
                              x => new
                              {
                                  Year = x[0],
                                  Country = x[1],
                                  Population = double.Parse(x[2]),
                                  City = new City(x[3], double.Parse(x[4]), new Coordinates(double.Parse(x[5]), double.Parse(x[6])))
                              })
                         .GroupBy(
                              x => new
                              {
                                  x.Year,
                                  x.Country,
                                  x.Population
                              })
                         .Select(x => new Country(x.Key.Country, x.Key.Year, x.Key.Population, x.Select(y => y.City)));

            return PopulationWeightedDistanceFromJson(countryData);
        }
    }
}