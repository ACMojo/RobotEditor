using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

using HelixToolkit.Wpf;

namespace RobotEditor.Model
{
    class Carbody
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public ModelVisual3D carbodyModel;
        public MeshGeometry3D CarbodyAsMesh { get; set; }
        public VoxelOctree octree;

        public ModelVisual3D CarbodyModel
        {
            get { return carbodyModel;  }
            set
            {
                carbodyModel = value;
                var mbs = ((Model3DGroup)value.Content).Children.Cast<GeometryModel3D>();
                foreach (var mb in mbs)
                {
                    CarbodyAsMesh = (MeshGeometry3D)mb.Geometry; ;
                }
            }

        }
        public BoundingBoxVisual3D BoundingBox { get; set; }


        public Carbody(string path, string name, ModelVisual3D model)
        {
            Path = path;
            Name = name;
            CarbodyModel = model; 
        }

        public void UpdateMesh()
        {
            var mbs = ((Model3DGroup)carbodyModel.Content).Children.Cast<GeometryModel3D>();
            foreach (var mb in mbs)
            {
                CarbodyAsMesh = (MeshGeometry3D)mb.Geometry; ;
            }

        }
    }
}
