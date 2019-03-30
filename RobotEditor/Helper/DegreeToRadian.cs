using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace RobotEditor.ViewModel
{
    public static class DegreeToRadian
    {
        #region Public static methods

        public static double getValue(double angle)
        {
            return (Math.PI / 180) * angle;
        }

        #endregion
    }
}
