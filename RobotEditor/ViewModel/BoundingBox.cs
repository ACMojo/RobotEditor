using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace RobotEditor.ViewModel
{
    class BoundingBox
    {

        #region Fields

        private static readonly int[] _defaultFaceList =
        {
            4,
            0,
            1,
            2,
            3,
            4,
            1,
            5,
            6,
            2,
            4,
            2,
            6,
            7,
            3,
            4,
            3,
            7,
            4,
            0,
            4,
            0,
            4,
            5,
            1,
            4,
            7,
            6,
            5,
            4
        };

        private readonly object _initLock = new object();

        private bool _initialized;
        private double _ii = double.NaN;
        private double _jj = double.NaN;
        private double _kk = double.NaN;
        private double _volume = double.NaN;
        private readonly Vector<float>[] _points;

        #endregion

        #region Instance

        public BoundingBox(BoundingBox box)
            : this()
        {
            _points = new Vector<float>[8];

            var length = box.Points.Length;
            for (var i = 0; i < length; i++)
            {
                var point = box.Points[i];
                _points[i] = Vector<float>.Build.Dense(new[] { point[0], point[1], point[2], 1 });
            }

            Min = box.Min.Clone();
            Max = box.Max.Clone();

            Volume = box.Volume;
        }

        public BoundingBox(Vector<float>[] pointsCuboid, Vector<float> min, Vector<float> max)
            : this()
        {
            _points = pointsCuboid;
            Min = min;
            Max = max;
        }

        public BoundingBox(Vector<float> min, Vector<float> max)
            : this()
        {
            Min = min;
            Max = max;

            _points = new[]
            {
                Vector<float>.Build.Dense(new[] { Min[0], Min[1], Min[2], 1 }),
                Vector<float>.Build.Dense(new[] { Min[0], Min[1], Max[2], 1 }),
                Vector<float>.Build.Dense(new[] { Max[0], Min[1], Max[2], 1 }),
                Vector<float>.Build.Dense(new[] { Max[0], Min[1], Min[2], 1 }),
                Vector<float>.Build.Dense(new[] { Min[0], Max[1], Min[2], 1 }),
                Vector<float>.Build.Dense(new[] { Min[0], Max[1], Max[2], 1 }),
                Vector<float>.Build.Dense(new[] { Max[0], Max[1], Max[2], 1 }),
                Vector<float>.Build.Dense(new[] { Max[0], Max[1], Min[2], 1 }),
            };
        }

        public BoundingBox(Vector<float>[] axisVectors, Vector<float> centerPoint, float[] extents)
            : this()
        {
            _points = new Vector<float>[8];

            var center = Vector<float>.Build.Dense(new[] { centerPoint[0], centerPoint[1], centerPoint[2] });

            var axis = new Vector<float>[3];
            axis[0] = Vector<float>.Build.Dense(new[] { axisVectors[0][0], axisVectors[0][1], axisVectors[0][2] });
            axis[1] = Vector<float>.Build.Dense(new[] { axisVectors[1][0], axisVectors[1][1], axisVectors[1][2] });
            axis[2] = Vector<float>.Build.Dense(new[] { axisVectors[2][0], axisVectors[2][1], axisVectors[2][2] });

            var points = new Vector<float>[8];
            points[0] = center + axis[0] * extents[0] * -1 + axis[1] * extents[1] * -1 + axis[2] * extents[2] * -1;
            points[1] = center + axis[0] * extents[0] * -1 + axis[1] * extents[1] * -1 + axis[2] * extents[2] * 1;
            points[2] = center + axis[0] * extents[0] * 1 + axis[1] * extents[1] * -1 + axis[2] * extents[2] * 1;
            points[3] = center + axis[0] * extents[0] * 1 + axis[1] * extents[1] * -1 + axis[2] * extents[2] * -1;
            points[4] = center + axis[0] * extents[0] * -1 + axis[1] * extents[1] * 1 + axis[2] * extents[2] * -1;
            points[5] = center + axis[0] * extents[0] * -1 + axis[1] * extents[1] * 1 + axis[2] * extents[2] * 1;
            points[6] = center + axis[0] * extents[0] * 1 + axis[1] * extents[1] * 1 + axis[2] * extents[2] * 1;
            points[7] = center + axis[0] * extents[0] * 1 + axis[1] * extents[1] * 1 + axis[2] * extents[2] * -1;

            for (var i = 0; i < points.Length; i++)
            {
                var point = points[i];
                point = Vector<float>.Build.Dense(new[] { point[0], point[1], point[2], 1 });
                _points[i] = point;

                if (Min == null || Max == null)
                {
                    Min = point.Clone();
                    Max = point.Clone();
                    continue;
                }

                if (Max[0] < point[0])
                    Max[0] = point[0];
                if (Max[1] < point[1])
                    Max[1] = point[1];
                if (Max[2] < point[2])
                    Max[2] = point[2];

                if (Min[0] > point[0])
                    Min[0] = point[0];
                if (Min[1] > point[1])
                    Min[1] = point[1];
                if (Min[2] > point[2])
                    Min[2] = point[2];
            }
        }

        private BoundingBox()
        {
            FaceList = _defaultFaceList;
        }

        #endregion

        #region Properties

        public Vector<float>[] Axis { get; private set; }

        public Vector<float>[] AxisUnit { get; private set; }

        public Vector<float> Origin { get; private set; }

        public Vector<float> Center { get; private set; }

        public Vector<float> Min { get; }

        public Vector<float> Max { get; }

        public Vector<float>[] Points => _points;

        public double[] Extents { get; private set; }

        public int[] FaceList { get; }

        public double Volume
        {
            get
            {
                if (double.IsNaN(_volume))
                    _volume = (Max[0] - Min[0]) * (Max[1] - Min[1]) * (Max[2] - Min[2]);

                return _volume;
            }
            private set { _volume = value; }
        }

        #endregion

        #region Public methods

        public void Init()
        {
            lock (_initLock)
            {
                if (_initialized)
                    return;

                var p1 = _points[0];
                var p2 = _points[1];
                var p3 = _points[2];
                var p4 = _points[3];
                var p5 = _points[4];

                Origin = p1;

                Axis = new Vector<float>[3];
                Axis[0] = p4 - p1;
                Axis[1] = p5 - p1;
                Axis[2] = p2 - p1;

                AxisUnit = new Vector<float>[3];
                AxisUnit[0] = Axis[0].Normalize(2);
                AxisUnit[1] = Axis[1].Normalize(2);
                AxisUnit[2] = Axis[2].Normalize(2);

                Center = p3 + (p5 - p3) * 0.5f;

                Extents = new double[3];
                Extents[0] = Axis[0].L2Norm() / 2;
                Extents[1] = Axis[1].L2Norm() / 2;
                Extents[2] = Axis[2].L2Norm() / 2;

                Volume = Axis[0].L2Norm() * Axis[1].L2Norm() * Axis[2].L2Norm();

                _ii = Axis[0] * Axis[0];
                _jj = Axis[1] * Axis[1];
                _kk = Axis[2] * Axis[2];

                _initialized = true;
            }
        }

        public bool IsPointInCube(Vector<float> p)
        {
            if (!_initialized)
                Init();

            var pv = p - Origin;

            var vi = pv * Axis[0];
            if (vi <= 0 || vi >= _ii)
                return false;

            var vj = pv * Axis[1];
            if (vj <= 0 || vj >= _jj)
                return false;

            var vk = pv * Axis[2];
            if (vk <= 0 || vk >= _kk)
                return false;

            return true;
        }

        public bool Colliding(BoundingBox bb)
        {
            var smin = Min;
            var smax = Max;
            var tmin = bb.Min;
            var tmax = bb.Max;

            if ((tmin[0] <= smin[0] && smin[0] <= tmax[0] ||
                 tmin[0] <= smax[0] && smax[0] <= tmax[0] ||
                 smin[0] <= tmin[0] && tmax[0] <= smax[0]) &&
                (tmin[1] <= smin[1] && smin[1] <= tmax[1] ||
                 tmin[1] <= smax[1] && smax[1] <= tmax[1] ||
                 smin[1] <= tmin[1] && tmax[1] <= smax[1]) &&
                (tmin[2] <= smin[2] && smin[2] <= tmax[2] ||
                 tmin[2] <= smax[2] && smax[2] <= tmax[2] ||
                 smin[2] <= tmin[2] && tmax[2] <= smax[2]))
                return CollidingFine(bb);

            return false;
        }

        public BoundingBox Union(BoundingBox bb)
        {
            var min = Vector<float>.Build.Dense(4);
            var max = Vector<float>.Build.Dense(4);

            for (var i = 0; i < 4; i++)
            {
                min[i] = Min[i];
                if (bb.Min[i] < min[i])
                    min[i] = bb.Min[i];
                max[i] = Max[i];
                if (bb.Max[i] > max[i])
                    max[i] = bb.Max[i];
            }

            return new BoundingBox(min, max);
        }

        public BoundingBox Transform(Matrix<float> matrix)
        {
            var length = _points.Length;

            Vector<float> min = null;
            Vector<float> max = null;
            var transformedPoints = new Vector<float>[length];
            for (var i = 0; i < length; i++)
            {
                var result = matrix.Multiply(_points[i]);
                result = result.Multiply(1 / result[3]);
                transformedPoints[i] = result;

                if (min == null || max == null)
                {
                    min = Vector<float>.Build.Dense(new[] { result[0], result[1], result[2], 1 });
                    max = Vector<float>.Build.Dense(new[] { result[0], result[1], result[2], 1 });
                    continue;
                }

                if (max[0] < result[0])
                    max[0] = result[0];
                if (max[1] < result[1])
                    max[1] = result[1];
                if (max[2] < result[2])
                    max[2] = result[2];

                if (min[0] > result[0])
                    min[0] = result[0];
                if (min[1] > result[1])
                    min[1] = result[1];
                if (min[2] > result[2])
                    min[2] = result[2];
            }

            return new BoundingBox(transformedPoints, min, max);
        }

        public BoundingBox Enlarge(double size)
        {
            if (!_initialized)
                Init();

            var extents = new float[3];
            for (var i = 0; i < 3; i++)
                extents[i] = (float)Extents[i] + (float)size;

            return new BoundingBox(AxisUnit, Center, extents);
        }

        #endregion

        #region Private methods

        private bool CollidingFine(BoundingBox bb)
        {
            if (!_initialized)
                Init();
            if (!bb._initialized)
                bb.Init();

            // If center of either bounding box is inside the other one they are colliding
            // This check is needed if one box is inside the other box without intersections
            if (IsPointInCube(bb.Center))
                return true;

            if (bb.IsPointInCube(Center))
                return true;

            // If there's at least one separating axis, they do not intersect
            // See https://www.geometrictools.com/Documentation/DynamicCollisionDetection.pdf
            return Intersecting(bb);
        }

        private bool Intersecting(BoundingBox bb)
        {
            var axisA = AxisUnit;
            var centerA = Center;
            var extentA = Extents;

            if (Math.Abs(axisA[0].L2Norm()) < 0.0000001 ||
                Math.Abs(axisA[1].L2Norm()) < 0.0000001 ||
                Math.Abs(axisA[2].L2Norm()) < 0.0000001)
                return false;

            var axisB = bb.AxisUnit;
            var centerB = bb.Center;
            var extentB = bb.Extents;

            if (Math.Abs(axisB[0].L2Norm()) < 0.0000001 ||
                Math.Abs(axisB[1].L2Norm()) < 0.0000001 ||
                Math.Abs(axisB[2].L2Norm()) < 0.0000001)
                return false;

            var d = centerB - centerA;

            var c = new float[3, 3];
            var cAbs = new float[3, 3];

            for (var j = 0; j < 3; j++)
            {
                var value = axisA[0] * axisB[j];
                c[0, j] = value;
                cAbs[0, j] = Math.Abs(value);
            }

            var axisADotD = new float[3];
            axisADotD[0] = axisA[0] * d;

            {
                var r0 = extentA[0];
                var r1 = extentB[0] * cAbs[0, 0] + extentB[1] * cAbs[0, 1] + extentB[2] * cAbs[0, 2];
                var r = Math.Abs(axisADotD[0]);

                var result = r0 + r1;
                if (r > result)
                    return false;
            }

            for (var j = 0; j < 3; j++)
            {
                var value = axisA[1] * axisB[j];
                c[1, j] = value;
                cAbs[1, j] = Math.Abs(value);
            }

            axisADotD[1] = axisA[1] * d;

            {
                var r0 = extentA[1];
                var r1 = extentB[0] * cAbs[1, 0] + extentB[1] * cAbs[1, 1] + extentB[2] * cAbs[1, 2];
                var r = Math.Abs(axisADotD[1]);

                var result = r0 + r1;
                if (r > result)
                    return false;
            }

            for (var j = 0; j < 3; j++)
            {
                var value = axisA[2] * axisB[j];
                c[2, j] = value;
                cAbs[2, j] = Math.Abs(value);
            }

            axisADotD[2] = axisA[2] * d;

            {
                var r0 = extentA[2];
                var r1 = extentB[0] * cAbs[2, 0] + extentB[1] * cAbs[2, 1] + extentB[2] * cAbs[2, 2];
                var r = Math.Abs(axisADotD[2]);

                var result = r0 + r1;
                if (r > result)
                    return false;
            }

            {
                var r0 = extentA[0] * cAbs[0, 0] + extentA[1] * cAbs[1, 0] + extentA[2] * cAbs[2, 0];
                var r1 = extentB[0];
                var r = Math.Abs(axisB[0] * d);

                var result = r0 + r1;
                if (r > result)
                    return false;
            }

            {
                var r0 = extentA[0] * cAbs[0, 1] + extentA[1] * cAbs[1, 1] + extentA[2] * cAbs[2, 1];
                var r1 = extentB[1];
                var r = Math.Abs(axisB[1] * d);

                var result = r0 + r1;
                if (r > result)
                    return false;
            }

            {
                var r0 = extentA[0] * cAbs[0, 2] + extentA[1] * cAbs[1, 2] + extentA[2] * cAbs[2, 2];
                var r1 = extentB[2];
                var r = Math.Abs(axisB[2] * d);

                var result = r0 + r1;
                if (r > result)
                    return false;
            }

            {
                var r0 = extentA[1] * cAbs[2, 0] + extentA[2] * cAbs[1, 0];
                var r1 = extentB[1] * cAbs[0, 2] + extentB[2] * cAbs[0, 1];
                var r = Math.Abs(c[1, 0] * axisADotD[2] - c[2, 0] * axisADotD[1]);

                var result = r0 + r1;
                if (r > result)
                    return false;
            }

            {
                var r0 = extentA[1] * cAbs[2, 1] + extentA[2] * cAbs[1, 1];
                var r1 = extentB[0] * cAbs[0, 2] + extentB[2] * cAbs[0, 0];
                var r = Math.Abs(c[1, 1] * axisADotD[2] - c[2, 1] * axisADotD[1]);

                var result = r0 + r1;
                if (r > result)
                    return false;
            }

            {
                var r0 = extentA[1] * cAbs[2, 2] + extentA[2] * cAbs[1, 2];
                var r1 = extentB[0] * cAbs[0, 1] + extentB[1] * cAbs[0, 0];
                var r = Math.Abs(c[1, 2] * axisADotD[2] - c[2, 2] * axisADotD[1]);

                var result = r0 + r1;
                if (r > result)
                    return false;
            }

            {
                var r0 = extentA[0] * cAbs[2, 0] + extentA[2] * cAbs[0, 0];
                var r1 = extentB[1] * cAbs[1, 2] + extentB[2] * cAbs[1, 1];
                var r = Math.Abs(c[2, 0] * axisADotD[0] - c[0, 0] * axisADotD[2]);

                var result = r0 + r1;
                if (r > result)
                    return false;
            }

            {
                var r0 = extentA[0] * cAbs[2, 1] + extentA[2] * cAbs[0, 1];
                var r1 = extentB[0] * cAbs[1, 2] + extentB[2] * cAbs[1, 0];
                var r = Math.Abs(c[2, 1] * axisADotD[0] - c[0, 1] * axisADotD[2]);

                var result = r0 + r1;
                if (r > result)
                    return false;
            }

            {
                var r0 = extentA[0] * cAbs[2, 2] + extentA[2] * cAbs[0, 2];
                var r1 = extentB[0] * cAbs[1, 1] + extentB[1] * cAbs[1, 0];
                var r = Math.Abs(c[2, 2] * axisADotD[0] - c[0, 2] * axisADotD[2]);

                var result = r0 + r1;
                if (r > result)
                    return false;
            }

            {
                var r0 = extentA[0] * cAbs[1, 0] + extentA[1] * cAbs[0, 0];
                var r1 = extentB[1] * cAbs[2, 2] + extentB[2] * cAbs[2, 1];
                var r = Math.Abs(c[0, 0] * axisADotD[1] - c[1, 0] * axisADotD[0]);

                var result = r0 + r1;
                if (r > result)
                    return false;
            }

            {
                var r0 = extentA[0] * cAbs[1, 1] + extentA[1] * cAbs[0, 1];
                var r1 = extentB[0] * cAbs[2, 2] + extentB[2] * cAbs[2, 0];
                var r = Math.Abs(c[0, 1] * axisADotD[1] - c[1, 1] * axisADotD[0]);

                var result = r0 + r1;
                if (r > result)
                    return false;
            }

            {
                var r0 = extentA[0] * cAbs[1, 2] + extentA[1] * cAbs[0, 2];
                var r1 = extentB[0] * cAbs[2, 1] + extentB[1] * cAbs[2, 0];
                var r = Math.Abs(c[0, 2] * axisADotD[1] - c[1, 2] * axisADotD[0]);

                var result = r0 + r1;
                if (r > result)
                    return false;
            }

            return true;
        }

        #endregion
    }
}
