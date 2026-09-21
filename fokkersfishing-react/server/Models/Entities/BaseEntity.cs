using System;
using System.Text.Json.Serialization;
using Azure;
using Azure.Data.Tables;

namespace FokkersFishing.Api.Models.Entities
{
    /// <summary>
    /// Base Azure Table Storage entity. Mirrors the original Blazor app so the
    /// same tables (Catches, Fish, Competitions, Teams, TeamMembers) are read/written.
    /// </summary>
    public abstract class BaseEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }

        [JsonIgnore]
        public ETag ETag { get; set; }
    }
}
