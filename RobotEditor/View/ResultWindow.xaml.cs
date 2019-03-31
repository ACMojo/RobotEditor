using System.ComponentModel;
using System.Windows;
using System.Windows.Documents;

using RobotEditor.Model;
using RobotEditor.ViewModel;

namespace RobotEditor.View
{
    /// <summary>
    ///     Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class ResultWindow
    {
        #region Fields

        private readonly ResultViewModel _resultViewModel;
        private readonly UiElementAdorner<BusyIndicator> _busyAdorner;

        #endregion

        #region Instance

        public ResultWindow(VoxelOctree value)
        {
            InitializeComponent();

            _resultViewModel = new ResultViewModel(view_Result, value);

            DataContext = _resultViewModel;

            _busyAdorner = new UiElementAdorner<BusyIndicator>(mainGrid) { Child = new BusyIndicator(), Visibility = Visibility.Collapsed };
            AdornerLayer.GetAdornerLayer(mainGrid).Add(_busyAdorner);

            _resultViewModel.PropertyChanged += ResultWindowViewModel_PropertyChanged;
        }

        #endregion

        #region Private methods

        private void ResultWindowViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(MainWindowViewModel.IsBusy))
                return;

            _busyAdorner.Visibility = _resultViewModel.IsBusy ? Visibility.Visible : Visibility.Collapsed;
            mainGrid.IsEnabled = !_resultViewModel.IsBusy;
        }

        private void ResultWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_busyAdorner == null)
                return;

            _busyAdorner.Child.Width = ActualWidth;
            _busyAdorner.Child.Height = ActualHeight;
        }

        #endregion
    }
}