﻿using System;
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

using Microsoft.Win32;
using System.Xml;
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

                robot.RobotModel.Children.Add(baseCoordinateSystem);

                Robots.Add(new RobotViewModel(robot));
            }

            this.viewportRobot.ZoomExtents(0);

            if (result != true)
                return;
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
            TranslateTransform3D test = new TranslateTransform3D(3000.0, 0.0, 0.0);
            SelectedCarbody.Model.CarbodyModel.Transform = test;


            //this.viewport.ZoomExtents(0);
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

                

                int i = 0;
                foreach (var joint in _selectedRobot.Joints)
                {
                    i++;
                    XmlNode JointNode = doc.CreateElement("RobotNode");
                    XmlAttribute JointNodeAttribute = doc.CreateAttribute("name");
                    JointNodeAttribute.InnerText = "joint " + i;
                    JointNode.Attributes.Append(JointNodeAttribute);
                    Robot.AppendChild(JointNode);

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

                    Child = doc.CreateElement("Child");
                    ChildAttribute = doc.CreateAttribute("name");
                    ChildAttribute.InnerText = "node " + i;
                    Child.Attributes.Append(ChildAttribute);
                    JointNode.AppendChild(Child);


                    XmlNode LinkNode = doc.CreateElement("RobotNode");
                    XmlAttribute LinkNodeAttribute = doc.CreateAttribute("name");
                    LinkNodeAttribute.InnerText = "node " + i;
                    LinkNode.Attributes.Append(LinkNodeAttribute);

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
                    LinkNode.AppendChild(TransformNode);

                    if (i < _selectedRobot.Joints.Count)
                    {
                        Child = doc.CreateElement("Child");
                        ChildAttribute = doc.CreateAttribute("name");
                        ChildAttribute.InnerText = "joint " + (i + 1);
                        Child.Attributes.Append(ChildAttribute);
                        LinkNode.AppendChild(Child);
                    }

                    Robot.AppendChild(LinkNode);
                
                }                

                doc.Save(@saveFileDialog.FileName);
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
            //viewport.Viewport.Children.Add();

            SelectedCarbody.boundingBox = new BoundingBoxVisual3D { BoundingBox = new Rect3D(SelectedCarbody.carbodyModel.Content.Bounds.Location, SelectedCarbody.carbodyModel.Content.Bounds.Size), Diameter = 2.0 };
            CarbodyModels.Add(SelectedCarbody.boundingBox);



            /*
            foreach (Point3D xy in SelectedCarbody.Model.CarbodyAsMesh.Positions)
            {
                Console.WriteLine(xy.X);
            }
            */

            RaisePropertyChanged();
        }

        

        private bool CalcBoundingBoxCanExecute(object arg)
        {
            return SelectedCarbody != null;
        }



    }
}
