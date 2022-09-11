using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Newtonsoft.Json;

namespace FokkersFishing.Shared.Models
{
    public class Competition
    {
        private DateTime m_StartDate;
        private DateTime m_EndDate;

        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; }

        [JsonProperty(PropertyName = "competitionName")]
        public String CompetitionName { get; set; }

        [JsonProperty(PropertyName = "active")]
        public bool Active { get; set; }

        [JsonProperty(PropertyName = "startDate")]
        public DateTime StartDate
        {
            get
            {
                return m_StartDate.ToLocalTime();
            }
            set
            {
                m_StartDate = value;
            }
        }

        [JsonProperty(PropertyName = "endDate")]
        public DateTime EndDate
        {
            get
            {
                return m_EndDate.ToLocalTime();
            }
            set
            {
                m_EndDate = value;
            }
        }
    } // end c
} // end ns
