namespace RobotEditor.Model
{
    internal class Booth
    {
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

        public string RobotName { get; set; }
        public double BestMatch { get; set; }
        public double ComputationTime { get; set; }

        public VoxelOctree Octree { get; }

        #endregion
    }
}