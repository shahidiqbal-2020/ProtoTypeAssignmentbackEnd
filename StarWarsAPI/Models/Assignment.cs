using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StarWarsAPI.Models
{
    public class Assignment
    {
        
        public List<Person> MostAppearedCharacter { get; set; }

        public List<FilmSpecies> MostAppearedSpecies { get; set; }

        public List<LongestOpeningCrawl> LongestCrawlMovie { get; set; }

        public List<PlanetVehiclePilots> PlanetMostVehiclePilots { get; set; }

    }
}
