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
        private readonly ILogger<LeaderboardController> _logger;
        private readonly IFokkersDbService _fokkersDbService;
        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private UserHelper _userHelper;

        public LeaderboardController(ILogger<LeaderboardController> logger, IFokkersDbService fokkersDbService, IHttpContextAccessor httpContextAccessor, ApplicationDbContext dbContext)
        {
            _logger = logger;
            _fokkersDbService = fokkersDbService;
            _dbContext = dbContext;
            _userHelper = new UserHelper(_httpContextAccessor, _dbContext);
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Catch>>> GetAll()
        {
            IEnumerable<CatchData> catchesMadeData = await _fokkersDbService.GetLeaderboardItemsAsync();
            if (catchesMadeData == null)
            {
                return NotFound();
            }
            List<Catch> catchesMade = new List<Catch>();
            foreach (CatchData catchMadeData in catchesMadeData)
            {
                Catch catchMade = catchMadeData.GetCatch();
                catchMade.UserName = _userHelper.GetUser(catchMade.UserEmail).UserName;
                catchesMade.Add(catchMade);
            }
            return catchesMade.ToList();
        }

        [HttpGet("fishermen")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<FisherMan>>> GetFishermen()
        {
            IEnumerable<FisherMan> fishermen = await _fokkersDbService.GetFishermenAsync();
            if (fishermen == null)
            {
                return NotFound();
            }
            foreach(var fisher in fishermen)
            {
                fisher.UserName = _userHelper.GetUser(fisher.UserEmail).UserName;
            }
            return fishermen.OrderByDescending(f => f.TotalLength).ToList();
        }

        [HttpGet("bigthree")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<BigThree>>> GetBigThree()
        {
            Dictionary<string, BigThree> bigThree = new Dictionary<string, BigThree>();
            IEnumerable<FisherMan> fishermen = await _fokkersDbService.GetFishermenAsync();
            
            // Fill initial dictionary
            foreach(var fisher in fishermen)
            {
                bigThree.Add(fisher.UserEmail, new BigThree { Name = _userHelper.GetUser(fisher.UserEmail).UserName });
            }
            
            // Pikes
            IEnumerable<Catch> pikes = await _fokkersDbService.GetTopCatchAsync("pike");
            if (pikes != null)
            {
                foreach (var pike in pikes)
                {
                    bigThree[pike.UserEmail].Pike = pike;
                }
            }

            // Bass
            IEnumerable<Catch> bass = await _fokkersDbService.GetTopCatchAsync("bass");
            if (bass != null)
            {
                foreach (var bassFish in bass)
                {
                    bigThree[bassFish.UserEmail].Bass = bassFish;
                }
            }

            // Zander
            IEnumerable<Catch> zander = await _fokkersDbService.GetTopCatchAsync("zander");
            if (zander != null)
            {
                foreach (var zanderFish in zander)
                {
                    bigThree[zanderFish.UserEmail].Zander = zanderFish;
                }
            }
            return bigThree.Values.ToList();
        }

       

    } // end c
} // end ns
