using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FokkersFishing.Api.Data;
using FokkersFishing.Api.Helpers;
using FokkersFishing.Api.Models.Dtos;
using FokkersFishing.Api.Models.Entities;
using FokkersFishing.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FokkersFishing.Api.Controllers
{
    [Authorize(Roles = "Administrator, User, ApiUser")]
    [ApiController]
    [Route("[controller]")]
    public class CatchController : ControllerBase
    {
        private readonly IFokkersDbService _fokkersDbService;
        private readonly UserHelper _userHelper;

        public CatchController(IFokkersDbService fokkersDbService, UserHelper userHelper)
        {
            _fokkersDbService = fokkersDbService;
            _userHelper = userHelper;
        }

        private List<Catch> Enrich(IEnumerable<CatchData> data)
        {
            var list = new List<Catch>();
            foreach (var catchMadeData in data)
            {
                var catchMade = catchMadeData.GetCatch();
                catchMade.UserName = _userHelper.GetUser(catchMade.UserEmail).UserName;
                if (catchMade.RegisterUserEmail != null)
                {
                    catchMade.RegisterUserName = _userHelper.GetUser(catchMade.RegisterUserEmail).UserName;
                }
                list.Add(catchMade);
            }
            return list;
        }

        #region Catch Calls
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Catch>>> Get()
        {
            var user = _userHelper.GetUser();
            var catchesMadeData = await _fokkersDbService.GetUserItemsAsync(user.Email);
            if (catchesMadeData == null) return NotFound();
            return Enrich(catchesMadeData);
        }

        [HttpGet("competition/{competitionId}")]
        public async Task<ActionResult<IEnumerable<Catch>>> GetCompetitionCatches(Guid competitionId)
        {
            var user = _userHelper.GetUser();
            var catchesMadeData = await _fokkersDbService.GetUserItemsAsync(user.Email);
            if (catchesMadeData == null) return NotFound();
            return Enrich(catchesMadeData.Where(c => c.CompetitionId == competitionId));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Catch>> GetByIdUser(Guid id)
        {
            var user = _userHelper.GetUser();
            var catchMade = await _fokkersDbService.GetUserItemAsync(id.ToString(), user.Email);
            if (catchMade == null) return NotFound();
            if (catchMade.UserEmail == user.Email) return catchMade.GetCatch();
            return Forbid();
        }

        [HttpPost]
        public async Task<ActionResult<Catch>> CreateUserCatch(Catch catchMade)
        {
            var user = _userHelper.GetUser();
            catchMade.LogDate = DateTime.Now;
            catchMade.EditDate = DateTime.Now;
            catchMade.CatchNumber = await _fokkersDbService.GetCatchNumberCount(user.Email) + 1;
            catchMade.GlobalCatchNumber = await _fokkersDbService.GetGlobalCatchNumberCount() + 1;
            catchMade.RegisterUserEmail = user.Email;

            var newCatch = new CatchData
            {
                CatchDate = catchMade.CatchDate.ToUniversalTime(),
                CatchNumber = catchMade.CatchNumber,
                EditDate = catchMade.EditDate.ToUniversalTime(),
                Fish = catchMade.Fish,
                GlobalCatchNumber = catchMade.GlobalCatchNumber,
                Length = catchMade.Length,
                LogDate = catchMade.LogDate.ToUniversalTime(),
                RowKey = catchMade.Id.ToString(),
                RegisterUserEmail = catchMade.RegisterUserEmail,
                Status = CatchStatusEnum.Pending,
                CompetitionId = catchMade.CompetitionId,
                CatchPhotoUrl = catchMade.CatchPhotoUrl,
                CatchThumbnailUrl = catchMade.CatchThumbnailUrl,
                MeasurePhotoUrl = catchMade.MeasurePhotoUrl,
                MeasureThumbnailUrl = catchMade.MeasureThumbnailUrl
            };

            if (catchMade.UserEmail != null)
            {
                newCatch.UserEmail = catchMade.UserEmail;
            }
            else
            {
                newCatch.UserEmail = catchMade.RegisterUserEmail;
                catchMade.UserEmail = catchMade.RegisterUserEmail;
            }

            await _fokkersDbService.AddItemAsync(newCatch);
            return CreatedAtAction(nameof(GetByIdUser), new { id = catchMade.Id }, catchMade);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Catch>> PutUser(Guid id, Catch catchMade)
        {
            var user = _userHelper.GetUser();
            var teamMemberData = await _fokkersDbService.GetTeamMembersFromMemberAsync(user.Email) ?? new List<TeamMemberData>();
            catchMade.EditDate = DateTime.Now;

            var isTeamMate = teamMemberData.Any(u =>
                u.UserEmail == user.Email || u.UserEmail == catchMade.RegisterUserEmail || u.UserEmail == catchMade.UserEmail);

            if (!isTeamMate) return Forbid();

            var updateCatch = await _fokkersDbService.GetItemAsync(id.ToString());
            if (updateCatch == null || updateCatch.Status == CatchStatusEnum.Rejected) return Forbid();

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
                    updateCatch.CatchNumber = await _fokkersDbService.GetCatchNumberCount(catchMade.UserEmail) + 1;
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

        [HttpDelete("{id}")]
        public async Task<ActionResult<Catch>> DeleteUserCatch(Guid id)
        {
            var user = _userHelper.GetUser();
            var catchMade = await _fokkersDbService.GetUserItemAsync(id.ToString(), user.Email);
            if (catchMade == null) return NotFound();
            if (catchMade.UserEmail != user.Email) return Forbid();
            await _fokkersDbService.DeleteItemAsync(id.ToString());
            return NoContent();
        }

        [HttpDelete("team/{id}")]
        public async Task<ActionResult<Catch>> DeleteTeamCatch(Guid id)
        {
            var user = _userHelper.GetUser();
            var catchMade = await _fokkersDbService.GetTeamItemAsync(id.ToString(), user.Email);
            if (catchMade == null) return NotFound();
            await _fokkersDbService.DeleteItemAsync(id.ToString());
            return NoContent();
        }
        #endregion

        #region Admin calls
        [Authorize(Roles = "Administrator")]
        [HttpGet("admin")]
        public async Task<ActionResult<IEnumerable<Catch>>> GetAdmin()
        {
            var data = await _fokkersDbService.GetAllCatches();
            if (data == null) return NotFound();
            return Enrich(data);
        }

        [Authorize(Roles = "ApiUser", AuthenticationSchemes = AuthenticationSchemaNames.BasicAuthentication)]
        [HttpGet("admin/data")]
        public async Task<ActionResult<IEnumerable<Catch>>> GetAdminData()
        {
            var data = await _fokkersDbService.GetAllCatches();
            if (data == null) return NotFound();
            return Enrich(data);
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet("admin/competition/{competitionId}")]
        public async Task<ActionResult<IEnumerable<Catch>>> GetAdminCompetition(Guid competitionId)
        {
            var data = await _fokkersDbService.GetAllCatches();
            if (data == null) return NotFound();
            return Enrich(data.Where(c => c.CompetitionId == competitionId));
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet("admin/pending")]
        public async Task<ActionResult<IEnumerable<Catch>>> GetPendingAdmin()
        {
            var data = await _fokkersDbService.GetPendingCatches();
            if (data == null) return NotFound();
            return Enrich(data);
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet("admin/pending/competition/{competitionId}")]
        public async Task<ActionResult<IEnumerable<Catch>>> GetPendingAdminCompetition(Guid competitionId)
        {
            var data = await _fokkersDbService.GetPendingCatches();
            if (data == null) return NotFound();
            return Enrich(data.Where(c => c.CompetitionId == competitionId));
        }

        [Authorize(Roles = "Administrator")]
        [HttpPut("admin/{id}")]
        public async Task<ActionResult<Catch>> PutAdmin(Guid id, Catch catchMade)
        {
            catchMade.EditDate = DateTime.Now;
            var updateCatch = await _fokkersDbService.GetItemAsync(id.ToString());
            if (updateCatch == null) return NotFound();

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
                    updateCatch.CatchNumber = await _fokkersDbService.GetCatchNumberCount(catchMade.UserEmail) + 1;
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
        public async Task<ActionResult<Catch>> DeleteAdminCatch(Guid id)
        {
            var catchMade = await _fokkersDbService.GetItemAsync(id.ToString());
            if (catchMade == null) return NotFound();
            await _fokkersDbService.DeleteItemAsync(id.ToString());
            return NoContent();
        }
        #endregion
    }
}
