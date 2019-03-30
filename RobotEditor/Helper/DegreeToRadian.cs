using System;

namespace RobotEditor.ViewModel
{
    public static class DegreeToRadian
    {
        #region Public methods

        public static double GetValue(double angle)
        {
            return Math.PI / 180 * angle;
        }

        #endregion
    }
}