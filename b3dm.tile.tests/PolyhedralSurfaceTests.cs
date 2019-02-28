﻿using NUnit.Framework;
using System;
using System.Reflection;
using Wkx;

namespace B3dm.Tile.Tests
{
    public class PolyhedralSurfaceTests
    {
        [Test]
        public void PolyhedralSurfaceBoundingBox3DTest()
        {
            // arrange
            const string testfile = "B3dm.Tile.Tests.testfixtures.building.wkb";
            var buildingWkb = Assembly.GetExecutingAssembly().GetManifestResourceStream(testfile);
            var g = Geometry.Deserialize<WkbSerializer>(buildingWkb);
            Assert.IsTrue(g.GeometryType == GeometryType.PolyhedralSurface);
            var polyhedralsurface = ((PolyhedralSurface)g);

            // act
            var bb = polyhedralsurface.GetBoundingBox3D();

            // assert
            Assert.IsTrue(polyhedralsurface.Geometries.Count == 15);  // 15 polygons
            Assert.IsTrue(polyhedralsurface.Geometries[0].ExteriorRing.Points.Count == 5);  // 5 points in exteriorring of first geometry
            Assert.IsTrue(g.GeometryType == GeometryType.PolyhedralSurface);

            Assert.IsTrue(Math.Round(bb.XMin, 2) == -8.75);
            Assert.IsTrue(Math.Round(bb.YMin, 2) == -7.36);
            Assert.IsTrue(Math.Round(bb.ZMin, 2) == -2.05);
            Assert.IsTrue(Math.Round(bb.XMax, 2) == 8.80);
            Assert.IsTrue(Math.Round(bb.YMax, 2) == 7.30);
            Assert.IsTrue(Math.Round(bb.ZMax, 2) == 2.05);
        }
    }
}
