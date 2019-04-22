using RobotEditor.Model;
using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace RobotEditor.Helper
{
    public static class MatrixHelper
    {
        #region Public methods

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
            matrix.M23 = -(Math.Sin(radians));
            matrix.M33 = Math.Cos(radians);
            return matrix;
        }

        public static Matrix3D NewMatrixRotateAroundY(double degree)
        {
            var radians = GetValue(degree);
            var matrix = Matrix3D.Identity;
            matrix.M11 = Math.Cos(radians);
            matrix.M31 = -(Math.Sin(radians));
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
            matrix.M12 = -(Math.Sin(radians));
            matrix.M22 = Math.Cos(radians);
            return matrix;
        }

        public static void allRotationsOfCube(VoxelOctree cubeToRotate)
        {
            VoxelOctree[] rotatedRobots = new VoxelOctree[24];
            rotatedRobots[0] = cubeToRotate;            // Identity
            rotatedRobots[1] = rotatedRobots[0];        // X
            rotatedRobots[1].RotateX(90);
            rotatedRobots[1].RecalcMinMaxSum();
            rotatedRobots[2] = rotatedRobots[0];        // Y
            rotatedRobots[2].RotateY(90);
            rotatedRobots[2].RecalcMinMaxSum();
            rotatedRobots[3] = rotatedRobots[1];        // XX
            rotatedRobots[3].RotateX(90);
            rotatedRobots[3].RecalcMinMaxSum();
            rotatedRobots[4] = rotatedRobots[1];        // XY
            rotatedRobots[4].RotateY(90);
            rotatedRobots[4].RecalcMinMaxSum();
            rotatedRobots[5] = rotatedRobots[2];        // YX
            rotatedRobots[5].RotateX(90);
            rotatedRobots[5].RecalcMinMaxSum();
            rotatedRobots[6] = rotatedRobots[2];        // YY
            rotatedRobots[6].RotateY(90);
            rotatedRobots[6].RecalcMinMaxSum();
            rotatedRobots[7] = rotatedRobots[3];        // XXX
            rotatedRobots[7].RotateX(90);
            rotatedRobots[7].RecalcMinMaxSum();
            rotatedRobots[8] = rotatedRobots[3];        // XXY
            rotatedRobots[8].RotateY(90);
            rotatedRobots[8].RecalcMinMaxSum();
            rotatedRobots[9] = rotatedRobots[4];        // XYX
            rotatedRobots[9].RotateY(90);
            rotatedRobots[9].RecalcMinMaxSum();
            rotatedRobots[10] = rotatedRobots[4];        // XYY
            rotatedRobots[10].RotateY(90);
            rotatedRobots[10].RecalcMinMaxSum();
            rotatedRobots[11] = rotatedRobots[5];        // YXX
            rotatedRobots[11].RotateX(90);
            rotatedRobots[11].RecalcMinMaxSum();
            rotatedRobots[12] = rotatedRobots[6];        // YYX
            rotatedRobots[12].RotateX(90);
            rotatedRobots[12].RecalcMinMaxSum();
            rotatedRobots[13] = rotatedRobots[6];        // YYY
            rotatedRobots[13].RotateY(90);
            rotatedRobots[13].RecalcMinMaxSum();
            rotatedRobots[14] = rotatedRobots[7];        // XXXY
            rotatedRobots[14].RotateY(90);
            rotatedRobots[14].RecalcMinMaxSum();
            rotatedRobots[15] = rotatedRobots[8];        // XXYX
            rotatedRobots[15].RotateX(90);
            rotatedRobots[15].RecalcMinMaxSum();
            rotatedRobots[16] = rotatedRobots[8];        // XXYY
            rotatedRobots[16].RotateY(90);
            rotatedRobots[16].RecalcMinMaxSum();
            rotatedRobots[17] = rotatedRobots[9];        // XYXX
            rotatedRobots[17].RotateX(90);
            rotatedRobots[17].RecalcMinMaxSum();
            rotatedRobots[18] = rotatedRobots[10];        // XYYY
            rotatedRobots[18].RotateY(90);
            rotatedRobots[18].RecalcMinMaxSum();
            rotatedRobots[19] = rotatedRobots[11];        // YXXX
            rotatedRobots[19].RotateX(90);
            rotatedRobots[19].RecalcMinMaxSum();
            rotatedRobots[20] = rotatedRobots[13];        // YYYX
            rotatedRobots[20].RotateX(90);
            rotatedRobots[20].RecalcMinMaxSum();
            rotatedRobots[21] = rotatedRobots[14];        // XXXYX
            rotatedRobots[21].RotateX(90);
            rotatedRobots[21].RecalcMinMaxSum();
            rotatedRobots[22] = rotatedRobots[17];        // XYXXX
            rotatedRobots[22].RotateX(90);
            rotatedRobots[22].RecalcMinMaxSum();
            rotatedRobots[23] = rotatedRobots[18];        // XYYYX
            rotatedRobots[23].RotateX(90);
            rotatedRobots[23].RecalcMinMaxSum();
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

                    term = (Math.Pow(8, j) / 2) * Math.Pow(0, (i % Math.Pow(2, j - 1))) * sign;
                    iList2.Add(term);
                    sum += term;
                }
                iList.Add(sum);
            }

            return null;
        }


        public static int moveOneFromRefOnLevel(int level, int nodeIndexOnLevel, Directions direction, PosNeg posNeg)
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

                term += (int)(Math.Pow(8, j) / (int)direction) * (int)Math.Pow(0, (start % Math.Pow(2, j - 1))) * sign;
            }

            if (term < 0)
                return 0;

            return ((int)posNeg * term);
        }


        public static int[,] getAllNeighborMovements(int level, int nodeIndexOnLevel)
        {
            int[,] moveConversion = new int[26, 8];

            for (var i = 0; i < 8; i++)
            {
                moveConversion[0, i] = nodeIndexOnLevel + moveOneFromRefOnLevel(level, nodeIndexOnLevel + i, Directions.X, PosNeg.Pos);
                moveConversion[1, i] = nodeIndexOnLevel + moveOneFromRefOnLevel(level, nodeIndexOnLevel + i, Directions.Y, PosNeg.Pos);
                moveConversion[2, i] = nodeIndexOnLevel + moveOneFromRefOnLevel(level, nodeIndexOnLevel + i, Directions.Z, PosNeg.Pos);
                moveConversion[3, i] = nodeIndexOnLevel + moveOneFromRefOnLevel(level, nodeIndexOnLevel + i, Directions.X, PosNeg.Neg);
                moveConversion[4, i] = nodeIndexOnLevel + moveOneFromRefOnLevel(level, nodeIndexOnLevel + i, Directions.Y, PosNeg.Neg);
                moveConversion[5, i] = nodeIndexOnLevel + moveOneFromRefOnLevel(level, nodeIndexOnLevel + i, Directions.Z, PosNeg.Neg);

                moveConversion[6, i] = nodeIndexOnLevel + moveOneFromRefOnLevel(level, moveOneFromRefOnLevel(level, nodeIndexOnLevel + i, Directions.X, PosNeg.Pos), Directions.Y, PosNeg.Pos);
                moveConversion[7, i] = nodeIndexOnLevel + moveOneFromRefOnLevel(level, moveOneFromRefOnLevel(level, nodeIndexOnLevel + i, Directions.X, PosNeg.Pos), Directions.Y, PosNeg.Neg);
                moveConversion[8, i] = nodeIndexOnLevel + moveOneFromRefOnLevel(level, moveOneFromRefOnLevel(level, nodeIndexOnLevel + i, Directions.X, PosNeg.Pos), Directions.Z, PosNeg.Pos);
                moveConversion[9, i] = nodeIndexOnLevel + moveOneFromRefOnLevel(level, moveOneFromRefOnLevel(level, nodeIndexOnLevel + i, Directions.X, PosNeg.Pos), Directions.Z, PosNeg.Neg);
                moveConversion[10, i] = nodeIndexOnLevel + moveOneFromRefOnLevel(level, moveOneFromRefOnLevel(level, nodeIndexOnLevel + i, Directions.X, PosNeg.Neg), Directions.Y, PosNeg.Pos);
                moveConversion[11, i] = nodeIndexOnLevel + moveOneFromRefOnLevel(level, moveOneFromRefOnLevel(level, nodeIndexOnLevel + i, Directions.X, PosNeg.Neg), Directions.Y, PosNeg.Neg);
                moveConversion[12, i] = nodeIndexOnLevel + moveOneFromRefOnLevel(level, moveOneFromRefOnLevel(level, nodeIndexOnLevel + i, Directions.X, PosNeg.Neg), Directions.Z, PosNeg.Pos);
                moveConversion[13, i] = nodeIndexOnLevel + moveOneFromRefOnLevel(level, moveOneFromRefOnLevel(level, nodeIndexOnLevel + i, Directions.X, PosNeg.Neg), Directions.Z, PosNeg.Neg);
                moveConversion[14, i] = nodeIndexOnLevel + moveOneFromRefOnLevel(level, moveOneFromRefOnLevel(level, nodeIndexOnLevel + i, Directions.Y, PosNeg.Pos), Directions.Z, PosNeg.Pos);
                moveConversion[15, i] = nodeIndexOnLevel + moveOneFromRefOnLevel(level, moveOneFromRefOnLevel(level, nodeIndexOnLevel + i, Directions.Y, PosNeg.Pos), Directions.Z, PosNeg.Neg);
                moveConversion[16, i] = nodeIndexOnLevel + moveOneFromRefOnLevel(level, moveOneFromRefOnLevel(level, nodeIndexOnLevel + i, Directions.Y, PosNeg.Neg), Directions.Z, PosNeg.Pos);
                moveConversion[17, i] = nodeIndexOnLevel + moveOneFromRefOnLevel(level, moveOneFromRefOnLevel(level, nodeIndexOnLevel + i, Directions.Y, PosNeg.Neg), Directions.Z, PosNeg.Neg);

                moveConversion[18, i] = nodeIndexOnLevel + moveOneFromRefOnLevel(level, moveOneFromRefOnLevel(level, moveOneFromRefOnLevel(level, nodeIndexOnLevel + i, Directions.X, PosNeg.Pos), Directions.Y, PosNeg.Pos), Directions.Z, PosNeg.Pos);
                moveConversion[19, i] = nodeIndexOnLevel + moveOneFromRefOnLevel(level, moveOneFromRefOnLevel(level, moveOneFromRefOnLevel(level, nodeIndexOnLevel + i, Directions.X, PosNeg.Pos), Directions.Y, PosNeg.Pos), Directions.Z, PosNeg.Neg);
                moveConversion[20, i] = nodeIndexOnLevel + moveOneFromRefOnLevel(level, moveOneFromRefOnLevel(level, moveOneFromRefOnLevel(level, nodeIndexOnLevel + i, Directions.X, PosNeg.Pos), Directions.Y, PosNeg.Neg), Directions.Z, PosNeg.Pos);
                moveConversion[21, i] = nodeIndexOnLevel + moveOneFromRefOnLevel(level, moveOneFromRefOnLevel(level, moveOneFromRefOnLevel(level, nodeIndexOnLevel + i, Directions.X, PosNeg.Pos), Directions.Y, PosNeg.Neg), Directions.Z, PosNeg.Neg);
                moveConversion[22, i] = nodeIndexOnLevel + moveOneFromRefOnLevel(level, moveOneFromRefOnLevel(level, moveOneFromRefOnLevel(level, nodeIndexOnLevel + i, Directions.X, PosNeg.Neg), Directions.Y, PosNeg.Pos), Directions.Z, PosNeg.Pos);
                moveConversion[23, i] = nodeIndexOnLevel + moveOneFromRefOnLevel(level, moveOneFromRefOnLevel(level, moveOneFromRefOnLevel(level, nodeIndexOnLevel + i, Directions.X, PosNeg.Neg), Directions.Y, PosNeg.Pos), Directions.Z, PosNeg.Neg);
                moveConversion[24, i] = nodeIndexOnLevel + moveOneFromRefOnLevel(level, moveOneFromRefOnLevel(level, moveOneFromRefOnLevel(level, nodeIndexOnLevel + i, Directions.X, PosNeg.Neg), Directions.Y, PosNeg.Neg), Directions.Z, PosNeg.Pos);
                moveConversion[25, i] = nodeIndexOnLevel + moveOneFromRefOnLevel(level, moveOneFromRefOnLevel(level, moveOneFromRefOnLevel(level, nodeIndexOnLevel + i, Directions.X, PosNeg.Neg), Directions.Y, PosNeg.Neg), Directions.Z, PosNeg.Neg);
            }

            return moveConversion;
        }


        public static int[] getAllSurroundingVoxel(int level, int startIndexOnLevel)
        {
            int[] surroundingVoxel = new int[64];

            //Lower left corner
            var startPos1 = moveOneFromRefOnLevel(level, moveOneFromRefOnLevel(level, moveOneFromRefOnLevel(level, startIndexOnLevel, Directions.Z, PosNeg.Neg), Directions.Y, PosNeg.Neg), Directions.X, PosNeg.Pos);

            var count = 0;
            for(var i = 0; i<4; i++) // Move along Z
            {
                var startPos2 = startPos1;
                for(var j=0; j<4; j++) // Move along Y
                {
                    var currentPos = startPos2;
                    for(var k=0; k<4; k++) // Move along X
                    {
                        surroundingVoxel[count] = currentPos;
                        currentPos = moveOneFromRefOnLevel(level, currentPos, Directions.X, PosNeg.Neg);
                        count++;
                    }
                    startPos2 = moveOneFromRefOnLevel(level, startPos2, Directions.Y, PosNeg.Pos);
                }
                startPos1 = moveOneFromRefOnLevel(level, startPos1, Directions.Z, PosNeg.Pos);
            }

            return surroundingVoxel;
        }

        #endregion
    }
}