using System;
using System.Text.Json.Serialization;

namespace FokkersFishing.Api.Models.Entities
{
    public class TeamMemberData : BaseEntity
    {
        public string UserEmail { get; set; }
        public Guid TeamId { get; set; }

        [JsonIgnore]
        public Guid Id => Guid.Parse(this.RowKey);

        public TeamMemberData()
        {
            PartitionKey = "TeamMember";
        }
    }
}
