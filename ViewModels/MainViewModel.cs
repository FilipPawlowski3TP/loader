using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using SecureLoader.Utils;
using SecureLoader.Security;
using SecureLoader.API;
using SecureLoader.Models;

namespace SecureLoader.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private string _licenseKey = string.Empty;
        private string _status = "Ready";
        private string _expirationDate = "---";
        private string _hwid = "---";
        private bool _isLaunchEnabled = false;
        
        private string? _userId;
        private Process? _childProcess;
        private CancellationTokenSource? _heartbeatCts;

        public string LicenseKey
        {
            get => _licenseKey;
            set => SetProperty(ref _licenseKey, value);
        }

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public string ExpirationDate
        {
            get => _expirationDate;
            set => SetProperty(ref _expirationDate, value);
        }

        public string Hwid
        {
            get => _hwid;
            set => SetProperty(ref _hwid, value);
        }

        public bool IsLaunchEnabled
        {
            get => _isLaunchEnabled;
            set => SetProperty(ref _isLaunchEnabled, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand LaunchCommand { get; }

        public MainViewModel()
        {
            LoginCommand = new RelayCommand(async _ => await ExecuteLoginAsync(), _ => !string.IsNullOrEmpty(LicenseKey));
            LaunchCommand = new RelayCommand(_ => ExecuteLaunch(), _ => IsLaunchEnabled);
            
            LoadInitialData();
            
            // Start background security monitor
            MonitorSecurity();
        }

        private void LoadInitialData()
        {
            // Set HWID from service
            Hwid = HWIDService.GenerateHWID();
        }

        private async void MonitorSecurity()
        {
            while (true)
            {
                AntiDebugService.CheckSecurity();
                await Task.Delay(5000);
            }
        }

        private async Task ExecuteLoginAsync()
        {
            try 
            {
                Status = "Connecting to server...";
                
                var request = new AuthRequest
                {
                    LicenseKey = LicenseKey,
                    Hwid = Hwid,
                    AppVersion = "1.0.0"
                };

                // Perform authentication
                var response = await ApiClient.Instance.AuthenticateAsync(request);

                if (response != null && response.Status.ToLower() == "active")
                {
                    Status = "Access Granted";
                    ExpirationDate = response.ExpiresAt;
                    _userId = response.UserId;
                    IsLaunchEnabled = true;

                    // Start Heartbeat system
                    StartHeartbeat();
                }
                else
                {
                    Status = "Invalid License";
                    IsLaunchEnabled = false;
                    MessageBox.Show("The license key provided is invalid or expired.", "Authentication Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                Status = "Connection Error";
                MessageBox.Show($"Server connection failed: {ex.Message}", "API Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StartHeartbeat()
        {
            _heartbeatCts?.Cancel();
            _heartbeatCts = new CancellationTokenSource();
            CancellationToken token = _heartbeatCts.Token;

            Task.Run(async () =>
            {
                int failedCount = 0;
                while (!token.IsCancellationRequested)
                {
                    // Wait 30 seconds
                    await Task.Delay(30000, token);

                    if (token.IsCancellationRequested) break;

                    bool heartBeatOk = await ApiClient.Instance.SendHeartbeatAsync(_userId ?? "unknown", Hwid);
                    
                    if (heartBeatOk)
                    {
                        failedCount = 0;
                    }
                    else
                    {
                        failedCount++;
                        if (failedCount >= 3)
                        {
                            Application.Current.Dispatcher.Invoke(() => 
                            {
                                TerminateAll("Heartbeat failed 3 times. Connection to server lost.");
                            });
                            break;
                        }
                    }
                }
            }, token);
        }

        private void ExecuteLaunch()
        {
            if (!IsLaunchEnabled) return;

            try
            {
                // Final security check
                AntiDebugService.CheckSecurity();

                string exePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ClientApp", "kalkulator.exe");
                
                if (!System.IO.File.Exists(exePath))
                {
                    MessageBox.Show($"Executable not found: {exePath}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                ProcessStartInfo startInfo = new ProcessStartInfo(exePath)
                {
                    WorkingDirectory = System.IO.Path.GetDirectoryName(exePath)
                };

                _childProcess = Process.Start(startInfo);
                
                if (_childProcess != null)
                {
                    Status = "Running";
                    IsLaunchEnabled = false;

                    _childProcess.EnableRaisingEvents = true;
                    _childProcess.Exited += (s, e) =>
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            Status = "App Terminated";
                            IsLaunchEnabled = true;
                            _childProcess = null;
                        });
                    };
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not start the application: {ex.Message}", "Launch Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TerminateAll(string reason)
        {
            _heartbeatCts?.Cancel();

            if (_childProcess != null && !_childProcess.HasExited)
            {
                try { _childProcess.Kill(); } catch { }
            }

            AntiDebugService.Terminate(reason);
        }
    }
}
