using FokkersFishing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FokkersFishing.Shared.Models;
using FokkersFishing.Data;

namespace FokkersFishing.Interfaces
{
    public interface IFokkersDbService
    {
        // Properties
        string StorageConnectionString { get; }

        // Catches
        Task<CatchData> GetItemAsync(string rowKey);
        Task<List<CatchData>> GetUserItemsAsync(string userName);
        Task AddItemAsync(CatchData item);
        Task UpdateItemAsync(CatchData item);
        Task DeleteItemAsync(string rowKey);
        Task<int> GetCatchNumberCount(string userEmail);
        Task<int> GetGlobalCatchNumberCount();

        // Fish
        Task<IEnumerable<FishData>> GetFishAsync();

        // Fishermen
        Task<IEnumerable<FisherMan>> GetFishermenAsync(ApplicationDbContext context);

        // Leaderboard
        Task<List<CatchData>> GetLeaderboardItemsAsync();
        Task<List<Catch>> GetTopCatchAsync(string fish);
    } // end i
} // end ns
