using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FokkersFishing.Api.Helpers;
using FokkersFishing.Api.Models.Dtos;
using FokkersFishing.Api.Models.Entities;
using FokkersFishing.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FokkersFishing.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class TeamController : ControllerBase
    {
        private readonly IFokkersDbService _fokkersDbService;
        private readonly UserHelper _userHelper;

        public TeamController(IFokkersDbService fokkersDbService, UserHelper userHelper)
        {
            _fokkersDbService = fokkersDbService;
            _userHelper = userHelper;
        }

        private async Task<Team> BuildTeam(TeamData teamData)
        {
            var team = teamData.GetTeam();
            var teamMembersData = await _fokkersDbService.GetTeamMembersAsync(team.Id);
            foreach (var teamMember in teamMembersData)
            {
                team.Users.Add(_userHelper.GetUser(teamMember.UserEmail));
            }
            return team;
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, User")]
        public async Task<ActionResult<List<Team>>> Get()
        {
            var teamsData = await _fokkersDbService.GetTeamsAsync();
            if (teamsData == null) return NotFound();
            var teams = new List<Team>();
            foreach (var teamData in teamsData) teams.Add(await BuildTeam(teamData));
            return teams;
        }

        [HttpGet("data")]
        [Authorize(Roles = "ApiUser", AuthenticationSchemes = AuthenticationSchemaNames.BasicAuthentication)]
        public async Task<ActionResult<List<Team>>> GetTeamData()
        {
            var teamsData = await _fokkersDbService.GetTeamsAsync();
            if (teamsData == null) return NotFound();
            var teams = new List<Team>();
            foreach (var teamData in teamsData) teams.Add(await BuildTeam(teamData));
            return teams;
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Administrator, User")]
        public async Task<ActionResult<Team>> GetById(Guid id)
        {
            var teamData = await _fokkersDbService.GetTeamAsync(id.ToString());
            if (teamData == null) return NotFound();
            return await BuildTeam(teamData);
        }

        [HttpGet("byuser")]
        [Authorize(Roles = "Administrator, User")]
        public async Task<ActionResult<Team>> GetTeamByUser()
        {
            var user = _userHelper.GetUser();
            var teamMembersData = await _fokkersDbService.GetTeamMembersFromMemberAsync(user.Email);
            if (teamMembersData == null) return NotFound();

            var member = teamMembersData.FirstOrDefault(tm => tm.UserEmail == user.Email);
            if (member == null) return NotFound();

            var teamData = await _fokkersDbService.GetTeamAsync(member.TeamId.ToString());
            if (teamData == null) return NotFound();

            var team = teamData.GetTeam();
            foreach (var teamMember in teamMembersData)
            {
                team.Users.Add(_userHelper.GetUser(teamMember.UserEmail));
            }
            return team;
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<Team>> CreateTeam(Team team)
        {
            team.Id = Guid.NewGuid();
            var teamData = new TeamData
            {
                RowKey = team.Id.ToString(),
                Name = team.Name,
                Description = team.Description
            };
            await _fokkersDbService.AddTeamAsync(teamData);
            return CreatedAtAction(nameof(CreateTeam), new { id = team.Id }, team);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<Team>> PutTeam(Guid id, Team team)
        {
            var updateTeam = await _fokkersDbService.GetTeamAsync(id.ToString());
            if (updateTeam == null) return Forbid();

            updateTeam.Name = team.Name;
            updateTeam.Description = team.Description;
            await _fokkersDbService.UpdateTeamAsync(updateTeam);

            // Rebuild membership: clear then re-add.
            await _fokkersDbService.DeleteTeamMembersAsync(id);
            foreach (var user in team.Users)
            {
                await _fokkersDbService.AddTeamMemberAsync(new TeamMemberData
                {
                    RowKey = Guid.NewGuid().ToString(),
                    TeamId = id,
                    UserEmail = user.Email
                });
            }
            return team;
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<Team>> DeleteTeam(Guid id)
        {
            var teamData = await _fokkersDbService.GetTeamAsync(id.ToString());
            if (teamData == null) return NotFound();
            await _fokkersDbService.DeleteTeamAsync(id.ToString());
            return NoContent();
        }
    }
}
