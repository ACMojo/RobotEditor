using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using RobotEditor.Model;

namespace RobotEditor.Helper
{
    public static class MatchAlgorithms
    {
        #region Fields

        public static int Cycles;

        #endregion

        #region Properties

        public static double LowerBound { get; set; }
        public static double InitLowerBound { get; set; }
        public static double UpperBound { get; set; }
        public static int StartLevel { get; set; }
        public static int EndLevel { get; set; }
        public static VoxelOctree CarTree { get; set; }
        public static VoxelOctree RobotTree { get; set; }
        public static VoxelOctree[] RotatedRobotTrees { get; set; }
        public static int[] TranslateOperator { get; set; }
        public static int RotateOperator { get; set; }

        #endregion

        #region Public methods

        public static void BestVoxelFirstSearch(
            IList<KeyValuePair<int, VoxelNode>> nodes,
            int level,
            int maxCycles,
            bool maxValue,
            bool maxLeafs,
            bool maxMax,
            int rotations,
            bool noGo,
            bool symmetry,
            double[] boundingBoxHalfExtents)
        {
            if (Cycles >= maxCycles && maxCycles > 0)
                return;

            Cycles++;
            var relLevel = level - (CarTree.Level - RobotTree.Level);

            IEnumerable<VoxelNodeInner> robotMaxNodes = null;
            if (level < CarTree.Level)
                robotMaxNodes = RobotTree.GetAncestorNodes(relLevel).Cast<VoxelNodeInner>().Where(n => n != null).OrderByDescending(n => n.Max);

            var robotRefNodePos = RobotTree.CalculateNodePosition(RobotTree.StartIndexPerLevel[RobotTree.Level]);

            Parallel.ForEach(
                nodes,
                node =>
                {
                    if (noGo || symmetry)
                    {
                        var diffBetweenRefs = CarTree.CalculateNodePosition(node.Key);
                        for (int i = 0; i < 3; i++)
                            diffBetweenRefs[i] = diffBetweenRefs[i] - robotRefNodePos[i];

                        if (IsRobotInNoGoZone(diffBetweenRefs, relLevel, boundingBoxHalfExtents) && noGo)
                            return;

                        if (IsRobotInSymmetryZone(diffBetweenRefs, relLevel, boundingBoxHalfExtents) && symmetry)
                            return;
                    }

                    if (level < CarTree.Level)
                    {
                        var maxValueValue = 0.0;
                        var maxMaxValue = 0.0;

                        List<double> maxValueList = new List<double>();
                        List<double> maxMaxList = new List<double>();
                        List<VoxelNodeLeaf> maxLeafsList = new List<VoxelNodeLeaf>();

                        int? carZIndex = node.Key - CarTree.StartIndexPerLevel[level];
                        for (int i = 0; i <= Math.Pow(2, relLevel); i++) // +Z
                        {
                            int? carYIndex = carZIndex;
                            for (int j = 0; j <= Math.Pow(2, relLevel); j++) // +Y
                            {
                                int? carXIndex = carYIndex;
                                for (int k = 0; k <= Math.Pow(2, relLevel); k++) // -X
                                {
                                    if (CarTree.Nodes[CarTree.StartIndexPerLevel[level] + (int)carXIndex] != null)
                                    {
                                        maxValueList.Add(CarTree.Nodes[CarTree.StartIndexPerLevel[level] + (int)carXIndex].Value);
                                        maxMaxList.Add(((VoxelNodeInner)CarTree.Nodes[CarTree.StartIndexPerLevel[level] + (int)carXIndex]).Max);
                                        maxLeafsList.AddRange(
                                            CarTree.GetLeafNodes((VoxelNodeInner)CarTree.Nodes[CarTree.StartIndexPerLevel[level] + (int)carXIndex]).ToList());
                                    }

                                    var carXIndexTemp = MatrixHelper.MoveOneInMxFromRefOnLevel(level, (int)carXIndex);

                                    if (carXIndexTemp == null)
                                        break;
                                    else
                                        carXIndex += (int)carXIndexTemp;
                                }

                                var carYIndexTemp = MatrixHelper.MoveOneInPyFromRefOnLevel(level, (int)carYIndex);

                                if (carYIndexTemp == null)
                                    break;
                                else
                                    carYIndex += (int)carYIndexTemp;
                            }

                            var carZIndexTemp = MatrixHelper.MoveOneInPzFromRefOnLevel(level, (int)carZIndex);

                            if (carZIndexTemp == null)
                                break;
                            else
                                carZIndex += (int)carZIndexTemp;
                        }

                        // Lower bound check maxValue and maxMax
                        var m = 0;
                        var maxValueListTemp = maxValueList.OrderByDescending(n => n).ToList();
                        foreach (var robotMaxNode in robotMaxNodes)
                        {
                            if (m >= maxValueListTemp.Count)
                                break;

                            if (maxValue)
                                maxValueValue += robotMaxNode.Max * maxValueListTemp.ElementAt(m);

                            if (maxMax)
                            {
                                var robCount = RobotTree.GetLeafNodes(robotMaxNode).Count(n => n != null);
                                maxMaxValue += robCount * robotMaxNode.Max * maxMaxList.ElementAt(m);
                            }

                            m++;
                        }

                        if (maxValueValue <= LowerBound && maxValue)
                            return;

                        if (maxMaxValue <= LowerBound && maxMax)
                            return;

                        // Lower bound check maxLeafs
                        if (maxLeafs)
                        {
                            var robLeafs = RobotTree.GetLeafNodes(RobotTree.Root).Where(n => n != null).OrderByDescending(n => n.Value);
                            var maxLeafsListTemp = maxLeafsList.Where(n => n != null).OrderByDescending(n => n.Value).ToList();
                            var t = 0;
                            var maxLeafsValue = 0.0;
                            foreach (var robLeaf in robLeafs)
                            {
                                if (t >= maxLeafsListTemp.Count)
                                    break;

                                maxLeafsValue += robLeaf.Value * maxLeafsListTemp.ElementAt(t).Value;
                                t++;
                            }

                            if (maxLeafsValue <= LowerBound)
                                return;
                        }

                        BestVoxelFirstSearch(
                            CarTree.GetChildNodesWithIndex(node.Key).OrderByDescending(n => n.Value?.Value ?? 0.0).ToList(),
                            level + 1,
                            maxCycles,
                            maxValue,
                            maxLeafs,
                            maxMax,
                            rotations,
                            noGo,
                            symmetry,
                            boundingBoxHalfExtents);
                    }
                    else
                    {
                        for (int m = 0; m < rotations; m++)
                        {
                            var value = 0.0;

                            int? robZIndex = 0;
                            int? carZIndex = node.Key - CarTree.StartIndexLeafNodes;
                            for (int i = 0; i < Math.Pow(2, RotatedRobotTrees[m].Level); i++) // +Z
                            {
                                int? robYIndex = robZIndex;
                                int? carYIndex = carZIndex;
                                for (int j = 0; j < Math.Pow(2, RotatedRobotTrees[m].Level); j++) // +Y
                                {
                                    int? robXIndex = robYIndex;
                                    int? carXIndex = carYIndex;
                                    for (int k = 0; k < Math.Pow(2, RotatedRobotTrees[m].Level); k++) // -X
                                    {
                                        if (CarTree.Nodes[CarTree.StartIndexPerLevel[CarTree.Level] + (int)carXIndex] != null && RotatedRobotTrees[m]
                                                .Nodes[RotatedRobotTrees[m].StartIndexPerLevel[RotatedRobotTrees[m].Level] + (int)robXIndex] != null)
                                            value += CarTree.Nodes[CarTree.StartIndexPerLevel[CarTree.Level] + (int)carXIndex].Value * RotatedRobotTrees[m]
                                                         .Nodes[RotatedRobotTrees[m].StartIndexPerLevel[RotatedRobotTrees[m].Level] + (int)robXIndex].Value;

                                        var robXIndexTemp = MatrixHelper.MoveOneInMxFromRefOnLevel(RotatedRobotTrees[m].Level, (int)robXIndex);
                                        var carXIndexTemp = MatrixHelper.MoveOneInMxFromRefOnLevel(CarTree.Level, (int)carXIndex);

                                        if (robXIndexTemp == null || carXIndexTemp == null)
                                            break;
                                        else
                                        {
                                            robXIndex += (int)robXIndexTemp;
                                            carXIndex += (int)carXIndexTemp;
                                        }
                                    }

                                    var robYIndexTemp = MatrixHelper.MoveOneInPyFromRefOnLevel(RotatedRobotTrees[m].Level, (int)robYIndex);
                                    var carYIndexTemp = MatrixHelper.MoveOneInPyFromRefOnLevel(CarTree.Level, (int)carYIndex);

                                    if (robYIndexTemp == null || carYIndexTemp == null)
                                        break;
                                    else
                                    {
                                        robYIndex += (int)robYIndexTemp;
                                        carYIndex += (int)carYIndexTemp;
                                    }
                                }

                                var robZIndexTemp = MatrixHelper.MoveOneInPzFromRefOnLevel(RotatedRobotTrees[m].Level, (int)robZIndex);
                                var carZIndexTemp = MatrixHelper.MoveOneInPzFromRefOnLevel(CarTree.Level, (int)carZIndex);

                                if (robZIndexTemp == null || carZIndexTemp == null)
                                    break;
                                else
                                {
                                    robZIndex += (int)robZIndexTemp;
                                    carZIndex += (int)carZIndexTemp;
                                }
                            }

                            if (value > LowerBound)
                            {
                                var diffBetweenRefs = CarTree.CalculateNodePosition(node.Key);
                                for (int i = 0; i < 3; i++)
                                    diffBetweenRefs[i] = diffBetweenRefs[i] - robotRefNodePos[i];

                                LowerBound = value;
                                TranslateOperator = diffBetweenRefs;
                                RotateOperator = m;
                            }
                        }
                    }
                });
        }

        public static void DepthFirstSearch(
            IEnumerable<KeyValuePair<int, VoxelNode>> nodes,
            int level,
            int maxCycles,
            bool maxValue,
            bool maxLeafs,
            bool maxMax,
            int rotations,
            bool noGo,
            bool symmetry,
            double[] boundingBoxHalfExtents)
        {
            if (Cycles >= maxCycles && maxCycles > 0)
                return;

            Cycles++;
            var relLevel = level - (CarTree.Level - RobotTree.Level);

            IEnumerable<VoxelNodeInner> robotMaxNodes = null;
            if (level < CarTree.Level)
                robotMaxNodes = RobotTree.GetAncestorNodes(relLevel).Cast<VoxelNodeInner>().Where(n => n != null).OrderByDescending(n => n.Max);

            var robotRefNodePos = RobotTree.CalculateNodePosition(RobotTree.StartIndexPerLevel[RobotTree.Level]);

            Parallel.ForEach(
                nodes,
                node =>
                {
                    if (noGo || symmetry)
                    {
                        var diffBetweenRefs = CarTree.CalculateNodePosition(node.Key);
                        for (int i = 0; i < 3; i++)
                            diffBetweenRefs[i] = diffBetweenRefs[i] - robotRefNodePos[i];

                        if (IsRobotInNoGoZone(diffBetweenRefs, relLevel, boundingBoxHalfExtents) && noGo)
                            return;

                        if (IsRobotInSymmetryZone(diffBetweenRefs, relLevel, boundingBoxHalfExtents) && symmetry)
                            return;
                    }

                    if (level < CarTree.Level)
                    {
                        var maxValueValue = 0.0;
                        var maxMaxValue = 0.0;

                        List<double> maxValueList = new List<double>();
                        List<double> maxMaxList = new List<double>();
                        List<VoxelNodeLeaf> maxLeafsList = new List<VoxelNodeLeaf>();

                        int? carZIndex = node.Key - CarTree.StartIndexPerLevel[level];
                        for (int i = 0; i <= Math.Pow(2, relLevel); i++) // +Z
                        {
                            int? carYIndex = carZIndex;
                            for (int j = 0; j <= Math.Pow(2, relLevel); j++) // +Y
                            {
                                int? carXIndex = carYIndex;
                                for (int k = 0; k <= Math.Pow(2, relLevel); k++) // -X
                                {
                                    if (CarTree.Nodes[CarTree.StartIndexPerLevel[level] + (int)carXIndex] != null)
                                    {
                                        maxValueList.Add(CarTree.Nodes[CarTree.StartIndexPerLevel[level] + (int)carXIndex].Value);
                                        maxMaxList.Add(((VoxelNodeInner)CarTree.Nodes[CarTree.StartIndexPerLevel[level] + (int)carXIndex]).Max);
                                        maxLeafsList.AddRange(
                                            CarTree.GetLeafNodes((VoxelNodeInner)CarTree.Nodes[CarTree.StartIndexPerLevel[level] + (int)carXIndex]).ToList());
                                    }

                                    var carXIndexTemp = MatrixHelper.MoveOneInMxFromRefOnLevel(level, (int)carXIndex);

                                    if (carXIndexTemp == null)
                                        break;
                                    else
                                        carXIndex += (int)carXIndexTemp;
                                }

                                var carYIndexTemp = MatrixHelper.MoveOneInPyFromRefOnLevel(level, (int)carYIndex);

                                if (carYIndexTemp == null)
                                    break;
                                else
                                    carYIndex += (int)carYIndexTemp;
                            }

                            var carZIndexTemp = MatrixHelper.MoveOneInPzFromRefOnLevel(level, (int)carZIndex);

                            if (carZIndexTemp == null)
                                break;
                            else
                                carZIndex += (int)carZIndexTemp;
                        }

                        // Lower bound check maxValue and maxMax
                        var m = 0;
                        var maxValueListTemp = maxValueList.OrderByDescending(n => n).ToList();
                        foreach (var robotMaxNode in robotMaxNodes)
                        {
                            if (m >= maxValueListTemp.Count)
                                break;

                            if (maxValue)
                                maxValueValue += robotMaxNode.Max * maxValueListTemp.ElementAt(m);

                            if (maxMax)
                            {
                                var robCount = RobotTree.GetLeafNodes(robotMaxNode).Count(n => n != null);
                                maxMaxValue += robCount * robotMaxNode.Max * maxMaxList.ElementAt(m);
                            }

                            m++;
                        }

                        if (maxValueValue <= LowerBound && maxValue)
                            return;

                        if (maxMaxValue <= LowerBound && maxMax)
                            return;

                        // Lower bound check maxLeafs
                        if (maxLeafs)
                        {
                            var robLeafs = RobotTree.GetLeafNodes(RobotTree.Root).Where(n => n != null).OrderByDescending(n => n.Value);
                            var maxLeafsListTemp = maxLeafsList.Where(n => n != null).OrderByDescending(n => n.Value).ToList();
                            var t = 0;
                            var maxLeafsValue = 0.0;
                            foreach (var robLeaf in robLeafs)
                            {
                                if (t >= maxLeafsListTemp.Count)
                                    break;

                                maxLeafsValue += robLeaf.Value * maxLeafsListTemp.ElementAt(t).Value;
                                t++;
                            }

                            if (maxLeafsValue <= LowerBound)
                                return;
                        }

                        DepthFirstSearch(
                            CarTree.GetChildNodesWithIndex(node.Key),
                            level + 1,
                            maxCycles,
                            maxValue,
                            maxLeafs,
                            maxMax,
                            rotations,
                            noGo,
                            symmetry,
                            boundingBoxHalfExtents);
                    }
                    else
                    {
                        for (int m = 0; m < rotations; m++)
                        {
                            var value = 0.0;

                            int? robZIndex = 0;
                            int? carZIndex = node.Key - CarTree.StartIndexLeafNodes;
                            for (int i = 0; i < Math.Pow(2, RotatedRobotTrees[m].Level); i++) // +Z
                            {
                                int? robYIndex = robZIndex;
                                int? carYIndex = carZIndex;
                                for (int j = 0; j < Math.Pow(2, RotatedRobotTrees[m].Level); j++) // +Y
                                {
                                    int? robXIndex = robYIndex;
                                    int? carXIndex = carYIndex;
                                    for (int k = 0; k < Math.Pow(2, RotatedRobotTrees[m].Level); k++) // -X
                                    {
                                        if (CarTree.Nodes[CarTree.StartIndexPerLevel[CarTree.Level] + (int)carXIndex] != null && RotatedRobotTrees[m]
                                                .Nodes[RotatedRobotTrees[m].StartIndexPerLevel[RotatedRobotTrees[m].Level] + (int)robXIndex] != null)
                                            value += CarTree.Nodes[CarTree.StartIndexPerLevel[CarTree.Level] + (int)carXIndex].Value * RotatedRobotTrees[m]
                                                         .Nodes[RotatedRobotTrees[m].StartIndexPerLevel[RotatedRobotTrees[m].Level] + (int)robXIndex].Value;

                                        var robXIndexTemp = MatrixHelper.MoveOneInMxFromRefOnLevel(RotatedRobotTrees[m].Level, (int)robXIndex);
                                        var carXIndexTemp = MatrixHelper.MoveOneInMxFromRefOnLevel(CarTree.Level, (int)carXIndex);

                                        if (robXIndexTemp == null || carXIndexTemp == null)
                                            break;
                                        else
                                        {
                                            robXIndex += (int)robXIndexTemp;
                                            carXIndex += (int)carXIndexTemp;
                                        }
                                    }

                                    var robYIndexTemp = MatrixHelper.MoveOneInPyFromRefOnLevel(RotatedRobotTrees[m].Level, (int)robYIndex);
                                    var carYIndexTemp = MatrixHelper.MoveOneInPyFromRefOnLevel(CarTree.Level, (int)carYIndex);

                                    if (robYIndexTemp == null || carYIndexTemp == null)
                                        break;
                                    else
                                    {
                                        robYIndex += (int)robYIndexTemp;
                                        carYIndex += (int)carYIndexTemp;
                                    }
                                }

                                var robZIndexTemp = MatrixHelper.MoveOneInPzFromRefOnLevel(RotatedRobotTrees[m].Level, (int)robZIndex);
                                var carZIndexTemp = MatrixHelper.MoveOneInPzFromRefOnLevel(CarTree.Level, (int)carZIndex);

                                if (robZIndexTemp == null || carZIndexTemp == null)
                                    break;
                                else
                                {
                                    robZIndex += (int)robZIndexTemp;
                                    carZIndex += (int)carZIndexTemp;
                                }
                            }

                            if (value > LowerBound)
                            {
                                var diffBetweenRefs = CarTree.CalculateNodePosition(node.Key);
                                for (int i = 0; i < 3; i++)
                                    diffBetweenRefs[i] = diffBetweenRefs[i] - robotRefNodePos[i];

                                LowerBound = value;
                                TranslateOperator = diffBetweenRefs;
                                RotateOperator = m;
                            }
                        }
                    }
                });
        }

        public static void BruteForce(
            IEnumerable<KeyValuePair<int, VoxelNode>> nodes,
            int level,
            int maxCycles,
            int rotations,
            bool noGo,
            bool symmetry,
            double[] boundingBoxHalfExtents)
        {
            if (Cycles >= maxCycles && maxCycles > 0)
                return;

            Cycles++;
            var relLevel = level - (CarTree.Level - RobotTree.Level);

            var robotRefNodePos = RobotTree.CalculateNodePosition(RobotTree.StartIndexPerLevel[RobotTree.Level]);

            foreach (var node in nodes)
            {
                if (noGo || symmetry)
                {
                    var diffBetweenRefs = CarTree.CalculateNodePosition(node.Key);
                    for (int i = 0; i < 3; i++)
                        diffBetweenRefs[i] = diffBetweenRefs[i] - robotRefNodePos[i];

                    if (IsRobotInNoGoZone(diffBetweenRefs, relLevel, boundingBoxHalfExtents) && noGo)
                        continue;

                    if (IsRobotInSymmetryZone(diffBetweenRefs, relLevel, boundingBoxHalfExtents) && symmetry)
                        continue;
                }

                if (level < CarTree.Level)
                    BruteForce(CarTree.GetChildNodesWithIndex(node.Key), level + 1, maxCycles, rotations, noGo, symmetry, boundingBoxHalfExtents);
                else
                {
                    for (int m = 0; m < rotations; m++)
                    {
                        var value = 0.0;

                        int? robZIndex = 0;
                        int? carZIndex = node.Key - CarTree.StartIndexLeafNodes;
                        for (int i = 0; i < Math.Pow(2, RotatedRobotTrees[m].Level); i++) // +Z
                        {
                            int? robYIndex = robZIndex;
                            int? carYIndex = carZIndex;
                            for (int j = 0; j < Math.Pow(2, RotatedRobotTrees[m].Level); j++) // +Y
                            {
                                int? robXIndex = robYIndex;
                                int? carXIndex = carYIndex;
                                for (int k = 0; k < Math.Pow(2, RotatedRobotTrees[m].Level); k++) // -X
                                {
                                    if (CarTree.Nodes[CarTree.StartIndexPerLevel[CarTree.Level] + (int)carXIndex] != null && RotatedRobotTrees[m]
                                            .Nodes[RotatedRobotTrees[m].StartIndexPerLevel[RotatedRobotTrees[m].Level] + (int)robXIndex] != null)
                                        value += CarTree.Nodes[CarTree.StartIndexPerLevel[CarTree.Level] + (int)carXIndex].Value * RotatedRobotTrees[m]
                                                     .Nodes[RotatedRobotTrees[m].StartIndexPerLevel[RotatedRobotTrees[m].Level] + (int)robXIndex].Value;

                                    var robXIndexTemp = MatrixHelper.MoveOneInMxFromRefOnLevel(RotatedRobotTrees[m].Level, (int)robXIndex);
                                    var carXIndexTemp = MatrixHelper.MoveOneInMxFromRefOnLevel(CarTree.Level, (int)carXIndex);

                                    if (robXIndexTemp == null || carXIndexTemp == null)
                                        break;
                                    else
                                    {
                                        robXIndex += (int)robXIndexTemp;
                                        carXIndex += (int)carXIndexTemp;
                                    }
                                }

                                var robYIndexTemp = MatrixHelper.MoveOneInPyFromRefOnLevel(RotatedRobotTrees[m].Level, (int)robYIndex);
                                var carYIndexTemp = MatrixHelper.MoveOneInPyFromRefOnLevel(CarTree.Level, (int)carYIndex);

                                if (robYIndexTemp == null || carYIndexTemp == null)
                                    break;
                                else
                                {
                                    robYIndex += (int)robYIndexTemp;
                                    carYIndex += (int)carYIndexTemp;
                                }
                            }

                            var robZIndexTemp = MatrixHelper.MoveOneInPzFromRefOnLevel(RotatedRobotTrees[m].Level, (int)robZIndex);
                            var carZIndexTemp = MatrixHelper.MoveOneInPzFromRefOnLevel(CarTree.Level, (int)carZIndex);

                            if (robZIndexTemp == null || carZIndexTemp == null)
                                break;
                            else
                            {
                                robZIndex += (int)robZIndexTemp;
                                carZIndex += (int)carZIndexTemp;
                            }
                        }

                        if (value > LowerBound)
                        {
                            var diffBetweenRefs = CarTree.CalculateNodePosition(node.Key);
                            for (int i = 0; i < 3; i++)
                                diffBetweenRefs[i] = diffBetweenRefs[i] - robotRefNodePos[i];

                            LowerBound = value;

                            TranslateOperator = diffBetweenRefs;
                            RotateOperator = m;
                        }
                    }
                }
            }
        }

        public static double MaxVoxelInit(
            IList<KeyValuePair<int, VoxelNode>> sortedNodes,
            int rotations,
            bool noGo,
            bool symmetry,
            double[] boundingBoxHalfExtents)
        {
            var lbInit = 0.0;
            var robotRefNodePos = RobotTree.CalculateNodePosition(RobotTree.Root);
            var diffBetweenRefs = new int[3];

            VoxelNode largestNode = null;
            foreach (var node in sortedNodes)
            {
                if (largestNode == null)
                    largestNode = node.Value;

                diffBetweenRefs = CarTree.CalculateNodePosition(node.Value);
                for (int i = 0; i < 3; i++)
                    diffBetweenRefs[i] = diffBetweenRefs[i] - robotRefNodePos[i];

                if (IsRobotInNoGoZone(diffBetweenRefs, RobotTree.Level, boundingBoxHalfExtents) && noGo)
                    continue;

                if (IsRobotInSymmetryZone(diffBetweenRefs, RobotTree.Level, boundingBoxHalfExtents) && symmetry)
                    continue;

                largestNode = (VoxelNodeInner)node.Value;
                break;
            }

            var directChilds = CarTree.GetChildNodes((VoxelNodeInner)largestNode).ToArray();

            // Find the best rotation
            // COMMENT FOR PAPER: Finding the rotation first is weak, in case of high values in neighbours but low values in the center, as rotation is than based on missing data
            var maxPossibleMatch = 0.0;
            var maxMatchPossibleRotation = 0;
            for (var i = 0; i < rotations; i++)
            {
                var maxPossibleMatchTemp = 0.0;
                for (var j = 0; j < 8; j++)
                {
                    if (RotatedRobotTrees[i].Nodes[j + 1] == null || directChilds[j] == null)
                        continue;

                    maxPossibleMatchTemp += ((VoxelNodeInner)RotatedRobotTrees[i].Nodes[j + 1]).Max * ((VoxelNodeInner)directChilds[j]).Max;
                }

                if (maxPossibleMatchTemp > maxPossibleMatch)
                {
                    maxPossibleMatch = maxPossibleMatchTemp;
                    maxMatchPossibleRotation = i;
                }
            }

            UpperBound = maxPossibleMatch;

            for (var j = 0; j < 8; j++)
            {
                if (RotatedRobotTrees[maxMatchPossibleRotation].Nodes[j + 1] == null || directChilds[j] == null)
                    continue;

                var robotLeafs = RotatedRobotTrees[maxMatchPossibleRotation]
                    .GetLeafNodes((VoxelNodeInner)RotatedRobotTrees[maxMatchPossibleRotation].Nodes[j + 1]).ToList();
                var carLeafs = CarTree.GetLeafNodes((VoxelNodeInner)directChilds[j]).ToList();

                if (robotLeafs.Count != carLeafs.Count)
                    return double.NaN;

                for (var i = 0; i < robotLeafs.Count; i++)
                {
                    if (robotLeafs.ElementAt(i) == null || carLeafs.ElementAt(i) == null)
                        continue;

                    lbInit += robotLeafs.ElementAt(i).Value * carLeafs.ElementAt(i).Value;
                }
            }

            RotateOperator = maxMatchPossibleRotation;
            TranslateOperator = diffBetweenRefs;

            return lbInit;
        }

        public static double MaxLeafInit(int rotations, double[] boundingBoxHalfExtents, bool noGo, bool symmetry)
        {
            var lbInit = 0.0;

            var carLeafs = CarTree.GetLeafNodesWithIndex();
            var largestCarLeaf = carLeafs.OrderByDescending(n => n.Value?.Value ?? 0.0).First();

            for (int j = 0; j < rotations; j++)
            {
                var robotLeafs = RotatedRobotTrees[j].GetLeafNodesWithIndex().ToList();
                var orderedRobotLeafs = robotLeafs.OrderByDescending(n => n.Value?.Value ?? 0.0).ToList();

                var k = 0;
                var diffBetweenRefs = new int[3];
                foreach (var robotLeaf in orderedRobotLeafs)
                {
                    diffBetweenRefs = CarTree.CalculateNodePosition(largestCarLeaf.Key);
                    var robotRefNodePos = RotatedRobotTrees[j].CalculateNodePosition(robotLeaf.Key);
                    for (var i = 0; i < 3; i++)
                        diffBetweenRefs[i] = diffBetweenRefs[i] - robotRefNodePos[i];

                    if (symmetry)
                    {
                        if (noGo)
                        {
                            if (!IsRobotInNoGoZone(diffBetweenRefs, RobotTree.Level, boundingBoxHalfExtents) && !IsRobotInSymmetryZone(
                                    diffBetweenRefs,
                                    RobotTree.Level,
                                    boundingBoxHalfExtents))
                                break;
                        }
                        else
                        {
                            if (!IsRobotInSymmetryZone(diffBetweenRefs, RobotTree.Level, boundingBoxHalfExtents))
                                break;
                        }
                    }
                    else
                    {
                        if (noGo)
                        {
                            if (!IsRobotInNoGoZone(diffBetweenRefs, RobotTree.Level, boundingBoxHalfExtents))
                                break;
                        }
                        else
                            break;
                    }

                    k++;
                }

                if (k >= orderedRobotLeafs.Count)
                    continue;

                var lbInitTemp = 0.0;
                foreach (var robotLeaf in robotLeafs)
                {
                    if (robotLeaf.Value == null)
                        continue;

                    var robotLeafPos = RotatedRobotTrees[j].CalculateNodePosition(robotLeaf.Key);
                    for (int i = 0; i < 3; i++)
                        robotLeafPos[i] = robotLeafPos[i] + diffBetweenRefs[i];

                    var carTreeLeafValue = CarTree.Get(robotLeafPos[0], robotLeafPos[1], robotLeafPos[2]);
                    if (double.IsNaN(carTreeLeafValue))
                        continue;

                    lbInitTemp += carTreeLeafValue * robotLeaf.Value.Value;
                }

                if (lbInitTemp > lbInit)
                {
                    lbInit = lbInitTemp;
                    RotateOperator = j;
                    TranslateOperator = diffBetweenRefs;
                }
            }

            return lbInit;
        }

        public static bool IsRobotInNoGoZone(int[] distance, int relLevel, double[] boundingBoxHalfExtents)
        {
            // Calc current Pos of reference Voxels
            var robotZeroPos = RobotTree.CalculateNodePosition(RobotTree.Root);

            var zeroPosOfRobotCenter = new int[3];
            var sevenPosOfRobotCenter = new int[3];
            for (int i = 0; i < 3; i++)
                zeroPosOfRobotCenter[i] = robotZeroPos[i] + distance[i];

            // Calc possible Pos of Center Voxels
            var stepSize = (int)((Math.Pow(2, RobotTree.Level - relLevel) - 1) * RobotTree.Precision);

            sevenPosOfRobotCenter[0] = zeroPosOfRobotCenter[0] - stepSize;
            sevenPosOfRobotCenter[1] = zeroPosOfRobotCenter[1] + stepSize;
            sevenPosOfRobotCenter[2] = zeroPosOfRobotCenter[2] + stepSize;

            // Below Grid?
            if (zeroPosOfRobotCenter[2] < 0 && sevenPosOfRobotCenter[2] < 0) // Z-Value of robot root in Worst-Case below 0?
                return true;

            // Above or in Bounding Box of Carbody?
            if (zeroPosOfRobotCenter[0] <= boundingBoxHalfExtents[0] && sevenPosOfRobotCenter[0] <= boundingBoxHalfExtents[0] &&
                zeroPosOfRobotCenter[0] >= -boundingBoxHalfExtents[0] && sevenPosOfRobotCenter[0] >= -boundingBoxHalfExtents[0])
            {
                if (zeroPosOfRobotCenter[1] >= -boundingBoxHalfExtents[1] && sevenPosOfRobotCenter[1] >= -boundingBoxHalfExtents[1] &&
                    zeroPosOfRobotCenter[1] <= boundingBoxHalfExtents[1] && sevenPosOfRobotCenter[1] <= boundingBoxHalfExtents[1])
                    return true;
            }

            // In Front, in or behind Bounding Box of Carbody?
            if (zeroPosOfRobotCenter[0] <= boundingBoxHalfExtents[0] && sevenPosOfRobotCenter[0] <= boundingBoxHalfExtents[0] &&
                zeroPosOfRobotCenter[0] >= -boundingBoxHalfExtents[0] && sevenPosOfRobotCenter[0] >= -boundingBoxHalfExtents[0])
            {
                if (zeroPosOfRobotCenter[2] <= boundingBoxHalfExtents[2] * 2 && sevenPosOfRobotCenter[2] <= boundingBoxHalfExtents[2] * 2 &&
                    zeroPosOfRobotCenter[2] >= 0 && sevenPosOfRobotCenter[2] >= 0)
                    return true;
            }

            return false;
        }

        public static bool IsRobotInSymmetryZone(int[] distance, int relLevel, double[] boundingBoxHalfExtents)
        {
            // Calc current Pos of reference Voxels
            var robotZeroPos = RobotTree.CalculateNodePosition(RobotTree.Root);

            var zeroPosOfRobotCenter = new int[3];
            var sevenPosOfRobotCenter = new int[3];
            for (int i = 0; i < 3; i++)
                zeroPosOfRobotCenter[i] = robotZeroPos[i] + distance[i];

            // Calc possible Pos of Center Voxels
            var stepSize = (int)((Math.Pow(2, RobotTree.Level - relLevel) - 1) * RobotTree.Precision);

            sevenPosOfRobotCenter[0] = zeroPosOfRobotCenter[0] - stepSize;
            sevenPosOfRobotCenter[1] = zeroPosOfRobotCenter[1] + stepSize;
            sevenPosOfRobotCenter[2] = zeroPosOfRobotCenter[2] + stepSize;

            // X-Value smaller than right side of bounding box?
            if (zeroPosOfRobotCenter[0] < -boundingBoxHalfExtents[0] && sevenPosOfRobotCenter[0] < -boundingBoxHalfExtents[0])
                return true;

            return false;
        }

        public static int[] BranchAndBound(
            VoxelOctree cTree,
            VoxelOctree rTree,
            bool maxVoxel,
            bool maxLeaf,
            bool maxValue,
            bool maxLeafs,
            bool maxMax,
            int searchMethod,
            int shiftMethod,
            int maxCycles,
            bool rotation,
            bool noGo,
            bool symmetry,
            double[] boundingBoxHalfExtents)
        {
            Cycles = 0;
            TranslateOperator = new int[3];
            RotateOperator = 0;
            CarTree = cTree;
            RobotTree = rTree;
            StartLevel = CarTree.Level - RobotTree.Level;
            EndLevel = CarTree.Level - StartLevel;
            RotatedRobotTrees = MatrixHelper.AllRotationsOfCube(RobotTree);
            var nodesOnStartLevel = CarTree.GetAncestorNodesWithIndex(StartLevel).ToList();
            var orderedNodesOnStartLevel = nodesOnStartLevel.OrderByDescending(n => n.Value?.Value ?? 0.0).ToList();

            // Number of robot rotations zu check
            var rotationsToCheck = rotation ? RotatedRobotTrees.Length : 1;

            // Find initial lower Bound
            LowerBound = 0.0;
            double lbTemp;

            if (maxVoxel)
                LowerBound = (lbTemp = MaxVoxelInit(orderedNodesOnStartLevel, rotationsToCheck, noGo, symmetry, boundingBoxHalfExtents)) > LowerBound
                                 ? lbTemp
                                 : LowerBound;
            if (maxLeaf)
                LowerBound = (lbTemp = MaxLeafInit(rotationsToCheck, boundingBoxHalfExtents, noGo, symmetry)) > LowerBound ? lbTemp : LowerBound;

            InitLowerBound = LowerBound;

            // Select Search Strategy
            switch (searchMethod)
            {
                case 0:
                    DepthFirstSearch(
                        nodesOnStartLevel,
                        StartLevel,
                        maxCycles,
                        maxValue,
                        maxLeafs,
                        maxMax,
                        rotationsToCheck,
                        noGo,
                        symmetry,
                        boundingBoxHalfExtents);
                    break;
                case 1:
                    BestVoxelFirstSearch(
                        orderedNodesOnStartLevel,
                        StartLevel,
                        maxCycles,
                        maxValue,
                        maxLeafs,
                        maxMax,
                        rotationsToCheck,
                        noGo,
                        symmetry,
                        boundingBoxHalfExtents);
                    break;
                case 2:
                    BruteForce(nodesOnStartLevel, StartLevel, maxCycles, rotationsToCheck, noGo, symmetry, boundingBoxHalfExtents);
                    break;
            }

            return TranslateOperator;
        }

        #endregion
    }
}