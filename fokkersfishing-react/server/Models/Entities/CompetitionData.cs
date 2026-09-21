using System;
using FokkersFishing.Api.Models.Dtos;

namespace FokkersFishing.Api.Models.Entities
{
    public class CompetitionData : BaseEntity
    {
        public string Name { get; set; }
        public bool Active { get; set; }
        public bool ShowLeaderboardAfterCompetitionEnds { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public CompetitionData()
        {
            PartitionKey = "Competition";
        }

        public Competition GetCompetition()
        {
            return new Competition
            {
                Id = Guid.Parse(this.RowKey),
                StartDate = this.StartDate.ToLocalTime(),
                EndDate = this.EndDate.ToLocalTime(),
                CompetitionName = this.Name,
                Active = this.Active,
                ShowLeaderboardAfterCompetitionEnds = this.ShowLeaderboardAfterCompetitionEnds
            };
        }
    }
}
