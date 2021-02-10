using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NbaStats.Data.Context;
using NbaStats.Data.DataObjects;
using NbaStats.Data.Engines;
using NbaStats.Web.Api.Models;

namespace NbaStats.Web.Api.Controllers
{
    [ApiController]
    public class TeamsController : Controller
    {
        private readonly IStatEngine<Team> teamEngine;
        private readonly IStatEngine<Data.DataObjects.Injury> engine;
        private readonly IStatEngine<RosterEntry> rosterEngine;
        private readonly IStatEngine<Player> playerEngine;
        
        public TeamsController(SqlContext ctx)
        {
            engine = new InjuryEngine(ctx);
            rosterEngine = new RosterEntryEngine(ctx);
            playerEngine = new PlayerEngine(ctx);
            teamEngine = new TeamEngine(ctx);
        }

        [HttpGet("api/{controller}")]
        public IActionResult Get()
        {
            try
            {
                var result = teamEngine.LoadAll().ToList().OrderBy(c => c.TeamName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("api/teams/{teamId}/injuries")]
        public IActionResult GetInjuries(int teamId)
        {
            try
            {
                var rosters = rosterEngine.LoadAll().Where(c => c.TeamId == teamId).ToList();
                var injuries = engine.LoadAll().ToList().Where(c => rosters.Any(r => r.PlayerId == c.PlayerId)).ToList();
                var team = teamEngine.Load(teamId);

                List<InjuryModel> models = new List<InjuryModel>();
                foreach (Data.DataObjects.Injury injury in injuries)
                {
                    Player player = playerEngine.Load(injury.PlayerId);

                    InjuryModel model = new InjuryModel()
                    {
                        InjuryStatus = injury.InjuryStatus,
                        Id = injury.Id,
                        GameDate = injury.ScratchDate.ToString("yyyy-MM-dd"),
                        PlayerName = player.PlayerName,
                        TeamName = team.TeamName
                    };
                    models.Add(model);
                }
                return Ok(models.OrderBy(c => c.TeamName));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
