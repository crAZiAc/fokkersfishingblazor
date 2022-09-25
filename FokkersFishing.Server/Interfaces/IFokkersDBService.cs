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
        Task<CatchData> GetUserItemAsync(string rowKey, string userName);
        Task AddItemAsync(CatchData item);
        Task UpdateItemAsync(CatchData item);
        Task DeleteItemAsync(string rowKey);

        Task<List<CatchData>> GetUserItemsAsync(string userName);
        Task<List<CatchData>> GetAllCatches();
        Task<List<CatchData>> GetPendingCatches();
        Task<int> GetCatchNumberCount(string userEmail);
        Task<int> GetGlobalCatchNumberCount();

        // Fish
        Task<List<FishData>> GetFishAsync();

        // Fishermen
        Task<List<FisherMan>> GetFishermenAsync();
        Task<List<FisherMan>> GetFishermenCompetitionAsync(Guid competitionId);

        // Leaderboard
        Task<List<CatchData>> GetLeaderboardItemsAsync();
        Task<List<CatchData>> GetCompetitionLeaderboardItemsAsync(Guid competitionId);
        Task<List<CatchData>> GetAdminCompetitionLeaderboardItemsAsync(Guid competitionId);
        Task<List<Catch>> GetTopCatchAsync(string fish);
        Task<List<Catch>> GetTopCatchesByUserInCompetitionAsync(Guid competitionId, string userEmail, string fishName);
        Task<List<CatchData>> GetCatchesByUserInCompetitionAsync(Guid competitionId, string userEmail);
        Task<List<Catch>> GetBiggestFishPerUserInCompetition(Guid competitionId);

        // Competition
        Task<List<CompetitionData>> GetCompetitionsAsync();
        Task<List<CatchData>> GetCatchesInCompetitionsAsync(Guid competitionId);
        Task<CompetitionData> GetCompetitionAsync(string rowKey);
        Task AddCompetitionAsync(CompetitionData item);
        Task DeleteCompetitionAsync(string rowKey);
        Task UpdateCompetitionAsync(CompetitionData item);

        // Team
        Task<List<TeamData>> GetTeamsAsync();
        Task<TeamData> GetTeamAsync(string rowKey);
        Task AddTeamAsync(TeamData item);
        Task DeleteTeamAsync(string rowKey);
        Task UpdateTeamAsync(TeamData item);
        

        // Team Members
        Task<List<TeamMemberData>> GetTeamMembersAsync();
        Task<List<TeamMemberData>> GetTeamMembersAsync(Guid teamId);
        Task<List<TeamMemberData>> GetTeamMembersFromMemberAsync(string userEmail);
        Task<TeamMemberData> GetTeamMemberAsync(string rowKey);
        Task AddTeamMemberAsync(TeamMemberData item);
        Task DeleteTeamMemberAsync(string rowKey);
        Task DeleteTeamMembersAsync(Guid teamId);
        Task UpdateTeamAsync(TeamMemberData item);
        

    } // end i
} // end ns
