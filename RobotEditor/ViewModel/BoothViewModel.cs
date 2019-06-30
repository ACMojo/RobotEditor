using System.Windows.Media.Media3D;

using RobotEditor.Model;

namespace RobotEditor.ViewModel
{
    internal class BoothViewModel : BaseViewModel
    {
        #region Instance

        public BoothViewModel(Booth booth)
        {
            Model = booth;
        }

        #endregion

        #region Properties

        public Booth Model { get; }

        public ModelVisual3D BoothModel
        {
            get { return Model.BoothModel; }
            set
            {
                if (value.Equals(Model.BoothModel))
                    return;

                Model.BoothModel = value;

                RaisePropertyChanged();
            }
        }

        public string RobotName
        {
            get { return Model.RobotName; }
            set
            {
                if (value.Equals(Model.RobotName))
                    return;

                if (string.IsNullOrWhiteSpace(value))
                    return;

                Model.RobotName = value;

                RaisePropertyChanged();
            }
        }

        public double BestMatch
        {
            get { return Model.BestMatch; }
            set
            {
                if (value.Equals(Model.BestMatch))
                    return;

                if (double.IsNaN(value))
                    return;

                Model.BestMatch = value;

                RaisePropertyChanged();
            }
        }

        public int Cycles
        {
            get { return Model.Cycles; }
            set
            {
                if (value.Equals(Model.Cycles))
                    return;

                if (value < 0)
                    return;

                Model.Cycles = value;

                RaisePropertyChanged();
            }
        }

        public double ComputationTime
        {
            get { return Model.ComputationTime; }
            set
            {
                if (value.Equals(Model.ComputationTime))
                    return;

                if (double.IsNaN(value))
                    return;

                Model.ComputationTime = value;

                RaisePropertyChanged();
            }
        }

        public double LowerBound
        {
            get { return Model.LowerBound; }
            set
            {
                if (value.Equals(Model.LowerBound))
                    return;

                if (double.IsNaN(value))
                    return;

                Model.LowerBound = value;

                RaisePropertyChanged();
            }
        }

        public int XPos
        {
            get { return Model.XPos; }
            set
            {
                if (value.Equals(Model.XPos))
                    return;

                Model.XPos = value;

                RaisePropertyChanged();
            }
        }

        public int YPos
        {
            get { return Model.YPos; }
            set
            {
                if (value.Equals(Model.YPos))
                    return;

                Model.YPos = value;

                RaisePropertyChanged();
            }
        }

        public int ZPos
        {
            get { return Model.ZPos; }
            set
            {
                if (value.Equals(Model.ZPos))
                    return;

                Model.ZPos = value;

                RaisePropertyChanged();
            }
        }

        #endregion
    }
}