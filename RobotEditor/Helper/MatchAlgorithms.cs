using RobotEditor.Model;
using System;
using System.Windows.Media.Media3D;
using System.Linq;
using System.Collections.Generic;

namespace RobotEditor.Helper
{
    public static class MatchAlgorithms
    {

        #region Properties

        public static double lowerBound { get; set; }
        public static double initLowerBound { get; set; }
        public static double upperBound { get; set; }
        public static int startLevel { get; set; }
        public static int endLevel { get; set; }
        public static VoxelOctree carTree { get; set; }
        public static VoxelOctree robotTree { get; set; }
        public static VoxelOctree[] rotatedRobotTrees { get; set; }
        public static int[] translateOperator { get; set; }
        public static int rotateOperator { get; set; }
        public static int cycles;

        #endregion

        #region Public methods

        public static void BestVoxelFirstSearch(IOrderedEnumerable<KeyValuePair<int, VoxelNode>> nodes, int level, int maxCycles, bool maxValue, bool maxLeafs, bool maxMax, int rotations, bool noGo, bool symmetry, double[] boundingBoxHalfExtents)
        {
            if (cycles >= maxCycles && maxCycles > 0)
                return;

            cycles++;
            var robotNodes = robotTree.GetLeafNodes(robotTree.Root);
            var relLevel = level - (carTree.Level - robotTree.Level);

            IEnumerable<VoxelNodeInner> robotMaxNodes = null;
            if (level < carTree.Level)
                robotMaxNodes = robotTree.GetAncestorNodes(relLevel).Cast<VoxelNodeInner>().Where(n => n != null).OrderByDescending(n => n.Max);

            var robotRefNodePos = robotTree.CalculateNodePosition(robotTree.StartIndexPerLevel[robotTree.Level]);

            foreach (var node in nodes)
            {
                if (noGo || symmetry)
                {
                    var diffBetweenRefs = carTree.CalculateNodePosition(node.Key);
                    for (int i = 0; i < 3; i++)
                        diffBetweenRefs[i] = diffBetweenRefs[i] - robotRefNodePos[i];

                    if (IsRobotInNoGoZone(diffBetweenRefs, relLevel, boundingBoxHalfExtents) && noGo)
                        continue;

                    if (IsRobotInSymmetryZone(diffBetweenRefs, relLevel, boundingBoxHalfExtents) && symmetry)
                        continue;
                }
                
                if (level < carTree.Level)
                {
                    var maxValueValue = 0.0;
                    var maxMaxValue = 0.0;

                    List<double> maxValueList = new List<double>();
                    List<double> maxMaxList = new List<double>();
                    List<VoxelNodeLeaf> maxLeafsList = new List<VoxelNodeLeaf>();

                    int? carZIndex = node.Key - carTree.StartIndexPerLevel[level];
                    for (int i = 0; i <= Math.Pow(2, relLevel); i++) // +Z
                    {
                        int? carYIndex = carZIndex;
                        for (int j = 0; j <= Math.Pow(2, relLevel); j++) // +Y
                        {
                            int? carXIndex = carYIndex;
                            for (int k = 0; k <= Math.Pow(2, relLevel); k++) // -X
                            {
                                if (carTree.Nodes[carTree.StartIndexPerLevel[level] + (int)carXIndex] != null)
                                {
                                    maxValueList.Add(carTree.Nodes[carTree.StartIndexPerLevel[level] + (int)carXIndex].Value);
                                    maxMaxList.Add(((VoxelNodeInner)carTree.Nodes[carTree.StartIndexPerLevel[level] + (int)carXIndex]).Max);
                                    maxLeafsList.AddRange(carTree.GetLeafNodes((VoxelNodeInner)carTree.Nodes[carTree.StartIndexPerLevel[level] + (int)carXIndex]).ToList());
                                }

                                var carXIndexTemp = MatrixHelper.moveOneInMXFromRefOnLevel(level, (int)carXIndex);

                                if (carXIndexTemp == null)
                                    break;
                                else
                                    carXIndex += (int)carXIndexTemp;
                            }
                            var carYIndexTemp = MatrixHelper.moveOneInPYFromRefOnLevel(level, (int)carYIndex);

                            if (carYIndexTemp == null)
                                break;
                            else
                                carYIndex += (int)carYIndexTemp;
                        }
                        var carZIndexTemp = MatrixHelper.moveOneInPZFromRefOnLevel(level, (int)carZIndex);

                        if (carZIndexTemp == null)
                            break;
                        else
                            carZIndex += (int)carZIndexTemp;
                    }


                    // Lower bound check maxValue and maxMax
                    var m = 0;
                    var maxValueListTemp = maxValueList.OrderByDescending(n => n);
                    foreach (var robotMaxNode in robotMaxNodes)
                    {
                        if (m >= maxValueListTemp.Count())
                            break;

                        if (maxValue)
                            maxValueValue += robotMaxNode.Max * maxValueListTemp.ElementAt(m);

                        if (maxMax)
                        {
                            var robCount = robotTree.GetLeafNodes(robotMaxNode).Where(n => n != null).Count();
                            maxMaxValue += robCount * robotMaxNode.Max * maxMaxList.ElementAt(m);
                        }

                        m++;
                    }

                    if (maxValueValue <= lowerBound && maxValue)
                        continue;

                    if (maxMaxValue <= lowerBound && maxMax)
                        continue;


                    // Lower bound check maxLeafs
                    if (maxLeafs)
                    {
                        var robLeafs = robotTree.GetLeafNodes(robotTree.Root).Where(n => n != null).OrderByDescending(n => n.Value);
                        var maxLeafsListTemp = maxLeafsList.Where(n => n != null).OrderByDescending(n => n.Value);
                        var t = 0;
                        var maxLeafsValue = 0.0;
                        foreach (var robLeaf in robLeafs)
                        {
                            if (t >= maxLeafsListTemp.Count())
                                break;

                            maxLeafsValue += robLeaf.Value * maxLeafsListTemp.ElementAt(t).Value;
                            t++;
                        }

                        if (maxLeafsValue <= lowerBound)
                            continue;
                    }

                    BestVoxelFirstSearch(carTree.GetChildNodesWithIndex(node.Key).OrderByDescending(n => n.Value != null ? n.Value.Value : 0.0), level + 1, maxCycles, maxValue, maxLeafs, maxMax, rotations, noGo, symmetry, boundingBoxHalfExtents);
                }
                else
                {
                    for (int m = 0; m < rotations; m++)
                    {
                        var value = 0.0;

                        int? robZIndex = 0;
                        int? carZIndex = node.Key - carTree.StartIndexLeafNodes;
                        for (int i = 0; i < Math.Pow(2, rotatedRobotTrees[m].Level); i++) // +Z
                        {
                            int? robYIndex = robZIndex;
                            int? carYIndex = carZIndex;
                            for (int j = 0; j < Math.Pow(2, rotatedRobotTrees[m].Level); j++) // +Y
                            {
                                int? robXIndex = robYIndex;
                                int? carXIndex = carYIndex;
                                for (int k = 0; k < Math.Pow(2, rotatedRobotTrees[m].Level); k++) // -X
                                {
                                    if (carTree.Nodes[carTree.StartIndexPerLevel[carTree.Level] + (int)carXIndex] != null && rotatedRobotTrees[m].Nodes[rotatedRobotTrees[m].StartIndexPerLevel[rotatedRobotTrees[m].Level] + (int)robXIndex] != null)
                                        value += carTree.Nodes[carTree.StartIndexPerLevel[carTree.Level] + (int)carXIndex].Value * rotatedRobotTrees[m].Nodes[rotatedRobotTrees[m].StartIndexPerLevel[rotatedRobotTrees[m].Level] + (int)robXIndex].Value;

                                    var robXIndexTemp = MatrixHelper.moveOneInMXFromRefOnLevel(rotatedRobotTrees[m].Level, (int)robXIndex);
                                    var carXIndexTemp = MatrixHelper.moveOneInMXFromRefOnLevel(carTree.Level, (int)carXIndex);

                                    if (robXIndexTemp == null || carXIndexTemp == null)
                                        break;
                                    else
                                    {
                                        robXIndex += (int)robXIndexTemp;
                                        carXIndex += (int)carXIndexTemp;
                                    }
                                }

                                var robYIndexTemp = MatrixHelper.moveOneInPYFromRefOnLevel(rotatedRobotTrees[m].Level, (int)robYIndex);
                                var carYIndexTemp = MatrixHelper.moveOneInPYFromRefOnLevel(carTree.Level, (int)carYIndex);

                                if (robYIndexTemp == null || carYIndexTemp == null)
                                    break;
                                else
                                {
                                    robYIndex += (int)robYIndexTemp;
                                    carYIndex += (int)carYIndexTemp;
                                }
                            }

                            var robZIndexTemp = MatrixHelper.moveOneInPZFromRefOnLevel(rotatedRobotTrees[m].Level, (int)robZIndex);
                            var carZIndexTemp = MatrixHelper.moveOneInPZFromRefOnLevel(carTree.Level, (int)carZIndex);

                            if (robZIndexTemp == null || carZIndexTemp == null)
                                break;
                            else
                            {
                                robZIndex += (int)robZIndexTemp;
                                carZIndex += (int)carZIndexTemp;
                            }
                        }

                        if (value > lowerBound)
                        {

                            var diffBetweenRefs = carTree.CalculateNodePosition(node.Key);
                            for (int i = 0; i < 3; i++)
                                diffBetweenRefs[i] = diffBetweenRefs[i] - robotRefNodePos[i];

                            lowerBound = value;
                            translateOperator = diffBetweenRefs;
                            rotateOperator = m;
                        }
                    }
                }
            }
        }

        public static void DepthFirstSearch(IEnumerable<KeyValuePair<int, VoxelNode>> nodes, int level, int maxCycles, bool maxValue, bool maxLeafs, bool maxMax, int rotations, bool noGo, bool symmetry, double[] boundingBoxHalfExtents)
        {
            if (cycles >= maxCycles && maxCycles > 0)
                return;

            cycles++;
            var robotNodes = robotTree.GetLeafNodes(robotTree.Root);
            var relLevel = level - (carTree.Level - robotTree.Level);

            IEnumerable<VoxelNodeInner> robotMaxNodes = null;
            if (level < carTree.Level)
                robotMaxNodes = robotTree.GetAncestorNodes(relLevel).Cast<VoxelNodeInner>().Where(n => n != null).OrderByDescending(n => n.Max);

            var robotRefNodePos = robotTree.CalculateNodePosition(robotTree.StartIndexPerLevel[robotTree.Level]);
            
            foreach (var node in nodes)
            {
                if (noGo || symmetry)
                {
                    var diffBetweenRefs = carTree.CalculateNodePosition(node.Key);
                    for (int i = 0; i < 3; i++)
                        diffBetweenRefs[i] = diffBetweenRefs[i] - robotRefNodePos[i];

                    if (IsRobotInNoGoZone(diffBetweenRefs, relLevel, boundingBoxHalfExtents) && noGo)
                        continue;

                    if (IsRobotInSymmetryZone(diffBetweenRefs, relLevel, boundingBoxHalfExtents) && symmetry)
                        continue;
                }

                if (level < carTree.Level)
                {
                    var maxValueValue = 0.0;
                    var maxMaxValue = 0.0;                   
                    
                    List<double> maxValueList = new List<double>();
                    List<double> maxMaxList = new List<double>();
                    List<VoxelNodeLeaf> maxLeafsList = new List<VoxelNodeLeaf>();

                    int? carZIndex = node.Key - carTree.StartIndexPerLevel[level];
                    for (int i = 0; i <= Math.Pow(2, relLevel); i++) // +Z
                    {
                        int? carYIndex = carZIndex;
                        for (int j = 0; j <= Math.Pow(2, relLevel); j++) // +Y
                        {
                            int? carXIndex = carYIndex;
                            for (int k = 0; k <= Math.Pow(2, relLevel); k++) // -X
                            {
                                if (carTree.Nodes[carTree.StartIndexPerLevel[level] + (int)carXIndex] != null)
                                {
                                    maxValueList.Add(carTree.Nodes[carTree.StartIndexPerLevel[level] + (int)carXIndex].Value);
                                    maxMaxList.Add(((VoxelNodeInner)carTree.Nodes[carTree.StartIndexPerLevel[level] + (int)carXIndex]).Max);
                                    maxLeafsList.AddRange(carTree.GetLeafNodes((VoxelNodeInner)carTree.Nodes[carTree.StartIndexPerLevel[level] + (int)carXIndex]).ToList());                                 
                                }

                                var carXIndexTemp = MatrixHelper.moveOneInMXFromRefOnLevel(level, (int)carXIndex);

                                if (carXIndexTemp == null)
                                    break;
                                else
                                    carXIndex += (int)carXIndexTemp;
                            }
                            var carYIndexTemp = MatrixHelper.moveOneInPYFromRefOnLevel(level, (int)carYIndex);

                            if (carYIndexTemp == null)
                                break;
                            else
                                carYIndex += (int)carYIndexTemp;
                        }
                        var carZIndexTemp = MatrixHelper.moveOneInPZFromRefOnLevel(level, (int)carZIndex);

                        if (carZIndexTemp == null)
                            break;
                        else
                            carZIndex += (int)carZIndexTemp;
                    }


                    // Lower bound check maxValue and maxMax
                    var m = 0;
                    var maxValueListTemp = maxValueList.OrderByDescending(n => n);
                    foreach (var robotMaxNode in robotMaxNodes)
                    {
                        if (m >= maxValueListTemp.Count())
                            break;

                        if(maxValue)
                            maxValueValue += robotMaxNode.Max * maxValueListTemp.ElementAt(m);

                        if (maxMax)
                        {
                            var robCount = robotTree.GetLeafNodes(robotMaxNode).Where(n => n != null).Count();
                            maxMaxValue += robCount * robotMaxNode.Max * maxMaxList.ElementAt(m);
                        }

                        m++;
                    }

                    if (maxValueValue <= lowerBound && maxValue)
                        continue;

                    if (maxMaxValue <= lowerBound && maxMax)
                        continue;


                    // Lower bound check maxLeafs
                    if (maxLeafs)
                    {
                        var robLeafs = robotTree.GetLeafNodes(robotTree.Root).Where(n => n != null).OrderByDescending(n => n.Value);
                        var maxLeafsListTemp = maxLeafsList.Where(n => n != null).OrderByDescending(n => n.Value);
                        var t = 0;
                        var maxLeafsValue = 0.0;
                        foreach (var robLeaf in robLeafs)
                        {
                            if (t >= maxLeafsListTemp.Count())
                                break;

                            maxLeafsValue += robLeaf.Value * maxLeafsListTemp.ElementAt(t).Value;
                            t++;
                        }

                        if (maxLeafsValue <= lowerBound)
                            continue;
                    }

                    DepthFirstSearch(carTree.GetChildNodesWithIndex(node.Key), level + 1, maxCycles, maxValue, maxLeafs, maxMax, rotations, noGo, symmetry, boundingBoxHalfExtents);
                }
                else
                {
                    for (int m = 0; m < rotations; m++)
                    {
                        var value = 0.0;

                        int? robZIndex = 0;
                        int? carZIndex = node.Key - carTree.StartIndexLeafNodes;
                        for (int i = 0; i < Math.Pow(2, rotatedRobotTrees[m].Level); i++) // +Z
                        {
                            int? robYIndex = robZIndex;
                            int? carYIndex = carZIndex;
                            for (int j = 0; j < Math.Pow(2, rotatedRobotTrees[m].Level); j++) // +Y
                            {
                                int? robXIndex = robYIndex;
                                int? carXIndex = carYIndex;
                                for (int k = 0; k < Math.Pow(2, rotatedRobotTrees[m].Level); k++) // -X
                                {
                                    if (carTree.Nodes[carTree.StartIndexPerLevel[carTree.Level] + (int)carXIndex] != null && rotatedRobotTrees[m].Nodes[rotatedRobotTrees[m].StartIndexPerLevel[rotatedRobotTrees[m].Level] + (int)robXIndex] != null)
                                        value += carTree.Nodes[carTree.StartIndexPerLevel[carTree.Level] + (int)carXIndex].Value * rotatedRobotTrees[m].Nodes[rotatedRobotTrees[m].StartIndexPerLevel[rotatedRobotTrees[m].Level] + (int)robXIndex].Value;

                                    var robXIndexTemp = MatrixHelper.moveOneInMXFromRefOnLevel(rotatedRobotTrees[m].Level, (int)robXIndex);
                                    var carXIndexTemp = MatrixHelper.moveOneInMXFromRefOnLevel(carTree.Level, (int)carXIndex);

                                    if (robXIndexTemp == null || carXIndexTemp == null)
                                        break;
                                    else
                                    {
                                        robXIndex += (int)robXIndexTemp;
                                        carXIndex += (int)carXIndexTemp;
                                    }
                                }

                                var robYIndexTemp = MatrixHelper.moveOneInPYFromRefOnLevel(rotatedRobotTrees[m].Level, (int)robYIndex);
                                var carYIndexTemp = MatrixHelper.moveOneInPYFromRefOnLevel(carTree.Level, (int)carYIndex);

                                if (robYIndexTemp == null || carYIndexTemp == null)
                                    break;
                                else
                                {
                                    robYIndex += (int)robYIndexTemp;
                                    carYIndex += (int)carYIndexTemp;
                                }
                            }

                            var robZIndexTemp = MatrixHelper.moveOneInPZFromRefOnLevel(rotatedRobotTrees[m].Level, (int)robZIndex);
                            var carZIndexTemp = MatrixHelper.moveOneInPZFromRefOnLevel(carTree.Level, (int)carZIndex);

                            if (robZIndexTemp == null || carZIndexTemp == null)
                                break;
                            else
                            {
                                robZIndex += (int)robZIndexTemp;
                                carZIndex += (int)carZIndexTemp;
                            }
                        }

                        if (value > lowerBound)
                        {

                            var diffBetweenRefs = carTree.CalculateNodePosition(node.Key);
                            for (int i = 0; i < 3; i++)
                                diffBetweenRefs[i] = diffBetweenRefs[i] - robotRefNodePos[i];

                            lowerBound = value;
                            translateOperator = diffBetweenRefs;
                            rotateOperator = m;
                        }
                    }
                }
            }
        }

        public static void BruteForce(IEnumerable<KeyValuePair<int, VoxelNode>> nodes, int level, int maxCycles, int rotations, bool noGo, bool symmetry, double[] boundingBoxHalfExtents)
        {
            if (cycles >= maxCycles && maxCycles > 0)
                return;

            cycles++;
            var robotNodes = robotTree.GetLeafNodes(robotTree.Root);
            var relLevel = level - (carTree.Level - robotTree.Level);

            var robotRefNodePos = robotTree.CalculateNodePosition(robotTree.StartIndexPerLevel[robotTree.Level]);

            foreach (var node in nodes)
            {
                if (noGo || symmetry)
                {
                    var diffBetweenRefs = carTree.CalculateNodePosition(node.Key);
                    for (int i = 0; i < 3; i++)
                        diffBetweenRefs[i] = diffBetweenRefs[i] - robotRefNodePos[i];

                    if (IsRobotInNoGoZone(diffBetweenRefs, relLevel, boundingBoxHalfExtents) && noGo)
                        continue;

                    if (IsRobotInSymmetryZone(diffBetweenRefs, relLevel, boundingBoxHalfExtents) && symmetry)
                        continue;
                }


                if (level < carTree.Level)
                {
                    BruteForce(carTree.GetChildNodesWithIndex(node.Key), level + 1, maxCycles, rotations, noGo, symmetry, boundingBoxHalfExtents);
                }
                else
                {
                    for (int m = 0; m < rotations; m++)
                    {
                        var value = 0.0;

                        int? robZIndex = 0;
                        int? carZIndex = node.Key - carTree.StartIndexLeafNodes;
                        for (int i = 0; i < Math.Pow(2, rotatedRobotTrees[m].Level); i++) // +Z
                        {
                            int? robYIndex = robZIndex;
                            int? carYIndex = carZIndex;
                            for (int j = 0; j < Math.Pow(2, rotatedRobotTrees[m].Level); j++) // +Y
                            {
                                int? robXIndex = robYIndex;
                                int? carXIndex = carYIndex;
                                for (int k = 0; k < Math.Pow(2, rotatedRobotTrees[m].Level); k++) // -X
                                {
                                    if (carTree.Nodes[carTree.StartIndexPerLevel[carTree.Level] + (int)carXIndex] != null && rotatedRobotTrees[m].Nodes[rotatedRobotTrees[m].StartIndexPerLevel[rotatedRobotTrees[m].Level] + (int)robXIndex] != null)
                                        value += carTree.Nodes[carTree.StartIndexPerLevel[carTree.Level] + (int)carXIndex].Value * rotatedRobotTrees[m].Nodes[rotatedRobotTrees[m].StartIndexPerLevel[rotatedRobotTrees[m].Level] + (int)robXIndex].Value;

                                    var robXIndexTemp = MatrixHelper.moveOneInMXFromRefOnLevel(rotatedRobotTrees[m].Level, (int)robXIndex);
                                    var carXIndexTemp = MatrixHelper.moveOneInMXFromRefOnLevel(carTree.Level, (int)carXIndex);

                                    if (robXIndexTemp == null || carXIndexTemp == null)
                                        break;
                                    else
                                    {
                                        robXIndex += (int)robXIndexTemp;
                                        carXIndex += (int)carXIndexTemp;
                                    }
                                }

                                var robYIndexTemp = MatrixHelper.moveOneInPYFromRefOnLevel(rotatedRobotTrees[m].Level, (int)robYIndex);
                                var carYIndexTemp = MatrixHelper.moveOneInPYFromRefOnLevel(carTree.Level, (int)carYIndex);

                                if (robYIndexTemp == null || carYIndexTemp == null)
                                    break;
                                else
                                {
                                    robYIndex += (int)robYIndexTemp;
                                    carYIndex += (int)carYIndexTemp;
                                }
                            }

                            var robZIndexTemp = MatrixHelper.moveOneInPZFromRefOnLevel(rotatedRobotTrees[m].Level, (int)robZIndex);
                            var carZIndexTemp = MatrixHelper.moveOneInPZFromRefOnLevel(carTree.Level, (int)carZIndex);

                            if (robZIndexTemp == null || carZIndexTemp == null)
                                break;
                            else
                            {
                                robZIndex += (int)robZIndexTemp;
                                carZIndex += (int)carZIndexTemp;
                            }
                        }

                        if (value > lowerBound)
                        {
                            var diffBetweenRefs = carTree.CalculateNodePosition(node.Key);
                            for (int i = 0; i < 3; i++)
                                diffBetweenRefs[i] = diffBetweenRefs[i] - robotRefNodePos[i];

                            lowerBound = value;

                            translateOperator = diffBetweenRefs;
                            rotateOperator = m;
                        }
                    }
                }
            }
        }

        public static double MaxVoxelInit(IOrderedEnumerable<KeyValuePair<int, VoxelNode>> sortedNodes, int rotations, bool noGo, bool symmetry, double[] boundingBoxHalfExtents)
        {
            var lbInit = 0.0;
            var robotRefNodePos = robotTree.CalculateNodePosition(robotTree.Root);
            var largestNode = sortedNodes.First().Value;
            var diffBetweenRefs = new int[3];

            foreach (var node in sortedNodes)
            {
                diffBetweenRefs = carTree.CalculateNodePosition(node.Value);
                for (int i = 0; i < 3; i++)
                    diffBetweenRefs[i] = diffBetweenRefs[i] - robotRefNodePos[i];

                if (IsRobotInNoGoZone(diffBetweenRefs, robotTree.Level, boundingBoxHalfExtents) && noGo)
                    continue;

                if (IsRobotInSymmetryZone(diffBetweenRefs, robotTree.Level, boundingBoxHalfExtents) && symmetry)
                    continue;

                largestNode = (VoxelNodeInner)node.Value;
                break;
            }

            var directChilds = carTree.GetChildNodes((VoxelNodeInner)largestNode).ToArray();        

            // Find the best rotation
            // COMMENT FOR PAPER: Finding the rotation first is weak, in case of high values in neighbours but low values in the center, as rotation is than based on missing data
            var maxPossibleMatch = 0.0;
            var maxMatchPossibleRotation = 0;
            for (var i = 0; i < rotations; i++)
            {
                var maxPossibleMatchTemp = 0.0;
                for (var j = 0; j < 8; j++)
                {
                    if (rotatedRobotTrees[i].Nodes[j + 1] == null || directChilds[j] == null)
                        continue;

                    maxPossibleMatchTemp += (((VoxelNodeInner)rotatedRobotTrees[i].Nodes[j + 1]).Max * ((VoxelNodeInner)directChilds[j]).Max);
                }
                if (maxPossibleMatchTemp > maxPossibleMatch)
                {
                    maxPossibleMatch = maxPossibleMatchTemp;
                    maxMatchPossibleRotation = i;
                }
            }
            upperBound = maxPossibleMatch;
            
            for (var j = 0; j < 8; j++)
            {
                if (rotatedRobotTrees[maxMatchPossibleRotation].Nodes[j + 1] == null || directChilds[j] == null)
                    continue;

                var robotLeafs = rotatedRobotTrees[maxMatchPossibleRotation].GetLeafNodes((VoxelNodeInner)rotatedRobotTrees[maxMatchPossibleRotation].Nodes[j + 1]);
                var carLeafs = carTree.GetLeafNodes((VoxelNodeInner)directChilds[j]);

                if (robotLeafs.Count() != carLeafs.Count())
                    return double.NaN;

                for (var i = 0; i < robotLeafs.Count(); i++)
                {
                    if (robotLeafs.ElementAt(i) == null || carLeafs.ElementAt(i) == null)
                        continue;
                    lbInit += robotLeafs.ElementAt(i).Value * carLeafs.ElementAt(i).Value;
                }
            }

            rotateOperator = maxMatchPossibleRotation;
            translateOperator = diffBetweenRefs;

            return lbInit;
        }

        public static double MaxLeafInit(int rotations, double[] boundingBoxHalfExtents, bool noGo, bool symmetry)
        {
            var lbInit = 0.0;

            var carLeafs = carTree.GetLeafNodesWithIndex();
            var largestCarLeaf = carLeafs.OrderByDescending(n => n.Value != null ? n.Value.Value : 0.0).First();

            for (int j = 0; j < rotations; j++)
            {
                var robotLeafs = rotatedRobotTrees[j].GetLeafNodesWithIndex();
                var orderedRobotLeafs = robotLeafs.OrderByDescending(n => n.Value != null ? n.Value.Value : 0.0);
               
                var k = 0;
                var diffBetweenRefs = new int[3];
                foreach(var robotLeaf in orderedRobotLeafs)
                {
                    diffBetweenRefs = carTree.CalculateNodePosition(largestCarLeaf.Key);
                    var robotRefNodePos = rotatedRobotTrees[j].CalculateNodePosition(robotLeaf.Key);
                    for (var i = 0; i < 3; i++)
                        diffBetweenRefs[i] = diffBetweenRefs[i] - robotRefNodePos[i];


                    if (symmetry)
                    {
                        if (noGo)
                        {
                            if (!IsRobotInNoGoZone(diffBetweenRefs, robotTree.Level, boundingBoxHalfExtents) && !IsRobotInSymmetryZone(diffBetweenRefs, robotTree.Level, boundingBoxHalfExtents))
                                break;
                        }
                        else
                        {
                            if (!IsRobotInSymmetryZone(diffBetweenRefs, robotTree.Level, boundingBoxHalfExtents))
                                break;
                        }
                    }
                    else
                    {
                        if (noGo)
                        {
                            if (!IsRobotInNoGoZone(diffBetweenRefs, robotTree.Level, boundingBoxHalfExtents))
                                break;
                        }
                        else
                            break;
                    }
                    
                    k++;
                }

                if (k >= orderedRobotLeafs.Count())
                    continue;


                var largestRobotLeaf = orderedRobotLeafs.ElementAt(k);

                var lbInitTemp = 0.0;
                foreach (var robotLeaf in robotLeafs)
                {
                    if (robotLeaf.Value == null)
                        continue;

                    var robotLeafPos = rotatedRobotTrees[j].CalculateNodePosition(robotLeaf.Key);
                    for (int i = 0; i < 3; i++)
                        robotLeafPos[i] = robotLeafPos[i] + diffBetweenRefs[i];

                    var carTreeLeafValue = carTree.Get(robotLeafPos[0], robotLeafPos[1], robotLeafPos[2]);
                    if (double.IsNaN(carTreeLeafValue))
                        continue;

                    lbInitTemp += carTreeLeafValue * robotLeaf.Value.Value;
                }
                if (lbInitTemp > lbInit)
                {
                    lbInit = lbInitTemp;
                    rotateOperator = j;
                    translateOperator = diffBetweenRefs;
                }
            }         

            return lbInit;
        }

        public static bool IsRobotInNoGoZone(int[] distance, int relLevel, double[] boundingBoxHalfExtents)
        {
            // Calc current Pos of reference Voxels
            var robotZeroPos = robotTree.CalculateNodePosition(robotTree.Root);

            var zeroPosOfRobotCenter = new int[3];
            var sevenPosOfRobotCenter = new int[3];
            for (int i = 0; i < 3; i++)
                zeroPosOfRobotCenter[i] = robotZeroPos[i] + distance[i];

            // Calc possible Pos of Center Voxels
            var stepSize = (int)((Math.Pow(2, robotTree.Level - relLevel) - 1) * robotTree.Precision);

            sevenPosOfRobotCenter[0] = zeroPosOfRobotCenter[0] - stepSize;
            sevenPosOfRobotCenter[1] = zeroPosOfRobotCenter[1] + stepSize;
            sevenPosOfRobotCenter[2] = zeroPosOfRobotCenter[2] + stepSize;

            // Below Grid?
            if (zeroPosOfRobotCenter[2] < 0 && sevenPosOfRobotCenter[2] < 0)   // Z-Value of robot root in Worst-Case below 0?
                return true;

            // Above or in Bounding Box of Carbody?
            if (zeroPosOfRobotCenter[0] <= boundingBoxHalfExtents[0] && sevenPosOfRobotCenter[0] <= boundingBoxHalfExtents[0] && zeroPosOfRobotCenter[0] >= -boundingBoxHalfExtents[0] && sevenPosOfRobotCenter[0] >= -boundingBoxHalfExtents[0])
                if (zeroPosOfRobotCenter[1] >= -boundingBoxHalfExtents[1] && sevenPosOfRobotCenter[1] >= -boundingBoxHalfExtents[1] && zeroPosOfRobotCenter[1] <= boundingBoxHalfExtents[1] && sevenPosOfRobotCenter[1] <= boundingBoxHalfExtents[1])
                    return true;

            // In Front, in or behind Bounding Box of Carbody?
            if (zeroPosOfRobotCenter[0] <= boundingBoxHalfExtents[0] && sevenPosOfRobotCenter[0] <= boundingBoxHalfExtents[0] && zeroPosOfRobotCenter[0] >= -boundingBoxHalfExtents[0] && sevenPosOfRobotCenter[0] >= -boundingBoxHalfExtents[0])
                if (zeroPosOfRobotCenter[2] <= (boundingBoxHalfExtents[2] * 2) && sevenPosOfRobotCenter[2] <= (boundingBoxHalfExtents[2] * 2) && zeroPosOfRobotCenter[2] >= 0 && sevenPosOfRobotCenter[2] >= 0)
                    return true;

            return false;
        }


        public static bool IsRobotInSymmetryZone(int[] distance, int relLevel, double[] boundingBoxHalfExtents)
        {
            // Calc current Pos of reference Voxels
            var robotZeroPos = robotTree.CalculateNodePosition(robotTree.Root);

            var zeroPosOfRobotCenter = new int[3];
            var sevenPosOfRobotCenter = new int[3];
            for (int i = 0; i < 3; i++)
                zeroPosOfRobotCenter[i] = robotZeroPos[i] + distance[i];

            // Calc possible Pos of Center Voxels
            var stepSize = (int)((Math.Pow(2, robotTree.Level - relLevel) - 1) * robotTree.Precision);

            sevenPosOfRobotCenter[0] = zeroPosOfRobotCenter[0] - stepSize;
            sevenPosOfRobotCenter[1] = zeroPosOfRobotCenter[1] + stepSize;
            sevenPosOfRobotCenter[2] = zeroPosOfRobotCenter[2] + stepSize;

            // X-Value smaller than right side of bounding box?
            if (zeroPosOfRobotCenter[0] < -boundingBoxHalfExtents[0] && sevenPosOfRobotCenter[0] < -boundingBoxHalfExtents[0])   
                return true;

            return false;
        }

        public static int[] BranchAndBound(VoxelOctree cTree, VoxelOctree rTree, bool maxVoxel, bool maxLeaf, bool maxValue, bool maxLeafs, bool maxMax, int searchMethod, int shiftMethod, int maxCycles, bool rotation, bool noGo, bool symmetry, double[] boundingBoxHalfExtents)
        {
            cycles = 0;
            translateOperator = new int[3];
            rotateOperator = 0;
            carTree = cTree;
            robotTree = rTree;
            startLevel = carTree.Level - robotTree.Level;
            endLevel = carTree.Level - startLevel;
            rotatedRobotTrees = MatrixHelper.allRotationsOfCube(robotTree);
            var nodesOnStartLevel = carTree.GetAncestorNodesWithIndex(startLevel);
            var orderedNodesOnStartLevel = nodesOnStartLevel.OrderByDescending(n => n.Value != null ? n.Value.Value : 0.0);


            // Number of robot rotations zu check
            var rotationsToCheck = rotation ? rotatedRobotTrees.Count() : 1;

            // Find initial lower Bound
            lowerBound = 0.0;
            var lbTemp = 0.0;

            if(maxVoxel)
                lowerBound = (lbTemp = MaxVoxelInit(orderedNodesOnStartLevel, rotationsToCheck, noGo, symmetry, boundingBoxHalfExtents)) > lowerBound ? lbTemp : lowerBound;
            if(maxLeaf)
                lowerBound = (lbTemp = MaxLeafInit(rotationsToCheck, boundingBoxHalfExtents, noGo, symmetry)) > lowerBound ? lbTemp : lowerBound;

            initLowerBound = lowerBound;

            // Select Search Strategy
            switch (searchMethod)
            {
                case 0:
                    DepthFirstSearch(nodesOnStartLevel, startLevel, maxCycles, maxValue, maxLeafs, maxMax, rotationsToCheck, noGo, symmetry, boundingBoxHalfExtents);
                    break;
                case 1:
                    BestVoxelFirstSearch(orderedNodesOnStartLevel, startLevel, maxCycles, maxValue, maxLeafs, maxMax, rotationsToCheck, noGo, symmetry, boundingBoxHalfExtents);                    
                    break;
                case 2:
                    BruteForce(nodesOnStartLevel, startLevel, maxCycles, rotationsToCheck, noGo, symmetry, boundingBoxHalfExtents);
                    break;
            }
            
            return translateOperator;
        }

         #endregion
    }
}