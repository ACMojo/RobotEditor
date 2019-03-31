using System;
using System.Collections.ObjectModel;
using System.Windows.Media.Media3D;

using HelixToolkit.Wpf;

using RobotEditor.Helper;
using RobotEditor.Model;

namespace RobotEditor.ViewModel
{
    internal class ResultViewModel : BaseViewModel
    {
        #region Fields

        private readonly IHelixViewport3D _viewportResult;
        private bool _isBusy;
        public VoxelOctree Octree;

        #endregion

        #region Instance

        public ResultViewModel(HelixViewport3D viewportResult, VoxelOctree value)
        {
            Start = new DelegateCommand<object>(StartExecute, StartCanExecute);
            FitToView = new DelegateCommand<object>(FitToViewExecute, FitToViewCanExecute);
            Octree = value;

            _viewportResult = viewportResult;
            viewportResult.Viewport.Children.Add(new CoordinateSystemVisual3D { ArrowLengths = 100.0 });
            viewportResult.Viewport.Children.Add(new DefaultLights());

            Booths.Add(new BoothViewModel(new Booth("Puma 560", 1560.1, 181.23)));
            Booths.Add(new BoothViewModel(new Booth("Fanuc P250", 6531.45, 267.09)));
            Booths.Add(new BoothViewModel(new Booth("EcoRP L033", 6441.34, 254.99)));
        }

        #endregion

        #region Properties

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                if (_isBusy == value)
                    return;

                _isBusy = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<BoothViewModel> Booths { get; } = new ObservableCollection<BoothViewModel>();

        public DelegateCommand<object> Start { get; }
        public DelegateCommand<object> FitToView { get; }

        #endregion

        #region Private methods

        private bool StartCanExecute(object arg)
        {
            return true;
        }

        private void StartExecute(object obj)
        {
            //var octreeBooth = VoxelOctree.Create(800d, 100d);
            //octreeBooth.Set(0, 0, 0, 1);
            //octreeBooth.Set(50, -50, -50, 2);
            //octreeBooth.Set(50, -50, 50, 3);
            //octreeBooth.Set(50, 50, 50, 4);

            Console.WriteLine(@"Level: {0} / Nodes: {1}", Octree.Level, Octree.Nodes.Length);

            //draw last level
            var i = Octree.StartIndexLeafNodes - 1;
            var maxValue = 0;

            for (var h = Octree.StartIndexPerLevel[Octree.Level - 2]; h < Octree.StartIndexPerLevel[Octree.Level - 1]; h++)
            {
                if (Octree.Nodes[h] == null)
                    continue;

                if (((VoxelNodeInner)Octree.Nodes[h]).Max > maxValue)
                    maxValue = (int)((VoxelNodeInner)Octree.Nodes[h]).Max;
            }

            foreach (var node in Octree.GetLeafNodes())
            {
                i++;
                if (node == null)
                    continue;

                var vm = new MeshGeometryVisual3D();
                var mb = new MeshBuilder();
                var startOffset = new Point3D(0, 0, 0);

                var k = i;
                for (var j = 0; j < Octree.Level; j++)
                {
                    var n = (k - Octree.StartIndexPerLevel[Octree.Level - 1 - j]) % 8;

                    switch (n)
                    {
                        case 0:
                            startOffset.X = startOffset.X + Math.Pow(2, j) * (Octree.Precision / 2);
                            startOffset.Y = startOffset.Y - Math.Pow(2, j) * (Octree.Precision / 2);
                            startOffset.Z = startOffset.Z - Math.Pow(2, j) * (Octree.Precision / 2);
                            break;
                        case 1:
                            startOffset.X = startOffset.X + Math.Pow(2, j) * (Octree.Precision / 2);
                            startOffset.Y = startOffset.Y + Math.Pow(2, j) * (Octree.Precision / 2);
                            startOffset.Z = startOffset.Z - Math.Pow(2, j) * (Octree.Precision / 2);
                            break;
                        case 2:
                            startOffset.X = startOffset.X - Math.Pow(2, j) * (Octree.Precision / 2);
                            startOffset.Y = startOffset.Y - Math.Pow(2, j) * (Octree.Precision / 2);
                            startOffset.Z = startOffset.Z - Math.Pow(2, j) * (Octree.Precision / 2);
                            break;
                        case 3:
                            startOffset.X = startOffset.X - Math.Pow(2, j) * (Octree.Precision / 2);
                            startOffset.Y = startOffset.Y + Math.Pow(2, j) * (Octree.Precision / 2);
                            startOffset.Z = startOffset.Z - Math.Pow(2, j) * (Octree.Precision / 2);
                            break;
                        case 4:
                            startOffset.X = startOffset.X + Math.Pow(2, j) * (Octree.Precision / 2);
                            startOffset.Y = startOffset.Y - Math.Pow(2, j) * (Octree.Precision / 2);
                            startOffset.Z = startOffset.Z + Math.Pow(2, j) * (Octree.Precision / 2);
                            break;
                        case 5:
                            startOffset.X = startOffset.X + Math.Pow(2, j) * (Octree.Precision / 2);
                            startOffset.Y = startOffset.Y + Math.Pow(2, j) * (Octree.Precision / 2);
                            startOffset.Z = startOffset.Z + Math.Pow(2, j) * (Octree.Precision / 2);
                            break;
                        case 6:
                            startOffset.X = startOffset.X - Math.Pow(2, j) * (Octree.Precision / 2);
                            startOffset.Y = startOffset.Y - Math.Pow(2, j) * (Octree.Precision / 2);
                            startOffset.Z = startOffset.Z + Math.Pow(2, j) * (Octree.Precision / 2);
                            break;
                        case 7:
                            startOffset.X = startOffset.X - Math.Pow(2, j) * (Octree.Precision / 2);
                            startOffset.Y = startOffset.Y + Math.Pow(2, j) * (Octree.Precision / 2);
                            startOffset.Z = startOffset.Z + Math.Pow(2, j) * (Octree.Precision / 2);
                            break;
                        default:
                            Console.WriteLine(@"Fehler");
                            break;
                    }

                    if (j < Octree.Level - 1)
                        k = Octree.StartIndexPerLevel[Octree.Level - 2 - j] + (k - Octree.StartIndexPerLevel[Octree.Level - 1 - j]) / 8;
                    //k = (int)Math.Floor((double)k / 8);
                }

                /*
                switch (i%8)
                {
                    case 0:
                        vm.Material = MaterialHelper.CreateMaterial(Colors.Red);
                        break;
                    case 1:
                        vm.Material = MaterialHelper.CreateMaterial(Colors.Purple);
                        break;
                    case 2:
                        vm.Material = MaterialHelper.CreateMaterial(Colors.Gray);
                        break;
                    case 3:
                        vm.Material = MaterialHelper.CreateMaterial(Colors.Green);
                        break;
                    case 4:
                        vm.Material = MaterialHelper.CreateMaterial(Colors.Orange);
                        break;
                    case 5:
                        vm.Material = MaterialHelper.CreateMaterial(Colors.DarkBlue);
                        break;
                    case 6:
                        vm.Material = MaterialHelper.CreateMaterial(Colors.LightBlue);
                        break;
                    case 7:
                        vm.Material = MaterialHelper.CreateMaterial(Colors.Yellow);
                        break;
                    default:
                        Console.WriteLine("Fehler");
                        break;
                }
                */

                vm.Material = MaterialHelper.CreateMaterial(ColorGradient.GetColorForValue(node.Value, maxValue, 1.0));
                mb.AddBox(new Point3D(startOffset.X, startOffset.Y, startOffset.Z), 100.0, 100.0, 100.0);
                vm.MeshGeometry = mb.ToMesh();
                _viewportResult.Viewport.Children.Add(vm);

                /*
                if (node == null)
                {
                    vm.Material = MaterialHelper.CreateMaterial(Colors.White);
                }
                else
                {
                    switch (node.Value)
                    {
                        case 1:
                            vm.Material = MaterialHelper.CreateMaterial(Colors.Red);
                            break;
                        case 2:
                            vm.Material = MaterialHelper.CreateMaterial(Colors.Yellow);
                            break;
                        case 3:
                            vm.Material = MaterialHelper.CreateMaterial(Colors.DarkBlue);
                            break;
                        case 4:
                            vm.Material = MaterialHelper.CreateMaterial(Colors.LightBlue);
                            break;
                        case 5:
                            vm.Material = MaterialHelper.CreateMaterial(Colors.Yellow);
                            break;
                        default:
                            vm.Material = MaterialHelper.CreateMaterial(Colors.Green);
                            break;
                    }

                   


                }*/
            }

            //octreeBooth.Set(0, 5, 0, 3);
            //octreeBooth.Set(0, 0, 5, 4);
            //octreeBooth.Set(5, 5, 5, 7);
            //octreeBooth.Set(0, 0, -32, 234234);
        }

        private bool FitToViewCanExecute(object arg)
        {
            return true;
        }

        private void FitToViewExecute(object obj)
        {
            _viewportResult.ZoomExtents(0);
        }

        #endregion
    }
}