using RobotEditor.ViewModel;

namespace RobotEditor.View
{
    /// <summary>
    ///     Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class ResultWindow
    {
        #region Instance

        public ResultWindow()
        {
            InitializeComponent();

            DataContext = new ResultViewModel(view_Result);
        }

        #endregion
    }
}