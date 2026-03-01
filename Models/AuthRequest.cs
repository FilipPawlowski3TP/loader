using System.Text.Json.Serialization;

namespace SecureLoader.Models
{
    public class AuthRequest
    {
        [JsonPropertyName("license_key")]
        public string LicenseKey { get; set; } = string.Empty;

        [JsonPropertyName("hwid")]
        public string Hwid { get; set; } = string.Empty;

        [JsonPropertyName("app_version")]
        public string AppVersion { get; set; } = string.Empty;
    }
}
