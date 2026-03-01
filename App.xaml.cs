using System.Configuration;
using System.Data;
using System.Windows;

namespace SecureLoader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Immediate security check before any UI is shown
            Security.AntiDebugService.CheckSecurity();
            base.OnStartup(e);
        }
    }

}
