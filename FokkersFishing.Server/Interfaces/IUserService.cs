using FokkersFishing.Shared.Models;
using System.Threading.Tasks;

namespace FokkersFishing.Server.Interfaces
{
    public interface IUserService
    {
        Task<bool> Authenticate(string username, string key);
    } //end i
} // end ns
