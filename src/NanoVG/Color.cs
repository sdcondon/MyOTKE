using System;

namespace NanoVG
{
    //// Colors in NanoVG are stored as unsigned ints in ABGR format.
    public struct Color
    {
        public float R;
        public float G;
        public float B;
        public float A;

        /// <summary>
        /// Returns a color value from red, green, blue values. Alpha will be set to 255 (1.0f).
        /// </summary>
        /// <param name="r">The red component of the color.</param>
        /// <param name="g">The green component of the color.</param>
        /// <param name="b">The blue component of the color.</param>
        /// <returns>The generated color value.</returns>
        public static Color RGB(byte r, byte g, byte b) => RGBA(r, g, b, 255);

        /// <summary>
        /// Returns a color value from red, green, blue values. Alpha will be set to 1.0f.
        /// </summary>
        /// <param name="r">The red component of the color.</param>
        /// <param name="g">The green component of the color.</param>
        /// <param name="b">The blue component of the color.</param>
        /// <returns>The generated color value.</returns>
        public static Color RGB(float r, float g, float b) => RGBA(r, g, b, 1.0f);

        /// <summary>
        /// Returns a color value from red, green, blue and alpha values.
        /// </summary>
        /// <param name="r">The red component of the color.</param>
        /// <param name="g">The green component of the color.</param>
        /// <param name="b">The blue component of the color.</param>
        /// <param name="a">The alpha component of the color.</param>
        /// <returns>The generated color value.</returns>
        public static Color RGBA(byte r, byte g, byte b, byte a)
        {
            return new Color
            {
                R = r / 255f,
                G = g / 255f,
                B = b / 255f,
                A = a / 255f,
            };
        }

        /// <summary>
        /// Returns a color value from red, green, blue and alpha values.
        /// </summary>
        /// <param name="r">The red component of the color.</param>
        /// <param name="g">The green component of the color.</param>
        /// <param name="b">The blue component of the color.</param>
        /// <param name="a">The alpha component of the color.</param>
        /// <returns>The generated color value.</returns>
        public static Color RGBA(float r, float g, float b, float a)
        {
            return new Color
            {
                R = r,
                G = g,
                B = b,
                A = a,
            };
        }

        /// <summary>
        /// Linearly interpolates from color c0 to c1, and returns resulting color value.
        /// </summary>
        /// <param name="c0">One color of the pair to interpolate between.</param>
        /// <param name="c1">The other color of the pair to interpolate between.</param>
        /// <param name="u">The weight of <see cref="c1"/> in the generated color.</param>
        /// <returns>The generated color.</returns>
        public static Color LerpRGBA(Color c0, Color c1, float u)
        {
            u = Math.Max(0f, Math.Min(u, 1f)); // clamp
            float oneminu = 1.0f - u;

            return new Color
            {
                R = c0.R * oneminu + c1.R * u,
                G = c0.G * oneminu + c1.G * u,
                B = c0.B * oneminu + c1.B * u,
                A = c0.A * oneminu + c1.A * u,
            };
        }

        /// <summary>
        /// Sets transparency of a color value.
        /// </summary>
        /// <param name="c0">The color to set the transparency of.</param>
        /// <param name="a">The new alpha value of the color.</param>
        /// <returns>The updated color.</returns>
        public static Color TransRGBA(Color c0, byte a)
        {
            c0.A = a / 255.0f;
            return c0;
        }

        /// <summary>
        /// Sets transparency of a color value.
        /// </summary>
        /// <param name="c0">The color to set the transparency of.</param>
        /// <param name="a">The new alpha value of the color.</param>
        /// <returns>The updated color.</returns>
        public static Color TransRGBA(Color c0, float a)
        {
            c0.A = a;
            return c0;
        }

        /// <summary>
        /// Returns a color value specified by hue, saturation and lightness.
        /// HSL values are all in range [0..1], alpha will be set to 255.
        /// </summary>
        /// <param name="h">The hue of the generated color.</param>
        /// <param name="s">The saturation of the generated color.</param>
        /// <param name="l">The lightness of the generated color.</param>
        /// <returns>The generated color.</returns>
        public static Color HSL(float h, float s, float l) => HSLA(h, s, l, 255);

        /// <summary>
        /// Returns color value specified by hue, saturation and lightness and alpha.
        /// HSL values are all in range [0..1], alpha in range [0..255].
        /// </summary>
        /// <param name="h">The hue of the generated color.</param>
        /// <param name="s">The saturation of the generated color.</param>
        /// <param name="l">The lightness of the generated color.</param>
        /// <param name="a">The alpha of the generated color.</param>
        /// <returns>The generated color.</returns>
        public static Color HSLA(float h, float s, float l, byte a)
        {
            float Hue(float hue, float n1, float n2)
            {
                // TODO: while? modulo?
                if (hue < 0)
                {
                    hue += 1;
                }

                // TODO: while? modulo?
                if (hue > 1)
                {
                    hue -= 1;
                }

                if (hue < 1.0f / 6.0f)
                {
                    return n1 + (n2 - n1) * hue * 6.0f;
                }
                else if (hue < 3.0f / 6.0f)
                {
                    return n2;
                }
                else if (hue < 4.0f / 6.0f)
                {
                    return n1 + (n2 - n1) * (2.0f / 3.0f - hue) * 6.0f;
                }

                return n1;
            }

            h %= 1;
            if (h < 0.0f)
            {
                h += 1.0f;
            }

            s = MathEx.Clamp(s, 0.0f, 1.0f);
            l = MathEx.Clamp(l, 0.0f, 1.0f);
            float m2 = l <= 0.5f ? (l * (1 + s)) : (l + s - l * s);
            float m1 = 2 * l - m2;

            return new Color
            {
                R = MathEx.Clamp(Hue(h + 1.0f / 3.0f, m1, m2), 0.0f, 1.0f),
                G = MathEx.Clamp(Hue(h, m1, m2), 0.0f, 1.0f),
                B = MathEx.Clamp(Hue(h - 1.0f / 3.0f, m1, m2), 0.0f, 1.0f),
                A = a / 255.0f,
            };
        }

        public static Color glnvg__premulColor(Color c)
        {
            c.R *= c.A;
            c.G *= c.A;
            c.B *= c.A;
            return c;
        }
    }
}
