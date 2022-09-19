using Azure.Data.Tables;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FokkersFishing.Shared.Models
{
    public class TeamScore
    {
        public string Fish { get; set; }
        public double TotalLength { get; set; }
        public int FishCount { get; set; }
        public string TeamName { get; set; }
        public int Ranking { get; set; }


    } // end c
} // end ns
