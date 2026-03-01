using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Runtime.InteropServices;

namespace SecureLoader.Security
{
    public static class AntiDebugService
    {
        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern bool IsDebuggerPresent();

        private static readonly string[] BadProcesses = 
        { 
            "x64dbg", "idag", "wireshark", "httpdebugger", "fiddler", "processhacker", "dnspy", "ollydbg", "charles"
        };

        public static void CheckSecurity()
        {
            if (Debugger.IsAttached || IsDebuggerPresent())
            {
                Terminate("Debugger detected! The application will now close.");
            }

            if (DetectAnalysisTools())
            {
                Terminate("Analysis or debugging tools detected! The application will now close.");
            }
        }

        private static bool DetectAnalysisTools()
        {
            try
            {
                var processes = Process.GetProcesses();
                foreach (var p in processes)
                {
                    try
                    {
                        string name = p.ProcessName.ToLower();
                        if (BadProcesses.Any(bad => name.Contains(bad)))
                        {
                            return true;
                        }
                    }
                    catch { /* Access denied or process exited */ }
                }
            }
            catch { }
            return false;
        }

        public static void Terminate(string message)
        {
            MessageBox.Show(message, "Security Violation", MessageBoxButton.OK, MessageBoxImage.Stop);
            Environment.Exit(0);
        }
    }
}
