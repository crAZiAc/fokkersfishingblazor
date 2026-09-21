using System;
using System.Text.Json.Serialization;

namespace FokkersFishing.Api.Models.Dtos
{
    public class Competition
    {
        private DateTime m_StartDate;
        private DateTime m_EndDate;

        public Guid Id { get; set; }
        public string CompetitionName { get; set; }
        public bool Active { get; set; }
        public bool ShowLeaderboardAfterCompetitionEnds { get; set; }

        public DateTime StartDate
        {
            get => m_StartDate.ToLocalTime();
            set => m_StartDate = value;
        }

        public DateTime EndDate
        {
            get => m_EndDate.ToLocalTime();
            set => m_EndDate = value;
        }

        [JsonIgnore]
        public TimeSpan TimeTillEnd => m_EndDate - DateTime.Now;

        [JsonIgnore]
        public TimeSpan TimeTillStart => m_StartDate - DateTime.Now;

        [JsonIgnore]
        public bool CompetitionEnded => DateTime.Now > EndDate;

        [JsonIgnore]
        public bool CompetitionNotStarted => DateTime.Now < StartDate;
    }
}
