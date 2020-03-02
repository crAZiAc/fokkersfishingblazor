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
    public class CatchController : Controller
    {
        private readonly ILogger<CatchController> _logger;
        private readonly IFokkersDbService _fokkersDbService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _dbContext;
        private UserHelper _userHelper;

        public CatchController(ILogger<CatchController> logger, IFokkersDbService cosmosDbService, IHttpContextAccessor httpContextAccessor, ApplicationDbContext dbContext)
        {
            _logger = logger;
            _fokkersDbService = cosmosDbService;
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
            _userHelper = new UserHelper(_httpContextAccessor, _dbContext);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Catch>>> Get()
        {
            // Check incoming ID and get username
            ApplicationUser user = _userHelper.GetUser();
            IEnumerable<Catch> catchesMade = null;
            catchesMade = await _fokkersDbService.GetItemsAsync("select * from c where c.userName = '" + user.Email + "'");
            if (catchesMade == null)
            {
                return NotFound();
            }
            return catchesMade.ToList();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Catch>> GetById(string id)
        {
            var catchMade = await _fokkersDbService.GetItemAsync(id);

            if (catchMade == null)
            {
                return NotFound();
            }
            return catchMade;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Catch>> Create(Catch catchMade)
        {
            ApplicationUser user = _userHelper.GetUser();
            catchMade.Id = Guid.NewGuid();
            catchMade.LogDate = DateTime.Now;
            catchMade.CatchNumber = _fokkersDbService.GetCatchNumberCount(user.Email).Result + 1;
            catchMade.GlobalCatchNumber = _fokkersDbService.GetGlobalCatchNumberCount().Result + 1;
            catchMade.UserName = user.Email;

            await _fokkersDbService.AddItemAsync(catchMade);
            return CreatedAtAction(nameof(GetById), new { id = catchMade.Id }, catchMade);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Catch>> Put(Guid id, Catch catchMade)
        {
            catchMade.EditDate = DateTime.Now;
            await _fokkersDbService.UpdateItemAsync(id.ToString(), catchMade);
            return catchMade;
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Catch>> Delete(Guid id)
        {
            await _fokkersDbService.DeleteItemAsync(id.ToString());
            return NoContent();
        }


    } // end c
} // end ns
