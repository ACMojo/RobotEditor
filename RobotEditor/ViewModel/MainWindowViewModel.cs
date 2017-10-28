using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RobotEditor.Model;
using RobotEditor.View;

namespace RobotEditor.ViewModel
{
    internal class MainWindowViewModel : BaseViewModel

    {

        private RobotViewModel _selectedRobot;

        public MainWindowViewModel()
        {


            AddRobot = new DelegateCommand<object>(AddRobotExecute, AddRobotCanExecute);
            DeleteRobot = new DelegateCommand<object>(DeleteRobotExecute, DeleteRobotCanExecute);
        }


        public ObservableCollection<RobotViewModel> Robots { get; } = new ObservableCollection<RobotViewModel>();

        public DelegateCommand<object> AddRobot { get; }
        public DelegateCommand<object> DeleteRobot { get; }


        public RobotViewModel SelectedRobot
        {
            get { return _selectedRobot; }
            set
            {
                if (Equals(value, _selectedRobot))
                    return;

                _selectedRobot = value;
                RaisePropertyChanged();

                DeleteRobot.RaisePropertyChanged();
            }
        }


        private bool AddRobotCanExecute(object arg)
        {
            return true;
        }

        private void AddRobotExecute(object obj)
        {
            var robot = new Robot(0, "Roboter " + Robots.Count);
            var newRobot = new RobotValues { DataContext = new RobotViewModel(robot) };
            var result = newRobot.ShowDialog();

            if (result == true)
                Robots.Add(new RobotViewModel(robot));

            if (result != true)
                return;
        }

        private void DeleteRobotExecute(object obj)
        {
            Robots.Remove(_selectedRobot);

            RaisePropertyChanged();
        }

        private bool DeleteRobotCanExecute(object arg)
        {
            return SelectedRobot != null;
        }
    }
}
