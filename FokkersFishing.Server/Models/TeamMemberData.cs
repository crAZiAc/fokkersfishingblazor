using Azure.Data.Tables;
using FokkersFishing.Shared.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FokkersFishing.Models
{
    public class TeamMemberData: BaseEntity, ITableEntity
    {
        public string UserEmail { get; set; }
        public Guid TeamId { get; set; }

        public Guid Id
        {
            get
            {
                return Guid.Parse(this.RowKey);
            }
        }
        public TeamMemberData()
        {
            PartitionKey = "TeamMember";
        }
    } // end c
} // end ns
