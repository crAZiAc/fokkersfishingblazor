using System;
using System.Collections.Generic;
using System.Text;

namespace FokkersFishing.Shared.Models
{
    public class UserInfo
    {
        public bool IsAuthenticated { get; set; }

        public string Name { get; set; }

        public string Id { get; set; }

        public string IdentityProvider { get; set; }

        public string Email { get; set; }

    } // end c
} // end ns
