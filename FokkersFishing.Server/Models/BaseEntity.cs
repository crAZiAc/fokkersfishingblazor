using Azure;
using Azure.Data.Tables;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FokkersFishing.Models
{
    public abstract class BaseEntity: ITableEntity
    {
        [JsonProperty(PropertyName = "partitionKey")]
        public string PartitionKey { get; set; }
        [JsonProperty(PropertyName = "rowKey")]
        public string RowKey { get; set; }
        [JsonProperty(PropertyName = "timeStamp")]
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    } // end class
} // end nsS
