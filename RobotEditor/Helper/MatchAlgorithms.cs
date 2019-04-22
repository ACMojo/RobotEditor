using RobotEditor.Model;
using System;
using System.Windows.Media.Media3D;
using System.Linq;

namespace RobotEditor.Helper
{
    public static class MatchAlgorithms
    {
        #region Public methods

        public static int[] MatchMaxValue(VoxelOctree carTree, VoxelOctree robotTree)
        {
            if (carTree.Precision != robotTree.Precision)       // invalid as common level is not guaranteed
                return null;

            int[] translateRotateOperator = new int[6];

            int[,] rotateConversion = new int[24, 8]{
                { 0,1,2,3,4,5,6,7 },
                { 2,0,3,1,6,4,7,5 },
                { 3,2,1,0,7,6,5,4 },
                { 1,3,0,2,5,7,4,6 },
                { 0,2,4,6,1,3,5,7 },
                { 4,6,5,7,0,2,1,3 },
                { 5,7,1,3,4,6,0,2 },
                { 2,3,6,7,0,1,4,5 },
                { 3,7,2,6,1,5,0,4 },
                { 7,6,3,2,5,4,1,0 },
                { 6,2,7,3,4,0,5,1 },
                { 7,3,5,1,6,2,4,0 },
                { 5,1,4,0,7,3,6,2 },
                { 4,0,6,2,5,1,7,3 },
                { 6,4,2,0,7,5,3,1 },
                { 4,5,0,1,6,7,2,3 },
                { 5,4,7,6,1,0,3,2 },
                { 3,1,7,5,2,0,6,4 },
                { 1,0,5,4,3,2,7,6 },
                { 0,4,1,5,2,6,3,7 },
                { 1,5,3,7,0,4,2,6 },
                { 2,6,0,4,3,7,1,5 },
                { 7,5,6,4,3,1,2,0 },
                { 6,7,4,5,2,3,0,1 },};



            var lowerBound = 0.0;
            

            var startLevel = carTree.Level - robotTree.Level;

            var nodesOnLevel = carTree.GetAncestorNodes(startLevel).Where(n => n != null).OrderByDescending(n => n.Value);
            // Startlösung mit Breitensuche

            var maxPossibleMatch = 0.0;
            var maxMatchPossibleRotation = 0;
            VoxelNode maxMatchCarNode = null;
            foreach (var nodeOnLevel in nodesOnLevel)
            {
                if (nodeOnLevel.Value < lowerBound)
                    continue;

                // Find best rotation of robot
                var directChilds = carTree.GetChildNodes((VoxelNodeInner)nodeOnLevel).ToArray();
                for (var i = 0; i < 24; i++)
                {
                    var maxPossibleMatchTemp = 0.0;
                    for (var j = 0; j<8; j++)
                    {
                        if (robotTree.Nodes[rotateConversion[i, j] + 1] == null || directChilds[j] == null)
                            continue;

                        maxPossibleMatchTemp += (((VoxelNodeInner)robotTree.Nodes[rotateConversion[i,j] + 1]).Max * directChilds[j].Value); // STARTINDEX herausfinden, weil DirectChild[0] auch null sein kann
                    }
                    if (maxPossibleMatchTemp > maxPossibleMatch)
                    {
                        maxPossibleMatch = maxPossibleMatchTemp;
                        maxMatchPossibleRotation = i;
                        maxMatchCarNode = directChilds[0];
                    }
                }

                
            }

            // Find best movement of robot to all 26 neighbours
            var ind = Array.IndexOf(carTree.Nodes, maxMatchCarNode) - carTree.StartIndexPerLevel[startLevel];
            var startIndex = MatrixHelper.getAllNeighborMovements(startLevel, ind);
        
            var maxPossibleMatchTemp2 = 0.0;
            for (var i = 0; i < 26; i++)
            {
                for (var j = 0; j < 8; j++)
                {
                    maxPossibleMatchTemp2 += (((VoxelNodeInner)robotTree.Nodes[rotateConversion[maxMatchPossibleRotation, j] + 1]).Max * carTree.Nodes[startIndex[i,j]].Value);
                }

                if (maxPossibleMatchTemp2 > maxPossibleMatch)
                {
                    ;
                }
            }


            var posOperator = carTree.CalculateNodePosition(maxMatchCarNode);
            translateRotateOperator[0] = posOperator[0];
            translateRotateOperator[1] = posOperator[1];
            translateRotateOperator[2] = posOperator[2];

            var rotOperator = MatrixHelper.getRotationOffset(maxMatchPossibleRotation);
            translateRotateOperator[3] = rotOperator[0];
            translateRotateOperator[4] = rotOperator[1];
            translateRotateOperator[5] = rotOperator[2];

            return translateRotateOperator;
        }


        // Implementation copied from de.wikibooks.org/wiki/Algorithmensammlung:_Sortierverfahren:_Quicksort 

        public static void sort(ref double[] array)
        {
            quicksort(0, array.Length - 1, ref array);
        }

        private static void quicksort(int left, int right, ref double[] data)
        {
            if (left < right)
            {
                int divisor = divide(left, right, ref data);
                quicksort(left, divisor - 1, ref data);
                quicksort(divisor + 1, right, ref data);
            }
        }

        private static int divide(int left, int right, ref double[] data)
        {
            int i = left;
            int j = right - 1;
            double pivot = data[right];

            do
            {
                while (data[i] <= pivot && i < right)
                    i += 1;

                while (data[j] >= pivot && j > left)
                    j -= 1;

                if (i < j)
                {
                    double z = data[i];
                    data[i] = data[j];
                    data[j] = z;
                }

            } while (i < j);

            if (data[i] > pivot)
            {
                double z = data[i];
                data[i] = data[right];
                data[right] = z;
            }
            return i; 
        }

        #endregion
    }
}