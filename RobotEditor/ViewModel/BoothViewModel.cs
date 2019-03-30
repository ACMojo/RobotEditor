using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

using HelixToolkit.Wpf;

using RobotEditor.Model;

namespace RobotEditor.ViewModel
{
    internal class BoothViewModel : BaseViewModel
    {

        private readonly Dictionary<string, string> _errors = new Dictionary<string, string>();

        public BoothViewModel(Booth booth)
        {

            Model = booth;

            _errors.Add(nameof(RobotName), string.Empty);
            _errors.Add(nameof(BestMatch), string.Empty);
            _errors.Add(nameof(ComputationTime), string.Empty);
        }


        public Booth Model { get; }


        public string RobotName
        {
            get { return Model.RobotName; }
            set
            {
                if (value.Equals(Model.RobotName))
                    return;

                if (value == "")
                {
                    _errors[nameof(RobotName)] = "No name given";
                    return;
                }

                _errors[nameof(RobotName)] = string.Empty;

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
                {
                    _errors[nameof(BestMatch)] = "No name given";
                    return;
                }

                _errors[nameof(BestMatch)] = string.Empty;

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
                {
                    _errors[nameof(ComputationTime)] = "No name given";
                    return;
                }

                _errors[nameof(ComputationTime)] = string.Empty;

                Model.ComputationTime = value;

                RaisePropertyChanged();
            }
        }

    }
}
