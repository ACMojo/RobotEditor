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

            _errors.Add(nameof(Nr), string.Empty);
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
            get { return Model.JTypes; }

            set
            {
                if (value.Equals(Model.JTypes))
                    return;

                _errors[nameof(JointTypes)] = string.Empty;

                Model.JTypes = value;

                RaisePropertyChanged();
            }
        }

        public int Nr
        {
            get { return Model.Nr; }
            set
            {
                if (value.Equals(Model.Nr))
                    return;

                /*if (value < 0)
                {
                    _errors[nameof(A)] = "Too small";
                    return;
                }*/

                _errors[nameof(Nr)] = string.Empty;

                Model.Nr = value;

                RaisePropertyChanged();
            }
        }

        public double A
        {
            get { return Model.A; }
            set
            {
                if (value.Equals(Model.A))
                    return;

                /*if (value < 0)
                {
                    _errors[nameof(A)] = "Too small";
                    return;
                }*/

                _errors[nameof(A)] = string.Empty;

                Model.A = value;

                RaisePropertyChanged();
            }
        }

        public double Alpha
        {
            get { return Model.Alpha; }
            set
            {
                if (value.Equals(Model.Alpha))
                    return;

                /*if (value < 0)
                {
                    _errors[nameof(Alpha)] = "Too small";
                    return;
                }*/

                _errors[nameof(Alpha)] = string.Empty;

                Model.Alpha = value;

                RaisePropertyChanged();
            }
        }

        public double D
        {
            get { return Model.D; }
            set
            {
                if (value.Equals(Model.D))
                    return;

                /*if (value < 0)
                {
                    _errors[nameof(D)] = "Too small";
                    return;
                }*/

                _errors[nameof(D)] = string.Empty;

                Model.D = value;

                RaisePropertyChanged();
            }
        }

        public double Theta
        {
            get { return Model.Theta; }
            set
            {
                if (value.Equals(Model.Theta))
                    return;

                /*if (value < 0)
                {
                    _errors[nameof(Theta)] = "Too small";
                    return;
                }*/

                _errors[nameof(Theta)] = string.Empty;

                Model.Theta = value;

                RaisePropertyChanged();
            }
        }

        public double MaxLim
        {
            get { return Model.MaxLim; }
            set
            {
                if (value.Equals(Model.MaxLim))
                    return;

                /*if (value < 0)
                {
                    _errors[nameof(MaxLim)] = "Too small";
                    return;
                }*/

                _errors[nameof(MaxLim)] = string.Empty;

                Model.MaxLim = value;

                RaisePropertyChanged();
            }
        }

        public double MinLim
        {
            get { return Model.MinLim; }
            set
            {
                if (value.Equals(Model.MinLim))
                    return;

                /*if (value < 0)
                {
                    _errors[nameof(MinLim)] = "Too small";
                    return;
                }*/

                _errors[nameof(MinLim)] = string.Empty;

                Model.MinLim = value;

                RaisePropertyChanged();
            }
        }

        public double Speed
        {
            get { return Model.Speed; }
            set
            {
                if (value.Equals(Model.Speed))
                    return;

                /*if (value < 0)
                {
                    _errors[nameof(Speed)] = "Too small";
                    return;
                }*/

                _errors[nameof(Speed)] = string.Empty;

                Model.Speed = value;

                RaisePropertyChanged();
            }
        }

        public double Acceleration
        {
            get { return Model.Acceleration; }
            set
            {
                if (value.Equals(Model.Acceleration))
                    return;

                /*if (value < 0)
                {
                    _errors[nameof(Acceleration)] = "Too small";
                    return;
                }*/

                _errors[nameof(Acceleration)] = string.Empty;

                Model.Acceleration = value;

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