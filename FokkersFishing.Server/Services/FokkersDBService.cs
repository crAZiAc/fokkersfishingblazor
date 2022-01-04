using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using FokkersFishing.Interfaces;
using FokkersFishing.Models;
using FokkersFishing.Shared.Models;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using Azure;

namespace FokkersFishing.Services
{
    public class FokkersDbService : IFokkersDbService
    {
        private TableClient _catchContainer;
        private TableClient _fishContainer;

        public FokkersDbService(TableServiceClient dbClient)
        {
            this._catchContainer = dbClient.GetTableClient("Catches");
            this._fishContainer = dbClient.GetTableClient("Fish");

            this._catchContainer.CreateIfNotExists();
            this._fishContainer.CreateIfNotExists();
        }

        // Catches
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
        public async Task<List<CatchData>> GetLeaderboardItemsAsync()
        {

            try
            {
                var query = from c in _catchContainer.Query<CatchData>()
                            orderby c.Length descending
                            select c;
                return query.ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<CatchData>> GetUserItemsAsync(string userName)
        {

            try
            {
                var query = from c in _catchContainer.Query<CatchData>()
                            where c.UserName == userName
                            orderby c.CatchNumber
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
                        where c.UserName == userEmail
                        where c.PartitionKey == "Catches"
                        orderby c.CatchNumber descending
                        select c;
            try
            {
                return query.FirstOrDefault().CatchNumber;
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
                return query.FirstOrDefault().CatchNumber;
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


        // Fish
        public async Task<IEnumerable<FishData>> GetFishAsync()
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

        // "SELECT SUM(c.length) AS totalLength, SUM(1) as fishCount, c.userName as userName FROM c GROUP BY c.userName");
        // Fishermen
        public async Task<IEnumerable<FisherMan>> GetFishermenAsync()
        {

            var query = from c in _catchContainer.Query<CatchData>()
                        group c by c.UserName into userGroup
                        select new FisherMan
                        {
                            TotalLength = userGroup.Sum(x => x.Length),
                            FishCount = userGroup.Count(),
                            UserName = userGroup.Key
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
    } // end c
} // end ns

