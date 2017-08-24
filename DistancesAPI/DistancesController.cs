using System.Collections.Generic;
using System.Linq;
using AD.Distances;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        [HttpPost]
        public IActionResult PopulationWeightedDistance([FromBody] IList<PostModel> countries)
        {

            //var ab = countries.Select(x => x.ToObject<PostModel>()).ToArray();
            var ac = PostModel.Convert(countries).ToArray();

            var a = Country.Distance(ac).ToArray();

            return Json(a);
        }
    }
}