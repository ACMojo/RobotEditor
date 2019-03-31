using RobotEditor.Model;
using System.Windows.Media.Media3D;

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

        #endregion
    }
}