using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FokkersFishing.Shared.Models
{
    public class CompetitionStats
    {
        [JsonProperty(PropertyName = "fishCaught")]
        public int FishCaught { get; set; }
        [JsonProperty(PropertyName = "totalLength")]
        public double TotalLength { get; set; }
    } // end c
} // end ns
