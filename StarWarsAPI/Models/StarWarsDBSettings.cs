using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StarWarsAPI.Models
{
    public interface IStarWarsDBSettings
    {
        string ConnectionString { get; set; }
    }

    public class StarWarsDBSettings : IStarWarsDBSettings
    {
        public string ConnectionString { get; set; }
    }
}
