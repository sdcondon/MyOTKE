//
// Copyright (c) 2013 Mikko Mononen memon@inside.org
//
// This software is provided 'as-is', without any express or implied
// warranty.  In no event will the authors be held liable for any damages
// arising from the use of this software.
// Permission is granted to anyone to use this software for any purpose,
// including commercial applications, and to alter it and redistribute it
// freely, subject to the following restrictions:
// 1. The origin of this software must not be misrepresented; you must not
//    claim that you wrote the original software. If you use this software
//    in a product, an acknowledgment in the product documentation would be
//    appreciated but is not required.
// 2. Altered source versions must be plainly marked as such, and must not be
//    misrepresented as being the original software.
// 3. This notice may not be removed or altered from any source distribution.
//
using System;

namespace NanoVG
{
    public struct Color
    {
        public float R;
        public float G;
        public float B;
        public float A;
    }

    public class Context
    {
        // should all be private
        public Params @params;
	    public float* commands;
        public int ccommands;
        public int ncommands;
        public float commandx, commandy;
        public NVGstate[] states;//[NVG_MAX_STATES];
        public int nstates;
        public NVGpathCache* cache;
        public float tessTol;
        public float distTol;
        public float fringeWidth;
        public float devicePxRatio;
        public struct FONScontext* fs;
	    public int[] fontImages;//[NVG_MAX_FONTIMAGES];
        public int fontImageIdx;
        public int drawCallCount;
        public int fillTriCount;
        public int strokeTriCount;
        public int textTriCount;
    }

    public struct Paint
    {
        public float[] xform;//[6];
        float[] extent;//[2];
        float radius;
        float feather;
        Color innerColor;
        Color outerColor;
        int image;
    }

    enum Winding
    {
        NVG_CCW = 1, // Winding for solid shapes
        NVG_CW = 2, // Winding for holes
    }

    enum Solidity
    {
        NVG_SOLID = 1, // CCW
        NVG_HOLE = 2, // CW
    }

    enum LineCap
    {
        NVG_BUTT,
        NVG_ROUND,
        NVG_SQUARE,
        NVG_BEVEL,
        NVG_MITER,
    }

    enum Align
    {
        // Horizontal align
        NVG_ALIGN_LEFT = 1 << 0,    // Default, align text horizontally to left.
        NVG_ALIGN_CENTER = 1 << 1,  // Align text horizontally to center.
        NVG_ALIGN_RIGHT = 1 << 2,   // Align text horizontally to right.

        // Vertical align
        NVG_ALIGN_TOP = 1 << 3, // Align text vertically to top.
        NVG_ALIGN_MIDDLE = 1 << 4,  // Align text vertically to middle.
        NVG_ALIGN_BOTTOM = 1 << 5,  // Align text vertically to bottom.
        NVG_ALIGN_BASELINE = 1 << 6, // Default, align text vertically to baseline.
    }

    public enum BlendFactor
    {
        NVG_ZERO = 1 << 0,
        NVG_ONE = 1 << 1,
        NVG_SRC_COLOR = 1 << 2,
        NVG_ONE_MINUS_SRC_COLOR = 1 << 3,
        NVG_DST_COLOR = 1 << 4,
        NVG_ONE_MINUS_DST_COLOR = 1 << 5,
        NVG_SRC_ALPHA = 1 << 6,
        NVG_ONE_MINUS_SRC_ALPHA = 1 << 7,
        NVG_DST_ALPHA = 1 << 8,
        NVG_ONE_MINUS_DST_ALPHA = 1 << 9,
        NVG_SRC_ALPHA_SATURATE = 1 << 10,
    }

    public enum CompositeOperation
    {
        NVG_SOURCE_OVER,
        NVG_SOURCE_IN,
        NVG_SOURCE_OUT,
        NVG_ATOP,
        NVG_DESTINATION_OVER,
        NVG_DESTINATION_IN,
        NVG_DESTINATION_OUT,
        NVG_DESTINATION_ATOP,
        NVG_LIGHTER,
        NVG_COPY,
        NVG_XOR,
    }

    struct CompositeOperationState
    {
        int srcRGB;
        int dstRGB;
        int srcAlpha;
        int dstAlpha;
    }

    struct GlyphPosition
    {
        const char* str;    // Position of the glyph in the input string.
        float x;            // The x-coordinate of the logical glyph position.
        float minx, maxx;   // The bounds of the glyph shape.
    }

    struct TextRow
    {
        const char* start;  // Pointer to the input text where the row starts.
        const char* end;    // Pointer to the input text where the row ends (one past the last character).
        const char* next;   // Pointer to the beginning of the next row.
        float width;        // Logical width of the row.
        float minx, maxx;   // Actual bounds of the row. Logical with and bounds can differ because of kerning and some parts over extending.
    }

    enum NVGimageFlags
    {
        NVG_IMAGE_GENERATE_MIPMAPS = 1 << 0,     // Generate mipmaps during creation of the image.
        NVG_IMAGE_REPEATX = 1 << 1,     // Repeat image in X direction.
        NVG_IMAGE_REPEATY = 1 << 2,     // Repeat image in Y direction.
        NVG_IMAGE_FLIPY = 1 << 3,       // Flips (inverses) image in Y direction when rendered.
        NVG_IMAGE_PREMULTIPLIED = 1 << 4,       // Image data has premultiplied alpha.
        NVG_IMAGE_NEAREST = 1 << 5,     // Image interpolation is Nearest instead Linear
    };

    public class NanoVG
    {
        // Begin drawing a new frame
        // Calls to nanovg drawing API should be wrapped in nvgBeginFrame() & nvgEndFrame()
        // nvgBeginFrame() defines the size of the window to render to in relation currently
        // set viewport (i.e. glViewport on GL backends). Device pixel ration allows to
        // control the rendering on Hi-DPI devices.
        // For example, GLFW returns two dimension for an opened window: window size and
        // frame buffer size. In that case you would set windowWidth/Height to the window size
        // devicePixelRatio to: frameBufferWidth / windowWidth.
        public static void BeginFrame(Context ctx, float windowWidth, float windowHeight, float devicePixelRatio)
        {
            throw new NotImplementedException();
            /*
            printf("Tris: draws:%d  fill:%d  stroke:%d  text:%d  TOT:%d\n",
		        ctx->drawCallCount, ctx->fillTriCount, ctx->strokeTriCount, ctx->textTriCount,
		        ctx->fillTriCount+ctx->strokeTriCount+ctx->textTriCount);

            ctx.nstates = 0;
            Save(ctx);
            Reset(ctx);

            nvg__setDevicePixelRatio(ctx, devicePixelRatio);

            ctx->params.renderViewport(ctx->params.userPtr, windowWidth, windowHeight, devicePixelRatio);

            ctx.drawCallCount = 0;
            ctx.fillTriCount = 0;
            ctx.strokeTriCount = 0;
            ctx.textTriCount = 0;
            */
        }

        // Cancels drawing the current frame.
        public static void CancelFrame(Context ctx)
        {
            throw new NotImplementedException();
            /*
            ctx->params.renderCancel(ctx->params.userPtr);
            */
        }

        // Ends drawing flushing remaining render state.
        public static void EndFrame(Context ctx)
        {
            throw new NotImplementedException();
            /*
            ctx->params.renderFlush(ctx->params.userPtr);
            if (ctx->fontImageIdx != 0)
            {
                int fontImage = ctx->fontImages[ctx->fontImageIdx];
                int i, j, iw, ih;
                // delete images that smaller than current one
                if (fontImage == 0)
                    return;
                nvgImageSize(ctx, fontImage, &iw, &ih);
                for (i = j = 0; i < ctx->fontImageIdx; i++)
                {
                    if (ctx->fontImages[i] != 0)
                    {
                        int nw, nh;
                        nvgImageSize(ctx, ctx->fontImages[i], &nw, &nh);
                        if (nw < iw || nh < ih)
                            nvgDeleteImage(ctx, ctx->fontImages[i]);
                        else
                            ctx->fontImages[j++] = ctx->fontImages[i];
                    }
                }
                // make current font image to first
                ctx->fontImages[j++] = ctx->fontImages[0];
                ctx->fontImages[0] = fontImage;
                ctx->fontImageIdx = 0;
                // clear all images after j
                for (i = j; i < NVG_MAX_FONTIMAGES; i++)
                    ctx->fontImages[i] = 0;
            }
            */
        }

        #region Composite operations

        //// The composite operations in NanoVG are modeled after HTML Canvas API, and
        //// the blend func is based on OpenGL (see corresponding manuals for more info).
        //// The colors in the blending state have premultiplied alpha.

        // Sets the composite operation. The op parameter should be one of NVGcompositeOperation.
        public static void GlobalCompositeOperation(Context ctx, CompositeOperation op)
        {
            throw new NotImplementedException();
            /*
            NVGstate* state = nvg__getState(ctx);
            state->compositeOperation = nvg__compositeOperationState(op);
            */
        }

        // Sets the composite operation with custom pixel arithmetic.
        public static void GlobalCompositeBlendFunc(Context ctx, BlendFactor sfactor, BlendFactor dfactor)
        {
            throw new NotImplementedException();
            /*
            nvgGlobalCompositeBlendFuncSeparate(ctx, sfactor, dfactor, sfactor, dfactor);
            */
        }

        // Sets the composite operation with custom pixel arithmetic for RGB and alpha components separately.
        public static void GlobalCompositeBlendFuncSeparate(Context ctx, BlendFactor srcRGB, BlendFactor dstRGB, BlendFactor srcAlpha, BlendFactor dstAlpha)
        {
            throw new NotImplementedException();
            /*
            NVGcompositeOperationState op;
            op.srcRGB = srcRGB;
            op.dstRGB = dstRGB;
            op.srcAlpha = srcAlpha;
            op.dstAlpha = dstAlpha;

            NVGstate* state = nvg__getState(ctx);
            state->compositeOperation = op;
            */
        }

        #endregion

        #region Color utils

        //// Colors in NanoVG are stored as unsigned ints in ABGR format.

        // Returns a color value from red, green, blue values. Alpha will be set to 255 (1.0f).
        public static Color RGB(byte r, byte g, byte b) => RGBA(r, g, b, 255);

        // Returns a color value from red, green, blue values. Alpha will be set to 1.0f.
        public static Color RGBf(float r, float g, float b) => RGBAf(r, g, b, 1.0f);

        // Returns a color value from red, green, blue and alpha values.
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

        // Returns a color value from red, green, blue and alpha values.
        public static Color RGBAf(float r, float g, float b, float a)
        {
            return new Color
            {
                R = r,
                G = g,
                B = b,
                A = a,
            };
        }

        // Linearly interpolates from color c0 to c1, and returns resulting color value.
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

        // Sets transparency of a color value.
        public static Color TransRGBA(Color c0, byte a)
        {
            c0.A = a / 255.0f;
            return c0;
        }

        // Sets transparency of a color value.
        public static Color TransRGBAf(Color c0, float a)
        {
            c0.A = a;
            return c0;
        }

        // Returns color value specified by hue, saturation and lightness.
        // HSL values are all in range [0..1], alpha will be set to 255.
        public static Color HSL(float h, float s, float l) => HSLA(h, s, l, 255);

        // Returns color value specified by hue, saturation and lightness and alpha.
        // HSL values are all in range [0..1], alpha in range [0..255]
        public static Color HSLA(float h, float s, float l, byte a)
        {
            float Hue(float hue, float n1, float n2)
            {
                if (hue < 0) hue += 1;
                if (hue > 1) hue -= 1;
                if (hue < 1.0f / 6.0f)
                    return n1 + (n2 - n1) * hue * 6.0f;
                else if (hue < 3.0f / 6.0f)
                    return n2;
                else if (hue < 4.0f / 6.0f)
                    return n1 + (n2 - n1) * (2.0f / 3.0f - hue) * 6.0f;
                return n1;
            }

            float Clamp(float x, float min, float max) => Math.Max(0f, Math.Min(x, 1f));

            float m1, m2;

            h %= 1;
            if (h < 0.0f) h += 1.0f;

            s = Clamp(s, 0.0f, 1.0f);
            l = Clamp(l, 0.0f, 1.0f);
            m2 = l <= 0.5f ? (l * (1 + s)) : (l + s - l * s);
            m1 = 2 * l - m2;

            return new Color
            {
                R = Clamp(Hue(h + 1.0f / 3.0f, m1, m2), 0.0f, 1.0f),
                G = Clamp(Hue(h, m1, m2), 0.0f, 1.0f),
                B = Clamp(Hue(h - 1.0f / 3.0f, m1, m2), 0.0f, 1.0f),
                A = a / 255.0f,
            };
        }

        #endregion

        #region State Handling

        //// NanoVG contains state which represents how paths will be rendered.
        //// The state contains transform, fill and stroke styles, text and font styles,
        //// and scissor clipping.

        // Pushes and saves the current render state into a state stack.
        // A matching nvgRestore() must be used to restore the state.
        public static void Save(Context ctx)
        {
            throw new NotSupportedException();
            /*
            if (ctx->nstates >= NVG_MAX_STATES)
                return;
            if (ctx->nstates > 0)
                memcpy(&ctx->states[ctx->nstates], &ctx->states[ctx->nstates - 1], sizeof(NVGstate));
            ctx->nstates++;
            */
        }

        // Pops and restores current render state.
        public static void Restore(Context ctx)
        {
            throw new NotSupportedException();
            /*
            if (ctx->nstates <= 1)
                return;
            ctx->nstates--;
            */
        }

        // Resets current render state to default values. Does not affect the render state stack.
        public static void Reset(Context ctx)
        {
            throw new NotSupportedException();
            /*
            NVGstate* state = nvg__getState(ctx);
            memset(state, 0, sizeof(*state));

            nvg__setPaintColor(&state->fill, nvgRGBA(255, 255, 255, 255));
            nvg__setPaintColor(&state->stroke, nvgRGBA(0, 0, 0, 255));
            state->compositeOperation = nvg__compositeOperationState(NVG_SOURCE_OVER);
            state->shapeAntiAlias = 1;
            state->strokeWidth = 1.0f;
            state->miterLimit = 10.0f;
            state->lineCap = NVG_BUTT;
            state->lineJoin = NVG_MITER;
            state->alpha = 1.0f;
            nvgTransformIdentity(state->xform);

            state->scissor.extent[0] = -1.0f;
            state->scissor.extent[1] = -1.0f;

            state->fontSize = 16.0f;
            state->letterSpacing = 0.0f;
            state->lineHeight = 1.0f;
            state->fontBlur = 0.0f;
            state->textAlign = NVG_ALIGN_LEFT | NVG_ALIGN_BASELINE;
            state->fontId = 0;
            */
        }

        #endregion

        #region Render styles

        //// Fill and stroke render style can be either a solid color or a paint which is a gradient or a pattern.
        //// Solid color is simply defined as a color value, different kinds of paints can be created
        //// using nvgLinearGradient(), nvgBoxGradient(), nvgRadialGradient() and nvgImagePattern().
        ////
        //// Current render style can be saved and restored using nvgSave() and nvgRestore().

        // Sets whether to draw antialias for nvgStroke() and nvgFill(). It's enabled by default.
        public static void ShapeAntiAlias(Context ctx, int enabled)
        {
            throw new NotImplementedException();
            /*
            NVGstate* state = nvg__getState(ctx);
	        state->shapeAntiAlias = enabled;
            */
        }

        // Sets current stroke style to a solid color.
        public static void StrokeColor(Context ctx, Color color)
        {
            throw new NotImplementedException();
        }

        // Sets current stroke style to a paint, which can be a one of the gradients or a pattern.
        public static void StrokePaint(Context ctx, Paint paint)
        {
            throw new NotImplementedException();
        }

        // Sets current fill style to a solid color.
        public static void FillColor(Context ctx, Color color)
        {
            throw new NotImplementedException();
        }

        // Sets current fill style to a paint, which can be a one of the gradients or a pattern.
        public static void FillPaint(Context ctx, Paint paint)
        {
            throw new NotImplementedException();
        }

        // Sets the miter limit of the stroke style.
        // Miter limit controls when a sharp corner is beveled.
        public static void MiterLimit(Context ctx, float limit)
        {
            throw new NotImplementedException();
            /*
            NVGstate* state = nvg__getState(ctx);
            state->miterLimit = limit;
            */
        }

        // Sets the stroke width of the stroke style.
        public static void StrokeWidth(Context ctx, float size)
        {
            throw new NotImplementedException();
            /*
            NVGstate* state = nvg__getState(ctx);
	        state->strokeWidth = width;
             */
        }

        // Sets how the end of the line (cap) is drawn,
        // Can be one of: NVG_BUTT (default), NVG_ROUND, NVG_SQUARE.
        public static void LineCap(Context ctx, int cap)
        {
            throw new NotImplementedException();
            /*
            NVGstate* state = nvg__getState(ctx);
	        state->lineCap = cap;
             */
        }

        // Sets how sharp path corners are drawn.
        // Can be one of NVG_MITER (default), NVG_ROUND, NVG_BEVEL.
        public static void LineJoin(Context ctx, int join)
        {
            throw new NotImplementedException();
            /*
            NVGstate* state = nvg__getState(ctx);
	        state->lineJoin = join;
            */
        }

        // Sets the transparency applied to all rendered shapes.
        // Already transparent paths will get proportionally more transparent as well.
        public static void GlobalAlpha(Context ctx, float alpha)
        {
            throw new NotImplementedException();
            /*
            NVGstate* state = nvg__getState(ctx);
            state->alpha = alpha;
            */
        }

        #endregion

        #region Transforms

        //// The paths, gradients, patterns and scissor region are transformed by an transformation
        //// matrix at the time when they are passed to the API.
        //// The current transformation matrix is a affine matrix:
        ////   [sx kx tx]
        ////   [ky sy ty]
        ////   [ 0  0  1]
        //// Where: sx,sy define scaling, kx,ky skewing, and tx,ty translation.
        //// The last row is assumed to be 0,0,1 and is not stored.
        ////
        //// Apart from nvgResetTransform(), each transformation function first creates
        //// specific transformation matrix and pre-multiplies the current transformation by it.
        ////
        //// Current coordinate system (transformation) can be saved and restored using nvgSave() and nvgRestore().

        // Resets current transform to a identity matrix.
        public static void ResetTransform(Context ctx)
        {
            throw new NotImplementedException();
            /*
       	    NVGstate* state = nvg__getState(ctx);
	        nvgTransformIdentity(state->xform);     
            */
        }

        // Premultiplies current coordinate system by specified matrix.
        // The parameters are interpreted as matrix as follows:
        //   [a c e]
        //   [b d f]
        //   [0 0 1]
        public static void Transform(Context ctx, float a, float b, float c, float d, float e, float f)
        {
            throw new NotImplementedException();
            /*
	        NVGstate* state = nvg__getState(ctx);
	        float t[6] = { a, b, c, d, e, f };
	        nvgTransformPremultiply(state->xform, t);
            */
        }

        // Translates current coordinate system.
        public static void Translate(Context ctx, float x, float y)
        {
            throw new NotImplementedException();
            /*
            NVGstate* state = nvg__getState(ctx);
	        float t[6];
	        nvgTransformTranslate(t, x,y);
	        nvgTransformPremultiply(state->xform, t);
            */
        }

        // Rotates current coordinate system. Angle is specified in radians.
        public static void Rotate(Context ctx, float angle)
        {
            throw new NotImplementedException();
            /*
            NVGstate* state = nvg__getState(ctx);
	        float t[6];
	        nvgTransformRotate(t, angle);
	        nvgTransformPremultiply(state->xform, t);
            */
        }

        // Skews the current coordinate system along X axis. Angle is specified in radians.
        public static void SkewX(Context ctx, float angle)
        {
            throw new NotImplementedException();
            /*
            NVGstate* state = nvg__getState(ctx);
	        float t[6];
	        nvgTransformSkewX(t, angle);
	        nvgTransformPremultiply(state->xform, t);
            */
        }

        // Skews the current coordinate system along Y axis. Angle is specified in radians.
        public static void SkewY(Context ctx, float angle)
        {
            throw new NotImplementedException();
            /*
            NVGstate* state = nvg__getState(ctx);
	        float t[6];
	        nvgTransformSkewY(t, angle);
	        nvgTransformPremultiply(state->xform, t);
            */
        }

        // Scales the current coordinate system.
        public static void Scale(Context ctx, float x, float y)
        {
            throw new NotImplementedException();
            /*
            NVGstate* state = nvg__getState(ctx);
	        float t[6];
	        nvgTransformScale(t, x,y);
	        nvgTransformPremultiply(state->xform, t);
            */
        }

        // Stores the top part (a-f) of the current transformation matrix in to the specified buffer.
        //   [a c e]
        //   [b d f]
        //   [0 0 1]
        // There should be space for 6 floats in the return buffer for the values a-f.
        public static void CurrentTransform(Context ctx, float[] xform)
        {
            throw new NotImplementedException();
            /*
            NVGstate* state = nvg__getState(ctx);
	        if (xform == NULL) return;
	        memcpy(xform, state->xform, sizeof(float)*6);
            */
        }


        // The following functions can be used to make calculations on 2x3 transformation matrices.
        // A 2x3 matrix is represented as float[6].

        // Sets the transform to identity matrix.
        public static void TransformIdentity(float[] dst)
        {
            dst[0] = 1.0f; dst[1] = 0.0f;
            dst[2] = 0.0f; dst[3] = 1.0f;
            dst[4] = 0.0f; dst[5] = 0.0f;
        }

        // Sets the transform to translation matrix matrix.
        public static void TransformTranslate(float[] dst, float tx, float ty)
        {
            dst[0] = 1.0f; dst[1] = 0.0f;
            dst[2] = 0.0f; dst[3] = 1.0f;
            dst[4] = tx; dst[5] = ty;
        }

        // Sets the transform to scale matrix.
        public static void TransformScale(float[] dst, float sx, float sy)
        {
            dst[0] = sx; dst[1] = 0.0f;
            dst[2] = 0.0f; dst[3] = sy;
            dst[4] = 0.0f; dst[5] = 0.0f;
        }

        // Sets the transform to rotate matrix. Angle is specified in radians.
        public static void TransformRotate(float[] dst, float a)
        {
            float cs = (float)Math.Cos(a);
            float sn = (float)Math.Sin(a);
            dst[0] = cs; dst[1] = sn;
            dst[2] = -sn; dst[3] = cs;
            dst[4] = 0.0f; dst[5] = 0.0f;
        }

        // Sets the transform to skew-x matrix. Angle is specified in radians.
        public static void TransformSkewX(float[] dst, float a)
        {
            dst[0] = 1.0f; dst[1] = 0.0f;
            dst[2] = (float)Math.Tan(a); dst[3] = 1.0f;
            dst[4] = 0.0f; dst[5] = 0.0f;
        }

        // Sets the transform to skew-y matrix. Angle is specified in radians.
        public static void TransformSkewY(float[] dst, float a)
        {
            dst[0] = 1.0f; dst[1] = (float)Math.Tan(a);
            dst[2] = 0.0f; dst[3] = 1.0f;
            dst[4] = 0.0f; dst[5] = 0.0f;
        }

        // Sets the transform to the result of multiplication of two transforms, of A = A*B.
        public static void TransformMultiply(float[] dst, float[] src)
        {
            float t0 = dst[0] * src[0] + dst[1] * src[2];
            float t2 = dst[2] * src[0] + dst[3] * src[2];
            float t4 = dst[4] * src[0] + dst[5] * src[2] + src[4];
            dst[1] = dst[0] * src[1] + dst[1] * src[3];
            dst[3] = dst[2] * src[1] + dst[3] * src[3];
            dst[5] = dst[4] * src[1] + dst[5] * src[3] + src[5];
            dst[0] = t0;
            dst[2] = t2;
            dst[4] = t4;
        }

        // Sets the transform to the result of multiplication of two transforms, of A = B*A.
        public static void TransformPremultiply(float[] dst, float[] src)
        {
            float[] s2 = new float[6];
            Array.Copy(src, s2, 6);
            TransformMultiply(s2, dst);
            Array.Copy(s2, dst, 6);
        }

        // Sets the destination to inverse of specified transform.
        // Returns 1 if the inverse could be calculated, else 0.
        public static int TransformInverse(float[] dst, float[] src)
        {
            double invdet, det = (double)src[0] * src[3] - (double)src[2] * src[1];
            if (det > -1e-6 && det < 1e-6)
            {
                TransformIdentity(dst);
                return 0;
            }
            invdet = 1.0 / det;
            dst[0] = (float)(src[3] * invdet);
            dst[2] = (float)(-src[2] * invdet);
            dst[4] = (float)(((double)src[2] * src[5] - (double)src[3] * src[4]) * invdet);
            dst[1] = (float)(-src[1] * invdet);
            dst[3] = (float)(src[0] * invdet);
            dst[5] = (float)(((double)src[1] * src[4] - (double)src[0] * src[5]) * invdet);
            return 1;
        }

        // Transform a point by given transform.
        public static void TransformPoint(out float dx, out float dy, float[] t, float sx, float sy)
        {
            dx = sx * t[0] + sy * t[2] + t[4];
            dy = sx * t[1] + sy * t[3] + t[5];
        }

        // Converts degrees to radians and vice versa.
        public static float DegToRad(float deg) => deg / 180.0f * (float)Math.PI;

        public static float RadToDeg(float rad) => rad / (float)Math.PI * 180.0f;

        #endregion

        #region Images

        //// NanoVG allows you to load jpg, png, psd, tga, pic and gif files to be used for rendering.
        //// In addition you can upload your own image. The image loading is provided by stb_image.
        //// The parameter imageFlags is combination of flags defined in NVGimageFlags.

        // Creates image by loading it from the disk from specified file name.
        // Returns handle to the image.
        public static int CreateImage(Context ctx, string filename, int imageFlags)
        {
            /*
            int w, h, n, image;
            unsigned char* img;
            stbi_set_unpremultiply_on_load(1);
            stbi_convert_iphone_png_to_rgb(1);
            img = stbi_load(filename, &w, &h, &n, 4);
            if (img == NULL)
            {
                //		printf("Failed to load %s - %s\n", filename, stbi_failure_reason());
                return 0;
            }
            image = nvgCreateImageRGBA(ctx, w, h, imageFlags, img);
            stbi_image_free(img);
            return image;
            */
            return 0;
        }

        // Creates image by loading it from the specified chunk of memory.
        // Returns handle to the image.
        public static int CreateImageMem(Context ctx, int imageFlags, byte[] data, int ndata)
        {
            /*
            int w, h, n, image;
            unsigned char* img = stbi_load_from_memory(data, ndata, &w, &h, &n, 4);
            if (img == NULL)
            {
                //		printf("Failed to load %s - %s\n", filename, stbi_failure_reason());
                return 0;
            }
            image = nvgCreateImageRGBA(ctx, w, h, imageFlags, img);
            stbi_image_free(img);
            return image;
            */
            return 0;
        }

        // Creates image from specified image data.
        // Returns handle to the image.
        public static int CreateImageRGBA(Context ctx, int w, int h, int imageFlags, byte[] data)
        {
            // return ctx->params.renderCreateTexture(ctx->params.userPtr, NVG_TEXTURE_RGBA, w, h, imageFlags, data);
            return 0;
        }

        // Updates image data specified by image handle.
        public static void UpdateImage(Context ctx, int image, byte[] data)
        {
            /*
            int w, h;
            ctx->params.renderGetTextureSize(ctx->params.userPtr, image, &w, &h);
            ctx->params.renderUpdateTexture(ctx->params.userPtr, image, 0,0, w, h, data);
            */
        }

        // Returns the dimensions of a created image.
        public static void ImageSize(Context ctx, int image, out int w, out int h)
        {
            // ctx->params.renderGetTextureSize(ctx->params.userPtr, image, w, h);
        }

        // Deletes created image.
        public static void DeleteImage(Context ctx, int image)
        {
            // ctx->params.renderGetTextureSize(ctx->params.userPtr, image, w, h);
        }

        #endregion

        #region Paints

        //// NanoVG supports four types of paints: linear gradient, box gradient, radial gradient and image pattern.
        //// These can be used as paints for strokes and fills.

        // Creates and returns a linear gradient. Parameters (sx,sy)-(ex,ey) specify the start and end coordinates
        // of the linear gradient, icol specifies the start color and ocol the end color.
        // The gradient is transformed by the current transform when it is passed to nvgFillPaint() or nvgStrokePaint().
        public static Paint LinearGradient(Context ctx, float sx, float sy, float ex, float ey, Color icol, Color ocol)
        {
            throw new NotImplementedException();
            /*
            NVGpaint p;
	        float dx, dy, d;
	        const float large = 1e5;
	        NVG_NOTUSED(ctx);
	        memset(&p, 0, sizeof(p));

	        // Calculate transform aligned to the line
	        dx = ex - sx;
	        dy = ey - sy;
	        d = sqrtf(dx*dx + dy*dy);
	        if (d > 0.0001f) {
		        dx /= d;
		        dy /= d;
	        } else {
		        dx = 0;
		        dy = 1;
	        }

	        p.xform[0] = dy; p.xform[1] = -dx;
	        p.xform[2] = dx; p.xform[3] = dy;
	        p.xform[4] = sx - dx*large; p.xform[5] = sy - dy*large;

	        p.extent[0] = large;
	        p.extent[1] = large + d*0.5f;

	        p.radius = 0.0f;

	        p.feather = nvg__maxf(1.0f, d);

	        p.innerColor = icol;
	        p.outerColor = ocol;

	        return p;
            */
        }

        // Creates and returns a box gradient. Box gradient is a feathered rounded rectangle, it is useful for rendering
        // drop shadows or highlights for boxes. Parameters (x,y) define the top-left corner of the rectangle,
        // (w,h) define the size of the rectangle, r defines the corner radius, and f feather. Feather defines how blurry
        // the border of the rectangle is. Parameter icol specifies the inner color and ocol the outer color of the gradient.
        // The gradient is transformed by the current transform when it is passed to nvgFillPaint() or nvgStrokePaint().
        public static Paint BoxGradient(Context ctx, float x, float y, float w, float h, float r, float f, Color icol, Color ocol)
        {
            throw new NotImplementedException();
            /*
            NVGpaint p;
            NVG_NOTUSED(ctx);
            memset(&p, 0, sizeof(p));

            nvgTransformIdentity(p.xform);
            p.xform[4] = x + w * 0.5f;
            p.xform[5] = y + h * 0.5f;

            p.extent[0] = w * 0.5f;
            p.extent[1] = h * 0.5f;

            p.radius = r;

            p.feather = nvg__maxf(1.0f, f);

            p.innerColor = icol;
            p.outerColor = ocol;

            return p;
            */
        }

        // Creates and returns a radial gradient. Parameters (cx,cy) specify the center, inr and outr specify
        // the inner and outer radius of the gradient, icol specifies the start color and ocol the end color.
        // The gradient is transformed by the current transform when it is passed to nvgFillPaint() or nvgStrokePaint().
        public static Paint RadialGradient(Context ctx, float cx, float cy, float inr, float outr, Color icol, Color ocol)
        {
            throw new NotImplementedException();
            /*
            NVGpaint p;
	        float r = (inr+outr)*0.5f;
	        float f = (outr-inr);
	        NVG_NOTUSED(ctx);
	        memset(&p, 0, sizeof(p));

	        nvgTransformIdentity(p.xform);
	        p.xform[4] = cx;
	        p.xform[5] = cy;

	        p.extent[0] = r;
	        p.extent[1] = r;

	        p.radius = r;

	        p.feather = nvg__maxf(1.0f, f);

	        p.innerColor = icol;
	        p.outerColor = ocol;

	        return p;
            */
        }

        // Creates and returns an image patter. Parameters (ox,oy) specify the left-top location of the image pattern,
        // (ex,ey) the size of one image, angle rotation around the top-left corner, image is handle to the image to render.
        // The gradient is transformed by the current transform when it is passed to nvgFillPaint() or nvgStrokePaint().
        public static Paint ImagePattern(Context ctx, float ox, float oy, float ex, float ey, float angle, int image, float alpha)
        {
            throw new NotImplementedException();
            /*
            NVGpaint p;
            NVG_NOTUSED(ctx);
            memset(&p, 0, sizeof(p));

            nvgTransformRotate(p.xform, angle);
            p.xform[4] = cx;
            p.xform[5] = cy;

            p.extent[0] = w;
            p.extent[1] = h;

            p.image = image;

            p.innerColor = p.outerColor = nvgRGBAf(1, 1, 1, alpha);

            return p;
            */
        }

        #endregion

        #region Scissoring

        //// Scissoring allows you to clip the rendering into a rectangle. This is useful for various
        //// user interface cases like rendering a text edit or a timeline.

        // Sets the current scissor rectangle.
        // The scissor rectangle is transformed by the current transform.
        public static void Scissor(Context ctx, float x, float y, float w, float h)
        {
            throw new NotImplementedException();
            /*
            NVGstate* state = nvg__getState(ctx);

            w = nvg__maxf(0.0f, w);
            h = nvg__maxf(0.0f, h);

            nvgTransformIdentity(state->scissor.xform);
            state->scissor.xform[4] = x + w * 0.5f;
            state->scissor.xform[5] = y + h * 0.5f;
            nvgTransformMultiply(state->scissor.xform, state->xform);

            state->scissor.extent[0] = w * 0.5f;
            state->scissor.extent[1] = h * 0.5f;
            */
        }

        // Intersects current scissor rectangle with the specified rectangle.
        // The scissor rectangle is transformed by the current transform.
        // Note: in case the rotation of previous scissor rect differs from
        // the current one, the intersection will be done between the specified
        // rectangle and the previous scissor rectangle transformed in the current
        // transform space. The resulting shape is always rectangle.
        public static void IntersectScissor(Context ctx, float x, float y, float w, float h)
        {
            throw new NotImplementedException();
            /*
            NVGstate* state = nvg__getState(ctx);
            float pxform[6], invxorm[6];
            float rect[4];
            float ex, ey, tex, tey;

            // If no previous scissor has been set, set the scissor as current scissor.
            if (state->scissor.extent[0] < 0)
            {
                nvgScissor(ctx, x, y, w, h);
                return;
            }

            // Transform the current scissor rect into current transform space.
            // If there is difference in rotation, this will be approximation.
            memcpy(pxform, state->scissor.xform, sizeof(float) * 6);
            ex = state->scissor.extent[0];
            ey = state->scissor.extent[1];
            nvgTransformInverse(invxorm, state->xform);
            nvgTransformMultiply(pxform, invxorm);
            tex = ex * nvg__absf(pxform[0]) + ey * nvg__absf(pxform[2]);
            tey = ex * nvg__absf(pxform[1]) + ey * nvg__absf(pxform[3]);

            // Intersect rects.
            nvg__isectRects(rect, pxform[4] - tex, pxform[5] - tey, tex * 2, tey * 2, x, y, w, h);

            nvgScissor(ctx, rect[0], rect[1], rect[2], rect[3]);
            */
        }

        // Reset and disables scissoring.
        public static void ResetScissor(Context ctx)
        {
            throw new NotImplementedException();
            /*
            NVGstate* state = nvg__getState(ctx);
	        memset(state->scissor.xform, 0, sizeof(state->scissor.xform));
	        state->scissor.extent[0] = -1.0f;
	        state->scissor.extent[1] = -1.0f;
            */
        }

        #endregion

        #region Paths

        // Drawing a new shape starts with nvgBeginPath(), it clears all the currently defined paths.
        // Then you define one or more paths and sub-paths which describe the shape. The are functions
        // to draw common shapes like rectangles and circles, and lower level step-by-step functions,
        // which allow to define a path curve by curve.
        //
        // NanoVG uses even-odd fill rule to draw the shapes. Solid shapes should have counter clockwise
        // winding and holes should have counter clockwise order. To specify winding of a path you can
        // call nvgPathWinding(). This is useful especially for the common shapes, which are drawn CCW.
        //
        // Finally you can fill the path using current fill style by calling nvgFill(), and stroke it
        // with current stroke style by calling nvgStroke().
        //
        // The curve segments and sub-paths are transformed by the current transform.

        // Clears the current path and sub-paths.
        public static void BeginPath(Context ctx)
        {
            throw new NotImplementedException();
            /*
            ctx->ncommands = 0;
            nvg__clearPathCache(ctx);
            */
        }

        // Starts new sub-path with specified point as first point.
        public static void MoveTo(Context ctx, float x, float y)
        {
            throw new NotImplementedException();
            /*
	float vals[] = { NVG_MOVETO, x, y };
	nvg__appendCommands(ctx, vals, NVG_COUNTOF(vals));
*/
        }

        // Adds line segment from the last point in the path to the specified point.
        public static void LineTo(Context ctx, float x, float y)
        {
            throw new NotImplementedException();
            /*
	float vals[] = { NVG_LINETO, x, y };
	nvg__appendCommands(ctx, vals, NVG_COUNTOF(vals));
*/
        }

        // Adds cubic bezier segment from last point in the path via two control points to the specified point.
        public static void BezierTo(Context ctx, float c1x, float c1y, float c2x, float c2y, float x, float y)
        {
            throw new NotImplementedException();
            /*
	float vals[] = { NVG_BEZIERTO, c1x, c1y, c2x, c2y, x, y };
	nvg__appendCommands(ctx, vals, NVG_COUNTOF(vals));
*/
        }

        // Adds quadratic bezier segment from last point in the path via a control point to the specified point.
        public static void QuadTo(Context ctx, float cx, float cy, float x, float y)
        {
            throw new NotImplementedException();
            /*
    float x0 = ctx->commandx;
    float y0 = ctx->commandy;
    float vals[] = { NVG_BEZIERTO,
        x0 + 2.0f/3.0f*(cx - x0), y0 + 2.0f/3.0f*(cy - y0),
        x + 2.0f/3.0f*(cx - x), y + 2.0f/3.0f*(cy - y),
        x, y };
    nvg__appendCommands(ctx, vals, NVG_COUNTOF(vals));
*/
        }

        // Adds an arc segment at the corner defined by the last path point, and two specified points.
        public static void ArcTo(Context ctx, float x1, float y1, float x2, float y2, float radius)
        {
            throw new NotImplementedException();
            /*
	float x0 = ctx->commandx;
	float y0 = ctx->commandy;
	float dx0,dy0, dx1,dy1, a, d, cx,cy, a0,a1;
	int dir;

	if (ctx->ncommands == 0) {
		return;
	}

	// Handle degenerate cases.
	if (nvg__ptEquals(x0,y0, x1,y1, ctx->distTol) ||
		nvg__ptEquals(x1,y1, x2,y2, ctx->distTol) ||
		nvg__distPtSeg(x1,y1, x0,y0, x2,y2) < ctx->distTol*ctx->distTol ||
		radius < ctx->distTol) {
		nvgLineTo(ctx, x1,y1);
		return;
	}

	// Calculate tangential circle to lines (x0,y0)-(x1,y1) and (x1,y1)-(x2,y2).
	dx0 = x0-x1;
	dy0 = y0-y1;
	dx1 = x2-x1;
	dy1 = y2-y1;
	nvg__normalize(&dx0,&dy0);
	nvg__normalize(&dx1,&dy1);
	a = nvg__acosf(dx0*dx1 + dy0*dy1);
	d = radius / nvg__tanf(a/2.0f);

//	printf("a=%f° d=%f\n", a/NVG_PI*180.0f, d);

	if (d > 10000.0f) {
		nvgLineTo(ctx, x1,y1);
		return;
	}

	if (nvg__cross(dx0,dy0, dx1,dy1) > 0.0f) {
		cx = x1 + dx0*d + dy0*radius;
		cy = y1 + dy0*d + -dx0*radius;
		a0 = nvg__atan2f(dx0, -dy0);
		a1 = nvg__atan2f(-dx1, dy1);
		dir = NVG_CW;
//		printf("CW c=(%f, %f) a0=%f° a1=%f°\n", cx, cy, a0/NVG_PI*180.0f, a1/NVG_PI*180.0f);
	} else {
		cx = x1 + dx0*d + -dy0*radius;
		cy = y1 + dy0*d + dx0*radius;
		a0 = nvg__atan2f(-dx0, dy0);
		a1 = nvg__atan2f(dx1, -dy1);
		dir = NVG_CCW;
//		printf("CCW c=(%f, %f) a0=%f° a1=%f°\n", cx, cy, a0/NVG_PI*180.0f, a1/NVG_PI*180.0f);
	}

	nvgArc(ctx, cx, cy, radius, a0, a1, dir);
*/
        }

        // Closes current sub-path with a line segment.
        public static void ClosePath(Context ctx)
        {
            throw new NotImplementedException();
            /*
	float vals[] = { NVG_CLOSE };
	nvg__appendCommands(ctx, vals, NVG_COUNTOF(vals));
*/
        }

        // Sets the current sub-path winding, see NVGwinding and NVGsolidity. 
        public static void PathWinding(Context ctx, int dir)
        {
            throw new NotImplementedException();
            /*
	float vals[] = { NVG_WINDING, (float)dir };
	nvg__appendCommands(ctx, vals, NVG_COUNTOF(vals));
*/
        }

        // Creates new circle arc shaped sub-path. The arc center is at cx,cy, the arc radius is r,
        // and the arc is drawn from angle a0 to a1, and swept in direction dir (NVG_CCW, or NVG_CW).
        // Angles are specified in radians.
        public static void Arc(Context ctx, float cx, float cy, float r, float a0, float a1, int dir)
        {
            throw new NotImplementedException();
            /*
	float a = 0, da = 0, hda = 0, kappa = 0;
	float dx = 0, dy = 0, x = 0, y = 0, tanx = 0, tany = 0;
	float px = 0, py = 0, ptanx = 0, ptany = 0;
	float vals[3 + 5*7 + 100];
	int i, ndivs, nvals;
	int move = ctx->ncommands > 0 ? NVG_LINETO : NVG_MOVETO;

	// Clamp angles
	da = a1 - a0;
	if (dir == NVG_CW) {
		if (nvg__absf(da) >= NVG_PI*2) {
			da = NVG_PI*2;
		} else {
			while (da < 0.0f) da += NVG_PI*2;
		}
	} else {
		if (nvg__absf(da) >= NVG_PI*2) {
			da = -NVG_PI*2;
		} else {
			while (da > 0.0f) da -= NVG_PI*2;
		}
	}

	// Split arc into max 90 degree segments.
	ndivs = nvg__maxi(1, nvg__mini((int)(nvg__absf(da) / (NVG_PI*0.5f) + 0.5f), 5));
	hda = (da / (float)ndivs) / 2.0f;
	kappa = nvg__absf(4.0f / 3.0f * (1.0f - nvg__cosf(hda)) / nvg__sinf(hda));

	if (dir == NVG_CCW)
		kappa = -kappa;

	nvals = 0;
	for (i = 0; i <= ndivs; i++) {
		a = a0 + da * (i/(float)ndivs);
		dx = nvg__cosf(a);
		dy = nvg__sinf(a);
		x = cx + dx*r;
		y = cy + dy*r;
		tanx = -dy*r*kappa;
		tany = dx*r*kappa;

		if (i == 0) {
			vals[nvals++] = (float)move;
			vals[nvals++] = x;
			vals[nvals++] = y;
		} else {
			vals[nvals++] = NVG_BEZIERTO;
			vals[nvals++] = px+ptanx;
			vals[nvals++] = py+ptany;
			vals[nvals++] = x-tanx;
			vals[nvals++] = y-tany;
			vals[nvals++] = x;
			vals[nvals++] = y;
		}
		px = x;
		py = y;
		ptanx = tanx;
		ptany = tany;
	}

	nvg__appendCommands(ctx, vals, nvals);
*/
        }

        // Creates new rectangle shaped sub-path.
        public static void Rect(Context ctx, float x, float y, float w, float h)
        {
            throw new NotImplementedException();
            /*
	float vals[] = {
		NVG_MOVETO, x,y,
		NVG_LINETO, x,y+h,
		NVG_LINETO, x+w,y+h,
		NVG_LINETO, x+w,y,
		NVG_CLOSE
	};
	nvg__appendCommands(ctx, vals, NVG_COUNTOF(vals));
*/
        }

        // Creates new rounded rectangle shaped sub-path.
        public static void RoundedRect(Context ctx, float x, float y, float w, float h, float r)
        {
            throw new NotImplementedException();
            /*
	nvgRoundedRectVarying(ctx, x, y, w, h, r, r, r, r);
*/
        }

        // Creates new rounded rectangle shaped sub-path with varying radii for each corner.
        public static void RoundedRectVarying(Context ctx, float x, float y, float w, float h, float radTopLeft, float radTopRight, float radBottomRight, float radBottomLeft)
        {
            throw new NotImplementedException();
            /*
	if(radTopLeft < 0.1f && radTopRight < 0.1f && radBottomRight < 0.1f && radBottomLeft < 0.1f) {
		nvgRect(ctx, x, y, w, h);
		return;
	} else {
		float halfw = nvg__absf(w)*0.5f;
		float halfh = nvg__absf(h)*0.5f;
		float rxBL = nvg__minf(radBottomLeft, halfw) * nvg__signf(w), ryBL = nvg__minf(radBottomLeft, halfh) * nvg__signf(h);
		float rxBR = nvg__minf(radBottomRight, halfw) * nvg__signf(w), ryBR = nvg__minf(radBottomRight, halfh) * nvg__signf(h);
		float rxTR = nvg__minf(radTopRight, halfw) * nvg__signf(w), ryTR = nvg__minf(radTopRight, halfh) * nvg__signf(h);
		float rxTL = nvg__minf(radTopLeft, halfw) * nvg__signf(w), ryTL = nvg__minf(radTopLeft, halfh) * nvg__signf(h);
		float vals[] = {
			NVG_MOVETO, x, y + ryTL,
			NVG_LINETO, x, y + h - ryBL,
			NVG_BEZIERTO, x, y + h - ryBL*(1 - NVG_KAPPA90), x + rxBL*(1 - NVG_KAPPA90), y + h, x + rxBL, y + h,
			NVG_LINETO, x + w - rxBR, y + h,
			NVG_BEZIERTO, x + w - rxBR*(1 - NVG_KAPPA90), y + h, x + w, y + h - ryBR*(1 - NVG_KAPPA90), x + w, y + h - ryBR,
			NVG_LINETO, x + w, y + ryTR,
			NVG_BEZIERTO, x + w, y + ryTR*(1 - NVG_KAPPA90), x + w - rxTR*(1 - NVG_KAPPA90), y, x + w - rxTR, y,
			NVG_LINETO, x + rxTL, y,
			NVG_BEZIERTO, x + rxTL*(1 - NVG_KAPPA90), y, x, y + ryTL*(1 - NVG_KAPPA90), x, y + ryTL,
			NVG_CLOSE
		};
		nvg__appendCommands(ctx, vals, NVG_COUNTOF(vals));
	}
*/
        }

        // Creates new ellipse shaped sub-path.
        public static void Ellipse(Context ctx, float cx, float cy, float rx, float ry)
        {
            throw new NotImplementedException();
            /*
	float vals[] = {
		NVG_MOVETO, cx-rx, cy,
		NVG_BEZIERTO, cx-rx, cy+ry*NVG_KAPPA90, cx-rx*NVG_KAPPA90, cy+ry, cx, cy+ry,
		NVG_BEZIERTO, cx+rx*NVG_KAPPA90, cy+ry, cx+rx, cy+ry*NVG_KAPPA90, cx+rx, cy,
		NVG_BEZIERTO, cx+rx, cy-ry*NVG_KAPPA90, cx+rx*NVG_KAPPA90, cy-ry, cx, cy-ry,
		NVG_BEZIERTO, cx-rx*NVG_KAPPA90, cy-ry, cx-rx, cy-ry*NVG_KAPPA90, cx-rx, cy,
		NVG_CLOSE
	};
	nvg__appendCommands(ctx, vals, NVG_COUNTOF(vals));
*/
        }

        // Creates new circle shaped sub-path. 
        public static void Circle(Context ctx, float cx, float cy, float r)
        {
            throw new NotImplementedException();
            /*
	nvgEllipse(ctx, cx,cy, r,r);
*/
        }

        // Fills the current path with current fill style.
        public static void Fill(Context ctx)
        {
            throw new NotImplementedException();
            /*
	NVGstate* state = nvg__getState(ctx);
	const NVGpath* path;
	NVGpaint fillPaint = state->fill;
	int i;

	nvg__flattenPaths(ctx);
	if (ctx->params.edgeAntiAlias && state->shapeAntiAlias)
		nvg__expandFill(ctx, ctx->fringeWidth, NVG_MITER, 2.4f);
	else
		nvg__expandFill(ctx, 0.0f, NVG_MITER, 2.4f);

	// Apply global alpha
	fillPaint.innerColor.a *= state->alpha;
	fillPaint.outerColor.a *= state->alpha;

	ctx->params.renderFill(ctx->params.userPtr, &fillPaint, state->compositeOperation, &state->scissor, ctx->fringeWidth,
						   ctx->cache->bounds, ctx->cache->paths, ctx->cache->npaths);

	// Count triangles
	for (i = 0; i < ctx->cache->npaths; i++) {
		path = &ctx->cache->paths[i];
		ctx->fillTriCount += path->nfill-2;
		ctx->fillTriCount += path->nstroke-2;
		ctx->drawCallCount += 2;
	}
*/
        }

        // Fills the current path with current stroke style.
        public static void nvgStroke(Context ctx)
        {
            throw new NotImplementedException();
            /*
	NVGstate* state = nvg__getState(ctx);
	float scale = nvg__getAverageScale(state->xform);
	float strokeWidth = nvg__clampf(state->strokeWidth * scale, 0.0f, 200.0f);
	NVGpaint strokePaint = state->stroke;
	const NVGpath* path;
	int i;


	if (strokeWidth < ctx->fringeWidth) {
		// If the stroke width is less than pixel size, use alpha to emulate coverage.
		// Since coverage is area, scale by alpha*alpha.
		float alpha = nvg__clampf(strokeWidth / ctx->fringeWidth, 0.0f, 1.0f);
		strokePaint.innerColor.a *= alpha*alpha;
		strokePaint.outerColor.a *= alpha*alpha;
		strokeWidth = ctx->fringeWidth;
	}

	// Apply global alpha
	strokePaint.innerColor.a *= state->alpha;
	strokePaint.outerColor.a *= state->alpha;

	nvg__flattenPaths(ctx);

	if (ctx->params.edgeAntiAlias && state->shapeAntiAlias)
		nvg__expandStroke(ctx, strokeWidth*0.5f, ctx->fringeWidth, state->lineCap, state->lineJoin, state->miterLimit);
	else
		nvg__expandStroke(ctx, strokeWidth*0.5f, 0.0f, state->lineCap, state->lineJoin, state->miterLimit);

	ctx->params.renderStroke(ctx->params.userPtr, &strokePaint, state->compositeOperation, &state->scissor, ctx->fringeWidth,
							 strokeWidth, ctx->cache->paths, ctx->cache->npaths);

	// Count triangles
	for (i = 0; i < ctx->cache->npaths; i++) {
		path = &ctx->cache->paths[i];
		ctx->strokeTriCount += path->nstroke-2;
		ctx->drawCallCount++;
	}
*/
        }

        #endregion

        #region Text
        //
        // NanoVG allows you to load .ttf files and use the font to render text.
        //
        // The appearance of the text can be defined by setting the current text style
        // and by specifying the fill color. Common text and font settings such as
        // font size, letter spacing and text align are supported. Font blur allows you
        // to create simple text effects such as drop shadows.
        //
        // At render time the font face can be set based on the font handles or name.
        //
        // Font measure functions return values in local space, the calculations are
        // carried in the same resolution as the final rendering. This is done because
        // the text glyph positions are snapped to the nearest pixels sharp rendering.
        //
        // The local space means that values are not rotated or scale as per the current
        // transformation. For example if you set font size to 12, which would mean that
        // line height is 16, then regardless of the current scaling and rotation, the
        // returned line height is always 16. Some measures may vary because of the scaling
        // since aforementioned pixel snapping.
        //
        // While this may sound a little odd, the setup allows you to always render the
        // same way regardless of scaling. I.e. following works regardless of scaling:
        //
        //		const char* txt = "Text me up.";
        //		nvgTextBounds(vg, x,y, txt, NULL, bounds);
        //		nvgBeginPath(vg);
        //		nvgRoundedRect(vg, bounds[0],bounds[1], bounds[2]-bounds[0], bounds[3]-bounds[1]);
        //		nvgFill(vg);
        //
        // Note: currently only solid color fill is supported for text.

        // Creates font by loading it from the disk from specified file name.
        // Returns handle to the font.
        public static int CreateFont(Context ctx, string name, string filename)
        {
            throw new NotImplementedException();
            /*
	return fonsAddFont(ctx->fs, name, path);
            */
        }

        // Creates font by loading it from the specified memory chunk.
        // Returns handle to the font.
        public static int CreateFontMem(Context ctx, string name, byte[] data, int ndata, int freeData)
        {
            throw new NotImplementedException();
            /*
	return fonsAddFontMem(ctx->fs, name, data, ndata, freeData);
            */
        }

        // Finds a loaded font of specified name, and returns handle to it, or -1 if the font is not found.
        public static int FindFont(Context ctx, string name)
        {
            throw new NotImplementedException();
            /*
	if (name == NULL) return -1;
	return fonsGetFontByName(ctx->fs, name);
            */
        }

        // Adds a fallback font by handle.
        public static int AddFallbackFontId(Context ctx, int baseFont, int fallbackFont)
        {
            throw new NotImplementedException();
            /*
	if(baseFont == -1 || fallbackFont == -1) return 0;
	return fonsAddFallbackFont(ctx->fs, baseFont, fallbackFont);
            */
        }

        // Adds a fallback font by name.
        public static int AddFallbackFont(Context ctx, string baseFont, string fallbackFont)
        {
            throw new NotImplementedException();
            /*
	return nvgAddFallbackFontId(ctx, nvgFindFont(ctx, baseFont), nvgFindFont(ctx, fallbackFont));
            */
        }

        // Sets the font size of current text style.
        public static void FontSize(Context ctx, float size)
        {
            throw new NotImplementedException();
            /*
	NVGstate* state = nvg__getState(ctx);
	state->fontSize = size;
            */
        }

        // Sets the blur of current text style.
        public static void FontBlur(Context ctx, float blur)
        {
            throw new NotImplementedException();
            /*
	NVGstate* state = nvg__getState(ctx);
	state->fontBlur = blur;
            */
        }

        // Sets the letter spacing of current text style.
        public static void TextLetterSpacing(Context ctx, float spacing)
        {
            throw new NotImplementedException();
            /*
	NVGstate* state = nvg__getState(ctx);
	state->letterSpacing = spacing;
            */
        }

        // Sets the proportional line height of current text style. The line height is specified as multiple of font size. 
        public static void TextLineHeight(Context ctx, float lineHeight)
        {
            throw new NotImplementedException();
            /*
	NVGstate* state = nvg__getState(ctx);
	state->lineHeight = lineHeight;
            */
        }

        // Sets the text align of current text style, see NVGalign for options.
        public static void TextAlign(Context ctx, int align)
        {
            throw new NotImplementedException();
            /*
	NVGstate* state = nvg__getState(ctx);
	state->textAlign = align;
            */
        }

        // Sets the font face based on specified id of current text style.
        public static void FontFaceId(Context ctx, int font)
        {
            throw new NotImplementedException();
            /*
	NVGstate* state = nvg__getState(ctx);
	state->fontId = font;
            */
        }

        // Sets the font face based on specified name of current text style.
        public static void FontFace(Context ctx, string font)
        {
            throw new NotImplementedException();
            /*
	NVGstate* state = nvg__getState(ctx);
	state->fontId = fonsGetFontByName(ctx->fs, font);
            */
        }

        // Draws text string at specified location. If end is specified only the sub-string up to the end is drawn.
        public static float Text(Context ctx, float x, float y, string @string, int chars)
        {
            throw new NotImplementedException();
            /*
	NVGstate* state = nvg__getState(ctx);
	FONStextIter iter, prevIter;
	FONSquad q;
	NVGvertex* verts;
	float scale = nvg__getFontScale(state) * ctx->devicePxRatio;
	float invscale = 1.0f / scale;
	int cverts = 0;
	int nverts = 0;

	if (end == NULL)
		end = string + strlen(string);

	if (state->fontId == FONS_INVALID) return x;

	fonsSetSize(ctx->fs, state->fontSize*scale);
	fonsSetSpacing(ctx->fs, state->letterSpacing*scale);
	fonsSetBlur(ctx->fs, state->fontBlur*scale);
	fonsSetAlign(ctx->fs, state->textAlign);
	fonsSetFont(ctx->fs, state->fontId);

	cverts = nvg__maxi(2, (int)(end - string)) * 6; // conservative estimate.
	verts = nvg__allocTempVerts(ctx, cverts);
	if (verts == NULL) return x;

	fonsTextIterInit(ctx->fs, &iter, x*scale, y*scale, string, end, FONS_GLYPH_BITMAP_REQUIRED);
	prevIter = iter;
	while (fonsTextIterNext(ctx->fs, &iter, &q)) {
		float c[4*2];
		if (iter.prevGlyphIndex == -1) { // can not retrieve glyph?
			if (nverts != 0) {
				nvg__renderText(ctx, verts, nverts);
				nverts = 0;
			}
			if (!nvg__allocTextAtlas(ctx))
				break; // no memory :(
			iter = prevIter;
			fonsTextIterNext(ctx->fs, &iter, &q); // try again
			if (iter.prevGlyphIndex == -1) // still can not find glyph?
				break;
		}
		prevIter = iter;
		// Transform corners.
		nvgTransformPoint(&c[0],&c[1], state->xform, q.x0*invscale, q.y0*invscale);
		nvgTransformPoint(&c[2],&c[3], state->xform, q.x1*invscale, q.y0*invscale);
		nvgTransformPoint(&c[4],&c[5], state->xform, q.x1*invscale, q.y1*invscale);
		nvgTransformPoint(&c[6],&c[7], state->xform, q.x0*invscale, q.y1*invscale);
		// Create triangles
		if (nverts+6 <= cverts) {
			nvg__vset(&verts[nverts], c[0], c[1], q.s0, q.t0); nverts++;
			nvg__vset(&verts[nverts], c[4], c[5], q.s1, q.t1); nverts++;
			nvg__vset(&verts[nverts], c[2], c[3], q.s1, q.t0); nverts++;
			nvg__vset(&verts[nverts], c[0], c[1], q.s0, q.t0); nverts++;
			nvg__vset(&verts[nverts], c[6], c[7], q.s0, q.t1); nverts++;
			nvg__vset(&verts[nverts], c[4], c[5], q.s1, q.t1); nverts++;
		}
	}

	// TODO: add back-end bit to do this just once per frame.
	nvg__flushTextTexture(ctx);

	nvg__renderText(ctx, verts, nverts);

	return iter.nextx / scale;
            */
        }

        // Draws multi-line text string at specified location wrapped at the specified width. If end is specified only the sub-string up to the end is drawn.
        // White space is stripped at the beginning of the rows, the text is split at word boundaries or when new-line characters are encountered.
        // Words longer than the max width are slit at nearest character (i.e. no hyphenation).
        public static void TextBox(Context ctx, float x, float y, float breakRowWidth, string @string, int chars)
        {
            throw new NotImplementedException();
            /*
	NVGstate* state = nvg__getState(ctx);
	NVGtextRow rows[2];
	int nrows = 0, i;
	int oldAlign = state->textAlign;
	int haling = state->textAlign & (NVG_ALIGN_LEFT | NVG_ALIGN_CENTER | NVG_ALIGN_RIGHT);
	int valign = state->textAlign & (NVG_ALIGN_TOP | NVG_ALIGN_MIDDLE | NVG_ALIGN_BOTTOM | NVG_ALIGN_BASELINE);
	float lineh = 0;

	if (state->fontId == FONS_INVALID) return;

	nvgTextMetrics(ctx, NULL, NULL, &lineh);

	state->textAlign = NVG_ALIGN_LEFT | valign;

	while ((nrows = nvgTextBreakLines(ctx, string, end, breakRowWidth, rows, 2))) {
		for (i = 0; i < nrows; i++) {
			NVGtextRow* row = &rows[i];
			if (haling & NVG_ALIGN_LEFT)
				nvgText(ctx, x, y, row->start, row->end);
			else if (haling & NVG_ALIGN_CENTER)
				nvgText(ctx, x + breakRowWidth*0.5f - row->width*0.5f, y, row->start, row->end);
			else if (haling & NVG_ALIGN_RIGHT)
				nvgText(ctx, x + breakRowWidth - row->width, y, row->start, row->end);
			y += lineh * state->lineHeight;
		}
		string = rows[nrows-1].next;
	}

	state->textAlign = oldAlign;
            */
        }

        // Measures the specified text string. Parameter bounds should be a pointer to float[4],
        // if the bounding box of the text should be returned. The bounds value are [xmin,ymin, xmax,ymax]
        // Returns the horizontal advance of the measured text (i.e. where the next character should drawn).
        // Measured values are returned in local coordinate space.
        public static float TextBounds(Context ctx, float x, float y, string @string, int chars, float* bounds)
        {
            throw new NotImplementedException();
            /*
	NVGstate* state = nvg__getState(ctx);
	float scale = nvg__getFontScale(state) * ctx->devicePxRatio;
	float invscale = 1.0f / scale;
	float width;

	if (state->fontId == FONS_INVALID) return 0;

	fonsSetSize(ctx->fs, state->fontSize*scale);
	fonsSetSpacing(ctx->fs, state->letterSpacing*scale);
	fonsSetBlur(ctx->fs, state->fontBlur*scale);
	fonsSetAlign(ctx->fs, state->textAlign);
	fonsSetFont(ctx->fs, state->fontId);

	width = fonsTextBounds(ctx->fs, x*scale, y*scale, string, end, bounds);
	if (bounds != NULL) {
		// Use line bounds for height.
		fonsLineBounds(ctx->fs, y*scale, &bounds[1], &bounds[3]);
		bounds[0] *= invscale;
		bounds[1] *= invscale;
		bounds[2] *= invscale;
		bounds[3] *= invscale;
	}
	return width * invscale;
            */
        }

        // Measures the specified multi-text string. Parameter bounds should be a pointer to float[4],
        // if the bounding box of the text should be returned. The bounds value are [xmin,ymin, xmax,ymax]
        // Measured values are returned in local coordinate space.
        public static void TextBoxBounds(Context ctx, float x, float y, float breakRowWidth, string @string, int chars, float* bounds)
        {
            throw new NotImplementedException();
            /*
	NVGstate* state = nvg__getState(ctx);
	NVGtextRow rows[2];
	float scale = nvg__getFontScale(state) * ctx->devicePxRatio;
	float invscale = 1.0f / scale;
	int nrows = 0, i;
	int oldAlign = state->textAlign;
	int haling = state->textAlign & (NVG_ALIGN_LEFT | NVG_ALIGN_CENTER | NVG_ALIGN_RIGHT);
	int valign = state->textAlign & (NVG_ALIGN_TOP | NVG_ALIGN_MIDDLE | NVG_ALIGN_BOTTOM | NVG_ALIGN_BASELINE);
	float lineh = 0, rminy = 0, rmaxy = 0;
	float minx, miny, maxx, maxy;

	if (state->fontId == FONS_INVALID) {
		if (bounds != NULL)
			bounds[0] = bounds[1] = bounds[2] = bounds[3] = 0.0f;
		return;
	}

	nvgTextMetrics(ctx, NULL, NULL, &lineh);

	state->textAlign = NVG_ALIGN_LEFT | valign;

	minx = maxx = x;
	miny = maxy = y;

	fonsSetSize(ctx->fs, state->fontSize*scale);
	fonsSetSpacing(ctx->fs, state->letterSpacing*scale);
	fonsSetBlur(ctx->fs, state->fontBlur*scale);
	fonsSetAlign(ctx->fs, state->textAlign);
	fonsSetFont(ctx->fs, state->fontId);
	fonsLineBounds(ctx->fs, 0, &rminy, &rmaxy);
	rminy *= invscale;
	rmaxy *= invscale;

	while ((nrows = nvgTextBreakLines(ctx, string, end, breakRowWidth, rows, 2))) {
		for (i = 0; i < nrows; i++) {
			NVGtextRow* row = &rows[i];
			float rminx, rmaxx, dx = 0;
			// Horizontal bounds
			if (haling & NVG_ALIGN_LEFT)
				dx = 0;
			else if (haling & NVG_ALIGN_CENTER)
				dx = breakRowWidth*0.5f - row->width*0.5f;
			else if (haling & NVG_ALIGN_RIGHT)
				dx = breakRowWidth - row->width;
			rminx = x + row->minx + dx;
			rmaxx = x + row->maxx + dx;
			minx = nvg__minf(minx, rminx);
			maxx = nvg__maxf(maxx, rmaxx);
			// Vertical bounds.
			miny = nvg__minf(miny, y + rminy);
			maxy = nvg__maxf(maxy, y + rmaxy);

			y += lineh * state->lineHeight;
		}
		string = rows[nrows-1].next;
	}

	state->textAlign = oldAlign;

	if (bounds != NULL) {
		bounds[0] = minx;
		bounds[1] = miny;
		bounds[2] = maxx;
		bounds[3] = maxy;
	}
            */
        }

        // Calculates the glyph x positions of the specified text. If end is specified only the sub-string will be used.
        // Measured values are returned in local coordinate space.
        public static int TextGlyphPositions(Context ctx, float x, float y, string @string, int chars, GlyphPosition* positions, int maxPositions)
        {
            throw new NotImplementedException();
            /*
	NVGstate* state = nvg__getState(ctx);
	float scale = nvg__getFontScale(state) * ctx->devicePxRatio;
	float invscale = 1.0f / scale;
	FONStextIter iter, prevIter;
	FONSquad q;
	int npos = 0;

	if (state->fontId == FONS_INVALID) return 0;

	if (end == NULL)
		end = string + strlen(string);

	if (string == end)
		return 0;

	fonsSetSize(ctx->fs, state->fontSize*scale);
	fonsSetSpacing(ctx->fs, state->letterSpacing*scale);
	fonsSetBlur(ctx->fs, state->fontBlur*scale);
	fonsSetAlign(ctx->fs, state->textAlign);
	fonsSetFont(ctx->fs, state->fontId);

	fonsTextIterInit(ctx->fs, &iter, x*scale, y*scale, string, end, FONS_GLYPH_BITMAP_OPTIONAL);
	prevIter = iter;
	while (fonsTextIterNext(ctx->fs, &iter, &q)) {
		if (iter.prevGlyphIndex < 0 && nvg__allocTextAtlas(ctx)) { // can not retrieve glyph?
			iter = prevIter;
			fonsTextIterNext(ctx->fs, &iter, &q); // try again
		}
		prevIter = iter;
		positions[npos].str = iter.str;
		positions[npos].x = iter.x * invscale;
		positions[npos].minx = nvg__minf(iter.x, q.x0) * invscale;
		positions[npos].maxx = nvg__maxf(iter.nextx, q.x1) * invscale;
		npos++;
		if (npos >= maxPositions)
			break;
	}

	return npos;
            */
        }

        // Returns the vertical metrics based on the current text style.
        // Measured values are returned in local coordinate space.
        public static void TextMetrics(Context ctx, out float ascender, out float descender, out float lineh)
        {
            throw new NotImplementedException();
            /*
	NVGstate* state = nvg__getState(ctx);
	float scale = nvg__getFontScale(state) * ctx->devicePxRatio;
	float invscale = 1.0f / scale;

	if (state->fontId == FONS_INVALID) return;

	fonsSetSize(ctx->fs, state->fontSize*scale);
	fonsSetSpacing(ctx->fs, state->letterSpacing*scale);
	fonsSetBlur(ctx->fs, state->fontBlur*scale);
	fonsSetAlign(ctx->fs, state->textAlign);
	fonsSetFont(ctx->fs, state->fontId);

	fonsVertMetrics(ctx->fs, ascender, descender, lineh);
	if (ascender != NULL)
		*ascender *= invscale;
	if (descender != NULL)
		*descender *= invscale;
	if (lineh != NULL)
		*lineh *= invscale;
            */
        }

        // Breaks the specified text into lines. If end is specified only the sub-string will be used.
        // White space is stripped at the beginning of the rows, the text is split at word boundaries or when new-line characters are encountered.
        // Words longer than the max width are slit at nearest character (i.e. no hyphenation).
        public static int TextBreakLines(Context ctx, string @string, int chars, float breakRowWidth, TextRow* rows, int maxRows)
        {
            throw new NotImplementedException();
            /*
	NVGstate* state = nvg__getState(ctx);
	float scale = nvg__getFontScale(state) * ctx->devicePxRatio;
	float invscale = 1.0f / scale;
	FONStextIter iter, prevIter;
	FONSquad q;
	int nrows = 0;
	float rowStartX = 0;
	float rowWidth = 0;
	float rowMinX = 0;
	float rowMaxX = 0;
	const char* rowStart = NULL;
	const char* rowEnd = NULL;
	const char* wordStart = NULL;
	float wordStartX = 0;
	float wordMinX = 0;
	const char* breakEnd = NULL;
	float breakWidth = 0;
	float breakMaxX = 0;
	int type = NVG_SPACE, ptype = NVG_SPACE;
	unsigned int pcodepoint = 0;

	if (maxRows == 0) return 0;
	if (state->fontId == FONS_INVALID) return 0;

	if (end == NULL)
		end = string + strlen(string);

	if (string == end) return 0;

	fonsSetSize(ctx->fs, state->fontSize*scale);
	fonsSetSpacing(ctx->fs, state->letterSpacing*scale);
	fonsSetBlur(ctx->fs, state->fontBlur*scale);
	fonsSetAlign(ctx->fs, state->textAlign);
	fonsSetFont(ctx->fs, state->fontId);

	breakRowWidth *= scale;

	fonsTextIterInit(ctx->fs, &iter, 0, 0, string, end, FONS_GLYPH_BITMAP_OPTIONAL);
	prevIter = iter;
	while (fonsTextIterNext(ctx->fs, &iter, &q)) {
		if (iter.prevGlyphIndex < 0 && nvg__allocTextAtlas(ctx)) { // can not retrieve glyph?
			iter = prevIter;
			fonsTextIterNext(ctx->fs, &iter, &q); // try again
		}
		prevIter = iter;
		switch (iter.codepoint) {
			case 9:			// \t
			case 11:		// \v
			case 12:		// \f
			case 32:		// space
			case 0x00a0:	// NBSP
				type = NVG_SPACE;
				break;
			case 10:		// \n
				type = pcodepoint == 13 ? NVG_SPACE : NVG_NEWLINE;
				break;
			case 13:		// \r
				type = pcodepoint == 10 ? NVG_SPACE : NVG_NEWLINE;
				break;
			case 0x0085:	// NEL
				type = NVG_NEWLINE;
				break;
			default:
				if ((iter.codepoint >= 0x4E00 && iter.codepoint <= 0x9FFF) ||
					(iter.codepoint >= 0x3000 && iter.codepoint <= 0x30FF) ||
					(iter.codepoint >= 0xFF00 && iter.codepoint <= 0xFFEF) ||
					(iter.codepoint >= 0x1100 && iter.codepoint <= 0x11FF) ||
					(iter.codepoint >= 0x3130 && iter.codepoint <= 0x318F) ||
					(iter.codepoint >= 0xAC00 && iter.codepoint <= 0xD7AF))
					type = NVG_CJK_CHAR;
				else
					type = NVG_CHAR;
				break;
		}

		if (type == NVG_NEWLINE) {
			// Always handle new lines.
			rows[nrows].start = rowStart != NULL ? rowStart : iter.str;
			rows[nrows].end = rowEnd != NULL ? rowEnd : iter.str;
			rows[nrows].width = rowWidth * invscale;
			rows[nrows].minx = rowMinX * invscale;
			rows[nrows].maxx = rowMaxX * invscale;
			rows[nrows].next = iter.next;
			nrows++;
			if (nrows >= maxRows)
				return nrows;
			// Set null break point
			breakEnd = rowStart;
			breakWidth = 0.0;
			breakMaxX = 0.0;
			// Indicate to skip the white space at the beginning of the row.
			rowStart = NULL;
			rowEnd = NULL;
			rowWidth = 0;
			rowMinX = rowMaxX = 0;
		} else {
			if (rowStart == NULL) {
				// Skip white space until the beginning of the line
				if (type == NVG_CHAR || type == NVG_CJK_CHAR) {
					// The current char is the row so far
					rowStartX = iter.x;
					rowStart = iter.str;
					rowEnd = iter.next;
					rowWidth = iter.nextx - rowStartX; // q.x1 - rowStartX;
					rowMinX = q.x0 - rowStartX;
					rowMaxX = q.x1 - rowStartX;
					wordStart = iter.str;
					wordStartX = iter.x;
					wordMinX = q.x0 - rowStartX;
					// Set null break point
					breakEnd = rowStart;
					breakWidth = 0.0;
					breakMaxX = 0.0;
				}
			} else {
				float nextWidth = iter.nextx - rowStartX;

				// track last non-white space character
				if (type == NVG_CHAR || type == NVG_CJK_CHAR) {
					rowEnd = iter.next;
					rowWidth = iter.nextx - rowStartX;
					rowMaxX = q.x1 - rowStartX;
				}
				// track last end of a word
				if (((ptype == NVG_CHAR || ptype == NVG_CJK_CHAR) && type == NVG_SPACE) || type == NVG_CJK_CHAR) {
					breakEnd = iter.str;
					breakWidth = rowWidth;
					breakMaxX = rowMaxX;
				}
				// track last beginning of a word
				if ((ptype == NVG_SPACE && (type == NVG_CHAR || type == NVG_CJK_CHAR)) || type == NVG_CJK_CHAR) {
					wordStart = iter.str;
					wordStartX = iter.x;
					wordMinX = q.x0 - rowStartX;
				}

				// Break to new line when a character is beyond break width.
				if ((type == NVG_CHAR || type == NVG_CJK_CHAR) && nextWidth > breakRowWidth) {
					// The run length is too long, need to break to new line.
					if (breakEnd == rowStart) {
						// The current word is longer than the row length, just break it from here.
						rows[nrows].start = rowStart;
						rows[nrows].end = iter.str;
						rows[nrows].width = rowWidth * invscale;
						rows[nrows].minx = rowMinX * invscale;
						rows[nrows].maxx = rowMaxX * invscale;
						rows[nrows].next = iter.str;
						nrows++;
						if (nrows >= maxRows)
							return nrows;
						rowStartX = iter.x;
						rowStart = iter.str;
						rowEnd = iter.next;
						rowWidth = iter.nextx - rowStartX;
						rowMinX = q.x0 - rowStartX;
						rowMaxX = q.x1 - rowStartX;
						wordStart = iter.str;
						wordStartX = iter.x;
						wordMinX = q.x0 - rowStartX;
					} else {
						// Break the line from the end of the last word, and start new line from the beginning of the new.
						rows[nrows].start = rowStart;
						rows[nrows].end = breakEnd;
						rows[nrows].width = breakWidth * invscale;
						rows[nrows].minx = rowMinX * invscale;
						rows[nrows].maxx = breakMaxX * invscale;
						rows[nrows].next = wordStart;
						nrows++;
						if (nrows >= maxRows)
							return nrows;
						rowStartX = wordStartX;
						rowStart = wordStart;
						rowEnd = iter.next;
						rowWidth = iter.nextx - rowStartX;
						rowMinX = wordMinX;
						rowMaxX = q.x1 - rowStartX;
						// No change to the word start
					}
					// Set null break point
					breakEnd = rowStart;
					breakWidth = 0.0;
					breakMaxX = 0.0;
				}
			}
		}

		pcodepoint = iter.codepoint;
		ptype = type;
	}

	// Break the line from the end of the last word, and start new line from the beginning of the new.
	if (rowStart != NULL) {
		rows[nrows].start = rowStart;
		rows[nrows].end = rowEnd;
		rows[nrows].width = rowWidth * invscale;
		rows[nrows].minx = rowMinX * invscale;
		rows[nrows].maxx = rowMaxX * invscale;
		rows[nrows].next = end;
		nrows++;
	}

	return nrows;
            */
        }

        #endregion

        #region Internal Render API

        internal enum Texture
        {
            NVG_TEXTURE_ALPHA = 0x01,
            NVG_TEXTURE_RGBA = 0x02,
        }

        internal struct Scissor
        {
            float[] xform;//[6];
            float[] extent;//[2];
        }

        internal struct Vertex
        {
            float x, y, u, v;
        }

        internal struct Path
        {
            int first;
            int count;
            unsigned char closed;
            int nbevel;
            NVGvertex* fill;
            int nfill;
            NVGvertex* stroke;
            int nstroke;
            int winding;
            int convex;
        }

        internal class Params
        {
            void* userPtr;
            int edgeAntiAlias;
            int (* renderCreate) (void* uptr);
            int (* renderCreateTexture) (void* uptr, int type, int w, int h, int imageFlags, const unsigned char* data);
            int (* renderDeleteTexture) (void* uptr, int image);
            int (* renderUpdateTexture) (void* uptr, int image, int x, int y, int w, int h, const unsigned char* data);
            int (* renderGetTextureSize) (void* uptr, int image, int* w, int* h);
            void (* renderViewport) (void* uptr, float width, float height, float devicePixelRatio);
            void (* renderCancel) (void* uptr);
            void (* renderFlush) (void* uptr);
            void (* renderFill) (void* uptr, NVGpaint* paint, NVGcompositeOperationState compositeOperation, NVGscissor* scissor, float fringe, const float* bounds, const NVGpath* paths, int npaths);
            void (* renderStroke) (void* uptr, NVGpaint* paint, NVGcompositeOperationState compositeOperation, NVGscissor* scissor, float fringe, float strokeWidth, const NVGpath* paths, int npaths);
            void (* renderTriangles) (void* uptr, NVGpaint* paint, NVGcompositeOperationState compositeOperation, NVGscissor* scissor, const NVGvertex* verts, int nverts);
            void (* renderDelete) (void* uptr);
        }

        // Constructor and destructor, called by the render back-end.
        internal static Context CreateInternal(Params @params)
        {

        }

        internal static void DeleteInternal(Context ctx)
        {

        }

        internal static Params InternalParams(Context ctx)
        {

        }

        // Debug function to dump cached path data.
        internal static void DebugDumpPathCache(Context ctx)
        {

        }

        #endregion

        private static void nvg__setDevicePixelRatio(Context ctx, float ratio)
        {
            ctx.tessTol = 0.25f / ratio;
            ctx.distTol = 0.01f / ratio;
            ctx.fringeWidth = 1.0f / ratio;
            ctx.devicePxRatio = ratio;
        }
    }
}
