using HelixToolkit.Wpf;
using RobotEditor.Helper;
using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace RobotEditor.Model
{
    internal class Booth
    {
        #region Instance

        public Booth(string robotName, VoxelOctree mixedCarsOctree, Robot robot)
        {
            MixedCarsOctree = mixedCarsOctree;
            MixedCarsOctreeCopy = mixedCarsOctree;
            RobotOctree = robot.Octree;
            RobotName = robotName;
            BestMatch = 0.0;
            ComputationTime = 0.0;
            LowerBound = 0.0;
            Cycles = 0;
            XPos = 0;
            YPos = 0;
            ZPos = 0;
            
            ResultOctree = VoxelOctree.Create(10000d, mixedCarsOctree.Precision);

            BoothModel = new ModelVisual3D();

        }

        #endregion

        #region Properties

        public string RobotName { get; set; }
        public double BestMatch { get; set; }
        public double ComputationTime { get; set; }
        public int Cycles { get; set; }
        public int XPos { get; set; }
        public int YPos { get; set; }
        public int ZPos { get; set; }
        public double LowerBound { get; set; }


        public VoxelOctree MixedCarsOctree { get; }
        public VoxelOctree MixedCarsOctreeCopy { get; set; }
        public VoxelOctree RobotOctree { get; }
        public VoxelOctree ResultOctree { get; }
        public ModelVisual3D BoothModel { get; set; }

        #endregion

        public void RenewMixedCarsCopy()
        {
            MixedCarsOctreeCopy = MixedCarsOctree.Clone();
        }

        public void Show3DBooth()
        {
            var maxValue = ((VoxelNodeInner)ResultOctree.Nodes[0]).Max;

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
                    
                    var n = (k - ResultOctree.StartIndexPerLevel[ResultOctree.Level - j]) % 8;
                    
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
                    
                    /*
                    // DELETE START
                    switch (node.Value)
                    {
                        case 1:
                            vm.Material = MaterialHelper.CreateMaterial(Colors.Red);
                            break;
                        case 2:
                            vm.Material = MaterialHelper.CreateMaterial(Colors.Purple);
                            break;
                        case 3:
                            vm.Material = MaterialHelper.CreateMaterial(Colors.Gray);
                            break;
                        case 4:
                            vm.Material = MaterialHelper.CreateMaterial(Colors.Green);
                            break;
                        case 5:
                            vm.Material = MaterialHelper.CreateMaterial(Colors.Orange);
                            break;
                        case 6:
                            vm.Material = MaterialHelper.CreateMaterial(Colors.DarkBlue);
                            break;
                        case 7:
                            vm.Material = MaterialHelper.CreateMaterial(Colors.Cyan);
                            break;
                        case 8:
                            vm.Material = MaterialHelper.CreateMaterial(Colors.Yellow);
                            break;
                        default:
                            vm.Material = MaterialHelper.CreateMaterial(Colors.Wheat);
                            break;
                    }
                    // DELETE END 
                    */
                    if (j < ResultOctree.Level)
                        k = ResultOctree.StartIndexPerLevel[ResultOctree.Level - 1 - j] + (k - ResultOctree.StartIndexPerLevel[ResultOctree.Level - j]) / 8;
                    //k = (int)Math.Floor((double)k / 8);
                }


                //INSERT
                vm.Material = MaterialHelper.CreateMaterial(ColorGradient.GetColorForValue(node.Value, maxValue, 0.0));     
                mb.AddBox(new Point3D(startOffset.X, startOffset.Y, startOffset.Z), ResultOctree.Precision/2, ResultOctree.Precision / 2, ResultOctree.Precision / 2);
                vm.MeshGeometry = mb.ToMesh();
                BoothModel.Children.Add(vm);
            }
        }

        public void Hide3DBooth()
        {
            BoothModel.Children.Clear(); ;
        }
    }
}