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

        //private Model3D currentModel;

        //private string currentModelPath;

        private readonly IHelixViewport3D viewport;

        public MainWindowViewModel(HelixViewport3D viewport)
        {
            CreateXML = new DelegateCommand<object>(CreateXMLExecute, CreateXMLCanExecute);
            AddCarbody = new DelegateCommand<object>(AddCarbodyExecute, AddCarbodyCanExecute);
            CalcBoundingBox = new DelegateCommand<object>(CalcBoundingBoxExecute, CalcBoundingBoxCanExecute);
            DeleteCarbody = new DelegateCommand<object>(DeleteCarbodyExecute, DeleteCarbodyCanExecute);
            AddRobot = new DelegateCommand<object>(AddRobotExecute, AddRobotCanExecute);
            DeleteRobot = new DelegateCommand<object>(DeleteRobotExecute, DeleteRobotCanExecute);
            FitToView = new DelegateCommand<object>(FitToViewExecute, FitToViewCanExecute);

            this.viewport = viewport;
            this.viewport.ZoomExtents(0);

            Models.Add(new DefaultLights());
            Models.Add(new CoordinateSystemVisual3D());
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

                _selectedRobot = value;
                RaisePropertyChanged();

                DeleteRobot.RaisePropertyChanged();
                CreateXML.RaisePropertyChanged();
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
                    Models.Remove(_selectedCarbody.carbodyModel);
                    Models.Remove(_selectedCarbody.boundingBox);
                }

                _selectedCarbody = value;

                if (_selectedCarbody != null)
                {
                    Models.Add(_selectedCarbody.carbodyModel);
                }

                RaisePropertyChanged();

                DeleteCarbody.RaisePropertyChanged();
                CalcBoundingBox.RaisePropertyChanged();

                this.viewport.ZoomExtents(0);
            }
        }

        public ObservableCollection<Visual3D> Models { get; } = new ObservableCollection<Visual3D>();

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
                Robots.Add(new RobotViewModel(robot));

            if (result != true)
                return;
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

            this.viewport.ZoomExtents(0);
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
            Models.Add(SelectedCarbody.boundingBox);
            

            foreach (Point3D xy in SelectedCarbody.Model.CarbodyAsMesh.Positions)
            {
                Console.WriteLine(xy.X);
            }

            RaisePropertyChanged();
        }

        

        private bool CalcBoundingBoxCanExecute(object arg)
        {
            return SelectedCarbody != null;
        }



    }
}
