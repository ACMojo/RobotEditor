using System;
using System.Collections.ObjectModel;
using System.Windows.Media.Media3D;

using HelixToolkit.Wpf;

using RobotEditor.Helper;
using RobotEditor.Model;
using System.Collections.Generic;
using System.Diagnostics;

namespace RobotEditor.ViewModel
{
    internal class ResultViewModel : BaseViewModel
    {
        #region Fields

        private readonly IHelixViewport3D _viewportResult;
        private BoothViewModel _selectedBooth;
        private bool _isBusy;
        private bool _isCheckedMaxVoxel;
        private bool _isCheckedMaxLeaf;
        private bool _isCheckedMaxValue;
        private bool _isCheckedMaxLeafs;
        private bool _isCheckedMaxMax;
        private bool _isCheckedRotation;
        private bool _isCheckedNoGo;
        private bool _isCheckedSymmetry;
        private int _selectedShiftMethod = 1;
        private int _selectedSearchMethod = 0;
        private int _searchCycles = 0;
        private int _noOfRobots = 1;
        private double[] boundingBoxHalfExtents;

        #endregion

        #region Instance

        public ResultViewModel(HelixViewport3D viewportResult, VoxelOctree mixedCars, List<Robot> robots, double[] boundingBoxHalfExtents)
        {
            Start = new DelegateCommand<object>(StartExecute, StartCanExecute);
            FitToView = new DelegateCommand<object>(FitToViewExecute, FitToViewCanExecute);
            MaxVoxel = new DelegateCommand<object>(MaxVoxelExecute, MaxVoxelCanExecute);
            MaxLeaf = new DelegateCommand<object>(MaxLeafExecute, MaxLeafCanExecute);
            MaxValue = new DelegateCommand<object>(MaxValueExecute, MaxValueCanExecute);
            MaxLeafs= new DelegateCommand<object>(MaxLeafsExecute, MaxLeafsCanExecute);
            MaxMax = new DelegateCommand<object>(MaxMaxExecute, MaxMaxCanExecute);
            Rotation = new DelegateCommand<object>(MaxValueExecute, MaxValueCanExecute);
            NoGo = new DelegateCommand<object>(MaxLeafsExecute, MaxLeafsCanExecute);
            Symmetry = new DelegateCommand<object>(MaxMaxExecute, MaxMaxCanExecute);

            this.boundingBoxHalfExtents = boundingBoxHalfExtents;

            _viewportResult = viewportResult;
            BoothModels.Add(new DefaultLights());
            BoothModels.Add(new CoordinateSystemVisual3D { ArrowLengths = 100.0 });

            foreach (var robot in robots)
                Booths.Add(new BoothViewModel(new Booth(robot.Name, mixedCars, robot)));
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

        public bool IsCheckedMaxVoxel
        {
            get { return _isCheckedMaxVoxel; }
            set
            {
                _isCheckedMaxVoxel = value;
                RaisePropertyChanged();
            }
        }

        public bool IsCheckedMaxLeaf
        {
            get { return _isCheckedMaxLeaf; }
            set
            {
                _isCheckedMaxLeaf = value;
                RaisePropertyChanged();
            }
        }

        public bool IsCheckedMaxValue
        {
            get { return _isCheckedMaxValue; }
            set
            {
                _isCheckedMaxValue = value;
                RaisePropertyChanged();
            }
        }

        public bool IsCheckedMaxLeafs
        {
            get { return _isCheckedMaxLeafs; }
            set
            {
                _isCheckedMaxLeafs = value;
                RaisePropertyChanged();
            }
        }

        public bool IsCheckedMaxMax
        {
            get { return _isCheckedMaxMax; }
            set
            {
                _isCheckedMaxMax = value;
                RaisePropertyChanged();
            }
        }

        public bool IsCheckedRotation
        {
            get { return _isCheckedRotation; }
            set
            {
                _isCheckedRotation = value;
                RaisePropertyChanged();
            }
        }

        public bool IsCheckedNoGo
        {
            get { return _isCheckedNoGo; }
            set
            {
                _isCheckedNoGo = value;
                RaisePropertyChanged();
            }
        }

        public bool IsCheckedSymmetry
        {
            get { return _isCheckedSymmetry; }
            set
            {
                _isCheckedSymmetry = value;
                RaisePropertyChanged();
            }
        }

        public int SelectedItemShiftMethod
        {
            get { return _selectedShiftMethod; }
            set
            {
                _selectedShiftMethod = value;
                RaisePropertyChanged();
            }
        }

        public int SelectedItemSearchMethod
        {
            get { return _selectedSearchMethod; }
            set
            {
                _selectedSearchMethod = value;

                RaisePropertyChanged();
                MaxVoxel.RaisePropertyChanged();
                MaxLeaf.RaisePropertyChanged();
                MaxValue.RaisePropertyChanged();
                MaxLeafs.RaisePropertyChanged();
                MaxMax.RaisePropertyChanged();
            }
        }

        public int SearchCycles
        {
            get { return _searchCycles; }
            set
            {
                _searchCycles = value;

                RaisePropertyChanged();
            }
        }

        public int NoOfRobots
        {
            get { return _noOfRobots; }
            set
            {
                _noOfRobots = value;

                RaisePropertyChanged();
            }
        }

        public ObservableCollection<BoothViewModel> Booths { get; } = new ObservableCollection<BoothViewModel>();
        public ObservableCollection<Visual3D> BoothModels { get; } = new ObservableCollection<Visual3D>();

        public DelegateCommand<object> Start { get; }
        public DelegateCommand<object> FitToView { get; }
        public DelegateCommand<object> MaxVoxel { get; }
        public DelegateCommand<object> MaxLeaf { get; }
        public DelegateCommand<object> MaxValue { get; }
        public DelegateCommand<object> MaxLeafs { get; }
        public DelegateCommand<object> MaxMax { get; }
        public DelegateCommand<object> Rotation { get; }
        public DelegateCommand<object> NoGo { get; }
        public DelegateCommand<object> Symmetry { get; }

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

            #region TestData
            VoxelOctree testCarTree = new VoxelOctree(3, 100.0);

            for (int i = 0; i < 512; i++)
            {
                /*
                if ( (i >= 56 && i <= 63) || (i >= 112 && i <= 119) || (i >= 168 && i <= 175) || (i >= 224 && i <= 231) || (i >= 280 && i <= 287) || (i >= 336 && i <= 343) || (i >= 392 && i <= 393) || (i >= 448 && i <= 455))
                {
                    testCarTree.Set(testCarTree.StartIndexLeafNodes + i, 20);
                }
                else
                {
                    testCarTree.Set(testCarTree.StartIndexLeafNodes + i, Math.Ceiling((i + 1) / 64.0));
                }*/

                
                if ((i >= 448 && i <= 511))
                {
                    testCarTree.Set(testCarTree.StartIndexLeafNodes + i, 20);
                }
                else
                {
                    testCarTree.Set(testCarTree.StartIndexLeafNodes + i, Math.Ceiling((i + 1) / 64.0));
                }
                


                /*
                if ( (i >= 0 && i <= 511))
                {
                    testCarTree.Set(testCarTree.StartIndexLeafNodes + i, 20);
                }
                else
                {
                    testCarTree.Set(testCarTree.StartIndexLeafNodes + i, Math.Ceiling((i + 1) / 512.0));
                }*/
            }

            testCarTree.RecalcMinMaxSum();

            VoxelOctree testRobotTree = new VoxelOctree(2, 100.0);

            for (int i = 0; i < 64; i++)
                testRobotTree.Set(testRobotTree.StartIndexLeafNodes + i, Math.Ceiling((i+1) / 8.0));

            testRobotTree.RecalcMinMaxSum();

            #endregion
            SelectedBooth.Model.Hide3DBooth();
            SelectedBooth.Model.ResultOctree.Clear();
            SelectedBooth.Model.RenewMixedCarsCopy();

            for (int i = 0; i < NoOfRobots; i++)
            {
                var watch = new Stopwatch();
                watch.Start();
                var pos = MatchAlgorithms.BranchAndBound(SelectedBooth.Model.MixedCarsOctreeCopy, SelectedBooth.Model.RobotOctree, IsCheckedMaxVoxel, IsCheckedMaxLeaf, IsCheckedMaxValue, IsCheckedMaxLeafs, IsCheckedMaxMax, SelectedItemSearchMethod, SelectedItemShiftMethod, SearchCycles, IsCheckedRotation, IsCheckedNoGo, IsCheckedSymmetry, boundingBoxHalfExtents);
                watch.Stop();

                SelectedBooth.XPos = pos[0];
                SelectedBooth.YPos = pos[1];
                SelectedBooth.ZPos = pos[2];

                SelectedBooth.BestMatch += MatchAlgorithms.lowerBound;
                SelectedBooth.Cycles += MatchAlgorithms.cycles;
                SelectedBooth.ComputationTime += watch.Elapsed.TotalMilliseconds;

                SelectedBooth.Model.MixedCarsOctreeCopy.ClearInXYZFromRoot(MatchAlgorithms.rotatedRobotTrees[MatchAlgorithms.rotateOperator], pos[0], pos[1], pos[2]);
                SelectedBooth.Model.MixedCarsOctreeCopy.RecalcMinMaxSum();

                SelectedBooth.Model.ResultOctree.AddInXYZFromRoot(MatchAlgorithms.rotatedRobotTrees[MatchAlgorithms.rotateOperator], pos[0], pos[1], pos[2]);
            }
            
            SelectedBooth.LowerBound = MatchAlgorithms.initLowerBound;

            SelectedBooth.Model.ResultOctree.Add(SelectedBooth.Model.MixedCarsOctreeCopy);
            SelectedBooth.Model.ResultOctree.RecalcMinMaxSum();



            /*
            var rotatedTestRobots = MatrixHelper.allRotationsOfCube(testRobotTree);

            for (int i = 0; i < rotatedTestRobots.Length; i++)
            {
                SelectedBooth.Model.ResultOctree.AddInXYZFromRoot(rotatedTestRobots[i], 0, 800 * (i % 4), -800 * (int)Math.Floor(i / 4.0));
            }

            SelectedBooth.Model.ResultOctree.RecalcMinMaxSum();*/

            SelectedBooth.Model.Show3DBooth();

        }

        private bool FitToViewCanExecute(object arg)
        {
            return true;
        }

        private void FitToViewExecute(object obj)
        {
            _viewportResult.ZoomExtents(0);
        }

        private bool MaxVoxelCanExecute(object arg)
        {
            return SelectedItemSearchMethod != 2;
        }

        private void MaxVoxelExecute(object obj)
        {
            ;
        }

        private bool MaxLeafCanExecute(object arg)
        {
            return SelectedItemSearchMethod != 2;
        }

        private void MaxLeafExecute(object obj)
        {
            ;
        }

        private bool MaxValueCanExecute(object arg)
        {
            return SelectedItemSearchMethod != 2;
        }

        private void MaxValueExecute(object obj)
        {
            ;
        }

        private bool MaxLeafsCanExecute(object arg)
        {
            return SelectedItemSearchMethod != 2;
        }

        private void MaxLeafsExecute(object obj)
        {
            ;
        }

        private bool MaxMaxCanExecute(object arg)
        {
            return SelectedItemSearchMethod != 2;
        }

        private void MaxMaxExecute(object obj)
        {
            ;
        }

        private bool RotationCanExecute(object arg)
        {
            return true;
        }

        private void RotationExecute(object obj)
        {
            ;
        }

        private bool NoGoCanExecute(object arg)
        {
            return true;
        }

        private void NoGoExecute(object obj)
        {
            ;
        }

        private bool SymmetryCanExecute(object arg)
        {
            return true;
        }

        private void SymmetryExecute(object obj)
        {
            ;
        }

        #endregion
    }
}
 