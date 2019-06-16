using System;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

using HelixToolkit.Wpf;

using RobotEditor.Model;

namespace RobotEditor.ViewModel
{
    internal class CarbodyViewModel : BaseViewModel
    {
        #region Instance

        public CarbodyViewModel(Carbody carbody)
        {
            Model = carbody;
        }

        #endregion

        #region Properties

        public Carbody Model { get; }

        public ModelVisual3D CarbodyModel
        {
            get { return Model.CarbodyModel; }
            set
            {
                if (value.Equals(Model.CarbodyModel))
                    return;

                Model.CarbodyModel = value;

                RaisePropertyChanged();
            }
        }

        public BoundingBoxVisual3D BoundingBox
        {
            get { return Model.BoundingBox; }
            set
            {
                if (value.Equals(Model.BoundingBox))
                    return;

                Model.BoundingBox = value;

                RaisePropertyChanged();
            }
        }

        public string Name
        {
            get { return Model.Name; }
            set
            {
                if (value.Equals(Model.Name))
                    return;

                if (string.IsNullOrWhiteSpace(value))
                    return;

                Model.Name = value;

                RaisePropertyChanged();
            }
        }

        #endregion

        #region Public methods

        public void CalcRefPosition(Dispatcher dispatcher)
        {
            // Find symmetry plane
            var directionOfTop = 0;
            var directionOfTopShift = 0;
            var directionOfFront = 0;
            var directionOfFrontShift = 0;
            var sumOfSquaresDivided = 9999.0;
            for (var m = 0; m < 2; m++)
            {
                var stepDirection = new[] { 0, 0, 0 };
                var sideASelector = new[] { 0, 0, 0 };
                var sideBSelector = new[] { 0, 0, 0 };

                var z = m;
                if (Array.IndexOf(Model.BoundingBoxHalfExtents, Model.BoundingBoxHalfExtents.Max()) == m)
                    z = m + 1;

                if (m == 1 && Array.IndexOf(Model.BoundingBoxHalfExtents, Model.BoundingBoxHalfExtents.Max()) == 0)
                    z = 2;

                sideASelector[Array.IndexOf(Model.BoundingBoxHalfExtents, Model.BoundingBoxHalfExtents.Max())] = 1;
                sideBSelector[Array.IndexOf(Model.BoundingBoxHalfExtents, Model.BoundingBoxHalfExtents.Max())] = 1;
                sideASelector[z] = 1;
                sideBSelector[z] = -1;

                var matrixStart = new Matrix3D(
                    Model.BoundingBoxAxis[0][0],
                    Model.BoundingBoxAxis[0][1],
                    Model.BoundingBoxAxis[0][2],
                    0.0,
                    Model.BoundingBoxAxis[1][0],
                    Model.BoundingBoxAxis[1][1],
                    Model.BoundingBoxAxis[1][2],
                    0.0,
                    Model.BoundingBoxAxis[2][0],
                    Model.BoundingBoxAxis[2][1],
                    Model.BoundingBoxAxis[2][2],
                    0.0,
                    Model.BoundingBoxPosition[0],
                    Model.BoundingBoxPosition[1],
                    Model.BoundingBoxPosition[2],
                    1.0
                );

                var matrixEnd = new Matrix3D(
                    Model.BoundingBoxAxis[0][0],
                    Model.BoundingBoxAxis[0][1],
                    Model.BoundingBoxAxis[0][2],
                    0.0,
                    Model.BoundingBoxAxis[1][0],
                    Model.BoundingBoxAxis[1][1],
                    Model.BoundingBoxAxis[1][2],
                    0.0,
                    Model.BoundingBoxAxis[2][0],
                    Model.BoundingBoxAxis[2][1],
                    Model.BoundingBoxAxis[2][2],
                    0.0,
                    Model.BoundingBoxPosition[0],
                    Model.BoundingBoxPosition[1],
                    Model.BoundingBoxPosition[2],
                    1.0
                );

                var matrixTranslationStart = new Vector3D(
                    sideASelector[0] * Model.BoundingBoxHalfExtents[0],
                    sideASelector[1] * Model.BoundingBoxHalfExtents[1],
                    sideASelector[2] * Model.BoundingBoxHalfExtents[2]
                );

                var matrixTranslationEnd = new Vector3D(
                    sideBSelector[0] * Model.BoundingBoxHalfExtents[0],
                    sideBSelector[1] * Model.BoundingBoxHalfExtents[1],
                    sideBSelector[2] * Model.BoundingBoxHalfExtents[2]
                );

                matrixStart.TranslatePrepend(matrixTranslationStart);
                matrixEnd.TranslatePrepend(matrixTranslationEnd);

                // symmetry plane
                var sumOfSquares = 0.0;
                var count = 0;

                var steps = (int)Math.Abs(Model.BoundingBoxHalfExtents.Max() * 2 / 50) + 1;
                var maxDistanceFromStart = double.NaN;
                var maxDistanceFromEnd = double.NaN;
                var totalDistanceFromEndFirst = 0d;
                var totalDistanceFromEndLast = 0d;
                var totalDistanceFromPlane = 0d;

                for (var k = 0; k < steps; k++)
                {
                    var distanceFromStart = double.NaN;
                    var distanceFromEnd = double.NaN;
                    if (dispatcher != null && !dispatcher.CheckAccess())
                        dispatcher.Invoke(() => RayHi(matrixStart, matrixEnd, out distanceFromStart, out distanceFromEnd));
                    else
                        RayHi(matrixStart, matrixEnd, out distanceFromStart, out distanceFromEnd);

                    stepDirection[Array.IndexOf(Model.BoundingBoxHalfExtents, Model.BoundingBoxHalfExtents.Max())] = 1;
                    var vector = new Vector3D(stepDirection[0] * -50, stepDirection[1] * -50, stepDirection[2] * -50);

                    matrixStart.TranslatePrepend(vector);
                    matrixEnd.TranslatePrepend(vector);

                    if (double.IsNaN(distanceFromStart) || double.IsNaN(distanceFromEnd))
                        continue;

                    count++;

                    if (double.IsNaN(maxDistanceFromStart) || distanceFromStart > maxDistanceFromStart)
                        maxDistanceFromStart = distanceFromStart;

                    if (double.IsNaN(maxDistanceFromEnd) || distanceFromEnd > maxDistanceFromEnd)
                        maxDistanceFromEnd = distanceFromEnd;

                    if (k < steps / 3)
                        totalDistanceFromEndFirst += distanceFromEnd;
                    else if (k >= steps - steps / 3)
                        totalDistanceFromEndLast += distanceFromEnd;

                    sumOfSquares += Math.Abs(distanceFromStart - distanceFromEnd);

                    if (distanceFromStart > 0.1 && distanceFromEnd > 0.1)
                        totalDistanceFromPlane += Model.BoundingBoxHalfExtents[z] * 2 - distanceFromStart - distanceFromEnd;
                }

                if (count <= 0)
                    continue;

                totalDistanceFromPlane = totalDistanceFromPlane / steps;
                if (totalDistanceFromPlane < 100.0)
                {
                    directionOfTop = z;
                    directionOfFront = Array.IndexOf(Model.BoundingBoxHalfExtents, Model.BoundingBoxHalfExtents.Max());

                    if (maxDistanceFromStart > maxDistanceFromEnd)
                        directionOfTopShift = 1;
                    else
                        directionOfTopShift = -1;

                    if (totalDistanceFromEndFirst > totalDistanceFromEndLast)
                        directionOfFrontShift = 1;
                    else
                        directionOfFrontShift = -1;
                }

                if (sumOfSquares / count >= sumOfSquaresDivided)
                    continue;

                sumOfSquaresDivided = sumOfSquares / count;
                Model.DirectionOfSymmetryPlane = Array.IndexOf(sideASelector, sideASelector.Min());
            }

            var topSelector = new[] { 0, 0, 0 };
            topSelector[directionOfTop] = directionOfTopShift;
            topSelector[directionOfFront] = directionOfFrontShift;

            var coordinateSelector = new[] { 0, 0, 0 };
            coordinateSelector[2] = directionOfTop;
            coordinateSelector[1] = directionOfFront;
            coordinateSelector[0] = Array.IndexOf(topSelector, 0);

            var centerOfTop = new Matrix3D(
                -Model.BoundingBoxAxis[coordinateSelector[0]][0],
                -Model.BoundingBoxAxis[coordinateSelector[0]][1],
                -Model.BoundingBoxAxis[coordinateSelector[0]][2],
                0.0,
                topSelector[directionOfFront] * Model.BoundingBoxAxis[coordinateSelector[1]][0],
                topSelector[directionOfFront] * Model.BoundingBoxAxis[coordinateSelector[1]][1],
                topSelector[directionOfFront] * Model.BoundingBoxAxis[coordinateSelector[1]][2],
                0.0,
                -topSelector[directionOfTop] * Model.BoundingBoxAxis[coordinateSelector[2]][0],
                -topSelector[directionOfTop] * Model.BoundingBoxAxis[coordinateSelector[2]][1],
                -topSelector[directionOfTop] * Model.BoundingBoxAxis[coordinateSelector[2]][2],
                0.0,
                Model.BoundingBoxPosition[0],
                Model.BoundingBoxPosition[1],
                Model.BoundingBoxPosition[2],
                1.0
            );

            var matrixTranslationTop = new Vector3D(
                0,
                Math.Abs(Model.BoundingBoxHalfExtents[directionOfFront]),
                -Math.Abs(Model.BoundingBoxHalfExtents[directionOfTop])
            );

            centerOfTop.TranslatePrepend(matrixTranslationTop);

            // Save index to recover bounding box sizes in result window
            Model.YIndex = directionOfFront;
            Model.ZIndex = directionOfTop;
            if (directionOfFront == 0 || directionOfTop == 0)
                if (directionOfFront == 1 || directionOfTop == 1)
                    Model.XIndex = 2;
                else
                    Model.XIndex = 1;
            else
                Model.XIndex = 0;

            //move carbody front to world     
            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                dispatcher.Invoke(
                    () =>
                    {
                        Model.CarbodyModel.Transform = new MatrixTransform3D(centerOfTop.Inverse());
                    });
            }
            else
            {
                Model.CarbodyModel.Transform = new MatrixTransform3D(centerOfTop.Inverse());
            }
        }

        public void RayHi(Matrix3D matrixStart, Matrix3D matrixEnd, VoxelOctree octree)
        {
            var startPoint = new Point3D();
            var endPoint = new Point3D();

            var transformToBoundingBoxStart = new MatrixTransform3D(matrixStart);
            var transformToBoundingBoxEnd = new MatrixTransform3D(matrixEnd);

            startPoint = transformToBoundingBoxStart.Transform(startPoint);
            endPoint = transformToBoundingBoxEnd.Transform(endPoint);

            // Only used to show start and end points of ray
            Model.Add3DRayOrigin(startPoint);
            Model.Add3DRayOrigin(endPoint);

            // Ray from side A
            var hitParams =
                new RayHitTestParameters(
                    startPoint,
                    new Vector3D(endPoint.X - startPoint.X, endPoint.Y - startPoint.Y, endPoint.Z - startPoint.Z)
                );
            VisualTreeHelper.HitTest(CarbodyModel, null, result => HitTestResultCallback(result, octree), hitParams);

            // Ray from side B
            hitParams =
                new RayHitTestParameters(
                    endPoint,
                    new Vector3D(startPoint.X - endPoint.X, startPoint.Y - endPoint.Y, startPoint.Z - endPoint.Z)
                );
            VisualTreeHelper.HitTest(CarbodyModel, null, result => HitTestResultCallback(result, octree), hitParams);
        }

        #endregion

        #region Private methods

        private void RayHi(Matrix3D matrixStart, Matrix3D matrixEnd, out double distanceFromStart, out double distanceFromEnd)
        {
            var startPoint = new Point3D();
            var endPoint = new Point3D();

            var transformToBoundingBoxStart = new MatrixTransform3D(matrixStart);
            var transformToBoundingBoxEnd = new MatrixTransform3D(matrixEnd);

            startPoint = transformToBoundingBoxStart.Transform(startPoint);
            endPoint = transformToBoundingBoxEnd.Transform(endPoint);

            // Only used to show start and end points of ray
            Model.Add3DRayOrigin(startPoint);
            Model.Add3DRayOrigin(endPoint);

            // Ray from side A
            var hitParams =
                new RayHitTestParameters(
                    startPoint,
                    new Vector3D(endPoint.X - startPoint.X, endPoint.Y - startPoint.Y, endPoint.Z - startPoint.Z)
                );
            var distance = double.NaN;
            VisualTreeHelper.HitTest(CarbodyModel, null, result => HitTestResultCallback(result, out distance), hitParams);
            distanceFromStart = distance;

            // Ray from side B
            hitParams =
                new RayHitTestParameters(
                    endPoint,
                    new Vector3D(startPoint.X - endPoint.X, startPoint.Y - endPoint.Y, startPoint.Z - endPoint.Z)
                );
            distance = double.NaN;
            VisualTreeHelper.HitTest(CarbodyModel, null, result => HitTestResultCallback(result, out distance), hitParams);
            distanceFromEnd = distance;
        }

        private HitTestResultBehavior HitTestResultCallback(HitTestResult result, out double distance)
        {
            // Did we hit 3D?
            var rayResult = result as RayHitTestResult;

            // Did we hit a MeshGeometry3D?
            var rayMeshResult = rayResult as RayMeshGeometry3DHitTestResult;

            if (rayMeshResult == null)
            {
                distance = double.NaN;
                return HitTestResultBehavior.Continue;
            }

            // Used to show surface hits of ray
            Model.Add3DHitPoint(rayMeshResult.PointHit);

            distance = rayResult.DistanceToRayOrigin;

            return HitTestResultBehavior.Stop;
        }

        private HitTestResultBehavior HitTestResultCallback(HitTestResult result, VoxelOctree octree)
        {
            // Did we hit 3D?
            var rayResult = result as RayHitTestResult;

            // Did we hit a MeshGeometry3D?
            var rayMeshResult = rayResult as RayMeshGeometry3DHitTestResult;

            if (rayMeshResult == null)
                return HitTestResultBehavior.Continue;

            Model.Add3DHitPoint(rayMeshResult.PointHit);

            var pointTest = rayMeshResult.PointHit;
            pointTest = Point3D.Multiply(pointTest, Model.CarbodyModel.GetTransform());
            octree.Set((int)pointTest.X, (int)pointTest.Y, (int)pointTest.Z, 1.0);

            return HitTestResultBehavior.Stop;
        }

        #endregion
    }
}