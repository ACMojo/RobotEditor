using HelixToolkit.Wpf;
using RobotEditor.Helper;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace RobotEditor.Model
{
    internal class Booth
    {
        #region Instance

        public Booth(string robotName, double bestMatch, double computationTime, VoxelOctree mixedCarsOctree, Robot robot)
        {
            MixedCarsOctree = mixedCarsOctree;
            RobotOctree = robot.Octree;
            RobotName = robotName;
            BestMatch = bestMatch;
            ComputationTime = computationTime;
            
            ResultOctree = VoxelOctree.Create(10000d, mixedCarsOctree.Precision);
            ResultOctree.Add(robot.Octree);
            ResultOctree.Add(mixedCarsOctree);

            BoothModel = new ModelVisual3D();

        }

        #endregion

        #region Properties

        public string RobotName { get; set; }
        public double BestMatch { get; set; }
        public double ComputationTime { get; set; }


        public VoxelOctree MixedCarsOctree { get; }
        public VoxelOctree RobotOctree { get; }
        public VoxelOctree ResultOctree { get; }
        public ModelVisual3D BoothModel { get; set; }

        #endregion


        public void Show3DBooth()
        {
            var maxValue = 0.0;
            for (var h = ResultOctree.StartIndexPerLevel[ResultOctree.Level - 2]; h < ResultOctree.StartIndexPerLevel[ResultOctree.Level - 1]; h++)
            {
                if (ResultOctree.Nodes[h] == null)
                    continue;

                if (((VoxelNodeInner)ResultOctree.Nodes[h]).Max > maxValue)
                    maxValue = ((VoxelNodeInner)ResultOctree.Nodes[h]).Max;
            }

            var i = ResultOctree.StartIndexLeafNodes - 1;
            foreach (var node in ResultOctree.GetLeafNodes())
            {
                i++;
                if (node == null)
                    continue;

                var vm = new MeshGeometryVisual3D();
                var mb = new MeshBuilder();
                var startOffset = new Point3D(0, 0, 0);

                var k = i;
                for (var j = 0; j < ResultOctree.Level; j++)
                {
                    var n = (k - ResultOctree.StartIndexPerLevel[ResultOctree.Level - 1 - j]) % 8;

                    switch (n)
                    {
                        case 0:
                            startOffset.X = startOffset.X + Math.Pow(2, j) * (ResultOctree.Precision / 2);
                            startOffset.Y = startOffset.Y - Math.Pow(2, j) * (ResultOctree.Precision / 2);
                            startOffset.Z = startOffset.Z - Math.Pow(2, j) * (ResultOctree.Precision / 2);
                            break;
                        case 1:
                            startOffset.X = startOffset.X + Math.Pow(2, j) * (ResultOctree.Precision / 2);
                            startOffset.Y = startOffset.Y + Math.Pow(2, j) * (ResultOctree.Precision / 2);
                            startOffset.Z = startOffset.Z - Math.Pow(2, j) * (ResultOctree.Precision / 2);
                            break;
                        case 2:
                            startOffset.X = startOffset.X - Math.Pow(2, j) * (ResultOctree.Precision / 2);
                            startOffset.Y = startOffset.Y - Math.Pow(2, j) * (ResultOctree.Precision / 2);
                            startOffset.Z = startOffset.Z - Math.Pow(2, j) * (ResultOctree.Precision / 2);
                            break;
                        case 3:
                            startOffset.X = startOffset.X - Math.Pow(2, j) * (ResultOctree.Precision / 2);
                            startOffset.Y = startOffset.Y + Math.Pow(2, j) * (ResultOctree.Precision / 2);
                            startOffset.Z = startOffset.Z - Math.Pow(2, j) * (ResultOctree.Precision / 2);
                            break;
                        case 4:
                            startOffset.X = startOffset.X + Math.Pow(2, j) * (ResultOctree.Precision / 2);
                            startOffset.Y = startOffset.Y - Math.Pow(2, j) * (ResultOctree.Precision / 2);
                            startOffset.Z = startOffset.Z + Math.Pow(2, j) * (ResultOctree.Precision / 2);
                            break;
                        case 5:
                            startOffset.X = startOffset.X + Math.Pow(2, j) * (ResultOctree.Precision / 2);
                            startOffset.Y = startOffset.Y + Math.Pow(2, j) * (ResultOctree.Precision / 2);
                            startOffset.Z = startOffset.Z + Math.Pow(2, j) * (ResultOctree.Precision / 2);
                            break;
                        case 6:
                            startOffset.X = startOffset.X - Math.Pow(2, j) * (ResultOctree.Precision / 2);
                            startOffset.Y = startOffset.Y - Math.Pow(2, j) * (ResultOctree.Precision / 2);
                            startOffset.Z = startOffset.Z + Math.Pow(2, j) * (ResultOctree.Precision / 2);
                            break;
                        case 7:
                            startOffset.X = startOffset.X - Math.Pow(2, j) * (ResultOctree.Precision / 2);
                            startOffset.Y = startOffset.Y + Math.Pow(2, j) * (ResultOctree.Precision / 2);
                            startOffset.Z = startOffset.Z + Math.Pow(2, j) * (ResultOctree.Precision / 2);
                            break;
                        default:
                            Console.WriteLine(@"Fehler");
                            break;
                    }

                    if (j < ResultOctree.Level - 1)
                        k = ResultOctree.StartIndexPerLevel[ResultOctree.Level - 2 - j] + (k - ResultOctree.StartIndexPerLevel[ResultOctree.Level - 1 - j]) / 8;
                    //k = (int)Math.Floor((double)k / 8);
                }

                vm.Material = MaterialHelper.CreateMaterial(ColorGradient.GetColorForValue(node.Value, maxValue, 0.0));
                mb.AddBox(new Point3D(startOffset.X, startOffset.Y, startOffset.Z), ResultOctree.Precision/2, ResultOctree.Precision / 2, ResultOctree.Precision / 2);
                vm.MeshGeometry = mb.ToMesh();
                BoothModel.Children.Add(vm);
            }
        }

        public void Hide3DBooth()
        {
            ;
        }
    }
}