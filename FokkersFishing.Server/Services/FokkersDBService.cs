using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FokkersFishing.Interfaces;
using FokkersFishing.Models;
using FokkersFishing.Shared.Models;
using Azure.Data.Tables;
using FokkersFishing.Data;

namespace FokkersFishing.Services
{
    public class FokkersDbService : IFokkersDbService
    {
        private TableClient _catchContainer;
        private TableClient _fishContainer;
        private TableClient _competitionContainer;
        private TableClient _teamContainer;
        private TableClient _teamMemberContainer;
        private string _connectionString;

        public string StorageConnectionString
        {
            get
            {
                return _connectionString;
            }
        }

        public FokkersDbService(TableServiceClient dbClient, string connectionString)
        {
            _connectionString = connectionString;
            this._catchContainer = dbClient.GetTableClient("Catches");
            this._fishContainer = dbClient.GetTableClient("Fish");
            this._competitionContainer = dbClient.GetTableClient("Competitions");
            this._teamContainer = dbClient.GetTableClient("Teams");
            this._teamMemberContainer = dbClient.GetTableClient("TeamMembers");

            this._catchContainer.CreateIfNotExists();
            this._fishContainer.CreateIfNotExists();
            this._competitionContainer.CreateIfNotExists();
            this._teamContainer.CreateIfNotExists();
            this._teamMemberContainer.CreateIfNotExists();
        }

        #region Catches
        public async Task AddItemAsync(CatchData item)
        {
            try
            {
                await this._catchContainer.AddEntityAsync<CatchData>(item);
            }
            catch (Exception ex)
            {

            }
        }

        public async Task DeleteItemAsync(string rowKey)
        {
            await this._catchContainer.DeleteEntityAsync("Catches", rowKey);
        }

        public async Task<CatchData> GetItemAsync(string rowKey)
        {
            try
            {
                var query = from c in _catchContainer.Query<CatchData>()
                            where c.RowKey == rowKey
                            where c.PartitionKey == "Catches"
                            select c;
                return query.FirstOrDefault();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<CatchData> GetUserItemAsync(string rowKey, string userEmail)

        {
            try
            {
                var query = from c in _catchContainer.Query<CatchData>()
                            where c.RowKey == rowKey
                            where c.UserEmail == userEmail
                            where c.PartitionKey == "Catches"
                            select c;
                return query.FirstOrDefault();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<List<CatchData>> GetLeaderboardItemsAsync()
        {

            try
            {
                var query = from c in _catchContainer.Query<CatchData>()
                            where c.Status == CatchStatusEnum.Approved
                            orderby c.Length descending
                            select c;
                return query.ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<CatchData>> GetCompetitionLeaderboardItemsAsync(Guid competitionId)
        {
            try
            {
                var query = from c in _catchContainer.Query<CatchData>()
                            where c.Status != CatchStatusEnum.Rejected
                            where c.CompetitionId == competitionId
                            orderby c.Length descending
                            select c;
                return query.ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<Catch>> GetTopCatchAsync(string fish)
        {
            try
            {
                var query = from c in _catchContainer.Query<CatchData>()
                            where c.Fish.ToLower() == fish.ToLower()
                            where c.Status == CatchStatusEnum.Approved
                            orderby c.Length descending
                            group c by c.UserEmail into userGroup
                            select new Catch
                            {
                                UserEmail = userGroup.Key,
                                Length = userGroup.Max(m => m.Length),
                                Fish = userGroup.Max(m => m.Fish),
                                CatchDate = userGroup.Max(m => m.CatchDate),
                                MeasurePhotoUrl = userGroup.Max(m => m.MeasurePhotoUrl),
                                CatchPhotoUrl = userGroup.Max(m => m.CatchPhotoUrl),
                                GlobalCatchNumber = userGroup.Max(m => m.GlobalCatchNumber)
                            };
                return query.ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        public async Task<List<CatchData>> GetCatchesByUserInCompetitionAsync(Guid competitionId, string userEmail)
        {
            try
            {
                var query = from c in _catchContainer.Query<CatchData>()
                             where c.Status == CatchStatusEnum.Approved | c.Status == CatchStatusEnum.Pending
                             where c.UserEmail == userEmail
                             where c.CompetitionId == competitionId
                             orderby c.Length descending
                             select c;
                return query.ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<Catch>> GetTopCatchesByUserInCompetitionAsync(Guid competitionId, string userEmail, string fishName)
        {
            try
            {
                var query = (from c in _catchContainer.Query<CatchData>()
                            where c.Status == CatchStatusEnum.Approved | c.Status == CatchStatusEnum.Pending
                            where c.UserEmail == userEmail
                            where c.Fish.ToLower() == fishName.ToLower()
                            where c.CompetitionId == competitionId
                            orderby c.Length descending
                            group c by new { c.Fish, c.Length, c.UserEmail } into fishGroup
                            select new Catch
                            {
                                UserEmail = fishGroup.Key.UserEmail,
                                Length = fishGroup.Key.Length,
                                Fish = fishGroup.Key.Fish,
                                Status = fishGroup.Max(m => m.Status),
                                CatchDate = fishGroup.Max(m => m.CatchDate),
                                MeasurePhotoUrl = fishGroup.Max(m => m.MeasurePhotoUrl),
                                CatchPhotoUrl = fishGroup.Max(m => m.CatchPhotoUrl),
                                GlobalCatchNumber = fishGroup.Max(m => m.GlobalCatchNumber),
                                CatchNumber = fishGroup.Max(m => m.CatchNumber)
                            }).Take(3);
                return query.ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<CatchData>> GetUserItemsAsync(string userEmail)
        {
            try
            {
                var query = from c in _catchContainer.Query<CatchData>()
                            where c.UserEmail == userEmail
                            orderby c.CatchNumber descending
                            select c;
                return query.ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<CatchData>> GetAllCatches()
        {
            try
            {
                var query = from c in _catchContainer.Query<CatchData>()
                            orderby c.GlobalCatchNumber descending
                            select c;
                return query.ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<CatchData>> GetCatchesInCompetitionsAsync(Guid competitionId)
        {
            try
            {
                var query = from c in _catchContainer.Query<CatchData>()
                            where c.CompetitionId == competitionId
                            orderby c.Fish, c.Length descending
                            select c;
                return query.ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<CatchData>> GetPendingCatches()
        {
            try
            {
                var query = from c in _catchContainer.Query<CatchData>()
                            where c.Status == CatchStatusEnum.Pending
                            orderby c.GlobalCatchNumber descending
                            select c;
                return query.ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<int> GetCatchNumberCount(string userEmail)
        {
            var query = from c in _catchContainer.Query<CatchData>()
                        where c.UserEmail == userEmail
                        where c.PartitionKey == "Catches"
                        orderby c.CatchNumber descending
                        select c;
            try
            {
                if (query.Count() > 0)
                {
                    return query.FirstOrDefault().CatchNumber;
                }
                else
                    return 0;
            }
            catch (Exception ex)
            {

            }
            return 0;
        }

        public async Task<int> GetGlobalCatchNumberCount()
        {
            var query = from c in _catchContainer.Query<CatchData>()
                        where c.PartitionKey == "Catches"
                        orderby c.GlobalCatchNumber descending
                        select c;
            try
            {
                if (query.Count() > 0)
                {
                    return query.FirstOrDefault().GlobalCatchNumber;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {

            }
            return 0;
        }

        public async Task UpdateItemAsync(CatchData item)
        {
            await this._catchContainer.UpdateEntityAsync<CatchData>(item, item.ETag);
        }

        #endregion
        #region Fish
        public async Task<List<FishData>> GetFishAsync()
        {
            var query = from f in _fishContainer.Query<FishData>()
                        orderby f.Name
                        select f;
            try
            {
                return query.ToList();
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public async Task<List<FisherMan>> GetFishermenAsync()
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
            try
            {
                return query.ToList();
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public async Task<List<FisherMan>> GetFishermenCompetitionAsync(Guid competitionId)
        {
            var query = from c in _catchContainer.Query<CatchData>()
                        where c.Status != CatchStatusEnum.Rejected
                        where c.CompetitionId == competitionId
                        group c by c.UserEmail into userGroup
                        select new FisherMan
                        {
                            TotalLength = userGroup.Sum(x => x.Length),
                            FishCount = userGroup.Count(),
                            UserEmail = userGroup.Key
                        };
            try
            {
                return query.ToList();
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        #endregion
        #region Competition
        public async Task<List<CompetitionData>> GetCompetitionsAsync()
        {
            var query = from c in _competitionContainer.Query<CompetitionData>()
                        orderby c.StartDate
                        select c;
            try
            {
                return query.ToList();
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public async Task<CompetitionData> GetCompetitionAsync(string rowKey)
        {
            try
            {
                var query = from c in _competitionContainer.Query<CompetitionData>()
                            where c.RowKey == rowKey
                            where c.PartitionKey == "Competition"
                            select c;
                return query.FirstOrDefault();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task AddCompetitionAsync(CompetitionData item)
        {
            try
            {

                await this._competitionContainer.AddEntityAsync<CompetitionData>(item);
            }
            catch (Exception ex)
            {
            }
        }

        public async Task DeleteCompetitionAsync(string rowKey)
        {
            await this._competitionContainer.DeleteEntityAsync("Competition", rowKey);
        }

        public async Task UpdateCompetitionAsync(CompetitionData item)
        {
            await this._competitionContainer.UpdateEntityAsync<CompetitionData>(item, item.ETag);
        }
        #endregion
        #region Team
        public async Task<List<TeamData>> GetTeamsAsync()
        {
            var query = from c in _teamContainer.Query<TeamData>()
                        orderby c.Name
                        select c;
            try
            {
                return query.ToList();
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public async Task<TeamData> GetTeamAsync(string rowKey)
        {
            try
            {
                var query = from c in _teamContainer.Query<TeamData>()
                            where c.RowKey == rowKey
                            where c.PartitionKey == "Team"
                            select c;
                return query.FirstOrDefault();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task AddTeamAsync(TeamData item)
        {
            try
            {

                await this._teamContainer.AddEntityAsync<TeamData>(item);
            }
            catch (Exception ex)
            {
            }
        }

        public async Task DeleteTeamAsync(string rowKey)
        {
            await this._teamContainer.DeleteEntityAsync("Team", rowKey);
        }

        public async Task UpdateTeamAsync(TeamData item)
        {
            await this._teamContainer.UpdateEntityAsync<TeamData>(item, item.ETag);
        }



        #endregion
        #region Team Members
        public async Task<List<TeamMemberData>> GetTeamMembersAsync()
        {
            var query = from c in _teamMemberContainer.Query<TeamMemberData>()
                        where c.PartitionKey == "TeamMember"
                        orderby c.TeamId, c.UserEmail
                        select c;
            try
            {
                return query.ToList();
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public async Task<List<TeamMemberData>> GetTeamMembersFromMemberAsync(string userEmail)
        {
            var query = from c in _teamMemberContainer.Query<TeamMemberData>()
                        where c.UserEmail == userEmail
                        where c.PartitionKey == "TeamMember"
                        orderby c.TeamId, c.UserEmail
                        select c;
            try
            {
                if (query.Count() > 0)
                {
                    return await GetTeamMembersAsync(query.FirstOrDefault().TeamId);
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public async Task<List<TeamMemberData>> GetTeamMembersAsync(Guid teamId)
        {
            var query = from c in _teamMemberContainer.Query<TeamMemberData>()
                        where c.PartitionKey == "TeamMember"
                        where c.TeamId == teamId
                        orderby c.UserEmail
                        select c;
            try
            {
                return query.ToList();
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        public async Task<TeamMemberData> GetTeamMemberAsync(string rowKey)
        {
            try
            {
                var query = from c in _teamMemberContainer.Query<TeamMemberData>()
                            where c.RowKey == rowKey
                            where c.PartitionKey == "TeamMember"
                            select c;
                return query.FirstOrDefault();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task AddTeamMemberAsync(TeamMemberData item)
        {
            try
            {
                await this._teamMemberContainer.AddEntityAsync<TeamMemberData>(item);
            }
            catch (Exception ex)
            {
            }
        }

        public async Task DeleteTeamMemberAsync(string rowKey)
        {
            await this._teamMemberContainer.DeleteEntityAsync("TeamMember", rowKey);
        }

        public async Task DeleteTeamMembersAsync(Guid teamId)
        {
            try
            {
                var query = from c in _teamMemberContainer.Query<TeamMemberData>()
                            where c.TeamId == teamId
                            where c.PartitionKey == "TeamMember"
                            select c;
                if (query.Count() > 0)
                {
                    foreach (TeamMemberData teamMemberData in query.ToList())
                    {
                        await this._teamMemberContainer.DeleteEntityAsync("TeamMember", teamMemberData.RowKey);
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        public async Task UpdateTeamAsync(TeamMemberData item)
        {
            await this._teamMemberContainer.UpdateEntityAsync<TeamMemberData>(item, item.ETag);
        }

        #endregion
    } // end c
} // end ns

