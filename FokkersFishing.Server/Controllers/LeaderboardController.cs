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
    [Route("[controller]")]
    public class LeaderboardController : Controller
    {
        private readonly ILogger<CatchController> _logger;
        private readonly IFokkersDbService _fokkersDbService;

        public LeaderboardController(ILogger<CatchController> logger, IFokkersDbService cosmosDbService)
        {
            _logger = logger;
            _fokkersDbService = cosmosDbService;
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Catch>>> GetAll()
        {
            IEnumerable<Catch> catchesMade = await _fokkersDbService.GetItemsAsync("SELECT * FROM c ORDER BY c.length DESC");
            if (catchesMade == null)
            {
                return NotFound();
            }
            return catchesMade.ToList();
        }

        [HttpGet("fishermen")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<FisherMan>>> GetFishermen()
        {
            _logger.LogError("In GetFishermen");
            IEnumerable<FisherMan> fishermen = await _fokkersDbService.GetFishermenAsync("SELECT SUM(c.length) AS totalLength, SUM(1) as fishCount, c.userName as userName FROM c GROUP BY c.userName");
            //IEnumerable<FisherMan> fishermen = await _fokkersDbService.GetFishermenAsync("SELECT * FROM c ORDER BY c.length DESC");
            _logger.LogError("In GetFishermen: " + fishermen.Count().ToString());
            if (fishermen == null)
            {
                return NotFound();
            }
            return fishermen.OrderByDescending(f => f.TotalLength).ToList();
        }

    } // end c
} // end ns
