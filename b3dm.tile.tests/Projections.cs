﻿using System;
using System.Collections.Generic;
using Wkx;

namespace B3dm.Tile.Tests
{
    public static class Projections
    {

        public static bool InvertTriangle(Point vectProd, Point point0, Point point1, Point point2)
        {
            var crossproduct = point1.Minus(point0).Cross(point2.Minus(point0));
            var dotproduct = DotProduct(vectProd, crossproduct);
            var invert = (dotproduct < 0);
            return invert;
        }


        public static double DotProduct(Point vec1, Point vec2)
        {
            double product = (double)vec1.X * (double)vec2.X;
            product += (double)vec1.Y * (double)vec2.Y;
            product += (double)vec1.Y * (double)vec2.Z;
            return product;
        }


        public static Point GetVectorProduct(Polygon polygon)
        {
            var points = polygon.ExteriorRing.Points;
            var vect1 = points[1].Minus(points[0]);
            var vect2 = points[2].Minus(points[0]);
            var vectProd = vect1.Cross(vect2);
            return vectProd;
        }

        public static List<double> Get2DPoints(Polygon polygon3d)
        {
            var points2d = new List<double>();
            var vectProd = GetVectorProduct(polygon3d);
            var points3d = polygon3d.ExteriorRing.Points;

            foreach (var point3d in points3d)
            {
                var point2d = GetPoint(vectProd, point3d);
                points2d.Add((double)point2d.X);
                points2d.Add((double)point2d.Y);
            }

            return points2d;
        }

        private static Point GetPoint(Point vectProd, Point point3d)
        {
            Point newpoint;
            if (IsYZProjection(vectProd))
            {
                newpoint = new Point((double)point3d.Y, (double)point3d.Z);
            }
            else if (IsZXProjection(vectProd))
            {
                newpoint = new Point((double)point3d.Y, (double)point3d.Z);
            }
            else
            {
                // does not occur in building.wkb?
                newpoint = new Point((double)point3d.X, (double)point3d.Y);
            }

            return newpoint;
        }

        public static bool IsYZProjection(Point vectProd)
        {
            return Math.Abs((double)vectProd.X) > Math.Abs((double)vectProd.Y) &&
                Math.Abs((double)vectProd.X) > Math.Abs((double)vectProd.Z);
        }

        public static bool IsZXProjection(Point vectProd)
        {
            return Math.Abs((double)vectProd.Y) > Math.Abs((double)vectProd.Z);
        }

    }
}
