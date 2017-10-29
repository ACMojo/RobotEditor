using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

using RobotEditor.Model;
using RobotEditor.View;

namespace RobotEditor.ViewModel
{
    internal class RobotViewModel : BaseViewModel
    {
        #region Fields

        //private readonly Robot _robot;
        private JointViewModel _selectedJoint;

        private readonly Dictionary<string, string> _errors = new Dictionary<string, string>();

        #endregion

        #region Instance

        public RobotViewModel(Robot robot)
        {

            Model = robot;

            _errors.Add(nameof(Name), string.Empty);

            foreach (var joint in Model.Joints)
                Joints.Add(new JointViewModel(joint));

            AddJoint = new DelegateCommand<object>(AddJointExecute, AddJointCanExecute);
            DeleteJoint = new DelegateCommand<object>(DeleteJointExecute, DeleteJointCanExecute);

            Joints.CollectionChanged += Joints_CollectionChanged;
        }

        #endregion

        #region Properties

        public Robot Model { get; }

        public ObservableCollection<JointViewModel> Joints { get; } = new ObservableCollection<JointViewModel>();

        public DelegateCommand<object> AddJoint { get; }
        public DelegateCommand<object> DeleteJoint { get; }

        public JointViewModel SelectedJoint
        {
            get { return _selectedJoint; }
            set
            {
                if (Equals(value, _selectedJoint))
                    return;

                _selectedJoint = value;
                RaisePropertyChanged();

                DeleteJoint.RaisePropertyChanged();
            }
        }

        #endregion

        #region Private methods

        public string Name
        {
            get { return Model.Name; }
            set
            {
                if (value.Equals(Model.Name))
                    return;

                if (value == "")
                {
                    _errors[nameof(Name)] = "No name given";
                    return;
                }

                _errors[nameof(Name)] = string.Empty;

                Model.Name = value;

                RaisePropertyChanged();
            }
        }


        private void AddJointExecute(object obj)
        {
            var joint = new Joint();

            Joints.Add(new JointViewModel(joint));

            AddJoint.RaisePropertyChanged();
        }

        private bool AddJointCanExecute(object arg)
        {
            return true;
        }


        private void DeleteJointExecute(object obj)
        {
            Joints.Remove(_selectedJoint);

            RaisePropertyChanged();
        }

        private bool DeleteJointCanExecute(object arg)
        {
            return SelectedJoint != null;
        }

        private void Joints_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var oldJoints = e.OldItems?.Cast<JointViewModel>();
            var newJoints = e.NewItems?.Cast<JointViewModel>();

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (newJoints != null)
                    {
                        foreach (var joint in newJoints)
                            Model.Joints.Add(joint.Model);
                    }

                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (oldJoints != null)
                    {
                        foreach (var joint in oldJoints)
                            Model.Joints.Remove(joint.Model);
                    }

                    break;
                case NotifyCollectionChangedAction.Replace:
                    //TODO
                    break;
                case NotifyCollectionChangedAction.Move:
                    //TODO
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Model.Joints.Clear();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion
    }
}