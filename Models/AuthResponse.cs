using System.Text.Json.Serialization;

namespace SecureLoader.Models
{
    public class AuthResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("expires_at")]
        public string ExpiresAt { get; set; } = string.Empty;

        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("signature")]
        public string Signature { get; set; } = string.Empty;
    }
}
