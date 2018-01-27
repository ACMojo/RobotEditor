using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.Design.Serialization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
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
        private VoxelOctree octree;
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

            this.viewportCarbody = viewportCarbody;
            this.viewportRobot = viewportRobot;
            octree = VoxelOctree.Create(10000, 100d);


            CarbodyModels.Add(new CoordinateSystemVisual3D() { ArrowLengths = 100.0 });
            CarbodyModels.Add(new DefaultLights());

            RobotModels.Add(new DefaultLights());
            RobotModels.Add(new CoordinateSystemVisual3D() { ArrowLengths = 100.0 });         

            RaisePropertyChanged();
            
        }

        public ObservableCollection<RobotViewModel> Robots { get; } = new ObservableCollection<RobotViewModel>();
        public ObservableCollection<CarbodyViewModel> Carbodies { get; } = new ObservableCollection<CarbodyViewModel>();

        public DelegateCommand<object> CreateXML { get; }
        public DelegateCommand<object> AddRobot { get; }
        public DelegateCommand<object> DeleteRobot { get; }
        public DelegateCommand<object> Compare { get; }
        public DelegateCommand<object> AddCarbody { get; }
        public DelegateCommand<object> DeleteCarbody { get; }
        public DelegateCommand<object> FitToView { get; }
        public DelegateCommand<object> EditRobot { get; }

        private bool CompareCanExecute(object arg)
        {
            //return (SelectedCarbody!=null && SelectedRobot!=null);
            return true;
        }

        private void CompareExecute(object obj)
        {
            
            foreach(var carbody in Carbodies)
            {
                SelectedCarbody = carbody;
                // Get points out of 3D carbody modell
                double[][] PointCloud = new double[SelectedCarbody.Model.CarbodyAsMesh.Positions.Count][];
                int i = 0;
                foreach (Point3D pointOnCarbody in SelectedCarbody.Model.CarbodyAsMesh.Positions)
                {
                    PointCloud[i] = new double[] { pointOnCarbody.X, pointOnCarbody.Y, pointOnCarbody.Z };
                    i++;
                }

                // Call wrapper for bounding box calculation
                OBBWrapper OBBCalculator = new OBBWrapper(PointCloud);


                // Only required to draw bounding box and center frame
                
                // Draw box with calculate lengths
                var boxPosVector = new Vector3D(-OBBCalculator.HalfExtents[0],-OBBCalculator.HalfExtents[1],-OBBCalculator.HalfExtents[2]);
                var BoxSize = new Size3D(Math.Abs(OBBCalculator.HalfExtents[0]) * 2, Math.Abs(OBBCalculator.HalfExtents[1]) * 2, Math.Abs(OBBCalculator.HalfExtents[2] * 2));
               
                octreeTemp = VoxelOctree.Create(Math.Abs(OBBCalculator.HalfExtents.Max()) * 4, 100d);

                // Move bounding box on correct position


                var center = new Matrix3D(
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



                // draw root frame
                var rootFrame = new CoordinateSystemVisual3D() { ArrowLengths = 100.0 };        
                rootFrame.Transform = new MatrixTransform3D(center);

                // Move center to fit box 
                center.TranslatePrepend(boxPosVector);
                var bbox = new BoundingBoxVisual3D { BoundingBox = new Rect3D(new Point3D(), BoxSize), Diameter = 20.0 };

                
                //bbox.Transform = new MatrixTransform3D(Matrix3D.Multiply(center, carbody.Model.CarbodyModel.GetTransform().Inverse()));
                bbox.Transform = new MatrixTransform3D(center);

                this.viewportCarbody.ZoomExtents(0);
                //SelectedCarbody.carbodyModel.Children.Add(bbox);
                SelectedCarbody.carbodyModel.Children.Add(rootFrame);
                

                // Perform hit test
                
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

                octree.Add(octreeTemp);
            }


            //octree = VoxelOctree.Create(400, 100d);
            //octree.Set(-790, -790, -790, 1);
            //octree.Set(-1, -1, -1, 2);
            //octree.Set(790, -790, -790, 1);
            //octree.Set(1, -1, -1, 2);
            
            var comparison = new ResultWindow(octree);
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
                    CarbodyModels.Remove(_selectedCarbody.carbodyModel);
                    CarbodyModels.Remove(_selectedCarbody.boundingBox);
                }

                _selectedCarbody = value;

                if (_selectedCarbody != null)
                {
                    CarbodyModels.Add(_selectedCarbody.carbodyModel);
                }

                RaisePropertyChanged();

                DeleteCarbody.RaisePropertyChanged();
                
                this.viewportCarbody.ZoomExtents(0);
            }
        }

        public ObservableCollection<Visual3D> CarbodyModels { get; } = new ObservableCollection<Visual3D>();
        public ObservableCollection<Visual3D> RobotModels { get; } = new ObservableCollection<Visual3D>();

        private bool AddRobotCanExecute(object arg)
        {
            return true;
        }

        private void AddRobotExecute(object obj)
        {
            var robot = new Robot(0, "Roboter " + Robots.Count);
            robot.Joints.Add(new Joint(1, 0.0, -90.0, 0.0, 90.0, 160.0, -160.0, JointTypes.Rotational, 0.0, 0.0));
            robot.Joints.Add(new Joint(2, 431.0, 0.0, 149.0, 0.0, 45.0, -225, JointTypes.Rotational, 0.0, 0.0));
            robot.Joints.Add(new Joint(3, -20.0, 90.0, 0.0, 90.0, 225, -45, JointTypes.Rotational, 0.0, 0.0));
            robot.Joints.Add(new Joint(4, 0.0, -90.0, 433.0, 0.0, 170, -110, JointTypes.Rotational, 0.0, 0.0));
            robot.Joints.Add(new Joint(5, 0.0, 90.0, 0.0, 0.0, 100, -100, JointTypes.Rotational, 0.0, 0.0));
            robot.Joints.Add(new Joint(6, 0.0, 0.0, 56.0, 0.0, 266, -266, JointTypes.Rotational, 0.0, 0.0));
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
                /*
                CarbodyModels.Add(new CoordinateSystemVisual3D() { ArrowLengths = 100.0, Transform = new TranslateTransform3D(rayMeshResult.PointHit.X, rayMeshResult.PointHit.Y, rayMeshResult.PointHit.Z) });
                */
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
                var coordinateSystem = new CoordinateSystemVisual3D() { ArrowLengths = 100.0, Transform = new TranslateTransform3D(rayMeshResult.PointHit.X, rayMeshResult.PointHit.Y, rayMeshResult.PointHit.Z) };
                var PointTest = rayMeshResult.PointHit;
                PointTest = Point3D.Multiply(PointTest, SelectedCarbody.Model.CarbodyModel.GetTransform());

                SelectedCarbody.Model.CarbodyModel.Children.Add(coordinateSystem);
                //Console.WriteLine(coordinateSystem.GetTransform().OffsetX + " " + PointTest.X + " " + coordinateSystem.GetTransform().OffsetY + " " + PointTest.Y + " " + coordinateSystem.GetTransform().OffsetZ + " " + PointTest.Z);
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

        private void FitToViewExecute(object obj)
        {
            viewportCarbody.ZoomExtents(0);
            RaisePropertyChanged();

            //octree.Set(-6, 27, 9, 1);
            //octree.Set(27, -6, 9, 1);

            //var octreeBooth = VoxelOctree.Create(6000d, 100d);
            //Console.WriteLine(@"Level: {0} / Nodes: {1}", octreeBooth.Level, octreeBooth.Nodes.Length);

            //octreeBooth.Set(5, 0, 0, 2);
            //octreeBooth.Set(0, 5, 0, 3);
            //octreeBooth.Set(0, 0, 5, 4);
            //octreeBooth.Set(5, 5, 5, 7);
            //octreeBooth.Set(0, 0, -32, 234234);
            //foreach (var node in octreeBooth.GetLeafNodes())
            //{
            //    if (node != null)
            //        Console.WriteLine(node.Value);
            //}

            //var octreeRobot = VoxelOctree.Create(3000d, 100d);
            //Console.WriteLine(@"Level: {0} / Nodes: {1}", octreeRobot.Level, octreeRobot.Nodes.Length);
            //octreeRobot.Set(5, 0, 0, 6);
            //octreeRobot.Set(0, 5, 0, 7);
            //octreeRobot.Set(0, 0, 5, 8);
            //octreeRobot.Set(5, 5, 5, 9);
            //foreach (var node in octreeRobot.GetLeafNodes())
            //{
            //    if (node != null)
            //        Console.WriteLine(node.Value);
            //}

            //double maxValue = 0;
            //VoxelNodeInner targetNode = null;
            //foreach (var node in octreeBooth.GetAncestorNodes(octreeBooth.Level - octreeRobot.Level))
            //{
            //    if (node == null)
            //        continue;

            //    var innerNode = (VoxelNodeInner)node;
            //    if (targetNode == null)
            //    {
            //        maxValue = innerNode.Max;
            //        targetNode = innerNode;
            //    }
            //    else if (!double.IsNaN(innerNode.Max) && maxValue < innerNode.Max)
            //    {
            //        maxValue = innerNode.Max;
            //        targetNode = innerNode;
            //    }
            //}

            //var result = octreeBooth.Multiply(targetNode, octreeRobot);

            //Console.WriteLine(@"Result: {0}", result);
        }

        private bool FitToViewCanExecute(object arg)
        {
            return true;
        }

        private bool AddCarbodyCanExecute(object arg)
        {
            return true;
        }

        private void CreateXMLExecute(object obj)
        {
            var saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                XmlDocument doc = new XmlDocument();

                XmlNode Robot = doc.CreateElement("Robot");
                XmlAttribute RobotAttribute = doc.CreateAttribute("Type");
                RobotAttribute.InnerText = _selectedRobot.Name;
                Robot.Attributes.Append(RobotAttribute);

                RobotAttribute = doc.CreateAttribute("RootNode");
                RobotAttribute.InnerText = "root";
                Robot.Attributes.Append(RobotAttribute);

                doc.AppendChild(Robot);

                XmlNode RootNode = doc.CreateElement("RobotNode");
                XmlAttribute RootNodeAttribute = doc.CreateAttribute("name");
                RootNodeAttribute.InnerText = "root";
                RootNode.Attributes.Append(RootNodeAttribute);

                Robot.AppendChild(RootNode);

                XmlNode Child = doc.CreateElement("Child");
                XmlAttribute ChildAttribute = doc.CreateAttribute("name");
                ChildAttribute.InnerText = "joint 1";
                Child.Attributes.Append(ChildAttribute);

                RootNode.AppendChild(Child);

                JointViewModel oldJoint = null;
                int i = 0;
                foreach (var joint in _selectedRobot.Joints)
                {
                    i++;
                    XmlNode JointNode = doc.CreateElement("RobotNode");
                    XmlAttribute JointNodeAttribute = doc.CreateAttribute("name");
                    JointNodeAttribute.InnerText = "joint " + i;
                    JointNode.Attributes.Append(JointNodeAttribute);

                    if (i > 1)
                    {
                        XmlNode TransformNode = doc.CreateElement("Transform");
                        XmlNode DHNode = doc.CreateElement("DH");
                        XmlAttribute DHNodeAttribute = doc.CreateAttribute("a");
                        DHNodeAttribute.InnerText = oldJoint.A.ToString();
                        DHNode.Attributes.Append(DHNodeAttribute);
                        DHNodeAttribute = doc.CreateAttribute("d");
                        DHNodeAttribute.InnerText = oldJoint.D.ToString();
                        DHNode.Attributes.Append(DHNodeAttribute);
                        DHNodeAttribute = doc.CreateAttribute("alpha");
                        DHNodeAttribute.InnerText = oldJoint.Alpha.ToString();
                        DHNode.Attributes.Append(DHNodeAttribute);
                        DHNodeAttribute = doc.CreateAttribute("theta");
                        DHNodeAttribute.InnerText = oldJoint.Theta.ToString();
                        DHNode.Attributes.Append(DHNodeAttribute);
                        DHNodeAttribute = doc.CreateAttribute("units");
                        DHNodeAttribute.InnerText = "degree";
                        DHNode.Attributes.Append(DHNodeAttribute);
                        TransformNode.AppendChild(DHNode);
                        JointNode.AppendChild(TransformNode);
                    }

                    XmlNode JointTypeNode = doc.CreateElement("Joint");
                    XmlAttribute JointTypeNodeAttribute = doc.CreateAttribute("type");
                    XmlNode LimitsNode = doc.CreateElement("Limits");
                    XmlAttribute LimitsNodeAttribute = doc.CreateAttribute("lo");
                    LimitsNodeAttribute.InnerText = joint.MinLim.ToString();
                    LimitsNode.Attributes.Append(LimitsNodeAttribute);
                    LimitsNodeAttribute = doc.CreateAttribute("hi");
                    LimitsNodeAttribute.InnerText = joint.MaxLim.ToString();
                    LimitsNode.Attributes.Append(LimitsNodeAttribute);
                    LimitsNodeAttribute = doc.CreateAttribute("units");
                    if (joint.JoinType == JointTypes.Linear)
                    {
                        JointTypeNodeAttribute.InnerText = "prismatic";
                        LimitsNodeAttribute.InnerText = "millimeter";
                    }
                    else
                    {
                        JointTypeNodeAttribute.InnerText = "revolute";
                        LimitsNodeAttribute.InnerText = "degree";
                    }
                    LimitsNode.Attributes.Append(LimitsNodeAttribute);

                    JointTypeNode.Attributes.Append(JointTypeNodeAttribute);
                    JointTypeNode.AppendChild(LimitsNode);
                    JointNode.AppendChild(JointTypeNode);

                    if (i < _selectedRobot.Joints.Count)
                    {
                        Child = doc.CreateElement("Child");
                        ChildAttribute = doc.CreateAttribute("name");
                        ChildAttribute.InnerText = "joint " + (i + 1);
                        Child.Attributes.Append(ChildAttribute);
                        JointNode.AppendChild(Child);
                    }

                    Robot.AppendChild(JointNode);

                    if (i == _selectedRobot.Joints.Count)
                    {
                        Child = doc.CreateElement("Child");
                        ChildAttribute = doc.CreateAttribute("name");
                        ChildAttribute.InnerText = "tcp";
                        Child.Attributes.Append(ChildAttribute);
                        JointNode.AppendChild(Child);

                        XmlNode TCPNode = doc.CreateElement("RobotNode");
                        XmlAttribute TCPNodeAttribute = doc.CreateAttribute("name");
                        TCPNodeAttribute.InnerText = "tcp";
                        TCPNode.Attributes.Append(TCPNodeAttribute);

                        XmlNode TransformNode = doc.CreateElement("Transform");
                        XmlNode DHNode = doc.CreateElement("DH");
                        XmlAttribute DHNodeAttribute = doc.CreateAttribute("a");
                        DHNodeAttribute.InnerText = joint.A.ToString();
                        DHNode.Attributes.Append(DHNodeAttribute);
                        DHNodeAttribute = doc.CreateAttribute("d");
                        DHNodeAttribute.InnerText = joint.D.ToString();
                        DHNode.Attributes.Append(DHNodeAttribute);
                        DHNodeAttribute = doc.CreateAttribute("alpha");
                        DHNodeAttribute.InnerText = joint.Alpha.ToString();
                        DHNode.Attributes.Append(DHNodeAttribute);
                        DHNodeAttribute = doc.CreateAttribute("theta");
                        DHNodeAttribute.InnerText = joint.Theta.ToString();
                        DHNode.Attributes.Append(DHNodeAttribute);
                        DHNodeAttribute = doc.CreateAttribute("units");
                        DHNodeAttribute.InnerText = "degree";
                        DHNode.Attributes.Append(DHNodeAttribute);
                        TransformNode.AppendChild(DHNode);
                        TCPNode.AppendChild(TransformNode);

                        Robot.AppendChild(TCPNode);
                    }

                    oldJoint = joint;
                }

                XmlNode RobotNodeSet = doc.CreateElement("RobotNodeSet");
                XmlAttribute RobotNodeSetAttribute = doc.CreateAttribute("name");
                RobotNodeSetAttribute.InnerText = "robotNodeSet";
                RobotNodeSet.Attributes.Append(RobotNodeSetAttribute);
                RobotNodeSetAttribute = doc.CreateAttribute("kinematicRoot");
                RobotNodeSetAttribute.InnerText = "root";
                RobotNodeSet.Attributes.Append(RobotNodeSetAttribute);
                RobotNodeSetAttribute = doc.CreateAttribute("tcp");
                RobotNodeSetAttribute.InnerText = "tcp";
                RobotNodeSet.Attributes.Append(RobotNodeSetAttribute);

                for (int j = 0; j < _selectedRobot.Joints.Count; j++)
                {
                    Child = doc.CreateElement("Node");
                    ChildAttribute = doc.CreateAttribute("name");
                    ChildAttribute.InnerText = "joint " + (j + 1);
                    Child.Attributes.Append(ChildAttribute);
                    RobotNodeSet.AppendChild(Child);
                }

                Robot.AppendChild(RobotNodeSet);
                doc.Save(@saveFileDialog.FileName);

                float[] maxB;
                float[] minB;
                float maxManip;
                using (var VrManip = new VirtualRobotManipulability())
                {
                    if (VrManip.Init(0, null, @saveFileDialog.FileName, "robotNodeSet", "root", "tcp"))
                    {
                        ManipulabilityVoxel[] vox = VrManip.GetManipulability((float)100.0, (float)(Math.PI / 2), 100000, false, false, true, 50f);


                        minB = VrManip.MinBox;
                        maxB = VrManip.MaxBox;

                        // Calc size of cube depending on reachability of robot
                        double octreeSize;
                        if (VrManip.MaxBox.Max() > VrManip.MinBox.Max())
                        {
                            octreeSize = VrManip.MaxBox.Max()*2;
                        }
                        else
                        {
                            octreeSize = VrManip.MinBox.Max()*2;
                        }

                        SelectedRobot.Model.octree = VoxelOctree.Create(octreeSize, 100.0);

                        maxManip = VrManip.MaxManipulability;


                        ManipulabilityVoxel voxOld = vox[0];
                        ColorGradient colorCalculator = new ColorGradient();
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
                                var vm = new MeshGeometryVisual3D();
                                var mb = new MeshBuilder();
                               


                                if (SelectedRobot.Model.octree.Set((int)Math.Floor((minB[0] / 100.0) + voxOld.x), (int)Math.Floor((minB[1] / 100.0) + voxOld.y), (int)Math.Floor((minB[2] / 100.0) + voxOld.z), maxValue))
                                {
                                    Console.WriteLine("Erfolgreich");
                                }
                                else
                                {
                                    var value = octree.Get((int)Math.Floor((minB[0] / 100.0) + voxOld.x), (int)Math.Floor((minB[1] / 100.0) + voxOld.y), (int)Math.Floor((minB[2] / 100.0) + voxOld.z));
                                    if (double.IsNaN(value))
                                        Console.WriteLine($"Nicht erfolgreich bei: { Math.Floor((minB[0] / 100.0) + voxOld.x) } { Math.Floor((minB[1] / 100.0) + voxOld.y) } { Math.Floor((minB[2] / 100.0) + voxOld.z) }");
                                    else
                                        ;
                                }




                                mb.AddBox(new Point3D(minB[0] + voxOld.x * 100, minB[1] + voxOld.y * 100, minB[2] + voxOld.z * 100), 10.0, 10.0, 10.0);
                                vm.MeshGeometry = mb.ToMesh();

                                vm.Material = MaterialHelper.CreateMaterial(colorCalculator.GetColorForValue(maxValue, 1.0, 0.0));
                                SelectedRobot.robotModel.Children.Add(vm);

                                voxOld = vox[j];
                                maxValue = vox[j].value;
                            }
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
            // Get points out of 3D carbody modell
            double[][] PointCloud = new double[SelectedCarbody.Model.CarbodyAsMesh.Positions.Count][];
            int i = 0;
            foreach (Point3D pointOnCarbody in SelectedCarbody.Model.CarbodyAsMesh.Positions)
            {
                PointCloud[i] = new double[] { pointOnCarbody.X, pointOnCarbody.Y, pointOnCarbody.Z };
                i++;
            }

            // Call wrapper for bounding box calculation
            OBBWrapper OBBCalculator = new OBBWrapper(PointCloud);


            // Only required to draw bounding box and center frame
            /*
            // Draw box with calculate lengths
            var boxPosVector = new Vector3D(-OBBCalculator.HalfExtents[0],-OBBCalculator.HalfExtents[1],-OBBCalculator.HalfExtents[2]);
            var BoxSize = new Size3D(Math.Abs(OBBCalculator.HalfExtents[0]) * 2, Math.Abs(OBBCalculator.HalfExtents[1]) * 2, Math.Abs(OBBCalculator.HalfExtents[2] * 2));


            // Move bounding box on correct position

            
            var center = new Matrix3D(
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
           
         
            
            // draw root frame
            var rootFrame = new CoordinateSystemVisual3D() { ArrowLengths = 100.0 };        
            rootFrame.Transform = new MatrixTransform3D(center);

            // Move center to fit box 
            center.TranslatePrepend(boxPosVector);
            var bbox = new BoundingBoxVisual3D { BoundingBox = new Rect3D(new Point3D(), BoxSize), Diameter = 20.0 };
            bbox.Transform = new MatrixTransform3D(center);

            this.viewportCarbody.ZoomExtents(0);

            viewportCarbody.Viewport.Children.Add(bbox);
            viewportCarbody.Viewport.Children.Add(rootFrame);
            */

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
            int directionOfSymmetryPlane = 0;
            int directionOfTop = 0;
            int directionOfTopShift = 0;
            int directionOfFront = 0;
            int directionOfFrontShift = 0;
            double sumOfSquaresDivided = 9999.0;
            double[] sizeOfSymmetryPlane = new double[3] { 10.0, 10.0, 10.0 };

            sizeOfSymmetryPlane[Array.IndexOf(OBBCalculator.HalfExtents, OBBCalculator.HalfExtents.Max())] = OBBCalculator.HalfExtents.Max();
            for (int m = 0; m < 2; m++)
            {
                Vector3D vector;
                int[] stepDirection = new int[3] { 0, 0, 0 };
                int[] sideASelector = new int[3] { 0, 0, 0 };
                int[] sideBSelector = new int[3] { 0, 0, 0 };

                int z = m;
                if (Array.IndexOf(OBBCalculator.HalfExtents, OBBCalculator.HalfExtents.Max()) == m)
                {
                    z = m + 1;
                }
                if (m == 1 && Array.IndexOf(OBBCalculator.HalfExtents, OBBCalculator.HalfExtents.Max()) == 0)
                {
                    z = 2;
                }
                sideASelector[Array.IndexOf(OBBCalculator.HalfExtents, OBBCalculator.HalfExtents.Max())] = 1;
                sideBSelector[Array.IndexOf(OBBCalculator.HalfExtents, OBBCalculator.HalfExtents.Max())] = 1;
                sizeOfSymmetryPlane[Array.IndexOf(OBBCalculator.HalfExtents, OBBCalculator.HalfExtents.Max())] = OBBCalculator.HalfExtents.Max() * 2;
                sideASelector[z] = 1;
                sideBSelector[z] = -1;

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
                             sideASelector[0] * OBBCalculator.HalfExtents[0],
                             sideASelector[1] * OBBCalculator.HalfExtents[1],
                             sideASelector[2] * OBBCalculator.HalfExtents[2]
                                );

                var matrixTranslationEnd = new Vector3D(
                             sideBSelector[0] * OBBCalculator.HalfExtents[0],
                             sideBSelector[1] * OBBCalculator.HalfExtents[1],
                             sideBSelector[2] * OBBCalculator.HalfExtents[2]
                             );

                matrixStart.TranslatePrepend(matrixTranslationStart);
                matrixEnd.TranslatePrepend(matrixTranslationEnd);


                double[] savor = new double[(int)(Math.Abs(OBBCalculator.HalfExtents.Max() * 2 / 50.0) + 1)];
                double[] savor2 = new double[(int)(Math.Abs(OBBCalculator.HalfExtents.Max() * 2 / 50.0) + 1)];
                double[] savor3 = new double[(int)(Math.Abs(OBBCalculator.HalfExtents.Max() * 2 / 50.0) + 1)];
                double[] savor4 = new double[(int)(Math.Abs(OBBCalculator.HalfExtents.Max() * 2 / 50.0) + 1)];


                for (int k = 0; k < Math.Abs(OBBCalculator.HalfExtents.Max() * 2 / 50); k++)
                {
                    savor[k] = double.NaN;
                    RayHi(matrixStart, matrixEnd, ref savor[k], ref savor2[k], ref savor3[k]);

                    stepDirection[Array.IndexOf(OBBCalculator.HalfExtents, OBBCalculator.HalfExtents.Max())] = 1;
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
                        savor4[q] = (OBBCalculator.HalfExtents[z] * 2) - savor2[q] - savor3[q];
                    }
                }
                if (savor4.Average() < 100.0)
                {
                    directionOfTop = z;
                    double totalFirst = 0.0;
                    double totalLast = 0.0;
                    directionOfFront = Array.IndexOf(OBBCalculator.HalfExtents, OBBCalculator.HalfExtents.Max());
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
                    directionOfSymmetryPlane = Array.IndexOf(sideASelector, sideASelector.Min());
                }

            }

            var centerOfPlane = new Matrix3D(
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


            int[] topSelector = new int[3] { 0, 0, 0 };
            topSelector[directionOfTop] = directionOfTopShift;
            topSelector[directionOfFront] = directionOfFrontShift;

            int[] coordinateSelector = new int[3] { 0, 0, 0 };
            coordinateSelector[2] = directionOfTop;
            coordinateSelector[1] = directionOfFront;
            coordinateSelector[0] = Array.IndexOf(topSelector, 0);

            
                        var centerOfTop = new Matrix3D(
                            -OBBCalculator.Axis[coordinateSelector[0]][0],
                            -OBBCalculator.Axis[coordinateSelector[0]][1],
                            -OBBCalculator.Axis[coordinateSelector[0]][2],
                            0.0,
                            topSelector[directionOfFront] * OBBCalculator.Axis[coordinateSelector[1]][0],
                            topSelector[directionOfFront] * OBBCalculator.Axis[coordinateSelector[1]][1],
                            topSelector[directionOfFront] * OBBCalculator.Axis[coordinateSelector[1]][2],
                            0.0,
                            -topSelector[directionOfTop] * OBBCalculator.Axis[coordinateSelector[2]][0],
                            -topSelector[directionOfTop] * OBBCalculator.Axis[coordinateSelector[2]][1],
                            -topSelector[directionOfTop] * OBBCalculator.Axis[coordinateSelector[2]][2],
                            0.0,
                            OBBCalculator.Position[0],
                            OBBCalculator.Position[1],
                            OBBCalculator.Position[2],
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
             Math.Abs(OBBCalculator.HalfExtents[directionOfFront]),
             -Math.Abs(OBBCalculator.HalfExtents[directionOfTop])
                );
            
            
            centerOfTop.TranslatePrepend(matrixTranslationTop);
            
            
            // draw symmetry plane
            sizeOfSymmetryPlane[directionOfSymmetryPlane] = OBBCalculator.HalfExtents[directionOfSymmetryPlane] * 2;

            var symmetryPlane = new BoxVisual3D() { Length = sizeOfSymmetryPlane[0], Width = sizeOfSymmetryPlane[1], Height = sizeOfSymmetryPlane[2], Material = MaterialHelper.CreateMaterial(Colors.HotPink) };
            symmetryPlane.Transform = new MatrixTransform3D(centerOfPlane);
            //viewportCarbody.Viewport.Children.Add(symmetryPlane);

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
             
            var vm = new MeshGeometryVisual3D();
            var mb = new MeshBuilder();
            mb.AddBox(startPoint, 10.0, 10.0, 10.0);
            vm.MeshGeometry = mb.ToMesh();
            vm.Material = MaterialHelper.CreateMaterial(Colors.Red);
            //CarbodyModels.Add(vm);


            var vm2 = new MeshGeometryVisual3D();
            var mb2 = new MeshBuilder();
            mb2.AddBox(EndPoint, 10.0, 10.0, 10.0);
            vm2.MeshGeometry = mb2.ToMesh();
            vm2.Material = MaterialHelper.CreateMaterial(Colors.Red);
            //CarbodyModels.Add(vm2);
            


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

            var vm = new MeshGeometryVisual3D();
            var mb = new MeshBuilder();
            mb.AddBox(startPoint, 10.0, 10.0, 10.0);
            vm.MeshGeometry = mb.ToMesh();
            vm.Material = MaterialHelper.CreateMaterial(Colors.Red);
           //SelectedCarbody.Model.CarbodyModel.Children.Add(vm);


            var vm2 = new MeshGeometryVisual3D();
            var mb2 = new MeshBuilder();
            mb2.AddBox(EndPoint, 10.0, 10.0, 10.0);
            vm2.MeshGeometry = mb2.ToMesh();
            vm2.Material = MaterialHelper.CreateMaterial(Colors.Red);
            //SelectedCarbody.Model.CarbodyModel.Children.Add(vm2);



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