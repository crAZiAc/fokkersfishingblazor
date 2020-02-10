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
        public IEnumerable<Catch> Get()
        {
            // Check incoming ID and get username
            ApplicationUser user = _userHelper.GetUser();
            Task<IEnumerable<Catch>> catchesMade = null;
            catchesMade = _fokkersDbService.GetItemsAsync("select * from c where c.userName = '" + user.Email + "'");
            return catchesMade.Result.ToList();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Catch> GetById(string id)
        {
            var catchMade = _fokkersDbService.GetItemAsync(id);

            if (catchMade.Result == null)
            {
                return NotFound();
            }
            return catchMade.Result;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<Catch> Create(Catch catchMade)
        {
            ApplicationUser user = _userHelper.GetUser();
            catchMade.Id = Guid.NewGuid();
            catchMade.CatchDate = DateTime.Now;
            catchMade.LogDate = DateTime.Now;
            catchMade.CatchNumber = _fokkersDbService.GetCatchNumberCount().Result + 1;
            catchMade.GlobalCatchNumber = _fokkersDbService.GetGlobalCatchNumberCount().Result + 1;
            catchMade.UserName = user.Email;

            _fokkersDbService.AddItemAsync(catchMade);

            return CreatedAtAction(nameof(GetById), new { id = catchMade.Id }, catchMade);
        }

        //TODO: Implement Update using PUT
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<Catch> Put(Guid id, Catch catchMade)
        {
            catchMade.EditDate = DateTime.Now;
            _fokkersDbService.UpdateItemAsync(id.ToString(), catchMade);
            return NoContent();
        }


    } // end c
} // end ns
