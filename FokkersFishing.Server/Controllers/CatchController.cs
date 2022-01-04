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
            IEnumerable<CatchData> catchesMadeData = null;
            catchesMadeData = await _fokkersDbService.GetUserItemsAsync(user.Email);
            if (catchesMadeData == null)
            {
                return NotFound();
            }
            List<Catch> catchesMade = new List<Catch>();
            foreach (CatchData catchMadeData in catchesMadeData)
            {
                catchesMade.Add(catchMadeData.GetCatch());
            }
            return catchesMade.ToList();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Catch>> GetById(Guid id)
        {
            ApplicationUser user = _userHelper.GetUser();
            CatchData catchMade = await _fokkersDbService.GetItemAsync(id.ToString());

            if (catchMade == null)
            {
                return NotFound();
            }
            else
            {
                if (catchMade.UserName == user.Email)
                {
                    return catchMade.GetCatch();
                }
                else
                {
                    return Forbid();
                }
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Catch>> Create(Catch catchMade)
        {
            ApplicationUser user = _userHelper.GetUser();
            catchMade.Id = Guid.NewGuid();
            catchMade.LogDate = DateTime.Now;
            catchMade.EditDate = DateTime.Now;
            catchMade.CatchNumber = _fokkersDbService.GetCatchNumberCount(user.Email).Result + 1;
            catchMade.GlobalCatchNumber = _fokkersDbService.GetGlobalCatchNumberCount().Result + 1;
            catchMade.UserName = user.Email;

            CatchData newCatch = new CatchData();
            newCatch.CatchDate = catchMade.CatchDate.ToUniversalTime();
            newCatch.CatchNumber = catchMade.CatchNumber;
            newCatch.EditDate = catchMade.EditDate.ToUniversalTime();
            newCatch.Fish = catchMade.Fish;
            newCatch.GlobalCatchNumber = catchMade.GlobalCatchNumber;
            newCatch.Length = catchMade.Length;
            newCatch.LogDate = catchMade.LogDate.ToUniversalTime();
            newCatch.RowKey = catchMade.Id.ToString();
            newCatch.UserName = catchMade.UserName;

            await _fokkersDbService.AddItemAsync(newCatch);
            return CreatedAtAction(nameof(GetById), new { id = catchMade.Id }, catchMade);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Catch>> Put(Guid id, Catch catchMade)
        {
            ApplicationUser user = _userHelper.GetUser();
            catchMade.UserName = user.Email;
            catchMade.EditDate = DateTime.Now;

            CatchData updateCatch = await _fokkersDbService.GetItemAsync(id.ToString());
            updateCatch.CatchDate = catchMade.CatchDate.ToUniversalTime();
            updateCatch.CatchNumber = catchMade.CatchNumber;
            updateCatch.EditDate = catchMade.EditDate.ToUniversalTime();
            updateCatch.Fish = catchMade.Fish;
            updateCatch.GlobalCatchNumber = catchMade.GlobalCatchNumber;
            updateCatch.Length = catchMade.Length;
            updateCatch.LogDate = catchMade.LogDate.ToUniversalTime();
            updateCatch.UserName = catchMade.UserName;

            await _fokkersDbService.UpdateItemAsync(updateCatch);
            return catchMade;
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Catch>> Delete(Guid id)
        {
            ApplicationUser user = _userHelper.GetUser();
            var catchMade = await _fokkersDbService.GetItemAsync(id.ToString());

            if (catchMade == null)
            {
                return NotFound();
            }
            else
            {
                if (catchMade.UserName == user.Email)
                {
                    await _fokkersDbService.DeleteItemAsync(id.ToString());
                    return NoContent();
                }
                else
                {
                    return Forbid();
                }

            }
        }


    } // end c
} // end ns
