using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace RobotEditor.Model
{
    internal class Voxel
    {
        #region Instance

        public Voxel()
        {
            PositionFromRobotBase = new Point3D(0.0, 0.0, 0.0);
            Colour = Colors.White;
            Rating = 0;
        }

        public Voxel(Point3D positionFromRobotBase, Color colour, int rating)
        {
            PositionFromRobotBase = positionFromRobotBase;
            Colour = colour;
            Rating = rating;
        }

        #endregion

        #region Properties

        public Point3D PositionFromRobotBase { get; set; }

        public Color Colour { get; set; }
        public int Rating { get; set; }

        #endregion
    }
}