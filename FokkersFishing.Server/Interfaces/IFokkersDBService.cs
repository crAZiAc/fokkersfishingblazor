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
        Task<IEnumerable<Catch>> GetItemsAsync(string queryString);
        Task<Catch> GetItemAsync(string id);
        Task AddItemAsync(Catch item);
        Task UpdateItemAsync(string id, Catch item);
        Task DeleteItemAsync(string id);
        Task<int> GetCatchNumberCount(string userEmail);
        Task<int> GetGlobalCatchNumberCount();

        // Fish
        Task<IEnumerable<Fish>> GetFishAsync(string queryString);

        // Fishermen
        Task<IEnumerable<FisherMan>> GetFishermenAsync(string queryString);
    } // end i
} // end ns
