using System.Linq;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using MathGeoLibWrapper;
using System.Windows.Media;
using System.Collections.Generic;
using System;

namespace RobotEditor.Model
{
    class Carbody
    {
        #region Instance

        public Carbody(string path, string name, ModelVisual3D model)
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
                FindBoundingBox();
            }        
        }

        #endregion

        #region Properties

        public OBBWrapper OBBCalculator { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public ModelVisual3D CarbodyModel { get; set; }
        public BoxVisual3D SymmetryPlane;
        public Matrix3D Center;
        public BoundingBoxVisual3D BoundingBox;
        public CoordinateSystemVisual3D RootFrame;
        public List<Point3D> HitPoints;
        public List<Point3D> RayOrigins;
        public List<CoordinateSystemVisual3D> HitPoints3D;
        public List<MeshGeometryVisual3D> RayOrigins3D;
        public int DirectionOfSymmetryPlane;

        public VoxelOctree Octree;

        #endregion

        #region Private methods

        private void FindBoundingBox()
        {
            var CarbodyAsMesh = (MeshGeometry3D)(((Model3DGroup)CarbodyModel.Content).Children.Cast<GeometryModel3D>()).First().Geometry;

            double[][] pointCloud = new double[CarbodyAsMesh.Positions.Count][];
            int i = 0;
            foreach (Point3D pointOnCarbody in CarbodyAsMesh.Positions)
            {
                pointCloud[i] = new double[] { pointOnCarbody.X, pointOnCarbody.Y, pointOnCarbody.Z };
                i++;
            }
            OBBCalculator = new OBBWrapper(pointCloud);
            Center = new Matrix3D(
                        OBBCalculator.Axis[0][0],
                        OBBCalculator.Axis[0][1],
                        OBBCalculator.Axis[0][2],
                        0.0,
                        OBBCalculator.Axis[1][0],
                        OBBCalculator.Axis[1][1],
                        OBBCalculator.Axis[1][2],
                        0.0,
                        OBBCalculator.Axis[2][0],
                        OBBCalculator.Axis[2][1],
                        OBBCalculator.Axis[2][2],
                        0.0,
                        OBBCalculator.Position[0],
                        OBBCalculator.Position[1],
                        OBBCalculator.Position[2],
                        1.0);
        }

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

        public void show3DHitPointGeometries()
        {
            foreach (var hitPoint in HitPoints)
            {
                var coordinateSystem = new CoordinateSystemVisual3D() { ArrowLengths = 100.0, Transform = new TranslateTransform3D(hitPoint.X, hitPoint.Y, hitPoint.Z) };
                HitPoints3D.Add(coordinateSystem);
                CarbodyModel.Children.Add(HitPoints3D.Last());
            }
        }

        public void show3DRayOriginGeometries()
        {
            foreach(var rayOrigin in RayOrigins)
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

        public void show3DBoundingBoxGeometry()
        {
            var boxPosVector = new Vector3D(-OBBCalculator.HalfExtents[0], -OBBCalculator.HalfExtents[1], -OBBCalculator.HalfExtents[2]);
            var centerTemp = Center;

            RootFrame = new CoordinateSystemVisual3D() { ArrowLengths = 100.0 };
            RootFrame.Transform = new MatrixTransform3D(Center);
            CarbodyModel.Children.Add(RootFrame);

            centerTemp.TranslatePrepend(boxPosVector);
            BoundingBox = new BoundingBoxVisual3D { BoundingBox = new Rect3D(new Point3D(), new Size3D(Math.Abs(OBBCalculator.HalfExtents[0]) * 2, Math.Abs(OBBCalculator.HalfExtents[1]) * 2, Math.Abs(OBBCalculator.HalfExtents[2] * 2))), Diameter = 20.0 };
            BoundingBox.Transform = new MatrixTransform3D(centerTemp);
            CarbodyModel.Children.Add(BoundingBox);
        }

        public void show3DSymmetryPlaneGeometry()
        {
            var sizeOfSymmetryPlane = new double[3] { 10.0, 10.0, 10.0 };

            sizeOfSymmetryPlane[Array.IndexOf(OBBCalculator.HalfExtents, OBBCalculator.HalfExtents.Max())] = OBBCalculator.HalfExtents.Max() * 2;
            sizeOfSymmetryPlane[DirectionOfSymmetryPlane] = OBBCalculator.HalfExtents[DirectionOfSymmetryPlane] * 2;

            SymmetryPlane = new BoxVisual3D() { Length = sizeOfSymmetryPlane[0], Width = sizeOfSymmetryPlane[1], Height = sizeOfSymmetryPlane[2], Material = MaterialHelper.CreateMaterial(Colors.HotPink) };
            SymmetryPlane.Transform = new MatrixTransform3D(Center);
            CarbodyModel.Children.Add(SymmetryPlane);
        }

        public void hide3DHitPointGeometries()
        {
            foreach (var hitPoint in HitPoints3D)
            {
                CarbodyModel.Children.Remove(hitPoint);
            }
            HitPoints3D.Clear();
        }

        public void hide3DRayOriginGeometries()
        {
            foreach(var rayOrigin in RayOrigins3D)
            {
                CarbodyModel.Children.Remove(rayOrigin);
            }
            RayOrigins3D.Clear();
        }

        public void hide3DBoundingBoxGeometry()
        {
            CarbodyModel.Children.Remove(BoundingBox);
            CarbodyModel.Children.Remove(RootFrame);
            BoundingBox = null;
        }

        public void hide3DSymmetryPlaneGeometry()
        {
            CarbodyModel.Children.Remove(SymmetryPlane);
            SymmetryPlane = null;
        }


        #endregion

    }
}
