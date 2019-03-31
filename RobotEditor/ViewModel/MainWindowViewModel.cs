using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media.Media3D;

using HelixToolkit.Wpf;

using Microsoft.Win32;

using RobotEditor.Model;
using RobotEditor.View;

using VirtualRobotWrapperLib.OBB;
using VirtualRobotWrapperLib.VirtualRobotManipulability;

namespace RobotEditor.ViewModel
{
    internal class MainWindowViewModel : BaseViewModel, IDisposable
    {
        #region Fields

        private readonly IHelixViewport3D _viewportCarbody;
        private readonly IHelixViewport3D _viewportRobot;
        private readonly CoordinateSystemVisual3D _carbodyCoordinateSystem;
        private IObbWrapper _obbCalculator;
        private IVirtualRobotManipulability _vrManip;

        private readonly Booth _booth;
        private RobotViewModel _selectedRobot;
        private CarbodyViewModel _selectedCarbody;

        private bool _isCheckedHitPoints;
        private bool _isCheckedRayOrigins;
        private bool _isCheckedBoundingBox;
        private bool _isCheckedSymmetryPlane;
        private bool _isCheckedManipulability;
        private double _precision;

        private VoxelOctree _octreeTemp;
        private bool _isBusy;

        #endregion

        #region Instance

        public MainWindowViewModel(HelixViewport3D viewportCarbody, HelixViewport3D viewportRobot)
        {
            CreateXml = new DelegateCommand<object>(CreateXmlExecute, CreateXmlCanExecute);
            AddCarbody = new DelegateCommand<object>(AddCarbodyExecute, AddCarbodyCanExecute);
            Compare = new DelegateCommand<object>(CompareExecute, CompareCanExecute);
            Update = new DelegateCommand<object>(UpdateExecute, UpdateCanExecute);
            DeleteCarbody = new DelegateCommand<object>(DeleteCarbodyExecute, DeleteCarbodyCanExecute);
            AddRobot = new DelegateCommand<object>(AddRobotExecute, AddRobotCanExecute);
            DeleteRobot = new DelegateCommand<object>(DeleteRobotExecute, DeleteRobotCanExecute);
            FitToViewCarbody = new DelegateCommand<object>(FitToViewCarbodyExecute, FitToViewCarbodyCanExecute);
            FitToViewRobot = new DelegateCommand<object>(FitToViewRobotExecute, FitToViewRobotCanExecute);
            EditRobot = new DelegateCommand<object>(EditRobotExecute, EditRobotCanExecute);
            HitPoints = new DelegateCommand<object>(HitPointsExecute, HitPointsCanExecute);
            RayOrigins = new DelegateCommand<object>(RayOriginsExecute, RayOriginsCanExecute);
            BoundingBox = new DelegateCommand<object>(BoundingBoxExecute, BoundingBoxCanExecute);
            Manipulability = new DelegateCommand<object>(ManipulabilityExecute, ManipulabilityCanExecute);
            SymmetryPlane = new DelegateCommand<object>(SymmetryPlaneExecute, SymmetryPlaneCanExecute);

            _viewportCarbody = viewportCarbody;
            _viewportRobot = viewportRobot;

            _booth = new Booth(10000, 100d);
            _precision = 100.0;
            _vrManip = new RemoteVirtualRobotManipulability();

            _carbodyCoordinateSystem = new CoordinateSystemVisual3D { ArrowLengths = 100.0 };
            CarbodyModels.Add(_carbodyCoordinateSystem);
            CarbodyModels.Add(new DefaultLights());

            RobotModels.Add(new DefaultLights());
            RobotModels.Add(new CoordinateSystemVisual3D() { ArrowLengths = 100.0 });

            _obbCalculator = new RemoteObbWrapper();

            RaisePropertyChanged();
        }

        #endregion

        #region Properties

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                if (value == _isBusy)
                    return;

                _isBusy = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<RobotViewModel> Robots { get; } = new ObservableCollection<RobotViewModel>();
        public ObservableCollection<CarbodyViewModel> Carbodies { get; } = new ObservableCollection<CarbodyViewModel>();

        public bool IsCheckedHitPoints
        {
            get { return _isCheckedHitPoints; }
            set
            {
                _isCheckedHitPoints = value;
                RaisePropertyChanged();
            }
        }

        public bool IsCheckedRayOrigins
        {
            get { return _isCheckedRayOrigins; }
            set
            {
                _isCheckedRayOrigins = value;
                RaisePropertyChanged();
            }
        }

        public bool IsCheckedBoundingBox
        {
            get { return _isCheckedBoundingBox; }
            set
            {
                _isCheckedBoundingBox = value;
                RaisePropertyChanged();
            }
        }

        public bool IsCheckedSymmetryPlane
        {
            get { return _isCheckedSymmetryPlane; }
            set
            {
                _isCheckedSymmetryPlane = value;
                RaisePropertyChanged();
            }
        }

        public bool IsCheckedManipulability
        {
            get { return _isCheckedManipulability; }
            set
            {
                _isCheckedManipulability = value;
                RaisePropertyChanged();
            }
        }

        public double Precision
        {
            get { return _precision; }
            set
            {
                _precision = value;
                RaisePropertyChanged();
            }
        }

        public DelegateCommand<object> CreateXml { get; }
        public DelegateCommand<object> HitPoints { get; }
        public DelegateCommand<object> RayOrigins { get; }
        public DelegateCommand<object> AddRobot { get; }
        public DelegateCommand<object> DeleteRobot { get; }
        public DelegateCommand<object> Compare { get; }
        public DelegateCommand<object> Update { get; }
        public DelegateCommand<object> AddCarbody { get; }
        public DelegateCommand<object> DeleteCarbody { get; }
        public DelegateCommand<object> FitToViewCarbody { get; }
        public DelegateCommand<object> FitToViewRobot { get; }
        public DelegateCommand<object> EditRobot { get; }
        public DelegateCommand<object> BoundingBox { get; }
        public DelegateCommand<object> SymmetryPlane { get; }
        public DelegateCommand<object> Manipulability { get; }

        public RobotViewModel SelectedRobot
        {
            get { return _selectedRobot; }
            set
            {
                if (Equals(value, _selectedRobot))
                    return;

                if (_selectedRobot != null)
                {
                    _selectedRobot.Model.Hide3DManipulabilityOctree();
                    RobotModels.Remove(_selectedRobot.RobotModel);
                }

                _selectedRobot = value;

                if (_selectedRobot != null)
                {
                    SelectedRobot.Model.Show3DRobot();

                    if (IsCheckedManipulability && SelectedRobot.Model.Octree != null)
                        SelectedRobot.Model.Show3DManipulabilityOctree();
                    RobotModels.Add(_selectedRobot.RobotModel);
                }

                RaisePropertyChanged();

                DeleteRobot.RaisePropertyChanged();

                CreateXml.RaisePropertyChanged();
                EditRobot.RaisePropertyChanged();
                Compare.RaisePropertyChanged();
                Manipulability.RaisePropertyChanged();
                Update.RaisePropertyChanged();

                _viewportRobot.ZoomExtents(0);
            }
        }

        public CarbodyViewModel SelectedCarbody
        {
            get { return _selectedCarbody; }
            set
            {
                if (Equals(value, _selectedCarbody))
                    return;

                if (_selectedCarbody != null)
                {
                    _selectedCarbody.Model.Hide3DHitPointGeometries();
                    _selectedCarbody.Model.Hide3DRayOriginGeometries();
                    _selectedCarbody.Model.Hide3DBoundingBoxGeometry();
                    _selectedCarbody.Model.Hide3DSymmetryPlaneGeometry();
                    CarbodyModels.Remove(_selectedCarbody.CarbodyModel);
                    CarbodyModels.Remove(_selectedCarbody.BoundingBox);
                }

                _selectedCarbody = value;

                if (_selectedCarbody != null)
                {
                    if (IsCheckedHitPoints && !IsBusy)
                        _selectedCarbody.Model.Show3DHitPointGeometries();
                    if (IsCheckedRayOrigins && !IsBusy)
                        _selectedCarbody.Model.Show3DRayOriginGeometries();
                    if (IsCheckedBoundingBox && !IsBusy)
                        _selectedCarbody.Model.Show3DBoundingBoxGeometry();
                    if (IsCheckedSymmetryPlane && !IsBusy)
                        _selectedCarbody.Model.Show3DSymmetryPlaneGeometry();

                    CarbodyModels.Add(_selectedCarbody.CarbodyModel);
                }

                RaisePropertyChanged();

                DeleteCarbody.RaisePropertyChanged();
                HitPoints.RaisePropertyChanged();
                RayOrigins.RaisePropertyChanged();
                BoundingBox.RaisePropertyChanged();
                SymmetryPlane.RaisePropertyChanged();
                Compare.RaisePropertyChanged();
                Update.RaisePropertyChanged();

                _viewportCarbody.ZoomExtents(0);
            }
        }

        public ObservableCollection<Visual3D> CarbodyModels { get; } = new ObservableCollection<Visual3D>();
        public ObservableCollection<Visual3D> RobotModels { get; } = new ObservableCollection<Visual3D>();

        #endregion

        #region Public methods

        public void Dispose()
        {
            _obbCalculator?.Dispose();
            _obbCalculator = null;

            _vrManip?.Dispose();
            _vrManip = null;
        }

        #endregion

        #region Private methods

        private bool AddRobotCanExecute(object arg)
        {
            return true;
        }

        private bool BoundingBoxCanExecute(object arg)
        {
            return SelectedCarbody != null;
        }

        private void BoundingBoxExecute(object obj)
        {
            if (IsCheckedBoundingBox)
                SelectedCarbody.Model.Show3DBoundingBoxGeometry();
            else
                SelectedCarbody.Model.Hide3DBoundingBoxGeometry();
        }

        private bool CompareCanExecute(object arg)
        {
            return Robots.Count > 0 && Carbodies.Count > 0;
        }

        private bool UpdateCanExecute(object arg)
        {
            return Robots.Count > 0 || Carbodies.Count > 0;
        }

        private bool SymmetryPlaneCanExecute(object arg)
        {
            return SelectedCarbody != null;
        }

        private void SymmetryPlaneExecute(object obj)
        {
            if (IsCheckedSymmetryPlane)
                SelectedCarbody.Model.Show3DSymmetryPlaneGeometry();
            else
                SelectedCarbody.Model.Hide3DSymmetryPlaneGeometry();
        }

        private bool HitPointsCanExecute(object arg)
        {
            if (SelectedCarbody == null)
                return false;

            return SelectedCarbody.Model.HitPoints.Count > 0;
        }

        private void HitPointsExecute(object obj)
        {
            if (IsCheckedHitPoints)
                SelectedCarbody.Model.Show3DHitPointGeometries();
            else
                SelectedCarbody.Model.Hide3DHitPointGeometries();
        }

        private bool RayOriginsCanExecute(object arg)
        {
            if (SelectedCarbody == null)
                return false;

            return SelectedCarbody.Model.RayOrigins.Count > 0;
        }

        private void RayOriginsExecute(object obj)
        {
            if (IsCheckedRayOrigins)
                SelectedCarbody.Model.Show3DRayOriginGeometries();
            else
                SelectedCarbody.Model.Hide3DRayOriginGeometries();
        }

        private void FitToViewCarbodyExecute(object obj)
        {
            _viewportCarbody.ZoomExtents(0);
            RaisePropertyChanged();
        }

        private bool FitToViewCarbodyCanExecute(object arg)
        {
            return true;
        }

        private void FitToViewRobotExecute(object obj)
        {
            _viewportRobot.ZoomExtents(0);
            RaisePropertyChanged();
        }

        private bool FitToViewRobotCanExecute(object arg)
        {
            return true;
        }

        private bool ManipulabilityCanExecute(object arg)
        {
            return SelectedRobot != null;
        }

        private void ManipulabilityExecute(object obj)
        {
            var backgroundWorker = new BackgroundWorker();

            backgroundWorker.DoWork += (s, e) =>
            {
                Application.Current.Dispatcher.Invoke(
                    () =>
                    {
                        if (IsCheckedManipulability)
                            SelectedRobot.Model.Show3DManipulabilityOctree();
                        else
                            SelectedRobot.Model.Hide3DManipulabilityOctree();
                    },
                    System.Windows.Threading.DispatcherPriority.ContextIdle);
            };

            backgroundWorker.RunWorkerCompleted += (s, e) => { IsBusy = false; };

            IsBusy = true;
            backgroundWorker.RunWorkerAsync();
        }

        private void CompareExecute(object obj)
        {
            HideAdditionGeometries();

            IsBusy = true;

            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += (s, e) =>
            {
                if (Robots.Any(r => r.Precision != Precision))
                    UpdateExecute(null);

                foreach (var carbody in Carbodies)
                {
                    Application.Current.Dispatcher.Invoke(() => SelectedCarbody = carbody);

                    _octreeTemp = VoxelOctree.Create(Math.Abs(SelectedCarbody.Model.BoundingBoxHalfExtents.Max()) * 4, Precision);

                    Application.Current.Dispatcher.Invoke(() => _viewportCarbody.ZoomExtents(0));

                    // Perform hit test

                    for (var m = 0; m < 3; m++)
                    {
                        var directionSelector = new [,] { { 1, 2 }, { 0, 2 }, { 0, 1 } };
                        var factor = new [] { 1, 1, 1 };
                        var factor2 = new [] { 0, 0, 0 };
                        var factor3 = new [] { 0, 0, 0 };
                        factor[m] = -1;

                        var matrixStart = new Matrix3D(
                            SelectedCarbody.Model.BoundingBoxAxis[0][0],
                            SelectedCarbody.Model.BoundingBoxAxis[0][1],
                            SelectedCarbody.Model.BoundingBoxAxis[0][2],
                            0.0,
                            SelectedCarbody.Model.BoundingBoxAxis[1][0],
                            SelectedCarbody.Model.BoundingBoxAxis[1][1],
                            SelectedCarbody.Model.BoundingBoxAxis[1][2],
                            0.0,
                            SelectedCarbody.Model.BoundingBoxAxis[2][0],
                            SelectedCarbody.Model.BoundingBoxAxis[2][1],
                            SelectedCarbody.Model.BoundingBoxAxis[2][2],
                            0.0,
                            SelectedCarbody.Model.BoundingBoxPosition[0],
                            SelectedCarbody.Model.BoundingBoxPosition[1],
                            SelectedCarbody.Model.BoundingBoxPosition[2],
                            1.0
                        );

                        var matrixEnd = new Matrix3D(
                            SelectedCarbody.Model.BoundingBoxAxis[0][0],
                            SelectedCarbody.Model.BoundingBoxAxis[0][1],
                            SelectedCarbody.Model.BoundingBoxAxis[0][2],
                            0.0,
                            SelectedCarbody.Model.BoundingBoxAxis[1][0],
                            SelectedCarbody.Model.BoundingBoxAxis[1][1],
                            SelectedCarbody.Model.BoundingBoxAxis[1][2],
                            0.0,
                            SelectedCarbody.Model.BoundingBoxAxis[2][0],
                            SelectedCarbody.Model.BoundingBoxAxis[2][1],
                            SelectedCarbody.Model.BoundingBoxAxis[2][2],
                            0.0,
                            SelectedCarbody.Model.BoundingBoxPosition[0],
                            SelectedCarbody.Model.BoundingBoxPosition[1],
                            SelectedCarbody.Model.BoundingBoxPosition[2],
                            1.0
                        );

                        var matrixTranslationStart = new Vector3D(
                            SelectedCarbody.Model.BoundingBoxHalfExtents[0],
                            SelectedCarbody.Model.BoundingBoxHalfExtents[1],
                            SelectedCarbody.Model.BoundingBoxHalfExtents[2]
                        );

                        var matrixTranslationEnd = new Vector3D(
                            factor[0] * SelectedCarbody.Model.BoundingBoxHalfExtents[0],
                            factor[1] * SelectedCarbody.Model.BoundingBoxHalfExtents[1],
                            factor[2] * SelectedCarbody.Model.BoundingBoxHalfExtents[2]
                        );

                        matrixStart.TranslatePrepend(matrixTranslationStart);
                        matrixEnd.TranslatePrepend(matrixTranslationEnd);

                        for (var j = 0; j < Math.Abs(SelectedCarbody.Model.BoundingBoxHalfExtents[directionSelector[m, 0]]) * 2 / Precision; j++)
                        {
                            var total = 0;
                            Vector3D vector;
                            for (var k = 0; k < Math.Abs(SelectedCarbody.Model.BoundingBoxHalfExtents[directionSelector[m, 1]]) * 2 / Precision; k++)
                            {
                                Application.Current.Dispatcher.Invoke(() => SelectedCarbody.RayHi(matrixStart, matrixEnd, _octreeTemp));

                                total += -100;
                                factor2[directionSelector[m, 1]] = 1;
                                vector = new Vector3D(factor2[0] * -100, factor2[1] * -100, factor2[2] * -100);

                                matrixStart.TranslatePrepend(vector);
                                matrixEnd.TranslatePrepend(vector);
                            }

                            vector = new Vector3D(factor2[0] * -total, factor2[1] * -total, factor2[2] * -total);

                            matrixStart.TranslatePrepend(vector);
                            matrixEnd.TranslatePrepend(vector);

                            factor3[directionSelector[m, 0]] = 1;
                            vector = new Vector3D(factor3[0] * -100, factor3[1] * -100, factor3[2] * -100);

                            matrixStart.TranslatePrepend(vector);
                            matrixEnd.TranslatePrepend(vector);
                        }
                    }

                    _booth.Octree.Add(_octreeTemp);
                }
            };

            backgroundWorker.RunWorkerCompleted += (s, e) =>
            {
                IsBusy = false;

                ShowAdditionalGeometries();

                //octree = VoxelOctree.Create(400, 100d);
                //octree.Set(-790, -790, -790, 1);
                //octree.Set(-1, -1, -1, 2);
                //octree.Set(790, -790, -790, 1);
                //octree.Set(1, -1, -1, 2);

                var comparison = new ResultWindow(_booth.Octree);
                var result = comparison.ShowDialog();

                if (result == true)
                    ;

                if (result != true)
                    return;
            };

            backgroundWorker.RunWorkerAsync();
        }

        private void UpdateExecute(object obj)
        {
            var currentlySelectedRobot = SelectedRobot;

            var backgroundWorker = new BackgroundWorker();

            backgroundWorker.DoWork += (s, e) =>
            {
                Application.Current.Dispatcher.Invoke(
                    () =>
                    {
                        foreach (var robot in Robots)
                        {
                            SelectedRobot = robot;
                            robot.CalcManipulability(_vrManip, _booth);
                        }

                        SelectedRobot = null;
                        SelectedRobot = currentlySelectedRobot;
                    },
                    System.Windows.Threading.DispatcherPriority.ContextIdle);
            };

            backgroundWorker.RunWorkerCompleted += (s, e) => { IsBusy = false; };

            IsBusy = true;
            backgroundWorker.RunWorkerAsync();
        }

        private void AddRobotExecute(object obj)
        {
            var i = Robots.Count;
            foreach (var r in Robots)
            {
                if (r.Name == $"Robot_{i}")
                    i++;
            }

            var robot = new Robot(Robot.RobotTypes.Empty, i);
            var newRobot = new RobotValues { DataContext = new RobotViewModel(robot) };
            var result = newRobot.ShowDialog();

            if (result == true)
            {
                var backgroundWorker = new BackgroundWorker();

                backgroundWorker.DoWork += (s, e) =>
                {
                    RobotViewModel robotViewModel = null;
                    Application.Current.Dispatcher.Invoke(
                        () =>
                        {
                            SelectedRobot?.Model.Hide3DRobot();

                            if (IsCheckedManipulability)
                                SelectedRobot?.Model.Hide3DManipulabilityOctree();

                            robotViewModel = new RobotViewModel(robot);
                            Robots.Add(robotViewModel);
                            SelectedRobot = robotViewModel;
                        });

                    robotViewModel?.CalcManipulability(_vrManip, _booth);

                    Application.Current.Dispatcher.Invoke(
                        () =>
                        {
                            if (IsCheckedManipulability)
                                SelectedRobot.Model.Show3DManipulabilityOctree();
                        });
                };

                backgroundWorker.RunWorkerCompleted += (s, e) => { IsBusy = false; };

                IsBusy = true;
                backgroundWorker.RunWorkerAsync();
            }

            _viewportRobot.ZoomExtents(0);
        }

        private bool EditRobotCanExecute(object arg)
        {
            return SelectedRobot != null;
        }

        private void EditRobotExecute(object obj)
        {
            SelectedRobot.Model.Hide3DRobot();
            if (IsCheckedManipulability)
                SelectedRobot.Model.Hide3DManipulabilityOctree();

            var robotViewModel = new RobotViewModel(_selectedRobot.Model);
            var newRobot = new RobotValues { DataContext = robotViewModel };
            var result = newRobot.ShowDialog();

            if (result == true)
                robotViewModel.CalcManipulability(_vrManip, _booth);

            SelectedRobot.Model.Show3DRobot();
            if (IsCheckedManipulability)
                SelectedRobot.Model.Show3DManipulabilityOctree();

            _viewportRobot.ZoomExtents(0);
        }

        private void DeleteRobotExecute(object obj)
        {
            var backgroundWorker = new BackgroundWorker();

            backgroundWorker.DoWork += (s, e) =>
            {
                Application.Current.Dispatcher.Invoke(
                    () =>
                    {
                        if (IsCheckedManipulability)
                            SelectedRobot?.Model.Hide3DManipulabilityOctree();

                        var currentlySelected = SelectedRobot;
                        SelectedRobot = Robots.FirstOrDefault(c => !ReferenceEquals(c, currentlySelected));
                        Robots.Remove(currentlySelected);

                        if (Robots.Count == 0)
                            IsCheckedManipulability = false;

                        if (IsCheckedManipulability)
                            SelectedRobot?.Model.Show3DManipulabilityOctree();
                    },
                    System.Windows.Threading.DispatcherPriority.ContextIdle);
            };

            backgroundWorker.RunWorkerCompleted += (s, e) => { IsBusy = false; };

            IsBusy = true;
            backgroundWorker.RunWorkerAsync();

            RaisePropertyChanged();
        }

        private bool DeleteRobotCanExecute(object arg)
        {
            return SelectedRobot != null;
        }

        private bool AddCarbodyCanExecute(object arg)
        {
            return true;
        }

        private void CreateXmlExecute(object obj)
        {
            SelectedRobot.Model.SaveRobotStructur();
        }

        private bool CreateXmlCanExecute(object arg)
        {
            return SelectedRobot != null;
        }

        private void AddCarbodyExecute(object obj)
        {
            var fileDialog = new OpenFileDialog()
            {
                Multiselect = true
            };
            if (fileDialog.ShowDialog() != true)
                return;

            var fileNames = fileDialog.FileNames;
            var safeFileNames = fileDialog.SafeFileNames;

            if (fileNames.Length <= 0 || safeFileNames.Length <= 0)
                return;

            var backgroundWorker = new BackgroundWorker();

            backgroundWorker.DoWork += (s, e) =>
            {
                for (var i = 0; i < fileNames.Length && i < safeFileNames.Length; i++)
                {
                    var fileName = fileNames[i];
                    var safeFileName = safeFileNames[i];
                    var mi = new ModelImporter();
                    var importedModel = mi.Load(fileName, null, true);
                    Application.Current.Dispatcher.Invoke(
                        () =>
                        {
                            var model = new ModelVisual3D { Content = importedModel };
                            var carbody = new Carbody(_obbCalculator, fileName, safeFileName, model);
                            var newCarbody = new CarbodyViewModel(carbody);
                            Carbodies.Add(newCarbody);
                            SelectedCarbody = newCarbody;
                        });

                    if (SelectedCarbody == null)
                        return;

                    try
                    {
                        SelectedCarbody.CalcRefPosition(Application.Current.Dispatcher);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            };

            backgroundWorker.RunWorkerCompleted += (s, e) =>
            {
                ShowAdditionalGeometries();

                RayOrigins.RaisePropertyChanged();
                HitPoints.RaisePropertyChanged();

                IsBusy = false;
            };

            IsBusy = true;

            HideAdditionGeometries();

            backgroundWorker.RunWorkerAsync();
        }

        private void DeleteCarbodyExecute(object obj)
        {
            var backgroundWorker = new BackgroundWorker();

            backgroundWorker.DoWork += (s, e) =>
            {
                Application.Current.Dispatcher.Invoke(
                    () =>
                    {
                        var currentlySelected = SelectedCarbody;
                        SelectedCarbody = Carbodies.FirstOrDefault(c => !ReferenceEquals(c, currentlySelected));
                        Carbodies.Remove(currentlySelected);
                    },
                    System.Windows.Threading.DispatcherPriority.ContextIdle);
            };

            backgroundWorker.RunWorkerCompleted += (s, e) => { IsBusy = false; };

            IsBusy = true;
            backgroundWorker.RunWorkerAsync();

            RaisePropertyChanged();
        }

        private bool DeleteCarbodyCanExecute(object arg)
        {
            return SelectedCarbody != null;
        }

        private void ShowAdditionalGeometries()
        {
            if (IsCheckedRayOrigins)
                SelectedCarbody.Model.Show3DRayOriginGeometries();

            if (IsCheckedHitPoints)
                SelectedCarbody.Model.Show3DHitPointGeometries();

            if (IsCheckedBoundingBox)
                SelectedCarbody.Model.Show3DBoundingBoxGeometry();

            if (IsCheckedSymmetryPlane)
                SelectedCarbody.Model.Show3DSymmetryPlaneGeometry();

            CarbodyModels.Insert(0, _carbodyCoordinateSystem);
        }

        private void HideAdditionGeometries()
        {
            foreach (var carbody in Carbodies)
            {
                carbody.Model.Hide3DRayOriginGeometries();

                carbody.Model.Hide3DHitPointGeometries();

                carbody.Model.Hide3DBoundingBoxGeometry();

                carbody.Model.Hide3DSymmetryPlaneGeometry();
            }

            CarbodyModels.Remove(_carbodyCoordinateSystem);
        }

        #endregion
    }
}