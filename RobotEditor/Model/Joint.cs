using System;
using System.Dynamic;

namespace RobotEditor.Model
{
    public enum JointTypes { Linear, Rotational };

    class Joint
    {

        public double a { get; set;  }
        public double alpha { get; set;  }
        public double d { get; set;  }
        public double theta { get; set;  }


        public double maxLim { get; set; }
        public double minLim { get; set; }
        public JointTypes JTypes { get; set; }
        public double speed { get; set; }
        public double acceleration { get; set; }

        public Joint(double a, double alpha, double d, double theta, double maxLim, double minLim, JointTypes jTypes, double speed, double acceleration)
        {
            this.a = a;
            this.alpha = alpha;
            this.d = d;
            this.theta = theta;
            this.maxLim = maxLim;
            this.minLim = minLim;
            this.JTypes = jTypes;
            this.speed = speed;
            this.acceleration = acceleration;
        }

        public Joint()
        {
            a = 0.0;
            alpha = 0.0;
            d = 0.0;
            theta = 0.0;
            maxLim = 0.0;
            minLim = 0.0;
            JTypes = JointTypes.Linear;
            speed = 0.0;
            acceleration = 0.0;
        }


    }
}
