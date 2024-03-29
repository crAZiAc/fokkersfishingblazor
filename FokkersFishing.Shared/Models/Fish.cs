﻿using Azure.Data.Tables;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FokkersFishing.Shared.Models
{
    public class Fish
    {
        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; }
        [JsonProperty(PropertyName = "name")]

        public string Name { get; set; }
        
        [JsonProperty(PropertyName = "genericName")]
        public string GenericName { get; set; }

        [JsonProperty(PropertyName = "kind")]
        public string Kind { get; set; }

        [JsonProperty(PropertyName = "predator")]
        public bool Predator { get; set; }

        [JsonProperty(PropertyName = "includeInCompetition")]
        public bool IncludeInCompetition { get; set; }
    } // end c
} // end ns
