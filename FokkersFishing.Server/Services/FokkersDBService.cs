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
        private Container _container;

        public FokkersDbService(
            CosmosClient dbClient,
            string databaseName,
            string containerName)
        {
            this._container = dbClient.GetContainer(databaseName, containerName);
        }

        public async Task AddItemAsync(Catch item)
        {
            await this._container.CreateItemAsync<Catch>(item, new PartitionKey(item.Id.ToString()));
        }

        public async Task DeleteItemAsync(string id)
        {
            await this._container.DeleteItemAsync<Catch>(id, new PartitionKey(id));
        }

        public async Task<Catch> GetItemAsync(string id)
        {
            try
            {
                ItemResponse<Catch> response = await this._container.ReadItemAsync<Catch>(id, new PartitionKey(id));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

        }

        public async Task<IEnumerable<Catch>> GetItemsAsync(string queryString)
        {
            var query = this._container.GetItemQueryIterator<Catch>(new QueryDefinition(queryString));
            List<Catch> results = new List<Catch>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();

                results.AddRange(response.ToList());
            }

            return results;
        }

        public async Task<int> GetCatchNumberCount()
        {
            string queryString = "SELECT top 1 c.catchNumber FROM c order by c.catchNumber DESC";
            var query = this._container.GetItemQueryIterator<Catch>(new QueryDefinition(queryString));
            List<Catch> results = new List<Catch>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();

                results.AddRange(response.ToList());
            }
            return results.FirstOrDefault().CatchNumber;
        }

        public async Task<int> GetGlobalCatchNumberCount()
        {
            string queryString = "SELECT top 1 c.globalCatchNumber FROM c order by c.globalCatchNumber DESC";
            var query = this._container.GetItemQueryIterator<Catch>(new QueryDefinition(queryString));
            List<Catch> results = new List<Catch>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();

                results.AddRange(response.ToList());
            }
            return results.FirstOrDefault().GlobalCatchNumber;
        }

        public async Task UpdateItemAsync(string id, Catch item)
        {
            await this._container.UpsertItemAsync<Catch>(item, new PartitionKey(id));
        }
    } // end c
} // end ns

