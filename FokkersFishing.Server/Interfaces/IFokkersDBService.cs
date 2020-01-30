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
        Task<IEnumerable<Catch>> GetItemsAsync(string query);
        Task<Catch> GetItemAsync(string id);
        Task AddItemAsync(Catch item);
        Task UpdateItemAsync(string id, Catch item);
        Task DeleteItemAsync(string id);
        Task<int> GetCatchNumberCount();
        Task<int> GetGlobalCatchNumberCount();
    } // end i
} // end ns
