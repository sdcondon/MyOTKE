#pragma warning disable SA1124
#pragma warning disable IDE1006 //upper case..
#pragma warning disable SA1300 //upper case element names..

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
using System.Diagnostics;
using System.Threading;

namespace NanoVG
{
    public class Context
    {
        // should all be private..
        internal Params @params;

        internal float[] commands;
        internal int ccommands;
        internal int ncommands;
        internal float commandx;
        internal float commandy;

        internal NVGstate[] states = new NVGstate[NVG_MAX_STATES];
        internal int nstates;

        internal NVGpathCache cache;

        internal float tessTol;
        internal float distTol;
        internal float fringeWidth;
        internal float devicePxRatio;
        internal FontStash.FONScontext fs;
        internal int[] fontImages = new int[NVG_MAX_FONTIMAGES];
        internal int fontImageIdx;

        internal int drawCallCount;
        internal int fillTriCount;
        internal int strokeTriCount;
        internal int textTriCount;

        internal NVGstate CurrentState => states[nstates - 1];

        private const float NVG_KAPPA90 = 0.5522847493f; // Length proportional to radius of a cubic bezier handle for 90deg arcs.

        private const int NVG_INIT_FONTIMAGE_SIZE = 512;
        private const int NVG_MAX_FONTIMAGE_SIZE = 2048;
        internal const int NVG_MAX_FONTIMAGES = 4;

        private const int NVG_INIT_COMMANDS_SIZE = 256;
        private const int NVG_INIT_POINTS_SIZE = 128;
        private const int NVG_INIT_PATHS_SIZE = 16;
        private const int NVG_INIT_VERTS_SIZE = 256;
        internal const int NVG_MAX_STATES = 32;

        #region Frames

        /// <summary>
        /// Begin drawing a new frame.
        /// </summary>
        /// <param name="windowWidth">Window width.</param>
        /// <param name="windowHeight">Window height.</param>
        /// <param name="devicePixelRatio">Device pixel ratio.</param>
        /// <remarks>
        /// Calls to nanovg drawing API should be wrapped in nvgBeginFrame() & nvgEndFrame()
        /// nvgBeginFrame() defines the size of the window to render to in relation currently
        /// set viewport (i.e. glViewport on GL backends). Device pixel ration allows to
        /// control the rendering on Hi-DPI devices.
        /// For example, GLFW returns two dimension for an opened window: window size and
        /// frame buffer size. In that case you would set windowWidth/Height to the window size
        /// devicePixelRatio to: frameBufferWidth / windowWidth.
        /// </remarks>
        public void BeginFrame(float windowWidth, float windowHeight, float devicePixelRatio)
        {
            Debug.WriteLine(
                "Tris: draws: {0}, fill: {1}, stroke: {2}, text: {3}, TOT: {4}",
                drawCallCount,
                fillTriCount,
                strokeTriCount,
                textTriCount,
                fillTriCount + strokeTriCount + textTriCount);

            nstates = 0;
            Save();
            Reset();

            nvg__setDevicePixelRatio(devicePixelRatio);

            @params.renderViewport(windowWidth, windowHeight, devicePixelRatio);

            drawCallCount = 0;
            fillTriCount = 0;
            strokeTriCount = 0;
            textTriCount = 0;
        }

        /// <summary>
        /// Cancels drawing the current frame.
        /// </summary>
        /// <param name="ctx">The context to use.</param>
        public void CancelFrame()
        {
            @params.renderCancel();
        }

        /// <summary>
        /// Ends drawing flushing remaining render state.
        /// </summary>
        public void EndFrame()
        {
            @params.renderFlush();

            if (fontImageIdx != 0)
            {
                int fontImage = fontImages[fontImageIdx];
                int i, j;

                // delete images that smaller than current one
                if (fontImage == 0)
                {
                    return;
                }

                ImageSize(fontImage, out int iw, out int ih);
                for (i = j = 0; i < fontImageIdx; i++)
                {
                    if (fontImages[i] != 0)
                    {
                        ImageSize(fontImages[i], out int nw, out int nh);
                        if (nw < iw || nh < ih)
                        {
                            DeleteImage(fontImages[i]);
                        }
                        else
                        {
                            fontImages[j++] = fontImages[i];
                        }
                    }
                }

                // make current font image to first
                fontImages[j++] = fontImages[0];
                fontImages[0] = fontImage;
                fontImageIdx = 0;

                // clear all images after j
                for (i = j; i < NVG_MAX_FONTIMAGES; i++)
                {
                    fontImages[i] = 0;
                }
            }
        }

        #endregion

        #region Composite operations

        //// The composite operations in NanoVG are modeled after HTML Canvas API, and
        //// the blend func is based on OpenGL (see corresponding manuals for more info).
        //// The colors in the blending state have premultiplied alpha.

        /// <summary>
        /// Sets the composite operation.
        /// </summary>
        /// <param name="op">The operation to set.</param>
        public void GlobalCompositeOperation(CompositeOperation op)
        {
            CurrentState.compositeOperation = new CompositeOperationState(op);
        }

        /// <summary>
        /// // Sets the composite operation with custom pixel arithmetic.
        /// </summary>
        /// <param name="sfactor">Source blend factor.</param>
        /// <param name="dfactor">Destination blend factor.</param>
        public void GlobalCompositeBlendFunc(BlendFactor sfactor, BlendFactor dfactor)
        {
            GlobalCompositeBlendFuncSeparate(sfactor, dfactor, sfactor, dfactor);
        }

        /// <summary>
        /// Sets the composite operation with custom pixel arithmetic for RGB and alpha components separately.
        /// </summary>
        /// <param name="srcRGB">Source blend factor for RGB components.</param>
        /// <param name="dstRGB">Destination blend factor for RGB components.</param>
        /// <param name="srcAlpha">Source blend factor for alpha component.</param>
        /// <param name="dstAlpha">Destination blend factor for alpha component.</param>
        public void GlobalCompositeBlendFuncSeparate(BlendFactor srcRGB, BlendFactor dstRGB, BlendFactor srcAlpha, BlendFactor dstAlpha)
        {
            CompositeOperationState op;
            op.srcRGB = srcRGB;
            op.dstRGB = dstRGB;
            op.srcAlpha = srcAlpha;
            op.dstAlpha = dstAlpha;

            CurrentState.compositeOperation = op;
        }

        #endregion

        #region State Handling

        //// NanoVG contains state which represents how paths will be rendered.
        //// The state contains transform, fill and stroke styles, text and font styles,
        //// and scissor clipping.

        /// <summary>
        /// Pushes and saves the current render state into a state stack.
        /// A matching <see cref="nvgRestore"/> must be used to restore the state.
        /// </summary>
        public void Save()
        {
            if (nstates >= NVG_MAX_STATES)
            {
                return; // todo: throw invalidoperationexception?
            }

            if (nstates > 0)
            {
                states[nstates] = states[nstates - 1].Clone();
            }
            else
            {
                states[0] = new NVGstate();
            }

            nstates++;
        }

        /// <summary>
        /// Pops and restores current render state.
        /// </summary>
        public void Restore()
        {
            if (nstates <= 1)
            {
                return; // TODO: Throw?
            }

            nstates--;
        }

        /// <summary>
        /// Resets current render state to default values. Does not affect the render state stack.
        /// </summary>
        public void Reset()
        {
            var state = CurrentState;
            ////memset(state, 0, sizeof(*state)); // TODO

            state.fill = Paint.Color(Color.RGBA(255, 255, 255, 255));
            state.stroke = Paint.Color(Color.RGBA(0, 0, 0, 255));
            state.compositeOperation = new CompositeOperationState(CompositeOperation.SOURCE_OVER);
            state.shapeAntiAlias = 1;
            state.strokeWidth = 1.0f;
            state.miterLimit = 10.0f;
            state.lineCap = NanoVG.LineCap.BUTT;
            state.lineJoin = NanoVG.LineCap.MITER;
            state.alpha = 1.0f;
            state.xform = Transform2D.Identity();

            state.scissor.extent = (-1.0f, -1.0f);

            state.fontSize = 16.0f;
            state.letterSpacing = 0.0f;
            state.lineHeight = 1.0f;
            state.fontBlur = 0.0f;
            state.textAlign = (int)(Align.LEFT | Align.BASELINE);
            state.fontId = 0;
        }

        #endregion

        #region Render style setters

        //// Fill and stroke render style can be either a solid color or a paint which is a gradient or a pattern.
        //// Solid color is simply defined as a color value, different kinds of paints can be created
        //// using nvgLinearGradient(), nvgBoxGradient(), nvgRadialGradient() and nvgImagePattern().
        ////
        //// Current render style can be saved and restored using nvgSave() and nvgRestore().

        /// <summary>
        /// Sets whether to draw antialias for nvgStroke() and nvgFill(). It's enabled by default.
        /// </summary>
        /// <param name="enabled">A value indicating whether antialiasing should be enabled.</param>
        public void ShapeAntiAlias(int enabled)
        {
            CurrentState.shapeAntiAlias = enabled;
        }

        /// <summary>
        /// Sets current stroke style to a solid color.
        /// </summary>
        /// <param name="color">The color to use.</param>
        public void StrokeColor(Color color)
        {
            CurrentState.stroke = Paint.Color(color);
        }

        /// <summary>
        /// Sets current stroke style to a paint, which can be a one of the gradients or a pattern.
        /// </summary>
        /// <param name="paint">The paint to use.</param>
        public void StrokePaint(Paint paint)
        {
            var state = CurrentState;
            state.stroke = paint;
            Transform2D.Multiply(ref state.stroke.xform, state.xform);
        }

        /// <summary>
        /// Sets current fill style to a solid color.
        /// </summary>
        /// <param name="color">The color to use.</param>
        public void FillColor(Color color)
        {
            CurrentState.fill = Paint.Color(color);
        }

        /// <summary>
        /// Sets current fill style to a paint, which can be a one of the gradients or a pattern.
        /// </summary>
        /// <param name="paint">The paint to use.</param>
        public void FillPaint(Paint paint)
        {
            var state = CurrentState;
            state.fill = paint;
            Transform2D.Multiply(ref state.fill.xform, state.xform);
        }

        /// <summary>
        /// Sets the miter limit of the stroke style.
        /// Miter limit controls when a sharp corner is beveled.
        /// </summary>
        /// <param name="limit">The miter limit to use.</param>
        public void MiterLimit(float limit)
        {
            CurrentState.miterLimit = limit;
        }

        /// <summary>
        /// Sets the stroke width of the stroke style.
        /// </summary>
        /// <param name="size">The stroke width to use.</param>
        public void StrokeWidth(float size)
        {
            CurrentState.strokeWidth = size;
        }

        /// <summary>
        /// Sets how the end of the line (cap) is drawn,
        /// Can be one of: NVG_BUTT (default), NVG_ROUND, NVG_SQUARE.
        /// </summary>
        /// <param name="cap">The cap type to use.</param>
        public void LineCap(LineCap cap)
        {
            CurrentState.lineCap = cap; // TODO: Verify it is a valid enum value (or split the enum)
        }

        /// <summary>
        /// Sets how sharp path corners are drawn.
        /// Can be one of NVG_MITER (default), NVG_ROUND, NVG_BEVEL.
        /// </summary>
        /// <param name="join">The corner type to use.</param>
        public void LineJoin(LineCap join)
        {
            CurrentState.lineJoin = join; // TODO: Verify it is a valid enum value (or split the enum)
        }

        /// <summary>
        /// Sets the transparency applied to all rendered shapes.
        /// Already transparent paths will get proportionally more transparent as well.
        /// </summary>
        /// <param name="alpha">The alpha level to use.</param>
        public void GlobalAlpha(float alpha)
        {
            CurrentState.alpha = alpha; // TODO: Validate or clamp to 0 - 1 range?
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

        /// <summary>
        /// Resets current transform to the identity matrix.
        /// </summary>
        public void ResetTransform()
        {
            CurrentState.xform = Transform2D.Identity();
        }

        /// <summary>
        /// Premultiplies current coordinate system by the specified matrix.
        /// </summary>
        /// <param name="m11">The value of the first column in the first row.</param>
        /// <param name="m21">The value of the first column in the second row.</param>
        /// <param name="m12">The value of the second column in the first row.</param>
        /// <param name="m22">The value of the second column in the second row.</param>
        /// <param name="m13">The value of the third column in the first row.</param>
        /// <param name="m23">The value of the third column in the second row.</param>
        public void Transform(float m11, float m21, float m12, float m22, float m13, float m23)
        {
            var t = new Transform2D(m11, m21, m12, m22, m13, m23);
            Transform2D.Premultiply(ref CurrentState.xform, t);
        }

        /// <summary>
        /// Translates current coordinate system.
        /// </summary>
        /// <param name="x">The x-offset of the translation.</param>
        /// <param name="y">The y-offset of the translation.</param>
        public void Translate(float x, float y)
        {
            var t = Transform2D.Translate(x, y);
            Transform2D.Premultiply(ref CurrentState.xform, t);
        }

        /// <summary>
        /// Rotates current coordinate system.
        /// </summary>
        /// <param name="angle">The angle (in radians) of the rotation.</param>
        public void Rotate(float angle)
        {
            var t = Transform2D.Rotate(angle);
            Transform2D.Premultiply(ref CurrentState.xform, t);
        }

        /// <summary>
        /// Skews the current coordinate system along X axis.
        /// </summary>
        /// <param name="angle">The angle of the skew, in radians.</param>
        public void SkewX(float angle)
        {
            var t = Transform2D.SkewX(angle);
            Transform2D.Premultiply(ref CurrentState.xform, t);
        }

        /// <summary>
        /// Skews the current coordinate system along Y axis.
        /// </summary>
        /// <param name="angle">The angle of the skew, in radians.</param>
        public void SkewY(Context ctx, float angle)
        {
            var t = Transform2D.SkewY(angle);
            Transform2D.Premultiply(ref CurrentState.xform, t);
        }

        /// <summary>
        /// Scales the current coordinate system.
        /// </summary>
        /// <param name="x">The scale in the x-direction.</param>
        /// <param name="y">The scale in the y-direction.</param>
        public void Scale(float x, float y)
        {
            var t = Transform2D.Scale(x, y);
            Transform2D.Premultiply(ref CurrentState.xform, t);
        }

        /// <summary>
        /// Stores the top part (a-f) of the current transformation matrix in to the specified buffer.
        /// </summary>
        /// <param name="xform">Buffer to be populated with values of the first two rows of the transform matrix, in column-major order.</param>
        public void CurrentTransform(Transform2D xform)
        {
            CurrentState.xform = xform;
        }

        // The following functions can be used to make calculations on 2x3 transformation matrices.
        // A 2x3 matrix is represented as float[6].

        /// <summary>
        /// Transform a point by given transform.
        /// </summary>
        /// <param name="dx">The x-ordinate of the resultant point.</param>
        /// <param name="dy">The y-ordinate of the resultant point.</param>
        /// <param name="t">The transformation matrix.</param>
        /// <param name="sx">The x-ordinate of the source point.</param>
        /// <param name="sy">The y-ordinate of the source point.</param>
        public static void TransformPoint(out float dx, out float dy, Transform2D t, float sx, float sy)
        {
            dx = sx * t.R1C1 + sy * t.R1C2 + t.R1C3;
            dy = sx * t.R2C1 + sy * t.R2C2 + t.R2C3;
        }

        /// <summary>
        /// Converts degrees to radians.
        /// </summary>
        /// <param name="deg">The value to convert.</param>
        /// <returns>The input value converted to radians.</returns>
        public static float DegToRad(float deg) => deg / 180.0f * (float)Math.PI;

        /// <summary>
        /// Converts radians to degrees.
        /// </summary>
        /// <param name="rad">The value to convert.</param>
        /// <returns>The input value converted to degrees.</returns>
        public static float RadToDeg(float rad) => rad / (float)Math.PI * 180.0f;

        private static float nvg__getAverageScale(Transform2D t)
        {
            float sx = (float)Math.Sqrt(t.R1C1 * t.R1C1 + t.R1C2 * t.R1C2);
            float sy = (float)Math.Sqrt(t.R2C1 * t.R2C1 + t.R2C2 * t.R2C2);
            return (sx + sy) * 0.5f;
        }

        #endregion

        #region Images

        //// NanoVG allows you to load jpg, png, psd, tga, pic and gif files to be used for rendering.
        //// In addition you can upload your own image. The image loading is provided by stb_image.
        //// The parameter imageFlags is combination of flags defined in NVGimageFlags.

        /// <summary>
        /// Creates image by loading it from the disk from specified file name.
        /// </summary>
        /// <param name="filename">The name of the file to load image data from.</param>
        /// <param name="imageFlags">Image flags.</param>
        /// <returns>The handle to the image.</returns>
        public int CreateImage(string filename, int imageFlags)
        {
            throw new NotImplementedException();
            /*
            int w, h, n, image;
            unsigned char* img;
            stbi_set_unpremultiply_on_load(1);
            stbi_convert_iphone_png_to_rgb(1);
            img = stbi_load(filename, &w, &h, &n, 4);
            if (img == NULL)
            {
                ////printf("Failed to load %s - %s\n", filename, stbi_failure_reason());
                return 0;
            }
            image = nvgCreateImageRGBA(ctx, w, h, imageFlags, img);
            stbi_image_free(img);
            return image;
            */
        }

        /// <summary>
        /// Creates image by loading it from the specified chunk of memory.
        /// </summary>
        /// <param name="imageFlags">Image flags.</param>
        /// <param name="data">Buffer containing image data.</param>
        /// <param name="ndata">The number of bytes of buffer data that there is.</param>
        /// <returns>The handle to the image.</returns>
        public int CreateImageMem(int imageFlags, byte[] data, int ndata)
        {
            throw new NotImplementedException();
            /*
            int w, h, n, image;
            unsigned char* img = stbi_load_from_memory(data, ndata, &w, &h, &n, 4);
            if (img == NULL)
            {
                ////printf("Failed to load %s - %s\n", filename, stbi_failure_reason());
                return 0;
            }
            image = nvgCreateImageRGBA(ctx, w, h, imageFlags, img);
            stbi_image_free(img);
            return image;
            */
        }

        /// <summary>
        /// Creates an image from specified image data.
        /// </summary>
        /// <param name="w">The width of the image.</param>
        /// <param name="h">The height of the image.</param>
        /// <param name="imageFlags">Image flags.</param>
        /// <param name="data">Image data.</param>
        /// <returns>The handle to the image.</returns>
        public int CreateImageRGBA(int w, int h, int imageFlags, byte[] data)
        {
            throw new NotImplementedException();
            ////return ctx->params.renderCreateTexture(ctx->params.userPtr, NVG_TEXTURE_RGBA, w, h, imageFlags, data);
        }

        /// <summary>
        /// Updates image data specified by image handle.
        /// </summary>
        /// <param name="image">The ID of the image to update.</param>
        /// <param name="data">The image data.</param>
        public void UpdateImage(int image, byte[] data)
        {
            @params.renderGetTextureSize(image, out int w, out int h);
            @params.renderUpdateTexture(image, 0, 0, w, h, data);
        }

        /// <summary>
        /// Returns the dimensions of a created image.
        /// </summary>
        /// <param name="image">The ID of the image.</param>
        /// <param name="w">The width of the image.</param>
        /// <param name="h">The height of the image.</param>
        public void ImageSize(int image, out int w, out int h)
        {
            @params.renderGetTextureSize(image, out w, out h);
        }

        /// <summary>
        /// Deletes a created image.
        /// </summary>
        /// <param name="image">The ID of the image to delete.</param>
        public void DeleteImage(int image)
        {
            @params.renderDeleteTexture(image);
        }

        #endregion

        #region Scissoring

        //// Scissoring allows you to clip the rendering into a rectangle. This is useful for various
        //// user interface cases like rendering a text edit or a timeline.

        /// <summary>
        /// Sets the current scissor rectangle.
        /// <para/>
        /// When applied, the scissor rectangle is transformed by the current transform.
        /// </summary>
        /// <param name="x">The x-ordinate of the top-left corner of the rectangle.</param>
        /// <param name="y">The y-ordinate of the top-left corner of the rectangle.</param>
        /// <param name="w">The width of the rectangle.</param>
        /// <param name="h">The height of the rectangle.</param>
        public void Scissor(float x, float y, float w, float h)
        {
            var state = CurrentState;

            w = Math.Max(0.0f, w);
            h = Math.Max(0.0f, h);

            state.scissor.xform = Transform2D.Identity();
            state.scissor.xform.R1C3 = x + w * 0.5f;
            state.scissor.xform.R2C3 = y + h * 0.5f;
            Transform2D.Multiply(ref state.scissor.xform, state.xform);

            state.scissor.extent.X = w * 0.5f;
            state.scissor.extent.Y = h * 0.5f;
        }

        /// <summary>
        /// Intersects current scissor rectangle with the specified rectangle.
        /// The scissor rectangle is transformed by the current transform.
        /// Note: in case the rotation of previous scissor rect differs from
        /// the current one, the intersection will be done between the specified
        /// rectangle and the previous scissor rectangle transformed in the current
        /// transform space. The resulting shape is always rectangle.
        /// </summary>
        /// <param name="x">The x-ordinate of the top-left corner of the rectangle.</param>
        /// <param name="y">The y-ordinate of the top-left corner of the rectangle.</param>
        /// <param name="w">The width of the rectangle.</param>
        /// <param name="h">The height of the rectangle.</param>
        public void IntersectScissor(float x, float y, float w, float h)
        {
            var state = CurrentState;
            float[] rect = new float[4];
            float ex, ey, tex, tey;

            // If no previous scissor has been set, set the scissor as current scissor.
            if (state.scissor.extent.X < 0)
            {
                Scissor(x, y, w, h);
                return;
            }

            // Transform the current scissor rect into current transform space.
            // If there is difference in rotation, this will be approximation.
            var pxform = state.scissor.xform;
            ex = state.scissor.extent.X;
            ey = state.scissor.extent.Y;
            state.xform.Inverse(out var invxorm);
            Transform2D.Multiply(ref pxform, invxorm);
            tex = ex * Math.Abs(pxform.R1C1) + ey * Math.Abs(pxform.R1C2);
            tey = ex * Math.Abs(pxform.R2C1) + ey * Math.Abs(pxform.R2C2);

            // Intersect rects.
            MathEx.IntersectRects(rect, pxform.R1C3 - tex, pxform.R2C3 - tey, tex * 2, tey * 2, x, y, w, h);
            Scissor(rect[0], rect[1], rect[2], rect[3]);
        }

        /// <summary>
        /// Reset and disables scissoring.
        /// </summary>
        public void ResetScissor()
        {
            var state = CurrentState;
            state.scissor.xform = default;
            state.scissor.extent = (-1.0f, -1.0f);
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

        /// <summary>
        /// Clears the current path and sub-paths.
        /// </summary>
        public void BeginPath()
        {
            ncommands = 0;
            nvg__clearPathCache();
        }

        /// <summary>
        /// Starts new sub-path with specified point as first point.
        /// </summary>
        /// <param name="x">The x-ordinate of the first point.</param>
        /// <param name="y">The y-ordinate of the first point.</param>
        public void MoveTo(float x, float y)
        {
            float[] vals = { (float)NVGcommands.NVG_MOVETO, x, y };
            nvg__appendCommands(vals, vals.Length);
        }

        /// <summary>
        /// Adds line segment from the last point in the path to the specified point.
        /// </summary>
        /// <param name="x">The x-ordinate of the new point.</param>
        /// <param name="y">The y-ordinate of the new point.</param>
        public void LineTo(float x, float y)
        {
            float[] vals = { (float)NVGcommands.NVG_LINETO, x, y };
            nvg__appendCommands(vals, vals.Length);
        }

        /// <summary>
        /// Adds cubic bezier segment from last point in the path via two control points to the specified point.
        /// </summary>
        /// <param name="c1x">The x-ordinate of the first control point.</param>
        /// <param name="c1y">The y-ordinate of the first control point.</param>
        /// <param name="c2x">The x-ordinate of the second control point.</param>
        /// <param name="c2y">The y-ordinate of the second control point.</param>
        /// <param name="x">The x-ordinate of the end point.</param>
        /// <param name="y">The y-ordinate of the end point.</param>
        public void BezierTo(float c1x, float c1y, float c2x, float c2y, float x, float y)
        {
            float[] vals = { (float)NVGcommands.NVG_BEZIERTO, c1x, c1y, c2x, c2y, x, y };
            nvg__appendCommands(vals, vals.Length);
        }

        /// <summary>
        /// Adds quadratic bezier segment from last point in the path via a control point to the specified point.
        /// </summary>
        /// <param name="cx">The x-ordinate of the control point.</param>
        /// <param name="cy">The y-ordinate of the control point.</param>
        /// <param name="x">The x-ordinate of the end point.</param>
        /// <param name="y">The y-ordinate of the end point.</param>
        public void QuadTo(float cx, float cy, float x, float y)
        {
            float x0 = commandx;
            float y0 = commandy;
            float[] vals =
            {
                (float)NVGcommands.NVG_BEZIERTO,
                x0 + 2.0f / 3.0f * (cx - x0), y0 + 2.0f / 3.0f * (cy - y0),
                x + 2.0f / 3.0f * (cx - x), y + 2.0f / 3.0f * (cy - y),
                x, y,
            };
            nvg__appendCommands(vals, vals.Length);
        }

        /// <summary>
        /// Adds an arc segment at the corner defined by the last path point, and two specified points.
        /// </summary>
        /// <param name="x1">The x-ordinate of the first point to connect.</param>
        /// <param name="y1">The y-ordinate of the first point to connect.</param>
        /// <param name="x2">The x-ordinate of the second point to connect.</param>
        /// <param name="y2">The y-ordinate of the second point to connect.</param>
        /// <param name="radius">The radius of the arc.</param>
        public void ArcTo(float x1, float y1, float x2, float y2, float radius)
        {
            float x0 = commandx;
            float y0 = commandy;
            float dx0, dy0, dx1, dy1, a, d, cx, cy, a0, a1;
            Winding dir;

            if (ncommands == 0)
            {
                return;
            }

            // Handle degenerate cases.
            if (MathEx.PointEquals(x0, y0, x1, y1, distTol)
                || MathEx.PointEquals(x1, y1, x2, y2, distTol)
                || MathEx.DistancePointToSegment(x1, y1, x0, y0, x2, y2) < distTol * distTol
                || radius < distTol)
            {
                LineTo(x1, y1);
                return;
            }

            // Calculate tangential circle to lines (x0,y0)-(x1,y1) and (x1,y1)-(x2,y2).
            dx0 = x0 - x1;
            dy0 = y0 - y1;
            dx1 = x2 - x1;
            dy1 = y2 - y1;
            MathEx.Normalize(ref dx0, ref dy0);
            MathEx.Normalize(ref dx1, ref dy1);
            a = (float)Math.Acos(dx0 * dx1 + dy0 * dy1);
            d = radius / (float)Math.Tan(a / 2.0f);

            ////printf("a=%f° d=%f\n", a/NVG_PI*180.0f, d);

            if (d > 10000.0f)
            {
                LineTo(x1, y1);
                return;
            }

            if (MathEx.Cross(dx0, dy0, dx1, dy1) > 0.0f)
            {
                cx = x1 + dx0 * d + dy0 * radius;
                cy = y1 + dy0 * d + -dx0 * radius;
                a0 = (float)Math.Atan2(dx0, -dy0);
                a1 = (float)Math.Atan2(-dx1, dy1);
                dir = Winding.CW;
                ////printf("CW c=(%f, %f) a0=%f° a1=%f°\n", cx, cy, a0/NVG_PI*180.0f, a1/NVG_PI*180.0f);
            }
            else
            {
                cx = x1 + dx0 * d + -dy0 * radius;
                cy = y1 + dy0 * d + dx0 * radius;
                a0 = (float)Math.Atan2(-dx0, dy0);
                a1 = (float)Math.Atan2(dx1, -dy1);
                dir = Winding.CCW;
                ////printf("CCW c=(%f, %f) a0=%f° a1=%f°\n", cx, cy, a0/NVG_PI*180.0f, a1/NVG_PI*180.0f);
            }

            Arc(cx, cy, radius, a0, a1, dir);
        }

        /// <summary>
        /// Closes current sub-path with a line segment.
        /// </summary>
        public void ClosePath()
        {
            float[] vals = { (float)NVGcommands.NVG_CLOSE };
            nvg__appendCommands(vals, vals.Length);
        }

        /// <summary>
        /// Sets the current sub-path winding, see NVGwinding and NVGsolidity.
        /// </summary>
        /// <param name="dir">The winding direction to use.</param>
        public void PathWinding(Winding dir)
        {
            float[] vals = { (float)NVGcommands.NVG_WINDING, (float)dir };
            nvg__appendCommands(vals, vals.Length);
        }

        /// <summary>
        /// Creates new circle arc shaped sub-path.
        /// </summary>
        /// <param name="cx">The x-ordinate of the arc center.</param>
        /// <param name="cy">The y-ordinate of the arc center.</param>
        /// <param name="r">The radius of the arc.</param>
        /// <param name="a0">The start angle of the arc, in radians.</param>
        /// <param name="a1">The end angle of the arc, in radians.</param>
        /// <param name="dir">The direction in which the arc should be swept.</param>
        public void Arc(float cx, float cy, float r, float a0, float a1, Winding dir)
        {
            float[] vals = new float[3 + 5 * 7 + 100]; // todo: stackalloc?
            NVGcommands move = ncommands > 0 ? NVGcommands.NVG_LINETO : NVGcommands.NVG_MOVETO;

            // Clamp angles
            float da = a1 - a0;
            if (dir == Winding.CW)
            {
                if (Math.Abs(da) >= Math.PI * 2)
                {
                    da = (float)Math.PI * 2;
                }
                else
                {
                    while (da < 0.0f)
                    {
                        da += (float)Math.PI * 2;
                    }
                }
            }
            else
            {
                if (Math.Abs(da) >= Math.PI * 2)
                {
                    da = -(float)Math.PI * 2;
                }
                else
                {
                    while (da > 0.0f)
                    {
                        da -= (float)Math.PI * 2;
                    }
                }
            }

            // Split arc into max 90 degree segments.
            int ndivs = Math.Max(1, Math.Min((int)(Math.Abs(da) / (Math.PI * 0.5f) + 0.5f), 5));
            float hda = (da / (float)ndivs) / 2.0f;
            float kappa = (float)Math.Abs(4.0f / 3.0f * (1.0f - Math.Cos(hda)) / Math.Sin(hda));

            if (dir == Winding.CCW)
            {
                kappa = -kappa;
            }

            int nvals = 0;
            float px = 0, py = 0, ptanx = 0, ptany = 0;
            for (int i = 0; i <= ndivs; i++)
            {
                float a = a0 + da * (i / (float)ndivs);
                float dx = (float)Math.Cos(a);
                float dy = (float)Math.Sin(a);
                float x = cx + dx * r;
                float y = cy + dy * r;
                float tanx = -dy * r * kappa;
                float tany = dx * r * kappa;

                if (i == 0)
                {
                    vals[nvals++] = (float)move;
                    vals[nvals++] = x;
                    vals[nvals++] = y;
                }
                else
                {
                    vals[nvals++] = (float)NVGcommands.NVG_BEZIERTO;
                    vals[nvals++] = px + ptanx;
                    vals[nvals++] = py + ptany;
                    vals[nvals++] = x - tanx;
                    vals[nvals++] = y - tany;
                    vals[nvals++] = x;
                    vals[nvals++] = y;
                }

                px = x;
                py = y;
                ptanx = tanx;
                ptany = tany;
            }

            nvg__appendCommands(vals, nvals);
        }

        /// <summary>
        /// Creates new rectangle shaped sub-path.
        /// </summary>
        /// <param name="x">The x-ordinate of the top-left corner of the rectangle.</param>
        /// <param name="y">The y-ordinate of the top-left corner of the rectangle.</param>
        /// <param name="w">The width of the rectangle.</param>
        /// <param name="h">The height of the rectangle.</param>
        public void Rect(float x, float y, float w, float h)
        {
            float[] vals =
            {
                (float)NVGcommands.NVG_MOVETO, x, y,
                (float)NVGcommands.NVG_LINETO, x, y + h,
                (float)NVGcommands.NVG_LINETO, x + w, y + h,
                (float)NVGcommands.NVG_LINETO, x + w, y,
                (float)NVGcommands.NVG_CLOSE,
            };

            nvg__appendCommands(vals, vals.Length);
        }

        /// <summary>
        /// Creates new rounded rectangle shaped sub-path.
        /// </summary>
        /// <param name="x">The x-ordinate of the top-left corner of the rectangle.</param>
        /// <param name="y">The y-ordinate of the top-left corner of the rectangle.</param>
        /// <param name="w">The width of the rectangle.</param>
        /// <param name="h">The height of the rectangle.</param>
        /// <param name="r">The radius of the corners of the rectangle.</param>
        public void RoundedRect(float x, float y, float w, float h, float r)
        {
            RoundedRectVarying(x, y, w, h, r, r, r, r);
        }

        /// <summary>
        /// Creates new rounded rectangle shaped sub-path with varying radii for each corner.
        /// </summary>
        /// <param name="x">The x-ordinate of the top-left corner of the rectangle.</param>
        /// <param name="y">The y-ordinate of the top-left corner of the rectangle.</param>
        /// <param name="w">The width of the rectangle.</param>
        /// <param name="h">The height of the rectangle.</param>
        /// <param name="radTopLeft">The radius of the top-left corner of the rectangle.</param>
        /// <param name="radTopRight">The radius of the top-right corner of the rectangle.</param>
        /// <param name="radBottomRight">The radius of the bottom-right corner of the rectangle.</param>
        /// <param name="radBottomLeft">The radius of the bottom-left corner of the rectangle.</param>
        public void RoundedRectVarying(float x, float y, float w, float h, float radTopLeft, float radTopRight, float radBottomRight, float radBottomLeft)
        {
            if (radTopLeft < 0.1f && radTopRight < 0.1f && radBottomRight < 0.1f && radBottomLeft < 0.1f)
            {
                Rect(x, y, w, h);
                return;
            }
            else
            {
                float halfw = Math.Abs(w) * 0.5f;
                float halfh = Math.Abs(h) * 0.5f;
                float rxBL = Math.Min(radBottomLeft, halfw) * Math.Sign(w);
                float ryBL = Math.Min(radBottomLeft, halfh) * Math.Sign(h);
                float rxBR = Math.Min(radBottomRight, halfw) * Math.Sign(w);
                float ryBR = Math.Min(radBottomRight, halfh) * Math.Sign(h);
                float rxTR = Math.Min(radTopRight, halfw) * Math.Sign(w);
                float ryTR = Math.Min(radTopRight, halfh) * Math.Sign(h);
                float rxTL = Math.Min(radTopLeft, halfw) * Math.Sign(w);
                float ryTL = Math.Min(radTopLeft, halfh) * Math.Sign(h);
                float[] vals =
                {
                    (float)NVGcommands.NVG_MOVETO, x, y + ryTL,
                    (float)NVGcommands.NVG_LINETO, x, y + h - ryBL,
                    (float)NVGcommands.NVG_BEZIERTO, x, y + h - ryBL * (1 - NVG_KAPPA90), x + rxBL * (1 - NVG_KAPPA90), y + h, x + rxBL, y + h,
                    (float)NVGcommands.NVG_LINETO, x + w - rxBR, y + h,
                    (float)NVGcommands.NVG_BEZIERTO, x + w - rxBR * (1 - NVG_KAPPA90), y + h, x + w, y + h - ryBR * (1 - NVG_KAPPA90), x + w, y + h - ryBR,
                    (float)NVGcommands.NVG_LINETO, x + w, y + ryTR,
                    (float)NVGcommands.NVG_BEZIERTO, x + w, y + ryTR * (1 - NVG_KAPPA90), x + w - rxTR * (1 - NVG_KAPPA90), y, x + w - rxTR, y,
                    (float)NVGcommands.NVG_LINETO, x + rxTL, y,
                    (float)NVGcommands.NVG_BEZIERTO, x + rxTL * (1 - NVG_KAPPA90), y, x, y + ryTL * (1 - NVG_KAPPA90), x, y + ryTL,
                    (float)NVGcommands.NVG_CLOSE,
                };
                nvg__appendCommands(vals, vals.Length);
            }
        }

        /// <summary>
        /// Creates new ellipse shaped sub-path.
        /// </summary>
        /// <param name="cx">The x-ordinate of the center of the ellipse.</param>
        /// <param name="cy">The y-ordinate of the center of the ellipse.</param>
        /// <param name="rx">The x-radius of the elllipse.</param>
        /// <param name="ry">The y-radius of the ellipse.</param>
        public void Ellipse(float cx, float cy, float rx, float ry)
        {
            float[] vals =
            {
                (float)NVGcommands.NVG_MOVETO, cx - rx, cy,
                (float)NVGcommands.NVG_BEZIERTO, cx - rx, cy + ry * NVG_KAPPA90, cx - rx * NVG_KAPPA90, cy + ry, cx, cy + ry,
                (float)NVGcommands.NVG_BEZIERTO, cx + rx * NVG_KAPPA90, cy + ry, cx + rx, cy + ry * NVG_KAPPA90, cx + rx, cy,
                (float)NVGcommands.NVG_BEZIERTO, cx + rx, cy - ry * NVG_KAPPA90, cx + rx * NVG_KAPPA90, cy - ry, cx, cy - ry,
                (float)NVGcommands.NVG_BEZIERTO, cx - rx * NVG_KAPPA90, cy - ry, cx - rx, cy - ry * NVG_KAPPA90, cx - rx, cy,
                (float)NVGcommands.NVG_CLOSE,
            };

            nvg__appendCommands(vals, vals.Length);
        }

        /// <summary>
        /// Creates new circle shaped sub-path.
        /// </summary>
        /// <param name="cx">The x-ordinate of the center of the circle.</param>
        /// <param name="cy">The y-ordinate of the center of the circle.</param>
        /// <param name="r">The radius of the circle.</param>
        public void Circle(float cx, float cy, float r)
        {
            Ellipse(cx, cy, r, r);
        }

        /// <summary>
        /// Fills the current path with current fill style.
        /// </summary>
        public void Fill()
        {
            var state = CurrentState;
            Path path;
            Paint fillPaint = state.fill;

            nvg__flattenPaths();
            if (@params.edgeAntiAlias != 0 && state.shapeAntiAlias != 0)
            {
                nvg__expandFill(fringeWidth, NanoVG.LineCap.MITER, 2.4f);
            }
            else
            {
                nvg__expandFill(0.0f, NanoVG.LineCap.MITER, 2.4f);
            }

            // Apply global alpha
            fillPaint.innerColor.A *= state.alpha;
            fillPaint.outerColor.A *= state.alpha;

            @params.renderFill(
                ref fillPaint,
                state.compositeOperation,
                ref state.scissor,
                fringeWidth,
                cache.bounds,
                cache.paths,
                cache.npaths);

            // Count triangles
            for (int i = 0; i < cache.npaths; i++)
            {
                path = cache.paths[i];
                fillTriCount += path.nfill - 2;
                fillTriCount += path.nstroke - 2;
                drawCallCount += 2;
            }
        }

        /// <summary>
        /// Fills the current path with current stroke style.
        /// </summary>
        public void Stroke()
        {
            NVGstate state = CurrentState;
            float scale = nvg__getAverageScale(state.xform);
            float strokeWidth = MathEx.Clamp(state.strokeWidth * scale, 0.0f, 200.0f);
            Paint strokePaint = state.stroke;

            if (strokeWidth < fringeWidth)
            {
                // If the stroke width is less than pixel size, use alpha to emulate coverage.
                // Since coverage is area, scale by alpha*alpha.
                float alpha = MathEx.Clamp(strokeWidth / fringeWidth, 0.0f, 1.0f);
                strokePaint.innerColor.A *= alpha * alpha;
                strokePaint.outerColor.A *= alpha * alpha;
                strokeWidth = fringeWidth;
            }

            // Apply global alpha
            strokePaint.innerColor.A *= state.alpha;
            strokePaint.outerColor.A *= state.alpha;

            nvg__flattenPaths();

            if (@params.edgeAntiAlias != 0 && state.shapeAntiAlias != 0)
            {
                nvg__expandStroke(strokeWidth * 0.5f, fringeWidth, state.lineCap, state.lineJoin, state.miterLimit);
            }
            else
            {
                nvg__expandStroke(strokeWidth * 0.5f, 0.0f, state.lineCap, state.lineJoin, state.miterLimit);
            }

            @params.renderStroke(
                ref strokePaint,
                state.compositeOperation,
                ref state.scissor,
                fringeWidth,
                strokeWidth,
                cache.paths,
                cache.npaths);

            // Count triangles
            for (int i = 0; i < cache.npaths; i++)
            {
                var path = cache.paths[i];
                strokeTriCount += path.nstroke - 2;
                drawCallCount++;
            }
        }

        #endregion

        #region Text

        ////NanoVG allows you to load .ttf files and use the font to render text.
        ////
        ////The appearance of the text can be defined by setting the current text style
        ////and by specifying the fill color. Common text and font settings such as
        ////font size, letter spacing and text align are supported. Font blur allows you
        ////to create simple text effects such as drop shadows.
        ////
        ////At render time the font face can be set based on the font handles or name.
        ////
        ////Font measure functions return values in local space, the calculations are
        ////carried in the same resolution as the final rendering. This is done because
        ////the text glyph positions are snapped to the nearest pixels sharp rendering.
        ////
        ////The local space means that values are not rotated or scale as per the current
        ////transformation. For example if you set font size to 12, which would mean that
        ////line height is 16, then regardless of the current scaling and rotation, the
        ////returned line height is always 16. Some measures may vary because of the scaling
        ////since aforementioned pixel snapping.
        ////
        ////While this may sound a little odd, the setup allows you to always render the
        ////same way regardless of scaling. I.e. following works regardless of scaling:
        ////
        ////	const char* txt = "Text me up.";
        ////	nvgTextBounds(vg, x,y, txt, NULL, bounds);
        ////	nvgBeginPath(vg);
        ////	nvgRoundedRect(vg, bounds[0],bounds[1], bounds[2]-bounds[0], bounds[3]-bounds[1]);
        ////	nvgFill(vg);
        ////
        ////Note: currently only solid color fill is supported for text.

        // Creates font by loading it from the disk from specified file name.
        // Returns handle to the font.
        public int CreateFont(string name, string filename)
        {
            throw new NotImplementedException();
            /*
            return fonsAddFont(ctx->fs, name, path);
            */
        }

        // Creates font by loading it from the specified memory chunk.
        // Returns handle to the font.
        public int CreateFontMem(string name, byte[] data, int ndata, int freeData)
        {
            throw new NotImplementedException();
            /*
            return fonsAddFontMem(ctx->fs, name, data, ndata, freeData);
            */
        }

        // Finds a loaded font of specified name, and returns handle to it, or -1 if the font is not found.
        public int FindFont(string name)
        {
            throw new NotImplementedException();
            /*
            if (name == NULL) return -1;
            return fonsGetFontByName(ctx->fs, name);
            */
        }

        // Adds a fallback font by handle.
        public int AddFallbackFontId(int baseFont, int fallbackFont)
        {
            throw new NotImplementedException();
            /*
            if(baseFont == -1 || fallbackFont == -1) return 0;
            return fonsAddFallbackFont(ctx->fs, baseFont, fallbackFont);
            */
        }

        // Adds a fallback font by name.
        public int AddFallbackFont(string baseFont, string fallbackFont)
        {
            throw new NotImplementedException();
            /*
            return nvgAddFallbackFontId(ctx, nvgFindFont(ctx, baseFont), nvgFindFont(ctx, fallbackFont));
            */
        }

        // Sets the font size of current text style.
        public void FontSize(float size)
        {
            throw new NotImplementedException();
            /*
            NVGstate* state = nvg__getState(ctx);
            state->fontSize = size;
            */
        }

        // Sets the blur of current text style.
        public void FontBlur(float blur)
        {
            throw new NotImplementedException();
            /*
            NVGstate* state = nvg__getState(ctx);
            state->fontBlur = blur;
            */
        }

        // Sets the letter spacing of current text style.
        public void TextLetterSpacing(float spacing)
        {
            throw new NotImplementedException();
            /*
            NVGstate* state = nvg__getState(ctx);
            state->letterSpacing = spacing;
            */
        }

        // Sets the proportional line height of current text style. The line height is specified as multiple of font size. 
        public void TextLineHeight(float lineHeight)
        {
            throw new NotImplementedException();
            /*
            NVGstate* state = nvg__getState(ctx);
            state->lineHeight = lineHeight;
            */
        }

        // Sets the text align of current text style, see NVGalign for options.
        public void TextAlign(Align align)
        {
            throw new NotImplementedException();
            /*
            NVGstate* state = nvg__getState(ctx);
            state->textAlign = align;
            */
        }

        // Sets the font face based on specified id of current text style.
        public void FontFaceId(int font)
        {
            throw new NotImplementedException();
            /*
            NVGstate* state = nvg__getState(ctx);
            state->fontId = font;
            */
        }

        // Sets the font face based on specified name of current text style.
        public void FontFace(string font)
        {
            throw new NotImplementedException();
            /*
            NVGstate* state = nvg__getState(ctx);
            state->fontId = fonsGetFontByName(ctx->fs, font);
            */
        }

        // Draws text string at specified location. If end is specified only the sub-string up to the end is drawn.
        public float Text(float x, float y, string @string, int chars)
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
        public void TextBox(float x, float y, float breakRowWidth, string @string, int chars)
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
        public float TextBounds(float x, float y, string @string, int chars, float[] bounds)
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
        public void TextBoxBounds(float x, float y, float breakRowWidth, string @string, int chars, float[] bounds)
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
        public int TextGlyphPositions(float x, float y, string @string, int chars, GlyphPosition[] positions, int maxPositions)
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
        public void TextMetrics(out float ascender, out float descender, out float lineh)
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
        public int TextBreakLines(string @string, int chars, float breakRowWidth, TextRow[] rows, int maxRows)
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

        internal struct nvgScissor
        {
            public Transform2D xform;
            public Extent2D extent; // todo: needed? xform could be implicitly of unit square..
        }

        internal class Params
        {
            public delegate int RenderCreate();

            public delegate int RenderCreateTexture(Texture type, int w, int h, ImageFlags imageFlags, byte[] data);

            public delegate int RenderDeleteTexture(int image);

            public delegate int RenderUpdateTexture(int image, int x, int y, int w, int h, byte[] data);

            public delegate int RenderGetTextureSize(int image, out int w, out int h);

            public delegate void RenderViewport(float width, float height, float devicePixelRatio);

            public delegate void RenderCancel();

            public delegate void RenderFlush();

            public delegate void RenderFill(ref Paint paint, CompositeOperationState compositeOperation, ref nvgScissor scissor, float fringe, Bounds2D bounds, Path[] paths, int npaths);

            public delegate void RenderStroke(ref Paint paint, CompositeOperationState compositeOperation, ref nvgScissor scissor, float fringe, float strokeWidth, Path[] paths, int npaths);

            public delegate void RenderTriangles(ref Paint paint, CompositeOperationState compositeOperation, ref nvgScissor scissor, Vertex[] verts, int nverts);

            public delegate void RenderDelete();

            public int edgeAntiAlias;
            public RenderCreate renderCreate;
            public RenderCreateTexture renderCreateTexture;
            public RenderDeleteTexture renderDeleteTexture;
            public RenderUpdateTexture renderUpdateTexture;
            public RenderGetTextureSize renderGetTextureSize;
            public RenderViewport renderViewport;
            public RenderCancel renderCancel;
            public RenderFlush renderFlush;
            public RenderFill renderFill;
            public RenderStroke renderStroke;
            public RenderTriangles renderTriangles;
            public RenderDelete renderDelete;
        }

        // Constructor and destructor, called by the render back-end.
        internal static Context CreateInternal(Params @params)
        {
            Context ctx = new Context();

            ctx.@params = @params;
            for (int i = 0; i < NVG_MAX_FONTIMAGES; i++)
            {
                ctx.fontImages[i] = 0;
            }

            ctx.commands = new float[NVG_INIT_COMMANDS_SIZE];
            ctx.ccommands = NVG_INIT_COMMANDS_SIZE;
            ctx.ncommands = 0;

            ctx.cache = nvg__allocPathCache();

            ctx.Save();
            ctx.Reset();

            ctx.nvg__setDevicePixelRatio(1.0f);

            ctx.@params.renderCreate();

            // Init font rendering
            FontStash.FONSparams fontParams = new FontStash.FONSparams();
            fontParams.width = NVG_INIT_FONTIMAGE_SIZE;
            fontParams.height = NVG_INIT_FONTIMAGE_SIZE;
            fontParams.flags = FontStash.FONSflags.FONS_ZERO_TOPLEFT;
            fontParams.renderCreate = null;
            fontParams.renderUpdate = null;
            fontParams.renderDraw = null;
            fontParams.renderDelete = null;
            fontParams.userPtr = null;

            ctx.fs = FontStash.fonsCreateInternal(ref fontParams);

            // Create font texture
            ctx.fontImages[0] = ctx.@params.renderCreateTexture(Texture.NVG_TEXTURE_ALPHA, fontParams.width, fontParams.height, 0, null);
            ctx.fontImageIdx = 0;

            return ctx;
        }

        internal static void DeleteInternal(Context ctx)
        {
            if (ctx == null) return;

            if (ctx.commands != null)
            {
                //free(ctx.commands);
            }

            if (ctx.cache != null)
            {
                nvg__deletePathCache(ctx.cache);
            }

            if (ctx.fs != null)
            {
                FontStash.fonsDeleteInternal(ctx.fs);
            }

            for (int i = 0; i < NVG_MAX_FONTIMAGES; i++)
            {
                if (ctx.fontImages[i] != 0)
                {
                    ctx.DeleteImage(ctx.fontImages[i]);
                    ctx.fontImages[i] = 0;
                }
            }

            if (ctx.@params.renderDelete != null)
            {
                ctx.@params.renderDelete();
            }

            //free(ctx);
        }

        internal Params InternalParams()
        {
            return @params;
        }

        // Debug function to dump cached path data.
        internal void DebugDumpPathCache()
        {

        }

        private static NVGpathCache nvg__allocPathCache()
        {
            NVGpathCache c = new NVGpathCache();

            c.points = new NVGpoint[NVG_INIT_POINTS_SIZE];
            c.npoints = 0;
            c.cpoints = NVG_INIT_POINTS_SIZE;

            c.paths = new Path[NVG_INIT_PATHS_SIZE];
            c.npaths = 0;
            c.cpaths = NVG_INIT_PATHS_SIZE;

            c.verts = new Vertex[NVG_INIT_VERTS_SIZE];
            c.nverts = 0;
            c.cverts = NVG_INIT_VERTS_SIZE;

            return c;
        }

        private static void nvg__deletePathCache(NVGpathCache c)
        {
            if (c == null)
            {
                return;
            }

            if (c.points != null)
            {
                ////free(c.points);
            }

            if (c.paths != null)
            {
                ////free(c.paths);
            }

            if (c.verts != null)
            {
                ////free(c.verts);
            }

            ////free(c);
        }

        #endregion

        #region Private - paths

        [Flags]
        internal enum NVGpointFlags
        {
            NVG_PT_CORNER = 0x01,
            NVG_PT_LEFT = 0x02,
            NVG_PT_BEVEL = 0x04,
            NVG_PR_INNERBEVEL = 0x08,
        };

        internal struct NVGpoint
        {
            public float x;
            public float y;
            public float dx;
            public float dy;
            public float len;
            public float dmx;
            public float dmy;
            public NVGpointFlags flags;
        }

        internal class Path
        {
            public int first;
            public int count;
            public bool closed;
            public int nbevel;

            public ArraySegment<Vertex> fill;
            public int nfill;

            public ArraySegment<Vertex> stroke;
            public int nstroke;

            public Winding winding;
            public bool convex;
        }

        internal class NVGpathCache
        {
            public NVGpoint[] points;
            public int npoints;
            public int cpoints;

            public Path[] paths;
            public int npaths;
            public int cpaths;

            public Vertex[] verts;
            public int nverts;
            public int cverts;

            public Bounds2D bounds;
        }

        internal struct Bounds2D
        {
            public float MinX;
            public float MinY;
            public float MaxX;
            public float MaxY;

            public Bounds2D(float minX, float minY, float maxX, float maxY)
            {
                MinX = minX;
                MinY = minY;
                MaxX = maxX;
                MaxY = maxY;
            }
        }

        private void nvg__clearPathCache()
        {
            cache.npoints = 0;
            cache.npaths = 0;
        }

        private void nvg__addPath()
        {
            // resize array if needed
            if (cache.npaths + 1 > cache.cpaths)
            {
                int cpaths = cache.npaths + 1 + cache.cpaths / 2;
                Array.Resize(ref cache.paths, cpaths);
                cache.cpaths = cpaths;
            }

            cache.paths[cache.npaths] = new Path()
            {
                first = cache.npoints,
                winding = Winding.CCW,
            };

            cache.npaths++;
        }

        private void nvg__pathWinding(Winding winding)
        {
            Path path = nvg__lastPath();
            if (path == null)
            {
                return;
            }

            path.winding = winding;
        }

        private void nvg__closePath()
        {
            var path = nvg__lastPath();
            if (path == null)
            {
                return;
            }

            path.closed = true;
        }

        private Path nvg__lastPath()
        {
            if (cache.npaths > 0)
            {
                return cache.paths[cache.npaths - 1];
            }

            return null;
        }

        private void nvg__flattenPaths()
        {
            if (cache.npaths > 0)
            {
                return;
            }

            // Flatten
            for (int i = 0; i < ncommands;)
            {
                NVGcommands cmd = (NVGcommands)commands[i];
                switch (cmd)
                {
                    case NVGcommands.NVG_MOVETO:
                        nvg__addPath();
                        nvg__addPoint(commands[i + 1], commands[i + 2], NVGpointFlags.NVG_PT_CORNER);
                        i += 3;
                        break;
                    case NVGcommands.NVG_LINETO:
                        nvg__addPoint(commands[i + 1], commands[i + 2], NVGpointFlags.NVG_PT_CORNER);
                        i += 3;
                        break;
                    case NVGcommands.NVG_BEZIERTO:
                        if (nvg__lastPoint(out NVGpoint last))
                        {
                            nvg__tesselateBezier(
                                last.x,
                                last.y,
                                commands[i + 1],
                                commands[i + 2],
                                commands[i + 3],
                                commands[i + 4],
                                commands[i + 5],
                                commands[i + 6],
                                0,
                                NVGpointFlags.NVG_PT_CORNER);
                        }

                        i += 7;
                        break;
                    case NVGcommands.NVG_CLOSE:
                        nvg__closePath();
                        i++;
                        break;
                    case NVGcommands.NVG_WINDING:
                        nvg__pathWinding((Winding)commands[i + 1]);
                        i += 2;
                        break;
                    default:
                        i++;
                        break;
                }
            }

            cache.bounds.MinX = cache.bounds.MinY = 1e6f;
            cache.bounds.MaxX = cache.bounds.MaxY = -1e6f;

            // Calculate the direction and length of line segments.
            for (int j = 0; j < cache.npaths; j++)
            {
                var path = cache.paths[j];
                var pts = new Span<NVGpoint>(cache.points, path.first, path.count);

                // If the first and last points are the same, remove the last, mark as closed path.
                var p0 = path.count - 1;
                var p1 = 0;
                if (MathEx.PointEquals(pts[p0].x, pts[p0].y, pts[p1].x, pts[p1].y, distTol))
                {
                    path.count--;
                    p0 = path.count - 1;
                    path.closed = true;
                }

                // Enforce winding
                if (path.count > 2)
                {
                    float area = nvg__polyArea(pts, path.count);
                    if (path.winding == Winding.CCW && area < 0.0f)
                    {
                        nvg__polyReverse(pts, path.count);
                    }

                    if (path.winding == Winding.CW && area > 0.0f)
                    {
                        nvg__polyReverse(pts, path.count);
                    }
                }

                for (int i = 0; i < path.count; i++)
                {
                    // Calculate segment direction and length
                    pts[p0].dx = pts[p1].x - pts[p0].x;
                    pts[p0].dy = pts[p1].y - pts[p0].y;
                    pts[p0].len = MathEx.Normalize(ref pts[p0].dx, ref pts[p0].dy);

                    // Update bounds
                    cache.bounds.MinX = Math.Min(cache.bounds.MinX, pts[p0].x);
                    cache.bounds.MinY = Math.Min(cache.bounds.MinY, pts[p0].y);
                    cache.bounds.MaxX = Math.Max(cache.bounds.MaxX, pts[p0].x);
                    cache.bounds.MaxY = Math.Max(cache.bounds.MaxY, pts[p0].y);

                    // Advance
                    p0 = p1++;
                }
            }
        }

        private void nvg__addPoint(float x, float y, NVGpointFlags flags)
        {
            var path = nvg__lastPath();

            if (path == null)
            {
                return; // todo: throw
            }

            if (path.count > 0 && cache.npoints > 0)
            {
                // if its the same point as last one, just add flags appropriately
                nvg__lastPoint(out NVGpoint pt);
                if (MathEx.PointEquals(pt.x, pt.y, x, y, distTol))
                {
                    pt.flags |= flags;
                    return;
                }
            }

            // resize array if needed
            if (cache.npoints + 1 > cache.cpoints)
            {
                int cpoints = cache.npoints + 1 + cache.cpoints / 2;
                Array.Resize(ref cache.points, cpoints);
                cache.cpoints = cpoints;
            }

            cache.points[cache.npoints] = new NVGpoint()
            {
                x = x,
                y = y,
                flags = flags,
            };

            cache.npoints++;
            path.count++;
        }

        private bool nvg__lastPoint(out NVGpoint pt)
        {
            if (cache.npoints > 0)
            {
                pt = cache.points[cache.npoints - 1];
                return true;
            }

            pt = default;
            return false;
        }

        private static float nvg__triarea2(float ax, float ay, float bx, float by, float cx, float cy)
        {
            float abx = bx - ax;
            float aby = by - ay;
            float acx = cx - ax;
            float acy = cy - ay;
            return acx * aby - abx * acy;
        }

        private static float nvg__polyArea(Span<NVGpoint> pts, int npts)
        {
            int i;
            float area = 0;
            for (i = 2; i < npts; i++)
            {
                ref var a = ref pts[0];
                ref var b = ref pts[i - 1];
                ref var c = ref pts[i];
                area += nvg__triarea2(a.x, a.y, b.x, b.y, c.x, c.y);
            }

            return area * 0.5f;
        }

        private static void nvg__polyReverse(Span<NVGpoint> pts, int npts) // todo: can be replaced with span.reverse?
        {
            NVGpoint tmp;
            int i = 0, j = npts - 1;
            while (i < j)
            {
                tmp = pts[i];
                pts[i] = pts[j];
                pts[j] = tmp;
                i++;
                j--;
            }
        }

        private void nvg__tesselateBezier(
            float x1,
            float y1,
            float x2,
            float y2,
            float x3,
            float y3,
            float x4,
            float y4,
            int level,
            NVGpointFlags type)
        {
            float x12, y12, x23, y23, x34, y34, x123, y123, x234, y234, x1234, y1234;
            float dx, dy, d2, d3;

            if (level > 10)
            {
                return;
            }

            x12 = (x1 + x2) * 0.5f;
            y12 = (y1 + y2) * 0.5f;
            x23 = (x2 + x3) * 0.5f;
            y23 = (y2 + y3) * 0.5f;
            x34 = (x3 + x4) * 0.5f;
            y34 = (y3 + y4) * 0.5f;
            x123 = (x12 + x23) * 0.5f;
            y123 = (y12 + y23) * 0.5f;

            dx = x4 - x1;
            dy = y4 - y1;
            d2 = Math.Abs((x2 - x4) * dy - (y2 - y4) * dx);
            d3 = Math.Abs((x3 - x4) * dy - (y3 - y4) * dx);

            if ((d2 + d3) * (d2 + d3) < tessTol * (dx * dx + dy * dy))
            {
                nvg__addPoint(x4, y4, type);
                return;
            }

            /*
            if (nvg__absf(x1+x3-x2-x2) + nvg__absf(y1+y3-y2-y2) + nvg__absf(x2+x4-x3-x3) + nvg__absf(y2+y4-y3-y3) < ctx->tessTol)
            {
                nvg__addPoint(ctx, x4, y4, type);
                return;
            }
            */

            x234 = (x23 + x34) * 0.5f;
            y234 = (y23 + y34) * 0.5f;
            x1234 = (x123 + x234) * 0.5f;
            y1234 = (y123 + y234) * 0.5f;

            nvg__tesselateBezier(x1, y1, x12, y12, x123, y123, x1234, y1234, level + 1, 0);
            nvg__tesselateBezier(x1234, y1234, x234, y234, x34, y34, x4, y4, level + 1, type);
        }

        private int nvg__expandFill(float w, LineCap lineJoin, float miterLimit)
        {
            float aa = fringeWidth;
            bool fringe = w > 0.0f;

            nvg__calculateJoins(w, lineJoin, miterLimit);

            // Calculate max vertex usage
            int cverts = 0;
            for (int i = 0; i < cache.npaths; i++)
            {
                Path path = cache.paths[i];
                cverts += path.count + path.nbevel + 1;
                if (fringe)
                {
                    cverts += (path.count + path.nbevel * 5 + 1) * 2; // plus one for loop
                }
            }

            var verts = nvg__allocTempVerts(cverts);
            var dst = 0;

            bool convex = cache.npaths == 1 && cache.paths[0].convex;

            for (int i = 0; i < cache.npaths; i++)
            {
                Path path = cache.paths[i];
                var pts = new Span<NVGpoint>(cache.points, path.first, path.count);

                // Calculate shape vertices
                float woff = 0.5f * aa;
                var currentSegmentOffset = dst;
                if (fringe)
                {
                    // Looping
                    int p0 = path.count - 1;
                    int p1 = 0;
                    for (int j = 0; j < path.count; ++j)
                    {
                        if (pts[p1].flags.HasFlag(NVGpointFlags.NVG_PT_BEVEL))
                        {
                            float dlx0 = pts[p0].dy;
                            float dly0 = -pts[p0].dx;
                            float dlx1 = pts[p1].dy;
                            float dly1 = -pts[p1].dx;
                            if (pts[p1].flags.HasFlag(NVGpointFlags.NVG_PT_LEFT))
                            {
                                float lx = pts[p1].x + pts[p1].dmx * woff;
                                float ly = pts[p1].y + pts[p1].dmy * woff;
                                verts[dst] = new Vertex(lx, ly, 0.5f, 1);
                                dst++;
                            }
                            else
                            {
                                float lx0 = pts[p1].x + dlx0 * woff;
                                float ly0 = pts[p1].y + dly0 * woff;
                                float lx1 = pts[p1].x + dlx1 * woff;
                                float ly1 = pts[p1].y + dly1 * woff;
                                verts[dst] = new Vertex(lx0, ly0, 0.5f, 1);
                                dst++;
                                verts[dst] = new Vertex(lx1, ly1, 0.5f, 1);
                                dst++;
                            }
                        }
                        else
                        {
                            verts[dst] = new Vertex(pts[p1].x + (pts[p1].dmx * woff), pts[p1].y + (pts[p1].dmy * woff), 0.5f, 1);
                            dst++;
                        }

                        p0 = p1++;
                    }
                }
                else
                {
                    for (int j = 0; j < path.count; ++j)
                    {
                        verts[dst] = new Vertex(pts[j].x, pts[j].y, 0.5f, 1);
                        dst++;
                    }
                }

                path.fill = new ArraySegment<Vertex>(verts, currentSegmentOffset, dst - currentSegmentOffset);
                path.nfill = dst - currentSegmentOffset;
                currentSegmentOffset = dst;

                // Calculate fringe
                if (fringe)
                {
                    float lw = w + woff;
                    float rw = w - woff;
                    float lu = 0;
                    float ru = 1;

                    // Create only half a fringe for convex shapes so that
                    // the shape can be rendered without stenciling.
                    if (convex)
                    {
                        lw = woff;  // This should generate the same vertex as fill inset above.
                        lu = 0.5f;  // Set outline fade at middle.
                    }

                    // Looping
                    int p0 = path.count - 1;
                    int p1 = 0;

                    for (int j = 0; j < path.count; ++j)
                    {
                        if (pts[p1].flags.HasFlag(NVGpointFlags.NVG_PT_BEVEL | NVGpointFlags.NVG_PR_INNERBEVEL))
                        {
                            nvg__bevelJoin(verts, ref dst, ref pts[p0], ref pts[p1], lw, rw, lu, ru, fringeWidth);
                        }
                        else
                        {
                            verts[dst] = new Vertex(pts[p1].x + (pts[p1].dmx * lw), pts[p1].y + (pts[p1].dmy * lw), lu, 1);
                            dst++;
                            verts[dst] = new Vertex(pts[p1].x - (pts[p1].dmx * rw), pts[p1].y - (pts[p1].dmy * rw), ru, 1);
                            dst++;
                        }

                        p0 = p1++;
                    }

                    // Loop it
                    verts[dst] = new Vertex(verts[currentSegmentOffset].x, verts[currentSegmentOffset].y, lu, 1);
                    dst++;
                    verts[dst] = new Vertex(verts[currentSegmentOffset + 1].x, verts[currentSegmentOffset + 1].y, ru, 1);
                    dst++;

                    path.stroke = new ArraySegment<Vertex>(verts, currentSegmentOffset, dst - currentSegmentOffset);
                    path.nstroke = dst - currentSegmentOffset;
                }
                else
                {
                    path.stroke = new ArraySegment<Vertex>(verts, currentSegmentOffset, 0);
                    path.nstroke = 0;
                }
            }

            return 1;
        }

        private int nvg__expandStroke(float w, float fringe, LineCap lineCap, LineCap lineJoin, float miterLimit)
        {
            float aa = fringe; // ctx->fringeWidth; nottodo - commented in source, too..
            float u0 = 0.0f, u1 = 1.0f;
            int ncap = nvg__curveDivs(w, (float)Math.PI, tessTol); // Calculate divisions per half circle.

            w += aa * 0.5f;

            // Disable the gradient used for antialiasing when antialiasing is not used.
            if (aa == 0.0f)
            {
                u0 = 0.5f;
                u1 = 0.5f;
            }

            nvg__calculateJoins(w, lineJoin, miterLimit);

            // Calculate max vertex usage.
            int cverts = 0;
            for (int i = 0; i < cache.npaths; i++)
            {
                Path path = cache.paths[i];
                bool loop = !path.closed;
                if (lineJoin == NanoVG.LineCap.ROUND)
                {
                    cverts += (path.count + path.nbevel * (ncap + 2) + 1) * 2; // plus one for loop
                }
                else
                {
                    cverts += (path.count + path.nbevel * 5 + 1) * 2; // plus one for loop
                }

                if (!loop)
                {
                    // space for caps
                    if (lineCap == NanoVG.LineCap.ROUND)
                    {
                        cverts += (ncap * 2 + 2) * 2;
                    }
                    else
                    {
                        cverts += (3 + 3) * 2;
                    }
                }
            }

            Vertex[] verts = nvg__allocTempVerts(cverts);
            int dst = 0;

            for (int i = 0; i < cache.npaths; i++)
            {
                Path path = cache.paths[i];
                Span<NVGpoint> pts = new Span<NVGpoint>(cache.points, path.first, path.count);
                int p0;
                int p1;
                int s, e;
                float dx, dy;

                // Calculate fringe or stroke
                bool loop = !path.closed;
                var currentSegmentOffset = dst;

                path.fill = new ArraySegment<Vertex>(verts, currentSegmentOffset, 0);
                path.nfill = 0;

                if (loop)
                {
                    // Looping
                    p0 = path.count - 1;
                    p1 = 0;
                    s = 0;
                    e = path.count;
                }
                else
                {
                    // Add cap
                    p0 = 0;
                    p1 = 1;
                    s = 1;
                    e = path.count - 1;
                }

                if (!loop)
                {
                    // Add cap
                    dx = pts[p1].x - pts[p0].x;
                    dy = pts[p1].y - pts[p0].y;
                    MathEx.Normalize(ref dx, ref dy);
                    if (lineCap == NanoVG.LineCap.BUTT)
                    {
                        nvg__buttCapStart(verts, ref dst, ref pts[p0], dx, dy, w, -aa * 0.5f, aa, u0, u1);
                    }
                    else if (lineCap == NanoVG.LineCap.BUTT || lineCap == NanoVG.LineCap.SQUARE)
                    {
                        nvg__buttCapStart(verts, ref dst, ref pts[p0], dx, dy, w, w - aa, aa, u0, u1);
                    }
                    else if (lineCap == NanoVG.LineCap.ROUND)
                    {
                        nvg__roundCapStart(verts, ref dst, ref pts[p0], dx, dy, w, ncap, aa, u0, u1);
                    }
                }

                for (int j = s; j < e; ++j)
                {
                    if (pts[p1].flags.HasFlag(NVGpointFlags.NVG_PT_BEVEL | NVGpointFlags.NVG_PR_INNERBEVEL))
                    {
                        if (lineJoin == NanoVG.LineCap.ROUND)
                        {
                            nvg__roundJoin(verts, ref dst, ref pts[p0], ref pts[p1], w, w, u0, u1, ncap, aa);
                        }
                        else
                        {
                            nvg__bevelJoin(verts, ref dst, ref pts[p0], ref pts[p1], w, w, u0, u1, aa);
                        }
                    }
                    else
                    {
                        verts[dst] = new Vertex(pts[p1].x + (pts[p1].dmx * w), pts[p1].y + (pts[p1].dmy * w), u0, 1);
                        dst++;
                        verts[dst] = new Vertex(pts[p1].x - (pts[p1].dmx * w), pts[p1].y - (pts[p1].dmy * w), u1, 1);
                        dst++;
                    }

                    p0 = p1++;
                }

                if (loop)
                {
                    // Loop it
                    verts[dst] = new Vertex(verts[currentSegmentOffset].x, verts[currentSegmentOffset].y, u0, 1);
                    dst++;
                    verts[dst] = new Vertex(verts[currentSegmentOffset + 1].x, verts[currentSegmentOffset + 1].y, u1, 1);
                    dst++;
                }
                else
                {
                    // Add cap
                    dx = pts[p1].x - pts[p0].x;
                    dy = pts[p1].y - pts[p0].y;
                    MathEx.Normalize(ref dx, ref dy);
                    if (lineCap == NanoVG.LineCap.BUTT)
                    {
                        nvg__buttCapEnd(verts, ref dst, ref pts[p1], dx, dy, w, -aa * 0.5f, aa, u0, u1);
                    }
                    else if (lineCap == NanoVG.LineCap.BUTT || lineCap == NanoVG.LineCap.SQUARE)
                    {
                        nvg__buttCapEnd(verts, ref dst, ref pts[p1], dx, dy, w, w - aa, aa, u0, u1);
                    }
                    else if (lineCap == NanoVG.LineCap.ROUND)
                    {
                        nvg__roundCapEnd(verts, ref dst, ref pts[p1], dx, dy, w, ncap, aa, u0, u1);
                    }
                }

                path.stroke = new ArraySegment<Vertex>(verts, currentSegmentOffset, dst - currentSegmentOffset);
                path.nstroke = dst - currentSegmentOffset;
            }

            return 1;
        }

        private static void nvg__chooseBevel(
            bool bevel,
            ref NVGpoint p0,
            ref NVGpoint p1,
            float w,
            out float x0,
            out float y0,
            out float x1,
            out float y1)
        {
            if (bevel)
            {
                x0 = p1.x + p0.dy * w;
                y0 = p1.y - p0.dx * w;
                x1 = p1.x + p1.dy * w;
                y1 = p1.y - p1.dx * w;
            }
            else
            {
                x0 = p1.x + p1.dmx * w;
                y0 = p1.y + p1.dmy * w;
                x1 = p1.x + p1.dmx * w;
                y1 = p1.y + p1.dmy * w;
            }
        }

        private static void nvg__roundJoin(
            Span<Vertex> verts,
            ref int dst,
            ref NVGpoint p0,
            ref NVGpoint p1,
            float lw,
            float rw,
            float lu,
            float ru,
            int ncap,
            float fringe)
        {
            int i, n;
            float dlx0 = p0.dy;
            float dly0 = -p0.dx;
            float dlx1 = p1.dy;
            float dly1 = -p1.dx;
            ////NVG_NOTUSED(fringe);

            if (p1.flags.HasFlag(NVGpointFlags.NVG_PT_LEFT))
            {
                float lx0, ly0, lx1, ly1, a0, a1;
                nvg__chooseBevel(p1.flags.HasFlag(NVGpointFlags.NVG_PR_INNERBEVEL), ref p0, ref p1, lw, out lx0, out ly0, out lx1, out ly1);
                a0 = (float)Math.Atan2(-dly0, -dlx0);
                a1 = (float)Math.Atan2(-dly1, -dlx1);
                if (a1 > a0)
                {
                    a1 -= (float)Math.PI * 2;
                }

                verts[dst++] = new Vertex(lx0, ly0, lu, 1);
                verts[dst++] = new Vertex(p1.x - dlx0 * rw, p1.y - dly0 * rw, ru, 1);

                n = MathEx.Clamp((int)Math.Ceiling(((a0 - a1) / Math.PI) * ncap), 2, ncap);
                for (i = 0; i < n; i++)
                {
                    float u = i / (float)(n - 1);
                    float a = a0 + u * (a1 - a0);
                    float rx = p1.x + (float)Math.Cos(a) * rw;
                    float ry = p1.y + (float)Math.Sin(a) * rw;
                    verts[dst++] = new Vertex(p1.x, p1.y, 0.5f, 1);
                    verts[dst++] = new Vertex(rx, ry, ru, 1);
                }

                verts[dst++] = new Vertex(lx1, ly1, lu, 1);
                verts[dst++] = new Vertex(p1.x - dlx1 * rw, p1.y - dly1 * rw, ru, 1);
            }
            else
            {
                float rx0, ry0, rx1, ry1, a0, a1;
                nvg__chooseBevel(p1.flags.HasFlag(NVGpointFlags.NVG_PR_INNERBEVEL), ref p0, ref p1, -rw, out rx0, out ry0, out rx1, out ry1);
                a0 = (float)Math.Atan2(dly0, dlx0);
                a1 = (float)Math.Atan2(dly1, dlx1);
                if (a1 < a0)
                {
                    a1 += (float)Math.PI * 2;
                }

                verts[dst++] = new Vertex(p1.x + dlx0 * rw, p1.y + dly0 * rw, lu, 1);
                verts[dst++] = new Vertex(rx0, ry0, ru, 1);

                n = MathEx.Clamp((int)Math.Ceiling(((a1 - a0) / Math.PI) * ncap), 2, ncap);
                for (i = 0; i < n; i++)
                {
                    float u = i / (float)(n - 1);
                    float a = a0 + u * (a1 - a0);
                    float ly = p1.y + (float)Math.Sin(a) * lw;
                    float lx = p1.x + (float)Math.Cos(a) * lw;
                    verts[dst++] = new Vertex(lx, ly, lu, 1);
                    verts[dst++] = new Vertex(p1.x, p1.y, 0.5f, 1);
                }

                verts[dst++] = new Vertex(p1.x + dlx1 * rw, p1.y + dly1 * rw, lu, 1);
                verts[dst++] = new Vertex(rx1, ry1, ru, 1);
            }
        }

        private static void nvg__bevelJoin(
            Span<Vertex> verts,
            ref int dst,
            ref NVGpoint p0,
            ref NVGpoint p1,
            float lw,
            float rw,
            float lu,
            float ru,
            float fringe)
        {
            float rx0, ry0, rx1, ry1;
            float lx0, ly0, lx1, ly1;
            float dlx0 = p0.dy;
            float dly0 = -p0.dx;
            float dlx1 = p1.dy;
            float dly1 = -p1.dx;
            ////NVG_NOTUSED(fringe);

            if (p1.flags.HasFlag(NVGpointFlags.NVG_PT_LEFT))
            {
                nvg__chooseBevel(p1.flags.HasFlag(NVGpointFlags.NVG_PR_INNERBEVEL), ref p0, ref p1, lw, out lx0, out ly0, out lx1, out ly1);

                verts[dst++] = new Vertex(lx0, ly0, lu, 1);
                verts[dst++] = new Vertex(p1.x - dlx0 * rw, p1.y - dly0 * rw, ru, 1);

                if (p1.flags.HasFlag(NVGpointFlags.NVG_PT_BEVEL))
                {
                    verts[dst++] = new Vertex(lx0, ly0, lu, 1);
                    verts[dst++] = new Vertex(p1.x - dlx0 * rw, p1.y - dly0 * rw, ru, 1);

                    verts[dst++] = new Vertex(lx1, ly1, lu, 1);
                    verts[dst++] = new Vertex(p1.x - dlx1 * rw, p1.y - dly1 * rw, ru, 1);
                }
                else
                {
                    rx0 = p1.x - p1.dmx * rw;
                    ry0 = p1.y - p1.dmy * rw;

                    verts[dst++] = new Vertex(p1.x, p1.y, 0.5f, 1);
                    verts[dst++] = new Vertex(p1.x - dlx0 * rw, p1.y - dly0 * rw, ru, 1);

                    verts[dst++] = new Vertex(rx0, ry0, ru, 1);
                    verts[dst++] = new Vertex(rx0, ry0, ru, 1);

                    verts[dst++] = new Vertex(p1.x, p1.y, 0.5f, 1);
                    verts[dst++] = new Vertex(p1.x - dlx1 * rw, p1.y - dly1 * rw, ru, 1);
                }

                verts[dst++] = new Vertex(lx1, ly1, lu, 1);
                verts[dst++] = new Vertex(p1.x - dlx1 * rw, p1.y - dly1 * rw, ru, 1);
            }
            else
            {
                nvg__chooseBevel(p1.flags.HasFlag(NVGpointFlags.NVG_PR_INNERBEVEL), ref p0, ref p1, -rw, out rx0, out ry0, out rx1, out ry1);

                verts[dst++] = new Vertex(p1.x + dlx0 * lw, p1.y + dly0 * lw, lu, 1);
                verts[dst++] = new Vertex(rx0, ry0, ru, 1);

                if (p1.flags.HasFlag(NVGpointFlags.NVG_PT_BEVEL))
                {
                    verts[dst++] = new Vertex(p1.x + dlx0 * lw, p1.y + dly0 * lw, lu, 1);
                    verts[dst++] = new Vertex(rx0, ry0, ru, 1);

                    verts[dst++] = new Vertex(p1.x + dlx1 * lw, p1.y + dly1 * lw, lu, 1);
                    verts[dst++] = new Vertex(rx1, ry1, ru, 1);
                }
                else
                {
                    lx0 = p1.x + p1.dmx * lw;
                    ly0 = p1.y + p1.dmy * lw;

                    verts[dst++] = new Vertex(p1.x + dlx0 * lw, p1.y + dly0 * lw, lu, 1);
                    verts[dst++] = new Vertex(p1.x, p1.y, 0.5f, 1);

                    verts[dst++] = new Vertex(lx0, ly0, lu, 1);
                    verts[dst++] = new Vertex(lx0, ly0, lu, 1);

                    verts[dst++] = new Vertex(p1.x + dlx1 * lw, p1.y + dly1 * lw, lu, 1);
                    verts[dst++] = new Vertex(p1.x, p1.y, 0.5f, 1);
                }

                verts[dst++] = new Vertex(p1.x + dlx1 * lw, p1.y + dly1 * lw, lu, 1);
                verts[dst++] = new Vertex(rx1, ry1, ru, 1);
            }
        }

        private void nvg__calculateJoins(float w, LineCap lineJoin, float miterLimit)
        {
            float iw = 0.0f;
            if (w > 0.0f)
            {
                iw = 1.0f / w;
            }

            // Calculate which joins needs extra vertices to append, and gather vertex count.
            for (int i = 0; i < cache.npaths; i++)
            {
                Path path = cache.paths[i];
                var pts = new Span<NVGpoint>(cache.points, path.first, path.count);
                int p0i = path.count - 1;
                int p1i = 0;
                int nleft = 0;

                path.nbevel = 0;

                for (int j = 0; j < path.count; j++)
                {
                    ref NVGpoint p0 = ref pts[p0i];
                    ref NVGpoint p1 = ref pts[p1i];
                    float cross, limit;
                    float dlx0 = p0.dy;
                    float dly0 = -p0.dx;
                    float dlx1 = p1.dy;
                    float dly1 = -p1.dx;

                    // Calculate extrusions
                    p1.dmx = (dlx0 + dlx1) * 0.5f;
                    p1.dmy = (dly0 + dly1) * 0.5f;
                    float dmr2 = p1.dmx * p1.dmx + p1.dmy * p1.dmy;
                    if (dmr2 > 0.000001f)
                    {
                        float scale = 1.0f / dmr2;
                        if (scale > 600.0f)
                        {
                            scale = 600.0f;
                        }

                        p1.dmx *= scale;
                        p1.dmy *= scale;
                    }

                    // Clear flags, but keep the corner.
                    p1.flags = p1.flags.HasFlag(NVGpointFlags.NVG_PT_CORNER) ? NVGpointFlags.NVG_PT_CORNER : 0;

                    // Keep track of left turns.
                    cross = p1.dx * p0.dy - p0.dx * p1.dy;
                    if (cross > 0.0f)
                    {
                        nleft++;
                        p1.flags |= NVGpointFlags.NVG_PT_LEFT;
                    }

                    // Calculate if we should use bevel or miter for inner join.
                    limit = Math.Max(1.01f, Math.Min(p0.len, p1.len) * iw);
                    if ((dmr2 * limit * limit) < 1.0f)
                    {
                        p1.flags |= NVGpointFlags.NVG_PR_INNERBEVEL;
                    }

                    // Check to see if the corner needs to be beveled.
                    if (p1.flags.HasFlag(NVGpointFlags.NVG_PT_CORNER))
                    {
                        if ((dmr2 * miterLimit * miterLimit) < 1.0f || lineJoin == NanoVG.LineCap.BEVEL || lineJoin == NanoVG.LineCap.ROUND)
                        {
                            p1.flags |= NVGpointFlags.NVG_PT_BEVEL;
                        }
                    }

                    if (p1.flags.HasFlag(NVGpointFlags.NVG_PT_BEVEL | NVGpointFlags.NVG_PR_INNERBEVEL))
                    {
                        path.nbevel++;
                    }

                    p0i = p1i++;
                }

                path.convex = nleft == path.count;
            }
        }

        private static void nvg__buttCapStart(
            Span<Vertex> verts,
            ref int dst,
            ref NVGpoint p,
            float dx,
            float dy,
            float w,
            float d,
            float aa,
            float u0,
            float u1)
        {
            float px = p.x - dx * d;
            float py = p.y - dy * d;
            float dlx = dy;
            float dly = -dx;
            verts[dst++] = new Vertex(px + dlx * w - dx * aa, py + dly * w - dy * aa, u0, 0);
            verts[dst++] = new Vertex(px - dlx * w - dx * aa, py - dly * w - dy * aa, u1, 0);
            verts[dst++] = new Vertex(px + dlx * w, py + dly * w, u0, 1);
            verts[dst++] = new Vertex(px - dlx * w, py - dly * w, u1, 1);
        }

        private static void nvg__buttCapEnd(
            Span<Vertex> verts,
            ref int dst,
            ref NVGpoint p,
            float dx,
            float dy,
            float w,
            float d,
            float aa,
            float u0,
            float u1)
        {
            float px = p.x + dx * d;
            float py = p.y + dy * d;
            float dlx = dy;
            float dly = -dx;
            verts[dst++] = new Vertex(px + dlx * w, py + dly * w, u0, 1);
            verts[dst++] = new Vertex(px - dlx * w, py - dly * w, u1, 1);
            verts[dst++] = new Vertex(px + dlx * w + dx * aa, py + dly * w + dy * aa, u0, 0);
            verts[dst++] = new Vertex(px - dlx * w + dx * aa, py - dly * w + dy * aa, u1, 0);
        }

        private static void nvg__roundCapStart(
            Span<Vertex> verts,
            ref int dst,
            ref NVGpoint p,
            float dx,
            float dy,
            float w,
            int ncap,
            float aa,
            float u0,
            float u1)
        {
            float px = p.x;
            float py = p.y;
            float dlx = dy;
            float dly = -dx;
            ////NVG_NOTUSED(aa);

            for (int i = 0; i < ncap; i++)
            {
                float a = i / (float)(ncap - 1) * (float)Math.PI;
                float ax = (float)Math.Cos(a) * w, ay = (float)Math.Sin(a) * w;
                verts[dst++] = new Vertex(px - dlx * ax - dx * ay, py - dly * ax - dy * ay, u0, 1);
                verts[dst++] = new Vertex(px, py, 0.5f, 1);
            }

            verts[dst++] = new Vertex(px + dlx * w, py + dly * w, u0, 1);
            verts[dst++] = new Vertex(px - dlx * w, py - dly * w, u1, 1);
        }

        private static void nvg__roundCapEnd(
            Span<Vertex> verts,
            ref int dst,
            ref NVGpoint p,
            float dx,
            float dy,
            float w,
            int ncap,
            float aa,
            float u0,
            float u1)
        {
            float px = p.x;
            float py = p.y;
            float dlx = dy;
            float dly = -dx;
            ////NVG_NOTUSED(aa);

            verts[dst++] = new Vertex(px + dlx * w, py + dly * w, u0, 1);
            verts[dst++] = new Vertex(px - dlx * w, py - dly * w, u1, 1);

            for (int i = 0; i < ncap; i++)
            {
                float a = i / (float)(ncap - 1) * (float)Math.PI;
                float ax = (float)Math.Cos(a) * w, ay = (float)Math.Sin(a) * w;
                verts[dst++] = new Vertex(px, py, 0.5f, 1);
                verts[dst++] = new Vertex(px - dlx * ax + dx * ay, py - dly * ax + dy * ay, u0, 1);
            }
        }

        private Vertex[] nvg__allocTempVerts(int nverts)
        {
            if (nverts > cache.cverts)
            {
                int cverts = (nverts + 0xff) & ~0xff; // Round up to prevent allocations when things change just slightly.
                Array.Resize(ref cache.verts, cverts);
                cache.cverts = cverts;
            }

            return cache.verts;
        }

        private static int nvg__curveDivs(float r, float arc, float tol)
        {
            float da = (float)Math.Acos(r / (r + tol)) * 2.0f;
            return Math.Max(2, (int)Math.Ceiling(arc / da));
        }

        #endregion

        #region Private methods

        private void nvg__setDevicePixelRatio(float ratio)
        {
            tessTol = 0.25f / ratio;
            distTol = 0.01f / ratio;
            fringeWidth = 1.0f / ratio;
            devicePxRatio = ratio;
        }

        private void nvg__appendCommands(float[] vals, int nvals)
        {
            var state = CurrentState;

            // resize command array if necessary
            if (ncommands + nvals > ccommands)
            {
                int newccommands = ncommands + nvals + ccommands / 2;
                Array.Resize(ref commands, newccommands);
                ccommands = newccommands;
            }

            // transform commands
            if ((int)vals[0] != (int)NVGcommands.NVG_CLOSE && (int)vals[0] != (int)NVGcommands.NVG_WINDING)
            {
                commandx = vals[nvals - 2];
                commandy = vals[nvals - 1];
            }

            int i = 0;
            while (i < nvals)
            {
                var cmd = (NVGcommands)vals[i];
                switch (cmd)
                {
                    case NVGcommands.NVG_MOVETO:
                        TransformPoint(out vals[i + 1], out vals[i + 2], state.xform, vals[i + 1], vals[i + 2]);
                        i += 3;
                        break;
                    case NVGcommands.NVG_LINETO:
                        TransformPoint(out vals[i + 1], out vals[i + 2], state.xform, vals[i + 1], vals[i + 2]);
                        i += 3;
                        break;
                    case NVGcommands.NVG_BEZIERTO:
                        TransformPoint(out vals[i + 1], out vals[i + 2], state.xform, vals[i + 1], vals[i + 2]);
                        TransformPoint(out vals[i + 3], out vals[i + 4], state.xform, vals[i + 3], vals[i + 4]);
                        TransformPoint(out vals[i + 5], out vals[i + 6], state.xform, vals[i + 5], vals[i + 6]);
                        i += 7;
                        break;
                    case NVGcommands.NVG_CLOSE:
                        i++;
                        break;
                    case NVGcommands.NVG_WINDING:
                        i += 2;
                        break;
                    default:
                        i++;
                        break;
                }
            }

            // append commands to context
            Array.Copy(vals, 0, commands, ncommands, nvals);
            ncommands += nvals;
        }

        private enum NVGcommands
        {
            NVG_MOVETO = 0,
            NVG_LINETO = 1,
            NVG_BEZIERTO = 2,
            NVG_CLOSE = 3,
            NVG_WINDING = 4,
        }

        internal class NVGstate
        {
            public CompositeOperationState compositeOperation;
            public int shapeAntiAlias;
            public Paint fill;
            public Paint stroke;
            public float strokeWidth;
            public float miterLimit;
            public LineCap lineJoin;
            public LineCap lineCap;
            public float alpha;
            public Transform2D xform;
            public nvgScissor scissor;
            public float fontSize;
            public float letterSpacing;
            public float lineHeight;
            public float fontBlur;
            public int textAlign;
            public int fontId;

            public NVGstate Clone()
            {
                return (NVGstate)this.MemberwiseClone();
            }
        }

        #endregion
    }

    public struct Extent2D
    {
        public float X;
        public float Y;

        public static implicit operator Extent2D((float x, float y) valueTuple)
        {
            return new Extent2D
            {
                X = valueTuple.x,
                Y = valueTuple.y,
            };
        }
    }

    public enum Winding
    {
        /// <summary>
        /// Winding for solid shapes
        /// </summary>
        CCW = 1,

        SOLID = 1,

        /// <summary>
        /// Winding for holes
        /// </summary>
        CW = 2,

        HOLE = 2,
    }

    public enum LineCap
    {
        BUTT,
        ROUND,
        SQUARE,
        BEVEL,
        MITER,
    }

    /// <summary>
    /// Enumeration of text alignment flags.
    /// </summary>
    public enum Align
    {
        /// <summary>Default, align text horizontally to left.</summary>
        LEFT = 1 << 0,

        /// <summary>Align text horizontally to center.</summary>
        CENTER = 1 << 1,

        /// <summary>Align text horizontally to right.</summary>
        RIGHT = 1 << 2,

        /// <summary>Align text vertically to top.</summary>
        TOP = 1 << 3,

        /// <summary>Align text vertically to middle.</summary>
        MIDDLE = 1 << 4,

        /// <summary>Align text vertically to bottom.</summary>
        BOTTOM = 1 << 5,

        /// <summary>Default, align text vertically to baseline.</summary>
        BASELINE = 1 << 6,
    }

    public enum BlendFactor
    {
        ZERO = 1 << 0,
        ONE = 1 << 1,
        SRC_COLOR = 1 << 2,
        ONE_MINUS_SRC_COLOR = 1 << 3,
        DST_COLOR = 1 << 4,
        ONE_MINUS_DST_COLOR = 1 << 5,
        SRC_ALPHA = 1 << 6,
        ONE_MINUS_SRC_ALPHA = 1 << 7,
        DST_ALPHA = 1 << 8,
        ONE_MINUS_DST_ALPHA = 1 << 9,
        SRC_ALPHA_SATURATE = 1 << 10,
    }

    public enum CompositeOperation
    {
        SOURCE_OVER,
        SOURCE_IN,
        SOURCE_OUT,
        ATOP,
        DESTINATION_OVER,
        DESTINATION_IN,
        DESTINATION_OUT,
        DESTINATION_ATOP,
        LIGHTER,
        COPY,
        XOR,
    }

    struct CompositeOperationState
    {
        public CompositeOperationState(CompositeOperation op)
        {
            BlendFactor sfactor, dfactor;

            if (op == CompositeOperation.SOURCE_OVER)
            {
                sfactor = BlendFactor.ONE;
                dfactor = BlendFactor.ONE_MINUS_SRC_ALPHA;
            }
            else if (op == CompositeOperation.SOURCE_IN)
            {
                sfactor = BlendFactor.DST_ALPHA;
                dfactor = BlendFactor.ZERO;
            }
            else if (op == CompositeOperation.SOURCE_OUT)
            {
                sfactor = BlendFactor.ONE_MINUS_DST_ALPHA;
                dfactor = BlendFactor.ZERO;
            }
            else if (op == CompositeOperation.ATOP)
            {
                sfactor = BlendFactor.DST_ALPHA;
                dfactor = BlendFactor.ONE_MINUS_SRC_ALPHA;
            }
            else if (op == CompositeOperation.DESTINATION_OVER)
            {
                sfactor = BlendFactor.ONE_MINUS_DST_ALPHA;
                dfactor = BlendFactor.ONE;
            }
            else if (op == CompositeOperation.DESTINATION_IN)
            {
                sfactor = BlendFactor.ZERO;
                dfactor = BlendFactor.SRC_ALPHA;
            }
            else if (op == CompositeOperation.DESTINATION_OUT)
            {
                sfactor = BlendFactor.ZERO;
                dfactor = BlendFactor.ONE_MINUS_SRC_ALPHA;
            }
            else if (op == CompositeOperation.DESTINATION_ATOP)
            {
                sfactor = BlendFactor.ONE_MINUS_DST_ALPHA;
                dfactor = BlendFactor.SRC_ALPHA;
            }
            else if (op == CompositeOperation.LIGHTER)
            {
                sfactor = BlendFactor.ONE;
                dfactor = BlendFactor.ONE;
            }
            else if (op == CompositeOperation.COPY)
            {
                sfactor = BlendFactor.ONE;
                dfactor = BlendFactor.ZERO;
            }
            else if (op == CompositeOperation.XOR)
            {
                sfactor = BlendFactor.ONE_MINUS_DST_ALPHA;
                dfactor = BlendFactor.ONE_MINUS_SRC_ALPHA;
            }
            else
            {
                sfactor = BlendFactor.ONE;
                dfactor = BlendFactor.ZERO;
            }

            srcRGB = sfactor;
            dstRGB = dfactor;
            srcAlpha = sfactor;
            dstAlpha = dfactor;
        }

        public BlendFactor srcRGB;
        public BlendFactor dstRGB;
        public BlendFactor srcAlpha;
        public BlendFactor dstAlpha;
    }

    public struct GlyphPosition
    {
        //// TODO: const char* str;    // Position of the glyph in the input string.
        float x;            // The x-coordinate of the logical glyph position.
        float minx, maxx;   // The bounds of the glyph shape.
    }

    public struct TextRow
    {
        //// TODO: const char* start;  // Pointer to the input text where the row starts.
        //// TODO: const char* end;    // Pointer to the input text where the row ends (one past the last character).
        //// TODO: const char* next;   // Pointer to the beginning of the next row.
        float width;        // Logical width of the row.
        float minx, maxx;   // Actual bounds of the row. Logical with and bounds can differ because of kerning and some parts over extending.
    }

    enum ImageFlags
    {
        /// <summary>Generate mipmaps during creation of the image.</summary>
        IMAGE_GENERATE_MIPMAPS = 1 << 0,

        /// <summary>Repeat image in X direction.</summary>
        IMAGE_REPEATX = 1 << 1,

        /// <summary>Repeat image in Y direction.</summary>
        IMAGE_REPEATY = 1 << 2,

        /// <summary>Flips (inverses) image in Y direction when rendered.</summary>
        IMAGE_FLIPY = 1 << 3,

        /// <summary>Image data has premultiplied alpha.</summary>
        IMAGE_PREMULTIPLIED = 1 << 4,

        /// <summary>Image interpolation is Nearest instead Linear.</summary>
        IMAGE_NEAREST = 1 << 5,
    }
}

#pragma warning restore SA1124
