﻿using System;
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
    [Authorize(Roles = "Administrator, User, ApiUser")]
    [ApiController]
    [Route("[controller]")]
    public class CatchController : Controller
    {
        private readonly ILogger<CatchController> _logger;
        private readonly IFokkersDbService _fokkersDbService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _dbContext;
        private UserHelper _userHelper;

        public CatchController(ILogger<CatchController> logger, IFokkersDbService fokkersDbService, IHttpContextAccessor httpContextAccessor, ApplicationDbContext dbContext)
        {
            _logger = logger;
            _fokkersDbService = fokkersDbService;
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
            _userHelper = new UserHelper(_httpContextAccessor, _dbContext);
        }

        #region Catch Calls
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
                Catch catchMade = catchMadeData.GetCatch();
                catchMade.UserName = _userHelper.GetUser(catchMade.UserEmail).UserName;
                if (catchMade.RegisterUserEmail != null)
                {
                    catchMade.RegisterUserName = _userHelper.GetUser(catchMade.RegisterUserEmail).UserName;
                }
                catchesMade.Add(catchMade);
            }
            return catchesMade.ToList();
        }

        [HttpGet("competition/{competitionId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Catch>>> GetCompetitionCatches(Guid competitionId)
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
                if (catchMadeData.CompetitionId == competitionId)
                {
                    Catch catchMade = catchMadeData.GetCatch();
                    catchMade.UserName = _userHelper.GetUser(catchMade.UserEmail).UserName;
                    if (catchMade.RegisterUserEmail != null)
                    {
                        catchMade.RegisterUserName = _userHelper.GetUser(catchMade.RegisterUserEmail).UserName;
                    }
                    catchesMade.Add(catchMade);
                }
            }
            return catchesMade.ToList();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Catch>> GetByIdUser(Guid id)
        {
            ApplicationUser user = _userHelper.GetUser();
            CatchData catchMade = await _fokkersDbService.GetUserItemAsync(id.ToString(), user.Email);
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
        public async Task<ActionResult<Catch>> CreateUserCatch(Catch catchMade)
        {
            ApplicationUser user = _userHelper.GetUser();
            catchMade.LogDate = DateTime.Now;
            catchMade.EditDate = DateTime.Now;
            catchMade.CatchNumber = _fokkersDbService.GetCatchNumberCount(user.Email).Result + 1;
            catchMade.GlobalCatchNumber = _fokkersDbService.GetGlobalCatchNumberCount().Result + 1;
            catchMade.RegisterUserEmail = user.Email;

            CatchData newCatch = new CatchData();
            newCatch.CatchDate = catchMade.CatchDate.ToUniversalTime();
            newCatch.CatchNumber = catchMade.CatchNumber;
            newCatch.EditDate = catchMade.EditDate.ToUniversalTime();
            newCatch.Fish = catchMade.Fish;
            newCatch.GlobalCatchNumber = catchMade.GlobalCatchNumber;
            newCatch.Length = catchMade.Length;
            newCatch.LogDate = catchMade.LogDate.ToUniversalTime();
            newCatch.RowKey = catchMade.Id.ToString();
            if (catchMade.UserEmail != null)
            {
                newCatch.UserEmail = catchMade.UserEmail;
            }
            else
            {
                newCatch.UserEmail = catchMade.RegisterUserEmail;
                catchMade.UserEmail = catchMade.RegisterUserEmail;
            }
            newCatch.RegisterUserEmail = catchMade.RegisterUserEmail;
            newCatch.Status = CatchStatusEnum.Pending;
            newCatch.CompetitionId = catchMade.CompetitionId;

            // Photos
            newCatch.CatchPhotoUrl = catchMade.CatchPhotoUrl;
            newCatch.CatchThumbnailUrl = catchMade.CatchThumbnailUrl;
            newCatch.MeasurePhotoUrl = catchMade.MeasurePhotoUrl;
            newCatch.MeasureThumbnailUrl = catchMade.MeasureThumbnailUrl;

            await _fokkersDbService.AddItemAsync(newCatch);
            return CreatedAtAction(nameof(GetByIdUser), new { id = catchMade.Id }, catchMade);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Catch>> PutUser(Guid id, Catch catchMade)
        {
            ApplicationUser user = _userHelper.GetUser();
            List<TeamMemberData> teamMemberData = await _fokkersDbService.GetTeamMembersFromMemberAsync(user.Email);
            catchMade.EditDate = DateTime.Now;

            var checkTeamMate = from u in teamMemberData
                                where (u.UserEmail == user.Email | u.UserEmail == catchMade.RegisterUserEmail | u.UserEmail == catchMade.UserEmail)
                                select u;

            if (checkTeamMate.Count() > 0)
            {
                CatchData updateCatch = await _fokkersDbService.GetItemAsync(id.ToString());
                if (updateCatch != null)
                {
                    if (updateCatch.Status != CatchStatusEnum.Rejected)
                    {
                        updateCatch.CatchDate = catchMade.CatchDate.ToUniversalTime();
                        updateCatch.CatchNumber = catchMade.CatchNumber;
                        updateCatch.EditDate = catchMade.EditDate.ToUniversalTime();
                        updateCatch.Fish = catchMade.Fish;
                        updateCatch.GlobalCatchNumber = catchMade.GlobalCatchNumber;
                        updateCatch.Length = catchMade.Length;
                        updateCatch.LogDate = catchMade.LogDate.ToUniversalTime();
                        updateCatch.UserEmail = catchMade.UserEmail;
                        updateCatch.RegisterUserEmail = catchMade.RegisterUserEmail;
                        updateCatch.CatchPhotoUrl = catchMade.CatchPhotoUrl;
                        updateCatch.CatchThumbnailUrl = catchMade.CatchThumbnailUrl;
                        updateCatch.MeasurePhotoUrl = catchMade.MeasurePhotoUrl;
                        updateCatch.MeasureThumbnailUrl = catchMade.MeasureThumbnailUrl;
                        updateCatch.Status = CatchStatusEnum.Pending;
                        updateCatch.CompetitionId = catchMade.CompetitionId;

                        if (catchMade.UserEmail != null)
                        {
                            if (updateCatch.UserEmail != catchMade.UserEmail)
                            {
                                // Also check catchnumber for user, because we are switching users
                                updateCatch.CatchNumber = _fokkersDbService.GetCatchNumberCount(catchMade.UserEmail).Result + 1;
                                updateCatch.UserEmail = catchMade.UserEmail;
                            }
                        }
                        else
                        {
                            updateCatch.UserEmail = catchMade.RegisterUserEmail;
                            catchMade.UserEmail = catchMade.RegisterUserEmail;
                        }

                        await _fokkersDbService.UpdateItemAsync(updateCatch);
                        return catchMade;
                    }
                    else
                    {
                        return Forbid();
                    }
                }
                else
                {
                    return Forbid();
                }
            }
            else
            {
                return Forbid();
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Catch>> DeleteUserCatch(Guid id)
        {
            ApplicationUser user = _userHelper.GetUser();
            var catchMade = await _fokkersDbService.GetUserItemAsync(id.ToString(), user.Email);

            if (catchMade == null)
            {
                return NotFound();
            }
            else
            {
                if (catchMade.UserEmail == user.Email)
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


        [HttpDelete("team/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Catch>> DeleteTeamCatch(Guid id)
        {
            ApplicationUser user = _userHelper.GetUser();
            var catchMade = await _fokkersDbService.GetTeamItemAsync(id.ToString(), user.Email);

            if (catchMade == null)
            {
                return NotFound();
            }
            else
            {
                await _fokkersDbService.DeleteItemAsync(id.ToString());

                return NoContent();
            }
        }
        #endregion
        #region Admin calls
        [Authorize(Roles = "Administrator")]
        [HttpGet("admin")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Catch>>> GetAdmin()
        {
            IEnumerable<CatchData> catchesMadeData = null;
            catchesMadeData = await _fokkersDbService.GetAllCatches();
            if (catchesMadeData == null)
            {
                return NotFound();
            }
            List<Catch> catchesMade = new List<Catch>();
            foreach (CatchData catchMadeData in catchesMadeData)
            {
                Catch catchMade = catchMadeData.GetCatch();
                catchMade.UserName = _userHelper.GetUser(catchMade.UserEmail).UserName;
                if (catchMade.RegisterUserEmail != null)
                {
                    catchMade.RegisterUserName = _userHelper.GetUser(catchMade.RegisterUserEmail).UserName;
                }
                catchesMade.Add(catchMade);
            }
            return catchesMade.ToList();
        }

        [Authorize(Roles = "ApiUser", AuthenticationSchemes = AuthenticationSchemaNames.BasicAuthentication)]
        [HttpGet("admin/data")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Catch>>> GetAdminData()
        {
            IEnumerable<CatchData> catchesMadeData = null;
            catchesMadeData = await _fokkersDbService.GetAllCatches();
            if (catchesMadeData == null)
            {
                return NotFound();
            }
            List<Catch> catchesMade = new List<Catch>();
            foreach (CatchData catchMadeData in catchesMadeData)
            {
                Catch catchMade = catchMadeData.GetCatch();
                catchMade.UserName = _userHelper.GetUser(catchMade.UserEmail).UserName;
                if (catchMade.RegisterUserEmail != null)
                {
                    catchMade.RegisterUserName = _userHelper.GetUser(catchMade.RegisterUserEmail).UserName;
                }
                catchesMade.Add(catchMade);
            }
            return catchesMade.ToList();
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet("admin/competition/{competitionId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Catch>>> GetAdminCompetition(Guid competitionId)
        {
            IEnumerable<CatchData> catchesMadeData = null;
            catchesMadeData = await _fokkersDbService.GetAllCatches();
            if (catchesMadeData == null)
            {
                return NotFound();
            }
            List<Catch> catchesMade = new List<Catch>();
            foreach (CatchData catchMadeData in catchesMadeData)
            {
                if (catchMadeData.CompetitionId == competitionId)
                {
                    Catch catchMade = catchMadeData.GetCatch();
                    catchMade.UserName = _userHelper.GetUser(catchMade.UserEmail).UserName;
                    if (catchMade.RegisterUserEmail != null)
                    {
                        catchMade.RegisterUserName = _userHelper.GetUser(catchMade.RegisterUserEmail).UserName;
                    }
                    catchesMade.Add(catchMade);
                }
            }
            return catchesMade.ToList();
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet("admin/pending")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Catch>>> GetPendingAdmin()
        {
            IEnumerable<CatchData> catchesMadeData = null;
            catchesMadeData = await _fokkersDbService.GetPendingCatches();
            if (catchesMadeData == null)
            {
                return NotFound();
            }
            List<Catch> catchesMade = new List<Catch>();
            foreach (CatchData catchMadeData in catchesMadeData)
            {
                Catch catchMade = catchMadeData.GetCatch();
                catchMade.UserName = _userHelper.GetUser(catchMade.UserEmail).UserName;
                if (catchMade.RegisterUserEmail != null)
                {
                    catchMade.RegisterUserName = _userHelper.GetUser(catchMade.RegisterUserEmail).UserName;
                }
                catchesMade.Add(catchMade);
            }
            return catchesMade.ToList();
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet("admin/pending/competition/{competitionId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Catch>>> GetPendingAdminCompetition(Guid competitionId)
        {
            IEnumerable<CatchData> catchesMadeData = null;
            catchesMadeData = await _fokkersDbService.GetPendingCatches();
            if (catchesMadeData == null)
            {
                return NotFound();
            }
            List<Catch> catchesMade = new List<Catch>();
            foreach (CatchData catchMadeData in catchesMadeData)
            {
                if (catchMadeData.CompetitionId == competitionId)
                {
                    Catch catchMade = catchMadeData.GetCatch();
                    catchMade.UserName = _userHelper.GetUser(catchMade.UserEmail).UserName;
                    if (catchMade.RegisterUserEmail != null)
                    {
                        catchMade.RegisterUserName = _userHelper.GetUser(catchMade.RegisterUserEmail).UserName;
                    }
                    catchesMade.Add(catchMade);
                }
            }
            return catchesMade.ToList();
        }

        [Authorize(Roles = "Administrator")]
        [HttpPut("admin/{id}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Catch>> PutAdmin(Guid id, Catch catchMade)
        {
            catchMade.EditDate = DateTime.Now;

            CatchData updateCatch = await _fokkersDbService.GetItemAsync(id.ToString());
            updateCatch.CatchDate = catchMade.CatchDate.ToUniversalTime();
            updateCatch.EditDate = catchMade.EditDate.ToUniversalTime();
            updateCatch.Fish = catchMade.Fish;
            updateCatch.Length = catchMade.Length;
            updateCatch.Status = catchMade.Status;
            updateCatch.CompetitionId = catchMade.CompetitionId;

            if (catchMade.UserEmail != null)
            {
                if (updateCatch.UserEmail != catchMade.UserEmail)
                {
                    // Also check catchnumber for user, because we are switching users
                    updateCatch.CatchNumber = _fokkersDbService.GetCatchNumberCount(catchMade.UserEmail).Result + 1;
                    updateCatch.UserEmail = catchMade.UserEmail;
                }
            }
            else
            {
                updateCatch.UserEmail = catchMade.RegisterUserEmail;
                catchMade.UserEmail = catchMade.RegisterUserEmail;
            }

            await _fokkersDbService.UpdateItemAsync(updateCatch);
            return catchMade;
        }

        [Authorize(Roles = "Administrator")]
        [HttpDelete("admin/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Catch>> DeleteAdminCatch(Guid id)
        {
            ApplicationUser user = _userHelper.GetUser();
            var catchMade = await _fokkersDbService.GetItemAsync(id.ToString());

            if (catchMade == null)
            {
                return NotFound();
            }
            else
            {
                await _fokkersDbService.DeleteItemAsync(id.ToString());
                return NoContent();
            }
        }
        #endregion

    } // end c
} // end ns
