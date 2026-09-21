using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Data.Tables;
using FokkersFishing.Api.Models.Dtos;
using FokkersFishing.Api.Models.Entities;

namespace FokkersFishing.Api.Services
{
    /// <summary>
    /// Azure Table Storage data access. Ported from the original Blazor server so it
    /// reads/writes the exact same tables and shapes.
    /// </summary>
    public class FokkersDbService : IFokkersDbService
    {
        private readonly TableClient _catchContainer;
        private readonly TableClient _fishContainer;
        private readonly TableClient _competitionContainer;
        private readonly TableClient _teamContainer;
        private readonly TableClient _teamMemberContainer;
        private readonly string _connectionString;

        public string StorageConnectionString => _connectionString;

        public FokkersDbService(TableServiceClient dbClient, string connectionString)
        {
            _connectionString = connectionString;
            _catchContainer = dbClient.GetTableClient("Catches");
            _fishContainer = dbClient.GetTableClient("Fish");
            _competitionContainer = dbClient.GetTableClient("Competitions");
            _teamContainer = dbClient.GetTableClient("Teams");
            _teamMemberContainer = dbClient.GetTableClient("TeamMembers");

            _catchContainer.CreateIfNotExists();
            _fishContainer.CreateIfNotExists();
            _competitionContainer.CreateIfNotExists();
            _teamContainer.CreateIfNotExists();
            _teamMemberContainer.CreateIfNotExists();
        }

        #region Catches
        public async Task AddItemAsync(CatchData item)
        {
            try { await _catchContainer.AddEntityAsync(item); } catch { }
        }

        public async Task DeleteItemAsync(string rowKey)
        {
            await _catchContainer.DeleteEntityAsync("Catches", rowKey);
        }

        public Task<CatchData> GetItemAsync(string rowKey)
        {
            try
            {
                var query = from c in _catchContainer.Query<CatchData>()
                            where c.RowKey == rowKey && c.PartitionKey == "Catches"
                            select c;
                return Task.FromResult(query.FirstOrDefault());
            }
            catch { return Task.FromResult<CatchData>(null); }
        }

        public Task<CatchData> GetUserItemAsync(string rowKey, string userEmail)
        {
            try
            {
                var query = from c in _catchContainer.Query<CatchData>()
                            where c.RowKey == rowKey && c.UserEmail == userEmail && c.PartitionKey == "Catches"
                            select c;
                return Task.FromResult(query.FirstOrDefault());
            }
            catch { return Task.FromResult<CatchData>(null); }
        }

        public async Task<CatchData> GetTeamItemAsync(string rowKey, string userEmail)
        {
            try
            {
                var usersInTeam = await GetTeamMembersFromMemberAsync(userEmail);
                if (usersInTeam != null && usersInTeam.Any())
                {
                    foreach (var member in usersInTeam)
                    {
                        var query = from c in _catchContainer.Query<CatchData>()
                                    where c.RowKey == rowKey && c.PartitionKey == "Catches" && c.UserEmail == member.UserEmail
                                    select c;
                        if (query.Any()) return query.FirstOrDefault();
                    }
                }
            }
            catch { return null; }
            return null;
        }

        public Task<List<CatchData>> GetLeaderboardItemsAsync()
        {
            try
            {
                var query = from c in _catchContainer.Query<CatchData>()
                            where c.Status == CatchStatusEnum.Approved
                            orderby c.Length descending
                            select c;
                return Task.FromResult(query.ToList());
            }
            catch { return Task.FromResult<List<CatchData>>(null); }
        }

        public Task<List<CatchData>> GetCompetitionLeaderboardItemsAsync(Guid competitionId)
        {
            try
            {
                var query = from c in _catchContainer.Query<CatchData>()
                            where c.Status != CatchStatusEnum.Rejected && c.CompetitionId == competitionId
                            orderby c.Length descending
                            select c;
                return Task.FromResult(query.ToList());
            }
            catch { return Task.FromResult<List<CatchData>>(null); }
        }

        public Task<List<CatchData>> GetAdminCompetitionLeaderboardItemsAsync(Guid competitionId)
            => GetCompetitionLeaderboardItemsAsync(competitionId);

        public Task<List<CatchData>> GetTopCatchAsync(string fish)
        {
            try
            {
                var query = from c in _catchContainer.Query<CatchData>()
                            where c.Fish.ToLower() == fish.ToLower() && c.Status == CatchStatusEnum.Approved
                            orderby c.Length descending
                            select c;
                return Task.FromResult(query.ToList());
            }
            catch { return Task.FromResult<List<CatchData>>(null); }
        }

        public Task<List<CatchData>> GetCatchesByUserInCompetitionAsync(Guid competitionId, string userEmail)
        {
            try
            {
                var query = from c in _catchContainer.Query<CatchData>()
                            where (c.Status == CatchStatusEnum.Approved || c.Status == CatchStatusEnum.Pending)
                                  && c.UserEmail == userEmail && c.CompetitionId == competitionId
                            orderby c.Length descending
                            select c;
                return Task.FromResult(query.ToList());
            }
            catch { return Task.FromResult<List<CatchData>>(null); }
        }

        public Task<List<Catch>> GetTopCatchesByUserInCompetitionAsync(Guid competitionId, string userEmail, string fishName)
        {
            try
            {
                var query = (from c in _catchContainer.Query<CatchData>()
                             where (c.Status == CatchStatusEnum.Approved || c.Status == CatchStatusEnum.Pending)
                                   && c.UserEmail == userEmail && c.Fish.ToLower() == fishName.ToLower()
                                   && c.CompetitionId == competitionId
                             orderby c.Length descending
                             group c by new { c.Fish, c.Length, c.UserEmail } into fishGroup
                             select new Catch
                             {
                                 UserEmail = fishGroup.Key.UserEmail,
                                 RegisterUserEmail = fishGroup.Max(m => m.RegisterUserEmail),
                                 Length = fishGroup.Key.Length,
                                 Fish = fishGroup.Key.Fish,
                                 Status = fishGroup.Max(m => m.Status),
                                 CatchDate = fishGroup.Max(m => m.CatchDate),
                                 MeasurePhotoUrl = fishGroup.Max(m => m.MeasurePhotoUrl),
                                 CatchPhotoUrl = fishGroup.Max(m => m.CatchPhotoUrl),
                                 GlobalCatchNumber = fishGroup.Max(m => m.GlobalCatchNumber),
                                 CatchNumber = fishGroup.Max(m => m.CatchNumber)
                             }).Take(3);
                return Task.FromResult(query.ToList());
            }
            catch { return Task.FromResult<List<Catch>>(null); }
        }

        public Task<List<Catch>> GetBiggestFishPerUserInCompetition(Guid competitionId)
        {
            try
            {
                var fishCompetition = (from f in _fishContainer.Query<FishData>()
                                       where f.IncludeInCompetition == true
                                       select f.Name).ToList();

                var query = from c in _catchContainer.Query<CatchData>()
                            where fishCompetition.Contains(c.Fish)
                                  && (c.Status == CatchStatusEnum.Approved || c.Status == CatchStatusEnum.Pending)
                                  && c.CompetitionId == competitionId
                            orderby c.Length descending
                            group c by new { c.Fish, c.UserEmail } into fishGroup
                            select new Catch
                            {
                                UserEmail = fishGroup.Key.UserEmail,
                                RegisterUserEmail = fishGroup.Max(m => m.RegisterUserEmail),
                                Length = fishGroup.Max(m => m.Length),
                                Fish = fishGroup.Key.Fish,
                                Status = fishGroup.Max(m => m.Status),
                                CatchDate = fishGroup.Max(m => m.CatchDate),
                                MeasurePhotoUrl = fishGroup.Max(m => m.MeasurePhotoUrl),
                                CatchPhotoUrl = fishGroup.Max(m => m.CatchPhotoUrl),
                                GlobalCatchNumber = fishGroup.Max(m => m.GlobalCatchNumber),
                                CatchNumber = fishGroup.Max(m => m.CatchNumber)
                            };
                return Task.FromResult(query.ToList());
            }
            catch { return Task.FromResult<List<Catch>>(null); }
        }

        public Task<List<CatchData>> GetUserItemsAsync(string userEmail)
        {
            try
            {
                var query = from c in _catchContainer.Query<CatchData>()
                            where c.UserEmail == userEmail
                            orderby c.CatchNumber descending
                            select c;
                return Task.FromResult(query.ToList());
            }
            catch { return Task.FromResult<List<CatchData>>(null); }
        }

        public Task<List<CatchData>> GetAllCatches()
        {
            try
            {
                var query = from c in _catchContainer.Query<CatchData>()
                            orderby c.GlobalCatchNumber descending
                            select c;
                return Task.FromResult(query.ToList());
            }
            catch { return Task.FromResult<List<CatchData>>(null); }
        }

        public Task<List<CatchData>> GetCatchesInCompetitionsAsync(Guid competitionId)
        {
            try
            {
                var query = from c in _catchContainer.Query<CatchData>()
                            where c.CompetitionId == competitionId
                            orderby c.Fish, c.Length descending
                            select c;
                return Task.FromResult(query.ToList());
            }
            catch { return Task.FromResult<List<CatchData>>(null); }
        }

        public Task<List<CatchData>> GetPendingCatches()
        {
            try
            {
                var query = from c in _catchContainer.Query<CatchData>()
                            where c.Status == CatchStatusEnum.Pending
                            orderby c.GlobalCatchNumber descending
                            select c;
                return Task.FromResult(query.ToList());
            }
            catch { return Task.FromResult<List<CatchData>>(null); }
        }

        public Task<int> GetCatchNumberCount(string userEmail)
        {
            try
            {
                var query = from c in _catchContainer.Query<CatchData>()
                            where c.UserEmail == userEmail && c.PartitionKey == "Catches"
                            orderby c.CatchNumber descending
                            select c;
                return Task.FromResult(query.Any() ? query.First().CatchNumber : 0);
            }
            catch { return Task.FromResult(0); }
        }

        public Task<int> GetGlobalCatchNumberCount()
        {
            try
            {
                var query = from c in _catchContainer.Query<CatchData>()
                            where c.PartitionKey == "Catches"
                            orderby c.GlobalCatchNumber descending
                            select c;
                return Task.FromResult(query.Any() ? query.First().GlobalCatchNumber : 0);
            }
            catch { return Task.FromResult(0); }
        }

        public async Task UpdateItemAsync(CatchData item)
        {
            await _catchContainer.UpdateEntityAsync(item, item.ETag);
        }
        #endregion

        #region Fish
        public Task<List<FishData>> GetFishAsync()
        {
            try
            {
                var query = from f in _fishContainer.Query<FishData>()
                            orderby f.Name
                            select f;
                return Task.FromResult(query.ToList());
            }
            catch { return Task.FromResult<List<FishData>>(null); }
        }

        public Task<List<FisherMan>> GetFishermenAsync()
        {
            try
            {
                var query = from c in _catchContainer.Query<CatchData>()
                            where c.Status == CatchStatusEnum.Approved
                            group c by c.UserEmail into userGroup
                            select new FisherMan
                            {
                                TotalLength = userGroup.Sum(x => x.Length),
                                FishCount = userGroup.Count(),
                                UserEmail = userGroup.Key
                            };
                return Task.FromResult(query.ToList());
            }
            catch { return Task.FromResult<List<FisherMan>>(null); }
        }

        public Task<List<FisherMan>> GetFishermenCompetitionAsync(Guid competitionId)
        {
            try
            {
                var query = from c in _catchContainer.Query<CatchData>()
                            where c.Status != CatchStatusEnum.Rejected && c.CompetitionId == competitionId
                            group c by c.UserEmail into userGroup
                            select new FisherMan
                            {
                                TotalLength = userGroup.Sum(x => x.Length),
                                FishCount = userGroup.Count(),
                                UserEmail = userGroup.Key
                            };
                return Task.FromResult(query.ToList());
            }
            catch { return Task.FromResult<List<FisherMan>>(null); }
        }
        #endregion

        #region Competition
        public Task<List<CompetitionData>> GetCompetitionsAsync()
        {
            try
            {
                var query = from c in _competitionContainer.Query<CompetitionData>()
                            orderby c.StartDate
                            select c;
                return Task.FromResult(query.ToList());
            }
            catch { return Task.FromResult<List<CompetitionData>>(null); }
        }

        public Task<CompetitionData> GetCompetitionAsync(string rowKey)
        {
            try
            {
                var query = from c in _competitionContainer.Query<CompetitionData>()
                            where c.RowKey == rowKey && c.PartitionKey == "Competition"
                            select c;
                return Task.FromResult(query.FirstOrDefault());
            }
            catch { return Task.FromResult<CompetitionData>(null); }
        }

        public async Task AddCompetitionAsync(CompetitionData item)
        {
            try { await _competitionContainer.AddEntityAsync(item); } catch { }
        }

        public async Task DeleteCompetitionAsync(string rowKey)
        {
            await _competitionContainer.DeleteEntityAsync("Competition", rowKey);
        }

        public async Task UpdateCompetitionAsync(CompetitionData item)
        {
            try { await _competitionContainer.UpdateEntityAsync(item, item.ETag); } catch { }
        }
        #endregion

        #region Team
        public Task<List<TeamData>> GetTeamsAsync()
        {
            try
            {
                var query = from c in _teamContainer.Query<TeamData>()
                            orderby c.Name
                            select c;
                return Task.FromResult(query.ToList());
            }
            catch { return Task.FromResult<List<TeamData>>(null); }
        }

        public Task<TeamData> GetTeamByUserAsync(string userEmail)
        {
            try
            {
                var memberData = from m in _teamMemberContainer.Query<TeamMemberData>()
                                 where m.UserEmail.ToLower() == userEmail.ToLower()
                                 select m;
                var member = memberData.FirstOrDefault();
                if (member == null) return Task.FromResult<TeamData>(null);

                var query = from c in _teamContainer.Query<TeamData>()
                            where c.RowKey == member.TeamId.ToString()
                            select c;
                return Task.FromResult(query.FirstOrDefault());
            }
            catch { return Task.FromResult<TeamData>(null); }
        }

        public Task<TeamData> GetTeamAsync(string rowKey)
        {
            try
            {
                var query = from c in _teamContainer.Query<TeamData>()
                            where c.RowKey == rowKey && c.PartitionKey == "Team"
                            select c;
                return Task.FromResult(query.FirstOrDefault());
            }
            catch { return Task.FromResult<TeamData>(null); }
        }

        public async Task AddTeamAsync(TeamData item)
        {
            try { await _teamContainer.AddEntityAsync(item); } catch { }
        }

        public async Task DeleteTeamAsync(string rowKey)
        {
            await DeleteTeamMembersAsync(Guid.Parse(rowKey));
            await _teamContainer.DeleteEntityAsync("Team", rowKey);
        }

        public async Task UpdateTeamAsync(TeamData item)
        {
            await _teamContainer.UpdateEntityAsync(item, item.ETag);
        }
        #endregion

        #region Team Members
        public Task<List<TeamMemberData>> GetTeamMembersAsync()
        {
            try
            {
                var query = from c in _teamMemberContainer.Query<TeamMemberData>()
                            where c.PartitionKey == "TeamMember"
                            orderby c.TeamId, c.UserEmail
                            select c;
                return Task.FromResult(query.ToList());
            }
            catch { return Task.FromResult<List<TeamMemberData>>(null); }
        }

        public async Task<List<TeamMemberData>> GetTeamMembersFromMemberAsync(string userEmail)
        {
            try
            {
                var query = from c in _teamMemberContainer.Query<TeamMemberData>()
                            where c.UserEmail == userEmail && c.PartitionKey == "TeamMember"
                            orderby c.TeamId, c.UserEmail
                            select c;
                if (query.Any())
                {
                    return await GetTeamMembersAsync(query.First().TeamId);
                }
            }
            catch { }
            return null;
        }

        public Task<List<TeamMemberData>> GetTeamMembersAsync(Guid teamId)
        {
            try
            {
                var query = from c in _teamMemberContainer.Query<TeamMemberData>()
                            where c.PartitionKey == "TeamMember" && c.TeamId == teamId
                            orderby c.UserEmail
                            select c;
                return Task.FromResult(query.ToList());
            }
            catch { return Task.FromResult<List<TeamMemberData>>(null); }
        }

        public Task<TeamMemberData> GetTeamMemberAsync(string rowKey)
        {
            try
            {
                var query = from c in _teamMemberContainer.Query<TeamMemberData>()
                            where c.RowKey == rowKey && c.PartitionKey == "TeamMember"
                            select c;
                return Task.FromResult(query.FirstOrDefault());
            }
            catch { return Task.FromResult<TeamMemberData>(null); }
        }

        public async Task AddTeamMemberAsync(TeamMemberData item)
        {
            try { await _teamMemberContainer.AddEntityAsync(item); } catch { }
        }

        public async Task DeleteTeamMemberAsync(string rowKey)
        {
            await _teamMemberContainer.DeleteEntityAsync("TeamMember", rowKey);
        }

        public async Task DeleteTeamMembersAsync(Guid teamId)
        {
            try
            {
                var query = from c in _teamMemberContainer.Query<TeamMemberData>()
                            where c.TeamId == teamId && c.PartitionKey == "TeamMember"
                            select c;
                foreach (var teamMemberData in query.ToList())
                {
                    await _teamMemberContainer.DeleteEntityAsync("TeamMember", teamMemberData.RowKey);
                }
            }
            catch { }
        }

        public async Task UpdateTeamMemberAsync(TeamMemberData item)
        {
            await _teamMemberContainer.UpdateEntityAsync(item, item.ETag);
        }
        #endregion
    }
}
