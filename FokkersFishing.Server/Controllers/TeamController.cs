using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FokkersFishing.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FokkersFishing.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using FokkersFishing.Data;
using FokkersFishing.Shared.Models;
using FokkersFishing.Server.Helpers;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace FokkersFishing.Controllers
{
    
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class TeamController : Controller
    {
        private readonly ILogger<TeamController> _logger;
        private readonly IFokkersDbService _fokkersDbService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _dbContext;
        private UserHelper _userHelper;

        public TeamController(ILogger<TeamController> logger, IFokkersDbService cosmosDbService, IHttpContextAccessor httpContextAccessor, ApplicationDbContext dbContext)
        {
            _logger = logger;
            _fokkersDbService = cosmosDbService;
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
            _userHelper = new UserHelper(_httpContextAccessor, _dbContext);
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, User")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<Team>>> Get()
        {
            IEnumerable<TeamData> teamsData;
            teamsData = await _fokkersDbService.GetTeamsAsync();
            if (teamsData == null)
            {
                return NotFound();
            }
            List<Team> teams = new List<Team>();
            foreach (TeamData teamData in teamsData)
            {
                Team team = teamData.GetTeam();
                var teamMembersData = await _fokkersDbService.GetTeamMembersAsync(team.Id);
                foreach(var teamMember in teamMembersData)
                {
                    User user = _userHelper.GetUser(teamMember.UserEmail);
                    team.Users.Add(user);
                }
                teams.Add(team);
            }

            return teams.ToList();
        }

        [HttpGet("data")]
        [Authorize(Roles = "ApiUser", AuthenticationSchemes = AuthenticationSchemaNames.BasicAuthentication)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<Team>>> GetTeamData()
        {
            IEnumerable<TeamData> teamsData;
            teamsData = await _fokkersDbService.GetTeamsAsync();
            if (teamsData == null)
            {
                return NotFound();
            }
            List<Team> teams = new List<Team>();
            foreach (TeamData teamData in teamsData)
            {
                Team team = teamData.GetTeam();
                var teamMembersData = await _fokkersDbService.GetTeamMembersAsync(team.Id);
                foreach (var teamMember in teamMembersData)
                {
                    User user = _userHelper.GetUser(teamMember.UserEmail);
                    team.Users.Add(user);
                }
                teams.Add(team);
            }

            return teams.ToList();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Administrator, User")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Team>> GetById(Guid id)
        {
            TeamData teamData = await _fokkersDbService.GetTeamAsync(id.ToString());
            if (teamData == null)
            {
                return NotFound();
            }
            else
            {
                Team team = teamData.GetTeam();
                var teamMembersData = await _fokkersDbService.GetTeamMembersAsync(team.Id);
                foreach (var teamMember in teamMembersData)
                {
                    User user = _userHelper.GetUser(teamMember.UserEmail);
                    team.Users.Add(user);
                }
                return team;
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Catch>> CreateTeam(Team team)
        {
            team.Id = Guid.NewGuid();

            TeamData teamData = new TeamData();
            teamData.RowKey = team.Id.ToString();
            teamData.Name = team.Name;
            teamData.Description = team.Description;

            await _fokkersDbService.AddTeamAsync(teamData);
            return CreatedAtAction(nameof(CreateTeam), new { id = team.Id }, team);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Team>> PutTeam(Guid id, Team team)
        {
            TeamData updateTeam = await _fokkersDbService.GetTeamAsync(id.ToString());
            if (updateTeam != null)
             {
                updateTeam.Name = team.Name;
                updateTeam.Description = team.Description;

                await _fokkersDbService.UpdateTeamAsync(updateTeam);

                // Team Members
                // Delete all first
                await _fokkersDbService.DeleteTeamMembersAsync(id);
                foreach (User user in team.Users)
                {
                    TeamMemberData teamMemberData = new TeamMemberData
                    {
                        RowKey = Guid.NewGuid().ToString(),
                        TeamId = id,
                        UserEmail = user.Email
                    };
                    await _fokkersDbService.AddTeamMemberAsync(teamMemberData);
                }

                return team;

            }
            else
            {
                return Forbid();
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Team>> DeleteTeam(Guid id)
        {
            var teamData = await _fokkersDbService.GetTeamAsync(id.ToString());

            if (teamData == null)
            {
                return NotFound();
            }
            else
            {
                await _fokkersDbService.DeleteTeamAsync(id.ToString());
                return NoContent();
            }
        }

    } // end c
} // end ns
