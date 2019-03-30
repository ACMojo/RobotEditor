using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using VirtualRobotWrapper;
using Microsoft.Win32;
using System.Xml;
using MathGeoLibWrapper;
using RobotEditor.Model;
using RobotEditor.View;

namespace RobotEditor.ViewModel
{
    internal class MainWindowViewModel : BaseViewModel
    {
        private RobotViewModel _selectedRobot;
        private CarbodyViewModel _selectedCarbody;
        private Booth booth;
        private VoxelOctree octreeTemp;
        private readonly IHelixViewport3D viewportCarbody;
        private readonly IHelixViewport3D viewportRobot;

        public MainWindowViewModel(HelixViewport3D viewportCarbody, HelixViewport3D viewportRobot)
        {
            CreateXML = new DelegateCommand<object>(CreateXMLExecute, CreateXMLCanExecute);
            AddCarbody = new DelegateCommand<object>(AddCarbodyExecute, AddCarbodyCanExecute);
            Compare = new DelegateCommand<object>(CompareExecute, CompareCanExecute);
            DeleteCarbody = new DelegateCommand<object>(DeleteCarbodyExecute, DeleteCarbodyCanExecute);
            AddRobot = new DelegateCommand<object>(AddRobotExecute, AddRobotCanExecute);
            DeleteRobot = new DelegateCommand<object>(DeleteRobotExecute, DeleteRobotCanExecute);
            FitToView = new DelegateCommand<object>(FitToViewExecute, FitToViewCanExecute);
            EditRobot = new DelegateCommand<object>(EditRobotExecute, EditRobotCanExecute);
            HitPoints = new DelegateCommand<object>(HitPointsExecute, HitPointsCanExecute);
            RayOrigins = new DelegateCommand<object>(RayOriginsExecute, RayOriginsCanExecute);
            BoundingBox = new DelegateCommand<object>(BoundingBoxExecute, BoundingBoxCanExecute);
            SymmetryPlane = new DelegateCommand<object>(SymmetryPlaneExecute, SymmetryPlaneCanExecute);

            this.viewportCarbody = viewportCarbody;
            this.viewportRobot = viewportRobot;
            booth = new Booth(10000, 100d);

            CarbodyModels.Add(new CoordinateSystemVisual3D() { ArrowLengths = 100.0 });
            CarbodyModels.Add(new DefaultLights());

            RobotModels.Add(new DefaultLights());
            RobotModels.Add(new CoordinateSystemVisual3D() { ArrowLengths = 100.0 });         

            RaisePropertyChanged();
            
        }

        public ObservableCollection<RobotViewModel> Robots { get; } = new ObservableCollection<RobotViewModel>();
        public ObservableCollection<CarbodyViewModel> Carbodies { get; } = new ObservableCollection<CarbodyViewModel>();
        public bool IsCheckedHitPoints { get; set; }
        public bool IsCheckedRayOrigins { get; set; }
        public bool IsCheckedBoundingBox { get; set; }
        public bool IsCheckedSymmetryPlane { get; set; }


        #region Public delegates

        public DelegateCommand<object> CreateXML { get; }
        public DelegateCommand<object> HitPoints { get; }
        public DelegateCommand<object> RayOrigins { get; }
        public DelegateCommand<object> AddRobot { get; }
        public DelegateCommand<object> DeleteRobot { get; }
        public DelegateCommand<object> Compare { get; }
        public DelegateCommand<object> AddCarbody { get; }
        public DelegateCommand<object> DeleteCarbody { get; }
        public DelegateCommand<object> FitToView { get; }
        public DelegateCommand<object> EditRobot { get; }
        public DelegateCommand<object> BoundingBox { get; }
        public DelegateCommand<object> SymmetryPlane { get; }

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
            {
                SelectedCarbody.Model.show3DBoundingBoxGeometry();
            }
            else
            {
                SelectedCarbody.Model.hide3DBoundingBoxGeometry();
            }
        }

        private bool CompareCanExecute(object arg)
        {
            return (Robots.Count > 0 && Carbodies.Count > 0);
        }

        private bool SymmetryPlaneCanExecute(object arg)
        {
            return SelectedCarbody != null;
        }

        private void SymmetryPlaneExecute(object obj)
        {
            if (IsCheckedSymmetryPlane)
            {
                SelectedCarbody.Model.show3DSymmetryPlaneGeometry();
            }
            else
            {
                SelectedCarbody.Model.hide3DSymmetryPlaneGeometry();
            }
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
            {
                SelectedCarbody.Model.show3DHitPointGeometries();
            }
            else
            {
                SelectedCarbody.Model.hide3DHitPointGeometries();
            }
        }

        private bool RayOriginsCanExecute(object arg)
        {
            if (SelectedCarbody == null)
                return false;

            return SelectedCarbody.Model.RayOrigins.Count > 0;
        }

        private void RayOriginsExecute(object obj)
        {
            if(IsCheckedRayOrigins)
            {
                SelectedCarbody.Model.show3DRayOriginGeometries();
            }
            else
            {
                SelectedCarbody.Model.hide3DRayOriginGeometries();
            }
        }

        private void FitToViewExecute(object obj)
        {
            viewportCarbody.ZoomExtents(0);
            RaisePropertyChanged();
        }

        private bool FitToViewCanExecute(object arg)
        {
            return true;
        }

        #endregion





        private void CompareExecute(object obj)
        {
            
            foreach(var carbody in Carbodies)
            {
                SelectedCarbody = carbody;

               
                octreeTemp = VoxelOctree.Create(Math.Abs(SelectedCarbody.Model.OBBCalculator.HalfExtents.Max()) * 4, 100d);

                this.viewportCarbody.ZoomExtents(0);
                

                // Perform hit test
                
                for(int m = 0; m<3; m++)
                {
                    int[,] directionSelector = new int[,] { { 1, 2 }, { 0, 2 }, { 0, 1 } };
                    int[] factor = new int[3] { 1, 1, 1 };
                    int[] factor2 = new int[3] { 0, 0, 0 };
                    int[] factor3 = new int[3] { 0, 0, 0 };
                    factor[m] = -1;

                    var matrixStart = new Matrix3D(
                                         SelectedCarbody.Model.OBBCalculator.Axis[0][0],
                                         SelectedCarbody.Model.OBBCalculator.Axis[0][1],
                                         SelectedCarbody.Model.OBBCalculator.Axis[0][2],
                                         0.0,
                                         SelectedCarbody.Model.OBBCalculator.Axis[1][0],
                                         SelectedCarbody.Model.OBBCalculator.Axis[1][1],
                                         SelectedCarbody.Model.OBBCalculator.Axis[1][2],
                                         0.0,
                                         SelectedCarbody.Model.OBBCalculator.Axis[2][0],
                                         SelectedCarbody.Model.OBBCalculator.Axis[2][1],
                                         SelectedCarbody.Model.OBBCalculator.Axis[2][2],
                                         0.0,
                                         SelectedCarbody.Model.OBBCalculator.Position[0],
                                         SelectedCarbody.Model.OBBCalculator.Position[1],
                                         SelectedCarbody.Model.OBBCalculator.Position[2],
                                         1.0
                                         );

                    var matrixEnd = new Matrix3D(
                                         SelectedCarbody.Model.OBBCalculator.Axis[0][0],
                                         SelectedCarbody.Model.OBBCalculator.Axis[0][1],
                                         SelectedCarbody.Model.OBBCalculator.Axis[0][2],
                                         0.0,
                                         SelectedCarbody.Model.OBBCalculator.Axis[1][0],
                                         SelectedCarbody.Model.OBBCalculator.Axis[1][1],
                                         SelectedCarbody.Model.OBBCalculator.Axis[1][2],
                                         0.0,
                                         SelectedCarbody.Model.OBBCalculator.Axis[2][0],
                                         SelectedCarbody.Model.OBBCalculator.Axis[2][1],
                                         SelectedCarbody.Model.OBBCalculator.Axis[2][2],
                                         0.0,
                                         SelectedCarbody.Model.OBBCalculator.Position[0],
                                         SelectedCarbody.Model.OBBCalculator.Position[1],
                                         SelectedCarbody.Model.OBBCalculator.Position[2],
                                         1.0
                                         );

                    var matrixTranslationStart = new Vector3D(
                                     SelectedCarbody.Model.OBBCalculator.HalfExtents[0],
                                     SelectedCarbody.Model.OBBCalculator.HalfExtents[1],
                                     SelectedCarbody.Model.OBBCalculator.HalfExtents[2]
                                    );

                    var matrixTranslationEnd = new Vector3D(
                                 factor[0] * SelectedCarbody.Model.OBBCalculator.HalfExtents[0],
                                 factor[1] * SelectedCarbody.Model.OBBCalculator.HalfExtents[1],
                                 factor[2] * SelectedCarbody.Model.OBBCalculator.HalfExtents[2]
                                 );

                    matrixStart.TranslatePrepend(matrixTranslationStart);
                    matrixEnd.TranslatePrepend(matrixTranslationEnd);

                    Vector3D vector;

                    for (int j = 0; j < (Math.Abs(SelectedCarbody.Model.OBBCalculator.HalfExtents[directionSelector[m,0]]) * 2) / 100.0; j++)
                    {
                        var total = 0;
                        for (int k = 0; k < (Math.Abs(SelectedCarbody.Model.OBBCalculator.HalfExtents[directionSelector[m,1]]) * 2) / 100.0; k++)
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


                booth.Octree.Add(octreeTemp);
            }


            //octree = VoxelOctree.Create(400, 100d);
            //octree.Set(-790, -790, -790, 1);
            //octree.Set(-1, -1, -1, 2);
            //octree.Set(790, -790, -790, 1);
            //octree.Set(1, -1, -1, 2);
            
            var comparison = new ResultWindow(booth.Octree);
            var result = comparison.ShowDialog();

            if (result == true)
            {
                ;
            }


            if (result != true)
                return;
        }


        public RobotViewModel SelectedRobot
        {
            get { return _selectedRobot; }
            set
            {
                if (Equals(value, _selectedRobot))
                    return;

                if (_selectedRobot != null)
                {
                    RobotModels.Remove(_selectedRobot.robotModel);
                }

                _selectedRobot = value;

                if (_selectedRobot != null)
                {
                    RobotModels.Add(_selectedRobot.robotModel);
                }

                RaisePropertyChanged();

                DeleteRobot.RaisePropertyChanged();
                
                CreateXML.RaisePropertyChanged();
                EditRobot.RaisePropertyChanged();
                Compare.RaisePropertyChanged();

                this.viewportRobot.ZoomExtents(0);
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
                    _selectedCarbody.Model.hide3DHitPointGeometries();
                    _selectedCarbody.Model.hide3DRayOriginGeometries();
                    _selectedCarbody.Model.hide3DBoundingBoxGeometry();
                    _selectedCarbody.Model.hide3DSymmetryPlaneGeometry();
                    CarbodyModels.Remove(_selectedCarbody.carbodyModel);
                    CarbodyModels.Remove(_selectedCarbody.boundingBox);
                }

                _selectedCarbody = value;

                if (_selectedCarbody != null)
                {
                    if (IsCheckedHitPoints)
                        _selectedCarbody.Model.show3DHitPointGeometries();
                    if (IsCheckedRayOrigins)
                        _selectedCarbody.Model.show3DRayOriginGeometries();
                    if (IsCheckedBoundingBox)
                        _selectedCarbody.Model.show3DBoundingBoxGeometry();
                    if (IsCheckedSymmetryPlane)
                        _selectedCarbody.Model.show3DSymmetryPlaneGeometry();

                    CarbodyModels.Add(_selectedCarbody.carbodyModel);
                }

                RaisePropertyChanged();

                DeleteCarbody.RaisePropertyChanged();
                HitPoints.RaisePropertyChanged();
                RayOrigins.RaisePropertyChanged();
                BoundingBox.RaisePropertyChanged();
                SymmetryPlane.RaisePropertyChanged();
                Compare.RaisePropertyChanged();

                this.viewportCarbody.ZoomExtents(0);
            }
        }

        public ObservableCollection<Visual3D> CarbodyModels { get; } = new ObservableCollection<Visual3D>();
        public ObservableCollection<Visual3D> RobotModels { get; } = new ObservableCollection<Visual3D>();



        private void AddRobotExecute(object obj)
        {
            var robot = new Robot(Robot.RobotTypes.Puma560);
            var newRobot = new RobotValues { DataContext = new RobotViewModel(robot) };
            var result = newRobot.ShowDialog();

            if (result == true)
            {
                //var coordinateSystem = new CoordinateSystemVisual3D();
                //var baseCoordinateSystem = coordinateSystem;

                //baseCoordinateSystem.XAxisColor = Colors.Yellow;
                //baseCoordinateSystem.YAxisColor = Colors.Yellow;
                //baseCoordinateSystem.ZAxisColor = Colors.Yellow;

                //baseCoordinateSystem.ArrowLengths = 100.0;

                //int i = 0;
                //foreach (Joint joint in robot.Joints)
                //{
                //    i++;
                //    var interimCS = new CoordinateSystemVisual3D();

                //    if (i == robot.Joints.Count)
                //    {
                //        interimCS.XAxisColor = Colors.Magenta;
                //        interimCS.YAxisColor = Colors.Magenta;
                //        interimCS.ZAxisColor = Colors.Magenta;
                //    }
                //    interimCS.ArrowLengths = 100.0;

                //    var DH_Matrix = new Matrix3D(
                //        Math.Cos(degreeToRadian(joint.theta)),
                //        Math.Sin(degreeToRadian(joint.theta)),
                //        0.0,
                //        0.0,
                //        -Math.Sin(degreeToRadian(joint.theta)) * Math.Cos(degreeToRadian(joint.alpha)),
                //        Math.Cos(degreeToRadian(joint.theta)) * Math.Cos(degreeToRadian(joint.alpha)),
                //        Math.Sin(degreeToRadian(joint.alpha)),
                //        0.0,
                //        Math.Sin(degreeToRadian(joint.theta)) * Math.Sin(degreeToRadian(joint.alpha)),
                //        -Math.Cos(degreeToRadian(joint.theta)) * Math.Sin(degreeToRadian(joint.alpha)),
                //        Math.Cos(degreeToRadian(joint.alpha)),
                //        0.0,
                //        joint.a * Math.Cos(degreeToRadian(joint.theta)),
                //        joint.a * Math.Sin(degreeToRadian(joint.theta)),
                //        joint.d,
                //        1.0
                //        );

                //    interimCS.Transform = new MatrixTransform3D(DH_Matrix);

                //    LinesVisual3D line = new LinesVisual3D();
                //    line.Thickness = 5.0;
                //    Point3DCollection PCollection = new Point3DCollection();
                //    PCollection.Add(new Point3D(0.0, 0.0, 0.0));
                //    PCollection.Add(new Point3D(joint.a * Math.Cos(degreeToRadian(joint.theta)), joint.a * Math.Sin(degreeToRadian(joint.theta)), joint.d));
                //    line.Color = Colors.Gray;
                //    line.Points = PCollection;
                //    coordinateSystem.Children.Add(line);

                //    coordinateSystem.Children.Add(interimCS);

                //    coordinateSystem = interimCS;

                //}

                //robot.RobotModel.Children.Add(baseCoordinateSystem);

                robot.RobotModel.Children.Clear();
                drawRobotModel(robot);
                //drawVoxelMap(robot);
                Robots.Add(new RobotViewModel(robot));
            }

            this.viewportRobot.ZoomExtents(0);

            if (result != true)
                return;
        }

        private void drawRobotModel(Robot robot)
        {
            var coordinateSystem = new CoordinateSystemVisual3D();
            var baseCoordinateSystem = coordinateSystem;

            baseCoordinateSystem.XAxisColor = Colors.Yellow;
            baseCoordinateSystem.YAxisColor = Colors.Yellow;
            baseCoordinateSystem.ZAxisColor = Colors.Yellow;

            baseCoordinateSystem.ArrowLengths = 100.0;

            int i = 0;
            foreach (Joint joint in robot.Joints)
            {
                i++;
                var interimCS = new CoordinateSystemVisual3D();

                if (i == robot.Joints.Count)
                {
                    interimCS.XAxisColor = Colors.Magenta;
                    interimCS.YAxisColor = Colors.Magenta;
                    interimCS.ZAxisColor = Colors.Magenta;
                }
                interimCS.ArrowLengths = 100.0;

                var DH_Matrix = new Matrix3D(
                    Math.Cos(degreeToRadian(joint.theta)),
                    Math.Sin(degreeToRadian(joint.theta)),
                    0.0,
                    0.0,
                    -Math.Sin(degreeToRadian(joint.theta)) * Math.Cos(degreeToRadian(joint.alpha)),
                    Math.Cos(degreeToRadian(joint.theta)) * Math.Cos(degreeToRadian(joint.alpha)),
                    Math.Sin(degreeToRadian(joint.alpha)),
                    0.0,
                    Math.Sin(degreeToRadian(joint.theta)) * Math.Sin(degreeToRadian(joint.alpha)),
                    -Math.Cos(degreeToRadian(joint.theta)) * Math.Sin(degreeToRadian(joint.alpha)),
                    Math.Cos(degreeToRadian(joint.alpha)),
                    0.0,
                    joint.a * Math.Cos(degreeToRadian(joint.theta)),
                    joint.a * Math.Sin(degreeToRadian(joint.theta)),
                    joint.d,
                    1.0
                );

                interimCS.Transform = new MatrixTransform3D(DH_Matrix);

                LinesVisual3D line = new LinesVisual3D();
                line.Thickness = 5.0;
                Point3DCollection PCollection = new Point3DCollection();
                PCollection.Add(new Point3D(0.0, 0.0, 0.0));
                PCollection.Add(new Point3D(joint.a * Math.Cos(degreeToRadian(joint.theta)), joint.a * Math.Sin(degreeToRadian(joint.theta)), joint.d));
                line.Color = Colors.Gray;
                line.Points = PCollection;
                coordinateSystem.Children.Add(line);
                
                coordinateSystem.Children.Add(interimCS);

                coordinateSystem = interimCS;
            }

            //robot.RobotModel.Children.Clear();
            robot.RobotModel.Children.Add(baseCoordinateSystem);
        }

        private void drawVoxelMap(Robot robot)
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

                var PointTest = rayMeshResult.PointHit;
                PointTest = Point3D.Multiply(PointTest, SelectedCarbody.Model.CarbodyModel.GetTransform());
                octreeTemp.Set((int)PointTest.X, (int)PointTest.Y, (int)PointTest.Z, 1.0);

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
                RobotModels.Remove(_selectedRobot.robotModel);
                //_selectedRobot.robotModel = new ModelVisual3D();
                drawRobotModel(_selectedRobot.Model);
                RobotModels.Add(_selectedRobot.robotModel);
                this.viewportRobot.ZoomExtents(0);
            }
        }

        private double degreeToRadian(double angle)
        {
            return (Math.PI / 180) * angle;
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

        private void CreateXMLExecute(object obj)
        {
            SelectedRobot.Model.SaveRobotStructur();

            float[] maxB;
            float[] minB;
            float maxManip;

            using (var VrManip = new VirtualRobotManipulability())
            {
                String path = AppDomain.CurrentDomain.BaseDirectory + "/" + SelectedRobot.Model.Name;
                if (VrManip.Init(0, null, @path, "robotNodeSet", "root", "tcp"))
                {
                    ManipulabilityVoxel[] vox = VrManip.GetManipulability((float)100.0, (float)(Math.PI / 2), 100000, false, false, true, 50f);


                    minB = VrManip.MinBox;
                    maxB = VrManip.MaxBox;

                    // Calc size of cube depending on reachability of robot
                    double octreeSize;
                    if (Math.Abs(VrManip.MaxBox.Max()) > Math.Abs(VrManip.MinBox.Max()))
                    {
                        octreeSize = Math.Abs(VrManip.MaxBox.Max())*2;
                    }
                    else
                    {
                        octreeSize = Math.Abs(VrManip.MinBox.Max())*2;
                    }

                    SelectedRobot.Model.Octree = VoxelOctree.Create(octreeSize, 100.0);

                    maxManip = VrManip.MaxManipulability;


                    ManipulabilityVoxel voxOld = vox[0];
                    double maxValue = vox[0].value;
                    for (int j = 1; j < vox.Length; j++)
                    {
                        // TODO: MaxWert gewichten, je nach Drehung zwsichen Roboter und Fahrzeug

                        if (vox[j].x == voxOld.x && vox[j].y == voxOld.y && vox[j].z == voxOld.z)
                        {
                            if (vox[j].value > maxValue)
                            {
                                maxValue = vox[j].value;
                            }
                        }                       
                        else
                        {
                            if (!SelectedRobot.Model.Octree.Set((int)(minB[0] + voxOld.x*100), (int)(minB[1] + voxOld.y*100), (int)(minB[2] + voxOld.z*100), maxValue))
                            {
                                    
                                var value = booth.Octree.Get((int)Math.Floor((minB[0] / 100.0) + voxOld.x), (int)Math.Floor((minB[1] / 100.0) + voxOld.y), (int)Math.Floor((minB[2] / 100.0) + voxOld.z));
                                if (double.IsNaN(value))
                                    Console.WriteLine($"Nicht erfolgreich bei: { Math.Floor((minB[0] / 100.0) + voxOld.x) } { Math.Floor((minB[1] / 100.0) + voxOld.y) } { Math.Floor((minB[2] / 100.0) + voxOld.z) }");
                                else
                                    ;
                            }

                            voxOld = vox[j];
                            maxValue = vox[j].value;
                        }
                    }

                    int i = SelectedRobot.Model.Octree.StartIndexLeafNodes - 1;
                    int maxValu = 0;

                    for (int h = SelectedRobot.Model.Octree.StartIndexPerLevel[SelectedRobot.Model.Octree.Level - 2]; h < SelectedRobot.Model.Octree.StartIndexPerLevel[SelectedRobot.Model.Octree.Level - 1]; h++)
                    {
                        if (SelectedRobot.Model.Octree.Nodes[h] == null)
                            continue;

                        if (((VoxelNodeInner)SelectedRobot.Model.Octree.Nodes[h]).Max > maxValu)
                        {
                            maxValu = (int)((VoxelNodeInner)SelectedRobot.Model.Octree.Nodes[h]).Max;

                        }
                    }


                    foreach (var node in SelectedRobot.Model.Octree.GetLeafNodes())
                    {
                        i++;
                        if (node == null)
                            continue;


                        var vm = new MeshGeometryVisual3D();
                        var mb = new MeshBuilder();
                        var StartOffset = new Point3D(0, 0, 0);

                        int n = 0;
                        int k = i;
                        for (int w = 0; w < SelectedRobot.Model.Octree.Level; w++)
                        {
                            n = (k - SelectedRobot.Model.Octree.StartIndexPerLevel[SelectedRobot.Model.Octree.Level - 1 - w]) % 8;



                            switch (n)
                            {
                                case 0:
                                    StartOffset.X = StartOffset.X + (Math.Pow(2, w) * (100 / 2));
                                    StartOffset.Y = StartOffset.Y - (Math.Pow(2, w) * (100 / 2));
                                    StartOffset.Z = StartOffset.Z - (Math.Pow(2, w) * (100 / 2));
                                    break;
                                case 1:
                                    StartOffset.X = StartOffset.X + (Math.Pow(2, w) * (100 / 2));
                                    StartOffset.Y = StartOffset.Y + (Math.Pow(2, w) * (100 / 2));
                                    StartOffset.Z = StartOffset.Z - (Math.Pow(2, w) * (100 / 2));
                                    break;
                                case 2:
                                    StartOffset.X = StartOffset.X - (Math.Pow(2, w) * (100 / 2));
                                    StartOffset.Y = StartOffset.Y - (Math.Pow(2, w) * (100 / 2));
                                    StartOffset.Z = StartOffset.Z - (Math.Pow(2, w) * (100 / 2));
                                    break;
                                case 3:
                                    StartOffset.X = StartOffset.X - (Math.Pow(2, w) * (100 / 2));
                                    StartOffset.Y = StartOffset.Y + (Math.Pow(2, w) * (100 / 2));
                                    StartOffset.Z = StartOffset.Z - (Math.Pow(2, w) * (100 / 2));
                                    break;
                                case 4:
                                    StartOffset.X = StartOffset.X + (Math.Pow(2, w) * (100 / 2));
                                    StartOffset.Y = StartOffset.Y - (Math.Pow(2, w) * (100 / 2));
                                    StartOffset.Z = StartOffset.Z + (Math.Pow(2, w) * (100 / 2));
                                    break;
                                case 5:
                                    StartOffset.X = StartOffset.X + (Math.Pow(2, w) * (100 / 2));
                                    StartOffset.Y = StartOffset.Y + (Math.Pow(2, w) * (100 / 2));
                                    StartOffset.Z = StartOffset.Z + (Math.Pow(2, w) * (100 / 2));
                                    break;
                                case 6:
                                    StartOffset.X = StartOffset.X - (Math.Pow(2, w) * (100 / 2));
                                    StartOffset.Y = StartOffset.Y - (Math.Pow(2, w) * (100 / 2));
                                    StartOffset.Z = StartOffset.Z + (Math.Pow(2, w) * (100 / 2));
                                    break;
                                case 7:
                                    StartOffset.X = StartOffset.X - (Math.Pow(2, w) * (100 / 2));
                                    StartOffset.Y = StartOffset.Y + (Math.Pow(2, w) * (100 / 2));
                                    StartOffset.Z = StartOffset.Z + (Math.Pow(2, w) * (100 / 2));
                                    break;
                                default:
                                    Console.WriteLine("Fehler");
                                    break;
                            }
                            if (w < SelectedRobot.Model.Octree.Level - 1)
                            {
                                k = SelectedRobot.Model.Octree.StartIndexPerLevel[SelectedRobot.Model.Octree.Level - 2 - w] + ((k - SelectedRobot.Model.Octree.StartIndexPerLevel[SelectedRobot.Model.Octree.Level - 1 - w]) / 8);
                            }
                            //k = (int)Math.Floor((double)k / 8);

                        }

                        /*
                        switch (i%8)
                        {
                            case 0:
                                vm.Material = MaterialHelper.CreateMaterial(Colors.Red);
                                break;
                            case 1:
                                vm.Material = MaterialHelper.CreateMaterial(Colors.Purple);
                                break;
                            case 2:
                                vm.Material = MaterialHelper.CreateMaterial(Colors.Gray);
                                break;
                            case 3:
                                vm.Material = MaterialHelper.CreateMaterial(Colors.Green);
                                break;
                            case 4:
                                vm.Material = MaterialHelper.CreateMaterial(Colors.Orange);
                                break;
                            case 5:
                                vm.Material = MaterialHelper.CreateMaterial(Colors.DarkBlue);
                                break;
                            case 6:
                                vm.Material = MaterialHelper.CreateMaterial(Colors.LightBlue);
                                break;
                            case 7:
                                vm.Material = MaterialHelper.CreateMaterial(Colors.Yellow);
                                break;
                            default:
                                Console.WriteLine("Fehler");
                                break;
                        }
                        */

                        if (node != null)
                        {
                            vm.Material = MaterialHelper.CreateMaterial(ColorGradient.GetColorForValue(node.Value, maxValu, 1.0));
                            mb.AddBox(new Point3D(StartOffset.X, StartOffset.Y, StartOffset.Z), 25.0, 25.0, 25.0);
                            vm.MeshGeometry = mb.ToMesh();
                            viewportRobot.Viewport.Children.Add(vm);

                        }
                    }
                }

            }

            RaisePropertyChanged();
        }

        private bool CreateXMLCanExecute(object arg)
        {
            return SelectedRobot != null;
        }

        private void AddCarbodyExecute(object obj)
        {
            var FileDialog = new OpenFileDialog();
            if (FileDialog.ShowDialog() == true)
            {
                var mi = new ModelImporter();
                var carbody = new Carbody(
                    FileDialog.FileName,
                    FileDialog.SafeFileName,
                    new ModelVisual3D { Content = mi.Load(FileDialog.FileName, null, true) });

                SelectedCarbody = new CarbodyViewModel(carbody);
                 Carbodies.Add(SelectedCarbody);
                
                if (SelectedCarbody != null)
                {
                    CalcRefPosition();
                }
            }
        }

        private void DeleteCarbodyExecute(object obj)
        {
            Carbodies.Remove(_selectedCarbody);

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
                                     OBBCalculator.Axis[0][0],
                                     OBBCalculator.Axis[0][1],
                                     OBBCalculator.Axis[0][2],
                                     0.0,
                                     OBBCalculator.Axis[1][0],
                                     OBBCalculator.Axis[1][1],
                                     OBBCalculator.Axis[1][2],
                                     0.0,
                                     OBBCalculator.Axis[2][0],
                                     OBBCalculator.Axis[2][1],
                                     OBBCalculator.Axis[2][2],
                                     0.0,
                                     OBBCalculator.Position[0],
                                     OBBCalculator.Position[1],
                                     OBBCalculator.Position[2],
                                     1.0
                                     );

                var matrixEnd = new Matrix3D(
                                     OBBCalculator.Axis[0][0],
                                     OBBCalculator.Axis[0][1],
                                     OBBCalculator.Axis[0][2],
                                     0.0,
                                     OBBCalculator.Axis[1][0],
                                     OBBCalculator.Axis[1][1],
                                     OBBCalculator.Axis[1][2],
                                     0.0,
                                     OBBCalculator.Axis[2][0],
                                     OBBCalculator.Axis[2][1],
                                     OBBCalculator.Axis[2][2],
                                     0.0,
                                     OBBCalculator.Position[0],
                                     OBBCalculator.Position[1],
                                     OBBCalculator.Position[2],
                                     1.0
                                     );

                var matrixTranslationStart = new Vector3D(
                                 OBBCalculator.HalfExtents[0],
                                 OBBCalculator.HalfExtents[1],
                                 OBBCalculator.HalfExtents[2]
                                );

                var matrixTranslationEnd = new Vector3D(
                             factor[0] * OBBCalculator.HalfExtents[0],
                             factor[1] * OBBCalculator.HalfExtents[1],
                             factor[2] * OBBCalculator.HalfExtents[2]
                             );

                matrixStart.TranslatePrepend(matrixTranslationStart);
                matrixEnd.TranslatePrepend(matrixTranslationEnd);

                Vector3D vector;

                for (int j = 0; j < (Math.Abs(OBBCalculator.HalfExtents[directionSelector[m,0]]) * 2) / 100.0; j++)
                {
                    var total = 0;
                    for (int k = 0; k < (Math.Abs(OBBCalculator.HalfExtents[directionSelector[m,1]]) * 2) / 100.0; k++)
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
                if (Array.IndexOf(SelectedCarbody.Model.OBBCalculator.HalfExtents, SelectedCarbody.Model.OBBCalculator.HalfExtents.Max()) == m)
                {
                    z = m + 1;
                }
                if (m == 1 && Array.IndexOf(SelectedCarbody.Model.OBBCalculator.HalfExtents, SelectedCarbody.Model.OBBCalculator.HalfExtents.Max()) == 0)
                {
                    z = 2;
                }
                sideASelector[Array.IndexOf(SelectedCarbody.Model.OBBCalculator.HalfExtents, SelectedCarbody.Model.OBBCalculator.HalfExtents.Max())] = 1;
                sideBSelector[Array.IndexOf(SelectedCarbody.Model.OBBCalculator.HalfExtents, SelectedCarbody.Model.OBBCalculator.HalfExtents.Max())] = 1;
                sideASelector[z] = 1;
                sideBSelector[z] = -1;

                var matrixStart = new Matrix3D(
                                     SelectedCarbody.Model.OBBCalculator.Axis[0][0],
                                     SelectedCarbody.Model.OBBCalculator.Axis[0][1],
                                     SelectedCarbody.Model.OBBCalculator.Axis[0][2],
                                     0.0,
                                     SelectedCarbody.Model.OBBCalculator.Axis[1][0],
                                     SelectedCarbody.Model.OBBCalculator.Axis[1][1],
                                     SelectedCarbody.Model.OBBCalculator.Axis[1][2],
                                     0.0,
                                     SelectedCarbody.Model.OBBCalculator.Axis[2][0],
                                     SelectedCarbody.Model.OBBCalculator.Axis[2][1],
                                     SelectedCarbody.Model.OBBCalculator.Axis[2][2],
                                     0.0,
                                     SelectedCarbody.Model.OBBCalculator.Position[0],
                                     SelectedCarbody.Model.OBBCalculator.Position[1],
                                     SelectedCarbody.Model.OBBCalculator.Position[2],
                                     1.0
                                     );

                var matrixEnd = new Matrix3D(
                                     SelectedCarbody.Model.OBBCalculator.Axis[0][0],
                                     SelectedCarbody.Model.OBBCalculator.Axis[0][1],
                                     SelectedCarbody.Model.OBBCalculator.Axis[0][2],
                                     0.0,
                                     SelectedCarbody.Model.OBBCalculator.Axis[1][0],
                                     SelectedCarbody.Model.OBBCalculator.Axis[1][1],
                                     SelectedCarbody.Model.OBBCalculator.Axis[1][2],
                                     0.0,
                                     SelectedCarbody.Model.OBBCalculator.Axis[2][0],
                                     SelectedCarbody.Model.OBBCalculator.Axis[2][1],
                                     SelectedCarbody.Model.OBBCalculator.Axis[2][2],
                                     0.0,
                                     SelectedCarbody.Model.OBBCalculator.Position[0],
                                     SelectedCarbody.Model.OBBCalculator.Position[1],
                                     SelectedCarbody.Model.OBBCalculator.Position[2],
                                     1.0
                                     );

                var matrixTranslationStart = new Vector3D(
                             sideASelector[0] * SelectedCarbody.Model.OBBCalculator.HalfExtents[0],
                             sideASelector[1] * SelectedCarbody.Model.OBBCalculator.HalfExtents[1],
                             sideASelector[2] * SelectedCarbody.Model.OBBCalculator.HalfExtents[2]
                                );

                var matrixTranslationEnd = new Vector3D(
                             sideBSelector[0] * SelectedCarbody.Model.OBBCalculator.HalfExtents[0],
                             sideBSelector[1] * SelectedCarbody.Model.OBBCalculator.HalfExtents[1],
                             sideBSelector[2] * SelectedCarbody.Model.OBBCalculator.HalfExtents[2]
                             );

                matrixStart.TranslatePrepend(matrixTranslationStart);
                matrixEnd.TranslatePrepend(matrixTranslationEnd);


                double[] savor = new double[(int)(Math.Abs(SelectedCarbody.Model.OBBCalculator.HalfExtents.Max() * 2 / 50.0) + 1)];
                double[] savor2 = new double[(int)(Math.Abs(SelectedCarbody.Model.OBBCalculator.HalfExtents.Max() * 2 / 50.0) + 1)];
                double[] savor3 = new double[(int)(Math.Abs(SelectedCarbody.Model.OBBCalculator.HalfExtents.Max() * 2 / 50.0) + 1)];
                double[] savor4 = new double[(int)(Math.Abs(SelectedCarbody.Model.OBBCalculator.HalfExtents.Max() * 2 / 50.0) + 1)];


                for (int k = 0; k < Math.Abs(SelectedCarbody.Model.OBBCalculator.HalfExtents.Max() * 2 / 50); k++)
                {
                    savor[k] = double.NaN;
                    RayHi(matrixStart, matrixEnd, ref savor[k], ref savor2[k], ref savor3[k]);

                    stepDirection[Array.IndexOf(SelectedCarbody.Model.OBBCalculator.HalfExtents, SelectedCarbody.Model.OBBCalculator.HalfExtents.Max())] = 1;
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
                    {
                        savor4[q] = (SelectedCarbody.Model.OBBCalculator.HalfExtents[z] * 2) - savor2[q] - savor3[q];
                    }
                }
                if (savor4.Average() < 100.0)
                {
                    directionOfTop = z;
                    double totalFirst = 0.0;
                    double totalLast = 0.0;
                    directionOfFront = Array.IndexOf(SelectedCarbody.Model.OBBCalculator.HalfExtents, SelectedCarbody.Model.OBBCalculator.HalfExtents.Max());
                    if (savor2.Max() > savor3.Max())
                    {
                        directionOfTopShift = 1;

                        for (int r = 0; r < savor3.Length / 3; r++)
                        {
                            totalFirst += savor3[r];
                            totalLast += savor3[savor3.Length - 1 - r];
                        }
                        if (totalFirst > totalLast)
                        {
                            directionOfFrontShift = 1;
                        }
                        else
                        {
                            directionOfFrontShift = -1;
                        }
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
                        {
                            directionOfFrontShift = 1;
                        }
                        else
                        {
                            directionOfFrontShift = -1;
                        }
                    }
                }


                if ((sumOfSquares / count) < sumOfSquaresDivided)
                {
                    sumOfSquaresDivided = (sumOfSquares / count);
                    SelectedCarbody.Model.DirectionOfSymmetryPlane = Array.IndexOf(sideASelector, sideASelector.Min()); 
                }

            }

            var centerOfPlane = new Matrix3D(
                SelectedCarbody.Model.OBBCalculator.Axis[0][0],
                SelectedCarbody.Model.OBBCalculator.Axis[0][1],
                SelectedCarbody.Model.OBBCalculator.Axis[0][2],
                0.0,
                SelectedCarbody.Model.OBBCalculator.Axis[1][0],
                SelectedCarbody.Model.OBBCalculator.Axis[1][1],
                SelectedCarbody.Model.OBBCalculator.Axis[1][2],
                0.0,
                SelectedCarbody.Model.OBBCalculator.Axis[2][0],
                SelectedCarbody.Model.OBBCalculator.Axis[2][1],
                SelectedCarbody.Model.OBBCalculator.Axis[2][2],
                0.0,
                SelectedCarbody.Model.OBBCalculator.Position[0],
                SelectedCarbody.Model.OBBCalculator.Position[1],
                SelectedCarbody.Model.OBBCalculator.Position[2],
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
                            -SelectedCarbody.Model.OBBCalculator.Axis[coordinateSelector[0]][0],
                            -SelectedCarbody.Model.OBBCalculator.Axis[coordinateSelector[0]][1],
                            -SelectedCarbody.Model.OBBCalculator.Axis[coordinateSelector[0]][2],
                            0.0,
                            topSelector[directionOfFront] * SelectedCarbody.Model.OBBCalculator.Axis[coordinateSelector[1]][0],
                            topSelector[directionOfFront] * SelectedCarbody.Model.OBBCalculator.Axis[coordinateSelector[1]][1],
                            topSelector[directionOfFront] * SelectedCarbody.Model.OBBCalculator.Axis[coordinateSelector[1]][2],
                            0.0,
                            -topSelector[directionOfTop] * SelectedCarbody.Model.OBBCalculator.Axis[coordinateSelector[2]][0],
                            -topSelector[directionOfTop] * SelectedCarbody.Model.OBBCalculator.Axis[coordinateSelector[2]][1],
                            -topSelector[directionOfTop] * SelectedCarbody.Model.OBBCalculator.Axis[coordinateSelector[2]][2],
                            0.0,
                            SelectedCarbody.Model.OBBCalculator.Position[0],
                            SelectedCarbody.Model.OBBCalculator.Position[1],
                            SelectedCarbody.Model.OBBCalculator.Position[2],
                            1.0
                            );
            /*
            var centerOfTop = new Matrix3D(
                OBBCalculator.Axis[0][0],
                OBBCalculator.Axis[0][1],
                OBBCalculator.Axis[0][2],
                0.0,
                OBBCalculator.Axis[1][0],
                OBBCalculator.Axis[1][1],
                OBBCalculator.Axis[1][2],
                0.0,
                OBBCalculator.Axis[2][0],
                OBBCalculator.Axis[2][1],
                OBBCalculator.Axis[2][2],
                0.0,
                OBBCalculator.Position[0],
                OBBCalculator.Position[1],
                OBBCalculator.Position[2],
                1.0
                );

                        var matrixTranslationTop = new Vector3D(
             topSelector[0] * OBBCalculator.HalfExtents[0],
             topSelector[1] * OBBCalculator.HalfExtents[1],
             topSelector[2] * OBBCalculator.HalfExtents[2]
                );


*/

            var matrixTranslationTop = new Vector3D(
             0,
             Math.Abs(SelectedCarbody.Model.OBBCalculator.HalfExtents[directionOfFront]),
             -Math.Abs(SelectedCarbody.Model.OBBCalculator.HalfExtents[directionOfTop])
                );
            
            
            centerOfTop.TranslatePrepend(matrixTranslationTop);
            
            // draw top
            var topCoordinate = new CoordinateSystemVisual3D() { ArrowLengths = 100.0 };        
            topCoordinate.Transform = new MatrixTransform3D(centerOfTop);
            //viewportCarbody.Viewport.Children.Add(topCoordinate);


            //move carbody front to world     
            Console.WriteLine("Positoon vorher: " + SelectedCarbody.carbodyModel.GetTransform().ToString());
            SelectedCarbody.Model.CarbodyModel.Transform = new MatrixTransform3D(centerOfTop.Inverse());
            //SelectedCarbody.Model.CarbodyModel.Transform = new MatrixTransform3D(centerOfPlane.Inverse());
            Console.WriteLine("Position nachher: " + SelectedCarbody.carbodyModel.GetTransform().ToString());


            RaisePropertyChanged();
        }

        private void RayHi(Matrix3D matrixStart, Matrix3D matrixEnd, ref double k, ref double o, ref double p)
        {
            Point3D startPoint = new Point3D();
            Point3D EndPoint = new Point3D();

            MatrixTransform3D transformToBoundingBoxStart = new MatrixTransform3D(matrixStart);
            MatrixTransform3D transformToBoundingBoxEnd = new MatrixTransform3D(matrixEnd);

            startPoint = transformToBoundingBoxStart.Transform(startPoint);
            EndPoint = transformToBoundingBoxEnd.Transform(EndPoint);


            // Only used to show start and end points of ray
            SelectedCarbody.Model.Add3DRayOrigin(startPoint);
            SelectedCarbody.Model.Add3DRayOrigin(EndPoint);



            // Ray from side A
            RayHitTestParameters hitParams =
                new RayHitTestParameters(
                startPoint,
                new Vector3D(EndPoint.X - startPoint.X, EndPoint.Y - startPoint.Y, EndPoint.Z - startPoint.Z)
            );
            double distanceFromStart = 0.0;
            VisualTreeHelper.HitTest(SelectedCarbody.carbodyModel, null, h => HitTestResultCallback(ref distanceFromStart, h), hitParams);
            

            // Ray from side B
            hitParams =
                new RayHitTestParameters(
                EndPoint,
                new Vector3D(startPoint.X - EndPoint.X, startPoint.Y - EndPoint.Y, startPoint.Z - EndPoint.Z)
            );
            double distanceFromEnd = 0.0;
            VisualTreeHelper.HitTest(SelectedCarbody.carbodyModel, null, h => HitTestResultCallback(ref distanceFromEnd, h), hitParams);

            k = Math.Abs(distanceFromStart - distanceFromEnd);
            o = distanceFromStart;
            p = distanceFromEnd;

        }


        private void RayHi(Matrix3D matrixStart, Matrix3D matrixEnd)
        {
            Point3D startPoint = new Point3D();
            Point3D EndPoint = new Point3D();

            MatrixTransform3D transformToBoundingBoxStart = new MatrixTransform3D(matrixStart);
            MatrixTransform3D transformToBoundingBoxEnd = new MatrixTransform3D(matrixEnd);

            startPoint = transformToBoundingBoxStart.Transform(startPoint);
            EndPoint = transformToBoundingBoxEnd.Transform(EndPoint);


            // Only used to show start and end points of ray
            SelectedCarbody.Model.Add3DRayOrigin(startPoint);
            SelectedCarbody.Model.Add3DRayOrigin(EndPoint);

            // Ray from side A
            RayHitTestParameters hitParams =
                new RayHitTestParameters(
                startPoint,
                new Vector3D(EndPoint.X - startPoint.X, EndPoint.Y - startPoint.Y, EndPoint.Z - startPoint.Z)
            );
            VisualTreeHelper.HitTest(SelectedCarbody.carbodyModel, null, HitTestResultCallback, hitParams);


            // Ray from side B
            hitParams =
                new RayHitTestParameters(
                EndPoint,
                new Vector3D(startPoint.X - EndPoint.X, startPoint.Y - EndPoint.Y, startPoint.Z - EndPoint.Z)
            );
            VisualTreeHelper.HitTest(SelectedCarbody.carbodyModel, null, HitTestResultCallback, hitParams);

        }
    }
}