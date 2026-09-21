using System;
using FokkersFishing.Api.Models.Dtos;

namespace FokkersFishing.Api.Models.Entities
{
    public class TeamData : BaseEntity
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
    }
}
