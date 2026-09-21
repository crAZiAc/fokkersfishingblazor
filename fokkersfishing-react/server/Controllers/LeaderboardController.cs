using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FokkersFishing.Api.Helpers;
using FokkersFishing.Api.Models.Dtos;
using FokkersFishing.Api.Models.Entities;
using FokkersFishing.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FokkersFishing.Api.Controllers
{
    [Authorize(Roles = "Administrator, User, ApiUser")]
    [ApiController]
    [Route("[controller]")]
    public class LeaderboardController : ControllerBase
    {
        private readonly IFokkersDbService _fokkersDbService;
        private readonly UserHelper _userHelper;

        public LeaderboardController(IFokkersDbService fokkersDbService, UserHelper userHelper)
        {
            _fokkersDbService = fokkersDbService;
            _userHelper = userHelper;
        }

        private Catch EnrichCatch(CatchData data)
        {
            var catchMade = data.GetCatch();
            catchMade.UserName = _userHelper.GetUser(catchMade.UserEmail).UserName;
            if (catchMade.RegisterUserEmail != null)
            {
                catchMade.RegisterUserName = _userHelper.GetUser(catchMade.RegisterUserEmail).UserName;
            }
            return catchMade;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Catch>>> GetAll()
        {
            var catchesMadeData = await _fokkersDbService.GetLeaderboardItemsAsync();
            if (catchesMadeData == null) return NotFound();
            return catchesMadeData.Select(EnrichCatch).ToList();
        }

        [AllowAnonymous]
        [HttpGet("stats/{competitionId}")]
        public async Task<ActionResult<CompetitionStats>> GetStats(Guid competitionId)
        {
            var competitionData = await _fokkersDbService.GetCompetitionAsync(competitionId.ToString());
            if (competitionData == null) return NotFound();

            var catchesMadeData = await _fokkersDbService.GetCompetitionLeaderboardItemsAsync(competitionId);
            if (catchesMadeData == null) return NotFound();

            var caught = catchesMadeData.Where(c => c.Status != CatchStatusEnum.Rejected).Select(c => c.Length).ToList();
            if (caught.Any())
            {
                return new CompetitionStats
                {
                    FishCaught = catchesMadeData.Count,
                    TotalLength = caught.Sum()
                };
            }
            return NotFound();
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet("{competitionId}")]
        public async Task<ActionResult<IEnumerable<Catch>>> GetAllCompetition(Guid competitionId)
        {
            var catchesMadeData = await _fokkersDbService.GetAdminCompetitionLeaderboardItemsAsync(competitionId);
            if (catchesMadeData == null) return NotFound();

            var teamMembers = await _fokkersDbService.GetTeamMembersAsync();
            var teams = await _fokkersDbService.GetTeamsAsync();
            var catchesMade = new List<Catch>();
            foreach (var catchMadeData in catchesMadeData)
            {
                var catchMade = EnrichCatch(catchMadeData);
                var member = teamMembers.FirstOrDefault(tm => tm.UserEmail.ToLower() == catchMade.UserEmail.ToLower());
                if (member != null)
                {
                    var team = teams.FirstOrDefault(t => t.RowKey == member.TeamId.ToString());
                    if (team != null) catchMade.TeamName = team.Name;
                }
                catchesMade.Add(catchMade);
            }
            return catchesMade;
        }

        [Authorize(Roles = "Administrator, User")]
        [HttpGet("member/bigthree/{competitionId}")]
        public async Task<ActionResult<BigThree>> GetBigThreeCompetitionForIndividuals(Guid competitionId)
        {
            var competitionData = await _fokkersDbService.GetCompetitionAsync(competitionId.ToString());
            if (competitionData == null) return NotFound();
            var competition = competitionData.GetCompetition();
            if (!competition.CompetitionEnded) return Ok();

            var catches = await _fokkersDbService.GetCatchesInCompetitionsAsync(competitionId);
            return BuildBigThree(catches, null);
        }

        [Authorize(Roles = "Administrator, User")]
        [HttpGet("user/bigthree/{competitionId}")]
        public async Task<ActionResult<BigThree>> GetBigThreeCompetitionForAnIndividual(Guid competitionId)
        {
            var competitionData = await _fokkersDbService.GetCompetitionAsync(competitionId.ToString());
            if (competitionData == null) return NotFound();
            var competition = competitionData.GetCompetition();
            if (!competition.Active) return Ok();

            var user = _userHelper.GetUser();
            var catches = await _fokkersDbService.GetCatchesInCompetitionsAsync(competitionId);
            return BuildBigThree(catches, user.Email);
        }

        private BigThree BuildBigThree(List<CatchData> catches, string userEmail)
        {
            var big3 = new BigThree();

            Catch Top(string fishName)
            {
                var q = catches.Where(c => c.Fish.ToLower() == fishName);
                if (userEmail != null) q = q.Where(c => c.UserEmail == userEmail);
                var top = q.OrderByDescending(c => c.Length).FirstOrDefault();
                if (top == null) return new Catch();
                var enriched = top.GetCatch();
                enriched.UserName = _userHelper.GetUser(enriched.UserEmail).UserName;
                if (enriched.RegisterUserEmail != null)
                    enriched.RegisterUserName = _userHelper.GetUser(enriched.RegisterUserEmail).UserName;
                return enriched;
            }

            big3.Pike = Top("snoek");
            big3.Bass = Top("baars");
            big3.Zander = Top("snoekbaars");
            return big3;
        }

        [Authorize(Roles = "Administrator, User")]
        [HttpGet("biggest/bigthree/{competitionId}")]
        public async Task<ActionResult<BigThreeWinner>> GetBiggestBigThreeCompetition(Guid competitionId)
        {
            var competitionData = await _fokkersDbService.GetCompetitionAsync(competitionId.ToString());
            if (competitionData == null) return NotFound();
            var competition = competitionData.GetCompetition();
            if (!(competition.Active && competition.CompetitionEnded)) return NotFound();

            var biggestCatches = await _fokkersDbService.GetCompetitionLeaderboardItemsAsync(competitionId);
            if (biggestCatches == null) return NotFound();

            var big3list = new List<BigThree>();
            foreach (var user in biggestCatches.Select(u => u.UserEmail).Distinct())
            {
                var big3 = new BigThree { Name = _userHelper.GetUser(user).UserName };
                var pike = biggestCatches.Where(c => c.UserEmail == user && c.Fish == "Snoek").OrderByDescending(c => c.Length).FirstOrDefault();
                if (pike != null) big3.Pike = pike.GetCatch();
                var bass = biggestCatches.Where(c => c.UserEmail == user && c.Fish == "Baars").OrderByDescending(c => c.Length).FirstOrDefault();
                if (bass != null) big3.Bass = bass.GetCatch();
                var zander = biggestCatches.Where(c => c.UserEmail == user && c.Fish == "Snoekbaars").OrderByDescending(c => c.Length).FirstOrDefault();
                if (zander != null) big3.Zander = zander.GetCatch();
                big3list.Add(big3);
            }
            var winner = big3list.OrderByDescending(o => o.TotalLength).FirstOrDefault();
            if (winner == null) return NotFound();
            return new BigThreeWinner { Name = winner.Name, Fish = "Big 3", TotalLength = winner.TotalLength };
        }

        [Authorize(Roles = "Administrator, User")]
        [HttpGet("team/bigthree/{competitionId}")]
        public async Task<ActionResult<List<BigThree>>> GetBigThreeCompetitionForTeam(Guid competitionId)
        {
            var competitionData = await _fokkersDbService.GetCompetitionAsync(competitionId.ToString());
            if (competitionData == null) return NotFound();
            var competition = competitionData.GetCompetition();
            if (!competition.Active) return Ok();

            var user = _userHelper.GetUser();
            var teamMemberData = await _fokkersDbService.GetTeamMembersFromMemberAsync(user.Email);
            if (teamMemberData == null) return Forbid();
            if (!teamMemberData.Any(u => u.UserEmail == user.Email)) return NotFound();

            var catches = await _fokkersDbService.GetCompetitionLeaderboardItemsAsync(competitionId);
            if (catches.Count == 0) return NotFound();

            return BuildTeamBigThree(catches, teamMemberData, out _);
        }

        [Authorize(Roles = "Administrator, User")]
        [HttpGet("team/bigthree/all/{competitionId}")]
        public async Task<ActionResult<List<Ranking>>> GetBigThreeCompetitionForAllTeams(Guid competitionId)
        {
            var rankList = new List<Ranking>();
            var competitionData = await _fokkersDbService.GetCompetitionAsync(competitionId.ToString());
            if (competitionData == null) return NotFound();
            var competition = competitionData.GetCompetition();
            if (!(competition.Active && competition.CompetitionEnded)) return Ok();

            var teamsData = await _fokkersDbService.GetTeamsAsync();
            var allTeamMembers = await _fokkersDbService.GetTeamMembersAsync();
            var catches = await _fokkersDbService.GetCompetitionLeaderboardItemsAsync(competitionId);

            foreach (var teamData in teamsData)
            {
                var members = allTeamMembers.Where(t => t.TeamId == new Guid(teamData.RowKey)).ToList();
                var bigThree = BuildTeamBigThree(catches, members, out bool big3);

                var ranking = new Ranking
                {
                    TeamName = teamData.Name,
                    Score = bigThree[0].TotalLength + bigThree[1].TotalLength + bigThree[2].TotalLength,
                    Big3 = big3
                };
                rankList.Add(ranking);
            }

            int rankCount = 1;
            foreach (var ranking in rankList.OrderByDescending(r => r.Score))
            {
                ranking.Rank = rankCount++;
            }
            return rankList;
        }

        private List<BigThree> BuildTeamBigThree(List<CatchData> catches, List<TeamMemberData> teamMemberData, out bool big3)
        {
            big3 = true;
            var pikeList = new List<Catch>();
            var bassList = new List<Catch>();
            var zanderList = new List<Catch>();

            foreach (var teamMember in teamMemberData)
            {
                pikeList.AddRange(EnrichFor(catches, teamMember.UserEmail, "snoek"));
                bassList.AddRange(EnrichFor(catches, teamMember.UserEmail, "baars"));
                zanderList.AddRange(EnrichFor(catches, teamMember.UserEmail, "snoekbaars"));
            }

            var bigThree = new Dictionary<int, BigThree>
            {
                { 0, new BigThree { Name = "First", Pike = new Catch(), Bass = new Catch(), Zander = new Catch() } },
                { 1, new BigThree { Name = "Second", Pike = new Catch(), Bass = new Catch(), Zander = new Catch() } },
                { 2, new BigThree { Name = "Third", Pike = new Catch(), Bass = new Catch(), Zander = new Catch() } }
            };

            big3 = FillSlot(pikeList, bigThree, (b, c) => b.Pike = c) & big3;
            big3 = FillSlot(bassList, bigThree, (b, c) => b.Bass = c) & big3;
            big3 = FillSlot(zanderList, bigThree, (b, c) => b.Zander = c) & big3;

            return bigThree.Values.ToList();
        }

        private static bool FillSlot(List<Catch> list, Dictionary<int, BigThree> bigThree, Action<BigThree, Catch> assign)
        {
            bool ok = true;
            var order = list.OrderByDescending(c => c.Length).Take(3).ToList();
            if (order.Count < Constants.NUMBER_OF_BIG3) ok = false;
            int count = 0;
            foreach (var item in order)
            {
                assign(bigThree[count], item);
                if (item.Length < Constants.REQUIRED_FISH_LENGTH) ok = false;
                count++;
            }
            return ok;
        }

        private List<Catch> EnrichFor(List<CatchData> catches, string userEmail, string fishName)
        {
            return catches
                .Where(c => c.UserEmail == userEmail && c.Fish.ToLower() == fishName)
                .OrderByDescending(c => c.Length)
                .Select(EnrichCatch)
                .ToList();
        }

        [Authorize(Roles = "Administrator, User")]
        [HttpGet("team/{competitionId}")]
        public async Task<ActionResult<List<Catch>>> GetAllCompetitionForTeam(Guid competitionId)
        {
            var user = _userHelper.GetUser();
            var teamMemberData = await _fokkersDbService.GetTeamMembersFromMemberAsync(user.Email);
            if (teamMemberData == null) return NotFound();
            if (!teamMemberData.Any(u => u.UserEmail == user.Email)) return Forbid();

            var catchesMadeData = await _fokkersDbService.GetCompetitionLeaderboardItemsAsync(competitionId);
            if (catchesMadeData == null) return NotFound();

            var emails = teamMemberData.Select(t => t.UserEmail).ToHashSet();
            return catchesMadeData.Where(c => emails.Contains(c.UserEmail)).Select(EnrichCatch).ToList();
        }

        [Authorize(Roles = "Administrator, User")]
        [HttpGet("team/scores/{competitionId}")]
        public async Task<ActionResult<IEnumerable<TeamScore>>> GetAllScoresForTeam(Guid competitionId)
        {
            var competitionData = await _fokkersDbService.GetCompetitionAsync(competitionId.ToString());
            if (competitionData == null) return NotFound();
            var competition = competitionData.GetCompetition();
            if (!competition.Active) return Ok();

            var user = _userHelper.GetUser();
            var teamMemberData = await _fokkersDbService.GetTeamMembersFromMemberAsync(user.Email);
            if (teamMemberData == null) return NotFound();
            if (!teamMemberData.Any(u => u.UserEmail == user.Email)) return Forbid();

            var catchesMadeData = await _fokkersDbService.GetCompetitionLeaderboardItemsAsync(competitionId);
            if (catchesMadeData == null) return NotFound();

            var fish = await _fokkersDbService.GetFishAsync();
            var competitionFish = fish.Where(f => f.IncludeInCompetition).Select(f => f.Name).ToHashSet();
            var emails = teamMemberData.Select(t => t.UserEmail).ToHashSet();

            var catchesMade = catchesMadeData
                .Where(c => competitionFish.Contains(c.Fish) && emails.Contains(c.UserEmail))
                .Select(EnrichCatch)
                .ToList();

            var catchList = catchesMade
                .GroupBy(f => f.Fish)
                .Select(g => new TeamScore { Fish = g.Key, FishCount = g.Count(), TotalLength = g.Sum(x => x.Length) })
                .ToList();

            var catchTotal = catchesMade
                .GroupBy(f => f.CompetitionId)
                .Select(g => new TeamScore { Fish = "_Total", FishCount = g.Count(), TotalLength = g.Sum(x => x.Length) })
                .FirstOrDefault();

            if (catchList.Any() && catchTotal != null)
            {
                catchList.Add(catchTotal);
                return catchList;
            }
            return new List<TeamScore>();
        }

        [AllowAnonymous]
        [HttpGet("open/scores/{competitionId}")]
        public async Task<ActionResult<IEnumerable<TeamScore>>> GetAllCompetitionOpenByTeam(Guid competitionId)
        {
            var scores = new List<TeamScore>();
            var competitionData = await _fokkersDbService.GetCompetitionAsync(competitionId.ToString());
            if (competitionData == null) return NotFound();
            var competition = competitionData.GetCompetition();
            if (!(competition.TimeTillEnd.TotalMinutes > 120 || competition.ShowLeaderboardAfterCompetitionEnds)) return Ok();

            var teamsData = await _fokkersDbService.GetTeamsAsync();
            foreach (var teamData in teamsData)
            {
                var teamScore = new TeamScore { TeamName = teamData.Name };
                var teamMemberData = await _fokkersDbService.GetTeamMembersAsync(new Guid(teamData.RowKey));
                foreach (var teamMember in teamMemberData)
                {
                    var catchesMember = await _fokkersDbService.GetCatchesByUserInCompetitionAsync(competitionId, teamMember.UserEmail);
                    var total = catchesMember
                        .Where(c => c.Fish.ToLower() == "snoek" || c.Fish.ToLower() == "snoekbaars" || c.Fish.ToLower() == "baars")
                        .Sum(c => c.Length);
                    teamScore.TotalLength += total;
                    teamScore.FishCount += catchesMember.Count;
                }
                scores.Add(teamScore);

                int ranking = 1;
                foreach (var score in scores.OrderByDescending(r => r.TotalLength))
                {
                    score.Ranking = ranking++;
                }
            }
            return scores;
        }

        [AllowAnonymous]
        [HttpGet("open/{competitionId}")]
        public async Task<ActionResult<IEnumerable<Catch>>> GetAllCompetitionOpen(Guid competitionId)
        {
            var competitionData = await _fokkersDbService.GetCompetitionAsync(competitionId.ToString());
            if (competitionData == null) return NotFound();
            var competition = competitionData.GetCompetition();
            if (!competition.Active) return NotFound();

            var fish = await _fokkersDbService.GetFishAsync();
            var predatorFish = fish.Where(f => f.Predator).Select(f => f.Name).ToHashSet();
            var catchesMadeData = await _fokkersDbService.GetCompetitionLeaderboardItemsAsync(competitionId);
            if (catchesMadeData == null) return NotFound();

            var catchesMade = new List<Catch>();
            foreach (var catchMadeData in catchesMadeData.Where(x => predatorFish.Contains(x.Fish)))
            {
                try { catchesMade.Add(EnrichCatch(catchMadeData)); } catch { }
            }
            return catchesMade;
        }

        [AllowAnonymous]
        [HttpGet("fishermen")]
        public async Task<ActionResult<IEnumerable<FisherMan>>> GetFishermen()
        {
            var fishermen = await _fokkersDbService.GetFishermenAsync();
            if (fishermen == null) return NotFound();
            foreach (var fisher in fishermen) fisher.UserName = _userHelper.GetUser(fisher.UserEmail).UserName;
            return fishermen.OrderByDescending(f => f.TotalLength).ToList();
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet("fishermen/{competitionId}")]
        public async Task<ActionResult<IEnumerable<FisherMan>>> GetCompetitionFishermen(Guid competitionId)
        {
            var fishermen = await _fokkersDbService.GetFishermenCompetitionAsync(competitionId);
            if (fishermen == null) return NotFound();
            foreach (var fisher in fishermen) fisher.UserName = _userHelper.GetUser(fisher.UserEmail).UserName;
            return fishermen.OrderByDescending(f => f.TotalLength).ToList();
        }

        [AllowAnonymous]
        [HttpGet("fishermen/open/{competitionId}")]
        public async Task<ActionResult<IEnumerable<FisherMan>>> GetCompetitionFishermenOpen(Guid competitionId)
        {
            var competitionData = await _fokkersDbService.GetCompetitionAsync(competitionId.ToString());
            if (competitionData == null) return NotFound();
            var competition = competitionData.GetCompetition();
            if (!(competition.TimeTillEnd.TotalMinutes > 120 || competition.ShowLeaderboardAfterCompetitionEnds)) return Ok();

            var fishermen = await _fokkersDbService.GetFishermenCompetitionAsync(competitionId);
            if (fishermen == null) return NotFound();
            foreach (var fisher in fishermen) fisher.UserName = _userHelper.GetUser(fisher.UserEmail).UserName;
            return fishermen.OrderByDescending(f => f.TotalLength).ToList();
        }

        [AllowAnonymous]
        [HttpGet("bigthree")]
        public async Task<ActionResult<List<BigThree>>> GetBigThree()
        {
            var bigThree = new Dictionary<string, BigThree>();
            var fishermen = await _fokkersDbService.GetFishermenAsync();
            var pikes = await _fokkersDbService.GetTopCatchAsync("snoek");
            var bass = await _fokkersDbService.GetTopCatchAsync("baars");
            var zander = await _fokkersDbService.GetTopCatchAsync("snoekbaars");

            foreach (var fisher in fishermen)
            {
                var newBig = new BigThree { Name = _userHelper.GetUser(fisher.UserEmail).UserName, Pike = new Catch(), Bass = new Catch(), Zander = new Catch() };

                var biggestPike = pikes.Where(c => c.UserEmail.ToLower() == fisher.UserEmail.ToLower()).ToList();
                if (biggestPike.Count > 0) newBig.Pike = biggestPike.First().GetCatch();

                var biggestBass = bass.Where(c => c.UserEmail.ToLower() == fisher.UserEmail.ToLower()).ToList();
                if (biggestBass.Count > 0) newBig.Bass = biggestBass.First().GetCatch();

                var biggestZander = zander.Where(c => c.UserEmail.ToLower() == fisher.UserEmail.ToLower()).ToList();
                if (biggestZander.Count > 0) newBig.Zander = biggestZander.First().GetCatch();

                bigThree.Add(fisher.UserEmail, newBig);
            }
            return bigThree.Values.ToList();
        }
    }
}
