using System;
using System.Collections.ObjectModel;
using System.Windows.Media.Media3D;

using HelixToolkit.Wpf;

using RobotEditor.Helper;
using RobotEditor.Model;
using System.Collections.Generic;

namespace RobotEditor.ViewModel
{
    internal class ResultViewModel : BaseViewModel
    {
        #region Fields

        private readonly IHelixViewport3D _viewportResult;
        private BoothViewModel _selectedBooth;
        private bool _isBusy;

        #endregion

        #region Instance

        public ResultViewModel(HelixViewport3D viewportResult, VoxelOctree mixedCars, List<VoxelOctree> robots)
        {
            Start = new DelegateCommand<object>(StartExecute, StartCanExecute);
            FitToView = new DelegateCommand<object>(FitToViewExecute, FitToViewCanExecute);

            _viewportResult = viewportResult;
            BoothModels.Add(new DefaultLights());
            BoothModels.Add(new CoordinateSystemVisual3D { ArrowLengths = 100.0 });

            foreach (var robot in robots)
                Booths.Add(new BoothViewModel(new Booth("Puma 560", 0.0, 0.0, mixedCars, robot)));
        }

        #endregion

        #region Properties

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                if (_isBusy == value)
                    return;

                _isBusy = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<BoothViewModel> Booths { get; } = new ObservableCollection<BoothViewModel>();
        public ObservableCollection<Visual3D> BoothModels { get; } = new ObservableCollection<Visual3D>();

        public DelegateCommand<object> Start { get; }
        public DelegateCommand<object> FitToView { get; }

        public BoothViewModel SelectedBooth
        {
            get { return _selectedBooth; }
            set
            {
                if (Equals(value, _selectedBooth))
                    return;

                if (_selectedBooth != null)
                {
                    _selectedBooth.Model.Hide3DBooth();
                    BoothModels.Remove(_selectedBooth.BoothModel);
                }

                _selectedBooth = value;

                if (_selectedBooth != null)
                {
                    _selectedBooth.Model.Show3DBooth();
                    BoothModels.Add(_selectedBooth.BoothModel);
                }

                RaisePropertyChanged();

                _viewportResult.ZoomExtents(0);
            }
        }

        #endregion

        #region Private methods

        private bool StartCanExecute(object arg)
        {
            return true;
        }

        private void StartExecute(object obj)
        {
            ;
        }

        private bool FitToViewCanExecute(object arg)
        {
            return true;
        }

        private void FitToViewExecute(object obj)
        {
            _viewportResult.ZoomExtents(0);
        }

        private void Show3DResult()
        {
            

        }

        #endregion
    }
}