using System;
using System.Collections.Generic;
using System.Linq;
using AD.Distances;
using AD.IO;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DistancesAPI.Controllers
{
    /// <summary>
    /// Provides endpoints to calculate population-weighted distances.
    /// </summary>
    /// <inheritdoc />
    [PublicAPI]
    [FormatFilter]
    [Route("")]
    [ApiVersion("1.0")]
    public class DistancesController : Controller
    {
        /// <summary>
        /// Returns instructions for submitting JSON data.
        /// </summary>
        /// <returns>
        /// A message with instructions for submitting JSON data.
        /// </returns>
        [HttpGet("")]
        [HttpGet("json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult PopulationWeightedDistanceFromJson()
            => Json(new { Message = "Submit a JSON array of Country objects." });

        /// <summary>
        /// Returns instructions for submitting CSV data.
        /// </summary>
        /// <returns>
        /// A message with instructions for submitting CSV data.
        /// </returns>
        [HttpGet("csv")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult PopulationWeightedDistanceFromCsv()
            => Json(new { Message = "Submit a CSV string of country-city data." });

        /// <summary>
        /// Receives country data from the user in JSON format.
        /// </summary>
        /// <param name="countries">The countries for which to calculate population-weighted distances.</param>
        /// <returns>
        /// The calculated distances.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="countries"/></exception>
        [HttpPost("")]
        [HttpPost("json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult PopulationWeightedDistanceFromJson([NotNull] [FromBody] IEnumerable<Country> countries)
        {
            if (countries == null)
                throw new ArgumentNullException(nameof(countries));

            return Json(Country.Distance(countries).Select(x => new { x.A.Year, A = x.A.Name, B = x.B.Name, x.Distance }));
        }

        /// <summary>
        /// Receives country data from the user in CSV format.
        /// </summary>
        /// <param name="countries">The countries for which to calculate population-weighted distances.</param>
        /// <returns>
        /// The calculated distances.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="countries"/></exception>
        [HttpPost("csv")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult PopulationWeightedDistanceFromDelimited([NotNull] [FromBody] string countries)
        {
            if (countries == null)
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