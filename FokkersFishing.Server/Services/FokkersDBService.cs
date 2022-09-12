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

            this._catchContainer.CreateIfNotExists();
            this._fishContainer.CreateIfNotExists();
            this._competitionContainer.CreateIfNotExists();
        }

        #region Catches
        public async Task AddItemAsync(CatchData item)
        {
            await this._catchContainer.AddEntityAsync<CatchData>(item);
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

    } // end c
} // end ns

