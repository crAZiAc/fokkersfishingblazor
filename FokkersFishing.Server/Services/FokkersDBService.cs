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
        private TableClient _fishermenContainer;
        private TableClient _fishContainer;

        public FokkersDbService(TableServiceClient dbClient)
        {
            this._catchContainer = dbClient.GetTableClient("Catches");
            this._fishContainer = dbClient.GetTableClient("Fishermen");
            this._fishermenContainer = dbClient.GetTableClient("Fish");

            this._catchContainer.CreateIfNotExists();
            this._fishContainer.CreateIfNotExists();
            this._fishermenContainer.CreateIfNotExists();
        }


        // Catches
        public async Task AddItemAsync(Catch item)
        {
            await this._catchContainer.AddEntityAsync<Catch>(item);
        }

        public async Task DeleteItemAsync(string rowKey)
        {
            await this._catchContainer.DeleteEntityAsync("Catches", rowKey);
        }

        public async Task<Catch> GetItemAsync(string rowKey)
        {
            try
            {
                var query = from c in _catchContainer.Query<Catch>()
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
        public async Task<List<Catch>> GetItemsAsync(string filter)
        {

            Pageable<Catch> queryResultsFilter = _catchContainer.Query<Catch>(
                filter: $"PartitionKey eq 'Catches' and {filter}");
            try
            {
                return queryResultsFilter.ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<int> GetCatchNumberCount(string userEmail)
        {
            var query = from c in _catchContainer.Query<Catch>()
                        where c.UserName == userEmail
                        where c.PartitionKey == "Catches"
                        orderby c.CatchNumber
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
            var query = from c in _catchContainer.Query<Catch>()
                        where c.PartitionKey == "Catches"
                        orderby c.GlobalCatchNumber
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

        public async Task UpdateItemAsync(Catch item)
        {
            await this._catchContainer.UpdateEntityAsync<Catch>(item, item.ETag);
        }


        // Fish
        public async Task<IEnumerable<Fish>> GetFishAsync()
        {
            var query = from f in _fishContainer.Query<Fish>()
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

            var query = from c in _catchContainer.Query<Catch>()
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

