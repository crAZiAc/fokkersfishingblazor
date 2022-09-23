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

namespace FokkersFishing.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class CompetitionController : Controller
    {
        private readonly ILogger<CompetitionController> _logger;
        private readonly IFokkersDbService _fokkersDbService;

        public CompetitionController(ILogger<CompetitionController> logger, IFokkersDbService cosmosDbService)
        {
            _logger = logger;
            _fokkersDbService = cosmosDbService;
        }

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<Competition>>> Get()
        {
            IEnumerable<CompetitionData> competitionsData;
            competitionsData = await _fokkersDbService.GetCompetitionsAsync();
            if (competitionsData == null)
            {
                return NotFound();
            }
            List<Competition> competitions = new List<Competition>();
            foreach (CompetitionData competitionData in competitionsData)
            {
                competitions.Add(competitionData.GetCompetition());
            }

            return competitions.ToList();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Administrator, User")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Competition>> GetById(Guid id)
        {
            CompetitionData competitionData = await _fokkersDbService.GetCompetitionAsync(id.ToString());
            if (competitionData == null)
            {
                return NotFound();
            }
            else
            {
                return competitionData.GetCompetition();
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Catch>> CreateCompetition(Competition competition)
        {
            competition.Id = Guid.NewGuid();

            CompetitionData competitionData = new CompetitionData();
            competitionData.RowKey = competition.Id.ToString();
            competitionData.Name = competition.CompetitionName;
            competitionData.StartDate = competition.StartDate.ToUniversalTime();
            competitionData.EndDate = competition.EndDate.ToUniversalTime();
            competitionData.Active = competition.Active;
            competitionData.ShowLeaderboardAfterCompetitionEnds = competition.ShowLeaderboardAfterCompetitionEnds;

            await _fokkersDbService.AddCompetitionAsync(competitionData);
            return CreatedAtAction(nameof(CreateCompetition), new { id = competition.Id }, competition);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Competition>> PutCompetition(Guid id, Competition competition)
        {
            CompetitionData updateCompetition = await _fokkersDbService.GetCompetitionAsync(id.ToString());
            if (updateCompetition != null)
            {
                updateCompetition.Name = competition.CompetitionName;
                updateCompetition.StartDate = competition.StartDate.ToUniversalTime();
                updateCompetition.EndDate = competition.EndDate.ToUniversalTime();
                updateCompetition.ShowLeaderboardAfterCompetitionEnds = competition.ShowLeaderboardAfterCompetitionEnds;

                await _fokkersDbService.UpdateCompetitionAsync(updateCompetition);
                return competition;

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
        public async Task<ActionResult<Competition>> DeleteCompetition(Guid id)
        {
            var competitionData = await _fokkersDbService.GetCompetitionAsync(id.ToString());

            if (competitionData == null)
            {
                return NotFound();
            }
            else
            {
                await _fokkersDbService.DeleteCompetitionAsync(id.ToString());
                return NoContent();
            }
        }

    } // end c
} // end ns
