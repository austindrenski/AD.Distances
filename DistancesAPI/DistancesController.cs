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
    [Route("[controller]")]
    public class DistancesController : Controller
    {
        [HttpGet]
        public IActionResult PopulationWeightedDistance()
        {
            return Json(new { Message = "Submit a JSON array of Country objects." });
        }

        [HttpPost("json")]
        public IActionResult PopulationWeightedDistanceFromJson([FromBody] IEnumerable<Country> countries)
        {
            return Json(Country.Distance(countries));
        }

        [HttpPost("csv")]
        public IActionResult PopulationWeightedDistanceFromDelimited([FromBody] string countries)
        {
            return 
                PopulationWeightedDistanceFromDelimited(
                    countries.Split(
                        new char[] { '\r', '\n' }, 
                        StringSplitOptions.RemoveEmptyEntries));
        }

        //[HttpPost("csv")]
        public IActionResult PopulationWeightedDistanceFromDelimited([FromBody] IEnumerable<string> countries)
        {
            Country[] dataArray =
                countries.SplitDelimitedLine(',')
                         .Skip(1)
                         .Select(x => x.Select(y => y.Trim()).ToArray())
                         .Select(
                             x => new
                             {
                                 Year = x[0],
                                 Country = x[1],
                                 Population = double.Parse(x[2]),
                                 City = new City(x[3], double.Parse(x[4]), new Coordinates(double.Parse(x[5]), double.Parse(x[6])))
                             })

                         .Where(x => x.Year == "2015")

                         .GroupBy(
                             x => new
                             {
                                 x.Year,
                                 x.Country,
                                 x.Population
                             })
                         .Select(x => new Country(x.Key.Country, x.Key.Population, x.Select(y => y.City)))
                         .ToArray();

            IEnumerable<(Country A, Country B, double Distance)> data = Country.Distance(dataArray);

            return Json(data.Select(x => new { A = x.A.Name, B = x.B.Name, x.Distance }));
        }
    }
}