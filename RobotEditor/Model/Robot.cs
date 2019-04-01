using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Xml;

using HelixToolkit.Wpf;

using RobotEditor.Helper;

namespace RobotEditor.Model
{
    public class Robot
    {
        #region Enums

        public enum RobotTypes
        {
            Puma560,
            EcoRp033,
            EcoRp043,
            Abbirb5400,
            Abbirb5500,
            FanucP250,
            Empty
        };

        #endregion

        #region Fields

        public List<MeshGeometryVisual3D> ManipulabilityVoxel3D;
        public CoordinateSystemVisual3D Robot3D;

        #endregion

        #region Instance

        public Robot(int nrOfJoints, string name)
        {
            for (var i = 0; i < nrOfJoints; i++)
                Joints.Add(new Joint(i + 1));

            Name = name;

            ManipulabilityVoxel3D = new List<MeshGeometryVisual3D>();
            Robot3D = new CoordinateSystemVisual3D();
            RobotModel = new ModelVisual3D();
        }

        public Robot(RobotTypes type, int count)
        {
            switch (type)
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
                    Name = $"Robot_{count}";
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
            RobotModel = new ModelVisual3D();
        }

        #endregion

        #region Properties

        public List<Joint> Joints { get; } = new List<Joint>();
        public string Name { get; set; }

        public VoxelOctree Octree { get; set; }

        public ModelVisual3D RobotModel { get; set; }

        #endregion

        #region Public methods

        public void SaveRobotStructur()
        {
            var doc = new XmlDocument();
            XmlNode robot = doc.CreateElement("Robot");
            var robotAttribute = doc.CreateAttribute("Type");
            robotAttribute.InnerText = Name;
            Debug.Assert(robot.Attributes != null, "robot.Attributes != null");
            robot.Attributes.Append(robotAttribute);

            robotAttribute = doc.CreateAttribute("RootNode");
            robotAttribute.InnerText = "root";
            robot.Attributes.Append(robotAttribute);

            doc.AppendChild(robot);

            XmlNode rootNode = doc.CreateElement("RobotNode");
            var rootNodeAttribute = doc.CreateAttribute("name");
            rootNodeAttribute.InnerText = "root";
            Debug.Assert(rootNode.Attributes != null, "rootNode.Attributes != null");
            rootNode.Attributes.Append(rootNodeAttribute);

            robot.AppendChild(rootNode);

            XmlNode child = doc.CreateElement("Child");
            var childAttribute = doc.CreateAttribute("name");
            childAttribute.InnerText = "joint 1";
            Debug.Assert(child.Attributes != null, "child.Attributes != null");
            child.Attributes.Append(childAttribute);

            rootNode.AppendChild(child);

            Joint oldJoint = null;
            var i = 0;
            foreach (var joint in Joints)
            {
                i++;
                XmlNode jointNode = doc.CreateElement("RobotNode");
                var jointNodeAttribute = doc.CreateAttribute("name");
                jointNodeAttribute.InnerText = "joint " + i;
                Debug.Assert(jointNode.Attributes != null, "jointNode.Attributes != null");
                jointNode.Attributes.Append(jointNodeAttribute);

                if (i > 1)
                {
                    XmlNode transformNode = doc.CreateElement("Transform");
                    XmlNode dhNode = doc.CreateElement("DH");
                    Debug.Assert(dhNode.Attributes != null, "dhNode.Attributes != null");
                    var dhNodeAttribute = doc.CreateAttribute("a");
                    Debug.Assert(oldJoint != null, nameof(oldJoint) + " != null");
                    dhNodeAttribute.InnerText = oldJoint.A.ToString(CultureInfo.InvariantCulture);
                    dhNode.Attributes.Append(dhNodeAttribute);
                    dhNodeAttribute = doc.CreateAttribute("d");
                    dhNodeAttribute.InnerText = oldJoint.D.ToString(CultureInfo.InvariantCulture);
                    dhNode.Attributes.Append(dhNodeAttribute);
                    dhNodeAttribute = doc.CreateAttribute("alpha");
                    dhNodeAttribute.InnerText = oldJoint.Alpha.ToString(CultureInfo.InvariantCulture);
                    dhNode.Attributes.Append(dhNodeAttribute);
                    dhNodeAttribute = doc.CreateAttribute("theta");
                    dhNodeAttribute.InnerText = oldJoint.Theta.ToString(CultureInfo.InvariantCulture);
                    dhNode.Attributes.Append(dhNodeAttribute);
                    dhNodeAttribute = doc.CreateAttribute("units");
                    dhNodeAttribute.InnerText = "degree";
                    dhNode.Attributes.Append(dhNodeAttribute);
                    transformNode.AppendChild(dhNode);
                    jointNode.AppendChild(transformNode);
                }

                XmlNode jointTypeNode = doc.CreateElement("Joint");
                var jointTypeNodeAttribute = doc.CreateAttribute("type");
                XmlNode limitsNode = doc.CreateElement("Limits");
                Debug.Assert(limitsNode.Attributes != null, "limitsNode.Attributes != null");
                var limitsNodeAttribute = doc.CreateAttribute("lo");
                limitsNodeAttribute.InnerText = joint.MinLim.ToString(CultureInfo.InvariantCulture);
                limitsNode.Attributes.Append(limitsNodeAttribute);
                limitsNodeAttribute = doc.CreateAttribute("hi");
                limitsNodeAttribute.InnerText = joint.MaxLim.ToString(CultureInfo.InvariantCulture);
                limitsNode.Attributes.Append(limitsNodeAttribute);
                limitsNodeAttribute = doc.CreateAttribute("units");
                if (joint.JTypes == JointTypes.Linear)
                {
                    jointTypeNodeAttribute.InnerText = "prismatic";
                    limitsNodeAttribute.InnerText = "millimeter";
                }
                else
                {
                    jointTypeNodeAttribute.InnerText = "revolute";
                    limitsNodeAttribute.InnerText = "degree";
                }

                limitsNode.Attributes.Append(limitsNodeAttribute);

                Debug.Assert(jointTypeNode.Attributes != null, "jointTypeNode.Attributes != null");
                jointTypeNode.Attributes.Append(jointTypeNodeAttribute);
                jointTypeNode.AppendChild(limitsNode);
                jointNode.AppendChild(jointTypeNode);

                if (i < Joints.Count)
                {
                    child = doc.CreateElement("Child");
                    Debug.Assert(child.Attributes != null, "child.Attributes != null");
                    childAttribute = doc.CreateAttribute("name");
                    childAttribute.InnerText = "joint " + (i + 1);
                    child.Attributes.Append(childAttribute);
                    jointNode.AppendChild(child);
                }

                robot.AppendChild(jointNode);

                if (i == Joints.Count)
                {
                    child = doc.CreateElement("Child");
                    childAttribute = doc.CreateAttribute("name");
                    childAttribute.InnerText = "tcp";
                    Debug.Assert(child.Attributes != null, "child.Attributes != null");
                    child.Attributes.Append(childAttribute);
                    jointNode.AppendChild(child);

                    XmlNode tcpNode = doc.CreateElement("RobotNode");
                    var tcpNodeAttribute = doc.CreateAttribute("name");
                    tcpNodeAttribute.InnerText = "tcp";
                    Debug.Assert(tcpNode.Attributes != null, "tcpNode.Attributes != null");
                    tcpNode.Attributes.Append(tcpNodeAttribute);

                    XmlNode transformNode = doc.CreateElement("Transform");
                    XmlNode dhNode = doc.CreateElement("DH");
                    var dhNodeAttribute = doc.CreateAttribute("a");
                    dhNodeAttribute.InnerText = joint.A.ToString(CultureInfo.InvariantCulture);
                    Debug.Assert(dhNode.Attributes != null, "dhNode.Attributes != null");
                    dhNode.Attributes.Append(dhNodeAttribute);
                    dhNodeAttribute = doc.CreateAttribute("d");
                    dhNodeAttribute.InnerText = joint.D.ToString(CultureInfo.InvariantCulture);
                    dhNode.Attributes.Append(dhNodeAttribute);
                    dhNodeAttribute = doc.CreateAttribute("alpha");
                    dhNodeAttribute.InnerText = joint.Alpha.ToString(CultureInfo.InvariantCulture);
                    dhNode.Attributes.Append(dhNodeAttribute);
                    dhNodeAttribute = doc.CreateAttribute("theta");
                    dhNodeAttribute.InnerText = joint.Theta.ToString(CultureInfo.InvariantCulture);
                    dhNode.Attributes.Append(dhNodeAttribute);
                    dhNodeAttribute = doc.CreateAttribute("units");
                    dhNodeAttribute.InnerText = "degree";
                    dhNode.Attributes.Append(dhNodeAttribute);
                    transformNode.AppendChild(dhNode);
                    tcpNode.AppendChild(transformNode);

                    robot.AppendChild(tcpNode);
                }

                oldJoint = joint;
            }

            XmlNode robotNodeSet = doc.CreateElement("RobotNodeSet");
            var robotNodeSetAttribute = doc.CreateAttribute("name");
            robotNodeSetAttribute.InnerText = "robotNodeSet";
            Debug.Assert(robotNodeSet.Attributes != null, "robotNodeSet.Attributes != null");
            robotNodeSet.Attributes.Append(robotNodeSetAttribute);
            robotNodeSetAttribute = doc.CreateAttribute("kinematicRoot");
            robotNodeSetAttribute.InnerText = "root";
            robotNodeSet.Attributes.Append(robotNodeSetAttribute);
            robotNodeSetAttribute = doc.CreateAttribute("tcp");
            robotNodeSetAttribute.InnerText = "tcp";
            robotNodeSet.Attributes.Append(robotNodeSetAttribute);

            for (var j = 0; j < Joints.Count; j++)
            {
                child = doc.CreateElement("Node");
                childAttribute = doc.CreateAttribute("name");
                childAttribute.InnerText = "joint " + (j + 1);
                Debug.Assert(child.Attributes != null, "child.Attributes != null");
                child.Attributes.Append(childAttribute);
                robotNodeSet.AppendChild(child);
            }

            robot.AppendChild(robotNodeSet);

            var path = AppDomain.CurrentDomain.BaseDirectory + Name;
            doc.Save(path);
        }

        public void Show3DManipulabilityOctree()
        {
            var i = Octree.StartIndexLeafNodes - 1;
            var maxValue = 0.0;

            for (var h = Octree.StartIndexPerLevel[Octree.Level - 2]; h < Octree.StartIndexPerLevel[Octree.Level - 1]; h++)
            {
                if (Octree.Nodes[h] == null)
                    continue;

                if (((VoxelNodeInner)Octree.Nodes[h]).Max > maxValue)
                    maxValue = ((VoxelNodeInner)Octree.Nodes[h]).Max;
            }

            foreach (var node in Octree.GetLeafNodes())
            {
                i++;
                if (node == null)
                    continue;

                var startOffset = new Point3D(0, 0, 0);

                var k = i;
                for (var w = 0; w < Octree.Level; w++)
                {
                    var n = (k - Octree.StartIndexPerLevel[Octree.Level - 1 - w]) % 8;

                    switch (n)
                    {
                        case 0:
                            startOffset.X = startOffset.X + Math.Pow(2, w) * (Octree.Precision / 2);
                            startOffset.Y = startOffset.Y - Math.Pow(2, w) * (Octree.Precision / 2);
                            startOffset.Z = startOffset.Z - Math.Pow(2, w) * (Octree.Precision / 2);
                            break;
                        case 1:
                            startOffset.X = startOffset.X + Math.Pow(2, w) * (Octree.Precision / 2);
                            startOffset.Y = startOffset.Y + Math.Pow(2, w) * (Octree.Precision / 2);
                            startOffset.Z = startOffset.Z - Math.Pow(2, w) * (Octree.Precision / 2);
                            break;
                        case 2:
                            startOffset.X = startOffset.X - Math.Pow(2, w) * (Octree.Precision / 2);
                            startOffset.Y = startOffset.Y - Math.Pow(2, w) * (Octree.Precision / 2);
                            startOffset.Z = startOffset.Z - Math.Pow(2, w) * (Octree.Precision / 2);
                            break;
                        case 3:
                            startOffset.X = startOffset.X - Math.Pow(2, w) * (Octree.Precision / 2);
                            startOffset.Y = startOffset.Y + Math.Pow(2, w) * (Octree.Precision / 2);
                            startOffset.Z = startOffset.Z - Math.Pow(2, w) * (Octree.Precision / 2);
                            break;
                        case 4:
                            startOffset.X = startOffset.X + Math.Pow(2, w) * (Octree.Precision / 2);
                            startOffset.Y = startOffset.Y - Math.Pow(2, w) * (Octree.Precision / 2);
                            startOffset.Z = startOffset.Z + Math.Pow(2, w) * (Octree.Precision / 2);
                            break;
                        case 5:
                            startOffset.X = startOffset.X + Math.Pow(2, w) * (Octree.Precision / 2);
                            startOffset.Y = startOffset.Y + Math.Pow(2, w) * (Octree.Precision / 2);
                            startOffset.Z = startOffset.Z + Math.Pow(2, w) * (Octree.Precision / 2);
                            break;
                        case 6:
                            startOffset.X = startOffset.X - Math.Pow(2, w) * (Octree.Precision / 2);
                            startOffset.Y = startOffset.Y - Math.Pow(2, w) * (Octree.Precision / 2);
                            startOffset.Z = startOffset.Z + Math.Pow(2, w) * (Octree.Precision / 2);
                            break;
                        case 7:
                            startOffset.X = startOffset.X - Math.Pow(2, w) * (Octree.Precision / 2);
                            startOffset.Y = startOffset.Y + Math.Pow(2, w) * (Octree.Precision / 2);
                            startOffset.Z = startOffset.Z + Math.Pow(2, w) * (Octree.Precision / 2);
                            break;
                        default:
                            Console.WriteLine(@"Fehler");
                            break;
                    }

                    if (w < Octree.Level - 1)
                        k = Octree.StartIndexPerLevel[Octree.Level - 2 - w] + (k - Octree.StartIndexPerLevel[Octree.Level - 1 - w]) / 8;
                }

                var mgv = new MeshGeometryVisual3D();
                var mb = new MeshBuilder();
                mb.AddBox(new Point3D(startOffset.X, startOffset.Y, startOffset.Z), Octree.Precision / 4, Octree.Precision / 4, Octree.Precision / 4);
                mgv.MeshGeometry = mb.ToMesh();
                mgv.Material = MaterialHelper.CreateMaterial(ColorGradient.GetColorForValue(node.Value, maxValue, 0.0));
                ManipulabilityVoxel3D.Add(mgv);
                RobotModel.Children.Add(ManipulabilityVoxel3D.Last());
            }
        }

        public void Show3DRobot()
        {
            var coordinateSystem = new CoordinateSystemVisual3D();
            Robot3D = coordinateSystem;

            Robot3D.XAxisColor = Colors.Yellow;
            Robot3D.YAxisColor = Colors.Yellow;
            Robot3D.ZAxisColor = Colors.Yellow;

            Robot3D.ArrowLengths = 100.0;

            var i = 0;
            foreach (var joint in Joints)
            {
                i++;
                var interimCs = new CoordinateSystemVisual3D();

                if (i == Joints.Count)
                {
                    interimCs.XAxisColor = Colors.Magenta;
                    interimCs.YAxisColor = Colors.Magenta;
                    interimCs.ZAxisColor = Colors.Magenta;
                }

                interimCs.ArrowLengths = 100.0;

                var dhMatrix = new Matrix3D(
                    Math.Cos(MatrixHelper.GetValue(joint.Theta)),
                    Math.Sin(MatrixHelper.GetValue(joint.Theta)),
                    0.0,
                    0.0,
                    -Math.Sin(MatrixHelper.GetValue(joint.Theta)) * Math.Cos(MatrixHelper.GetValue(joint.Alpha)),
                    Math.Cos(MatrixHelper.GetValue(joint.Theta)) * Math.Cos(MatrixHelper.GetValue(joint.Alpha)),
                    Math.Sin(MatrixHelper.GetValue(joint.Alpha)),
                    0.0,
                    Math.Sin(MatrixHelper.GetValue(joint.Theta)) * Math.Sin(MatrixHelper.GetValue(joint.Alpha)),
                    -Math.Cos(MatrixHelper.GetValue(joint.Theta)) * Math.Sin(MatrixHelper.GetValue(joint.Alpha)),
                    Math.Cos(MatrixHelper.GetValue(joint.Alpha)),
                    0.0,
                    joint.A * Math.Cos(MatrixHelper.GetValue(joint.Theta)),
                    joint.A * Math.Sin(MatrixHelper.GetValue(joint.Theta)),
                    joint.D,
                    1.0
                );

                interimCs.Transform = new MatrixTransform3D(dhMatrix);

                var line = new LinesVisual3D { Thickness = 5.0 };
                var pCollection = new Point3DCollection
                {
                    new Point3D(0.0, 0.0, 0.0),
                    new Point3D(joint.A * Math.Cos(MatrixHelper.GetValue(joint.Theta)), joint.A * Math.Sin(MatrixHelper.GetValue(joint.Theta)), joint.D)
                };
                line.Color = Colors.Gray;
                line.Points = pCollection;
                coordinateSystem.Children.Add(line);

                coordinateSystem.Children.Add(interimCs);

                coordinateSystem = interimCs;
            }

            RobotModel.Children.Add(Robot3D);
        }

        public void Hide3DManipulabilityOctree()
        {
            foreach (var manipulabilityVoxel in ManipulabilityVoxel3D)
                RobotModel.Children.Remove(manipulabilityVoxel);
            ManipulabilityVoxel3D.Clear();
        }

        public void Hide3DRobot()
        {
            RobotModel.Children.Remove(Robot3D);
            Robot3D = null;
        }

        #endregion
    }
}