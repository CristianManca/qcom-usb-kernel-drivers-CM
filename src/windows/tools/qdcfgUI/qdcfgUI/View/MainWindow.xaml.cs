using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using QCDevice;
using QCDriverConstant;

namespace qdcfgUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }

        public void BringToForeground()
        {
            if (WindowState == WindowState.Minimized || Visibility == Visibility.Hidden)
            {
                Show();
                WindowState = WindowState.Normal;
            }
            Activate();
            Topmost = true;
            Topmost = false;
            Focus();
        }

        public void OnDeviceSelected(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // TODO: move this method to MainWindowViewModel?
            ((MainWindowViewModel)DataContext).OnDeviceSelected((LogDevice)e.NewValue);
        }

        public void OnDebugMaskChecked(object sender, RoutedEventArgs e)
        {
            // TODO: move this method to MainWindowViewModel?
            ((MainWindowViewModel)DataContext).OnMaskChecked(((CheckBox)sender).DataContext as WPPConstantItem, true);
        }

        public void OnDebugMaskUnchecked(object sender, RoutedEventArgs e)
        {
            // TODO: move this method to MainWindowViewModel?
            ((MainWindowViewModel)DataContext).OnMaskChecked(((CheckBox)sender).DataContext as WPPConstantItem, false);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            // Disable gpu hardware acceleration
            HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            if (hwndSource != null)
            {
                hwndSource.CompositionTarget.RenderMode = RenderMode.SoftwareOnly;
            }
            base.OnSourceInitialized(e);
        }
    }
}
