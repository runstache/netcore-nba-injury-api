using Microsoft.AspNetCore.Mvc;
using NbaStats.Data.Engines;
using NbaStats.Data.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NbaStats.Data.Context;
using NbaStats.Web.Api.Models;

namespace NbaStats.Web.Api.Controllers
{
    
    public class InjuryController : Controller
    {
        private readonly IStatEngine<Data.DataObjects.Injury> engine;
        private readonly IStatEngine<RosterEntry> rosterEngine;
        private readonly IStatEngine<Player> playerEngine;
        private readonly IStatEngine<Team> teamEngine;

        public InjuryController(SqlContext ctx)
        {
            engine = new InjuryEngine(ctx);
            rosterEngine = new RosterEntryEngine(ctx);
            playerEngine = new PlayerEngine(ctx);
            teamEngine = new TeamEngine(ctx);
        }
        
        [HttpGet("api/{controler}/{teamId}")]
        public IActionResult GetByTeamId(int teamId)
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
                return Ok(models);
            } 
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("api/{controller}")]
        public IActionResult Get()
        {
            try
            {
                var injuries = engine.LoadAll().ToList();

                List<InjuryModel> models = new List<InjuryModel>();
                foreach (Data.DataObjects.Injury injury in injuries)
                {
                    Player player = playerEngine.Load(injury.PlayerId);
                    RosterEntry roster = rosterEngine.Query(c => c.PlayerId == player.Id).FirstOrDefault();
                    Team team = teamEngine.Load(roster.TeamId);

                    InjuryModel model = new InjuryModel()
                    {
                        Id = injury.Id,
                        PlayerId = injury.PlayerId,
                        InjuryStatus = injury.InjuryStatus,
                        GameDate = injury.ScratchDate.ToString("yyyy-MM-dd"),
                        PlayerName = player.PlayerName,
                        TeamName = team.TeamName
                    };
                    models.Add(model);
                }
                return Ok(models);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("api/{controller}")]
        public IActionResult Post([FromBody] InjuryModel model)
        {
            try
            {
                Data.DataObjects.Injury injury = new Data.DataObjects.Injury()
                {
                    Id = model.Id,
                    PlayerId = model.PlayerId,
                    InjuryStatus = model.InjuryStatus                    
                };

                DateTime result = new DateTime();
                if (!DateTime.TryParse(model.GameDate, out result))
                {
                    result = DateTime.Now;
                }
                injury.ScratchDate = result;
                engine.Save(injury);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
