using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FokkersFishing.Api.Models.Dtos;
using FokkersFishing.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FokkersFishing.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FishController : ControllerBase
    {
        private readonly IFokkersDbService _fokkersDbService;

        public FishController(IFokkersDbService fokkersDbService)
        {
            _fokkersDbService = fokkersDbService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Fish>>> Get()
        {
            var fishesData = await _fokkersDbService.GetFishAsync();
            if (fishesData == null) return NotFound();
            return fishesData.Select(f => f.GetFish()).ToList();
        }
    }
}
