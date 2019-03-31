using System;
using System.Collections.Generic;
using System.Linq;

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
            NodePathFactorPerLevel = new int[Level];
            // Start points in Array for each level (2)
            StartIndexPerLevel = new int[Level];

            var index = 0;
            for (var i = 0; i < Level; i++)
            {
                var factor = (int)Math.Pow(8, i);
                NodePathFactorPerLevel[i] = factor;

                index += factor;
                StartIndexPerLevel[i] = index;
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

        public int StartIndexLeafNodes => StartIndexPerLevel[Level - 1];

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

        public IEnumerable<VoxelNode> GetAncestorNodes(VoxelNode node)
        {
            var endIndex = StartIndexLeafNodes + NodePathFactorPerLevel[CalculateNodeLevel(node) + 1];

            for (var i = StartIndexLeafNodes; i < endIndex; i++)
                yield return Nodes[i];
        }

        public IEnumerable<VoxelNode> GetAncestorNodes(int level)
        {
            var startIndex = StartIndexPerLevel[level - 1];
            var endIndex = startIndex + NodePathFactorPerLevel[level];

            for (var i = startIndex; i < endIndex; i++)
                yield return Nodes[i];
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
            var level = CalculateNodeLevel(node) + 1;

            var levelBelow = Level - level;
            var leafNodeCount = NodePathFactorPerLevel[levelBelow];

            var startIndex = StartIndexLeafNodes;

            var nodeIndex = Array.IndexOf(Nodes, node);
            var previousLeafNodeCount = nodeIndex - StartIndexPerLevel[level - 1];
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

        public void RotateX()
        {
            //var tempTree = new VoxelOctree(Level)
            //{
            //    Nodes =
            //    {
            //        [1] = Nodes[5],
            //        [2] = Nodes[6],
            //        [3] = Nodes[1],
            //        [4] = Nodes[2],
            //        [5] = Nodes[7],
            //        [6] = Nodes[8],
            //        [7] = Nodes[3],
            //        [8] = Nodes[4]
            //    }
            //};

            var tempTree = new VoxelOctree(Level, Precision)
            {
                Nodes =
                {
                    [1] = Nodes[1],
                    [2] = Nodes[2],
                    [3] = Nodes[3],
                    [4] = Nodes[4],
                    [5] = Nodes[5],
                    [6] = Nodes[6],
                    [7] = Nodes[7],
                    [8] = Nodes[8]
                }
            };

            CopyChildNodesBelowLevel1(tempTree);

            Nodes = tempTree.Nodes;
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

            foreach (var leafNodeThis in leafNodesThis)
                RecalcMinMax(leafNodeThis.Key);
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

        #endregion

        #region Private methods

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
            if (index <= 0)
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
            // Find center of cube in octree

            /*
            var path = new int[Level];
            path[0] = 5;
            for (var i = 1; i < Level; i++)
                path[i] = 2;

            if (z != 0 && !Search(path, Math.Abs(z), Math.Sign(z) * 4, new[] { 4, 5, 6, 7 }))
                return -1;

            if (y != 0 && !Search(path, Math.Abs(y), Math.Sign(y) * 1, new[] { 1, 3, 5, 7 }))
                return -1;

            if (x != 0 && !Search(path, Math.Abs(x), Math.Sign(x) * -2, new[] { 2, 3, 6, 7 }))
                return -1;
                */

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
                index += (path[i] + 1) * NodePathFactorPerLevel[path.Length - 1 - i];

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

        private bool Set(int nodeIndex, double value)
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

        private void Update(int nodeIndex, double value)
        {
            var node = (VoxelNodeLeaf)Nodes[nodeIndex];
            if (node == null)
            {
                node = new VoxelNodeLeaf();
                Nodes[nodeIndex] = node;
            }

            node.Value = value;

            VoxelNodeInner parent = null;
            while ((parent = GetParent((VoxelNode)parent ?? node)) != null)
            {
                parent.Min = double.NaN;
                parent.Max = double.NaN;
            }
        }

        private void RecalcMinMax(int nodeIndex)
        {
            var node = (VoxelNodeLeaf)Nodes[nodeIndex];
            if (node == null)
                return;

            var value = node.Value;

            VoxelNodeInner parent = null;
            while ((parent = GetParent((VoxelNode)parent ?? node)) != null)
            {
                parent.Min = double.IsNaN(parent.Min) ? value : Math.Min(value, parent.Min);
                parent.Max = double.IsNaN(parent.Max) ? value : Math.Max(value, parent.Max);
            }
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