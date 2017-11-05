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

        private readonly IHelixViewport3D viewportCarbody;
        private readonly IHelixViewport3D viewportRobot;

        public MainWindowViewModel(HelixViewport3D viewportCarbody, HelixViewport3D viewportRobot)
        {
            CreateXML = new DelegateCommand<object>(CreateXMLExecute, CreateXMLCanExecute);
            AddCarbody = new DelegateCommand<object>(AddCarbodyExecute, AddCarbodyCanExecute);
            CalcBoundingBox = new DelegateCommand<object>(CalcBoundingBoxExecute, CalcBoundingBoxCanExecute);
            DeleteCarbody = new DelegateCommand<object>(DeleteCarbodyExecute, DeleteCarbodyCanExecute);
            AddRobot = new DelegateCommand<object>(AddRobotExecute, AddRobotCanExecute);
            DeleteRobot = new DelegateCommand<object>(DeleteRobotExecute, DeleteRobotCanExecute);
            FitToView = new DelegateCommand<object>(FitToViewExecute, FitToViewCanExecute);
            EditRobot = new DelegateCommand<object>(EditRobotExecute, EditRobotCanExecute);

            this.viewportCarbody = viewportCarbody;
            this.viewportRobot = viewportRobot;
            this.viewportCarbody.ZoomExtents(0);
            this.viewportRobot.ZoomExtents(0);


            CarbodyModels.Add(new DefaultLights());
            CarbodyModels.Add(new CoordinateSystemVisual3D());

            RobotModels.Add(new DefaultLights());
            //RobotModels.Add(new CoordinateSystemVisual3D());
        }


        public ObservableCollection<RobotViewModel> Robots { get; } = new ObservableCollection<RobotViewModel>();
        public ObservableCollection<CarbodyViewModel> Carbodies { get; } = new ObservableCollection<CarbodyViewModel>();

        public DelegateCommand<object> CalcBoundingBox { get; }
        public DelegateCommand<object> CreateXML { get; }
        public DelegateCommand<object> AddRobot { get; }
        public DelegateCommand<object> DeleteRobot { get; }
        public DelegateCommand<object> AddCarbody { get; }
        public DelegateCommand<object> DeleteCarbody { get; }
        public DelegateCommand<object> FitToView { get; }
        public DelegateCommand<object> EditRobot { get; }


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
                CalcBoundingBox.RaisePropertyChanged();

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
                        ChildAttribute.InnerText = "joint " + (i+1);
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




                VirtualRobotManipulability test = new VirtualRobotManipulability();
                if (test.Init(0, null, saveFileDialog.FileName, "robotNodeSet", "root", "tcp") == true)
                {
                    ManipulabilityVoxel[] vox = test.GetManipulability((float)Math.PI / 2, (float)100.0, 10000);

                    PointsVisual3D line = new PointsVisual3D();
                    line.Size = 5.0;
                    TranslateTransform3D tr = new TranslateTransform3D();

                    tr.OffsetX = vox[0].x;
                    tr.OffsetY = vox[0].y;
                    tr.OffsetZ = vox[0].z;

                    line.Transform = tr;
                    line.Color = Colors.Gray;

                    SelectedRobot.robotModel.Children.Add(line);

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
                var carbody = new Carbody(FileDialog.FileName, FileDialog.SafeFileName, new ModelVisual3D { Content = mi.Load(FileDialog.FileName, null, true) });
                Carbodies.Add(new CarbodyViewModel(carbody));
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


        private void CalcBoundingBoxExecute(object obj)
        {
            double[][] PointCloud = new double[SelectedCarbody.Model.CarbodyAsMesh.Positions.Count][];
            int i = 0;
            foreach (Point3D pointOnCarbody in SelectedCarbody.Model.CarbodyAsMesh.Positions)
            {
                PointCloud[i] = new double[] { pointOnCarbody.X, pointOnCarbody.Y, pointOnCarbody.Z }; 
                i++;
            }
            OBBWrapper OBBCalculator = new OBBWrapper(PointCloud);

            var BoxPos = new Point3D(OBBCalculator.Position[0], OBBCalculator.Position[1], OBBCalculator.Position[2]);
            var BoxSize = new Size3D(OBBCalculator.HalfExtents[0]*2, OBBCalculator.HalfExtents[1]*2, OBBCalculator.HalfExtents[2]*2);
            var bbox = new BoundingBoxVisual3D { BoundingBox = new Rect3D(BoxPos, BoxSize), Diameter = 2.0 };

            RotateTransform3D BoxRot3D = new RotateTransform3D();
            AxisAngleRotation3D BoxRot = new AxisAngleRotation3D(new Vector3D(1, 0, 0), OBBCalculator.Axis[0][0]);
            Transform3DGroup BoxTransformGroup = new Transform3DGroup();
            BoxRot3D.Rotation = BoxRot;
            BoxTransformGroup.Children.Add(BoxRot3D);

            BoxRot = new AxisAngleRotation3D(new Vector3D(0, 1, 0), OBBCalculator.Axis[1][1]);
            BoxRot3D.Rotation = BoxRot;
            BoxTransformGroup.Children.Add(BoxRot3D);

            BoxRot = new AxisAngleRotation3D(new Vector3D(0, 0, 1), OBBCalculator.Axis[2][2]);
            BoxRot3D.Rotation = BoxRot;
            BoxTransformGroup.Children.Add(BoxRot3D);

            bbox.Transform = BoxTransformGroup;

            viewportCarbody.Viewport.Children.Add(bbox);

            RaisePropertyChanged();
        }

        

        private bool CalcBoundingBoxCanExecute(object arg)
        {
            return SelectedCarbody != null;
        }



    }
}
