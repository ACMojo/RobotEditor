using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

using RobotEditor.Model;
using RobotEditor.View;
using System.Windows.Media;

namespace RobotEditor.ViewModel
{
    internal class ResultViewModel : BaseViewModel
    {
        #region Fields

        private readonly IHelixViewport3D viewportResult;

        #endregion

        #region Instance

        public ResultViewModel(HelixViewport3D viewportResult)
        {
            Start = new DelegateCommand<object>(StartExecute, StartCanExecute);
            FitToView = new DelegateCommand<object>(FitToViewExecute, FitToViewCanExecute);


            this.viewportResult = viewportResult;
            viewportResult.Viewport.Children.Add(new CoordinateSystemVisual3D() { ArrowLengths = 100.0 });
            viewportResult.Viewport.Children.Add(new DefaultLights());
        }


        #endregion

        public DelegateCommand<object> Start { get; }
        public DelegateCommand<object> FitToView { get; }

        private bool StartCanExecute(object arg)
        {
            return true;
        }

        private void StartExecute(object obj)
        {
            var octreeBooth = VoxelOctree.Create(800d, 100d);
            octreeBooth.Set(0, 0, 0, 1);
            octreeBooth.Set(250, -150, -150, 2);
            octreeBooth.Set(50, -50, 350, 3);
            octreeBooth.Set(200, 450, 250, 4);
            Console.WriteLine(@"Level: {0} / Nodes: {1}", octreeBooth.Level, octreeBooth.Nodes.Length);

            //draw last level
            int i = 0;
            foreach (var node in octreeBooth.GetLeafNodes())
            {

                var vm = new MeshGeometryVisual3D();
                var mb = new MeshBuilder();
                var StartOffset = new Point3D(0, 0, 0);

                int n = 0;
                int k = i;
                for (int j = 0; j < octreeBooth.Level; j++)
                {
                    n = k % 8;
                    k = (int)Math.Floor(k / 8d);

                    switch (n)
                    {
                        case 0:
                            StartOffset.X = StartOffset.X + (Math.Pow(2, j) * (100 / 2));
                            StartOffset.Y = StartOffset.Y - (Math.Pow(2, j) * (100 / 2));
                            StartOffset.Z = StartOffset.Z - (Math.Pow(2, j) * (100 / 2));
                            break;
                        case 1:
                            StartOffset.X = StartOffset.X + (Math.Pow(2, j) * (100 / 2));
                            StartOffset.Y = StartOffset.Y + (Math.Pow(2, j) * (100 / 2));
                            StartOffset.Z = StartOffset.Z - (Math.Pow(2, j) * (100 / 2));
                            break;
                        case 2:
                            StartOffset.X = StartOffset.X - (Math.Pow(2, j) * (100 / 2));
                            StartOffset.Y = StartOffset.Y - (Math.Pow(2, j) * (100 / 2));
                            StartOffset.Z = StartOffset.Z - (Math.Pow(2, j) * (100 / 2));
                            break;
                        case 3:
                            StartOffset.X = StartOffset.X - (Math.Pow(2, j) * (100 / 2));
                            StartOffset.Y = StartOffset.Y + (Math.Pow(2, j) * (100 / 2));
                            StartOffset.Z = StartOffset.Z - (Math.Pow(2, j) * (100 / 2));
                            break;
                        case 4:
                            StartOffset.X = StartOffset.X + (Math.Pow(2, j) * (100 / 2));
                            StartOffset.Y = StartOffset.Y - (Math.Pow(2, j) * (100 / 2));
                            StartOffset.Z = StartOffset.Z + (Math.Pow(2, j) * (100 / 2));
                            break;
                        case 5:
                            StartOffset.X = StartOffset.X + (Math.Pow(2, j) * (100 / 2));
                            StartOffset.Y = StartOffset.Y + (Math.Pow(2, j) * (100 / 2));
                            StartOffset.Z = StartOffset.Z + (Math.Pow(2, j) * (100 / 2));
                            break;
                        case 6:
                            StartOffset.X = StartOffset.X - (Math.Pow(2, j) * (100 / 2));
                            StartOffset.Y = StartOffset.Y - (Math.Pow(2, j) * (100 / 2));
                            StartOffset.Z = StartOffset.Z + (Math.Pow(2, j) * (100 / 2));
                            break;
                        case 7:
                            StartOffset.X = StartOffset.X - (Math.Pow(2, j) * (100 / 2));
                            StartOffset.Y = StartOffset.Y + (Math.Pow(2, j) * (100 / 2));
                            StartOffset.Z = StartOffset.Z + (Math.Pow(2, j) * (100 / 2));
                            break;
                        default:
                            Console.WriteLine("Fehler");
                            break;
                    }

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

                }


                mb.AddBox(new Point3D(StartOffset.X, StartOffset.Y, StartOffset.Z), 10.0, 10.0, 10.0);
              
                vm.MeshGeometry = mb.ToMesh();               
                viewportResult.Viewport.Children.Add(vm);


                i++;

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
            viewportResult.ZoomExtents(0);
        }


        #region Properties



        #endregion

        #region Private methods

        #endregion
    }
}