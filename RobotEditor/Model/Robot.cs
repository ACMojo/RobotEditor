using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using System.Xml;

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

        public Robot(int nrOfJoints, string name)
        {
            for (int i = 0; i < nrOfJoints; i++)
            {
                Joints.Add(new Joint(i+1));
            }

            Name = name;
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
    }
}
