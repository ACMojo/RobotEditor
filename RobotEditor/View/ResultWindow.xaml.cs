using RobotEditor.Model;
using RobotEditor.ViewModel;

namespace RobotEditor.View
{
    /// <summary>
    ///     Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class ResultWindow
    {
        #region Instance

        public ResultWindow(VoxelOctree value)
        {
            InitializeComponent();

            DataContext = new ResultViewModel(view_Result, value);
        }

        #endregion
    }
}