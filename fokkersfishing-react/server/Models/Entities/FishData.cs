using System;
using FokkersFishing.Api.Models.Dtos;

namespace FokkersFishing.Api.Models.Entities
{
    public class FishData : BaseEntity
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
    }
}
