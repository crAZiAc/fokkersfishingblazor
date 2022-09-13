using Azure.Data.Tables;
using FokkersFishing.Shared.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FokkersFishing.Models
{
    public class TeamData: BaseEntity, ITableEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public TeamData()
        {
            PartitionKey = "Team";
        }
        public Team GetTeam()
        {
            return new Team
            {
                Id = Guid.Parse(this.RowKey),
                Name = this.Name,
                Description = this.Description
            };
        }
    } // end c
} // end ns
