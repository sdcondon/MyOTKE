using System;

namespace NanoVG
{
    //// NanoVG supports four types of paints: linear gradient, box gradient, radial gradient and image pattern.
    //// These can be used as paints for strokes and fills.
    public struct Paint
    {
        public Transform2D xform;
        public Extent2D extent;
        public float radius;
        public float feather;
        public Color innerColor;
        public Color outerColor;
        public int image;

        /// <summary>
        /// Creates and returns a linear gradient.
        /// <para/>
        /// The gradient is transformed by the current transform when it is passed to <see cref="FillPaint"/> or <see cref="StrokePaint"/>.
        /// </summary>
        /// <param name="sx">Start x-ordinate of the gradient.</param>
        /// <param name="sy">Start y-ordinate of the gradient.</param>
        /// <param name="ex">End x-ordinate of the gradient.</param>
        /// <param name="ey">End y-ordinate of the gradient.</param>
        /// <param name="icol">The start color of the gradient.</param>
        /// <param name="ocol">The end color of the gradient.</param>
        /// <returns>The created <see cref="Paint"/> instance.</returns>
        public static Paint LinearGradient(float sx, float sy, float ex, float ey, Color icol, Color ocol)
        {
            Paint p = new Paint();
            float dx, dy, d;
            const float large = 100000;

            // Calculate transform aligned to the line
            dx = ex - sx;
            dy = ey - sy;
            d = (float)Math.Sqrt(dx * dx + dy * dy);
            if (d > 0.0001f)
            {
                dx /= d;
                dy /= d;
            }
            else
            {
                dx = 0;
                dy = 1;
            }

            p.xform.R1C1 = dy;
            p.xform.R2C1 = -dx;
            p.xform.R1C2 = dx;
            p.xform.R2C2 = dy;
            p.xform.R1C3 = sx - dx * large;
            p.xform.R2C3 = sy - dy * large;

            p.extent.X = large;
            p.extent.Y = large + d * 0.5f;

            p.radius = 0.0f;

            p.feather = Math.Max(1.0f, d);

            p.innerColor = icol;
            p.outerColor = ocol;

            return p;
        }

        /// <summary>
        /// Creates and returns a box gradient. A box gradient is a feathered rounded rectangle, it is useful for rendering
        /// drop shadows or highlights for boxes.
        /// <para/>
        /// The gradient is transformed by the current transform when it is passed to <see cref="FillPaint"/> or <see cref="StrokePaint"/>.
        /// </summary>
        /// <param name="x">The x-ordinate of the top-left corner of the rectangle.</param>
        /// <param name="y">The y-ordinate of the top-left corner of the rectangle.</param>
        /// <param name="w">The width of the rectangle.</param>
        /// <param name="h">The height of the rectangle.</param>
        /// <param name="r">The corner radius of the rectangle.</param>
        /// <param name="f">The feather - how blurry the border of the rectangle is.</param>
        /// <param name="icol">The inner color of the gradient.</param>
        /// <param name="ocol">The outer color of the gradient.</param>
        /// <returns>The created paint.</returns>
        public static Paint BoxGradient(float x, float y, float w, float h, float r, float f, Color icol, Color ocol)
        {
            Paint p = new Paint();

            p.xform = Transform2D.Identity();
            p.xform.R1C3 = x + w * 0.5f;
            p.xform.R2C3 = y + h * 0.5f;

            p.extent.X = w * 0.5f;
            p.extent.Y = h * 0.5f;

            p.radius = r;

            p.feather = Math.Max(1.0f, f);

            p.innerColor = icol;
            p.outerColor = ocol;

            return p;
        }

        /// <summary>
        /// Creates and returns a radial gradient.
        /// <para/>
        /// The gradient is transformed by the current transform when it is passed to <see cref="FillPaint"/> or <see cref="StrokePaint"/>.
        /// </summary>
        /// <param name="cx">The x-ordinate of the center of the gradient.</param>
        /// <param name="cy">The y-ordinate of the center of the gradient.</param>
        /// <param name="inr">The inner radius of the gradient.</param>
        /// <param name="outr">The outer radius of the gradient.</param>
        /// <param name="icol">The inner color of the gradient.</param>
        /// <param name="ocol">The outer color of the gradient.</param>
        /// <returns>The created paint.</returns>
        public static Paint RadialGradient(float cx, float cy, float inr, float outr, Color icol, Color ocol)
        {
            Paint p = new Paint();
            float r = (inr + outr) * 0.5f;
            float f = outr - inr;

            p.xform = Transform2D.Identity();
            p.xform.R1C3 = cx;
            p.xform.R2C3 = cy;

            p.extent.X = r;
            p.extent.Y = r;

            p.radius = r;

            p.feather = Math.Max(1.0f, f);

            p.innerColor = icol;
            p.outerColor = ocol;

            return p;
        }

        /// <summary>
        /// Creates and returns an image pattern.
        /// <para/>
        /// The gradient is transformed by the current transform when it is passed to <see cref="FillPaint"/> or <see cref="StrokePaint"/>.
        /// </summary>
        /// <param name="cx">The x-ordinate of the top-left corner of the imsage pattern.</param>
        /// <param name="cy">The y-ordinate of the top-left corner of the imsage pattern.</param>
        /// <param name="w">The width of one image.</param>
        /// <param name="h">The height of one image.</param>
        /// <param name="angle">The angle of rotation around the top-left corner.</param>
        /// <param name="image">The handle of the image to render.</param>
        /// <param name="alpha">The alpha component of the image.</param>
        /// <returns>The created paint.</returns>
        public static Paint ImagePattern(float cx, float cy, float w, float h, float angle, int image, float alpha)
        {
            Paint p = new Paint();

            p.xform = Transform2D.Rotate(angle);
            p.xform.R1C3 = cx;
            p.xform.R2C3 = cy;

            p.extent.X = w;
            p.extent.Y = h;

            p.image = image;

            p.innerColor = p.outerColor = NanoVG.Color.RGBA(1, 1, 1, alpha);

            return p;
        }

        /// <summary>
        /// Creates and returns a flat color.
        /// </summary>
        /// <param name="color">The color of the paint.</param>
        /// <returns>The created paint.</returns>
        public static Paint Color(Color color)
        {
            Paint p = new Paint();
            p.extent.X = 0f;
            p.extent.Y = 0f;
            p.xform = Transform2D.Identity();
            p.radius = 0.0f;
            p.feather = 1.0f;
            p.innerColor = color;
            p.outerColor = color;
            p.image = 0;

            return p;
        }
    }
}
