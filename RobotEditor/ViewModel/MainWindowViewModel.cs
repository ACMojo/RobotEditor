using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
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
        private IObbWrapper _obbCalculator;
        private IVirtualRobotManipulability _vrManip;
        private RobotViewModel _selectedRobot;
        private CarbodyViewModel _selectedCarbody;
        private readonly Booth _booth;
        private VoxelOctree _octreeTemp;

        #endregion

        #region Instance

        public MainWindowViewModel(HelixViewport3D viewportCarbody, HelixViewport3D viewportRobot)
        {
            CreateXml = new DelegateCommand<object>(CreateXmlExecute, CreateXmlCanExecute);
            AddCarbody = new DelegateCommand<object>(AddCarbodyExecute, AddCarbodyCanExecute);
            Compare = new DelegateCommand<object>(CompareExecute, CompareCanExecute);
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

            this._viewportCarbody = viewportCarbody;
            this._viewportRobot = viewportRobot;
            _booth = new Booth(10000, 100d);

            CarbodyModels.Add(new CoordinateSystemVisual3D() { ArrowLengths = 100.0 });
            CarbodyModels.Add(new DefaultLights());

            RobotModels.Add(new DefaultLights());
            RobotModels.Add(new CoordinateSystemVisual3D() { ArrowLengths = 100.0 });

            _obbCalculator = new RemoteObbWrapper();
            _vrManip = new RemoteVirtualRobotManipulability();

            RaisePropertyChanged();
        }

        #endregion

        #region Properties

        public ObservableCollection<RobotViewModel> Robots { get; } = new ObservableCollection<RobotViewModel>();
        public ObservableCollection<CarbodyViewModel> Carbodies { get; } = new ObservableCollection<CarbodyViewModel>();
        public bool IsCheckedHitPoints { get; set; }
        public bool IsCheckedRayOrigins { get; set; }
        public bool IsCheckedBoundingBox { get; set; }
        public bool IsCheckedSymmetryPlane { get; set; }
        public bool IsCheckedManipulability { get; set; }

        public DelegateCommand<object> CreateXml { get; }
        public DelegateCommand<object> HitPoints { get; }
        public DelegateCommand<object> RayOrigins { get; }
        public DelegateCommand<object> AddRobot { get; }
        public DelegateCommand<object> DeleteRobot { get; }
        public DelegateCommand<object> Compare { get; }
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
                    RobotModels.Remove(_selectedRobot.RobotModel);

                _selectedRobot = value;

                if (_selectedRobot != null)
                    RobotModels.Add(_selectedRobot.RobotModel);

                RaisePropertyChanged();

                DeleteRobot.RaisePropertyChanged();

                CreateXml.RaisePropertyChanged();
                EditRobot.RaisePropertyChanged();
                Compare.RaisePropertyChanged();
                Manipulability.RaisePropertyChanged();

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
                    if (IsCheckedHitPoints)
                        _selectedCarbody.Model.Show3DHitPointGeometries();
                    if (IsCheckedRayOrigins)
                        _selectedCarbody.Model.Show3DRayOriginGeometries();
                    if (IsCheckedBoundingBox)
                        _selectedCarbody.Model.Show3DBoundingBoxGeometry();
                    if (IsCheckedSymmetryPlane)
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
            if (IsCheckedManipulability)
                SelectedRobot.Model.Show3DManipulabilityOctree();
            else
                SelectedRobot.Model.Hide3DManipulabilityOctree();
        }

        private void CompareExecute(object obj)
        {
            foreach (var carbody in Carbodies)
            {
                SelectedCarbody = carbody;

                _octreeTemp = VoxelOctree.Create(Math.Abs(SelectedCarbody.Model.BoundingBoxHalfExtents.Max()) * 4, 100d);

                _viewportCarbody.ZoomExtents(0);

                // Perform hit test

                for (int m = 0; m < 3; m++)
                {
                    int[,] directionSelector = new int[,] { { 1, 2 }, { 0, 2 }, { 0, 1 } };
                    int[] factor = new int[3] { 1, 1, 1 };
                    int[] factor2 = new int[3] { 0, 0, 0 };
                    int[] factor3 = new int[3] { 0, 0, 0 };
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

                    Vector3D vector;

                    for (int j = 0; j < Math.Abs(SelectedCarbody.Model.BoundingBoxHalfExtents[directionSelector[m, 0]]) * 2 / 100.0; j++)
                    {
                        var total = 0;
                        for (int k = 0; k < Math.Abs(SelectedCarbody.Model.BoundingBoxHalfExtents[directionSelector[m, 1]]) * 2 / 100.0; k++)
                        {
                            RayHi(matrixStart, matrixEnd);

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
        }

        private void AddRobotExecute(object obj)
        {
            var robot = new Robot(Robot.RobotTypes.Puma560);
            var newRobot = new RobotValues { DataContext = new RobotViewModel(robot) };
            var result = newRobot.ShowDialog();

            if (result == true)
            {
                robot.RobotModel.Children.Clear();
                //drawVoxelMap(robot);
                SelectedRobot = new RobotViewModel(robot);
                Robots.Add(SelectedRobot);

                if (SelectedRobot != null)
                {
                    SelectedRobot.Model.Show3DRobot();
                    CalcManipulability();
                }
            }

            _viewportRobot.ZoomExtents(0);

            if (result != true)
                return;
        }

        private void DrawVoxelMap(Robot robot)
        {
            robot.RobotModel.Children.Clear();
            for (int i = 0; i < 120; i++)
            {
                for (int j = 0; j < 120; j++)
                {
                    for (int k = 0; k < 120; k++)
                    {
                        /*var vm = new MeshGeometryVisual3D();
                        var mb = new MeshBuilder();
                        mb.AddBox(robot.VoxelMap[i, j, k].PositionFromRobotBase, 10.0, 10.0, 10.0);
                        vm.MeshGeometry = mb.ToMesh();
                        vm.Material = MaterialHelper.CreateMaterial(robot.VoxelMap[i, j, k].Colour);
                        robot.RobotModel.Children.Add(vm);
                         */

                        PointsVisual3D line = new PointsVisual3D();
                        line.Size = 5.0;
                        TranslateTransform3D tr = new TranslateTransform3D();
                        tr.OffsetX = i * 6;
                        tr.OffsetY = j * 6;
                        tr.OffsetZ = k * 6;

                        line.Transform = tr;
                        line.Color = Colors.Gray;
                        robot.RobotModel.Children.Add(line);
                    }
                }
            }
            //robot.RobotModel.Children.Clear();
            //robot.RobotModel.Children.Add(baseCoordinateSystem);

            //return robot;
        }

        //private HitTestResultBehavior HitTestResultCallback( RayHitTestParameters parameters, HitTestResult result)

        private HitTestResultBehavior HitTestResultCallback(ref double x, HitTestResult result)
        {
            // Did we hit 3D?
            var rayResult = result as RayHitTestResult;

            // Did we hit a MeshGeometry3D?
            var rayMeshResult = rayResult as RayMeshGeometry3DHitTestResult;

            if (rayMeshResult != null)
            {
                // Yes we did!

                // Used to show surface hits of ray
                SelectedCarbody.Model.Add3DHitPoint(rayMeshResult.PointHit);

                x = rayResult.DistanceToRayOrigin;

                /*
                if (octree.Set((int)Math.Floor(rayMeshResult.PointHit.X / 100.0), (int)Math.Floor(rayMeshResult.PointHit.Y / 100.0), (int)Math.Floor(rayMeshResult.PointHit.Z / 100.0), 1))
                {
                    //Console.WriteLine("Erfolgreich");
                }
                else
                {
                    var value = octree.Get((int)Math.Floor(rayMeshResult.PointHit.X / 100.0), (int)Math.Floor(rayMeshResult.PointHit.Y / 100.0), (int)Math.Floor(rayMeshResult.PointHit.Z / 100.0));
                    if (double.IsNaN(value))
                        Console.WriteLine($"Nicht erfolgreich bei: {rayMeshResult.PointHit.X} {rayMeshResult.PointHit.Y} {rayMeshResult.PointHit.Z}");
                    else
                        ;
                }*/

                //Console.WriteLine(rayMeshResult.DistanceToRayOrigin);
                return HitTestResultBehavior.Stop;
            }

            return HitTestResultBehavior.Continue;
        }

        private HitTestResultBehavior HitTestResultCallback(HitTestResult result)
        {
            // Did we hit 3D?
            var rayResult = result as RayHitTestResult;

            // Did we hit a MeshGeometry3D?
            var rayMeshResult = rayResult as RayMeshGeometry3DHitTestResult;

            if (rayMeshResult != null)
            {
                // Yes we did!

                // Used to show surface hits of ray
                SelectedCarbody.Model.Add3DHitPoint(rayMeshResult.PointHit);

                //Console.WriteLine(coordinateSystem.GetTransform().OffsetX + " " + PointTest.X + " " + coordinateSystem.GetTransform().OffsetY + " " + PointTest.Y + " " + coordinateSystem.GetTransform().OffsetZ + " " + PointTest.Z);

                var pointTest = rayMeshResult.PointHit;
                pointTest = Point3D.Multiply(pointTest, SelectedCarbody.Model.CarbodyModel.GetTransform());
                _octreeTemp.Set((int)pointTest.X, (int)pointTest.Y, (int)pointTest.Z, 1.0);

                /*
                if (octree.Set((int)Math.Floor(rayMeshResult.PointHit.X / 100.0), (int)Math.Floor(rayMeshResult.PointHit.Y / 100.0), (int)Math.Floor(rayMeshResult.PointHit.Z / 100.0), 1))
                {
                    //Console.WriteLine("Erfolgreich");
                }
                else
                {
                    var value = octree.Get((int)Math.Floor(rayMeshResult.PointHit.X / 100.0), (int)Math.Floor(rayMeshResult.PointHit.Y / 100.0), (int)Math.Floor(rayMeshResult.PointHit.Z / 100.0));
                    if (double.IsNaN(value))
                        Console.WriteLine($"Nicht erfolgreich bei: {rayMeshResult.PointHit.X} {rayMeshResult.PointHit.Y} {rayMeshResult.PointHit.Z}");
                    else
                        ;
                }*/

                //Console.WriteLine(rayMeshResult.DistanceToRayOrigin);
                return HitTestResultBehavior.Stop;
            }

            return HitTestResultBehavior.Continue;
        }

        private bool EditRobotCanExecute(object arg)
        {
            return SelectedRobot != null;
        }

        private void EditRobotExecute(object obj)
        {
            //var robot = new Robot(0, "Roboter " + Robots.Count);
            var newRobot = new RobotValues { DataContext = new RobotViewModel(_selectedRobot.Model) };
            var result = newRobot.ShowDialog();

            if (result == true)
            {
                Robots.Remove(_selectedRobot);
                //RobotModels.Remove(_selectedRobot.robotModel);
                //_selectedRobot.robotModel = new ModelVisual3D();
                _selectedRobot.Model.Show3DRobot();
                RobotModels.Add(_selectedRobot.RobotModel);
                _viewportRobot.ZoomExtents(0);
            }
        }

        private void DeleteRobotExecute(object obj)
        {
            Robots.Remove(_selectedRobot);

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

        private void CalcManipulability()
        {
            SelectedRobot.Model.SaveRobotStructur();

            float[] maxB;
            float[] minB;
            float maxManip;

            string path = AppDomain.CurrentDomain.BaseDirectory + SelectedRobot.Model.Name;
            if (_vrManip.Init(0, null, path, "robotNodeSet", "root", "tcp"))
            {
                ManipulabilityVoxel[] vox = _vrManip.GetManipulabilityWithPenalty((float)100.0, (float)(Math.PI / 2), 100000, false, false, true, 50f);

                minB = _vrManip.MinBox;
                maxB = _vrManip.MaxBox;

                // Calc size of cube depending on reachability of robot
                double octreeSize;
                if (Math.Abs(_vrManip.MaxBox.Max()) > Math.Abs(_vrManip.MinBox.Max()))
                    octreeSize = Math.Abs(_vrManip.MaxBox.Max()) * 2;
                else
                    octreeSize = Math.Abs(_vrManip.MinBox.Max()) * 2;

                SelectedRobot.Model.Octree = VoxelOctree.Create(octreeSize, 100.0);

                maxManip = _vrManip.MaxManipulability;

                ManipulabilityVoxel voxOld = vox[0];
                double maxValue = vox[0].Value;
                for (int j = 1; j < vox.Length; j++)
                {
                    // TODO: MaxWert gewichten, je nach Drehung zwsichen Roboter und Fahrzeug

                    if (vox[j].X == voxOld.X && vox[j].Y == voxOld.Y && vox[j].Z == voxOld.Z)
                    {
                        if (vox[j].Value > maxValue)
                            maxValue = vox[j].Value;
                    }
                    else
                    {
                        if (!SelectedRobot.Model.Octree.Set(
                                (int)(minB[0] + voxOld.X * 100),
                                (int)(minB[1] + voxOld.Y * 100),
                                (int)(minB[2] + voxOld.Z * 100),
                                maxValue))
                        {
                            var value = _booth.Octree.Get(
                                (int)Math.Floor(minB[0] / 100.0 + voxOld.X),
                                (int)Math.Floor(minB[1] / 100.0 + voxOld.Y),
                                (int)Math.Floor(minB[2] / 100.0 + voxOld.Z));
                            if (double.IsNaN(value))
                                Console.WriteLine(
                                    $"Nicht erfolgreich bei: {Math.Floor(minB[0] / 100.0 + voxOld.X)} {Math.Floor(minB[1] / 100.0 + voxOld.Y)} {Math.Floor(minB[2] / 100.0 + voxOld.Z)}");
                        }

                        voxOld = vox[j];
                        maxValue = vox[j].Value;
                    }
                }
            }

            RaisePropertyChanged();
        }

        private bool CalcManipulabilityCanExecute(object arg)
        {
            return SelectedRobot != null;
        }

        private void AddCarbodyExecute(object obj)
        {
            var fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() == true)
            {
                var mi = new ModelImporter();

                var carbody = new Carbody(
                    _obbCalculator,
                    fileDialog.FileName,
                    fileDialog.SafeFileName,
                    new ModelVisual3D { Content = mi.Load(fileDialog.FileName, null, true) });

                var newCarbody = new CarbodyViewModel(carbody);
                Carbodies.Add(newCarbody);
                SelectedCarbody = newCarbody;

                if (SelectedCarbody != null)
                    CalcRefPosition();
            }
        }

        private void DeleteCarbodyExecute(object obj)
        {
            var currentlySelected = SelectedCarbody;
            SelectedCarbody = Carbodies.FirstOrDefault(c => !ReferenceEquals(c, currentlySelected));
            Carbodies.Remove(currentlySelected);

            RaisePropertyChanged();
        }

        private bool DeleteCarbodyCanExecute(object arg)
        {
            return SelectedCarbody != null;
        }

        private void CalcRefPosition()
        {
            // Perform hit test
            /*
            for(int m = 0; m<3; m++)
            {
                int[,] directionSelector = new int[,] { { 1, 2 }, { 0, 2 }, { 0, 1 } };
                int[] factor = new int[3] { 1, 1, 1 };
                int[] factor2 = new int[3] { 0, 0, 0 };
                int[] factor3 = new int[3] { 0, 0, 0 };
                factor[m] = -1;

                var matrixStart = new Matrix3D(
                                     BoundingBoxAxis[0][0],
                                     BoundingBoxAxis[0][1],
                                     BoundingBoxAxis[0][2],
                                     0.0,
                                     BoundingBoxAxis[1][0],
                                     BoundingBoxAxis[1][1],
                                     BoundingBoxAxis[1][2],
                                     0.0,
                                     BoundingBoxAxis[2][0],
                                     BoundingBoxAxis[2][1],
                                     BoundingBoxAxis[2][2],
                                     0.0,
                                     BoundingBoxPosition[0],
                                     BoundingBoxPosition[1],
                                     BoundingBoxPosition[2],
                                     1.0
                                     );

                var matrixEnd = new Matrix3D(
                                     BoundingBoxAxis[0][0],
                                     BoundingBoxAxis[0][1],
                                     BoundingBoxAxis[0][2],
                                     0.0,
                                     BoundingBoxAxis[1][0],
                                     BoundingBoxAxis[1][1],
                                     BoundingBoxAxis[1][2],
                                     0.0,
                                     BoundingBoxAxis[2][0],
                                     BoundingBoxAxis[2][1],
                                     BoundingBoxAxis[2][2],
                                     0.0,
                                     BoundingBoxPosition[0],
                                     BoundingBoxPosition[1],
                                     BoundingBoxPosition[2],
                                     1.0
                                     );

                var matrixTranslationStart = new Vector3D(
                                 BoundingBoxHalfExtents[0],
                                 BoundingBoxHalfExtents[1],
                                 BoundingBoxHalfExtents[2]
                                );

                var matrixTranslationEnd = new Vector3D(
                             factor[0] * BoundingBoxHalfExtents[0],
                             factor[1] * BoundingBoxHalfExtents[1],
                             factor[2] * BoundingBoxHalfExtents[2]
                             );

                matrixStart.TranslatePrepend(matrixTranslationStart);
                matrixEnd.TranslatePrepend(matrixTranslationEnd);

                Vector3D vector;

                for (int j = 0; j < (Math.Abs(BoundingBoxHalfExtents[directionSelector[m,0]]) * 2) / 100.0; j++)
                {
                    var total = 0;
                    for (int k = 0; k < (Math.Abs(BoundingBoxHalfExtents[directionSelector[m,1]]) * 2) / 100.0; k++)
                    {
                        RayHi(matrixStart, matrixEnd);

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
            */

            // Find symmetry plane
            int directionOfTop = 0;
            int directionOfTopShift = 0;
            int directionOfFront = 0;
            int directionOfFrontShift = 0;
            double sumOfSquaresDivided = 9999.0;
            for (int m = 0; m < 2; m++)
            {
                Vector3D vector;
                int[] stepDirection = new int[3] { 0, 0, 0 };
                int[] sideASelector = new int[3] { 0, 0, 0 };
                int[] sideBSelector = new int[3] { 0, 0, 0 };

                int z = m;
                if (Array.IndexOf(SelectedCarbody.Model.BoundingBoxHalfExtents, SelectedCarbody.Model.BoundingBoxHalfExtents.Max()) == m)
                    z = m + 1;

                if (m == 1 && Array.IndexOf(SelectedCarbody.Model.BoundingBoxHalfExtents, SelectedCarbody.Model.BoundingBoxHalfExtents.Max()) == 0)
                    z = 2;

                sideASelector[Array.IndexOf(SelectedCarbody.Model.BoundingBoxHalfExtents, SelectedCarbody.Model.BoundingBoxHalfExtents.Max())] = 1;
                sideBSelector[Array.IndexOf(SelectedCarbody.Model.BoundingBoxHalfExtents, SelectedCarbody.Model.BoundingBoxHalfExtents.Max())] = 1;
                sideASelector[z] = 1;
                sideBSelector[z] = -1;

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
                    sideASelector[0] * SelectedCarbody.Model.BoundingBoxHalfExtents[0],
                    sideASelector[1] * SelectedCarbody.Model.BoundingBoxHalfExtents[1],
                    sideASelector[2] * SelectedCarbody.Model.BoundingBoxHalfExtents[2]
                );

                var matrixTranslationEnd = new Vector3D(
                    sideBSelector[0] * SelectedCarbody.Model.BoundingBoxHalfExtents[0],
                    sideBSelector[1] * SelectedCarbody.Model.BoundingBoxHalfExtents[1],
                    sideBSelector[2] * SelectedCarbody.Model.BoundingBoxHalfExtents[2]
                );

                matrixStart.TranslatePrepend(matrixTranslationStart);
                matrixEnd.TranslatePrepend(matrixTranslationEnd);

                double[] savor = new double[(int)(Math.Abs(SelectedCarbody.Model.BoundingBoxHalfExtents.Max() * 2 / 50.0) + 1)];
                double[] savor2 = new double[(int)(Math.Abs(SelectedCarbody.Model.BoundingBoxHalfExtents.Max() * 2 / 50.0) + 1)];
                double[] savor3 = new double[(int)(Math.Abs(SelectedCarbody.Model.BoundingBoxHalfExtents.Max() * 2 / 50.0) + 1)];
                double[] savor4 = new double[(int)(Math.Abs(SelectedCarbody.Model.BoundingBoxHalfExtents.Max() * 2 / 50.0) + 1)];

                for (int k = 0; k < Math.Abs(SelectedCarbody.Model.BoundingBoxHalfExtents.Max() * 2 / 50); k++)
                {
                    savor[k] = double.NaN;
                    RayHi(matrixStart, matrixEnd, ref savor[k], ref savor2[k], ref savor3[k]);

                    stepDirection[Array.IndexOf(SelectedCarbody.Model.BoundingBoxHalfExtents, SelectedCarbody.Model.BoundingBoxHalfExtents.Max())] = 1;
                    vector = new Vector3D(stepDirection[0] * -50, stepDirection[1] * -50, stepDirection[2] * -50);

                    matrixStart.TranslatePrepend(vector);
                    matrixEnd.TranslatePrepend(vector);
                }

                double sumOfSquares = 0.0;
                int count = 0;
                // symmetry plane
                foreach (double x in savor)
                {
                    if (x != double.NaN)
                    {
                        count++;
                        sumOfSquares += x;
                    }
                }
                // bottom top calculation

                for (int q = 0; q < savor2.Length; q++)
                {
                    if (savor2[q] > 0.1 && savor3[q] > 0.1)
                        savor4[q] = SelectedCarbody.Model.BoundingBoxHalfExtents[z] * 2 - savor2[q] - savor3[q];
                }

                if (savor4.Average() < 100.0)
                {
                    directionOfTop = z;
                    double totalFirst = 0.0;
                    double totalLast = 0.0;
                    directionOfFront = Array.IndexOf(SelectedCarbody.Model.BoundingBoxHalfExtents, SelectedCarbody.Model.BoundingBoxHalfExtents.Max());
                    if (savor2.Max() > savor3.Max())
                    {
                        directionOfTopShift = 1;

                        for (int r = 0; r < savor3.Length / 3; r++)
                        {
                            totalFirst += savor3[r];
                            totalLast += savor3[savor3.Length - 1 - r];
                        }

                        if (totalFirst > totalLast)
                            directionOfFrontShift = 1;
                        else
                            directionOfFrontShift = -1;
                    }
                    else
                    {
                        directionOfTopShift = -1;
                        for (int r = 0; r < savor3.Length / 3; r++)
                        {
                            totalFirst += savor3[r];
                            totalLast += savor3[savor3.Length - 1 - r];
                        }

                        if (totalFirst > totalLast)
                            directionOfFrontShift = 1;
                        else
                            directionOfFrontShift = -1;
                    }
                }

                if (sumOfSquares / count < sumOfSquaresDivided)
                {
                    sumOfSquaresDivided = sumOfSquares / count;
                    SelectedCarbody.Model.DirectionOfSymmetryPlane = Array.IndexOf(sideASelector, sideASelector.Min());
                }
            }

            var centerOfPlane = new Matrix3D(
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

            int[] topSelector = new int[3] { 0, 0, 0 };
            topSelector[directionOfTop] = directionOfTopShift;
            topSelector[directionOfFront] = directionOfFrontShift;

            int[] coordinateSelector = new int[3] { 0, 0, 0 };
            coordinateSelector[2] = directionOfTop;
            coordinateSelector[1] = directionOfFront;
            coordinateSelector[0] = Array.IndexOf(topSelector, 0);

            var centerOfTop = new Matrix3D(
                -SelectedCarbody.Model.BoundingBoxAxis[coordinateSelector[0]][0],
                -SelectedCarbody.Model.BoundingBoxAxis[coordinateSelector[0]][1],
                -SelectedCarbody.Model.BoundingBoxAxis[coordinateSelector[0]][2],
                0.0,
                topSelector[directionOfFront] * SelectedCarbody.Model.BoundingBoxAxis[coordinateSelector[1]][0],
                topSelector[directionOfFront] * SelectedCarbody.Model.BoundingBoxAxis[coordinateSelector[1]][1],
                topSelector[directionOfFront] * SelectedCarbody.Model.BoundingBoxAxis[coordinateSelector[1]][2],
                0.0,
                -topSelector[directionOfTop] * SelectedCarbody.Model.BoundingBoxAxis[coordinateSelector[2]][0],
                -topSelector[directionOfTop] * SelectedCarbody.Model.BoundingBoxAxis[coordinateSelector[2]][1],
                -topSelector[directionOfTop] * SelectedCarbody.Model.BoundingBoxAxis[coordinateSelector[2]][2],
                0.0,
                SelectedCarbody.Model.BoundingBoxPosition[0],
                SelectedCarbody.Model.BoundingBoxPosition[1],
                SelectedCarbody.Model.BoundingBoxPosition[2],
                1.0
            );
            /*
            var centerOfTop = new Matrix3D(
                BoundingBoxAxis[0][0],
                BoundingBoxAxis[0][1],
                BoundingBoxAxis[0][2],
                0.0,
                BoundingBoxAxis[1][0],
                BoundingBoxAxis[1][1],
                BoundingBoxAxis[1][2],
                0.0,
                BoundingBoxAxis[2][0],
                BoundingBoxAxis[2][1],
                BoundingBoxAxis[2][2],
                0.0,
                BoundingBoxPosition[0],
                BoundingBoxPosition[1],
                BoundingBoxPosition[2],
                1.0
                );

                        var matrixTranslationTop = new Vector3D(
             topSelector[0] * BoundingBoxHalfExtents[0],
             topSelector[1] * BoundingBoxHalfExtents[1],
             topSelector[2] * BoundingBoxHalfExtents[2]
                );


*/

            var matrixTranslationTop = new Vector3D(
                0,
                Math.Abs(SelectedCarbody.Model.BoundingBoxHalfExtents[directionOfFront]),
                -Math.Abs(SelectedCarbody.Model.BoundingBoxHalfExtents[directionOfTop])
            );

            centerOfTop.TranslatePrepend(matrixTranslationTop);

            // draw top
            var topCoordinate = new CoordinateSystemVisual3D() { ArrowLengths = 100.0 };
            topCoordinate.Transform = new MatrixTransform3D(centerOfTop);
            //viewportCarbody.Viewport.Children.Add(topCoordinate);

            //move carbody front to world     
            Console.WriteLine("Positoon vorher: " + SelectedCarbody.CarbodyModel.GetTransform().ToString());
            SelectedCarbody.Model.CarbodyModel.Transform = new MatrixTransform3D(centerOfTop.Inverse());
            //SelectedCarbody.Model.CarbodyModel.Transform = new MatrixTransform3D(centerOfPlane.Inverse());
            Console.WriteLine("Position nachher: " + SelectedCarbody.CarbodyModel.GetTransform().ToString());

            RaisePropertyChanged();
        }

        private void RayHi(Matrix3D matrixStart, Matrix3D matrixEnd, ref double k, ref double o, ref double p)
        {
            Point3D startPoint = new Point3D();
            Point3D endPoint = new Point3D();

            MatrixTransform3D transformToBoundingBoxStart = new MatrixTransform3D(matrixStart);
            MatrixTransform3D transformToBoundingBoxEnd = new MatrixTransform3D(matrixEnd);

            startPoint = transformToBoundingBoxStart.Transform(startPoint);
            endPoint = transformToBoundingBoxEnd.Transform(endPoint);

            // Only used to show start and end points of ray
            SelectedCarbody.Model.Add3DRayOrigin(startPoint);
            SelectedCarbody.Model.Add3DRayOrigin(endPoint);

            // Ray from side A
            RayHitTestParameters hitParams =
                new RayHitTestParameters(
                    startPoint,
                    new Vector3D(endPoint.X - startPoint.X, endPoint.Y - startPoint.Y, endPoint.Z - startPoint.Z)
                );
            double distanceFromStart = 0.0;
            VisualTreeHelper.HitTest(SelectedCarbody.CarbodyModel, null, h => HitTestResultCallback(ref distanceFromStart, h), hitParams);

            // Ray from side B
            hitParams =
                new RayHitTestParameters(
                    endPoint,
                    new Vector3D(startPoint.X - endPoint.X, startPoint.Y - endPoint.Y, startPoint.Z - endPoint.Z)
                );
            double distanceFromEnd = 0.0;
            VisualTreeHelper.HitTest(SelectedCarbody.CarbodyModel, null, h => HitTestResultCallback(ref distanceFromEnd, h), hitParams);

            k = Math.Abs(distanceFromStart - distanceFromEnd);
            o = distanceFromStart;
            p = distanceFromEnd;
        }

        private void RayHi(Matrix3D matrixStart, Matrix3D matrixEnd)
        {
            Point3D startPoint = new Point3D();
            Point3D endPoint = new Point3D();

            MatrixTransform3D transformToBoundingBoxStart = new MatrixTransform3D(matrixStart);
            MatrixTransform3D transformToBoundingBoxEnd = new MatrixTransform3D(matrixEnd);

            startPoint = transformToBoundingBoxStart.Transform(startPoint);
            endPoint = transformToBoundingBoxEnd.Transform(endPoint);

            // Only used to show start and end points of ray
            SelectedCarbody.Model.Add3DRayOrigin(startPoint);
            SelectedCarbody.Model.Add3DRayOrigin(endPoint);

            // Ray from side A
            RayHitTestParameters hitParams =
                new RayHitTestParameters(
                    startPoint,
                    new Vector3D(endPoint.X - startPoint.X, endPoint.Y - startPoint.Y, endPoint.Z - startPoint.Z)
                );
            VisualTreeHelper.HitTest(SelectedCarbody.CarbodyModel, null, HitTestResultCallback, hitParams);

            // Ray from side B
            hitParams =
                new RayHitTestParameters(
                    endPoint,
                    new Vector3D(startPoint.X - endPoint.X, startPoint.Y - endPoint.Y, startPoint.Z - endPoint.Z)
                );
            VisualTreeHelper.HitTest(SelectedCarbody.CarbodyModel, null, HitTestResultCallback, hitParams);
        }

        #endregion
    }
}