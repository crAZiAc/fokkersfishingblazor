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
using Microsoft.AspNetCore.Identity;

namespace FokkersFishing.Controllers
{
    [Authorize(Roles = "Administrator")]
    [ApiController]
    [Route("[controller]")]
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IFokkersDbService _fokkersDbService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _dbContext;
        private UserHelper _userHelper;

        public AdminController(ILogger<AdminController> logger, IFokkersDbService fokkersDbService, IHttpContextAccessor httpContextAccessor, ApplicationDbContext dbContext)
        {
            _logger = logger;
            _fokkersDbService = fokkersDbService;
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
            _userHelper = new UserHelper(_httpContextAccessor, _dbContext);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<ApplicationUser>>> Get()
        {
            return _userHelper.GetUsers();
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
                if (catchMade.UserEmail == user.Email)
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
            catchMade.UserEmail = user.Email;

            CatchData newCatch = new CatchData();
            newCatch.CatchDate = catchMade.CatchDate.ToUniversalTime();
            newCatch.CatchNumber = catchMade.CatchNumber;
            newCatch.EditDate = catchMade.EditDate.ToUniversalTime();
            newCatch.Fish = catchMade.Fish;
            newCatch.GlobalCatchNumber = catchMade.GlobalCatchNumber;
            newCatch.Length = catchMade.Length;
            newCatch.LogDate = catchMade.LogDate.ToUniversalTime();
            newCatch.RowKey = catchMade.Id.ToString();
            newCatch.UserEmail = catchMade.UserEmail;

            await _fokkersDbService.AddItemAsync(newCatch);
            return CreatedAtAction(nameof(GetById), new { id = catchMade.Id }, catchMade);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Catch>> Put(Guid id, Catch catchMade)
        {
            ApplicationUser user = _userHelper.GetUser();
            catchMade.UserEmail = user.Email;
            catchMade.EditDate = DateTime.Now;

            CatchData updateCatch = await _fokkersDbService.GetItemAsync(id.ToString());
            updateCatch.CatchDate = catchMade.CatchDate.ToUniversalTime();
            updateCatch.CatchNumber = catchMade.CatchNumber;
            updateCatch.EditDate = catchMade.EditDate.ToUniversalTime();
            updateCatch.Fish = catchMade.Fish;
            updateCatch.GlobalCatchNumber = catchMade.GlobalCatchNumber;
            updateCatch.Length = catchMade.Length;
            updateCatch.LogDate = catchMade.LogDate.ToUniversalTime();
            updateCatch.UserEmail = catchMade.UserEmail;
            updateCatch.CatchPhotoUrl = catchMade.CatchPhotoUrl;
            updateCatch.CatchThumbnailUrl = catchMade.CatchThumbnailUrl;
            updateCatch.MeasurePhotoUrl = catchMade.MeasurePhotoUrl;
            updateCatch.MeasureThumbnailUrl = catchMade.MeasureThumbnailUrl;
            
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
                if (catchMade.UserEmail == user.Email)
                {
                    // TODO: Delete photos


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
