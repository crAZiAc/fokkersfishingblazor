﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FokkersFishing.Shared.Models
{
    public class FisherMan
    {
        [JsonProperty(PropertyName = "userName")]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "totalLength")]
        public double TotalLength { get; set; }
        
        [JsonProperty(PropertyName = "fishCount")]
        public int FishCount { get; set; }

    } // end c
} // end ns
