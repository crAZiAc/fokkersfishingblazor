using System;
using System.Collections.Generic;
using System.Text;

namespace FokkersFishing.Shared.Models
{
    public class Photo
    {
        public Guid Id { get; set; }
        public byte[] ImageContent { get; set; }
        public PhotoTypeEnum PhotoType { get; set; }
    } // end c

    public enum PhotoTypeEnum
    {
        Catch,
        Measure
    }
} // end ns
