using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Newtonsoft.Json;

namespace FokkersFishing.Shared.Models
{
    public class FisherMan
    {
        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; }
        [JsonProperty(PropertyName = "userName")]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "totalLength")]
        public double TotalLength { get; set; }

        [JsonProperty(PropertyName = "fishCount")]
        public int FishCount { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string UserEmail { get; set; }

    } // end c
} // end ns
