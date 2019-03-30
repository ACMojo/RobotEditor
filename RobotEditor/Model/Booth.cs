using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;

using RobotEditor.ViewModel;

namespace RobotEditor.Model
{
    internal class Booth
    {
        public string RobotName { get; set; }
        public double BestMatch { get; set; }
        public double ComputationTime { get; set; }


        #region Instance

        public Booth(double diameter, double stepSize)
        {
            Octree = VoxelOctree.Create(diameter, stepSize);
        }

        public Booth(string robotName, double bestMatch, double computationTime)
        {
            RobotName = robotName;
            BestMatch = bestMatch;
            ComputationTime = computationTime;
        }

        #endregion

        #region Properties

        public VoxelOctree Octree { get; }

        #endregion
    }
}
