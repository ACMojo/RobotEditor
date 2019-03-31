using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Media.Media3D;

using RobotEditor.Model;

using VirtualRobotWrapperLib.VirtualRobotManipulability;

namespace RobotEditor.ViewModel
{
    internal class RobotViewModel : BaseViewModel
    {
        #region Fields

        //private readonly Robot _robot;
        private JointViewModel _selectedJoint;

        #endregion

        #region Instance

        public RobotViewModel(Robot robot)
        {
            Model = robot;

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

        public ModelVisual3D RobotModel
        {
            get { return Model.RobotModel; }
            set
            {
                if (value.Equals(Model.RobotModel))
                    return;

                Model.RobotModel = value;

                RaisePropertyChanged();
            }
        }

        public string Name
        {
            get { return Model.Name; }
            set
            {
                if (value.Equals(Model.Name))
                    return;

                if (string.IsNullOrWhiteSpace(value))
                    return;

                Model.Name = value;

                RaisePropertyChanged();
            }
        }

        public double Precision
        {
            get { return Model.Octree?.Precision ?? 0; }
            set
            {
                if (value.Equals(Model.Octree.Precision))
                    return;

                if (Math.Abs(value) < double.Epsilon)
                    return;

                Model.Octree.Precision = value;

                RaisePropertyChanged();
            }
        }

        #endregion

        #region Public methods

        public void UpdatePrecision()
        {
            RaisePropertyChanged(nameof(Precision));
        }

        public void CalcManipulability(IVirtualRobotManipulability vrManip, Booth booth)
        {
            Model.SaveRobotStructur();

            var path = AppDomain.CurrentDomain.BaseDirectory + Model.Name;
            if (!vrManip.Init(0, null, path, "robotNodeSet", "root", "tcp"))
                return;

            var vox = vrManip.GetManipulabilityWithPenalty((float)Precision, (float)(Math.PI / 2), 100000, false, false, true, 50f);

            var minB = vrManip.MinBox;

            // Calc size of cube depending on reachability of robot
            double octreeSize;
            if (Math.Abs(vrManip.MaxBox.Max()) > Math.Abs(vrManip.MinBox.Max()))
                octreeSize = Math.Abs(vrManip.MaxBox.Max()) * 2;
            else
                octreeSize = Math.Abs(vrManip.MinBox.Max()) * 2;

            Model.Octree = VoxelOctree.Create(octreeSize, Precision);
            UpdatePrecision();

            var voxOld = vox[0];
            double maxValue = vox[0].Value;
            for (var j = 1; j < vox.Length; j++)
            {
                // TODO: MaxWert gewichten, je nach Drehung zwsichen Roboter und Fahrzeug

                if (vox[j].X == voxOld.X && vox[j].Y == voxOld.Y && vox[j].Z == voxOld.Z)
                {
                    if (vox[j].Value > maxValue)
                        maxValue = vox[j].Value;
                }
                else
                {
                    if (!Model.Octree.Set(
                            (int)(minB[0] + voxOld.X * Precision),
                            (int)(minB[1] + voxOld.Y * Precision),
                            (int)(minB[2] + voxOld.Z * Precision),
                            maxValue))
                    {
                        var value = booth.Octree.Get(
                            (int)Math.Floor(minB[0] / Precision + voxOld.X),
                            (int)Math.Floor(minB[1] / Precision + voxOld.Y),
                            (int)Math.Floor(minB[2] / Precision + voxOld.Z));
                        if (double.IsNaN(value))
                            Console.WriteLine(
                                $@"Nicht erfolgreich bei: {Math.Floor(minB[0] / Precision + voxOld.X)} {Math.Floor(minB[1] / Precision + voxOld.Y)} {Math.Floor(minB[2] / Precision + voxOld.Z)}");
                    }

                    voxOld = vox[j];
                    maxValue = vox[j].Value;
                }
            }
        }

        #endregion

        #region Private methods

        private void AddJointExecute(object obj)
        {
            var joint = new Joint(Model.Joints.Count + 1);

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