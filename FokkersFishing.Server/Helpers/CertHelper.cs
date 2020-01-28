using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace FokkersFishing.Helpers
{
    public class CertHelper
    {
        public static X509Certificate2 GetCert()
        {
            X509Store certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            X509Certificate2 cert;
            certStore.Open(OpenFlags.ReadOnly);
            X509Certificate2Collection certCollection = certStore.Certificates.Find(
                                        X509FindType.FindByThumbprint,
                                        // Replace below with your certificate's thumbprint
                                        "3EE887D73B5504C7DFD149330439EB7A9E4DBF83",
                                        false);
            // Get the first cert with the thumbprint
            if (certCollection.Count > 0)
            {
                cert = certCollection[0];
                // Use certificate
                certStore.Close();
                return cert;
            }
            else
            {
                certStore.Close();
                return null;
            }
        }
    } // end c
} // end ns
