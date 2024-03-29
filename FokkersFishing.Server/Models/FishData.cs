﻿using Azure.Data.Tables;
using FokkersFishing.Shared.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FokkersFishing.Models
{
    public class FishData: BaseEntity, ITableEntity
    {
        public string Name { get; set; }
        public string GenericName { get; set; }
        public string Kind { get; set; }
        public bool Predator { get; set; }

        public bool IncludeInCompetition { get; set; }
        public FishData()
        {
            PartitionKey = "Fish";
        }
        public Fish GetFish()
        {
            return new Fish
            {
                Id = Guid.Parse(this.RowKey),
                Kind = this.Kind,
                Name = this.Name,
                GenericName = this.GenericName,
                IncludeInCompetition = this.IncludeInCompetition,
                Predator = this.Predator
            };
        }
    } // end c
} // end ns
