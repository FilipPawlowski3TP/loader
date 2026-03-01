using System.Windows;
using System.Windows.Input;

namespace SecureLoader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            
            // Allow dragging the window since WindowStyle="None"
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }
    }
}