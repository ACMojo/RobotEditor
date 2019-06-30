using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

using RobotEditor.Model;

namespace RobotEditor.Helper
{
    public static class MatrixHelper
    {
        #region Enums

        public enum Directions
        {
            X = 4,
            Y = 8,
            Z = 2
        }

        public enum PosNeg
        {
            Pos = 1,
            Neg = -1
        }

        #endregion

        #region Public methods

        public static double GetValue(double angle)
        {
            return Math.PI / 180 * angle;
        }

        public static Matrix3D NewMatrixRotateAroundX(double degree)
        {
            var radians = GetValue(degree);
            var matrix = Matrix3D.Identity;
            matrix.M22 = Math.Cos(radians);
            matrix.M32 = Math.Sin(radians);
            matrix.M23 = -Math.Sin(radians);
            matrix.M33 = Math.Cos(radians);
            return matrix;
        }

        public static Matrix3D NewMatrixRotateAroundY(double degree)
        {
            var radians = GetValue(degree);
            var matrix = Matrix3D.Identity;
            matrix.M11 = Math.Cos(radians);
            matrix.M31 = -Math.Sin(radians);
            matrix.M13 = Math.Sin(radians);
            matrix.M33 = Math.Cos(radians);
            return matrix;
        }

        public static Matrix3D NewMatrixRotateAroundZ(double degree)
        {
            var radians = GetValue(degree);
            var matrix = Matrix3D.Identity;
            matrix.M11 = Math.Cos(radians);
            matrix.M21 = Math.Sin(radians);
            matrix.M12 = -Math.Sin(radians);
            matrix.M22 = Math.Cos(radians);
            return matrix;
        }

        public static VoxelOctree[] allRotationsOfCube(VoxelOctree cubeToRotate)
        {
            VoxelOctree[] rotatedRobots = new VoxelOctree[24];

            rotatedRobots[0] = cubeToRotate.Clone(); // Identity

            rotatedRobots[1] = rotatedRobots[0].Clone(); // X     
            rotatedRobots[1].RotateX(90);
            rotatedRobots[1].RecalcMinMaxSum();

            rotatedRobots[2] = rotatedRobots[0].Clone(); // Y
            rotatedRobots[2].RotateY(90);
            rotatedRobots[2].RecalcMinMaxSum();

            rotatedRobots[3] = rotatedRobots[1].Clone(); // XX
            rotatedRobots[3].RotateX(90);
            rotatedRobots[3].RecalcMinMaxSum();

            rotatedRobots[4] = rotatedRobots[1].Clone(); // XY
            rotatedRobots[4].RotateY(90);
            rotatedRobots[4].RecalcMinMaxSum();

            rotatedRobots[5] = rotatedRobots[2].Clone(); // YX
            rotatedRobots[5].RotateX(90);
            rotatedRobots[5].RecalcMinMaxSum();

            rotatedRobots[6] = rotatedRobots[2].Clone(); // YY
            rotatedRobots[6].RotateY(90);
            rotatedRobots[6].RecalcMinMaxSum();

            rotatedRobots[7] = rotatedRobots[3].Clone(); // XXX
            rotatedRobots[7].RotateX(90);
            rotatedRobots[7].RecalcMinMaxSum();

            rotatedRobots[8] = rotatedRobots[3].Clone(); // XXY
            rotatedRobots[8].RotateY(90);
            rotatedRobots[8].RecalcMinMaxSum();

            rotatedRobots[9] = rotatedRobots[4].Clone(); // XYX
            rotatedRobots[9].RotateY(90);
            rotatedRobots[9].RecalcMinMaxSum();

            rotatedRobots[10] = rotatedRobots[4].Clone(); // XYY
            rotatedRobots[10].RotateY(90);
            rotatedRobots[10].RecalcMinMaxSum();

            rotatedRobots[11] = rotatedRobots[5].Clone(); // YXX
            rotatedRobots[11].RotateX(90);
            rotatedRobots[11].RecalcMinMaxSum();

            rotatedRobots[12] = rotatedRobots[6].Clone(); // YYX
            rotatedRobots[12].RotateX(90);
            rotatedRobots[12].RecalcMinMaxSum();

            rotatedRobots[13] = rotatedRobots[6].Clone(); // YYY
            rotatedRobots[13].RotateY(90);
            rotatedRobots[13].RecalcMinMaxSum();

            rotatedRobots[14] = rotatedRobots[7].Clone(); // XXXY
            rotatedRobots[14].RotateY(90);
            rotatedRobots[14].RecalcMinMaxSum();

            rotatedRobots[15] = rotatedRobots[8].Clone(); // XXYX
            rotatedRobots[15].RotateX(90);
            rotatedRobots[15].RecalcMinMaxSum();

            rotatedRobots[16] = rotatedRobots[8].Clone(); // XXYY
            rotatedRobots[16].RotateY(90);
            rotatedRobots[16].RecalcMinMaxSum();

            rotatedRobots[17] = rotatedRobots[9].Clone(); // XYXX
            rotatedRobots[17].RotateX(90);
            rotatedRobots[17].RecalcMinMaxSum();

            rotatedRobots[18] = rotatedRobots[10].Clone(); // XYYY
            rotatedRobots[18].RotateY(90);
            rotatedRobots[18].RecalcMinMaxSum();

            rotatedRobots[19] = rotatedRobots[11].Clone(); // YXXX
            rotatedRobots[19].RotateX(90);
            rotatedRobots[19].RecalcMinMaxSum();

            rotatedRobots[20] = rotatedRobots[13].Clone(); // YYYX
            rotatedRobots[20].RotateX(90);
            rotatedRobots[20].RecalcMinMaxSum();

            rotatedRobots[21] = rotatedRobots[14].Clone(); // XXXYX
            rotatedRobots[21].RotateX(90);
            rotatedRobots[21].RecalcMinMaxSum();

            rotatedRobots[22] = rotatedRobots[17].Clone(); // XYXXX
            rotatedRobots[22].RotateX(90);
            rotatedRobots[22].RecalcMinMaxSum();

            rotatedRobots[23] = rotatedRobots[18].Clone(); // XYYYX
            rotatedRobots[23].RotateX(90);
            rotatedRobots[23].RecalcMinMaxSum();

            return rotatedRobots;
        }

        public static int[] getRotationOffset(int value)
        {
            int[] offset = new int[3]; // Rotation in X - Y - Z
            switch (value)
            {
                case 0:
                    offset[0] = 0;
                    offset[1] = 0;
                    offset[2] = 0;
                    break;
                case 1:
                    offset[0] = 0;
                    offset[1] = 0;
                    offset[2] = 270;
                    break;
                case 2:
                    offset[0] = 0;
                    offset[1] = 0;
                    offset[2] = 180;
                    break;
                case 3:
                    offset[0] = 0;
                    offset[1] = 0;
                    offset[2] = 90;
                    break;
                case 4:
                    offset[0] = 270;
                    offset[1] = 0;
                    offset[2] = 90;
                    break;
                case 5:
                    offset[0] = 180;
                    offset[1] = 0;
                    offset[2] = 90;
                    break;
                case 6:
                    offset[0] = 90;
                    offset[1] = 0;
                    offset[2] = 90;
                    break;
                case 7:
                    offset[0] = 0;
                    offset[1] = 90;
                    offset[2] = 0;
                    break;
                case 8:
                    offset[0] = 0;
                    offset[1] = 90;
                    offset[2] = 90;
                    break;
                case 9:
                    offset[0] = 0;
                    offset[1] = 90;
                    offset[2] = 180;
                    break;
                case 10:
                    offset[0] = 0;
                    offset[1] = 90;
                    offset[2] = 270;
                    break;
                case 11:
                    offset[0] = 90;
                    offset[1] = 0;
                    offset[2] = 180;
                    break;
                case 12:
                    offset[0] = 0;
                    offset[1] = 270;
                    offset[2] = 90;
                    break;
                case 13:
                    offset[0] = 270;
                    offset[1] = 0;
                    offset[2] = 0;
                    break;
                case 14:
                    offset[0] = 270;
                    offset[1] = 0;
                    offset[2] = 270;
                    break;
                case 15:
                    offset[0] = 0;
                    offset[1] = 270;
                    offset[2] = 0;
                    break;
                case 16:
                    offset[0] = 180;
                    offset[1] = 0;
                    offset[2] = 0;
                    break;
                case 17:
                    offset[0] = 90;
                    offset[1] = 0;
                    offset[2] = 270;
                    break;
                case 18:
                    offset[0] = 0;
                    offset[1] = 270;
                    offset[2] = 180;
                    break;
                case 19:
                    offset[0] = 0;
                    offset[1] = 270;
                    offset[2] = 270;
                    break;
                case 20:
                    offset[0] = 90;
                    offset[1] = 0;
                    offset[2] = 0;
                    break;
                case 21:
                    offset[0] = 270;
                    offset[1] = 0;
                    offset[2] = 180;
                    break;
                case 22:
                    offset[0] = 180;
                    offset[1] = 0;
                    offset[2] = 270;
                    break;
                case 23:
                    offset[0] = 180;
                    offset[1] = 0;
                    offset[2] = 180;
                    break;
            }

            return offset;
        }

        public static int[] allMovementsOnLevelinZ(int level)
        {
            List<double> iList = new List<double>();
            List<double> iList2 = new List<double>();

            var sum = 0.0;
            for (int i = 1; i <= Math.Pow(2, level); i++)
            {
                var term = 0.0;
                for (int j = 1; j <= level; j++)
                {
                    var sign = 0.0;

                    if ((i & (int)Math.Pow(2, j - 1)) > 0)
                        sign = 1;
                    else
                        sign = -1;

                    #region relevant for paper

                    // Mathematical expression for bitwise and operator
                    /*
                    for (int k = 0; k <= Math.Floor(Math.Log(i, 2)); k++)
                    {
                        intermediateTerm += Math.Pow(2, k) * (Math.Floor(i / Math.Pow(2, k)) % 2) * (Math.Floor(Math.Pow(2, j - 1) / Math.Pow(2, k)) % 2);
                        //if ((Math.Pow(2, k) * (Math.Floor(i / Math.Pow(2, k)) % 2) * (Math.Floor(Math.Pow(2, j - 1) / Math.Pow(2, k)) % 2)) > 0)
                    }
                    if (intermediateTerm > 0.001)
                        intermediateTerm = 1;
                    else
                        intermediateTerm = -1;
                    */

                    #endregion

                    term = Math.Pow(8, j) / 2 * Math.Pow(0, i % Math.Pow(2, j - 1)) * sign;
                    iList2.Add(term);
                    sum += term;
                }

                iList.Add(sum);
            }

            return null;
        }

        public static int[] allMovementsOnLevelinX(int level)
        {
            List<double> iList = new List<double>();
            List<double> iList2 = new List<double>();

            var sum = 0.0;
            for (int i = 1; i <= Math.Pow(2, level); i++)
            {
                var term = 0.0;
                for (int j = 1; j <= level; j++)
                {
                    var sign = 0.0;

                    if ((i & (int)Math.Pow(2, j - 1)) > 0)
                        sign = 1;
                    else
                        sign = -1;

                    #region relevant for paper

                    // Mathematical expression for bitwise and operator
                    /*
                    for (int k = 0; k <= Math.Floor(Math.Log(i, 2)); k++)
                    {
                        intermediateTerm += Math.Pow(2, k) * (Math.Floor(i / Math.Pow(2, k)) % 2) * (Math.Floor(Math.Pow(2, j - 1) / Math.Pow(2, k)) % 2);
                        //if ((Math.Pow(2, k) * (Math.Floor(i / Math.Pow(2, k)) % 2) * (Math.Floor(Math.Pow(2, j - 1) / Math.Pow(2, k)) % 2)) > 0)
                    }
                    if (intermediateTerm > 0.001)
                        intermediateTerm = 1;
                    else
                        intermediateTerm = -1;
                    */

                    #endregion

                    term = Math.Pow(8, j) / 4 * Math.Pow(0, i % Math.Pow(2, j - 1)) * sign;
                    iList2.Add(term);
                    sum -= term;
                }

                iList.Add(sum);
            }

            return null;
        }

        public static int[] allMovementsOnLevelinY(int level)
        {
            List<double> iList = new List<double>();
            List<double> iList2 = new List<double>();

            var sum = 0.0;
            for (int i = 1; i <= Math.Pow(2, level); i++)
            {
                var term = 0.0;
                for (int j = 1; j <= level; j++)
                {
                    var sign = 0.0;

                    if ((i & (int)Math.Pow(2, j - 1)) > 0)
                        sign = 1;
                    else
                        sign = -1;

                    #region relevant for paper

                    // Mathematical expression for bitwise and operator
                    /*
                    for (int k = 0; k <= Math.Floor(Math.Log(i, 2)); k++)
                    {
                        intermediateTerm += Math.Pow(2, k) * (Math.Floor(i / Math.Pow(2, k)) % 2) * (Math.Floor(Math.Pow(2, j - 1) / Math.Pow(2, k)) % 2);
                        //if ((Math.Pow(2, k) * (Math.Floor(i / Math.Pow(2, k)) % 2) * (Math.Floor(Math.Pow(2, j - 1) / Math.Pow(2, k)) % 2)) > 0)
                    }
                    if (intermediateTerm > 0.001)
                        intermediateTerm = 1;
                    else
                        intermediateTerm = -1;
                    */

                    #endregion

                    term = Math.Pow(8, j) / 8 * Math.Pow(0, i % Math.Pow(2, j - 1)) * sign;
                    iList2.Add(term);
                    sum += term;
                }

                iList.Add(sum);
            }

            return null;
        }

        public static int? moveOneInPXFromRefOnLevel(int level, int nodeIndexOnLevel)
        {
            var i = 0;
            for (int h = level; h > 0; h--) // find out where the current voxel is located
            {
                if ((nodeIndexOnLevel & (int)(Math.Pow(8, h) / 4)) > 0)
                    i += (int)Math.Pow(2, h - 1);
            }

            if (i == 0) // limit of octree reached
                return null;

            var sum = 0;

            var term = 0.0;
            for (int j = 1; j <= level; j++)
            {
                var sign = 0.0;

                if ((i & (int)Math.Pow(2, j - 1)) > 0)
                    sign = 1;
                else
                    sign = -1;

                #region relevant for paper

                // Mathematical expression for bitwise and operator
                /*
                for (int k = 0; k <= Math.Floor(Math.Log(i, 2)); k++)
                {
                    intermediateTerm += Math.Pow(2, k) * (Math.Floor(i / Math.Pow(2, k)) % 2) * (Math.Floor(Math.Pow(2, j - 1) / Math.Pow(2, k)) % 2);
                    //if ((Math.Pow(2, k) * (Math.Floor(i / Math.Pow(2, k)) % 2) * (Math.Floor(Math.Pow(2, j - 1) / Math.Pow(2, k)) % 2)) > 0)
                }
                if (intermediateTerm > 0.001)
                    intermediateTerm = 1;
                else
                    intermediateTerm = -1;
                */

                #endregion

                term = Math.Pow(8, j) / 4 * Math.Pow(0, i % Math.Pow(2, j - 1)) * sign;
                sum += (int)term;
            }

            return -sum;
        }

        public static int? moveOneInPYFromRefOnLevel(int level, int nodeIndexOnLevel)
        {
            var i = 1;
            for (int h = level; h > 0; h--) // find out where the current voxel is located
            {
                if ((nodeIndexOnLevel & (int)(Math.Pow(8, h) / 8)) > 0)
                    i += (int)Math.Pow(2, h - 1);
            }

            if (i == Math.Pow(2, level)) // limit of octree reached          
                return null;

            var sum = 0;

            var term = 0.0;
            for (int j = 1; j <= level; j++)
            {
                var sign = 0.0;

                if ((i & (int)Math.Pow(2, j - 1)) > 0)
                    sign = 1;
                else
                    sign = -1;

                #region relevant for paper

                // Mathematical expression for bitwise and operator
                /*
                for (int k = 0; k <= Math.Floor(Math.Log(i, 2)); k++)
                {
                    intermediateTerm += Math.Pow(2, k) * (Math.Floor(i / Math.Pow(2, k)) % 2) * (Math.Floor(Math.Pow(2, j - 1) / Math.Pow(2, k)) % 2);
                    //if ((Math.Pow(2, k) * (Math.Floor(i / Math.Pow(2, k)) % 2) * (Math.Floor(Math.Pow(2, j - 1) / Math.Pow(2, k)) % 2)) > 0)
                }
                if (intermediateTerm > 0.001)
                    intermediateTerm = 1;
                else
                    intermediateTerm = -1;
                */

                #endregion

                term = Math.Pow(8, j) / 8 * Math.Pow(0, i % Math.Pow(2, j - 1)) * sign;
                sum += (int)term;
            }

            return sum;
        }

        public static int? moveOneInPZFromRefOnLevel(int level, int nodeIndexOnLevel)
        {
            var i = 1;
            for (int h = level; h > 0; h--) // find out where the current voxel is located
            {
                if ((nodeIndexOnLevel & (int)(Math.Pow(8, h) / 2)) > 0)
                    i += (int)Math.Pow(2, h - 1);
            }

            if (i == Math.Pow(2, level)) // limit of octree reached
                return null;

            var sum = 0;

            var term = 0.0;
            for (int j = 1; j <= level; j++)
            {
                var sign = 0.0;

                if ((i & (int)Math.Pow(2, j - 1)) > 0)
                    sign = 1;
                else
                    sign = -1;

                #region relevant for paper

                // Mathematical expression for bitwise and operator
                /*
                for (int k = 0; k <= Math.Floor(Math.Log(i, 2)); k++)
                {
                    intermediateTerm += Math.Pow(2, k) * (Math.Floor(i / Math.Pow(2, k)) % 2) * (Math.Floor(Math.Pow(2, j - 1) / Math.Pow(2, k)) % 2);
                    //if ((Math.Pow(2, k) * (Math.Floor(i / Math.Pow(2, k)) % 2) * (Math.Floor(Math.Pow(2, j - 1) / Math.Pow(2, k)) % 2)) > 0)
                }
                if (intermediateTerm > 0.001)
                    intermediateTerm = 1;
                else
                    intermediateTerm = -1;
                */

                #endregion

                term = Math.Pow(8, j) / 2 * Math.Pow(0, i % Math.Pow(2, j - 1)) * sign;
                sum += (int)term;
            }

            return sum;
        }

        public static int? moveOneInMXFromRefOnLevel(int level, int nodeIndexOnLevel)
        {
            var i = 1;
            for (int h = level; h > 0; h--) // find out where the current voxel is located
            {
                if ((nodeIndexOnLevel & (int)(Math.Pow(8, h) / 4)) > 0)
                    i += (int)Math.Pow(2, h - 1);
            }

            if (i == Math.Pow(2, level)) // limit of octree reached
                return null;

            var sum = 0;

            var term = 0.0;
            for (int j = 1; j <= level; j++)
            {
                var sign = 0.0;

                if ((i & (int)Math.Pow(2, j - 1)) > 0)
                    sign = 1;
                else
                    sign = -1;

                #region relevant for paper

                // Mathematical expression for bitwise and operator
                /*
                for (int k = 0; k <= Math.Floor(Math.Log(i, 2)); k++)
                {
                    intermediateTerm += Math.Pow(2, k) * (Math.Floor(i / Math.Pow(2, k)) % 2) * (Math.Floor(Math.Pow(2, j - 1) / Math.Pow(2, k)) % 2);
                    //if ((Math.Pow(2, k) * (Math.Floor(i / Math.Pow(2, k)) % 2) * (Math.Floor(Math.Pow(2, j - 1) / Math.Pow(2, k)) % 2)) > 0)
                }
                if (intermediateTerm > 0.001)
                    intermediateTerm = 1;
                else
                    intermediateTerm = -1;
                */

                #endregion

                term = Math.Pow(8, j) / 4 * Math.Pow(0, i % Math.Pow(2, j - 1)) * sign;
                sum += (int)term;
            }

            return sum;
        }

        public static int? moveOneInMYFromRefOnLevel(int level, int nodeIndexOnLevel)
        {
            var i = 0;
            for (int h = level; h > 0; h--) // find out where the current voxel is located
            {
                if ((nodeIndexOnLevel & (int)(Math.Pow(8, h) / 8)) > 0)
                    i += (int)Math.Pow(2, h - 1);
            }

            if (i == 0) // limit of octree reached
                return null;

            var sum = 0;

            var term = 0.0;
            for (int j = 1; j <= level; j++)
            {
                var sign = 0.0;

                if ((i & (int)Math.Pow(2, j - 1)) > 0)
                    sign = 1;
                else
                    sign = -1;

                #region relevant for paper

                // Mathematical expression for bitwise and operator
                /*
                for (int k = 0; k <= Math.Floor(Math.Log(i, 2)); k++)
                {
                    intermediateTerm += Math.Pow(2, k) * (Math.Floor(i / Math.Pow(2, k)) % 2) * (Math.Floor(Math.Pow(2, j - 1) / Math.Pow(2, k)) % 2);
                    //if ((Math.Pow(2, k) * (Math.Floor(i / Math.Pow(2, k)) % 2) * (Math.Floor(Math.Pow(2, j - 1) / Math.Pow(2, k)) % 2)) > 0)
                }
                if (intermediateTerm > 0.001)
                    intermediateTerm = 1;
                else
                    intermediateTerm = -1;
                */

                #endregion

                term = Math.Pow(8, j) / 8 * Math.Pow(0, i % Math.Pow(2, j - 1)) * sign;
                sum += (int)term;
            }

            return -sum;
        }

        public static int? moveOneInMZFromRefOnLevel(int level, int nodeIndexOnLevel)
        {
            var i = 0;
            for (int h = level; h > 0; h--) // find out where the current voxel is located
            {
                if ((nodeIndexOnLevel & (int)(Math.Pow(8, h) / 2)) > 0)
                    i += (int)Math.Pow(2, h - 1);
            }

            if (i == 0) // limit of octree reached
                return null;

            var sum = 0;

            var term = 0.0;
            for (int j = 1; j <= level; j++)
            {
                var sign = 0.0;

                if ((i & (int)Math.Pow(2, j - 1)) > 0)
                    sign = 1;
                else
                    sign = -1;

                #region relevant for paper

                // Mathematical expression for bitwise and operator
                /*
                for (int k = 0; k <= Math.Floor(Math.Log(i, 2)); k++)
                {
                    intermediateTerm += Math.Pow(2, k) * (Math.Floor(i / Math.Pow(2, k)) % 2) * (Math.Floor(Math.Pow(2, j - 1) / Math.Pow(2, k)) % 2);
                    //if ((Math.Pow(2, k) * (Math.Floor(i / Math.Pow(2, k)) % 2) * (Math.Floor(Math.Pow(2, j - 1) / Math.Pow(2, k)) % 2)) > 0)
                }
                if (intermediateTerm > 0.001)
                    intermediateTerm = 1;
                else
                    intermediateTerm = -1;
                */

                #endregion

                term = Math.Pow(8, j) / 2 * Math.Pow(0, i % Math.Pow(2, j - 1)) * sign;
                sum += (int)term;
            }

            return -sum;
        }

        public static int moveOneFromRefOnLevel(int level, int nodeIndexOnLevel, Directions direction, PosNeg posNeg) // FEHLER!
        {
            var start = 1;
            if (posNeg == PosNeg.Neg)
                start = 0;

            var rest = nodeIndexOnLevel;
            for (int h = level; h > 0; h--)
            {
                start += (int)(Math.Pow(2, h - 1) * (rest / (Math.Pow(8, h) / (int)direction)));
                rest = rest % (int)(Math.Pow(8, h) / (int)direction);
            }

            var term = 0;
            for (int j = 1; j <= level; j++)
            {
                var sign = 0;

                if ((start & (int)Math.Pow(2, j - 1)) > 0)
                    sign = 1;
                else
                    sign = -1;

                term += (int)(Math.Pow(8, j) / (int)direction) * (int)Math.Pow(0, start % Math.Pow(2, j - 1)) * sign;
            }
            /*
            if (term < 0)
                return 0;

            
            if((nodeIndexOnLevel + ((int)posNeg * term)) < 0 )
                return 0;*/

            return (int)posNeg * term;
        }

        public static int?[] getDedicatedNeighborMovements(int level, int nodeCount, IEnumerable<KeyValuePair<int, VoxelNode>> nodeGroup, int neighborDirection)
        {
            int?[] moveConversion = new int?[nodeCount];

            var i = 0;
            foreach (var node in nodeGroup)
            {
                moveConversion[i] = getAllSurroundingVoxel(level, node.Key)[neighborDirection];
                i++;
            }

            return moveConversion;
        }

        public static int?[][] getAllNeighborMovements(int level, int nodeCount, IEnumerable<KeyValuePair<int, VoxelNode>> nodeGroup)
        {
            int?[][] moveConversion = new int?[nodeCount][];

            var i = 0;
            foreach (var node in nodeGroup)
            {
                moveConversion[i] = getAllSurroundingVoxel(level, node.Key);
                i++;
            }

            return moveConversion;
        }

        public static int?[] getAllSurroundingVoxel(int level, int nodeIndexOnLevel)
        {
            int?[] surroundingVoxel = new int?[27];

            int? offsetTemp = nodeIndexOnLevel;

            surroundingVoxel[0] = nodeIndexOnLevel;
            surroundingVoxel[1] = (offsetTemp = moveOneInPXFromRefOnLevel(level, nodeIndexOnLevel)) != null ? nodeIndexOnLevel + offsetTemp : offsetTemp; //+X
            surroundingVoxel[2] = (offsetTemp = moveOneInPYFromRefOnLevel(level, nodeIndexOnLevel)) != null ? nodeIndexOnLevel + offsetTemp : offsetTemp; //+Y
            surroundingVoxel[3] = (offsetTemp = moveOneInPZFromRefOnLevel(level, nodeIndexOnLevel)) != null ? nodeIndexOnLevel + offsetTemp : offsetTemp; //+Z
            surroundingVoxel[4] = (offsetTemp = moveOneInMXFromRefOnLevel(level, nodeIndexOnLevel)) != null ? nodeIndexOnLevel + offsetTemp : offsetTemp; //-X
            surroundingVoxel[5] = (offsetTemp = moveOneInMYFromRefOnLevel(level, nodeIndexOnLevel)) != null ? nodeIndexOnLevel + offsetTemp : offsetTemp; //-Y
            surroundingVoxel[6] = (offsetTemp = moveOneInMZFromRefOnLevel(level, nodeIndexOnLevel)) != null ? nodeIndexOnLevel + offsetTemp : offsetTemp; //-Z

            if (surroundingVoxel[1] == null)
            {
                surroundingVoxel[7] = null;
                surroundingVoxel[8] = null;
                surroundingVoxel[9] = null;
                surroundingVoxel[10] = null;
            }
            else
            {
                surroundingVoxel[7] = (offsetTemp = moveOneInPYFromRefOnLevel(level, nodeIndexOnLevel)) != null
                                          ? surroundingVoxel[1] + offsetTemp
                                          : offsetTemp; //+X +Y
                surroundingVoxel[8] = (offsetTemp = moveOneInMYFromRefOnLevel(level, nodeIndexOnLevel)) != null
                                          ? surroundingVoxel[1] + offsetTemp
                                          : offsetTemp; //+X -Y
                surroundingVoxel[9] = (offsetTemp = moveOneInPZFromRefOnLevel(level, nodeIndexOnLevel)) != null
                                          ? surroundingVoxel[1] + offsetTemp
                                          : offsetTemp; //+X +Z
                surroundingVoxel[10] = (offsetTemp = moveOneInMZFromRefOnLevel(level, nodeIndexOnLevel)) != null
                                           ? surroundingVoxel[1] + offsetTemp
                                           : offsetTemp; //+X -Z
            }

            if (surroundingVoxel[4] == null)
            {
                surroundingVoxel[11] = null;
                surroundingVoxel[12] = null;
                surroundingVoxel[13] = null;
                surroundingVoxel[14] = null;
            }
            else
            {
                surroundingVoxel[11] = (offsetTemp = moveOneInPYFromRefOnLevel(level, nodeIndexOnLevel)) != null
                                           ? surroundingVoxel[4] + offsetTemp
                                           : offsetTemp; //-X +Y
                surroundingVoxel[12] = (offsetTemp = moveOneInMYFromRefOnLevel(level, nodeIndexOnLevel)) != null
                                           ? surroundingVoxel[4] + offsetTemp
                                           : offsetTemp; //-X -Y
                surroundingVoxel[13] = (offsetTemp = moveOneInPZFromRefOnLevel(level, nodeIndexOnLevel)) != null
                                           ? surroundingVoxel[4] + offsetTemp
                                           : offsetTemp; //-X +Z
                surroundingVoxel[14] = (offsetTemp = moveOneInMZFromRefOnLevel(level, nodeIndexOnLevel)) != null
                                           ? surroundingVoxel[4] + offsetTemp
                                           : offsetTemp; //-X -Z
            }

            if (surroundingVoxel[2] == null)
            {
                surroundingVoxel[15] = null;
                surroundingVoxel[16] = null;
            }
            else
            {
                surroundingVoxel[15] = (offsetTemp = moveOneInPZFromRefOnLevel(level, nodeIndexOnLevel)) != null
                                           ? surroundingVoxel[2] + offsetTemp
                                           : offsetTemp; //+Y +Z
                surroundingVoxel[16] = (offsetTemp = moveOneInMZFromRefOnLevel(level, nodeIndexOnLevel)) != null
                                           ? surroundingVoxel[2] + offsetTemp
                                           : offsetTemp; //+Y -Z
            }

            if (surroundingVoxel[5] == null)
            {
                surroundingVoxel[17] = null;
                surroundingVoxel[18] = null;
            }
            else
            {
                surroundingVoxel[17] = (offsetTemp = moveOneInPZFromRefOnLevel(level, nodeIndexOnLevel)) != null
                                           ? surroundingVoxel[5] + offsetTemp
                                           : offsetTemp; //-Y +Z
                surroundingVoxel[18] = (offsetTemp = moveOneInMZFromRefOnLevel(level, nodeIndexOnLevel)) != null
                                           ? surroundingVoxel[5] + offsetTemp
                                           : offsetTemp; //-Y -Z
            }

            if (surroundingVoxel[7] == null)
            {
                surroundingVoxel[19] = null;
                surroundingVoxel[20] = null;
            }
            else
            {
                surroundingVoxel[19] = (offsetTemp = moveOneInPZFromRefOnLevel(level, nodeIndexOnLevel)) != null
                                           ? surroundingVoxel[7] + offsetTemp
                                           : offsetTemp; //+X +Y +Z
                surroundingVoxel[20] = (offsetTemp = moveOneInMZFromRefOnLevel(level, nodeIndexOnLevel)) != null
                                           ? surroundingVoxel[7] + offsetTemp
                                           : offsetTemp; //+X +Y -Z
            }

            if (surroundingVoxel[8] == null)
            {
                surroundingVoxel[21] = null;
                surroundingVoxel[22] = null;
            }
            else
            {
                surroundingVoxel[21] = (offsetTemp = moveOneInPZFromRefOnLevel(level, nodeIndexOnLevel)) != null
                                           ? surroundingVoxel[8] + offsetTemp
                                           : offsetTemp; //+X -Y +Z
                surroundingVoxel[22] = (offsetTemp = moveOneInMZFromRefOnLevel(level, nodeIndexOnLevel)) != null
                                           ? surroundingVoxel[8] + offsetTemp
                                           : offsetTemp; //+X -Y -Z
            }

            if (surroundingVoxel[11] == null)
            {
                surroundingVoxel[23] = null;
                surroundingVoxel[24] = null;
            }
            else
            {
                surroundingVoxel[23] = (offsetTemp = moveOneInPZFromRefOnLevel(level, nodeIndexOnLevel)) != null
                                           ? surroundingVoxel[11] + offsetTemp
                                           : offsetTemp; //-X +Y +Z
                surroundingVoxel[24] = (offsetTemp = moveOneInMZFromRefOnLevel(level, nodeIndexOnLevel)) != null
                                           ? surroundingVoxel[11] + offsetTemp
                                           : offsetTemp; //-X +Y -Z
            }

            if (surroundingVoxel[12] == null)
            {
                surroundingVoxel[25] = null;
                surroundingVoxel[26] = null;
            }
            else
            {
                surroundingVoxel[25] = (offsetTemp = moveOneInPZFromRefOnLevel(level, nodeIndexOnLevel)) != null
                                           ? surroundingVoxel[12] + offsetTemp
                                           : offsetTemp; //-X -Y +Z
                surroundingVoxel[26] = (offsetTemp = moveOneInMZFromRefOnLevel(level, nodeIndexOnLevel)) != null
                                           ? surroundingVoxel[12] + offsetTemp
                                           : offsetTemp; //-X -Y -Z
            }

            return surroundingVoxel;
        }

        #endregion

        /*
        public static int[,] rotateConversion = new int[24, 8]{
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
        */
    }
}