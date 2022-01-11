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

        [JsonProperty(PropertyName = "catchNumber")]
        public int CatchNumber { get; set; }

        [JsonProperty(PropertyName = "userEmail")]
        public string UserEmail { get; set; }

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

    } // end c
} // end ns
