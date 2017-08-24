//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using AD.IO;

//namespace GravityDistance
//{
//    public static class Program
//    {
//        public static void Main()
//        {
//            const string file = "\\users\\adren\\desktop\\gravity distance data.csv";
//            // year iso3 population city city_population latitude longitude

//            Country[] dataArray =
//                File.ReadLines(file)
//                    .SplitDelimitedLine(',')
//                    .Skip(1)
//                    .Select(x => x.Select(y => y.Trim()).ToArray())
//                    .Select(
//                        x => new
//                        {
//                            Year = x[0],
//                            Country = x[1],
//                            Population = double.Parse(x[2]),
//                            City = new City(x[3], double.Parse(x[4]), new Location(double.Parse(x[5]), double.Parse(x[6])))
//                        })
//                    .GroupBy(
//                        x => new
//                        {
//                            x.Year,
//                            x.Country,
//                            x.Population
//                        })
//                    .Where(x => x.Key.Year == "2015")
//                    .OrderBy(x => x.Key.Year)
//                    .ThenBy(x => x.Key.Country)
//                    .Select(x => new Country(x.Key.Country, x.Key.Population, x.Select(y => y.City)))
//                    .ToArray();


//            IEnumerable<(Country A, Country B, double Distance)> results = Country.Distance(dataArray);
//        }
//    }
//}