using System;
using System.Collections.Generic;
using System.Linq;

namespace RobotEditor.Model
{
    public class VoxelOctree
    {
        #region Instance

        public VoxelOctree(int level)
        {
            Level = level;

            var size = (int)Math.Ceiling((Math.Pow(8, level + 1) - 1) / 7);

            Nodes = new VoxelNode[size];
            Nodes[0] = new VoxelNodeInner();
        }

        #endregion

        #region Properties

        public int Level { get; }

        public VoxelNode Root => Nodes[0];
        public VoxelNode[] Nodes { get; }

        #endregion

        #region Public methods

        public static VoxelOctree Create(double dimension, double stepSize)
        {
            var steps = dimension / stepSize;

            return Create(steps * steps * steps);
        }

        public static VoxelOctree Create(double steps)
        {
            var level = (int)Math.Ceiling(Math.Log(steps, 8));
            if (level < 1)
                return null;

            return new VoxelOctree(level);
        }

        public VoxelNodeInner GetParent(VoxelNode node)
        {
            if (ReferenceEquals(Root, node))
                return null;

            var index = Array.IndexOf(Nodes, node);
            if (index == 0)
                return null;

            var level = (int)Math.Log(index, 8d);
            index = index % (int)Math.Pow(8d, level);

            for (var i = 0; i < level; i++)
                index += (int)Math.Pow(8, i);

            var parentNode = (VoxelNodeInner)Nodes[index];
            if (parentNode == null)
            {
                parentNode = new VoxelNodeInner();
                Nodes[index] = parentNode;
            }

            return parentNode;
        }

        public IEnumerable<VoxelNodeLeaf> GetLeafNodes()
        {
            var index = 0;
            for (var i = 0; i < Level; i++)
                index += (int)Math.Pow(8, i);

            return Nodes.Skip(index).Cast<VoxelNodeLeaf>();
        }

        public IEnumerable<VoxelNodeLeaf> GetLeafNodes(VoxelNodeInner targetNode, int level)
        {
            var levelBelow = Level - level;
            var leafNodeCount = Math.Pow(8, levelBelow);

            var startIndex = 0;
            for (var i = 0; i < Level; i++)
                startIndex += (int)Math.Pow(8, i);

            var previousLeafNodeCount = (int)(Array.IndexOf(Nodes, targetNode) / Math.Pow(8, level));
            startIndex += (int)(previousLeafNodeCount * leafNodeCount);

            var endIndex = startIndex + leafNodeCount;

            for (var i = startIndex; i < endIndex; i++)
                yield return (VoxelNodeLeaf)Nodes[i];
        }

        public IEnumerable<VoxelNode> GetAncestorNodes(int level)
        {
            var startIndex = 0;
            for (var i = 0; i < level; i++)
                startIndex += (int)Math.Pow(8, i);

            var endIndex = startIndex + (int)Math.Pow(8, level);

            for (var i = startIndex; i < endIndex; i++)
                yield return Nodes[i];
        }

        public int GetNodeIndex(int x, int y, int z)
        {
            var path = new int[Level];
            path[0] = 5;
            for (var i = 1; i < Level; i++)
                path[i] = 2;

            if (y != 0 && !Add(path, Math.Abs(y), Math.Sign(y) * 1, new[] { 1, 3, 5, 7 }))
                return -1;

            if (x != 0 && !Add(path, Math.Abs(x), Math.Sign(x) * 2, new[] { 0, 1, 4, 5 }))
                return -1;

            if (z != 0 && !Add(path, Math.Abs(z), Math.Sign(z) * 4, new[] { 4, 5, 6, 7 }))
                return -1;

            var nodeIndex = FindNodeIndex(path);

            return nodeIndex;
        }

        public bool Add(int x, int y, int z, double value)
        {
            if (double.IsNaN(value) || value <= 0)
                return false;

            var nodeIndex = GetNodeIndex(x, y, z);
            if (nodeIndex < 0)
                return false;

            var node = (VoxelNodeLeaf)Nodes[nodeIndex];
            if (node != null)
                return true;

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

        public int FindNodeIndex(int[] path)
        {
            var index = 0;
            for (var i = 0; i < path.Length; i++)
                index += (int)Math.Pow(8, i) + path[i];

            return index;
        }

        #endregion

        #region Private methods

        private static bool AddFactor(int[] path, int level, int factor, int[] jumpLimits)
        {
            if (level < 0)
                return false;

            if (factor >= 0 && !jumpLimits.Contains(path[level]) || factor < 0 && jumpLimits.Contains(path[level]))
            {
                path[level] = path[level] + factor;
                return true;
            }

            path[level] -= factor;
            return AddFactor(path, level - 1, factor, jumpLimits);
        }

        private bool Add(int[] path, int size, int factor, int[] jumpLimits)
        {
            for (var i = 0; i < size; i++)
            {
                if (!AddFactor(path, Level - 1, factor, jumpLimits))
                    return false;
            }

            return true;
        }

        #endregion
    }
}