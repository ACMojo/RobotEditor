using System.Windows;

using RobotEditor.View;
using RobotEditor.ViewModel;

using VirtualRobotWrapperLib.Wcf;

namespace RobotEditor
{
    /// <summary>
    ///     Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App
    {
        #region Fields

        private MainWindowViewModel _mainWindowViewModel;

        #endregion

        #region Instance

        static App()
        {
            WindowsJobObject.Instance = new WindowsJobObject();
        }

        #endregion

        #region Protected methods

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainWindow = new MainWindow();
            _mainWindowViewModel = mainWindow.DataContext as MainWindowViewModel;
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            try
            {
                _mainWindowViewModel?.Dispose();
            }
            catch
            {
                // ignored
            }
        }

        #endregion
    }
}