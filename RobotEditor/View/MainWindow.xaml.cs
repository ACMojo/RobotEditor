using RobotEditor.ViewModel;

namespace RobotEditor.View
{
    /// <summary>
    ///     Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Instance

        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainWindowViewModel(view_Carbody, view_Robot);
        }

        #endregion
    }
}