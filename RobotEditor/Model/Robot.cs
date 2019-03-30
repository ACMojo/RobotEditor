using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using System.Linq;
using System.Xml;
using RobotEditor.ViewModel;
using System.Windows.Media;

namespace RobotEditor.Model
{  
    internal class Robot
    {
        public enum RobotTypes { Puma560, EcoRP033, EcoRP043, ABBIRB5400, ABBIRB5500, FanucP250 };

        public List<Joint> Joints { get; } = new List<Joint>();
        public string Name { get; set; }
        public ModelVisual3D robotModel;
        public MeshGeometry3D RobotAsMesh { get; set; }
        public VoxelOctree Octree { get; set; }
        public List<MeshGeometryVisual3D> ManipulabilityVoxel3D;
        public CoordinateSystemVisual3D Robot3D;

        public Robot(int nrOfJoints, string name)
        {
            for (int i = 0; i < nrOfJoints; i++)
            {
                Joints.Add(new Joint(i+1));
            }

            Name = name;
            ManipulabilityVoxel3D = new List<MeshGeometryVisual3D>();
            Robot3D = new CoordinateSystemVisual3D();
            robotModel = new ModelVisual3D();
        }

        public Robot(RobotTypes type)
        {
            switch(type)
            {
                case RobotTypes.Puma560:
                    Name = "Puma 560";
                    Joints.Add(new Joint(1, 0.0, -90.0, 0.0, 90.0, 160.0, -160.0, JointTypes.Rotational, 0.0, 0.0));
                    Joints.Add(new Joint(2, 431.0, 0.0, 149.0, 0.0, 45.0, -225, JointTypes.Rotational, 0.0, 0.0));
                    Joints.Add(new Joint(3, -20.0, 90.0, 0.0, 90.0, 225, -45, JointTypes.Rotational, 0.0, 0.0));
                    Joints.Add(new Joint(4, 0.0, -90.0, 433.0, 0.0, 170, -110, JointTypes.Rotational, 0.0, 0.0));
                    Joints.Add(new Joint(5, 0.0, 90.0, 0.0, 0.0, 100, -100, JointTypes.Rotational, 0.0, 0.0));
                    Joints.Add(new Joint(6, 0.0, 0.0, 56.0, 0.0, 266, -266, JointTypes.Rotational, 0.0, 0.0));
                    break;
                default:
                    Name = "Puma 560";
                    Joints.Add(new Joint(1, 0.0, -90.0, 0.0, 90.0, 160.0, -160.0, JointTypes.Rotational, 0.0, 0.0));
                    Joints.Add(new Joint(2, 431.0, 0.0, 149.0, 0.0, 45.0, -225, JointTypes.Rotational, 0.0, 0.0));
                    Joints.Add(new Joint(3, -20.0, 90.0, 0.0, 90.0, 225, -45, JointTypes.Rotational, 0.0, 0.0));
                    Joints.Add(new Joint(4, 0.0, -90.0, 433.0, 0.0, 170, -110, JointTypes.Rotational, 0.0, 0.0));
                    Joints.Add(new Joint(5, 0.0, 90.0, 0.0, 0.0, 100, -100, JointTypes.Rotational, 0.0, 0.0));
                    Joints.Add(new Joint(6, 0.0, 0.0, 56.0, 0.0, 266, -266, JointTypes.Rotational, 0.0, 0.0));
                    break;
            }

            ManipulabilityVoxel3D = new List<MeshGeometryVisual3D>();
            Robot3D = new CoordinateSystemVisual3D();
            robotModel = new ModelVisual3D();
        }

        public ModelVisual3D RobotModel
        {
            get { return robotModel; }
            set
            {
                robotModel = value;

                var mbs = ((Model3DGroup)value.Content).Children.Cast<GeometryModel3D>();
                foreach (var mb in mbs)
                {
                    RobotAsMesh = (MeshGeometry3D)mb.Geometry;
                }               
            }
        }

        public void SaveRobotStructur()
        {
            XmlDocument doc = new XmlDocument();
            XmlNode Robot = doc.CreateElement("Robot");
            XmlAttribute RobotAttribute = doc.CreateAttribute("Type");
            RobotAttribute.InnerText = Name;
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

            Joint oldJoint = null;
            int i = 0;
            foreach (var joint in Joints)
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
                    DHNodeAttribute.InnerText = oldJoint.a.ToString();
                    DHNode.Attributes.Append(DHNodeAttribute);
                    DHNodeAttribute = doc.CreateAttribute("d");
                    DHNodeAttribute.InnerText = oldJoint.d.ToString();
                    DHNode.Attributes.Append(DHNodeAttribute);
                    DHNodeAttribute = doc.CreateAttribute("alpha");
                    DHNodeAttribute.InnerText = oldJoint.alpha.ToString();
                    DHNode.Attributes.Append(DHNodeAttribute);
                    DHNodeAttribute = doc.CreateAttribute("theta");
                    DHNodeAttribute.InnerText = oldJoint.theta.ToString();
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
                LimitsNodeAttribute.InnerText = joint.minLim.ToString();
                LimitsNode.Attributes.Append(LimitsNodeAttribute);
                LimitsNodeAttribute = doc.CreateAttribute("hi");
                LimitsNodeAttribute.InnerText = joint.maxLim.ToString();
                LimitsNode.Attributes.Append(LimitsNodeAttribute);
                LimitsNodeAttribute = doc.CreateAttribute("units");
                if (joint.JTypes == JointTypes.Linear)
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

                if (i < Joints.Count)
                {
                    Child = doc.CreateElement("Child");
                    ChildAttribute = doc.CreateAttribute("name");
                    ChildAttribute.InnerText = "joint " + (i + 1);
                    Child.Attributes.Append(ChildAttribute);
                    JointNode.AppendChild(Child);
                }

                Robot.AppendChild(JointNode);

                if (i == Joints.Count)
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
                    DHNodeAttribute.InnerText = joint.a.ToString();
                    DHNode.Attributes.Append(DHNodeAttribute);
                    DHNodeAttribute = doc.CreateAttribute("d");
                    DHNodeAttribute.InnerText = joint.d.ToString();
                    DHNode.Attributes.Append(DHNodeAttribute);
                    DHNodeAttribute = doc.CreateAttribute("alpha");
                    DHNodeAttribute.InnerText = joint.alpha.ToString();
                    DHNode.Attributes.Append(DHNodeAttribute);
                    DHNodeAttribute = doc.CreateAttribute("theta");
                    DHNodeAttribute.InnerText = joint.theta.ToString();
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

            for (int j = 0; j < Joints.Count; j++)
            {
                Child = doc.CreateElement("Node");
                ChildAttribute = doc.CreateAttribute("name");
                ChildAttribute.InnerText = "joint " + (j + 1);
                Child.Attributes.Append(ChildAttribute);
                RobotNodeSet.AppendChild(Child);
            }

            Robot.AppendChild(RobotNodeSet);

            String path = AppDomain.CurrentDomain.BaseDirectory + "/" + Name;
            doc.Save(@path);
        }

        public void show3DManipulabilityOctree()
        {
            int i = Octree.StartIndexLeafNodes - 1;
            int maxValu = 0;

            for (int h = Octree.StartIndexPerLevel[Octree.Level - 2]; h < Octree.StartIndexPerLevel[Octree.Level - 1]; h++)
            {
                if (Octree.Nodes[h] == null)
                    continue;

                if (((VoxelNodeInner)Octree.Nodes[h]).Max > maxValu)
                {
                    maxValu = (int)((VoxelNodeInner)Octree.Nodes[h]).Max;
                }
            }

            foreach (var node in Octree.GetLeafNodes())
            {
                i++;
                if (node == null)
                    continue;
               
                var StartOffset = new Point3D(0, 0, 0);

                int n = 0;
                int k = i;
                for (int w = 0; w < Octree.Level; w++)
                {
                    n = (k - Octree.StartIndexPerLevel[Octree.Level - 1 - w]) % 8;

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

                    if (w < Octree.Level - 1)
                    {
                        k = Octree.StartIndexPerLevel[Octree.Level - 2 - w] + ((k - Octree.StartIndexPerLevel[Octree.Level - 1 - w]) / 8);
                    }
                }

                if (node != null)
                {
                    var mgv = new MeshGeometryVisual3D();
                    var mb = new MeshBuilder();
                    mb.AddBox(new Point3D(StartOffset.X, StartOffset.Y, StartOffset.Z), 25.0, 25.0, 25.0);
                    mgv.MeshGeometry = mb.ToMesh();
                    mgv.Material = MaterialHelper.CreateMaterial(ColorGradient.GetColorForValue(node.Value, maxValu, 1.0));
                    ManipulabilityVoxel3D.Add(mgv);
                    RobotModel.Children.Add(ManipulabilityVoxel3D.Last());
                }
            }
        }
        public void show3DRobot()
        {
            var coordinateSystem = new CoordinateSystemVisual3D();
            Robot3D = coordinateSystem;

            Robot3D.XAxisColor = Colors.Yellow;
            Robot3D.YAxisColor = Colors.Yellow;
            Robot3D.ZAxisColor = Colors.Yellow;

            Robot3D.ArrowLengths = 100.0;

            int i = 0;
            foreach (Joint joint in Joints)
            {
                i++;
                var interimCS = new CoordinateSystemVisual3D();

                if (i == Joints.Count)
                {
                    interimCS.XAxisColor = Colors.Magenta;
                    interimCS.YAxisColor = Colors.Magenta;
                    interimCS.ZAxisColor = Colors.Magenta;
                }
                interimCS.ArrowLengths = 100.0;

                var DH_Matrix = new Matrix3D(
                    Math.Cos(DegreeToRadian.getValue(joint.theta)),
                    Math.Sin(DegreeToRadian.getValue(joint.theta)),
                    0.0,
                    0.0,
                    -Math.Sin(DegreeToRadian.getValue(joint.theta)) * Math.Cos(DegreeToRadian.getValue(joint.alpha)),
                    Math.Cos(DegreeToRadian.getValue(joint.theta)) * Math.Cos(DegreeToRadian.getValue(joint.alpha)),
                    Math.Sin(DegreeToRadian.getValue(joint.alpha)),
                    0.0,
                    Math.Sin(DegreeToRadian.getValue(joint.theta)) * Math.Sin(DegreeToRadian.getValue(joint.alpha)),
                    -Math.Cos(DegreeToRadian.getValue(joint.theta)) * Math.Sin(DegreeToRadian.getValue(joint.alpha)),
                    Math.Cos(DegreeToRadian.getValue(joint.alpha)),
                    0.0,
                    joint.a * Math.Cos(DegreeToRadian.getValue(joint.theta)),
                    joint.a * Math.Sin(DegreeToRadian.getValue(joint.theta)),
                    joint.d,
                    1.0
                );

                interimCS.Transform = new MatrixTransform3D(DH_Matrix);

                LinesVisual3D line = new LinesVisual3D();
                line.Thickness = 5.0;
                Point3DCollection PCollection = new Point3DCollection();
                PCollection.Add(new Point3D(0.0, 0.0, 0.0));
                PCollection.Add(new Point3D(joint.a * Math.Cos(DegreeToRadian.getValue(joint.theta)), joint.a * Math.Sin(DegreeToRadian.getValue(joint.theta)), joint.d));
                line.Color = Colors.Gray;
                line.Points = PCollection;
                coordinateSystem.Children.Add(line);

                coordinateSystem.Children.Add(interimCS);

                coordinateSystem = interimCS;
            }

            RobotModel.Children.Add(Robot3D);
        }

        public void hide3DManipulabilityOctree()
        {
            foreach (var manipulabilityVoxel in ManipulabilityVoxel3D)
            {
                RobotModel.Children.Remove(manipulabilityVoxel);
            }
            ManipulabilityVoxel3D.Clear();
        }
        public void hide3DRobot()
        {
            RobotModel.Children.Remove(Robot3D);
            Robot3D = null;
        }



        /*

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
        */
    }
}
