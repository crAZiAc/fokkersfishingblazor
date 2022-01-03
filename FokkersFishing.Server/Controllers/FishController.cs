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
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class FishController : Controller
    {
        private readonly ILogger<FishController> _logger;
        private readonly IFokkersDbService _fokkersDbService;

        public FishController(ILogger<FishController> logger, IFokkersDbService cosmosDbService)
        {
            _logger = logger;
            _fokkersDbService = cosmosDbService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Fish>>> Get()
        {
            IEnumerable<Fish> fishes;
            fishes = await _fokkersDbService.GetFishAsync();
            if (fishes == null)
            {
                return NotFound();
            }
            return fishes.ToList();
        }

    } // end c
} // end ns
