﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media.Media3D;

using RobotEditor.Helper;

namespace RobotEditor.Model
{
    public class VoxelOctree
    {
        #region Instance

        public VoxelOctree(int level, double precision)
        {
            Level = level;
            Precision = precision;

            var size = (int)Math.Ceiling((Math.Pow(8, level + 1) - 1) / 7);

            Nodes = new VoxelNode[size];
            Nodes[0] = new VoxelNodeInner();

            // Length of Array segment per level (1)
            NodePathFactorPerLevel = new int[Level + 1];
            // Start points in Array for each level (2)
            StartIndexPerLevel = new int[Level + 1];

            var index = 0;
            for (var i = 0; i <= Level; i++)
            {
                var factor = (int)Math.Pow(8, i);
                NodePathFactorPerLevel[i] = factor;

                StartIndexPerLevel[i] = index;
                index += factor;
            }
        }

        #endregion

        #region Properties

        public int Level { get; }

        public double Precision { get; set; }

        public VoxelNodeInner Root => (VoxelNodeInner)Nodes[0];

        public VoxelNode[] Nodes { get; private set; }

        public int[] NodePathFactorPerLevel { get; }

        public int[] StartIndexPerLevel { get; }

        public int StartIndexLeafNodes => StartIndexPerLevel[Level];

        #endregion

        #region Public methods

        public static VoxelOctree Create(double dimension, double stepSize)
        {
            var leafNodeCount = dimension / stepSize;

            var level = (int)Math.Ceiling(Math.Log(leafNodeCount * leafNodeCount * leafNodeCount, 8));
            if (level < 1)
                return null;

            return new VoxelOctree(level, stepSize);
        }

        public void Clear()
        {
            var size = (int)Math.Ceiling((Math.Pow(8, Level + 1) - 1) / 7);

            Nodes = new VoxelNode[size];
            Nodes[0] = new VoxelNodeInner();
        }

        public VoxelNodeInner GetParent(VoxelNode node)
        {
            if (ReferenceEquals(Root, node))
                return null;

            var index = Array.IndexOf(Nodes, node);
            if (index == 0)
                return null;

            var level = CalculateNodeLevel(index);
            if (level == 0)
                return Root;

            if (level < 0)
                return null;

            index = (index - StartIndexPerLevel[level]) / 8;
            index += StartIndexPerLevel[level - 1];

            var parentNode = (VoxelNodeInner)Nodes[index];
            if (parentNode == null)
            {
                parentNode = new VoxelNodeInner();
                Nodes[index] = parentNode;
            }

            return parentNode;
        }

        public int? GetParentIndex(int nodeIndex)
        {
            if (nodeIndex == 0)
                return null;

            var level = CalculateNodeLevel(nodeIndex);
            if (level == 0)
                return null;

            if (level < 0)
                return null;

            nodeIndex = (nodeIndex - StartIndexPerLevel[level]) / 8;
            nodeIndex += StartIndexPerLevel[level - 1];

            /*
            var parentNode = (VoxelNodeInner)Nodes[nodeIndex];
            if (parentNode == null)
            {
                parentNode = new VoxelNodeInner();
                Nodes[nodeIndex] = parentNode;
            }*/

            return nodeIndex;
        }

        public IEnumerable<VoxelNode> GetAncestorNodes(VoxelNode node)
        {
            var endIndex = StartIndexLeafNodes + NodePathFactorPerLevel[CalculateNodeLevel(node) + 1];

            for (var i = StartIndexLeafNodes; i < endIndex; i++)
                yield return Nodes[i];
        }

        public IEnumerable<VoxelNode> GetAncestorNodes(int level)
        {
            var startIndex = StartIndexPerLevel[level];
            var endIndex = startIndex + NodePathFactorPerLevel[level];

            for (var i = startIndex; i < endIndex; i++)
                yield return Nodes[i];
        }

        public IEnumerable<KeyValuePair<int, VoxelNode>> GetAncestorNodesWithIndex(int level)
        {
            var startIndex = StartIndexPerLevel[level];
            var endIndex = startIndex + NodePathFactorPerLevel[level];

            for (var i = startIndex; i < endIndex; i++)
                yield return new KeyValuePair<int, VoxelNode>(i, Nodes[i]);
        }

        public IEnumerable<KeyValuePair<int, double>> GetAncestorNeighValueWithindex(int level)
        {
            var startIndex = StartIndexPerLevel[level];
            var endIndex = startIndex + NodePathFactorPerLevel[level];

            for (var i = startIndex; i < endIndex; i++)
            {
                var neighborOffsets = MatrixHelper.GetAllSurroundingVoxel(level, i - startIndex);

                var neighborValue = Nodes[startIndex + (int)neighborOffsets[0]] == null ? 0.0 : Nodes[startIndex + (int)neighborOffsets[0]].Value;
                for (var k = 1; k < 27; k++)
                {
                    if (neighborOffsets[k] == null) // Border of Octree reached
                        continue;

                    if (Nodes[startIndex + (int)neighborOffsets[k]] == null)
                        continue;

                    neighborValue += Nodes[startIndex + (int)neighborOffsets[k]].Value;
                }

                yield return new KeyValuePair<int, double>(i, neighborValue);
            }
        }

        public IEnumerable<VoxelNodeLeaf> GetLeafNodes()
        {
            for (var i = StartIndexLeafNodes; i < Nodes.Length; i++)
                yield return (VoxelNodeLeaf)Nodes[i];
        }

        public IEnumerable<KeyValuePair<int, VoxelNodeLeaf>> GetLeafNodesWithIndex()
        {
            for (var i = StartIndexLeafNodes; i < Nodes.Length; i++)
                yield return new KeyValuePair<int, VoxelNodeLeaf>(i, (VoxelNodeLeaf)Nodes[i]);
        }

        public IEnumerable<VoxelNodeLeaf> GetLeafNodes(VoxelNodeInner node)
        {
            var level = CalculateNodeLevel(node);

            var levelBelow = Level - level;
            var leafNodeCount = NodePathFactorPerLevel[levelBelow];

            var startIndex = StartIndexPerLevel[Level];

            var nodeIndex = Array.IndexOf(Nodes, node);

            var previousLeafNodeCount = nodeIndex - StartIndexPerLevel[level];
            startIndex += previousLeafNodeCount * leafNodeCount;

            var endIndex = startIndex + leafNodeCount;

            for (var i = startIndex; i < endIndex; i++)
                yield return (VoxelNodeLeaf)Nodes[i];
        }

        public IEnumerable<VoxelNode> GetChildNodes(VoxelNodeInner node)
        {
            var level = CalculateNodeLevel(node) + 1;

            var startIndex = StartIndexPerLevel[level];

            var nodeIndex = Array.IndexOf(Nodes, node);
            var previousLeafNodeCount = nodeIndex - StartIndexPerLevel[level - 1];
            startIndex += previousLeafNodeCount * 8;

            var endIndex = startIndex + 8;

            for (var i = startIndex; i < endIndex; i++)
                yield return Nodes[i];
        }

        public IEnumerable<KeyValuePair<int, VoxelNode>> GetChildNodesWithIndex(int nodeIndex)
        {
            var level = CalculateNodeLevel(nodeIndex) + 1;

            var startIndex = StartIndexPerLevel[level];

            var previousLeafNodeCount = nodeIndex - StartIndexPerLevel[level - 1];
            startIndex += previousLeafNodeCount * 8;

            var endIndex = startIndex + 8;

            for (var i = startIndex; i < endIndex; i++)
                yield return new KeyValuePair<int, VoxelNode>(i, Nodes[i]);
        }

        public IEnumerable<KeyValuePair<int, VoxelNode>> GetChildNodesWithIndex(VoxelNodeInner node)
        {
            var level = CalculateNodeLevel(node) + 1;

            var startIndex = StartIndexPerLevel[level];

            var nodeIndex = Array.IndexOf(Nodes, node);
            var previousLeafNodeCount = nodeIndex - StartIndexPerLevel[level - 1];
            startIndex += previousLeafNodeCount * 8;

            var endIndex = startIndex + 8;

            for (var i = startIndex; i < endIndex; i++)
                yield return new KeyValuePair<int, VoxelNode>(i, Nodes[i]);
        }

        public IEnumerable<KeyValuePair<int, VoxelNode>> GetChildNodesWithIndexOnLevel(VoxelNode node)
        {
            var level = CalculateNodeLevel(node) + 1;

            var startIndex = 0;

            var nodeIndex = Array.IndexOf(Nodes, node);
            var previousLeafNodeCount = nodeIndex - StartIndexPerLevel[level - 1];
            startIndex += previousLeafNodeCount * 8;

            var endIndex = startIndex + 8;

            for (var i = startIndex; i < endIndex; i++)
                yield return new KeyValuePair<int, VoxelNode>(i, Nodes[i]);
        }

        public VoxelOctree Clone()
        {
            var clone = new VoxelOctree(Level, Precision);
            Nodes.CopyTo(clone.Nodes, 0);

            return clone;
        }

        public void RotateX(int rx)
        {
            if (rx == 0)
                return;

            var leafNodesThis = GetLeafNodesWithIndex().ToArray();
            var octreeTemp = new VoxelOctree(Level, Precision);

            var maxLeafThis = leafNodesThis.Max(n => n.Key);
            for (var i = StartIndexLeafNodes; i <= maxLeafThis; i++)
            {
                var leafNodeThis = (VoxelNodeLeaf)Nodes[i];
                var valueThis = leafNodeThis == null || double.IsNaN(leafNodeThis.Value) ? 0 : leafNodeThis.Value;

                if (Math.Abs(valueThis) < 0.0000001)
                    continue;

                var position = CalculateNodePosition(leafNodeThis);

                var vectorTemp = new Vector3D(position[0], position[1], position[2]);
                vectorTemp = Vector3D.Multiply(vectorTemp, MatrixHelper.NewMatrixRotateAroundX(rx));

                octreeTemp.Set((int)vectorTemp.X, (int)vectorTemp.Y, (int)vectorTemp.Z, valueThis);
            }

            Nodes = octreeTemp.Nodes;
        }

        public void RotateY(int ry)
        {
            if (ry == 0)
                return;

            var leafNodesThis = GetLeafNodesWithIndex().ToArray();
            var octreeTemp = new VoxelOctree(Level, Precision);

            var maxLeafThis = leafNodesThis.Max(n => n.Key);
            for (var i = StartIndexLeafNodes; i <= maxLeafThis; i++)
            {
                var leafNodeThis = (VoxelNodeLeaf)Nodes[i];
                var valueThis = leafNodeThis == null || double.IsNaN(leafNodeThis.Value) ? 0 : leafNodeThis.Value;

                if (Math.Abs(valueThis) < 0.0000001)
                    continue;

                var position = CalculateNodePosition(leafNodeThis);

                var vectorTemp = new Vector3D(position[0], position[1], position[2]);
                vectorTemp = Vector3D.Multiply(vectorTemp, MatrixHelper.NewMatrixRotateAroundY(ry));

                octreeTemp.Set((int)vectorTemp.X, (int)vectorTemp.Y, (int)vectorTemp.Z, valueThis);
            }

            Nodes = octreeTemp.Nodes;
        }

        public void RotateZ(int rz)
        {
            if (rz == 0)
                return;

            var leafNodesThis = GetLeafNodesWithIndex().ToArray();
            var octreeTemp = new VoxelOctree(Level, Precision);

            var maxLeafThis = leafNodesThis.Max(n => n.Key);
            for (var i = StartIndexLeafNodes; i <= maxLeafThis; i++)
            {
                var leafNodeThis = (VoxelNodeLeaf)Nodes[i];
                var valueThis = leafNodeThis == null || double.IsNaN(leafNodeThis.Value) ? 0 : leafNodeThis.Value;

                if (Math.Abs(valueThis) < 0.0000001)
                    continue;

                var position = CalculateNodePosition(leafNodeThis);

                var vectorTemp = new Vector3D(position[0], position[1], position[2]);
                vectorTemp = Vector3D.Multiply(vectorTemp, MatrixHelper.NewMatrixRotateAroundZ(rz));

                octreeTemp.Set((int)vectorTemp.X, (int)vectorTemp.Y, (int)vectorTemp.Z, valueThis);
            }

            Nodes = octreeTemp.Nodes;
        }

        public double Get(int x, int y, int z)
        {
            var nodeIndex = CalculateNodeIndex(x, y, z);
            if (nodeIndex < 0)
                return double.NaN;

            return Get(nodeIndex);
        }

        // X, Y, Z from center of cube
        public bool Set(int x, int y, int z, double value)
        {
            if (double.IsNaN(value) || Math.Abs(value) < 0.0000001)
                return false;

            var nodeIndex = CalculateNodeIndex(x, y, z);
            if (nodeIndex < 0)
                return false;

            return Set(nodeIndex, value);
        }

        public double Get(int[] path)
        {
            var nodeIndex = CalculateNodeIndex(path);
            if (nodeIndex < 0)
                return double.NaN;

            return Get(nodeIndex);
        }

        public bool Set(int[] path, double value)
        {
            if (double.IsNaN(value) || Math.Abs(value) < 0.0000001)
                return false;

            var nodeIndex = CalculateNodeIndex(path);
            if (nodeIndex < 0)
                return false;

            return Set(nodeIndex, value);
        }

        public void Add(VoxelOctree tree)
        {
            Debug.Assert(Level >= tree.Level, "Wrong tree size");

            if (tree.Level == Level)
                AddSameLevelSize(tree);
            else
                AddFromRoot(tree);
        }

        public double Multiply(VoxelNodeInner node, VoxelOctree tree)
        {
            var count = 0;
            var leafNodesOther = tree.GetLeafNodes().ToArray();
            var result = 0d;
            foreach (var leafNodeThis in GetLeafNodes(node))
            {
                var leafNodeOther = leafNodesOther[count];

                count++;

                if (leafNodeThis == null || double.IsNaN(leafNodeThis.Value))
                    continue;

                if (leafNodeOther == null || double.IsNaN(leafNodeOther.Value))
                    continue;

                result += leafNodeThis.Value * leafNodeOther.Value;
            }

            return result;
        }

        public void RecalcMinMaxSum()
        {
            for (var i = Level; i > 0; i--) // Start from leaf nodes
            {
                VoxelNodeInner parentNode = null;
                var endIndexOfThisLevel = StartIndexPerLevel[i] + NodePathFactorPerLevel[i];
                for (var j = StartIndexPerLevel[i]; j < endIndexOfThisLevel; j++)
                {
                    if (Nodes[j] == null)
                        continue;

                    var parentNodeTemp = GetParent(Nodes[j]);

                    if (parentNodeTemp != parentNode) // Reset values for new parent node
                    {
                        parentNode = parentNodeTemp;
                        parentNode.Min = double.NaN;
                        parentNode.Max = double.NaN;
                        parentNode.Value = 0.0;
                    }

                    if (i == Level)
                    {
                        parentNode.Min = double.IsNaN(parentNode.Min) ? Nodes[j].Value : Math.Min(Nodes[j].Value, parentNode.Min);
                        parentNode.Max = double.IsNaN(parentNode.Max) ? Nodes[j].Value : Math.Max(Nodes[j].Value, parentNode.Max);
                    }
                    else
                    {
                        parentNode.Min = double.IsNaN(parentNode.Min)
                                             ? ((VoxelNodeInner)Nodes[j]).Min
                                             : Math.Min(((VoxelNodeInner)Nodes[j]).Min, parentNode.Min);
                        parentNode.Max = double.IsNaN(parentNode.Max)
                                             ? ((VoxelNodeInner)Nodes[j]).Max
                                             : Math.Max(((VoxelNodeInner)Nodes[j]).Max, parentNode.Max);
                    }

                    parentNode.Value += double.IsNaN(Nodes[j].Value) ? 0.0 : Nodes[j].Value;

                    if (double.IsNaN(parentNode.Min))
                        Console.WriteLine("Fehler");
                }
            }

            ;
        }

        public void ClearInXyzFromRoot(VoxelOctree tree, int x, int y, int z)
        {
            var leafNodesOther = tree.GetLeafNodesWithIndex().ToArray();

            var maxLeafOther = leafNodesOther.Max(n => n.Key);
            for (var i = tree.StartIndexLeafNodes; i <= maxLeafOther; i++)
            {
                var leafNodeOther = (VoxelNodeLeaf)tree.Nodes[i];
                var valueOther = leafNodeOther == null || double.IsNaN(leafNodeOther.Value) ? 0 : leafNodeOther.Value;

                if (Math.Abs(valueOther) < 0.0000001)
                    continue;

                var position = tree.CalculateNodePosition(leafNodeOther);

                Nodes[CalculateNodeIndex(position[0] + x, position[1] + y, position[2] + z)] = null;
            }
        }

        public void AddInXyzFromRoot(VoxelOctree tree, int x, int y, int z)
        {
            var leafNodesOther = tree.GetLeafNodesWithIndex().ToArray();

            var maxLeafOther = leafNodesOther.Max(n => n.Key);
            for (var i = tree.StartIndexLeafNodes; i <= maxLeafOther; i++)
            {
                var leafNodeOther = (VoxelNodeLeaf)tree.Nodes[i];
                var valueOther = leafNodeOther == null || double.IsNaN(leafNodeOther.Value) ? 0 : leafNodeOther.Value;

                if (Math.Abs(valueOther) < 0.0000001)
                    continue;

                var position = tree.CalculateNodePosition(leafNodeOther);
                Set(position[0] + x, position[1] + y, position[2] + z, valueOther);
            }
        }

        public void AddInXyz(VoxelOctree tree, int x, int y, int z)
        {
            var leafNodesOther = tree.GetLeafNodesWithIndex().ToArray();

            var maxLeafOther = leafNodesOther.Max(n => n.Key);
            for (var i = tree.StartIndexLeafNodes; i <= maxLeafOther; i++)
            {
                var leafNodeOther = (VoxelNodeLeaf)tree.Nodes[i];
                var valueOther = leafNodeOther == null || double.IsNaN(leafNodeOther.Value) ? 0 : leafNodeOther.Value;

                if (Math.Abs(valueOther) < 0.0000001)
                    continue;

                var position = tree.CalculateNodePosition(leafNodeOther);
                var valueThis = Get(position[0] + x, position[1] + y, position[2] + z);
                if (double.IsNaN(valueThis))
                    valueThis = 0.0;

                Update(position[0] + x, position[1] + y, position[2] + z, valueOther + valueThis);
            }
        }

        public int[] CalculateNodePosition(int nodeIndex)
        {
            double[] positionTemp = new double[3];
            var tempNodeIndex = nodeIndex;
            var startLevel = CalculateNodeLevel(nodeIndex);

            for (var i = startLevel; i > 0; i--)
            {
                var differenceToAdd = Precision * Math.Pow(2, Level - i) / 2;
                var path = tempNodeIndex;
                tempNodeIndex = (int)GetParentIndex(tempNodeIndex);

                switch ((path - StartIndexPerLevel[i]) % 8)
                {
                    case 0:
                        positionTemp[0] += differenceToAdd;
                        positionTemp[1] -= differenceToAdd;
                        positionTemp[2] -= differenceToAdd;
                        break;
                    case 1:
                        positionTemp[0] += differenceToAdd;
                        positionTemp[1] += differenceToAdd;
                        positionTemp[2] -= differenceToAdd;
                        break;
                    case 2:
                        positionTemp[0] -= differenceToAdd;
                        positionTemp[1] -= differenceToAdd;
                        positionTemp[2] -= differenceToAdd;
                        break;
                    case 3:
                        positionTemp[0] -= differenceToAdd;
                        positionTemp[1] += differenceToAdd;
                        positionTemp[2] -= differenceToAdd;
                        break;
                    case 4:
                        positionTemp[0] += differenceToAdd;
                        positionTemp[1] -= differenceToAdd;
                        positionTemp[2] += differenceToAdd;
                        break;
                    case 5:
                        positionTemp[0] += differenceToAdd;
                        positionTemp[1] += differenceToAdd;
                        positionTemp[2] += differenceToAdd;
                        break;
                    case 6:
                        positionTemp[0] -= differenceToAdd;
                        positionTemp[1] -= differenceToAdd;
                        positionTemp[2] += differenceToAdd;
                        break;
                    case 7:
                        positionTemp[0] -= differenceToAdd;
                        positionTemp[1] += differenceToAdd;
                        positionTemp[2] += differenceToAdd;
                        break;
                }
            }

            int[] position = new int[3];
            position[0] = (int)positionTemp[0];
            position[1] = (int)positionTemp[1];
            position[2] = (int)positionTemp[2];

            return position;
        }

        public int[] CalculateNodePosition(VoxelNode node)
        {
            double[] positionTemp = new double[3];
            var tempNode = node;
            var startLevel = CalculateNodeLevel(node);

            for (var i = startLevel; i > 0; i--)
            {
                var differenceToAdd = Precision * Math.Pow(2, Level - i) / 2;
                var path = Array.IndexOf(Nodes, tempNode);
                tempNode = GetParent(tempNode);

                switch ((path - StartIndexPerLevel[i]) % 8)
                {
                    case 0:
                        positionTemp[0] += differenceToAdd;
                        positionTemp[1] -= differenceToAdd;
                        positionTemp[2] -= differenceToAdd;
                        break;
                    case 1:
                        positionTemp[0] += differenceToAdd;
                        positionTemp[1] += differenceToAdd;
                        positionTemp[2] -= differenceToAdd;
                        break;
                    case 2:
                        positionTemp[0] -= differenceToAdd;
                        positionTemp[1] -= differenceToAdd;
                        positionTemp[2] -= differenceToAdd;
                        break;
                    case 3:
                        positionTemp[0] -= differenceToAdd;
                        positionTemp[1] += differenceToAdd;
                        positionTemp[2] -= differenceToAdd;
                        break;
                    case 4:
                        positionTemp[0] += differenceToAdd;
                        positionTemp[1] -= differenceToAdd;
                        positionTemp[2] += differenceToAdd;
                        break;
                    case 5:
                        positionTemp[0] += differenceToAdd;
                        positionTemp[1] += differenceToAdd;
                        positionTemp[2] += differenceToAdd;
                        break;
                    case 6:
                        positionTemp[0] -= differenceToAdd;
                        positionTemp[1] -= differenceToAdd;
                        positionTemp[2] += differenceToAdd;
                        break;
                    case 7:
                        positionTemp[0] -= differenceToAdd;
                        positionTemp[1] += differenceToAdd;
                        positionTemp[2] += differenceToAdd;
                        break;
                }
            }

            int[] position = new int[3];
            position[0] = (int)positionTemp[0];
            position[1] = (int)positionTemp[1];
            position[2] = (int)positionTemp[2];

            return position;
        }

        public bool Set(int nodeIndex, double value)
        {
            var node = (VoxelNodeLeaf)Nodes[nodeIndex];
            if (node != null)
                return false;

            node = new VoxelNodeLeaf { Value = value };
            Nodes[nodeIndex] = node;

            VoxelNodeInner parent = null;
            while ((parent = GetParent((VoxelNode)parent ?? node)) != null)
            {
                parent.Min = double.IsNaN(parent.Min) ? value : Math.Min(value, parent.Min);
                parent.Max = double.IsNaN(parent.Max) ? value : Math.Max(value, parent.Max);
            }

            return true;
        }

        #endregion

        #region Private methods

        private void AddFromRoot(VoxelOctree tree)
        {
            var leafNodesOther = tree.GetLeafNodesWithIndex().ToArray();

            var maxLeafOther = leafNodesOther.Max(n => n.Key);
            for (var i = tree.StartIndexLeafNodes; i <= maxLeafOther; i++)
            {
                var leafNodeOther = (VoxelNodeLeaf)tree.Nodes[i];
                var valueOther = leafNodeOther == null || double.IsNaN(leafNodeOther.Value) ? 0 : leafNodeOther.Value;

                if (Math.Abs(valueOther) < 0.0000001)
                    continue;

                var position = tree.CalculateNodePosition(leafNodeOther);
                Set(position[0], position[1], position[2], valueOther);
            }
        }

        private void AddSameLevelSize(VoxelOctree tree)
        {
            var index = 0;
            var leafNodesOther = tree.GetLeafNodes().ToArray();
            var leafNodesThis = GetLeafNodesWithIndex().ToArray();

            foreach (var leafNodeThis in leafNodesThis)
            {
                if (index >= leafNodesOther.Length)
                    break;

                var leafNodeOther = leafNodesOther[index];

                index++;

                var valueThis = leafNodeThis.Value == null || double.IsNaN(leafNodeThis.Value.Value) ? 0 : leafNodeThis.Value.Value;
                var valueOther = leafNodeOther == null || double.IsNaN(leafNodeOther.Value) ? 0 : leafNodeOther.Value;

                if (Math.Abs(valueThis) < 0.0000001 && Math.Abs(valueOther) < 0.0000001)
                    continue;

                Update(leafNodeThis.Key, valueThis + valueOther);
            }
        }

        private void SetChildNodes(VoxelNodeInner node, IEnumerable<VoxelNode> children)
        {
            var level = CalculateNodeLevel(node) + 1;

            var startIndex = StartIndexPerLevel[level];

            var nodeIndex = Array.IndexOf(Nodes, node);
            var previousLeafNodeCount = nodeIndex - StartIndexPerLevel[level - 1];
            startIndex += previousLeafNodeCount * 8;

            var count = 0;
            foreach (var child in children)
            {
                Nodes[startIndex + count] = child;

                count++;
            }
        }

        private int CalculateNodeLevel(VoxelNode node)
        {
            var index = Array.IndexOf(Nodes, node);

            return CalculateNodeLevel(index);
        }

        private int CalculateNodeLevel(int index)
        {
            if (index < 0)
                return -1;

            for (var i = StartIndexPerLevel.Length - 1; i >= 0; i--)
            {
                if (index >= StartIndexPerLevel[i])
                    return i;
            }

            return -1;
        }

        private int CalculateNodeIndex(int x, int y, int z)
        {
            var path = new int[Level];

            int centerBoundX = 0;
            int centerBoundY = 0;
            int centerBoundZ = 0;

            for (var i = 0; i < Level; i++)
            {
                var j = Level - 2 - i;
                if (x >= centerBoundX)
                {
                    centerBoundX += (int)(Math.Pow(2, j) * Precision);
                    if (y >= centerBoundY)
                    {
                        centerBoundY += (int)(Math.Pow(2, j) * Precision);
                        if (z >= centerBoundZ)
                        {
                            path[i] = 5;
                            centerBoundZ += (int)(Math.Pow(2, j) * Precision);
                        }
                        else
                        {
                            path[i] = 1;
                            centerBoundZ += -(int)(Math.Pow(2, j) * Precision);
                        }
                    }
                    else
                    {
                        centerBoundY += -(int)(Math.Pow(2, j) * Precision);
                        if (z >= centerBoundZ)
                        {
                            path[i] = 4;
                            centerBoundZ += (int)(Math.Pow(2, j) * Precision);
                        }
                        else
                        {
                            path[i] = 0;
                            centerBoundZ += -(int)(Math.Pow(2, j) * Precision);
                        }
                    }
                }
                else
                {
                    centerBoundX += -(int)(Math.Pow(2, j) * Precision);
                    if (y >= centerBoundY)
                    {
                        centerBoundY += (int)(Math.Pow(2, j) * Precision);
                        if (z >= centerBoundZ)
                        {
                            path[i] = 7;
                            centerBoundZ += (int)(Math.Pow(2, j) * Precision);
                        }
                        else
                        {
                            path[i] = 3;
                            centerBoundZ += -(int)(Math.Pow(2, j) * Precision);
                        }
                    }
                    else
                    {
                        centerBoundY += -(int)(Math.Pow(2, j) * Precision);
                        if (z >= centerBoundZ)
                        {
                            path[i] = 6;
                            centerBoundZ += (int)(Math.Pow(2, j) * Precision);
                        }
                        else
                        {
                            path[i] = 2;
                            centerBoundZ += -(int)(Math.Pow(2, j) * Precision);
                        }
                    }
                }
            }

            var nodeIndex = CalculateNodeIndex(path);

            return nodeIndex;
        }

        private int CalculateNodeIndex(int[] path)
        {
            var index = 0;
            for (var i = 0; i < path.Length; i++)
                index += (path[i] + 1) * NodePathFactorPerLevel[path.Length - i - 1];

            //index += path[path.Length - 1];

            return index;
        }

        private double Get(int nodeIndex)
        {
            var node = (VoxelNodeLeaf)Nodes[nodeIndex];
            if (node == null)
                return double.NaN;

            return node.Value;
        }

        private void Update(int nodeIndex, double value)
        {
            var node = (VoxelNodeLeaf)Nodes[nodeIndex];
            if (node == null)
            {
                node = new VoxelNodeLeaf();
                Nodes[nodeIndex] = node;
            }

            node.Value = value;
        }

        private void Update(int x, int y, int z, double value)
        {
            var nodeIndex = CalculateNodeIndex(x, y, z);
            if (nodeIndex < 0)
                return;

            var node = (VoxelNodeLeaf)Nodes[nodeIndex];
            if (node == null)
            {
                node = new VoxelNodeLeaf();
                Nodes[nodeIndex] = node;
            }

            node.Value = value;
        }

        private void CopyChildNodesBelowLevel1(VoxelOctree tree)
        {
            for (var i = 1; i < 9; i++)
            {
                var node = (VoxelNodeInner)Nodes[i];
                CopyChildNodes(node, tree);
            }
        }

        private void CopyChildNodes(VoxelNodeInner node, VoxelOctree tree)
        {
            var childNodes = GetChildNodes(node).ToArray();

            tree.SetChildNodes(node, childNodes);

            foreach (var child in childNodes)
            {
                var childInner = child as VoxelNodeInner;
                if (childInner != null)
                    CopyChildNodes(childInner, tree);
            }
        }

        #endregion
    }
}