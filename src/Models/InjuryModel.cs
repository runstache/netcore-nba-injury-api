using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NbaStats.Web.Api.Models
{
    public class InjuryModel
    {
        [JsonProperty(PropertyName = "id")]
        public long Id { get; set; }

        [JsonProperty(PropertyName = "playerId")]
        public long PlayerId { get; set; }

        [JsonProperty(PropertyName = "playerName")]
        public string PlayerName { get; set; }

        [JsonProperty(PropertyName = "teamName")]
        public string TeamName { get; set; }

        [JsonProperty(PropertyName = "injuryStatus")]
        public string InjuryStatus { get; set; }

        [JsonProperty(PropertyName = "gameDate")]
        public string GameDate { get; set; }
    }
}
