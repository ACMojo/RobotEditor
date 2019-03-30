using System.ComponentModel;
using System.Windows;
using System.Windows.Documents;

using RobotEditor.ViewModel;

namespace RobotEditor.View
{
    /// <summary>
    ///     Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Fields

        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly UiElementAdorner<BusyIndicator> _busyAdorner;

        #endregion

        #region Instance

        public MainWindow()
        {
            InitializeComponent();

            _mainWindowViewModel = new MainWindowViewModel(view_Carbody, view_Robot);

            DataContext = _mainWindowViewModel;

            _busyAdorner = new UiElementAdorner<BusyIndicator>(mainGrid) { Child = new BusyIndicator(), Visibility = Visibility.Collapsed };
            AdornerLayer.GetAdornerLayer(mainGrid).Add(_busyAdorner);

            _mainWindowViewModel.PropertyChanged += MainWindowViewModel_PropertyChanged;
        }

        #endregion

        #region Private methods

        private void MainWindowViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(MainWindowViewModel.IsBusy))
                return;

            _busyAdorner.Visibility = _mainWindowViewModel.IsBusy ? Visibility.Visible : Visibility.Collapsed;
            mainGrid.IsEnabled = !_mainWindowViewModel.IsBusy;
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_busyAdorner == null)
                return;

            _busyAdorner.Child.Width = ActualWidth;
            _busyAdorner.Child.Height = ActualHeight;
        }

        #endregion
    }
}