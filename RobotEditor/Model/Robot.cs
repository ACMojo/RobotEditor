using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;

using RobotEditor.ViewModel;

namespace RobotEditor.Model
{

    
    internal class Robot
    {
        public List<Joint> Joints { get; } = new List<Joint>();
        public string Name { get; set; }
        public ModelVisual3D robotModel;
        public MeshGeometry3D RobotAsMesh { get; set; }

        public Robot(int nrOfJoints, string name)
        {
            for (int i = 0; i < nrOfJoints; i++)
            {
                Joints.Add(new Joint());
            }

            Name = name;
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
                    RobotAsMesh = (MeshGeometry3D)mb.Geometry; ;
                }
            }

        }


    }
}
