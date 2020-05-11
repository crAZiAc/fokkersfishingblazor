using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Configuration;
using FokkersFishing.Interfaces;
using FokkersFishing.Models;
using FokkersFishing.Shared.Models;

namespace FokkersFishing.Services
{
    public class FokkersDbService : IFokkersDbService
    {
        private Container _catchContainer;
        private Container _fishContainer;

        public FokkersDbService(
            CosmosClient dbClient,
            string databaseName,
            string containerName)
        {
            this._catchContainer = dbClient.GetContainer(databaseName, "Catches");
            this._fishContainer = dbClient.GetContainer(databaseName, "Fish");
        }


        // Catches
        public async Task AddItemAsync(Catch item)
        {
            await this._catchContainer.CreateItemAsync<Catch>(item, new PartitionKey(item.Id.ToString()));
        }

        public async Task DeleteItemAsync(string id)
        {
            await this._catchContainer.DeleteItemAsync<Catch>(id, new PartitionKey(id));
        }

        public async Task<Catch> GetItemAsync(string id)
        {
            try
            {
                ItemResponse<Catch> response = await this._catchContainer.ReadItemAsync<Catch>(id, new PartitionKey(id));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

        }

        public async Task<IEnumerable<Catch>> GetItemsAsync(string queryString)
        {
            var query = this._catchContainer.GetItemQueryIterator<Catch>(new QueryDefinition(queryString));
            List<Catch> results = new List<Catch>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();

                results.AddRange(response.ToList());
            }

            return results;
        }

        public async Task<int> GetCatchNumberCount(string userEmail)
        {
            string queryString = "SELECT top 1 c.catchNumber FROM c where c.userName = '" + userEmail + "' order by c.catchNumber DESC";
            var query = this._catchContainer.GetItemQueryIterator<Catch>(new QueryDefinition(queryString));
            List<Catch> results = new List<Catch>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();

                results.AddRange(response.ToList());
            }
            try
            {
                return results.FirstOrDefault().CatchNumber;
            }
            catch (Exception ex)
            {

            }
            return 0;
        }

        public async Task<int> GetGlobalCatchNumberCount()
        {
            string queryString = "SELECT top 1 c.globalCatchNumber FROM c order by c.globalCatchNumber DESC";
            var query = this._catchContainer.GetItemQueryIterator<Catch>(new QueryDefinition(queryString));
                        List<Catch> results = new List<Catch>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();

                results.AddRange(response.ToList());
            }
            try
            {
                return results.FirstOrDefault().GlobalCatchNumber;
            }
            catch (Exception ex)
            {

            }
            return 0;
        }

        public async Task UpdateItemAsync(string id, Catch item)
        {
            await this._catchContainer.UpsertItemAsync<Catch>(item, new PartitionKey(id));
        }


        // Fish
        public async Task<IEnumerable<Fish>> GetFishAsync(string queryString)
        {
            var query = this._fishContainer.GetItemQueryIterator<Fish>(new QueryDefinition(queryString));
            List<Fish> results = new List<Fish>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();

                results.AddRange(response.ToList());
            }

            return results;
        }


        // Fishermen
        public async Task<IEnumerable<FisherMan>> GetFishermenAsync(string queryString)
        {
            var query = this._catchContainer.GetItemQueryIterator<FisherMan>(new QueryDefinition(queryString));
            List<FisherMan> results = new List<FisherMan>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();

                results.AddRange(response.ToList());
            }

            return results;
        }
    } // end c
} // end ns

