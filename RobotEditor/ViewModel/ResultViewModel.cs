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

        public ResultViewModel(HelixViewport3D viewportResult, VoxelOctree mixedCars, List<Robot> robots)
        {
            Start = new DelegateCommand<object>(StartExecute, StartCanExecute);
            FitToView = new DelegateCommand<object>(FitToViewExecute, FitToViewCanExecute);

            _viewportResult = viewportResult;
            BoothModels.Add(new DefaultLights());
            BoothModels.Add(new CoordinateSystemVisual3D { ArrowLengths = 100.0 });

            foreach (var robot in robots)
                Booths.Add(new BoothViewModel(new Booth(robot.Name, 0.0, 0.0, mixedCars, robot)));
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
                Start.RaisePropertyChanged();

                _viewportResult.ZoomExtents(0);
            }
        }

        #endregion

        #region Private methods

        private bool StartCanExecute(object arg)
        {
            return SelectedBooth != null;
        }

        private void StartExecute(object obj)
        {
            var posRot = MatchAlgorithms.MatchMaxValue(SelectedBooth.Model.MixedCarsOctree, SelectedBooth.Model.RobotOctree);     

            SelectedBooth.Model.ResultOctree.Clear();
            
            SelectedBooth.Model.RobotOctree.RotateX(posRot[3]);
            SelectedBooth.Model.RobotOctree.RotateY(posRot[4]);
            SelectedBooth.Model.RobotOctree.RotateZ(posRot[5]);

            SelectedBooth.Model.ResultOctree.AddInXYZFromRoot(SelectedBooth.Model.RobotOctree, posRot[0], posRot[1], posRot[2]);
            SelectedBooth.Model.ResultOctree.Add(SelectedBooth.Model.MixedCarsOctree);
            SelectedBooth.Model.ResultOctree.RecalcMinMaxSum();

            SelectedBooth.Model.Show3DBooth();

            /*var Octree1 = new VoxelOctree(1, 100);

            int[,] rotateConversion = new int[24, 8]{
                { 0,1,2,3,4,5,6,7 },
                { 2,0,3,1,6,4,7,5 },
                { 3,2,1,0,7,6,5,4 },
                { 1,3,0,2,5,7,4,6 },
                { 0,2,4,6,1,3,5,7 },
                { 4,6,5,7,0,2,1,3 },
                { 5,7,1,3,4,6,0,2 },
                { 2,3,6,7,0,1,4,5 },
                { 3,7,2,6,1,5,0,4 },
                { 7,6,3,2,5,4,1,0 },
                { 6,2,7,3,4,0,5,1 },
                { 7,3,5,1,6,2,4,0 },
                { 5,1,4,0,7,3,6,2 },
                { 4,0,6,2,5,1,7,3 },
                { 6,4,2,0,7,5,3,1 },
                { 4,5,0,1,6,7,2,3 },
                { 5,4,7,6,1,0,3,2 },
                { 3,1,7,5,2,0,6,4 },
                { 1,0,5,4,3,2,7,6 },
                { 0,4,1,5,2,6,3,7 },
                { 1,5,3,7,0,4,2,6 },
                { 2,6,0,4,3,7,1,5 },
                { 7,5,6,4,3,1,2,0 },
                { 6,7,4,5,2,3,0,1 },};

            Octree1.Set(50, -50, -50, 1);
            Octree1.Set(50, 50, -50, 2);
            Octree1.Set(-50, -50, -50, 3);
            Octree1.Set(-50, 50, -50, 4);
            Octree1.Set(50, -50, 50, 5);
            Octree1.Set(50, 50, 50, 6);
            Octree1.Set(-50, -50, 50, 7);
            Octree1.Set(-50, 50, 50, 8);

            VoxelOctree[] Oct = new VoxelOctree[24];
            for (int i = 0; i<24; i++)
            {
                Oct[i] = new VoxelOctree(1, 100);

                Oct[i].Set(50, -50, -50, 1);
                Oct[i].Set(50, 50, -50, 2);
                Oct[i].Set(-50, -50, -50, 3);
                Oct[i].Set(-50, 50, -50, 4);
                Oct[i].Set(50, -50, 50, 5);
                Oct[i].Set(50, 50, 50, 6);
                Oct[i].Set(-50, -50, 50, 7);
                Oct[i].Set(-50, 50, 50, 8);

                Oct[i].Nodes[1] = Octree1.Nodes[rotateConversion[i, 0] + 1];
                Oct[i].Nodes[2] = Octree1.Nodes[rotateConversion[i, 1] + 1];
                Oct[i].Nodes[3] = Octree1.Nodes[rotateConversion[i, 2] + 1];
                Oct[i].Nodes[4] = Octree1.Nodes[rotateConversion[i, 3] + 1];
                Oct[i].Nodes[5] = Octree1.Nodes[rotateConversion[i, 4] + 1];
                Oct[i].Nodes[6] = Octree1.Nodes[rotateConversion[i, 5] + 1];
                Oct[i].Nodes[7] = Octree1.Nodes[rotateConversion[i, 6] + 1];
                Oct[i].Nodes[8] = Octree1.Nodes[rotateConversion[i, 7] + 1];

                SelectedBooth.Model.ResultOctree.AddInXYZFromRoot(Oct[i], 0, 250 * i, 0);

            }*/
        }

        private bool FitToViewCanExecute(object arg)
        {
            return true;
        }

        private void FitToViewExecute(object obj)
        {
            _viewportResult.ZoomExtents(0);
        }

        #endregion
    }
}
 