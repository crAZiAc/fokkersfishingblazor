using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Newtonsoft.Json;

namespace FokkersFishing.Shared.Models
{
    public class Catch
    {
        private DateTime m_catchDate;

        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; }

        [JsonProperty(PropertyName = "competitionId")]
        public Guid CompetitionId { get; set; }

        [JsonProperty(PropertyName = "catchNumber")]
        public int CatchNumber { get; set; }

        [JsonProperty(PropertyName = "userEmail")]
        public string UserEmail { get; set; }

        public string UserName { get; set; }

        [JsonProperty(PropertyName = "registerUserEmail")]
        public string RegisterUserEmail { get; set; }

        public string RegisterUserName { get; set; }

        [JsonProperty(PropertyName = "fish")]
        public string Fish { get; set; }

        [JsonProperty(PropertyName = "length")]
        public double Length { get; set; }

        [JsonProperty(PropertyName = "catchDate")]
        public DateTime CatchDate
        {
            get
            {
                return m_catchDate.ToLocalTime();
            }
            set
            {
                m_catchDate = value;
            }
        }

        // This is a non-persistent property
        [JsonProperty(PropertyName = "teamName")]
        public string TeamName { get; set; }

        [JsonProperty(PropertyName = "logDate")]
        public DateTime LogDate { get; set; }

        [JsonProperty(PropertyName = "editDate")]
        public DateTime EditDate { get; set; }

        [JsonProperty(PropertyName = "globalCatchNumber")]
        public int GlobalCatchNumber { get; set; }

        [JsonProperty(PropertyName = "measurePhotoUrl")]
        public string MeasurePhotoUrl { get; set; }

        [JsonProperty(PropertyName = "catchPhotoUrl")]
        public string CatchPhotoUrl { get; set; }

        [JsonProperty(PropertyName = "measureThumbnailUrl")]
        public string MeasureThumbnailUrl { get; set; }

        [JsonProperty(PropertyName = "catchThumbnailUrl")]
        public string CatchThumbnailUrl { get; set; }

        [JsonProperty(PropertyName = "status")]
        public CatchStatusEnum Status { get; set; }

        public bool CaughtInCompetition
        {
            get
            {
                if (this.CompetitionId != Guid.Parse("00000000-0000-0000-0000-000000000000"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    } // end c
} // end ns
