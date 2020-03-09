using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using StarWarsAPI;
using StarWarsAPI.Models;

namespace StarWarsAPI.Services
{
    public class StarWarsDBService
    {
        #region storedProcedures
        private string usp_GetLongestOpeningCrawl = @"Begin
                                                        declare @MaxOpeningCrawl int
                                                        select @MaxOpeningCrawl =  max(len(opening_crawl)) from films
                                                        select Title, len(opening_crawl) Length from films where len(opening_crawl) = @MaxOpeningCrawl
                                                        end";
        private string usp_MostAppearedCharacter = @"Begin
                            Declare @MostAppearedCharacter int
                            select @MostAppearedCharacter = max(Movies) from (
                                select people_id, count(film_id) Movies 
                                    from films_Characters group by people_id) MovieCharacters
                                    ----- if there is a tie, will have more than 1 row
                            select p.name, count(film_id) Movies 
                            from films_Characters fc
                            inner join people p on p.id = fc.people_id group by [name]
                            having count(film_id) = @MostAppearedCharacter
                            end";
        private string usp_MostAppearedSpecies = @"Declare @MostAppearedSpecies int
                                                    select @MostAppearedSpecies = max(Movies) from (
                                                           select species_id, count(film_id) Movies 
                                                            from films_species group by species_id) MovieSpecies
                                                    ;with SpeciesPeople_CTE (species_id, SpeciesPeopleCount)
                                                    as
                                                     (
                                                        select species_id, count(people_id) SpeciesPeopleCount from species_people
                                                        group by species_id
                                                    ),
                                                     FilmSpecies_CTE (species_id, Movies)
                                                    AS
                                                    (
                                                        select species_id, count(film_id) Movies 
                                                        from films_species fs group by species_id
                                                        having count(film_id) = @MostAppearedSpecies
                                                    )

                                                    select s.id, s.Name, fs.Movies Films, sp.SpeciesPeopleCount People, Concat(s.Name,  ' (' , sp.SpeciesPeopleCount  , ')') as FormatedString
                                                    from FilmSpecies_CTE fs
                                                    inner join species s on s.id = fs.species_id
                                                    inner join SpeciesPeople_CTE sp on sp.species_id = fs.species_id
                                                    ";
        private string usp_PlanetMostPilots = @"Begin
                                drop table if Exists #PlanetPilots
                                Declare @MaxVehiclePilots int
                                Declare @Planet_MaxVehiclePilots int
                                select p.id planet_id, p.name planet, ppl.id people_id, ppl.name pilotName, s.id species_id, s.name species
                                into #PlanetPilots
                                from films_planets fp
                                inner join planets p on p.id = fp.planet_id
                                inner join films f on f.id = fp.film_id
                                inner join films_vehicles fv on fv.film_id = fp.film_id
                                inner join vehicles v on v.id = fv.vehicle_id
                                inner join vehicles_pilots vp on vp.vehicle_id = v.id
                                inner join people ppl on ppl.id = vp.people_id
                                inner join species_people sp on sp.people_id = ppl.id
                                inner join species s on s.id = sp.species_id


                                select @MaxVehiclePilots = max(NoOfPlanetPilots) from 
	                                ( select planet_id, count(pilotName) NoOfPlanetPilots from #PlanetPilots group by planet_id ) MaxPlanetPilots

	                                --select planet_id, planet, count(pilotName) NoOfPlanetPilots from #PlanetPilots group by planet_id, planet
	                                --	having count(pilotName) = @MaxVehiclePilots

	                                ;with CTE_MAXPlanetPIlots as 
	                                ( select planet_id, count(pilotName) NoOfPlanetPilots from #PlanetPilots group by planet_id, planet
		                                having count(pilotName) = @MaxVehiclePilots)
	                                select pp.planet_id, pp.planet, cmp.NoOfPlanetPilots, Concat(pp.pilotName, ' - ' , pp.Species) as 'Pilot-Species'
	                                from #PlanetPilots pp
	                                inner join CTE_MAXPlanetPIlots cmp on cmp.planet_id = pp.planet_id

                                drop table if Exists #PlanetPilots 
                                end
                                ";
        #endregion
        private string _SqlConnectionString;


        public StarWarsDBService(IStarWarsDBSettings settings)
        {
            _SqlConnectionString = settings.ConnectionString;
        }

        public List<LongestOpeningCrawl> GetLongestOpeningCrawlMovie()
        {
            List<LongestOpeningCrawl> _LongestOpeningCrawl = new List<LongestOpeningCrawl>();

            LongestOpeningCrawl longestOpening;
            try
            { 
                using (SqlConnection connection = new SqlConnection(_SqlConnectionString))
                {
                    SqlCommand command = new SqlCommand(usp_GetLongestOpeningCrawl, connection);
                    DataSet resultSet = ExecuteDataSet(command);
                    if (resultSet != null && resultSet.Tables.Count > 0)
                    {
                        foreach (DataRow row in resultSet.Tables[0].Rows)
                        {
                            longestOpening = new LongestOpeningCrawl();
                            longestOpening.Title = row["Title"].ToString();
                            longestOpening.CrawlLength = row["Length"].ToString();
                            _LongestOpeningCrawl.Add(longestOpening);
                        }
                    }

                }
            }
            catch (Exception ex)
            { 
                
            }
            return _LongestOpeningCrawl;
        }

        public List<Person> GetMostAppearedCharacter()
        {
            List<Person> _person = new List<Person>();

            Person person;
            try
            {
                using (SqlConnection connection = new SqlConnection(_SqlConnectionString))
                {
                    SqlCommand command = new SqlCommand(usp_MostAppearedCharacter, connection);
                    DataSet resultSet = ExecuteDataSet(command);
                    if (resultSet != null && resultSet.Tables.Count > 0)
                    {
                        foreach (DataRow row in resultSet.Tables[0].Rows)
                        {
                            person = new Person();
                            person.Name = row["Name"].ToString();
                            person.Movies = row["Movies"].ToString();
                            _person.Add(person);
                        }
                    }

                }
            }
            catch (Exception ex)
            {

            }
            return _person;

        }

        public List<FilmSpecies> GetMostAppearedSpecies()
        {
            List<FilmSpecies> _filmSpecies = new List<FilmSpecies>();

            FilmSpecies filmSpecies;
            try
            {
                using (SqlConnection connection = new SqlConnection(_SqlConnectionString))
                {
                    SqlCommand command = new SqlCommand(usp_MostAppearedSpecies, connection);
                    DataSet resultSet = ExecuteDataSet(command);
                    if (resultSet != null && resultSet.Tables.Count > 0)
                    {
                        foreach (DataRow row in resultSet.Tables[0].Rows)
                        {
                            filmSpecies = new FilmSpecies();
                            filmSpecies.Species = row["Name"].ToString();
                            filmSpecies.Films = row["Films"].ToString();
                            filmSpecies.PeopleCount = row["People"].ToString();
                            _filmSpecies.Add(filmSpecies);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return _filmSpecies;
        }

        public List<PlanetVehiclePilots> GetPlanetMostPilots()
        {
            List<PlanetVehiclePilots> _PlanetVehiclePilots = new List<PlanetVehiclePilots>();

            PlanetVehiclePilots planetVehiclePilots;
            try
            {
                using (SqlConnection connection = new SqlConnection(_SqlConnectionString))
                {
                    SqlCommand command = new SqlCommand(usp_PlanetMostPilots, connection);
                    DataSet resultSet = ExecuteDataSet(command);
                    if (resultSet != null && resultSet.Tables.Count > 0)
                    {
                        DataTable planets = resultSet.Tables[0].DefaultView.ToTable(true,  "planet_id", "planet", "NoOfPlanetPilots");
                        foreach (DataRow planet in planets.Rows)
                        {
                            planetVehiclePilots = new PlanetVehiclePilots();
                            string planetPilotSpecies = "";
                            planetVehiclePilots.Planet = planet["planet"].ToString();
                            planetVehiclePilots.NimberOfPilots = planet["NoOfPlanetPilots"].ToString();

                            DataRow[] pilots = resultSet.Tables[0].Select("planet_id = " + planet["planet_id"].ToString());
                            foreach (DataRow pilot in pilots)
                            {
                                planetPilotSpecies += pilot["Pilot-Species"].ToString() + ", ";
                            }
                            planetVehiclePilots.pilotSpecies = planetPilotSpecies.Trim().TrimEnd(',');
                            _PlanetVehiclePilots.Add(planetVehiclePilots);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return _PlanetVehiclePilots;


        }

        private DataSet ExecuteDataSet(SqlCommand command)
        {
            var ds = new DataSet();
            using (var dataAdapter = new SqlDataAdapter(command))
            {
                dataAdapter.Fill(ds);
            }
            return ds;
        }
    }
}
