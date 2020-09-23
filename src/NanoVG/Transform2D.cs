using System;

namespace NanoVG
{
    public struct Transform2D
    {
        public Transform2D(float r1c1, float r2c1, float r1c2, float r2c2, float r1c3, float r2c3)
        {
            R1C1 = r1c1;
            R2C1 = r2c1;
            R1C2 = r1c2;
            R2C2 = r2c2;
            R1C3 = r1c3;
            R2C3 = r2c3;
        }

        /// <summary>Row 1, column 1 - the coefficient of X in the X component of the output.</summary>
        public float R1C1;

        /// <summary>Row 2, column 1 - the coefficient of X in the Y component of the output.</summary>
        public float R2C1;

        /// <summary>Row 1, column 2 - the coefficient of Y in the X component of the output.</summary>
        public float R1C2;

        /// <summary>Row 2, column 2 - the coefficient of Y in the Y component of the output.</summary>
        public float R2C2;

        /// <summary>Row 1, column 3 - the constant term in the X component of the output.</summary>
        public float R1C3;

        /// <summary>Row 2, column 3 - the constant term in the Y component of the output.</summary>
        public float R2C3;

        /// <summary>
        /// Creates a new identity transform.
        /// </summary>
        /// <returns>The new transform.</returns>
        public static Transform2D Identity()
        {
            return new Transform2D
            {
                R1C1 = 1.0f,
                R2C1 = 0.0f,
                R1C2 = 0.0f,
                R2C2 = 1.0f,
                R1C3 = 0.0f,
                R2C3 = 0.0f,
            };
        }

        /// <summary>
        /// Sets a matrix buffer to a translation matrix.
        /// </summary>
        /// <param name="dst">The buffer to populate with (the first two rows of) the translation matrix (in column-major order).</param>
        /// <param name="tx">The x-offset of the translation.</param>
        /// <param name="ty">The y-offset of the translation.</param>
        public static Transform2D Translate(float tx, float ty)
        {
            return new Transform2D
            {
                R1C1 = 1.0f,
                R2C1 = 0.0f,
                R1C2 = 0.0f,
                R2C2 = 1.0f,
                R1C3 = tx,
                R2C3 = ty,
            };
        }

        /// <summary>
        /// Sets a matrix buffer to a scale matrix.
        /// </summary>
        /// <param name="dst">The buffer to populate with (the first two rows of) the scale matrix (in column-major order).</param>
        /// <param name="sx">The scale in the x-direction.</param>
        /// <param name="sy">The scale in the y-direction.</param>
        public static Transform2D Scale(float sx, float sy)
        {
            return new Transform2D
            {
                R1C1 = sx,
                R2C1 = 0.0f,
                R1C2 = 0.0f,
                R2C2 = sy,
                R1C3 = 0.0f,
                R2C3 = 0.0f,
            };
        }

        /// <summary>
        /// Sets the transform to rotate matrix. Angle is specified in radians.
        /// </summary>
        /// <param name="dst">The buffer to populate with (the first two rows of) the rotation matrix (in column-major order).</param>
        /// <param name="a">The angle (in radians) of the rotation.</param>
        public static Transform2D Rotate(float a)
        {
            float cs = (float)Math.Cos(a);
            float sn = (float)Math.Sin(a);

            return new Transform2D
            {
                R1C1 = cs,
                R2C1 = sn,
                R1C2 = -sn,
                R2C2 = cs,
                R1C3 = 0.0f,
                R2C3 = 0.0f,
            };
        }

        /// <summary>
        /// Sets the transform to skew-x matrix. Angle is specified in radians.
        /// </summary>
        /// <param name="dst">The buffer to populate with (the first two rows of) the skew matrix (in column-major order).</param>
        /// <param name="a">The angle of the skew, in radians.</param>
        public static Transform2D SkewX(float a)
        {
            return new Transform2D
            {
                R1C1 = 1.0f,
                R2C1 = 0.0f,
                R1C2 = (float)Math.Tan(a),
                R2C2 = 1.0f,
                R1C3 = 0.0f,
                R2C3 = 0.0f,
            };
        }

        /// <summary>
        /// // Sets the transform to skew-y matrix. Angle is specified in radians.
        /// </summary>
        /// <param name="dst">The buffer to populate with (the first two rows of) the skew matrix (in column-major order).</param>
        /// <param name="a">The angle of the skew, in radians.</param>
        public static Transform2D SkewY(float a)
        {
            return new Transform2D
            {
                R1C1 = 1.0f,
                R2C1 = (float)Math.Tan(a),
                R1C2 = 0.0f,
                R2C2 = 1.0f,
                R1C3 = 0.0f,
                R2C3 = 0.0f,
            };
        }

        /// <summary>
        /// Sets the transform to the result of multiplication of two transforms, of A = A*B.
        /// </summary>
        /// <param name="dst">The initial matrix to be updated.</param>
        /// <param name="src">The matrix to multiply <see cref="dst"/> by (on the right).</param>
        public static void Multiply(ref Transform2D dst, Transform2D src)
        {
            float r1c1 = dst.R1C1 * src.R1C1 + dst.R2C1 * src.R1C2;
            float r1c2 = dst.R1C2 * src.R1C1 + dst.R2C2 * src.R1C2;
            float r1c3 = dst.R1C3 * src.R1C1 + dst.R2C3 * src.R1C2 + src.R1C3;
            dst.R2C1 = dst.R1C1 * src.R2C1 + dst.R2C1 * src.R2C2;
            dst.R2C2 = dst.R1C2 * src.R2C1 + dst.R2C2 * src.R2C2;
            dst.R2C3 = dst.R1C3 * src.R2C1 + dst.R2C3 * src.R2C2 + src.R2C3;
            dst.R1C1 = r1c1;
            dst.R1C2 = r1c2;
            dst.R1C3 = r1c3;
        }

        /// <summary>
        /// Sets the transform to the result of multiplication of two transforms, of A = B*A.
        /// </summary>
        /// <param name="dst">The initial matrix to be updated.</param>
        /// <param name="src">The matrix to multiply <see cref="dst"/> by (on the left).</param>
        public static void Premultiply(ref Transform2D dst, Transform2D src)
        {
            var s2 = src;
            Multiply(ref s2, dst);
            dst = s2;
        }

        /// <summary>
        /// Sets the destination to inverse of specified transform.
        /// </summary>
        /// <param name="dst">The buffer to populate with the (first two rows of the) inverse matrix (in column-major order).</param>
        /// <returns>1 if the inverse could be calculated, else 0.</returns>
        public int Inverse(out Transform2D dst)
        {
            //// TODO: rename to 'Try..' & return a boolean
            double invdet, det = (double)R1C1 * R2C2 - (double)R1C2 * R2C1;
            if (det > -1e-6 && det < 1e-6)
            {
                dst = Identity();
                return 0;
            }

            invdet = 1.0 / det;
            dst.R1C1 = (float)(R2C2 * invdet);
            dst.R1C2 = (float)(-R1C2 * invdet);
            dst.R1C3 = (float)(((double)R1C2 * R2C3 - (double)R2C2 * R1C3) * invdet);
            dst.R2C1 = (float)(-R2C1 * invdet);
            dst.R2C2 = (float)(R1C1 * invdet);
            dst.R2C3 = (float)(((double)R2C1 * R1C3 - (double)R1C1 * R2C3) * invdet);
            return 1;
        }

        public float GetAverageScale()
        {
            float sx = (float)Math.Sqrt(R1C1 * R1C1 + R1C2 * R1C2);
            float sy = (float)Math.Sqrt(R2C1 * R2C1 + R2C2 * R2C2);
            return (sx + sy) * 0.5f;
        }

        /// <summary>
        /// Transform a point by this transform.
        /// </summary>
        /// <param name="dx">The x-ordinate of the resultant point.</param>
        /// <param name="dy">The y-ordinate of the resultant point.</param>
        /// <param name="sx">The x-ordinate of the source point.</param>
        /// <param name="sy">The y-ordinate of the source point.</param>
        public void TransformPoint(out float dx, out float dy, float sx, float sy)
        {
            dx = sx * R1C1 + sy * R1C2 + R1C3;
            dy = sx * R2C1 + sy * R2C2 + R2C3;
        }
    }
}
