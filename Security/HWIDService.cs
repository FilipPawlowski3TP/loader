using System;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Net.NetworkInformation;

namespace SecureLoader.Security
{
    public static class HWIDService
    {
        public static string GenerateHWID()
        {
            try
            {
                string cpuId = GetCpuId();
                string diskId = GetDiskId();
                string macAddress = GetMacAddress();

                string rawId = $"CPU:{cpuId}|DISK:{diskId}|MAC:{macAddress}";
                return ComputeSha256Hash(rawId);
            }
            catch (Exception)
            {
                return "UNKNOWN-HWID-ERROR";
            }
        }

        private static string GetCpuId()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor");
                foreach (var obj in searcher.Get())
                {
                    return obj["ProcessorId"]?.ToString() ?? "CPU-ID-EMPTY";
                }
            }
            catch { }
            return "CPU-ID-UNAVAILABLE";
        }

        private static string GetDiskId()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_DiskDrive");
                foreach (var obj in searcher.Get())
                {
                    var serial = obj["SerialNumber"]?.ToString()?.Trim();
                    if (!string.IsNullOrEmpty(serial)) return serial;
                }
            }
            catch { }
            return "DISK-ID-UNAVAILABLE";
        }

        private static string GetMacAddress()
        {
            try
            {
                var ni = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(n => n.OperationalStatus == OperationalStatus.Up && n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    .OrderByDescending(n => n.Speed)
                    .FirstOrDefault();

                return ni?.GetPhysicalAddress().ToString() ?? "MAC-ID-EMPTY";
            }
            catch { }
            return "MAC-ID-UNAVAILABLE";
        }

        private static string ComputeSha256Hash(string rawData)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData));
            StringBuilder builder = new StringBuilder();
            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString().ToUpper();
        }
    }
}
