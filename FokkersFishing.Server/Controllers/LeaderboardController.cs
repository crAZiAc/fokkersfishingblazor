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
    [Authorize(Roles = "Administrator, User, ApiUser")]
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
            _httpContextAccessor = httpContextAccessor;
            _userHelper = new UserHelper(_httpContextAccessor, _dbContext);

        }
        [HttpGet]
        [AllowAnonymous]
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
                if (catchMade.RegisterUserEmail != null)
                {
                    catchMade.RegisterUserName = _userHelper.GetUser(catchMade.RegisterUserEmail).UserName;
                }
                catchesMade.Add(catchMade);
            }
            return catchesMade.ToList();
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet("{competitionId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Catch>>> GetAllCompetition(Guid competitionId)
        {

            IEnumerable<CatchData> catchesMadeData = await _fokkersDbService.GetCompetitionLeaderboardItemsAsync(competitionId);
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



        [Authorize(Roles = "Administrator, User")]
        [HttpGet("member/bigthree/{competitionId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BigThree>> GetBigThreeCompetitionForIndividuals(Guid competitionId)
        {
            CompetitionData competitionData = await _fokkersDbService.GetCompetitionAsync(competitionId.ToString());
            if (competitionData != null)
            {
                Competition competition = competitionData.GetCompetition();
                if (competition.TimeTillEnd.TotalMinutes > 120)
                {
                    // Get biggest fish per type
                    List<CatchData> catches = await _fokkersDbService.GetCatchesInCompetitionsAsync(competitionId);

                    BigThree big3 = new BigThree();

                    // Pike
                    var pike = (from c in catches
                                where c.Fish.ToLower() == "snoek"
                                orderby c.Length descending
                                select c).Take(1);

                    big3.Pike = pike.FirstOrDefault().GetCatch();
                    big3.Pike.UserName = _userHelper.GetUser(big3.Pike.UserEmail).UserName;
                    if (big3.Pike.RegisterUserEmail != null)
                    {
                        big3.Pike.RegisterUserName = _userHelper.GetUser(big3.Pike.RegisterUserEmail).UserName;
                    }

                    // Bass
                    var bass = (from c in catches
                                where c.Fish.ToLower() == "baars"
                                orderby c.Length descending
                                select c).Take(1);

                    big3.Bass = bass.FirstOrDefault().GetCatch();
                    big3.Bass.UserName = _userHelper.GetUser(big3.Bass.UserEmail).UserName;
                    if (big3.Bass.RegisterUserEmail != null)
                    {
                        big3.Bass.RegisterUserName = _userHelper.GetUser(big3.Bass.RegisterUserEmail).UserName;
                    }

                    // Zander
                    var zander = (from c in catches
                                  where c.Fish.ToLower() == "snoekbaars"
                                  orderby c.Length descending
                                  select c).Take(1);

                    big3.Zander = zander.FirstOrDefault().GetCatch();
                    big3.Zander.UserName = _userHelper.GetUser(big3.Zander.UserEmail).UserName;
                    if (big3.Zander.RegisterUserEmail != null)
                    {
                        big3.Zander.RegisterUserName = _userHelper.GetUser(big3.Zander.RegisterUserEmail).UserName;
                    }

                    return big3;
                }
                else
                {
                    return Ok();
                }
            }
            else
            {
                return NotFound();
            }
        }


        [Authorize(Roles = "Administrator, User")]
        [HttpGet("biggest/bigthree/{competitionId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BigThreeWinner>> GetBiggestBigThreeCompetition(Guid competitionId)
        {
            CompetitionData competitionData = await _fokkersDbService.GetCompetitionAsync(competitionId.ToString());
            if (competitionData != null)
            {
                Competition competition = competitionData.GetCompetition();
                if (competition.TimeTillEnd.TotalMinutes > 120)
                {
                    // Get biggest fish per type
                    List<CatchData> catches = await _fokkersDbService.GetCatchesInCompetitionsAsync(competitionId);

                    var highestCatches = (from c in catches
                                          orderby c.Length descending
                                          group c by new { c.UserEmail, c.Fish } into fishGroup
                                          select new BigThreeWinner
                                          {
                                              TotalLength = fishGroup.Max(x => x.Length),
                                              Fish = fishGroup.Key.Fish,
                                              Name = fishGroup.Key.UserEmail
                                          });

                    var winner = (from w in highestCatches
                                  orderby w.TotalLength descending
                                  group w by w.Name into winnerGroup
                                  select new BigThreeWinner
                                  {
                                      TotalLength = winnerGroup.Sum(x => x.TotalLength),
                                      Name = _userHelper.GetUser(winnerGroup.Key).UserName
                                  });
                    return winner.FirstOrDefault();

                }
                else
                {
                    return Ok();
                }
            }
            else
            {
                return NotFound();
            }
        }


        [AllowAnonymous]
        [HttpGet("team/bigthree/{competitionId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<BigThree>>> GetBigThreeCompetitionForTeam(Guid competitionId)
        {
            CompetitionData competitionData = await _fokkersDbService.GetCompetitionAsync(competitionId.ToString());
            if (competitionData != null)
            {
                Competition competition = competitionData.GetCompetition();
                if (competition.TimeTillEnd.TotalMinutes > 120)
                {
                    // Get user
                    ApplicationUser user = _userHelper.GetUser();

                    // Get Team members
                    List<TeamMemberData> teamMemberData = await _fokkersDbService.GetTeamMembersFromMemberAsync(user.Email);
                    if (teamMemberData != null)
                    {
                        Dictionary<int, BigThree> bigThree = new Dictionary<int, BigThree>();
                        // Check if user part of requested team
                        var checkTeam = from u in teamMemberData
                                        where u.UserEmail == user.Email
                                        select u;
                        if (checkTeam.Count() > 0)
                        {
                            List<Catch> catchesMade = new List<Catch>();

                            bigThree.Add(0, new BigThree { Name = "First", Pike = new Catch(), Bass = new Catch(), Zander = new Catch() });
                            bigThree.Add(1, new BigThree { Name = "Second", Pike = new Catch(), Bass = new Catch(), Zander = new Catch() });
                            bigThree.Add(2, new BigThree { Name = "Third", Pike = new Catch(), Bass = new Catch(), Zander = new Catch() });
                            foreach (TeamMemberData teamMember in teamMemberData)
                            {
                                IEnumerable<Catch> pikeCatches = await _fokkersDbService.GetTopCatchesByUserInCompetitionAsync(competitionId, teamMember.UserEmail, "snoek");
                                if (pikeCatches != null)
                                {
                                    int count = 0;
                                    foreach (Catch catchMadeData in pikeCatches)
                                    {
                                        Catch catchMade = catchMadeData;
                                        catchMade.UserName = _userHelper.GetUser(catchMade.UserEmail).UserName;
                                        if (catchMade.RegisterUserEmail != null)
                                        {
                                            catchMade.RegisterUserName = _userHelper.GetUser(catchMade.RegisterUserEmail).UserName;
                                        }
                                        if (bigThree[count].Pike.Length < catchMade.Length)
                                        {
                                            bigThree[count].Pike = catchMade;
                                        }
                                        count++;
                                    }
                                }

                                IEnumerable<Catch> bassCatches = await _fokkersDbService.GetTopCatchesByUserInCompetitionAsync(competitionId, teamMember.UserEmail, "baars");
                                if (bassCatches != null)
                                {
                                    int count = 0;
                                    foreach (Catch catchMadeData in bassCatches)
                                    {
                                        Catch catchMade = catchMadeData;
                                        catchMade.UserName = _userHelper.GetUser(catchMade.UserEmail).UserName;
                                        if (catchMade.RegisterUserEmail != null)
                                        {
                                            catchMade.RegisterUserName = _userHelper.GetUser(catchMade.RegisterUserEmail).UserName;
                                        }
                                        if (bigThree[count].Bass.Length < catchMade.Length)
                                        {
                                            bigThree[count].Bass = catchMade;
                                        }
                                        count++;
                                    }
                                }

                                IEnumerable<Catch> zanderCatches = await _fokkersDbService.GetTopCatchesByUserInCompetitionAsync(competitionId, teamMember.UserEmail, "snoekbaars");
                                if (zanderCatches != null)
                                {
                                    int count = 0;
                                    foreach (Catch catchMadeData in zanderCatches)
                                    {
                                        Catch catchMade = catchMadeData;
                                        catchMade.UserName = _userHelper.GetUser(catchMade.UserEmail).UserName;
                                        if (catchMade.RegisterUserEmail != null)
                                        {
                                            catchMade.RegisterUserName = _userHelper.GetUser(catchMade.RegisterUserEmail).UserName;
                                        }
                                        if (bigThree[count].Zander.Length < catchMade.Length)
                                        {
                                            bigThree[count].Zander = catchMade;
                                        }
                                        count++;
                                    }
                                }
                            }
                            return bigThree.Values.ToList();
                        }
                        else
                        {
                            return NotFound();
                        }
                    }
                    else
                    {
                        return Forbid();
                    }
                }
                else
                {
                    return Ok();
                }
            }
            else
            {
                return NotFound();
            }
        }


        [AllowAnonymous]
        [HttpGet("team/bigthree/all/{competitionId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<Ranking>>> GetBigThreeCompetitionForAllTeams(Guid competitionId)
        {
            List<Ranking> rankList = new List<Ranking>();
            CompetitionData competitionData = await _fokkersDbService.GetCompetitionAsync(competitionId.ToString());
            if (competitionData != null)
            {
                Competition competition = competitionData.GetCompetition();
                if (competition.TimeTillEnd.TotalMinutes > 120)
                {
                    // Get all teams
                    var teamsData = await _fokkersDbService.GetTeamsAsync();
                    foreach (TeamData teamData in teamsData)
                    {
                        // Get Team members
                        List<TeamMemberData> teamMemberData = await _fokkersDbService.GetTeamMembersAsync(new Guid(teamData.RowKey));
                        if (teamMemberData != null)
                        {
                            Dictionary<int, BigThree> bigThree = new Dictionary<int, BigThree>();
                            List<Catch> catchesMade = new List<Catch>();
                            bigThree.Add(0, new BigThree { Name = "First", Pike = new Catch(), Bass = new Catch(), Zander = new Catch() });
                            bigThree.Add(1, new BigThree { Name = "Second", Pike = new Catch(), Bass = new Catch(), Zander = new Catch() });
                            bigThree.Add(2, new BigThree { Name = "Third", Pike = new Catch(), Bass = new Catch(), Zander = new Catch() });
                            foreach (TeamMemberData teamMember in teamMemberData)
                            {
                                IEnumerable<Catch> pikeCatches = await _fokkersDbService.GetTopCatchesByUserInCompetitionAsync(competitionId, teamMember.UserEmail, "snoek");
                                if (pikeCatches != null)
                                {
                                    int count = 0;
                                    foreach (Catch catchMadeData in pikeCatches)
                                    {
                                        Catch catchMade = catchMadeData;
                                        catchMade.UserName = _userHelper.GetUser(catchMade.UserEmail).UserName;
                                        if (catchMade.RegisterUserEmail != null)
                                        {
                                            catchMade.RegisterUserName = _userHelper.GetUser(catchMade.RegisterUserEmail).UserName;
                                        }
                                        if (bigThree[count].Pike.Length < catchMade.Length)
                                        {
                                            bigThree[count].Pike = catchMade;
                                        }
                                        count++;
                                    }
                                }

                                IEnumerable<Catch> bassCatches = await _fokkersDbService.GetTopCatchesByUserInCompetitionAsync(competitionId, teamMember.UserEmail, "baars");
                                if (bassCatches != null)
                                {
                                    int count = 0;
                                    foreach (Catch catchMadeData in bassCatches)
                                    {
                                        Catch catchMade = catchMadeData;
                                        catchMade.UserName = _userHelper.GetUser(catchMade.UserEmail).UserName;
                                        if (catchMade.RegisterUserEmail != null)
                                        {
                                            catchMade.RegisterUserName = _userHelper.GetUser(catchMade.RegisterUserEmail).UserName;
                                        }
                                        if (bigThree[count].Bass.Length < catchMade.Length)
                                        {
                                            bigThree[count].Bass = catchMade;
                                        }
                                        count++;
                                    }
                                }

                                IEnumerable<Catch> zanderCatches = await _fokkersDbService.GetTopCatchesByUserInCompetitionAsync(competitionId, teamMember.UserEmail, "snoekbaars");
                                if (zanderCatches != null)
                                {
                                    int count = 0;
                                    foreach (Catch catchMadeData in zanderCatches)
                                    {
                                        Catch catchMade = catchMadeData;
                                        catchMade.UserName = _userHelper.GetUser(catchMade.UserEmail).UserName;
                                        if (catchMade.RegisterUserEmail != null)
                                        {
                                            catchMade.RegisterUserName = _userHelper.GetUser(catchMade.RegisterUserEmail).UserName;
                                        }
                                        if (bigThree[count].Zander.Length < catchMade.Length)
                                        {
                                            bigThree[count].Zander = catchMade;
                                        }
                                        count++;
                                    }
                                }
                            } // end teammember for
                            // Get ranking and score
                            Ranking ranking = new Ranking();
                            ranking.TeamName = teamData.Name;
                            ranking.Score = bigThree[0].TotalLength;
                            ranking.Score += bigThree[1].TotalLength;
                            ranking.Score += bigThree[2].TotalLength;
                            rankList.Add(ranking);
                        }
                        else
                        {
                            return NotFound();
                        } // end if teamdata
                    } // end teams

                    // Rank rankings
                    int rankCount = 1;
                    var rank = from r in rankList
                               orderby r.Score descending
                               select r;
                    foreach (Ranking ranking in rank)
                    {
                        ranking.Rank = rankCount;
                        rankCount++;
                    }

                    return rankList;
                } // end totalminutes
                else
                {
                    return Ok();
                }
            } // end competitiondata
            else
            {
                return NotFound();
            }
        }

        [Authorize(Roles = "Administrator, User")]
        [HttpGet("team/{competitionId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<Catch>>> GetAllCompetitionForTeam(Guid competitionId)
        {
            CompetitionData competitionData = await _fokkersDbService.GetCompetitionAsync(competitionId.ToString());
            if (competitionData != null)
            {
                Competition competition = competitionData.GetCompetition();
                if (competition.TimeTillEnd.TotalMinutes > 120)
                {
                    // Get user
                    ApplicationUser user = _userHelper.GetUser();

                    // Check if

                    // Get Team members
                    List<TeamMemberData> teamMemberData = await _fokkersDbService.GetTeamMembersFromMemberAsync(user.Email);
                    if (teamMemberData != null)
                    {
                        // Check if user part of requested team
                        var checkTeam = from u in teamMemberData
                                        where u.UserEmail == user.Email
                                        select u;
                        if (checkTeam.Count() > 0)
                        {
                            IEnumerable<CatchData> catchesMadeData = await _fokkersDbService.GetCompetitionLeaderboardItemsAsync(competitionId);
                            if (catchesMadeData == null)
                            {
                                return NotFound();
                            }
                            List<Catch> catchesMade = new List<Catch>();
                            foreach (CatchData catchMadeData in catchesMadeData.Where(f => f.Fish.ToLower() == "snoek" | f.Fish.ToLower() == "baars" | f.Fish.ToLower() == "snoekbaars"))
                            {
                                var checkCatch = from ut in teamMemberData
                                                 where ut.UserEmail == catchMadeData.UserEmail
                                                 select ut;
                                if (checkCatch.Count() > 0)
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
                        else
                        {
                            return Forbid();
                        }
                    }
                    else
                    {
                        return NotFound();
                    }
                }
                else
                {
                    return Ok();
                }
            }
            else
            {
                return NotFound();
            }
        }

        [Authorize(Roles = "Administrator, User")]
        [HttpGet("team/scores/{competitionId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<TeamScore>>> GetAllScoresForTeam(Guid competitionId)
        {
            CompetitionData competitionData = await _fokkersDbService.GetCompetitionAsync(competitionId.ToString());
            if (competitionData != null)
            {
                Competition competition = competitionData.GetCompetition();
                if (competition.TimeTillEnd.TotalMinutes > 120)
                {
                    List<TeamScore> scores = new List<TeamScore>();

                    // Get user
                    ApplicationUser user = _userHelper.GetUser();

                    // Check if

                    // Get Team members
                    List<TeamMemberData> teamMemberData = await _fokkersDbService.GetTeamMembersFromMemberAsync(user.Email);
                    if (teamMemberData != null)
                    {
                        // Check if user part of requested team
                        var checkTeam = from u in teamMemberData
                                        where u.UserEmail == user.Email
                                        select u;
                        if (checkTeam.Count() > 0)
                        {

                            IEnumerable<CatchData> catchesMadeData = await _fokkersDbService.GetCompetitionLeaderboardItemsAsync(competitionId);
                            if (catchesMadeData == null)
                            {
                                return NotFound();
                            }
                            List<Catch> catchesMade = new List<Catch>();
                            foreach (CatchData catchMadeData in catchesMadeData.Where(f => f.Fish.ToLower() == "snoek" | f.Fish.ToLower() == "baars" | f.Fish.ToLower() == "snoekbaars"))
                            {
                                var checkCatch = from ut in teamMemberData
                                                 where ut.UserEmail == catchMadeData.UserEmail
                                                 select ut;
                                if (checkCatch.Count() > 0)
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

                            // CatchList
                            var catchList = from f in catchesMade
                                            group f by f.Fish into fishGroup
                                            select new TeamScore
                                            {
                                                Fish = fishGroup.Key,
                                                FishCount = fishGroup.Count(),
                                                TotalLength = fishGroup.Sum(x => x.Length)
                                            };

                            var catchTotal = from f in catchesMade
                                             group f by f.CompetitionId into compGroup
                                             select new TeamScore
                                             {
                                                 Fish = "_Total",
                                                 FishCount = compGroup.Count(),
                                                 TotalLength = compGroup.Sum(x => x.Length)
                                             };

                            if (catchList != null & catchTotal != null)
                            {
                                scores = catchList.ToList();
                                scores.Add(catchTotal.FirstOrDefault());
                            }

                            return scores;

                        }
                        else
                        {
                            return Forbid();
                        }
                    }
                    else
                    {
                        return NotFound();
                    }
                }
                else
                {
                    return Ok();
                }
            }
            else
            {
                return NotFound();
            }
        }

        [AllowAnonymous]
        [HttpGet("open/scores/{competitionId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<TeamScore>>> GetAllCompetitionOpenByTeam(Guid competitionId)
        {
            List<TeamScore> scores = new List<TeamScore>();
            CompetitionData competitionData = await _fokkersDbService.GetCompetitionAsync(competitionId.ToString());
            if (competitionData != null)
            {
                Competition competition = competitionData.GetCompetition();
                if (competition.TimeTillEnd.TotalMinutes > 120)
                {
                    // Get all teams
                    var teamsData = await _fokkersDbService.GetTeamsAsync();
                    foreach (TeamData teamData in teamsData)
                    {
                        TeamScore teamScore = new TeamScore
                        {
                            TeamName = teamData.Name
                        };

                        // Get members from team
                        List<TeamMemberData> teamMemberData = await _fokkersDbService.GetTeamMembersAsync(new Guid(teamData.RowKey));
                        foreach (TeamMemberData teamMember in teamMemberData)
                        {
                            // Get catches for each member in competition
                            List<CatchData> catchesMember = await _fokkersDbService.GetCatchesByUserInCompetitionAsync(competitionId, teamMember.UserEmail);
                            var total = (from c in catchesMember
                                         where c.Fish.ToLower() == "snoek" | c.Fish.ToLower() == "snoekbaars" | c.Fish.ToLower() == "baars"
                                         select c.Length).Sum();
                            var totalCount = (from c in catchesMember select c.Length).Count();
                            teamScore.TotalLength += total;
                            teamScore.FishCount += totalCount;
                        }
                        scores.Add(teamScore);

                        int ranking = 1;
                        var rank = from r in scores
                                   orderby r.TotalLength descending
                                   select r;
                        foreach (TeamScore score in rank)
                        {
                            score.Ranking = ranking;
                            ranking++;
                        }
                    }
                    return scores;
                }
                else
                {
                    return Ok();
                }
            }
            else
            {
                return NotFound();
            }
        }


        [AllowAnonymous]
        [HttpGet("open/{competitionId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Catch>>> GetAllCompetitionOpen(Guid competitionId)
        {
            CompetitionData competitionData = await _fokkersDbService.GetCompetitionAsync(competitionId.ToString());
            if (competitionData != null)
            {
                Competition competition = competitionData.GetCompetition();
                if (competition.TimeTillEnd.TotalMinutes > 120)
                {
                    List<FishData> fish = await _fokkersDbService.GetFishAsync();
                    var predatorFish = from f in fish
                                       where f.Predator == true
                                       select f.Name;
                    IEnumerable < CatchData > catchesMadeData = await _fokkersDbService.GetCompetitionLeaderboardItemsAsync(competitionId);
                    if (catchesMadeData == null)
                    {
                        return NotFound();
                    }
                    List<Catch> catchesMade = new List<Catch>();
                    foreach (CatchData catchMadeData in catchesMadeData.Where(x => predatorFish.Contains(x.Fish)))
                    {
                        try
                        {
                            Catch catchMade = catchMadeData.GetCatch();
                            catchMade.UserName = _userHelper.GetUser(catchMade.UserEmail).UserName;
                            if (catchMade.RegisterUserEmail != null)
                            {
                                catchMade.RegisterUserName = _userHelper.GetUser(catchMade.RegisterUserEmail).UserName;
                            }
                            catchesMade.Add(catchMade);
                        }
                        catch (Exception ex)
                        {
                        }

                    }
                    return catchesMade.ToList();
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                return NotFound();
            }
        }

        [AllowAnonymous]
        [HttpGet("fishermen")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<FisherMan>>> GetFishermen()
        {
            IEnumerable<FisherMan> fishermen = await _fokkersDbService.GetFishermenAsync();
            if (fishermen == null)
            {
                return NotFound();
            }
            foreach (var fisher in fishermen)
            {
                fisher.UserName = _userHelper.GetUser(fisher.UserEmail).UserName;
            }
            return fishermen.OrderByDescending(f => f.TotalLength).ToList();
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet("fishermen/{competitionId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<FisherMan>>> GetCompetitionFishermen(Guid competitionId)
        {
            IEnumerable<FisherMan> fishermen = await _fokkersDbService.GetFishermenCompetitionAsync(competitionId);
            if (fishermen == null)
            {
                return NotFound();
            }
            foreach (var fisher in fishermen)
            {
                fisher.UserName = _userHelper.GetUser(fisher.UserEmail).UserName;
            }
            return fishermen.OrderByDescending(f => f.TotalLength).ToList();
        }

        [AllowAnonymous]
        [HttpGet("fishermen/open/{competitionId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<FisherMan>>> GetCompetitionFishermenOpen(Guid competitionId)
        {
            CompetitionData competitionData = await _fokkersDbService.GetCompetitionAsync(competitionId.ToString());
            if (competitionData != null)
            {
                Competition competition = competitionData.GetCompetition();
                if (competition.TimeTillEnd.TotalMinutes > 120)
                {
                    IEnumerable<FisherMan> fishermen = await _fokkersDbService.GetFishermenCompetitionAsync(competitionId);
                    if (fishermen == null)
                    {
                        return NotFound();
                    }
                    foreach (var fisher in fishermen)
                    {
                        fisher.UserName = _userHelper.GetUser(fisher.UserEmail).UserName;
                    }
                    return fishermen.OrderByDescending(f => f.TotalLength).ToList();
                }
                else
                {
                    return Ok();
                }
            }
            else
            {
                return NotFound();
            }
        }

        [AllowAnonymous]
        [HttpGet("bigthree")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<BigThree>>> GetBigThree()
        {
            Dictionary<string, BigThree> bigThree = new Dictionary<string, BigThree>();
            IEnumerable<FisherMan> fishermen = await _fokkersDbService.GetFishermenAsync();

            // Fill initial dictionary
            foreach (var fisher in fishermen)
            {
                bigThree.Add(fisher.UserEmail, new BigThree { Name = _userHelper.GetUser(fisher.UserEmail).UserName });
            }

            // Pikes
            IEnumerable<Catch> pikes = await _fokkersDbService.GetTopCatchAsync("snoek");
            if (pikes != null)
            {
                foreach (var pike in pikes)
                {
                    bigThree[pike.UserEmail].Pike = pike;
                }
            }

            // Bass
            IEnumerable<Catch> bass = await _fokkersDbService.GetTopCatchAsync("baars");
            if (bass != null)
            {
                foreach (var bassFish in bass)
                {
                    bigThree[bassFish.UserEmail].Bass = bassFish;
                }
            }

            // Zander
            IEnumerable<Catch> zander = await _fokkersDbService.GetTopCatchAsync("snoekbaars");
            if (zander != null)
            {
                foreach (var zanderFish in zander)
                {
                    bigThree[zanderFish.UserEmail].Zander = zanderFish;
                }
            }
            return bigThree.Values.ToList();
        }

        private List<Team> GetTeamData()
        {
            IEnumerable<TeamData> teamsData;
            teamsData = _fokkersDbService.GetTeamsAsync().Result;
            if (teamsData == null)
            {
                return null;
            }
            List<Team> teams = new List<Team>();
            foreach (TeamData teamData in teamsData)
            {
                Team team = teamData.GetTeam();
                var teamMembersData = _fokkersDbService.GetTeamMembersAsync(team.Id);
                foreach (var teamMember in teamMembersData.Result)
                {
                    User user = _userHelper.GetUser(teamMember.UserEmail);
                    team.Users.Add(user);
                }
                teams.Add(team);
            }

            return teams.ToList();
        }



        private Team GetTeamByUser(string userEmail)
        {
            var teamMembersData = _fokkersDbService.GetTeamMembersFromMemberAsync(userEmail);
            if (teamMembersData != null)
            {
                var checkUser = from tm in teamMembersData.Result
                                where tm.UserEmail == userEmail
                                select tm;
                if (checkUser.Count() > 0)
                {
                    var teamData = _fokkersDbService.GetTeamAsync(checkUser.FirstOrDefault().TeamId.ToString());
                    if (teamData != null)
                    {
                        Team team = teamData.Result.GetTeam();
                        foreach (var teamMember in teamMembersData.Result)
                        {
                            User userTeam = _userHelper.GetUser(teamMember.UserEmail);
                            team.Users.Add(userTeam);
                        }
                        return team;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

    } // end c
} // end ns
