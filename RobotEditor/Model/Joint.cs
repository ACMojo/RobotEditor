namespace RobotEditor.Model
{
    public enum JointTypes
    {
        Linear,
        Rotational
    };

    class Joint
    {
        #region Instance

        public Joint(int nr, double a, double alpha, double d, double theta, double maxLim, double minLim, JointTypes jTypes, double speed, double acceleration)
        {
            Nr = nr;
            this.A = a;
            this.Alpha = alpha;
            this.D = d;
            this.Theta = theta;
            this.MaxLim = maxLim;
            this.MinLim = minLim;
            JTypes = jTypes;
            this.Speed = speed;
            this.Acceleration = acceleration;
        }

        public Joint(int nr)
        {
            Nr = nr;
            A = 0.0;
            Alpha = 0.0;
            D = 0.0;
            Theta = 0.0;
            MaxLim = 0.0;
            MinLim = 0.0;
            JTypes = JointTypes.Linear;
            Speed = 0.0;
            Acceleration = 0.0;
        }

        #endregion

        #region Properties

        public int Nr { get; set; }
        public double A { get; set; }
        public double Alpha { get; set; }
        public double D { get; set; }
        public double Theta { get; set; }

        public double MaxLim { get; set; }
        public double MinLim { get; set; }
        public JointTypes JTypes { get; set; }
        public double Speed { get; set; }
        public double Acceleration { get; set; }

        #endregion
    }
}