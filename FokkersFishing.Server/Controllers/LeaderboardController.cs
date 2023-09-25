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
using System.Runtime.CompilerServices;
using FokkersFishing.Client.Pages;
using System.IO.Pipelines;

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

        [AllowAnonymous]
        [HttpGet("stats/{competitionId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CompetitionStats>> GetStats(Guid competitionId)
        {
            CompetitionData competitionData = await _fokkersDbService.GetCompetitionAsync(competitionId.ToString());
            if (competitionData != null)
            {
                Competition competition = competitionData.GetCompetition();
                IEnumerable<CatchData> catchesMadeData = await _fokkersDbService.GetCompetitionLeaderboardItemsAsync(competitionId);
                if (catchesMadeData == null)
                {
                    return NotFound();
                }
                var caught = (from c in catchesMadeData
                              where c.Status != CatchStatusEnum.Rejected
                              select c.Length);
                if (caught.Any())
                {
                    double totalCm = caught.ToList<double>().Sum();
                    int totalCaught = catchesMadeData.Count();
                    return new CompetitionStats
                    {
                        FishCaught = totalCaught,
                        TotalLength = totalCm
                    };

                }
            }
            return NotFound();
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet("{competitionId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Catch>>> GetAllCompetition(Guid competitionId)
        {
            IEnumerable<CatchData> catchesMadeData = await _fokkersDbService.GetAdminCompetitionLeaderboardItemsAsync(competitionId);
            if (catchesMadeData == null)
            {
                return NotFound();
            }
            List<Catch> catchesMade = new List<Catch>();
            List<TeamMemberData> teamMembers = await _fokkersDbService.GetTeamMembersAsync();
            List<TeamData> teams = await _fokkersDbService.GetTeamsAsync();
            foreach (CatchData catchMadeData in catchesMadeData)
            {
                Catch catchMade = catchMadeData.GetCatch();
                catchMade.UserName = _userHelper.GetUser(catchMade.UserEmail).UserName;

                // Get team
                Guid teamId = teamMembers.Where(tm => tm.UserEmail.ToLower() == catchMade.UserEmail.ToLower()).FirstOrDefault().TeamId;
                if (teamId != null)
                {
                    string teamName = teams.Where(team => team.RowKey == teamId.ToString()).FirstOrDefault().Name;
                    catchMade.TeamName = teamName;
                }

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
                if (competition.CompetitionEnded)
                {
                    // Get biggest fish per type
                    List<CatchData> catches = await _fokkersDbService.GetCatchesInCompetitionsAsync(competitionId);

                    BigThree big3 = new BigThree();

                    // Pike
                    var pike = (from c in catches
                                where c.Fish.ToLower() == "snoek"
                                orderby c.Length descending
                                select c).Take(1);

                    if (pike.Count() > 0)
                    {
                        big3.Pike = pike.FirstOrDefault().GetCatch();
                        big3.Pike.UserName = _userHelper.GetUser(big3.Pike.UserEmail).UserName;
                        if (big3.Pike.RegisterUserEmail != null)
                        {
                            big3.Pike.RegisterUserName = _userHelper.GetUser(big3.Pike.RegisterUserEmail).UserName;
                        }
                    }
                    else
                    {
                        big3.Pike = new Catch();
                    }

                    // Bass
                    var bass = (from c in catches
                                where c.Fish.ToLower() == "baars"
                                orderby c.Length descending
                                select c).Take(1);

                    if (bass.Count() > 0)
                    {
                        big3.Bass = bass.FirstOrDefault().GetCatch();
                        big3.Bass.UserName = _userHelper.GetUser(big3.Bass.UserEmail).UserName;
                        if (big3.Bass.RegisterUserEmail != null)
                        {
                            big3.Bass.RegisterUserName = _userHelper.GetUser(big3.Bass.RegisterUserEmail).UserName;
                        }
                    }
                    else
                    {
                        big3.Bass = new Catch();
                    }

                    // Zander
                    var zander = (from c in catches
                                  where c.Fish.ToLower() == "snoekbaars"
                                  orderby c.Length descending
                                  select c).Take(1);
                    if (zander.Count() > 0)
                    {
                        big3.Zander = zander.FirstOrDefault().GetCatch();
                        big3.Zander.UserName = _userHelper.GetUser(big3.Zander.UserEmail).UserName;
                        if (big3.Zander.RegisterUserEmail != null)
                        {
                            big3.Zander.RegisterUserName = _userHelper.GetUser(big3.Zander.RegisterUserEmail).UserName;
                        }
                    }
                    else
                    {
                        big3.Zander = new Catch();
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
        [HttpGet("user/bigthree/{competitionId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BigThree>> GetBigThreeCompetitionForAnIndividual(Guid competitionId)
        {
            CompetitionData competitionData = await _fokkersDbService.GetCompetitionAsync(competitionId.ToString());
            if (competitionData != null)
            {
                Competition competition = competitionData.GetCompetition();
                if (competition.Active)
                {
                    // Get user
                    ApplicationUser user = _userHelper.GetUser();

                    // Get biggest fish per type
                    List<CatchData> catches = await _fokkersDbService.GetCatchesInCompetitionsAsync(competitionId);

                    BigThree big3 = new BigThree();

                    // Pike
                    var pike = (from c in catches
                                where c.Fish.ToLower() == "snoek"
                                where c.UserEmail == user.Email
                                orderby c.Length descending
                                select c).Take(1);

                    if (pike.Count() > 0)
                    {
                        big3.Pike = pike.FirstOrDefault().GetCatch();
                        big3.Pike.UserName = _userHelper.GetUser(big3.Pike.UserEmail).UserName;
                        if (big3.Pike.RegisterUserEmail != null)
                        {
                            big3.Pike.RegisterUserName = _userHelper.GetUser(big3.Pike.RegisterUserEmail).UserName;
                        }
                    }
                    else
                    {
                        big3.Pike = new Catch();
                    }

                    // Bass
                    var bass = (from c in catches
                                where c.Fish.ToLower() == "baars"
                                where c.UserEmail == user.Email
                                orderby c.Length descending
                                select c).Take(1);

                    if (bass.Count() > 0)
                    {
                        big3.Bass = bass.FirstOrDefault().GetCatch();
                        big3.Bass.UserName = _userHelper.GetUser(big3.Bass.UserEmail).UserName;
                        if (big3.Bass.RegisterUserEmail != null)
                        {
                            big3.Bass.RegisterUserName = _userHelper.GetUser(big3.Bass.RegisterUserEmail).UserName;
                        }
                    }
                    else
                    {
                        big3.Bass = new Catch();
                    }

                    // Zander
                    var zander = (from c in catches
                                  where c.Fish.ToLower() == "snoekbaars"
                                  where c.UserEmail == user.Email
                                  orderby c.Length descending
                                  select c).Take(1);
                    if (zander.Count() > 0)
                    {
                        big3.Zander = zander.FirstOrDefault().GetCatch();
                        big3.Zander.UserName = _userHelper.GetUser(big3.Zander.UserEmail).UserName;
                        if (big3.Zander.RegisterUserEmail != null)
                        {
                            big3.Zander.RegisterUserName = _userHelper.GetUser(big3.Zander.RegisterUserEmail).UserName;
                        }
                    }
                    else
                    {
                        big3.Zander = new Catch();
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


        // bigthreeBiggest = await client.GetFromJsonAsync<BigThreeWinner>("leaderboard/biggest/bigthree/" + competitionState.CompetitionId);
        [Authorize(Roles = "Administrator, User")]
        [HttpGet("biggest/bigthree/{competitionId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BigThreeWinner>> GetBiggestBigThreeCompetition(Guid competitionId)
        {
            CompetitionData competitionData = await _fokkersDbService.GetCompetitionAsync(competitionId.ToString());
            if (competitionData != null)
            {
                Competition competition = competitionData.GetCompetition();
                if (competition.Active & competition.CompetitionEnded)
                {
                    IEnumerable<CatchData> biggestCatches = await _fokkersDbService.GetCompetitionLeaderboardItemsAsync(competitionId);
                    List<BigThree> big3list = new List<BigThree>();
                    if (biggestCatches != null)
                    {
                        foreach (string user in biggestCatches.Select(u => u.UserEmail).Distinct())
                        {
                            BigThree big3 = new BigThree
                            {
                                Name = _userHelper.GetUser(user).UserName
                            };

                            var pike = (from c in biggestCatches
                                        where c.UserEmail == user
                                        where c.Fish == "Snoek"
                                        orderby c.Length descending
                                        select c).FirstOrDefault();
                            if (pike != null)
                            {
                                big3.Pike = pike.GetCatch();
                            }

                            var bass = (from c in biggestCatches
                                        where c.UserEmail == user
                                        where c.Fish == "Baars"
                                        orderby c.Length descending
                                        select c).FirstOrDefault();
                            if (bass != null)
                            {
                                big3.Bass = bass.GetCatch();
                            }

                            var zander = (from c in biggestCatches
                                          where c.UserEmail == user
                                          where c.Fish == "Snoekbaars"
                                          orderby c.Length descending
                                          select c).FirstOrDefault();
                            if (zander != null)
                            {
                                big3.Zander = zander.GetCatch();
                            }

                            big3list.Add(big3);
                        }
                        var winner = big3list.OrderByDescending(o => o.TotalLength).FirstOrDefault();
                        BigThreeWinner big3winner = new BigThreeWinner
                        {
                            Name = winner.Name,
                            Fish = "Big 3",
                            TotalLength = winner.TotalLength
                        };
                        return big3winner;
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
            else
            {
                return NotFound();
            }
        }


        // bigthree = await client.GetFromJsonAsync<List<BigThree>>("leaderboard/team/bigthree/" + competitionState.CompetitionId);
        [Authorize(Roles = "Administrator, User")]
        [HttpGet("team/bigthree/{competitionId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<BigThree>>> GetBigThreeCompetitionForTeam(Guid competitionId)
        {
            CompetitionData competitionData = await _fokkersDbService.GetCompetitionAsync(competitionId.ToString());
            if (competitionData != null)
            {
                Competition competition = competitionData.GetCompetition();
                if (competition.Active)
                {
                    // Get user
                    ApplicationUser user = _userHelper.GetUser();

                    // Get Team members
                    List<TeamMemberData> teamMemberData = await _fokkersDbService.GetTeamMembersFromMemberAsync(user.Email);
                    if (teamMemberData != null)
                    {
                        List<CatchData> catches = await _fokkersDbService.GetCompetitionLeaderboardItemsAsync(competitionId);
                        if (catches.Count > 0)
                        {
                            Dictionary<int, BigThree> bigThree = new Dictionary<int, BigThree>();
                            // Check if user part of requested team
                            var checkTeam = from u in teamMemberData
                                            where u.UserEmail == user.Email
                                            select u;
                            if (checkTeam.Count() > 0)
                            {

                                List<Catch> pikeList = new List<Catch>();
                                List<Catch> bassList = new List<Catch>();
                                List<Catch> zanderList = new List<Catch>();

                                foreach (TeamMemberData teamMember in teamMemberData)
                                {
                                    var pikeCatches = from c in catches
                                                      where (c.UserEmail == teamMember.UserEmail)
                                                      where (c.Fish.ToLower() == "snoek")
                                                      orderby c.Length descending
                                                      select c;

                                    if (pikeCatches != null)
                                    {
                                        List<Catch> catchesMade = new List<Catch>();
                                        foreach (CatchData catchMadeData in pikeCatches)
                                        {
                                            Catch catchMade = catchMadeData.GetCatch();
                                            catchMade.UserName = _userHelper.GetUser(catchMade.UserEmail).UserName;
                                            if (catchMade.RegisterUserEmail != null)
                                            {
                                                catchMade.RegisterUserName = _userHelper.GetUser(catchMade.RegisterUserEmail).UserName;
                                            }
                                            catchesMade.Add(catchMade);
                                        }
                                        pikeList.AddRange(catchesMade);
                                    }

                                    var bassCatches = from c in catches
                                                      where (c.UserEmail == teamMember.UserEmail)
                                                      where (c.Fish.ToLower() == "baars")
                                                      orderby c.Length descending
                                                      select c;

                                    if (bassCatches != null)
                                    {
                                        List<Catch> catchesMade = new List<Catch>();
                                        foreach (CatchData catchMadeData in bassCatches)
                                        {
                                            Catch catchMade = catchMadeData.GetCatch();
                                            catchMade.UserName = _userHelper.GetUser(catchMade.UserEmail).UserName;
                                            if (catchMade.RegisterUserEmail != null)
                                            {
                                                catchMade.RegisterUserName = _userHelper.GetUser(catchMade.RegisterUserEmail).UserName;
                                            }
                                            catchesMade.Add(catchMade);
                                        }
                                        bassList.AddRange(catchesMade);
                                    }

                                    var zanderCatches = from c in catches
                                                        where (c.UserEmail == teamMember.UserEmail)
                                                        where (c.Fish.ToLower() == "snoekbaars")
                                                        orderby c.Length descending
                                                        select c;

                                    if (zanderCatches != null)
                                    {
                                        List<Catch> catchesMade = new List<Catch>();
                                        foreach (CatchData catchMadeData in zanderCatches)
                                        {
                                            Catch catchMade = catchMadeData.GetCatch();
                                            catchMade.UserName = _userHelper.GetUser(catchMade.UserEmail).UserName;
                                            if (catchMade.RegisterUserEmail != null)
                                            {
                                                catchMade.RegisterUserName = _userHelper.GetUser(catchMade.RegisterUserEmail).UserName;
                                            }
                                            catchesMade.Add(catchMade);
                                        }
                                        zanderList.AddRange(catchesMade);
                                    }
                                } // end teammember
                                bigThree.Add(0, new BigThree { Name = "First", Pike = new Catch(), Bass = new Catch(), Zander = new Catch() });
                                bigThree.Add(1, new BigThree { Name = "Second", Pike = new Catch(), Bass = new Catch(), Zander = new Catch() });
                                bigThree.Add(2, new BigThree { Name = "Third", Pike = new Catch(), Bass = new Catch(), Zander = new Catch() });

                                var pikeOrder = (from c in pikeList
                                                 orderby c.Length descending
                                                 select c).Take(3);
                                int count = 0;
                                foreach (var pike in pikeOrder)
                                {
                                    bigThree[count].Pike = pike;
                                    count++;
                                }

                                var bassOrder = (from c in bassList
                                                 orderby c.Length descending
                                                 select c).Take(3);
                                count = 0;
                                foreach (var bass in bassOrder)
                                {
                                    bigThree[count].Bass = bass;
                                    count++;
                                }

                                var zanderOrder = (from c in zanderList
                                                   orderby c.Length descending
                                                   select c).Take(3);
                                count = 0;
                                foreach (var zander in zanderOrder)
                                {
                                    bigThree[count].Zander = zander;
                                    count++;
                                }

                                return bigThree.Values.ToList();
                            }
                            else
                            {
                                return NotFound();
                            }
                        } // end catches
                        else
                        {
                            return NotFound();
                        }
                    } // end teammember
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

        // scores = await clientOpen.GetFromJsonAsync<List<Ranking>>("leaderboard/team/bigthree/all/" + competitionState.CompetitionId);
        [Authorize(Roles = "Administrator, User")]
        [HttpGet("team/bigthree/all/{competitionId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<Ranking>>> GetBigThreeCompetitionForAllTeams(Guid competitionId)
        {
            // TODO: Speed this up. Get all catches top for user: _fokkersDbService.GetBiggestFishPerUserInCompetition
            List<Ranking> rankList = new List<Ranking>();
            CompetitionData competitionData = await _fokkersDbService.GetCompetitionAsync(competitionId.ToString());
            if (competitionData != null)
            {
                Competition competition = competitionData.GetCompetition();
                if (competition.Active & competition.CompetitionEnded)
                {
                    // Get all teams
                    var teamsData = await _fokkersDbService.GetTeamsAsync();
                    List<TeamMemberData> teamMemberData = await _fokkersDbService.GetTeamMembersAsync();
                    IEnumerable<CatchData> catches = await _fokkersDbService.GetCompetitionLeaderboardItemsAsync(competitionId);
                    foreach (TeamData teamData in teamsData)
                    {
                        // Get Team members

                        if (teamMemberData != null)
                        {
                            Dictionary<int, BigThree> bigThree = new Dictionary<int, BigThree>();
                            List<Catch> pikeList = new List<Catch>();
                            List<Catch> bassList = new List<Catch>();
                            List<Catch> zanderList = new List<Catch>();

                            foreach (TeamMemberData teamMember in teamMemberData.Where(t => t.TeamId == new Guid(teamData.RowKey)))
                            {
                                IEnumerable<CatchData> pikeCatches = from c in catches
                                                                     where c.UserEmail == teamMember.UserEmail
                                                                     where c.Fish.ToLower() == "snoek"
                                                                     orderby c.Length descending
                                                                     select c;
                                if (pikeCatches != null)
                                {
                                    List<Catch> catchesMade = new List<Catch>();
                                    foreach (CatchData catchMadeData in pikeCatches)
                                    {
                                        Catch catchMade = catchMadeData.GetCatch();
                                        catchMade.UserName = _userHelper.GetUser(catchMade.UserEmail).UserName;
                                        if (catchMade.RegisterUserEmail != null)
                                        {
                                            catchMade.RegisterUserName = _userHelper.GetUser(catchMade.RegisterUserEmail).UserName;
                                        }
                                        catchesMade.Add(catchMade);
                                    }
                                    pikeList.AddRange(catchesMade);
                                }

                                IEnumerable<CatchData> bassCatches = from c in catches
                                                                     where c.UserEmail == teamMember.UserEmail
                                                                     where c.Fish.ToLower() == "baars"
                                                                     orderby c.Length descending
                                                                     select c;
                                if (bassCatches != null)
                                {
                                    List<Catch> catchesMade = new List<Catch>();
                                    foreach (CatchData catchMadeData in bassCatches)
                                    {
                                        Catch catchMade = catchMadeData.GetCatch();
                                        catchMade.UserName = _userHelper.GetUser(catchMade.UserEmail).UserName;
                                        if (catchMade.RegisterUserEmail != null)
                                        {
                                            catchMade.RegisterUserName = _userHelper.GetUser(catchMade.RegisterUserEmail).UserName;
                                        }
                                        catchesMade.Add(catchMade);
                                    }
                                    bassList.AddRange(catchesMade);
                                }

                                IEnumerable<CatchData> zanderCatches = from c in catches
                                                                       where c.UserEmail == teamMember.UserEmail
                                                                       where c.Fish.ToLower() == "snoekbaars"
                                                                       orderby c.Length descending
                                                                       select c;
                                if (zanderCatches != null)
                                {
                                    List<Catch> catchesMade = new List<Catch>();
                                    foreach (CatchData catchMadeData in zanderCatches)
                                    {
                                        Catch catchMade = catchMadeData.GetCatch();
                                        catchMade.UserName = _userHelper.GetUser(catchMade.UserEmail).UserName;
                                        if (catchMade.RegisterUserEmail != null)
                                        {
                                            catchMade.RegisterUserName = _userHelper.GetUser(catchMade.RegisterUserEmail).UserName;
                                        }
                                        catchesMade.Add(catchMade);
                                    }
                                    zanderList.AddRange(catchesMade);
                                }
                            } // end teammember for

                            bigThree.Add(0, new BigThree { Name = "First", Pike = new Catch(), Bass = new Catch(), Zander = new Catch() });
                            bigThree.Add(1, new BigThree { Name = "Second", Pike = new Catch(), Bass = new Catch(), Zander = new Catch() });
                            bigThree.Add(2, new BigThree { Name = "Third", Pike = new Catch(), Bass = new Catch(), Zander = new Catch() });

                            // Start positive
                            bool big3 = true;

                            var pikeOrder = (from c in pikeList
                                             orderby c.Length descending
                                             select c).Take(3);
                            int count = 0;
                            if (pikeOrder.Count() < Constants.NUMBER_OF_BIG3)
                            {
                                big3 = false;
                            }
                            foreach (var pike in pikeOrder)
                            {
                                bigThree[count].Pike = pike;
                                if (pike.Length < Constants.REQUIRED_FISH_LENGTH)
                                {
                                    big3 = false;
                                }
                                count++;
                            }

                            var bassOrder = (from c in bassList
                                             orderby c.Length descending
                                             select c).Take(3);
                            count = 0;
                            if (bassOrder.Count() < Constants.NUMBER_OF_BIG3)
                            {
                                big3 = false;
                            }
                            foreach (var bass in bassOrder)
                            {
                                bigThree[count].Bass = bass;
                                if (bass.Length < Constants.REQUIRED_FISH_LENGTH)
                                {
                                    big3 = false;
                                }
                                count++;
                            }

                            var zanderOrder = (from c in zanderList
                                               orderby c.Length descending
                                               select c).Take(3);
                            count = 0;
                            if (zanderOrder.Count() < Constants.NUMBER_OF_BIG3)
                            {
                                big3 = false;
                            }
                            foreach (var zander in zanderOrder)
                            {
                                bigThree[count].Zander = zander;
                                if (zander.Length < Constants.REQUIRED_FISH_LENGTH)
                                {
                                    big3 = false;
                                }
                                count++;
                            }


                            // Get ranking and score
                            Ranking ranking = new Ranking();
                            ranking.TeamName = teamData.Name;
                            ranking.Score = bigThree[0].TotalLength;
                            ranking.Score += bigThree[1].TotalLength;
                            ranking.Score += bigThree[2].TotalLength;
                            ranking.Big3 = big3;
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
            // Get user
            ApplicationUser user = _userHelper.GetUser();

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
                    //foreach (CatchData catchMadeData in catchesMadeData.Where(f => f.Fish.ToLower() == "snoek" | f.Fish.ToLower() == "baars" | f.Fish.ToLower() == "snoekbaars"))
                    foreach (CatchData catchMadeData in catchesMadeData)
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


        [Authorize(Roles = "Administrator, User")]
        [HttpGet("team/scores/{competitionId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<TeamScore>>> GetAllScoresForTeam(Guid competitionId)
        {
            CompetitionData competitionData = await _fokkersDbService.GetCompetitionAsync(competitionId.ToString());
            if (competitionData != null)
            {
                Competition competition = competitionData.GetCompetition();
                if (competition.Active)
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
                            List<FishData> fish = await _fokkersDbService.GetFishAsync();
                            var competitionFish = from f in fish
                                                  where f.IncludeInCompetition == true
                                                  select f.Name;

                            foreach (CatchData catchMadeData in catchesMadeData.Where(x => competitionFish.Contains(x.Fish)))
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

                            if (catchList.Count() > 0 & catchTotal.Count() > 0)
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
                if (competition.TimeTillEnd.TotalMinutes > 120 | competition.ShowLeaderboardAfterCompetitionEnds)
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


        // var userCatches = await clientOpen.GetFromJsonAsync<List<Catch>>("leaderboard/open/" + competitionState.CompetitionId);
        [AllowAnonymous]
        [HttpGet("open/{competitionId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Catch>>> GetAllCompetitionOpen(Guid competitionId)
        {
            CompetitionData competitionData = await _fokkersDbService.GetCompetitionAsync(competitionId.ToString());
            if (competitionData != null)
            {
                Competition competition = competitionData.GetCompetition();
                if (competition.Active)
                {
                    List<FishData> fish = await _fokkersDbService.GetFishAsync();
                    var predatorFish = from f in fish
                                       where f.Predator == true
                                       select f.Name;
                    IEnumerable<CatchData> catchesMadeData = await _fokkersDbService.GetCompetitionLeaderboardItemsAsync(competitionId);
                    if (catchesMadeData == null)
                    {
                        return NotFound();
                    }
                    List<Catch> catchesMade = new List<Catch>();
                    foreach (CatchData catchMadeData in catchesMadeData.Where(x => predatorFish.Contains(x.Fish)))
                    //foreach (CatchData catchMadeData in catchesMadeData.Where(x => x.Fish.ToLower() == "snoek" | x.Fish.ToLower() == "baars" | x.Fish.ToLower() == "snoekbaars"))
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
                if (competition.TimeTillEnd.TotalMinutes > 120 | competition.ShowLeaderboardAfterCompetitionEnds)
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

            IEnumerable<CatchData> pikes = await _fokkersDbService.GetTopCatchAsync("snoek");
            IEnumerable<CatchData> bass = await _fokkersDbService.GetTopCatchAsync("baars");
            IEnumerable<CatchData> zander = await _fokkersDbService.GetTopCatchAsync("snoekbaars");

            // Fill initial dictionary
            foreach (var fisher in fishermen)
            {
                BigThree newBig = new BigThree { Name = _userHelper.GetUser(fisher.UserEmail).UserName };

                // Pikes
                newBig.Pike = new Catch();
                var biggestPike = pikes.Where(c => c.UserEmail.ToLower() == fisher.UserEmail.ToLower()).ToList();
                if (biggestPike.Count > 0)
                {
                    newBig.Pike = biggestPike.Take(1).FirstOrDefault().GetCatch();
                }

                // Bass
                newBig.Bass = new Catch();
                var biggestBass = bass.Where(c => c.UserEmail.ToLower() == fisher.UserEmail.ToLower()).ToList();
                if (biggestBass.Count > 0)
                {
                    newBig.Bass = biggestBass.Take(1).FirstOrDefault().GetCatch();
                }

                // Zander
                newBig.Zander = new Catch();
                var biggestZander = zander.Where(c => c.UserEmail.ToLower() == fisher.UserEmail.ToLower()).ToList();
                if (biggestZander.Count > 0)
                {
                    newBig.Zander = biggestZander.Take(1).FirstOrDefault().GetCatch();
                }

                bigThree.Add(fisher.UserEmail, newBig);
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
