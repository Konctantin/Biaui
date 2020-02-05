﻿
// ReSharper disable All
// <auto-generated />

using System;
using System.Runtime.CompilerServices;
using Biaui.Controls.NodeEditor;

namespace Biaui.Internals
{
    public static class BiaNodeEditorHelper
    {

        private const float ControlPointLength_float = (float)200;

        internal static ImmutableVec2_float MakeBezierControlPoint(in ImmutableVec2_float src, BiaNodeSlotDir dir)
        {
            switch (dir)
            {
                case BiaNodeSlotDir.Left:
                    return new ImmutableVec2_float(src.X - ControlPointLength_float, src.Y);

                case BiaNodeSlotDir.Top:
                    return new ImmutableVec2_float(src.X, src.Y - ControlPointLength_float);

                case BiaNodeSlotDir.Right:
                    return new ImmutableVec2_float(src.X + ControlPointLength_float, src.Y);

                case BiaNodeSlotDir.Bottom:
                    return new ImmutableVec2_float(src.X, src.Y + ControlPointLength_float);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool HitTestBezier(Span<ImmutableVec2_float> bezierPoints, in ImmutableRect_float rect)
        {
            if (NumberHelper.AreCloseZero(rect.Width) ||
                NumberHelper.AreCloseZero(rect.Height))
                return false;

            return HitTestBezierInternal(bezierPoints, rect);
        }

        private static bool HitTestBezierInternal(Span<ImmutableVec2_float> bezierPoints, in ImmutableRect_float rect)
        {
            Span<ImmutableVec2_float> cl = stackalloc ImmutableVec2_float[4];

            while (true)
            {
                if (rect.Contains(bezierPoints[0]) || rect.Contains(bezierPoints[3]))
                    return true;

                var area = new ImmutableRect_float(bezierPoints, (ImmutableRect_float.CtorPoint4) 0);

                if (rect.IntersectsWith(area) == false)
                    return false;

                var v01 = new ImmutableVec2_float(
                    (bezierPoints[0].X + bezierPoints[1].X) * (float)0.5,
                    (bezierPoints[0].Y + bezierPoints[1].Y) * (float)0.5);
                var v12 = new ImmutableVec2_float(
                    (bezierPoints[1].X + bezierPoints[2].X) * (float)0.5,
                    (bezierPoints[1].Y + bezierPoints[2].Y) * (float)0.5);
                var v23 = new ImmutableVec2_float(
                    (bezierPoints[2].X + bezierPoints[3].X) * (float)0.5,
                    (bezierPoints[2].Y + bezierPoints[3].Y) * (float)0.5);

                var v0112 = new ImmutableVec2_float((v01.X + v12.X) * (float)0.5, (v01.Y + v12.Y) * (float)0.5);
                var v1223 = new ImmutableVec2_float((v12.X + v23.X) * (float)0.5, (v12.Y + v23.Y) * (float)0.5);

                if (NumberHelper.Abs(v0112.X - v1223.X) < (float)0.00001 &&
                    NumberHelper.Abs(v0112.Y - v1223.Y) < (float)0.00001)
                    return rect.IntersectsWith(v0112);

                var c = new ImmutableVec2_float((v0112.X + v1223.X) * (float)0.5, (v0112.Y + v1223.Y) * (float)0.5);

                cl[0] = bezierPoints[0];
                cl[1] = v01;
                cl[2] = v0112;
                cl[3] = c;

                if (HitTestBezierInternal(cl, rect))
                    return true;

                bezierPoints[0] = c;
                bezierPoints[1] = v1223;
                bezierPoints[2] = v23;
            }
        }

        // 参考： https://floris.briolas.nl/floris/2009/10/bounding-box-of-cubic-bezier/ 
        public static ImmutableRect_float MakeBoundingBox(in ImmutableVec2_float p1, in ImmutableVec2_float c1, in ImmutableVec2_float c2, in ImmutableVec2_float p2)
        {
            var aX = A(p1.X, c1.X, c2.X, p2.X);
            var bX = B(p1.X, c1.X, c2.X);
            var cX = C(p1.X, c1.X);

            var aY = A(p1.Y, c1.Y, c2.Y, p2.Y);
            var bY = B(p1.Y, c1.Y, c2.Y);
            var cY = C(p1.Y, c1.Y);

            Span<ImmutableVec2_float> bBox = stackalloc ImmutableVec2_float[2 + 2 + 2];
            bBox[0] = p1;
            bBox[1] = p2;
            var bBoxCount = 2;

            Span<float> res = stackalloc float[2 + 2];

            var resCount = Solve(res, aX, bX, cX);
            resCount += Solve(res.Slice(resCount, res.Length - resCount), aY, bY, cY);

            for (var i = 0; i != resCount; ++i)
            {
                var x = Bezier(p1.X, c1.X, c2.X, p2.X, res[i]);
                var y = Bezier(p1.Y, c1.Y, c2.Y, p2.Y, res[i]);

                bBox[bBoxCount++] = new ImmutableVec2_float(x, y);
            }

            return new ImmutableRect_float(bBox.Slice(0, bBoxCount));
        }

        public static ImmutableRect_float MakeBoundingBox(ReadOnlySpan<ImmutableVec2_float> p)
        {
            var aX = A(p[0].X, p[1].X, p[2].X, p[3].X);
            var bX = B(p[0].X, p[1].X, p[2].X);
            var cX = C(p[0].X, p[1].X);

            var aY = A(p[0].Y, p[1].Y, p[2].Y, p[3].Y);
            var bY = B(p[0].Y, p[1].Y, p[2].Y);
            var cY = C(p[0].Y, p[1].Y);

            Span<ImmutableVec2_float> bBox = stackalloc ImmutableVec2_float[2 + 2 + 2];
            bBox[0] = p[0];
            bBox[1] = p[3];
            var bBoxCount = 2;

            Span<float> res = stackalloc float[2 + 2];

            var resCount = Solve(res, aX, bX, cX);
            resCount += Solve(res.Slice(resCount, res.Length - resCount), aY, bY, cY);

            for (var i = 0; i != resCount; ++i)
            {
                var x = Bezier(p[0].X, p[1].X, p[2].X, p[3].X, res[i]);
                var y = Bezier(p[0].Y, p[1].Y, p[2].Y, p[3].Y, res[i]);

                bBox[bBoxCount++] = new ImmutableVec2_float(x, y);
            }

            return new ImmutableRect_float(bBox.Slice(0, bBoxCount));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Bezier(float x1, float x2, float x3, float x4, float t)
        {
            var mt2 = ((float)1 - t) * ((float)1 - t);
            var mt3 = mt2 * ((float)1 - t);
            var pt2 = t * t;
            var pt3 = pt2 * t;

            return mt3 * x1 + (float)3 * (mt2 * t * x2 + ((float)1 - t) * pt2 * x3) + pt3 * x4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float A(float p0, float p1, float p2, float p3) => (float)3 * (p3 - p0) + (float)9 * (p1 - p2);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float B(float p0, float p1, float p2) => (float)6 * (p2 - p1 - p1 + p0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float C(float p0, float p1) => (float)3 * (p1 - p0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float Determinant(float a, float b, float c) => b * b - (float)4 * a * c;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float SolveP(float a, float b, float sqrtD) => (-b + sqrtD) / ((float)2 * a);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float SolveM(float a, float b, float sqrtD) => (-b - sqrtD) / ((float)2 * a);

        private static int Solve(Span<float> result, float a, float b, float c)
        {
            var d = Determinant(a, b, c);

            if (d <= (float)0)
                return 0;

            if (NumberHelper.AreCloseZero(a))
            {
                result[0] = -c / b;
                return 1;
            }

            var sqrtD = MathF.Sqrt(d);

            if (NumberHelper.AreCloseZero(d))
            {
                result[0] = SolveP(a, b, sqrtD);
                return 1;
            }
            else
            {
                result[0] = SolveP(a, b, sqrtD);
                result[1] = SolveM(a, b, sqrtD);
                return 2;
            }
        }

        private const double ControlPointLength_double = (double)200;

        internal static ImmutableVec2_double MakeBezierControlPoint(in ImmutableVec2_double src, BiaNodeSlotDir dir)
        {
            switch (dir)
            {
                case BiaNodeSlotDir.Left:
                    return new ImmutableVec2_double(src.X - ControlPointLength_double, src.Y);

                case BiaNodeSlotDir.Top:
                    return new ImmutableVec2_double(src.X, src.Y - ControlPointLength_double);

                case BiaNodeSlotDir.Right:
                    return new ImmutableVec2_double(src.X + ControlPointLength_double, src.Y);

                case BiaNodeSlotDir.Bottom:
                    return new ImmutableVec2_double(src.X, src.Y + ControlPointLength_double);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool HitTestBezier(Span<ImmutableVec2_double> bezierPoints, in ImmutableRect_double rect)
        {
            if (NumberHelper.AreCloseZero(rect.Width) ||
                NumberHelper.AreCloseZero(rect.Height))
                return false;

            return HitTestBezierInternal(bezierPoints, rect);
        }

        private static bool HitTestBezierInternal(Span<ImmutableVec2_double> bezierPoints, in ImmutableRect_double rect)
        {
            Span<ImmutableVec2_double> cl = stackalloc ImmutableVec2_double[4];

            while (true)
            {
                if (rect.Contains(bezierPoints[0]) || rect.Contains(bezierPoints[3]))
                    return true;

                var area = new ImmutableRect_double(bezierPoints, (ImmutableRect_double.CtorPoint4) 0);

                if (rect.IntersectsWith(area) == false)
                    return false;

                var v01 = new ImmutableVec2_double(
                    (bezierPoints[0].X + bezierPoints[1].X) * (double)0.5,
                    (bezierPoints[0].Y + bezierPoints[1].Y) * (double)0.5);
                var v12 = new ImmutableVec2_double(
                    (bezierPoints[1].X + bezierPoints[2].X) * (double)0.5,
                    (bezierPoints[1].Y + bezierPoints[2].Y) * (double)0.5);
                var v23 = new ImmutableVec2_double(
                    (bezierPoints[2].X + bezierPoints[3].X) * (double)0.5,
                    (bezierPoints[2].Y + bezierPoints[3].Y) * (double)0.5);

                var v0112 = new ImmutableVec2_double((v01.X + v12.X) * (double)0.5, (v01.Y + v12.Y) * (double)0.5);
                var v1223 = new ImmutableVec2_double((v12.X + v23.X) * (double)0.5, (v12.Y + v23.Y) * (double)0.5);

                if (NumberHelper.Abs(v0112.X - v1223.X) < (double)0.00001 &&
                    NumberHelper.Abs(v0112.Y - v1223.Y) < (double)0.00001)
                    return rect.IntersectsWith(v0112);

                var c = new ImmutableVec2_double((v0112.X + v1223.X) * (double)0.5, (v0112.Y + v1223.Y) * (double)0.5);

                cl[0] = bezierPoints[0];
                cl[1] = v01;
                cl[2] = v0112;
                cl[3] = c;

                if (HitTestBezierInternal(cl, rect))
                    return true;

                bezierPoints[0] = c;
                bezierPoints[1] = v1223;
                bezierPoints[2] = v23;
            }
        }

        // 参考： https://floris.briolas.nl/floris/2009/10/bounding-box-of-cubic-bezier/ 
        public static ImmutableRect_double MakeBoundingBox(in ImmutableVec2_double p1, in ImmutableVec2_double c1, in ImmutableVec2_double c2, in ImmutableVec2_double p2)
        {
            var aX = A(p1.X, c1.X, c2.X, p2.X);
            var bX = B(p1.X, c1.X, c2.X);
            var cX = C(p1.X, c1.X);

            var aY = A(p1.Y, c1.Y, c2.Y, p2.Y);
            var bY = B(p1.Y, c1.Y, c2.Y);
            var cY = C(p1.Y, c1.Y);

            Span<ImmutableVec2_double> bBox = stackalloc ImmutableVec2_double[2 + 2 + 2];
            bBox[0] = p1;
            bBox[1] = p2;
            var bBoxCount = 2;

            Span<double> res = stackalloc double[2 + 2];

            var resCount = Solve(res, aX, bX, cX);
            resCount += Solve(res.Slice(resCount, res.Length - resCount), aY, bY, cY);

            for (var i = 0; i != resCount; ++i)
            {
                var x = Bezier(p1.X, c1.X, c2.X, p2.X, res[i]);
                var y = Bezier(p1.Y, c1.Y, c2.Y, p2.Y, res[i]);

                bBox[bBoxCount++] = new ImmutableVec2_double(x, y);
            }

            return new ImmutableRect_double(bBox.Slice(0, bBoxCount));
        }

        public static ImmutableRect_double MakeBoundingBox(ReadOnlySpan<ImmutableVec2_double> p)
        {
            var aX = A(p[0].X, p[1].X, p[2].X, p[3].X);
            var bX = B(p[0].X, p[1].X, p[2].X);
            var cX = C(p[0].X, p[1].X);

            var aY = A(p[0].Y, p[1].Y, p[2].Y, p[3].Y);
            var bY = B(p[0].Y, p[1].Y, p[2].Y);
            var cY = C(p[0].Y, p[1].Y);

            Span<ImmutableVec2_double> bBox = stackalloc ImmutableVec2_double[2 + 2 + 2];
            bBox[0] = p[0];
            bBox[1] = p[3];
            var bBoxCount = 2;

            Span<double> res = stackalloc double[2 + 2];

            var resCount = Solve(res, aX, bX, cX);
            resCount += Solve(res.Slice(resCount, res.Length - resCount), aY, bY, cY);

            for (var i = 0; i != resCount; ++i)
            {
                var x = Bezier(p[0].X, p[1].X, p[2].X, p[3].X, res[i]);
                var y = Bezier(p[0].Y, p[1].Y, p[2].Y, p[3].Y, res[i]);

                bBox[bBoxCount++] = new ImmutableVec2_double(x, y);
            }

            return new ImmutableRect_double(bBox.Slice(0, bBoxCount));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Bezier(double x1, double x2, double x3, double x4, double t)
        {
            var mt2 = ((double)1 - t) * ((double)1 - t);
            var mt3 = mt2 * ((double)1 - t);
            var pt2 = t * t;
            var pt3 = pt2 * t;

            return mt3 * x1 + (double)3 * (mt2 * t * x2 + ((double)1 - t) * pt2 * x3) + pt3 * x4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double A(double p0, double p1, double p2, double p3) => (double)3 * (p3 - p0) + (double)9 * (p1 - p2);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double B(double p0, double p1, double p2) => (double)6 * (p2 - p1 - p1 + p0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double C(double p0, double p1) => (double)3 * (p1 - p0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double Determinant(double a, double b, double c) => b * b - (double)4 * a * c;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double SolveP(double a, double b, double sqrtD) => (-b + sqrtD) / ((double)2 * a);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double SolveM(double a, double b, double sqrtD) => (-b - sqrtD) / ((double)2 * a);

        private static int Solve(Span<double> result, double a, double b, double c)
        {
            var d = Determinant(a, b, c);

            if (d <= (double)0)
                return 0;

            if (NumberHelper.AreCloseZero(a))
            {
                result[0] = -c / b;
                return 1;
            }

            var sqrtD = Math.Sqrt(d);

            if (NumberHelper.AreCloseZero(d))
            {
                result[0] = SolveP(a, b, sqrtD);
                return 1;
            }
            else
            {
                result[0] = SolveP(a, b, sqrtD);
                result[1] = SolveM(a, b, sqrtD);
                return 2;
            }
        }
    }
}
