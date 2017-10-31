using System.Collections.Generic;
using System.ComponentModel;

using RobotEditor.Model;

namespace RobotEditor.ViewModel
{
    internal class JointViewModel : BaseViewModel, IDataErrorInfo
    {
        #region Fields

        private readonly Dictionary<string, string> _errors = new Dictionary<string, string>();

        #endregion

        #region Instance

        public JointViewModel(Joint joint)
        {
            Model = joint;

            _errors.Add(nameof(A), string.Empty);
            _errors.Add(nameof(Alpha), string.Empty);
            _errors.Add(nameof(D), string.Empty);
            _errors.Add(nameof(Theta), string.Empty);
            _errors.Add(nameof(MaxLim), string.Empty);
            _errors.Add(nameof(MinLim), string.Empty);
            _errors.Add(nameof(JoinType), string.Empty);
            _errors.Add(nameof(Speed), string.Empty);
            _errors.Add(nameof(Acceleration), string.Empty);
        }

        #endregion

        #region Properties

        public Joint Model { get; }

        public JointTypes JoinType
        {
            get { return Model.JTypes;  }

            set
            {
                if (value.Equals(Model.JTypes))
                    return;

                _errors[nameof(JointTypes)] = string.Empty;

                Model.JTypes = value;

                RaisePropertyChanged();
            }
        }

        public double A
        {
            get { return Model.a; }
            set
            {
                if (value.Equals(Model.a))
                    return;

                /*if (value < 0)
                {
                    _errors[nameof(A)] = "Too small";
                    return;
                }*/

                _errors[nameof(A)] = string.Empty;

                Model.a = value;

                RaisePropertyChanged();
            }
        }


        public double Alpha
        {
            get { return Model.alpha; }
            set
            {
                if (value.Equals(Model.alpha))
                    return;

                /*if (value < 0)
                {
                    _errors[nameof(Alpha)] = "Too small";
                    return;
                }*/

                _errors[nameof(Alpha)] = string.Empty;

                Model.alpha = value;

                RaisePropertyChanged();
            }
        }


        public double D
        {
            get { return Model.d; }
            set
            {
                if (value.Equals(Model.d))
                    return;

                /*if (value < 0)
                {
                    _errors[nameof(D)] = "Too small";
                    return;
                }*/

                _errors[nameof(D)] = string.Empty;

                Model.d = value;

                RaisePropertyChanged();
            }
        }


        public double Theta
        {
            get { return Model.theta; }
            set
            {
                if (value.Equals(Model.theta))
                    return;

                /*if (value < 0)
                {
                    _errors[nameof(Theta)] = "Too small";
                    return;
                }*/

                _errors[nameof(Theta)] = string.Empty;

                Model.theta = value;

                RaisePropertyChanged();
            }
        }


        public double MaxLim
        {
            get { return Model.maxLim; }
            set
            {
                if (value.Equals(Model.maxLim))
                    return;

                if (value < 0)
                {
                    _errors[nameof(MaxLim)] = "Too small";
                    return;
                }

                _errors[nameof(MaxLim)] = string.Empty;

                Model.maxLim = value;

                RaisePropertyChanged();
            }
        }


        public double MinLim
        {
            get { return Model.minLim; }
            set
            {
                
                if (value.Equals(Model.minLim))
                    return;

                if (value < 0)
                {
                    _errors[nameof(MinLim)] = "Too small";
                    return;
                }

                _errors[nameof(MinLim)] = string.Empty;

                Model.minLim = value;

                RaisePropertyChanged();
            }
        }


        public double Speed
        {
            get { return Model.speed; }
            set
            {
                if (value.Equals(Model.speed))
                    return;

                if (value < 0)
                {
                    _errors[nameof(Speed)] = "Too small";
                    return;
                }

                _errors[nameof(Speed)] = string.Empty;

                Model.speed = value;

                RaisePropertyChanged();
            }
        }


        public double Acceleration
        {
            get { return Model.acceleration; }
            set
            {
                if (value.Equals(Model.acceleration))
                    return;

                if (value < 0)
                {
                    _errors[nameof(Acceleration)] = "Too small";
                    return;
                }

                _errors[nameof(Acceleration)] = string.Empty;

                Model.acceleration = value;

                RaisePropertyChanged();
            }
        }

    public string this[string columnName]
        {
            get
            {
                if (_errors.ContainsKey(columnName))
                    return _errors[columnName];

                return string.Empty;
            }
        }

        public string Error { get; } = string.Empty;

        #endregion
    }
}