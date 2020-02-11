using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FokkersFishing.Shared.Models
{
    public class Catch
    {
        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; }

        [JsonProperty(PropertyName = "catchNumber")]
        public int CatchNumber { get; set; }

        [JsonProperty(PropertyName = "userName")]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "fish")]
        public string Fish { get; set; }

        [JsonProperty(PropertyName = "length")]
        public double Length { get; set; }

        [JsonProperty(PropertyName = "catchDate")]
        public DateTime CatchDate
        {
            get; set;
        }

        [JsonProperty(PropertyName = "logDate")]
        public DateTime LogDate { get; set; }

        [JsonProperty(PropertyName = "editDate")]
        public DateTime EditDate { get; set; }

        [JsonProperty(PropertyName = "globalCatchNumber")]
        public int GlobalCatchNumber { get; set; }

    } // end c
} // end ns
