using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using FokkersFishing.Interfaces;
using Microsoft.AspNetCore.Http;
using FokkersFishing.Data;
using FokkersFishing.Server.Helpers;
using FokkersFishing.Shared.Models;
using FokkersFishing.Server.Interfaces;

namespace FokkersFishing.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class DataController : ControllerBase
    {
        private readonly ILogger<DataController> _logger;
        private readonly IFokkersDbService _fokkersDbService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _dbContext;
        private UserHelper _userHelper;
        private IUserService _userService;

        public DataController(ILogger<DataController> logger, IFokkersDbService fokkersDbService, IHttpContextAccessor httpContextAccessor, ApplicationDbContext dbContext, IUserService userService)
        {
            _logger = logger;
            _fokkersDbService = fokkersDbService;
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
            _userHelper = new UserHelper(_httpContextAccessor, _dbContext);
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(null);
        }
    } // end c
} // end ns