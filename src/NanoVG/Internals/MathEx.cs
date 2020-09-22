using System;

namespace NanoVG
{
    internal static class MathEx
    {
        public static float Cross(float dx0, float dy0, float dx1, float dy1)
        {
            return dx1 * dy0 - dx0 * dy1;
        }

        public static float Clamp(float a, float mn, float mx)
        {
            return a < mn ? mn : (a > mx ? mx : a);
        }

        public static int Clamp(int a, int mn, int mx)
        {
            return a < mn ? mn : (a > mx ? mx : a);
        }

        public static void IntersectRects(
            float[] dst,
            float ax,
            float ay,
            float aw,
            float ah,
            float bx,
            float by,
            float bw,
            float bh)
        {
            float minx = Math.Max(ax, bx);
            float miny = Math.Max(ay, by);
            float maxx = Math.Min(ax + aw, bx + bw);
            float maxy = Math.Min(ay + ah, by + bh);
            dst[0] = minx;
            dst[1] = miny;
            dst[2] = Math.Max(0.0f, maxx - minx);
            dst[3] = Math.Max(0.0f, maxy - miny);
        }

        public static bool PointEquals(float x1, float y1, float x2, float y2, float tol)
        {
            float dx = x2 - x1;
            float dy = y2 - y1;
            return dx * dx + dy * dy < tol * tol;
        }

        public static float DistancePointToSegment(float x, float y, float px, float py, float qx, float qy)
        {
            float pqx, pqy, dx, dy, d, t;
            pqx = qx - px;
            pqy = qy - py;
            dx = x - px;
            dy = y - py;
            d = pqx * pqx + pqy * pqy;
            t = pqx * dx + pqy * dy;
            if (d > 0) t /= d;
            if (t < 0) t = 0;
            else if (t > 1) t = 1;
            dx = px + t * pqx - x;
            dy = py + t * pqy - y;
            return dx * dx + dy * dy;
        }

        public static float Normalize(ref float x, ref float y)
        {
            float d = (float)Math.Sqrt(x * x + y * y);
            if (d > 1e-6f)
            {
                float id = 1.0f / d;
                x *= id;
                y *= id;
            }

            return d;
        }
    }
}
