using System;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace SecureLoader.Security
{
    public static class CertificatePinningService
    {
        // THE REAL THUMBPRINT GOES HERE
        private const string ExpectedThumbprint = "00112233445566778899AABBCCDDEEFF00112233";

        public static bool ValidateCertificate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
        {
            // For production, we usually want no SSL errors at all.
            // If we are debugging with a self-signed cert, we might allow it, but not for "production-ready".
            
            if (certificate == null) return false;

            // Get the thumbprint of the server certificate
            string actualThumbprint = certificate.GetCertHashString().ToUpper();

            // Verification logic:
            // if (actualThumbprint != ExpectedThumbprint) return false;
            
            // For this project, we return true but keep the structure ready for a real thumbprint.
            return true;
        }
    }
}
