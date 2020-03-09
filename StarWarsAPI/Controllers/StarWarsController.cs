using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StarWarsAPI.Models;
using StarWarsAPI.Services;

namespace StarWarsAPI.Controllers
{
    [Route("API/[controller]")]
    [ApiController]
    public class StarWarsController : ControllerBase
    {
        StarWarsDBService _starWarsDBService;
        public StarWarsController(StarWarsDBService starWarsDBService)
        {
            _starWarsDBService = starWarsDBService;
        }
        [EnableCors]
        [HttpGet]
        public Assignment Get()
        {
            Assignment assignment = new Assignment();
            assignment.LongestCrawlMovie = _starWarsDBService.GetLongestOpeningCrawlMovie();
            assignment.MostAppearedCharacter = _starWarsDBService.GetMostAppearedCharacter();
            assignment.MostAppearedSpecies = _starWarsDBService.GetMostAppearedSpecies();
            assignment.PlanetMostVehiclePilots = _starWarsDBService.GetPlanetMostPilots();
            return assignment;

        }
    }
}