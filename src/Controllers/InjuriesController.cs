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
    [ApiController]
    public class InjuriesController : Controller
    {
        private readonly IStatEngine<Data.DataObjects.Injury> engine;
        private readonly IStatEngine<RosterEntry> rosterEngine;
        private readonly IStatEngine<Player> playerEngine;
        private readonly IStatEngine<Team> teamEngine;

        public InjuriesController(SqlContext ctx)
        {
            engine = new InjuryEngine(ctx);
            rosterEngine = new RosterEntryEngine(ctx);
            playerEngine = new PlayerEngine(ctx);
            teamEngine = new TeamEngine(ctx);
        }
        
        [HttpGet("api/{controler}/{id}")]
        public IActionResult GetInJury(int id)
        {
            try
            {
                var injury = engine.Load(id);
                var player = playerEngine.Load(injury.PlayerId);
                var roster = rosterEngine.LoadAll().Where(c => c.PlayerId == injury.PlayerId).FirstOrDefault();
                var team = teamEngine.Load(roster.TeamId);
                InjuryModel model = new InjuryModel()
                {
                    Id = injury.Id,
                    PlayerId = injury.PlayerId,
                    PlayerName = player.PlayerName,
                    GameDate = injury.ScratchDate.ToString("yyyy-MM-dd"),
                    InjuryStatus = injury.InjuryStatus,
                    TeamName = team.TeamName                                                          
                };
                return Ok(model);

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
                if (model.Id > 0)
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
                else
                {
                    Data.DataObjects.Injury injury = new Injury()
                    {
                        PlayerId = model.PlayerId,
                        InjuryStatus = "OUT",
                        ScratchDate = DateTime.Now
                    };
                    engine.Save(injury);
                    return Ok();

                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
