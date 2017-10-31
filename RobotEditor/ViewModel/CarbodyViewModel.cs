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
    internal class CarbodyViewModel : BaseViewModel
    {

        private readonly Dictionary<string, string> _errors = new Dictionary<string, string>();

        public CarbodyViewModel(Carbody carbody)
        {

            Model = carbody;

            _errors.Add(nameof(Path), string.Empty);
            _errors.Add(nameof(Name), string.Empty);
            _errors.Add(nameof(carbodyModel), string.Empty);
        }


        public Carbody Model { get; }


        public string Path
        {
            get { return Model.Path; }
            set
            {
                if (value.Equals(Model.Path))
                    return;

                if (value == "")
                {
                    _errors[nameof(Path)] = "No name given";
                    return;
                }

                _errors[nameof(Path)] = string.Empty;

                Model.Path = value;

                RaisePropertyChanged();
            }
        }

        public ModelVisual3D carbodyModel
        {
            get { return Model.CarbodyModel; }
            set
            {
                if (value.Equals(Model.CarbodyModel))
                    return;

                Model.CarbodyModel = value;

                RaisePropertyChanged();
            }
        }


        public BoundingBoxVisual3D boundingBox
        {
            get { return Model.BoundingBox; }
            set
            {
                if (value.Equals(Model.BoundingBox))
                    return;

                Model.BoundingBox = value;

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

    }
}
