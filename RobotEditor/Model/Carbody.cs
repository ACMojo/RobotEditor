using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Media3D;

using HelixToolkit.Wpf;

using VirtualRobotWrapperLib.OBB;

namespace RobotEditor.Model
{
    class Carbody
    {
        #region Fields

        public BoxVisual3D SymmetryPlane;
        public Matrix3D Center;
        public BoundingBoxVisual3D BoundingBox;
        public CoordinateSystemVisual3D RootFrame;
        public List<Point3D> HitPoints;
        public List<Point3D> RayOrigins;
        public List<CoordinateSystemVisual3D> HitPoints3D;
        public List<MeshGeometryVisual3D> RayOrigins3D;
        public int DirectionOfSymmetryPlane;

        #endregion

        #region Instance

        public Carbody(IObbWrapper obbCalculator, string path, string name, ModelVisual3D model)
        {
            Path = path;
            Name = name;
            HitPoints = new List<Point3D>();
            RayOrigins = new List<Point3D>();
            DirectionOfSymmetryPlane = 0;

            HitPoints3D = new List<CoordinateSystemVisual3D>();
            RayOrigins3D = new List<MeshGeometryVisual3D>();

            if (model != null)
            {
                CarbodyModel = model;
                FindBoundingBox(obbCalculator);
            }
        }

        #endregion

        #region Properties

        public string Path { get; set; }
        public string Name { get; set; }
        public ModelVisual3D CarbodyModel { get; set; }

        public double[][] BoundingBoxAxis { get; private set; }
        public double[] BoundingBoxHalfExtents { get; private set; }
        public double[] BoundingBoxPosition { get; private set; }

        #endregion

        #region Public methods

        public void Add3DHitPoint(Point3D value)
        {
            HitPoints.Add(value);
        }

        public void Add3DRayOrigin(Point3D value)
        {
            RayOrigins.Add(value);
        }

        public void Show3DHitPointGeometries()
        {
            foreach (var hitPoint in HitPoints)
            {
                var coordinateSystem = new CoordinateSystemVisual3D { ArrowLengths = 100.0, Transform = new TranslateTransform3D(hitPoint.X, hitPoint.Y, hitPoint.Z) };
                HitPoints3D.Add(coordinateSystem);
                CarbodyModel.Children.Add(HitPoints3D.Last());
            }
        }

        public void Show3DRayOriginGeometries()
        {
            foreach (var rayOrigin in RayOrigins)
            {
                var mgv = new MeshGeometryVisual3D();
                var mb = new MeshBuilder();
                mb.AddBox(rayOrigin, 10.0, 10.0, 10.0);
                mgv.MeshGeometry = mb.ToMesh();
                mgv.Material = MaterialHelper.CreateMaterial(Colors.Red);
                RayOrigins3D.Add(mgv);
                CarbodyModel.Children.Add(RayOrigins3D.Last());
            }
        }

        public void Show3DBoundingBoxGeometry()
        {
            var boxPosVector = new Vector3D(-BoundingBoxHalfExtents[0], -BoundingBoxHalfExtents[1], -BoundingBoxHalfExtents[2]);
            var centerTemp = Center;

            RootFrame = new CoordinateSystemVisual3D { ArrowLengths = 100.0, Transform = new MatrixTransform3D(Center) };
            CarbodyModel.Children.Add(RootFrame);

            centerTemp.TranslatePrepend(boxPosVector);
            BoundingBox = new BoundingBoxVisual3D
            {
                BoundingBox = new Rect3D(
                    new Point3D(),
                    new Size3D(
                        Math.Abs(BoundingBoxHalfExtents[0]) * 2,
                        Math.Abs(BoundingBoxHalfExtents[1]) * 2,
                        Math.Abs(BoundingBoxHalfExtents[2] * 2))),
                Diameter = 20.0,
                Transform = new MatrixTransform3D(centerTemp)
            };
            CarbodyModel.Children.Add(BoundingBox);
        }

        public void Show3DSymmetryPlaneGeometry()
        {
            var sizeOfSymmetryPlane = new[] { 10.0, 10.0, 10.0 };

            sizeOfSymmetryPlane[Array.IndexOf(BoundingBoxHalfExtents, BoundingBoxHalfExtents.Max())] = BoundingBoxHalfExtents.Max() * 2;
            sizeOfSymmetryPlane[DirectionOfSymmetryPlane] = BoundingBoxHalfExtents[DirectionOfSymmetryPlane] * 2;

            SymmetryPlane = new BoxVisual3D
            {
                Length = sizeOfSymmetryPlane[0],
                Width = sizeOfSymmetryPlane[1],
                Height = sizeOfSymmetryPlane[2],
                Material = MaterialHelper.CreateMaterial(Colors.HotPink),
                Transform = new MatrixTransform3D(Center)
            };
            CarbodyModel.Children.Add(SymmetryPlane);
        }

        public void Hide3DHitPointGeometries()
        {
            foreach (var hitPoint in HitPoints3D)
                CarbodyModel.Children.Remove(hitPoint);

            HitPoints3D.Clear();
        }

        public void Hide3DRayOriginGeometries()
        {
            foreach (var rayOrigin in RayOrigins3D)
                CarbodyModel.Children.Remove(rayOrigin);

            RayOrigins3D.Clear();
        }

        public void Hide3DBoundingBoxGeometry()
        {
            CarbodyModel.Children.Remove(BoundingBox);
            CarbodyModel.Children.Remove(RootFrame);
            BoundingBox = null;
        }

        public void Hide3DSymmetryPlaneGeometry()
        {
            CarbodyModel.Children.Remove(SymmetryPlane);
            SymmetryPlane = null;
        }

        #endregion

        #region Private methods

        private void FindBoundingBox(IObbWrapper obbCalculator)
        {
            var carbodyAsMesh = (MeshGeometry3D)((Model3DGroup)CarbodyModel.Content).Children.Cast<GeometryModel3D>().First().Geometry;

            var pointCloud = new double[carbodyAsMesh.Positions.Count][];
            var i = 0;
            foreach (var pointOnCarbody in carbodyAsMesh.Positions)
            {
                pointCloud[i] = new[] { pointOnCarbody.X, pointOnCarbody.Y, pointOnCarbody.Z };
                i++;
            }

            obbCalculator.Calculate(pointCloud);

            BoundingBoxAxis = obbCalculator.Axis;
            BoundingBoxHalfExtents = obbCalculator.HalfExtents;
            BoundingBoxPosition = obbCalculator.Position;

            Center = new Matrix3D(
                BoundingBoxAxis[0][0],
                BoundingBoxAxis[0][1],
                BoundingBoxAxis[0][2],
                0.0,
                BoundingBoxAxis[1][0],
                BoundingBoxAxis[1][1],
                BoundingBoxAxis[1][2],
                0.0,
                BoundingBoxAxis[2][0],
                BoundingBoxAxis[2][1],
                BoundingBoxAxis[2][2],
                0.0,
                BoundingBoxPosition[0],
                BoundingBoxPosition[1],
                BoundingBoxPosition[2],
                1.0);
        }

        #endregion
    }
}