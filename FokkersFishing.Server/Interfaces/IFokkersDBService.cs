using FokkersFishing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FokkersFishing.Shared.Models;

namespace FokkersFishing.Interfaces
{
    public interface IFokkersDbService
    {
        // Catches
        Task<Catch> GetItemAsync(string rowKey);
        Task<List<Catch>> GetItemsAsync(string filter);
        Task AddItemAsync(Catch item);
        Task UpdateItemAsync(Catch item);
        Task DeleteItemAsync(string rowKey);
        Task<int> GetCatchNumberCount(string userEmail);
        Task<int> GetGlobalCatchNumberCount();

        // Fish
        Task<IEnumerable<Fish>> GetFishAsync();

        // Fishermen
        Task<IEnumerable<FisherMan>> GetFishermenAsync();
    } // end i
} // end ns
