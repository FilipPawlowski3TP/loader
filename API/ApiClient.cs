using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using SecureLoader.Models;
using SecureLoader.Security;

namespace SecureLoader.API
{
    public class ApiClient
    {
        private static readonly Lazy<ApiClient> _instance = new Lazy<ApiClient>(() => new ApiClient());
        public static ApiClient Instance => _instance.Value;

        private readonly HttpClient _httpClient;
        
        // This would be your real backend URL
        private const string BaseUrl = "https://api.secure-loader-demo.com/";

        private ApiClient()
        {
            var handler = new HttpClientHandler
            {
                // Attach the certificate pinning callback
                ServerCertificateCustomValidationCallback = CertificatePinningService.ValidateCertificate
            };

            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(BaseUrl),
                Timeout = TimeSpan.FromSeconds(15)
            };

            // Common headers
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "SecureLoader-Client/1.0");
        }

        public async Task<AuthResponse?> AuthenticateAsync(AuthRequest request)
        {
            try
            {
                // Anti-debug check before critical API call
                AntiDebugService.CheckSecurity();

                var response = await _httpClient.PostAsJsonAsync("v1/auth/login", request);
                
                if (response.IsSuccessStatusCode)
                {
                    var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
                    
                    if (authResponse != null)
                    {
                        // VERIFY SIGNATURE BEFORE RETURNING
                        // The server should sign (status + expires_at + user_id)
                        string dataToVerify = $"{authResponse.Status}|{authResponse.ExpiresAt}|{authResponse.UserId}";
                        
                        if (!RSAService.VerifySignature(dataToVerify, authResponse.Signature))
                        {
                            // If signature is invalid, immediately terminate as per requirements
                            AntiDebugService.Terminate("Server response signature verification failed! Possible man-in-the-middle attack.");
                            return null;
                        }
                    }
                    
                    return authResponse;
                }
            }
            catch (Exception ex)
            {
                // Basic logging
                Debug.WriteLine($"[API Error] {ex.Message}");
            }
            
            return null;
        }

        public async Task<bool> SendHeartbeatAsync(string userId, string hwid)
        {
            try
            {
                // Anti-debug check during heartbeat
                AntiDebugService.CheckSecurity();

                var payload = new { user_id = userId, hwid = hwid, timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() };
                var response = await _httpClient.PostAsJsonAsync("v1/auth/heartbeat", payload);
                
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
