﻿using System;
using Wkx;

namespace Wkb2Gltf
{
    public class BoundingBox3D
    {
        public BoundingBox3D()
        {

        }
        public BoundingBox3D(double XMin, double YMin, double ZMin, double XMax, double YMax, double ZMax)
        {
            this.XMin = XMin;
            this.YMin = YMin;
            this.ZMin = ZMin;
            this.XMax = XMax;
            this.YMax = YMax;
            this.ZMax = ZMax;

        }
        public double XMin { get; set; }
        public double XMax { get; set; }
        public double YMin { get; set; }
        public double YMax { get; set; }
        public double ZMin { get; set; }
        public double ZMax { get; set; }

        public Point GetCenter()
        {
            var x = (XMax + XMin) / 2;
            var y = (YMax + YMin) / 2;
            var z = (ZMax + ZMin) / 2;
            return new Point(x,y,z);
        }

        public BoundingBox3D TransformYToZ()
        {
            var res = new BoundingBox3D();
            res.XMin = XMin;
            res.YMin = ZMin * -1;
            res.ZMin = YMin;
            res.XMax = XMax;
            res.YMax = ZMin * -1; // heuh?
            res.ZMax = YMax;
            return res;
        }

        public override string ToString()
        {
            return $"{XMin},{YMin},{ZMin},{XMax},{YMax},{ZMax}";
        }

        public BoundingBox ToBoundingBox()
        {
            return new BoundingBox(XMin, YMin, XMax, YMax);
        }

        public double ExtentX()
        {
            return (XMax - XMin);
        }
        public double ExtentY()
        {
            return (YMax - YMin);
        }
        public double ExtentZ()
        {
            return (ZMax - ZMin);
        }

        public double[] GetBox()
        {
            var center = GetCenter();
            var xAxis = Math.Round(ExtentX() / 2, 3);
            var yAxis = Math.Round(ExtentY() / 2, 3);
            var zAxis = Math.Round(ExtentZ() / 2, 3);

            var result = new double[] { Math.Round((double)center.X, 3), Math.Round((double)center.Y, 3), Math.Round((double)center.Z, 3),
                xAxis,0,0,0,yAxis,0,0,0,zAxis
            };
            return result;
        }
    }
}
