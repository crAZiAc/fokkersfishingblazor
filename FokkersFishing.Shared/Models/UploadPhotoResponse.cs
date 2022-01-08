using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace FokkersFishing.Shared.Models
{
    public class UploadPhotoResponse
    {
        [JsonPropertyName("photoUrl")]
        public string PhotoUrl { get; set; }
        [JsonPropertyName("thumbnailUrl")]
        public string ThumbnailUrl { get; set; }
    }
}
