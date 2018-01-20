using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;

using RobotEditor.ViewModel;

namespace RobotEditor.Model
{
    internal class Booth
    {
        public VoxelOctree octree;

        public Booth(double size, double stepSize)
        {
            octree = VoxelOctree.Create(size, stepSize);
        }

    }
}
