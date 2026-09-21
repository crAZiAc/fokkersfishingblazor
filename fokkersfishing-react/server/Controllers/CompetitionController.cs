using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class CompetitionController : ControllerBase
    {
        private readonly IFokkersDbService _fokkersDbService;

        public CompetitionController(IFokkersDbService fokkersDbService)
        {
            _fokkersDbService = fokkersDbService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<Competition>>> Get()
        {
            var competitionsData = await _fokkersDbService.GetCompetitionsAsync();
            if (competitionsData == null) return NotFound();
            return competitionsData.Select(c => c.GetCompetition()).ToList();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Administrator, User")]
        public async Task<ActionResult<Competition>> GetById(Guid id)
        {
            var competitionData = await _fokkersDbService.GetCompetitionAsync(id.ToString());
            if (competitionData == null) return NotFound();
            return competitionData.GetCompetition();
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<Competition>> CreateCompetition(Competition competition)
        {
            competition.Id = Guid.NewGuid();
            var competitionData = new CompetitionData
            {
                RowKey = competition.Id.ToString(),
                Name = competition.CompetitionName,
                StartDate = competition.StartDate.ToUniversalTime(),
                EndDate = competition.EndDate.ToUniversalTime(),
                Active = competition.Active,
                ShowLeaderboardAfterCompetitionEnds = competition.ShowLeaderboardAfterCompetitionEnds
            };
            await _fokkersDbService.AddCompetitionAsync(competitionData);
            return CreatedAtAction(nameof(CreateCompetition), new { id = competition.Id }, competition);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<Competition>> PutCompetition(Guid id, Competition competition)
        {
            var updateCompetition = await _fokkersDbService.GetCompetitionAsync(id.ToString());
            if (updateCompetition == null) return Forbid();

            updateCompetition.Name = competition.CompetitionName;
            updateCompetition.StartDate = competition.StartDate.ToUniversalTime();
            updateCompetition.EndDate = competition.EndDate.ToUniversalTime();
            updateCompetition.ShowLeaderboardAfterCompetitionEnds = competition.ShowLeaderboardAfterCompetitionEnds;
            updateCompetition.Active = competition.Active;

            await _fokkersDbService.UpdateCompetitionAsync(updateCompetition);
            return competition;
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<Competition>> DeleteCompetition(Guid id)
        {
            var competitionData = await _fokkersDbService.GetCompetitionAsync(id.ToString());
            if (competitionData == null) return NotFound();
            await _fokkersDbService.DeleteCompetitionAsync(id.ToString());
            return NoContent();
        }
    }
}
