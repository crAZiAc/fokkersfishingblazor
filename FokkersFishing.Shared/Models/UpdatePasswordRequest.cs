using System;
using System.Collections.Generic;
using System.Text;

namespace FokkersFishing.Shared.Models
{
    public class UpdatePasswordRequest
    {
        public string UserEmail { get; set; }
        public string NewPassword { get; set; }
    } // end c
} // end ns
