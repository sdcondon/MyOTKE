#pragma warning disable IDE1006 //upper case..
#pragma warning disable SA1300 //upper case..

//
// Copyright (c) 2009-2013 Mikko Mononen memon@inside.org
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

////#define NANO_GL2
#define NANOVG_GL3
////#define NANO_GLES2
////#define NANO_GLES3

#define NANOVG_GL_USE_UNIFORMBUFFER

using OpenTK.Graphics.OpenGL;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NanoVG
{
    // These are additional flags on top of NVGimageFlags.
    enum ImageFlagsGL
    {
        NoDelete = 1 << 16,   // Do not delete GL texture handle.
    }

    enum Shader
    {
        FillGradient,
        FillImage,
        Simple,
        Image,
    }

#if NANOVG_GL_USE_UNIFORMBUFFER
    enum UniformBindings
    {
        FRAG_BINDING = 0,
    }
#endif

    struct GlShader
    {
        public readonly int programId;
        public readonly int fragmentShaderId;
        public readonly int vertexShaderId;
        public readonly int uniformLocViewSize;
        public readonly int uniformLocTex;
        public readonly int uniformLocFrag;

        public GlShader(string name, string header, string opts, string vshader, string fshader)
        {
            int MakeShader(ShaderType type, string body)
            {
                var id = GL.CreateShader(type);
                GL.ShaderSource(id, header + (opts ?? string.Empty) + body);
                GL.CompileShader(id);
                GL.GetShader(id, ShaderParameter.CompileStatus, out var compileStatus);
                if (compileStatus != (int)OpenTK.Graphics.OpenGL.Boolean.True)
                {
                    throw new ArgumentException($"{name} {type} compile error: {GL.GetShaderInfoLog(id)}");
                }

                return id;
            }

            var vert = MakeShader(ShaderType.VertexShader, vshader);
            var frag = MakeShader(ShaderType.FragmentShader, fshader);

            var prog = GL.CreateProgram();
            GL.AttachShader(prog, vert);
            GL.AttachShader(prog, frag);
            GL.BindAttribLocation(prog, 0, "vertex");
            GL.BindAttribLocation(prog, 1, "tcoord");
            GL.LinkProgram(prog);
            GL.GetProgram(prog, GetProgramParameterName.LinkStatus, out var linkStatus);
            if (linkStatus != (int)OpenTK.Graphics.OpenGL.Boolean.True)
            {
                throw new ArgumentException($"Program {name} error: {GL.GetProgramInfoLog(prog)}");
            }

            programId = prog;
            vertexShaderId = vert;
            fragmentShaderId = frag;
            uniformLocViewSize = GL.GetUniformLocation(prog, "viewSize");
            uniformLocTex = GL.GetUniformLocation(prog, "tex");
            uniformLocFrag = GL.GetUniformBlockIndex(prog, "frag");
        }

        // todo: should be a class because of this (for finalizer)
        public static void DeleteShader(ref GlShader shader)
        {
            if (shader.programId != 0)
            {
                GL.DeleteProgram(shader.programId);
            }

            if (shader.vertexShaderId != 0)
            {
                GL.DeleteShader(shader.vertexShaderId);
            }

            if (shader.fragmentShaderId != 0)
            {
                GL.DeleteShader(shader.fragmentShaderId);
            }
        }
    }

    struct GlTexture
    {
        public int id;
        public uint tex;
        public int width;
        public int height;
        public Texture type;
        public ImageFlags flags;
    }

    struct GlBlend
    {
        public BlendingFactorSrc srcRGB;
        public BlendingFactorDest dstRGB;
        public BlendingFactorSrc srcAlpha;
        public BlendingFactorDest dstAlpha;
    }

    enum GlCallType
    {
        NONE = 0,
        Fill,
        CONVEXFILL,
        STROKE,
        TRIANGLES,
    }

    struct GlCall
    {
        public GlCallType type;
        public int image;
        public int pathOffset;
        public int pathCount;
        public int triangleOffset;
        public int triangleCount;
        public int uniformOffset;
        public GlBlend blendFunc;
    }

    struct GlPath
    {
        public int fillOffset;
        public int fillCount;
        public int strokeOffset;
        public int strokeCount;
    }

    struct GlFragUniforms
    {
        public Mat3x4 scissorMat;
        public Mat3x4 paintMat;
        public Color innerCol;
        public Color outerCol;
        public Extent2D scissorExt;
        public Extent2D scissorScale;
        public Extent2D extent;
        public float radius;
        public float feather;
        public float strokeMult;
        public float strokeThr;
        public int texType;
        public Shader type;
    }

    internal class GlRenderer : IRenderer
    {
        private readonly NVGFlags flags;

        private GlShader shader;
        private Extent2D view;

        private GlTexture[] textures;
        private int ntextures;
        private int ctextures;

        private int textureId;
        private uint vertBuf;
#if NANOVG_GL3
        private uint vertArr;
#endif
        private uint fragBuf;

        private int fragSize;

        // Per frame buffers
        private GlCall[] calls;
        private int ccalls;
        private int ncalls;

        private GlPath[] paths;
        private int cpaths;
        private int npaths;

        private Vertex[] verts;
        private int cverts;
        private int nverts;

        private GlFragUniforms[] uniforms;
        private int cuniforms;
        private int nuniforms;

        // Cached GL state
        private uint boundTexture;
        private uint stencilMask;
        private StencilFunction stencilFunc;
        private int stencilFuncRef;
        private uint stencilFuncMask;
        private GlBlend blendFunc;

        public GlRenderer(NVGFlags flags)
        {
            this.flags = flags;
        }

        public void RenderCreate()
        {
            int align = 4;

            // TODO: mediump float may not be enough for GLES2 in iOS.
            // see the following discussion: https://github.com/memononen/nanovg/issues/46
            const string shaderHeader =
#if NANOVG_GL2
                "#define NANOVG_GL2 1\n"
#elif NANOVG_GL3
                "#version 150 core\n"
                + "#define NANOVG_GL3 1\n"
#elif NANOVG_GLES2
                "#version 100\n"
                + "#define NANOVG_GL2 1\n"
#elif NANOVG_GLES3
                "#version 300 es\n"
                + "#define NANOVG_GL3 1\n"
#endif

#if NANOVG_GL_USE_UNIFORMBUFFER
                + "#define USE_UNIFORMBUFFER 1\n"
#else
                + "#define UNIFORMARRAY_SIZE 11\n"
#endif
                + "\n";

            const string fillVertShader =
                "#ifdef NANOVG_GL3\n"
                + "	uniform vec2 viewSize;\n"
                + "	in vec2 vertex;\n"
                + "	in vec2 tcoord;\n"
                + "	out vec2 ftcoord;\n"
                + "	out vec2 fpos;\n"
                + "#else\n"
                + "	uniform vec2 viewSize;\n"
                + "	attribute vec2 vertex;\n"
                + "	attribute vec2 tcoord;\n"
                + "	varying vec2 ftcoord;\n"
                + "	varying vec2 fpos;\n"
                + "#endif\n"
                + "void main(void) {\n"
                + "	ftcoord = tcoord;\n"
                + "	fpos = vertex;\n"
                + "	gl_Position = vec4(2.0*vertex.x/viewSize.x - 1.0, 1.0 - 2.0*vertex.y/viewSize.y, 0, 1);\n"
                + "}\n";

            const string fillFragShader =
                "#ifdef GL_ES\n"
                + "#if defined(GL_FRAGMENT_PRECISION_HIGH) || defined(NANOVG_GL3)\n"
                + " precision highp float;\n"
                + "#else\n"
                + " precision mediump float;\n"
                + "#endif\n"
                + "#endif\n"
                + "#ifdef NANOVG_GL3\n"
                + "#ifdef USE_UNIFORMBUFFER\n"
                + "	layout(std140) uniform frag {\n"
                + "		mat3 scissorMat;\n"
                + "		mat3 paintMat;\n"
                + "		vec4 innerCol;\n"
                + "		vec4 outerCol;\n"
                + "		vec2 scissorExt;\n"
                + "		vec2 scissorScale;\n"
                + "		vec2 extent;\n"
                + "		float radius;\n"
                + "		float feather;\n"
                + "		float strokeMult;\n"
                + "		float strokeThr;\n"
                + "		int texType;\n"
                + "		int type;\n"
                + "	};\n"
                + "#else\n" // NANOVG_GL3 && !USE_UNIFORMBUFFER
                + "	uniform vec4 frag[UNIFORMARRAY_SIZE];\n"
                + "#endif\n"
                + "	uniform sampler2D tex;\n"
                + "	in vec2 ftcoord;\n"
                + "	in vec2 fpos;\n"
                + "	out vec4 outColor;\n"
                + "#else\n" // !NANOVG_GL3
                + "	uniform vec4 frag[UNIFORMARRAY_SIZE];\n"
                + "	uniform sampler2D tex;\n"
                + "	varying vec2 ftcoord;\n"
                + "	varying vec2 fpos;\n"
                + "#endif\n"
                + "#ifndef USE_UNIFORMBUFFER\n"
                + "	#define scissorMat mat3(frag[0].xyz, frag[1].xyz, frag[2].xyz)\n"
                + "	#define paintMat mat3(frag[3].xyz, frag[4].xyz, frag[5].xyz)\n"
                + "	#define innerCol frag[6]\n"
                + "	#define outerCol frag[7]\n"
                + "	#define scissorExt frag[8].xy\n"
                + "	#define scissorScale frag[8].zw\n"
                + "	#define extent frag[9].xy\n"
                + "	#define radius frag[9].z\n"
                + "	#define feather frag[9].w\n"
                + "	#define strokeMult frag[10].x\n"
                + "	#define strokeThr frag[10].y\n"
                + "	#define texType int(frag[10].z)\n"
                + "	#define type int(frag[10].w)\n"
                + "#endif\n"
                + "\n"
                + "float sdroundrect(vec2 pt, vec2 ext, float rad) {\n"
                + "	vec2 ext2 = ext - vec2(rad,rad);\n"
                + "	vec2 d = abs(pt) - ext2;\n"
                + "	return min(max(d.x,d.y),0.0) + length(max(d,0.0)) - rad;\n"
                + "}\n"
                + "\n"
                + "// Scissoring\n"
                + "float scissorMask(vec2 p) {\n"
                + "	vec2 sc = (abs((scissorMat * vec3(p,1.0)).xy) - scissorExt);\n"
                + "	sc = vec2(0.5,0.5) - sc * scissorScale;\n"
                + "	return clamp(sc.x,0.0,1.0) * clamp(sc.y,0.0,1.0);\n"
                + "}\n"
                + "#ifdef EDGE_AA\n"
                + "// Stroke - from [0..1] to clipped pyramid, where the slope is 1px.\n"
                + "float strokeMask() {\n"
                + "	return min(1.0, (1.0-abs(ftcoord.x*2.0-1.0))*strokeMult) * min(1.0, ftcoord.y);\n"
                + "}\n"
                + "#endif\n"
                + "\n"
                + "void main(void) {\n"
                + "   vec4 result;\n"
                + "	float scissor = scissorMask(fpos);\n"
                + "#ifdef EDGE_AA\n"
                + "	float strokeAlpha = strokeMask();\n"
                + "	if (strokeAlpha < strokeThr) discard;\n"
                + "#else\n"
                + "	float strokeAlpha = 1.0;\n"
                + "#endif\n"
                + "	if (type == 0) {			// Gradient\n"
                + "		// Calculate gradient color using box gradient\n"
                + "		vec2 pt = (paintMat * vec3(fpos,1.0)).xy;\n"
                + "		float d = clamp((sdroundrect(pt, extent, radius) + feather*0.5) / feather, 0.0, 1.0);\n"
                + "		vec4 color = mix(innerCol,outerCol,d);\n"
                + "		// Combine alpha\n"
                + "		color *= strokeAlpha * scissor;\n"
                + "		result = color;\n"
                + "	} else if (type == 1) {		// Image\n"
                + "		// Calculate color fron texture\n"
                + "		vec2 pt = (paintMat * vec3(fpos,1.0)).xy / extent;\n"
                + "#ifdef NANOVG_GL3\n"
                + "		vec4 color = texture(tex, pt);\n"
                + "#else\n"
                + "		vec4 color = texture2D(tex, pt);\n"
                + "#endif\n"
                + "		if (texType == 1) color = vec4(color.xyz*color.w,color.w);"
                + "		if (texType == 2) color = vec4(color.x);"
                + "		// Apply color tint and alpha.\n"
                + "		color *= innerCol;\n"
                + "		// Combine alpha\n"
                + "		color *= strokeAlpha * scissor;\n"
                + "		result = color;\n"
                + "	} else if (type == 2) {		// Stencil fill\n"
                + "		result = vec4(1,1,1,1);\n"
                + "	} else if (type == 3) {		// Textured tris\n"
                + "#ifdef NANOVG_GL3\n"
                + "		vec4 color = texture(tex, ftcoord);\n"
                + "#else\n"
                + "		vec4 color = texture2D(tex, ftcoord);\n"
                + "#endif\n"
                + "		if (texType == 1) color = vec4(color.xyz*color.w,color.w);"
                + "		if (texType == 2) color = vec4(color.x);"
                + "		color *= scissor;\n"
                + "		result = color * innerCol;\n"
                + "	}\n"
                + "#ifdef NANOVG_GL3\n"
                + "	outColor = result;\n"
                + "#else\n"
                + "	gl_FragColor = result;\n"
                + "#endif\n"
                + "}\n";

            var shaderOpts = flags.HasFlag(NVGFlags.AntiAlias) ? "#define EDGE_AA 1\n" : null;
            shader = new GlShader("shader", shaderHeader, shaderOpts, fillVertShader, fillFragShader);
            GlDebugError("program creation");

            // Create dynamic vertex array
#if NANOVG_GL3
            GL.GenVertexArrays(1, out vertArr);
#endif
            GL.GenBuffers(1, out vertBuf);

#if NANOVG_GL_USE_UNIFORMBUFFER
            // Create UBOs
            GL.UniformBlockBinding(
                shader.programId,
                shader.uniformLocFrag,
                (int)UniformBindings.FRAG_BINDING);
            GL.GenBuffers(1, out fragBuf);
            GL.GetInteger(GetPName.UniformBufferOffsetAlignment, out align);
#endif
            fragSize = Marshal.SizeOf(typeof(GlFragUniforms)) + align - Marshal.SizeOf(typeof(GlFragUniforms)) % align;

            GlDebugError("creation done");

            GL.Finish();
        }

        public int RenderCreateTexture(Texture type, int w, int h, ImageFlags imageFlags, byte[] data)
        {
            ref GlTexture tex = ref AllocTexture();

#if NANOVG_GLES2
            // Check for non-power of 2.
            if (glnvg__nearestPow2(w) != (unsignedint)w || glnvg__nearestPow2(h) != (unsignedint)h)
            {
                // No repeat
                if ((imageFlags & NVG_IMAGE_REPEATX) != 0 || (imageFlags & NVG_IMAGE_REPEATY) != 0)
                {
                    printf("Repeat X/Y is not supported for non power-of-two textures (%d x %d)\n", w, h);
                    imageFlags &= ~(NVG_IMAGE_REPEATX | NVG_IMAGE_REPEATY);
                }
                // No mips.
                if (imageFlags & NVG_IMAGE_GENERATE_MIPMAPS)
                {
                    printf("Mip-maps is not support for non power-of-two textures (%d x %d)\n", w, h);
                    imageFlags &= ~NVG_IMAGE_GENERATE_MIPMAPS;
                }
            }
#endif

            GL.GenTextures(1, out tex.tex);
            tex.width = w;
            tex.height = h;
            tex.type = type;
            tex.flags = imageFlags;
            GlBindTexture(tex.tex);

            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
#if !NANOVG_GLES2
            GL.PixelStore(PixelStoreParameter.UnpackRowLength, tex.width);
            GL.PixelStore(PixelStoreParameter.UnpackSkipPixels, 0);
            GL.PixelStore(PixelStoreParameter.UnpackSkipRows, 0);
#endif

#if NANOVG_GL2
            // GL 1.4 and later has support for generating mipmaps using a tex parameter.
            if (imageFlags & NVG_IMAGE_GENERATE_MIPMAPS)
            {
                GL.TexParameteri(GL_TEXTURE_2D, GL_GENERATE_MIPMAP, GL_TRUE);
            }
#endif

            if (type == Texture.RGBA)
            {
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, w, h, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
            }
            else
            {
#if NANOVG_GLES2 || NANOVG_GL2
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.LUMINANCE, w, h, 0, PixelFormat.LUMINANCE, PixelType.UnsignedByte, data);
#elif NANOVG_GLES3
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8, w, h, 0, PixelFormat.Red, PixelType.UnsignedByte, data);
#else
                GL.TexImage2D(TextureTarget.Texture2D, 0, /*PixelInternalFormat.Red*/PixelInternalFormat.R8, w, h, 0, PixelFormat.Red, PixelType.UnsignedByte, data);
#endif
            }

            if (imageFlags.HasFlag(ImageFlags.GenerateMipMaps))
            {
                if (imageFlags.HasFlag(ImageFlags.Nearest))
                {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapNearest);
                }
                else
                {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                }
            }
            else
            {
                if (imageFlags.HasFlag(ImageFlags.Nearest))
                {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                }
                else
                {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                }
            }

            GL.TexParameter(
                TextureTarget.Texture2D,
                TextureParameterName.TextureMagFilter,
                imageFlags.HasFlag(ImageFlags.Nearest) ? (int)TextureMagFilter.Nearest : (int)TextureMagFilter.Linear);

            GL.TexParameter(
                TextureTarget.Texture2D,
                TextureParameterName.TextureWrapS,
                imageFlags.HasFlag(ImageFlags.RepeatX) ? (int)TextureWrapMode.Repeat : (int)TextureWrapMode.ClampToEdge);

            GL.TexParameter(
                TextureTarget.Texture2D,
                TextureParameterName.TextureWrapT,
                imageFlags.HasFlag(ImageFlags.RepeatY) ? (int)TextureWrapMode.Repeat : (int)TextureWrapMode.ClampToEdge);

            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 4);
#if !NANOVG_GLES2
            GL.PixelStore(PixelStoreParameter.UnpackRowLength, 0);
            GL.PixelStore(PixelStoreParameter.UnpackSkipPixels, 0);
            GL.PixelStore(PixelStoreParameter.UnpackSkipRows, 0);
#endif

            // The new way to build mipmaps on GLES and GL3
#if !NANOVG_GL2
            if (imageFlags.HasFlag(ImageFlags.GenerateMipMaps))
            {
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            }
#endif

            GlDebugError("create tex");
            GlBindTexture(0);

            return tex.id;
        }

        public int RenderDeleteTexture(int image)
        {
            return DeleteTexture(image);
        }

        public int RenderUpdateTexture(int image, int x, int y, int w, int h, byte[] data)
        {
            if (!FindTexture(image, out var tex)) return 0;

            GlBindTexture(tex.tex);

            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

#if !NANOVG_GLES2
            GL.PixelStore(PixelStoreParameter.UnpackRowLength, tex.width);
            GL.PixelStore(PixelStoreParameter.UnpackSkipPixels, x);
            GL.PixelStore(PixelStoreParameter.UnpackSkipRows, y);
#else
            // No support for all of skip, need to update a whole row at a time.
            if (tex->type == NVG_TEXTURE_RGBA)
                data += y* tex->width* 4;
            else
                data += y* tex->width;
            x = 0;
            w = tex->width;
#endif

            if (tex.type == Texture.RGBA)
            {
                GL.TexSubImage2D(TextureTarget.Texture2D, 0, x, y, w, h, PixelFormat.Rgba, PixelType.UnsignedByte, data);
            }
            else
            {
#if NANOVG_GLES2 || NANOVG_GL2
                GL.TexSubImage2D(TextureTarget.Texture2D, 0, x, y, w, h, PixelFormat.Luminance, PixelType.UnsignedByte, data);
#else
                GL.TexSubImage2D(TextureTarget.Texture2D, 0, x, y, w, h, PixelFormat.Red, PixelType.UnsignedByte, data);
#endif
            }

            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 4);
#if !NANOVG_GLES2
            GL.PixelStore(PixelStoreParameter.UnpackRowLength, 0);
            GL.PixelStore(PixelStoreParameter.UnpackSkipPixels, 0);
            GL.PixelStore(PixelStoreParameter.UnpackSkipRows, 0);
#endif

            GlBindTexture(0);

            return 1;
        }

        public int RenderGetTextureSize(int image, out int w, out int h)
        {
            if (!FindTexture(image, out var tex))
            {
                w = h = 0;
                return 0;
            }

            w = tex.width;
            h = tex.height;
            return 1;
        }

        public void RenderViewport(float width, float height, float devicePixelRatio)
        {
            //NVG_NOTUSED(devicePixelRatio);
            view.X = width;
            view.Y = height;
        }

        public void RenderCancel()
        {
            nverts = 0;
            npaths = 0;
            ncalls = 0;
            nuniforms = 0;
        }

        public void RenderFlush()
        {
            if (ncalls > 0)
            {
                // Setup required GL state
                GL.UseProgram(shader.programId);

                GL.Enable(EnableCap.CullFace);
                GL.CullFace(CullFaceMode.Back);
                GL.FrontFace(FrontFaceDirection.Ccw);
                GL.Enable(EnableCap.Blend);
                GL.Disable(EnableCap.DepthTest);
                GL.Disable(EnableCap.ScissorTest);
                GL.ColorMask(true, true, true, true);
                GL.StencilMask(0xffffffff);
                GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);
                GL.StencilFunc(StencilFunction.Always, 0, 0xffffffff);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, 0);
#if NANOVG_GL_USE_STATE_FILTER
                gl.boundTexture = 0;
                gl.stencilMask = 0xffffffff;
                gl.stencilFunc = GL_ALWAYS;
                gl.stencilFuncRef = 0;
                gl.stencilFuncMask = 0xffffffff;
                gl.blendFunc.srcRGB = GL_INVALID_ENUM;
                gl.blendFunc.srcAlpha = GL_INVALID_ENUM;
                gl.blendFunc.dstRGB = GL_INVALID_ENUM;
                gl.blendFunc.dstAlpha = GL_INVALID_ENUM;
#endif

#if NANOVG_GL_USE_UNIFORMBUFFER
                // Upload ubo for frag shaders
                GL.BindBuffer(BufferTarget.UniformBuffer, fragBuf);
                GL.BufferData(BufferTarget.UniformBuffer, nuniforms * fragSize, uniforms, BufferUsageHint.StreamDraw);
#endif

                // Upload vertex data
#if NANOVG_GL3
                GL.BindVertexArray(vertArr);
#endif
                GL.BindBuffer(BufferTarget.ArrayBuffer, vertBuf);
                GL.BufferData(BufferTarget.ArrayBuffer, nverts, verts, BufferUsageHint.StreamDraw);
                GL.EnableVertexAttribArray(0);
                GL.EnableVertexAttribArray(1);
                GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf<Vertex>(), Marshal.OffsetOf<Vertex>(nameof(Vertex.x)));
                GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf<Vertex>(), Marshal.OffsetOf<Vertex>(nameof(Vertex.u)));

                // Set view and texture just once per frame.
                GL.Uniform1(shader.uniformLocTex, 0);
                GL.Uniform2(shader.uniformLocViewSize, view.X, view.Y);

#if NANOVG_GL_USE_UNIFORMBUFFER
                GL.BindBuffer(BufferTarget.UniformBuffer, fragBuf);
#endif

                for (int i = 0; i < ncalls; i++)
                {
                    ref GlCall call = ref calls[i];
                    GlBlendFuncSeparate(ref call.blendFunc);
                    if (call.type == GlCallType.Fill)
                    {
                        FlushFill(ref call);
                    }
                    else if (call.type == GlCallType.CONVEXFILL)
                    {
                        FlushConvexFill(ref call);
                    }
                    else if (call.type == GlCallType.STROKE)
                    {
                        FlushStroke(ref call);
                    }
                    else if (call.type == GlCallType.TRIANGLES)
                    {
                        FlushTriangles(ref call);
                    }
                }

                GL.DisableVertexAttribArray(0);
                GL.DisableVertexAttribArray(1);
#if NANOVG_GL3
                GL.BindVertexArray(0);
#endif
                GL.Disable(EnableCap.CullFace);
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                GL.UseProgram(0);
                GlBindTexture(0);
            }

            // Reset calls
            nverts = 0;
            npaths = 0;
            ncalls = 0;
            nuniforms = 0;
        }

        public void RenderFill(
            ref Paint paint,
            CompositeOperationState compositeOperation,
            ref ScissorInfo scissor,
            float fringe,
            NVG.Bounds2D bounds,
            NVG.Path[] paths,
            int npaths)
        {
            ref GlCall call = ref AllocCall();
            call.type = GlCallType.Fill;
            call.triangleCount = 4;
            call.pathOffset = AllocPaths(npaths);
            call.pathCount = npaths;
            call.image = paint.image;
            call.blendFunc = BlendCompositeOperation(compositeOperation);

            if (npaths == 1 && paths[0].convex)
            {
                call.type = GlCallType.CONVEXFILL;
                call.triangleCount = 0; // Bounding box fill quad not needed for convex fill
            }

            // Allocate vertices for all the paths.
            int maxverts = GetMaxVertCount(paths, npaths) + call.triangleCount;
            int offset = AllocVerts(maxverts);

            for (int i = 0; i < npaths; i++)
            {
                ref GlPath copy = ref this.paths[call.pathOffset + i];
                ref NVG.Path path = ref paths[i];
                copy = new GlPath();
                if (path.nfill > 0)
                {
                    copy.fillOffset = offset;
                    copy.fillCount = path.nfill;
                    Array.Copy(path.fill.Array, path.fill.Offset, verts, offset, path.nfill);
                    offset += path.nfill;
                }

                if (path.nstroke > 0)
                {
                    copy.strokeOffset = offset;
                    copy.strokeCount = path.nstroke;
                    Array.Copy(path.stroke.Array, path.stroke.Offset, verts, offset, path.nstroke);
                    offset += path.nstroke;
                }
            }

            // Setup uniforms for draw calls
            if (call.type == GlCallType.Fill)
            {
                // Quad
                call.triangleOffset = offset;
                var quad = new Span<Vertex>(verts, call.triangleOffset, 4);
                quad[0] = new Vertex(bounds.MaxX, bounds.MaxY, 0.5f, 1.0f);
                quad[1] = new Vertex(bounds.MaxX, bounds.MinY, 0.5f, 1.0f);
                quad[2] = new Vertex(bounds.MinX, bounds.MaxY, 0.5f, 1.0f);
                quad[3] = new Vertex(bounds.MinX, bounds.MinY, 0.5f, 1.0f);

                call.uniformOffset = AllocFragUniforms(2);

                // Simple shader for stencil
                ref var frag = ref FragUniformRef(call.uniformOffset / this.fragSize);
                frag = new GlFragUniforms();
                frag.strokeThr = -1.0f;
                frag.type = Shader.Simple;

                // Fill shader
                glnvg__convertPaint(ref FragUniformRef(call.uniformOffset / this.fragSize + 1/* + gl->fragSize*/), ref paint, ref scissor, fringe, fringe, -1.0f);
            }
            else
            {
                call.uniformOffset = AllocFragUniforms(1);

                // Fill shader
                glnvg__convertPaint(ref FragUniformRef(call.uniformOffset / this.fragSize), ref paint, ref scissor, fringe, fringe, -1.0f);
            }
        }

        public void RenderStroke(
            ref Paint paint,
            CompositeOperationState compositeOperation,
            ref ScissorInfo scissor,
            float fringe,
            float strokeWidth,
            NVG.Path[] paths,
            int npaths)
        {
            ref GlCall call = ref AllocCall();
            call.type = GlCallType.STROKE;
            call.pathOffset = AllocPaths(npaths);
            call.pathCount = npaths;
            call.image = paint.image;
            call.blendFunc = BlendCompositeOperation(compositeOperation);

            // Allocate vertices for all the paths.
            int maxverts = GetMaxVertCount(paths, npaths);
            int offset = AllocVerts(maxverts);

            for (int i = 0; i < npaths; i++)
            {
                ref GlPath copy = ref this.paths[call.pathOffset + i];
                ref NVG.Path path = ref paths[i];
                path = new NVG.Path();
                if (path.nstroke > 0)
                {
                    copy.strokeOffset = offset;
                    copy.strokeCount = path.nstroke;
                    Array.Copy(path.stroke.Array, path.stroke.Offset, verts, offset, path.nstroke);
                    offset += path.nstroke;
                }
            }

            if (flags.HasFlag(NVGFlags.StencilStrokes))
            {
                // Fill shader
                call.uniformOffset = AllocFragUniforms(2);
                glnvg__convertPaint(ref FragUniformRef(call.uniformOffset / this.fragSize), ref paint, ref scissor, strokeWidth, fringe, -1.0f);
                glnvg__convertPaint(ref FragUniformRef(call.uniformOffset / this.fragSize + 1/*+ gl->fragSize*/), ref paint, ref scissor, strokeWidth, fringe, 1.0f - 0.5f / 255.0f);
            }
            else
            {
                // Fill shader
                call.uniformOffset = AllocFragUniforms(1);
                glnvg__convertPaint(ref FragUniformRef(call.uniformOffset / this.fragSize), ref paint, ref scissor, strokeWidth, fringe, -1.0f);
            }
        }

        public void RenderTriangles(
            ref Paint paint,
            CompositeOperationState compositeOperation,
            ref ScissorInfo scissor,
            Vertex[] verts,
            int nverts)
        {
            ref GlCall call = ref AllocCall();
            call.type = GlCallType.TRIANGLES;
            call.image = paint.image;
            call.blendFunc = BlendCompositeOperation(compositeOperation);

            // Allocate vertices for all the paths.
            call.triangleOffset = AllocVerts(nverts);
            call.triangleCount = nverts;

            Array.Copy(verts, 0, verts, call.triangleOffset, nverts);

            // Fill shader
            call.uniformOffset = AllocFragUniforms(1);
            ref var frag = ref FragUniformRef(call.uniformOffset / this.fragSize);
            glnvg__convertPaint(ref frag, ref paint, ref scissor, 1.0f, 1.0f, -1.0f);
            frag.type = Shader.Image;
        }

        public void RenderDelete()
        {
            GlShader.DeleteShader(ref shader);

#if NANOVG_GL3
#if NANOVG_GL_USE_UNIFORMBUFFER
            if (fragBuf != 0)
                GL.DeleteBuffers(1, ref fragBuf);
#endif
            if (vertArr != 0)
                GL.DeleteVertexArrays(1, ref vertArr);
#endif
            if (vertBuf != 0)
                GL.DeleteBuffers(1, ref vertBuf);

            for (int i = 0; i < ntextures; i++)
            {
                if (textures[i].tex != 0 && !textures[i].flags.HasFlag(ImageFlagsGL.NoDelete))
                    GL.DeleteTextures(1, ref textures[i].tex);
            }

            // TODO> Probably not needed - all ref types
            ////free(gl.textures);
            ////free(gl.paths);
            ////free(gl.verts);
            ////free(gl.uniforms);
            ////free(gl.calls);
            ////free(gl);
        }

        private void GlBindTexture(uint tex)
        {
            if (boundTexture != tex)
            {
                boundTexture = tex;
                GL.BindTexture(TextureTarget.Texture2D, tex);
            }
        }

        private void GlStencilMask(uint mask)
        {
            if (stencilMask != mask)
            {
                stencilMask = mask;
                GL.StencilMask(mask);
            }
        }

        private void GlStencilFunc(StencilFunction func, int @ref, uint mask)
        {
            if (stencilFunc != func || stencilFuncRef != @ref || stencilFuncMask != mask)
            {
                stencilFunc = func;
                stencilFuncRef = @ref;
                stencilFuncMask = mask;
                GL.StencilFunc(func, @ref, mask);
            }
        }

        private void GlBlendFuncSeparate(ref GlBlend blend)
        {
            if (blendFunc.srcRGB != blend.srcRGB
                || blendFunc.dstRGB != blend.dstRGB
                || blendFunc.srcAlpha != blend.srcAlpha
                || blendFunc.dstAlpha != blend.dstAlpha)
            {
                blendFunc = blend;
                GL.BlendFuncSeparate(blend.srcRGB, blend.dstRGB, blend.srcAlpha, blend.dstAlpha);
            }
        }

        private ref GlTexture AllocTexture()
        {
            int texi = -1;

            for (int i = 0; i < ntextures; i++)
            {
                if (textures[i].id == 0)
                {
                    texi = i;
                    break;
                }
            }

            if (texi == -1)
            {
                if (ntextures + 1 > ctextures)
                {
                    int newctextures = Math.Max(ntextures + 1, 4) + ctextures / 2; // 1.5x Overallocate
                    Array.Resize(ref textures, newctextures);
                    ctextures = newctextures;
                }

                texi = ++ntextures;
            }

            textures[texi] = new GlTexture
            {
                id = ++textureId,
            };

            return ref textures[texi];
        }

        private bool FindTexture(int id, out GlTexture tex)
        {
            for (int i = 0; i < ntextures; i++)
            {
                if (textures[i].id == id)
                {
                    tex = textures[i];
                    return true;
                }
            }

            tex = default;
            return false;
        }

        private int DeleteTexture(int id)
        {
            for (int i = 0; i < ntextures; i++)
            {
                if (textures[i].id == id)
                {
                    if (textures[i].tex != 0 && !textures[i].flags.HasFlag(ImageFlagsGL.NoDelete))
                    {
                        GL.DeleteTextures(1, ref textures[i].tex);
                    }

                    textures[i] = new GlTexture();
                    return 1;
                }
            }

            return 0; // todo: return bool
        }

        private void GlDebugError(string str)
        {
            if (!flags.HasFlag(NVGFlags.Debug))
            {
                return;
            }

            var err = GL.GetError();
            if (err != ErrorCode.NoError)
            {
                Debug.WriteLine($"OpenGL error {err} after {str}", "OPENGL");
            }
        }

        private void glnvg__convertPaint(
            ref GlFragUniforms frag,
            ref Paint paint,
            ref ScissorInfo scissor,
            float width,
            float fringe,
            float strokeThr)
        {
            Transform2D invxform;

            frag = new GlFragUniforms();
            frag.innerCol = Color.Premultiply(paint.innerColor);
            frag.outerCol = Color.Premultiply(paint.outerColor);

            if (scissor.extent.X < -0.5f || scissor.extent.Y < -0.5f)
            {
                frag.scissorMat.Clear();
                frag.scissorExt.X = 1.0f;
                frag.scissorExt.Y = 1.0f;
                frag.scissorScale.X = 1.0f;
                frag.scissorScale.Y = 1.0f;
            }
            else
            {
                invxform = scissor.xform.Inverse();
                frag.scissorMat = Mat3x4.FromTransform2D(invxform);
                frag.scissorExt = scissor.extent;
                frag.scissorScale.X = (float)Math.Sqrt(scissor.xform.R1C1 * scissor.xform.R1C1 + scissor.xform.R1C2 * scissor.xform.R1C2) / fringe;
                frag.scissorScale.Y = (float)Math.Sqrt(scissor.xform.R2C1 * scissor.xform.R2C1 + scissor.xform.R2C2 * scissor.xform.R2C2) / fringe;
            }

            frag.extent = paint.extent;
            frag.strokeMult = (width * 0.5f + fringe * 0.5f) / fringe;
            frag.strokeThr = strokeThr;

            if (paint.image != 0)
            {
                if (!FindTexture(paint.image, out var tex))
                {
                    throw new ArgumentException("Paint texture not found");
                }

                if (tex.flags.HasFlag(ImageFlags.FlipY))
                {
                    var m1 = Transform2D.Translation(0.0f, frag.extent.Y * 0.5f);
                    Transform2D.Multiply(ref m1, paint.xform);
                    var m2 = Transform2D.Scaling(1.0f, -1.0f);
                    Transform2D.Multiply(ref m2, m1);
                    m1 = Transform2D.Translation(0.0f, -frag.extent.Y * 0.5f);
                    Transform2D.Multiply(ref m1, m2);
                    invxform = m1.Inverse();
                }
                else
                {
                    invxform = paint.xform.Inverse();
                }

                frag.type = Shader.FillImage;

#if NANOVG_GL_USE_UNIFORMBUFFER
                if (tex.type == Texture.RGBA)
                {
                    frag.texType = tex.flags.HasFlag(ImageFlags.Premultiplied) ? 0 : 1;
                }
                else
                {
                    frag.texType = 2;
                }
#else
                if (tex.type == Texture.RGBA)
                    frag.texType = tex.flags.HasFlag(ImageFlags.Premultiplied) ? 0 : 1;
                else
                    frag.texType = 2;
#endif
            }
            else
            {
                frag.type = Shader.FillGradient;
                frag.radius = paint.radius;
                frag.feather = paint.feather;
                invxform = paint.xform.Inverse();
            }

            frag.paintMat = Mat3x4.FromTransform2D(invxform);
        }

        private void GlSetUniforms(int uniformOffset, int image)
        {
#if NANOVG_GL_USE_UNIFORMBUFFER
            GL.BindBufferRange(
                target: BufferRangeTarget.UniformBuffer,
                index: (int)UniformBindings.FRAG_BINDING,
                buffer: this.fragBuf,
                offset: (IntPtr)uniformOffset,
                size: Marshal.SizeOf(typeof(GlFragUniforms)));
            GlDebugError("bind buff range");
#else
            ref GLNVGfragUniforms frag = ref FragUniformRef(uniformOffset);
            GL.Uniform4(shader.locFrag, NANOVG_GL_UNIFORMARRAY_SIZE, frag.uniformArray);
#endif

            if (image != 0)
            {
                var foundtex = FindTexture(image, out var tex);
                GlBindTexture(foundtex ? tex.tex : 0);
                GlDebugError("tex paint bind texture");
            }
            else
            {
                GlBindTexture(0);
            }
        }

        private void FlushFill(ref GlCall call)
        {
            var paths = new Span<GlPath>(this.paths, call.pathOffset, call.pathCount);

            // Draw shapes
            GL.Enable(EnableCap.StencilTest);
            GlStencilMask(0xff);
            GlStencilFunc(StencilFunction.Always, 0, 0xff);
            GL.ColorMask(false, false, false, false);
            GlDebugError("fill init");

            // set bindpoint for solid loc
            GlSetUniforms(call.uniformOffset, 0);
            GlDebugError("fill simple");

            GL.StencilOpSeparate(StencilFace.Front, StencilOp.Keep, StencilOp.Keep, StencilOp.IncrWrap);
            GL.StencilOpSeparate(StencilFace.Back, StencilOp.Keep, StencilOp.Keep, StencilOp.DecrWrap);
            GL.Disable(EnableCap.CullFace);
            for (int i = 0; i < paths.Length; i++)
            {
                GL.DrawArrays(BeginMode.TriangleFan, paths[i].fillOffset, paths[i].fillCount);
            }

            GL.Enable(EnableCap.CullFace);

            // Draw anti-aliased pixels
            GL.ColorMask(true, true, true, true);

            GlSetUniforms(call.uniformOffset + fragSize, call.image);
            GlDebugError("fill fill");

            if (flags.HasFlag(NVGFlags.AntiAlias))
            {
                GlStencilFunc(StencilFunction.Equal, 0x00, 0xff);
                GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);

                // Draw fringes
                for (int i = 0; i < paths.Length; i++)
                {
                    GL.DrawArrays(BeginMode.TriangleStrip, paths[i].strokeOffset, paths[i].strokeCount);
                }
            }

            // Draw fill
            GlStencilFunc(StencilFunction.Notequal, 0x0, 0xff);
            GL.StencilOp(StencilOp.Zero, StencilOp.Zero, StencilOp.Zero);
            GL.DrawArrays(BeginMode.TriangleStrip, call.triangleOffset, call.triangleCount);

            GL.Disable(EnableCap.StencilTest);
        }

        private void FlushConvexFill(ref GlCall call)
        {
            var paths = new Span<GlPath>(this.paths, call.pathOffset, call.pathCount);

            GlSetUniforms(call.uniformOffset, call.image);
            GlDebugError("convex fill");

            for (int i = 0; i < paths.Length; i++)
            {
                GL.DrawArrays(BeginMode.TriangleFan, paths[i].fillOffset, paths[i].fillCount);

                // Draw fringes
                if (paths[i].strokeCount > 0)
                {
                    GL.DrawArrays(BeginMode.TriangleStrip, paths[i].strokeOffset, paths[i].strokeCount);
                }
            }
        }

        private void FlushStroke(ref GlCall call)
        {
            var paths = new Span<GlPath>(this.paths, call.pathOffset, call.pathCount);

            if (flags.HasFlag(NVGFlags.StencilStrokes))
            {
                GL.Enable(EnableCap.StencilTest);
                GlStencilMask(0xff);

                // Fill the stroke base without overlap
                GlStencilFunc(StencilFunction.Equal, 0x0, 0xff);
                GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Incr);
                GlSetUniforms(call.uniformOffset + fragSize, call.image);
                GlDebugError("stroke fill 0");
                for (int i = 0; i < paths.Length; i++)
                {
                    GL.DrawArrays(BeginMode.TriangleStrip, paths[i].strokeOffset, paths[i].strokeCount);
                }

                // Draw anti-aliased pixels.
                GlSetUniforms(call.uniformOffset, call.image);
                GlStencilFunc(StencilFunction.Equal, 0x00, 0xff);
                GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);
                for (int i = 0; i < paths.Length; i++)
                {
                    GL.DrawArrays(BeginMode.TriangleStrip, paths[i].strokeOffset, paths[i].strokeCount);
                }

                // Clear stencil buffer.
                GL.ColorMask(false, false, false, false);
                GlStencilFunc(StencilFunction.Always, 0x0, 0xff);
                GL.StencilOp(StencilOp.Zero, StencilOp.Zero, StencilOp.Zero);
                GlDebugError("stroke fill 1");
                for (int i = 0; i < paths.Length; i++)
                {
                    GL.DrawArrays(BeginMode.TriangleStrip, paths[i].strokeOffset, paths[i].strokeCount);
                }

                GL.ColorMask(true, true, true, true);

                GL.Disable(EnableCap.StencilTest);

                // nottodo: commented out in source
                ////glnvg__convertPaint(gl, nvg__fragUniformPtr(gl, call->uniformOffset + gl->fragSize), paint, scissor, strokeWidth, fringe, 1.0f - 0.5f/255.0f);
            }
            else
            {
                GlSetUniforms(call.uniformOffset, call.image);
                GlDebugError("stroke fill");

                // Draw Strokes
                for (int i = 0; i < paths.Length; i++)
                {
                    GL.DrawArrays(BeginMode.TriangleStrip, paths[i].strokeOffset, paths[i].strokeCount);
                }
            }
        }

        private void FlushTriangles(ref GlCall call)
        {
            GlSetUniforms(call.uniformOffset, call.image);
            GlDebugError("triangles fill");

            GL.DrawArrays(BeginMode.Triangles, call.triangleOffset, call.triangleCount);
        }

        private ref GlCall AllocCall()
        {
            if (ncalls + 1 > ccalls)
            {
                int newccalls = Math.Max(ncalls + 1, 128) + ccalls / 2; // 1.5x Overallocate
                Array.Resize(ref calls, newccalls);
                ccalls = newccalls;
            }

            ref var ret = ref calls[ncalls++];
            ret = new GlCall();
            return ref ret;
        }

        private int AllocPaths(int n)
        {
            if (npaths + n > cpaths)
            {
                int newcpaths = Math.Max(npaths + n, 128) + cpaths / 2; // 1.5x Overallocate
                Array.Resize(ref paths, newcpaths);
                cpaths = newcpaths;
            }

            int ret = npaths;
            npaths += n;
            return ret;
        }

        private int AllocVerts(int n)
        {
            if (nverts + n > cverts)
            {
                int newcverts = Math.Max(nverts + n, 4096) + cverts / 2; // 1.5x Overallocate
                Array.Resize(ref verts, newcverts);
                cverts = newcverts;
            }

            int ret = nverts;
            nverts += n;
            return ret;
        }

        private int AllocFragUniforms(int n)
        {
            int structSize = this.fragSize;

            if (nuniforms + n > cuniforms)
            {
                int newcuniforms = Math.Max(nuniforms + n, 128) + cuniforms / 2; // 1.5x Overallocate
                Array.Resize(ref uniforms, newcuniforms);
                cuniforms = newcuniforms;
            }

            int ret = nuniforms * structSize;
            nuniforms += n;
            return ret;
        }

        private ref GlFragUniforms FragUniformRef(int i)
        {
            return ref uniforms[i];
        }

#if NANOVG_GL2
        int nvglCreateImageFromHandleGL2(uint textureId, int w, int h, NVGimageFlagsGL imageFlags)
#elif NANOVG_GL3
        int nvglCreateImageFromHandleGL3(uint textureId, int w, int h, ImageFlags imageFlags)
#elif NANOVG_GLES2
        int nvglCreateImageFromHandleGLES2(uint textureId, int w, int h, NVGimageFlagsGL imageFlags)
#elif NANOVG_GLES3
        int nvglCreateImageFromHandleGLES3(uint textureId, int w, int h, NVGimageFlagsGL imageFlags)
#endif
        {
            ref GlTexture tex = ref AllocTexture();

            tex.type = Texture.RGBA;
            tex.tex = textureId;
            tex.flags = imageFlags;
            tex.width = w;
            tex.height = h;

            return tex.id;
        }

#if NANOVG_GL2
        uint nvglImageHandleGL2(int image)
#elif NANOVG_GL3
        uint nvglImageHandleGL3(int image)
#elif NANOVG_GLES2
        uint nvglImageHandleGLES2(int image)
#elif NANOVG_GLES3
        uint nvglImageHandleGLES3(int image)
#endif
        {
            FindTexture(image, out var tex);
            return tex.tex;
        }

        private static BlendingFactorSrc ConvertBlendFuncFactorSrc(BlendFactor factor)
        {
            if (factor == BlendFactor.ZERO)
                return BlendingFactorSrc.Zero;
            if (factor == BlendFactor.ONE)
                return BlendingFactorSrc.One;
            if (factor == BlendFactor.SRC_COLOR)
                return BlendingFactorSrc.SrcColor;
            if (factor == BlendFactor.ONE_MINUS_SRC_COLOR)
                return BlendingFactorSrc.OneMinusSrcColor;
            if (factor == BlendFactor.DST_COLOR)
                return BlendingFactorSrc.DstColor;
            if (factor == BlendFactor.ONE_MINUS_DST_COLOR)
                return BlendingFactorSrc.OneMinusDstColor;
            if (factor == BlendFactor.SRC_ALPHA)
                return BlendingFactorSrc.SrcAlpha;
            if (factor == BlendFactor.ONE_MINUS_SRC_ALPHA)
                return BlendingFactorSrc.OneMinusSrcAlpha;
            if (factor == BlendFactor.DST_ALPHA)
                return BlendingFactorSrc.DstAlpha;
            if (factor == BlendFactor.ONE_MINUS_DST_ALPHA)
                return BlendingFactorSrc.OneMinusDstAlpha;
            if (factor == BlendFactor.SRC_ALPHA_SATURATE)
                return BlendingFactorSrc.SrcAlphaSaturate;
            throw new ArgumentException(nameof(factor));
            //return BlendingFactor.GL_INVALID_ENUM;
        }

        private static BlendingFactorDest ConvertBlendFuncFactorDest(BlendFactor factor)
        {
            if (factor == BlendFactor.ZERO)
                return BlendingFactorDest.Zero;
            if (factor == BlendFactor.ONE)
                return BlendingFactorDest.One;
            if (factor == BlendFactor.SRC_COLOR)
                return BlendingFactorDest.SrcColor;
            if (factor == BlendFactor.ONE_MINUS_SRC_COLOR)
                return BlendingFactorDest.OneMinusSrcColor;
            if (factor == BlendFactor.DST_COLOR)
                return BlendingFactorDest.DstColor;
            if (factor == BlendFactor.ONE_MINUS_DST_COLOR)
                return BlendingFactorDest.OneMinusDstColor;
            if (factor == BlendFactor.SRC_ALPHA)
                return BlendingFactorDest.SrcAlpha;
            if (factor == BlendFactor.ONE_MINUS_SRC_ALPHA)
                return BlendingFactorDest.OneMinusSrcAlpha;
            if (factor == BlendFactor.DST_ALPHA)
                return BlendingFactorDest.DstAlpha;
            if (factor == BlendFactor.ONE_MINUS_DST_ALPHA)
                return BlendingFactorDest.OneMinusDstAlpha;
            if (factor == BlendFactor.SRC_ALPHA_SATURATE)
                return BlendingFactorDest.SrcAlphaSaturate;
            throw new ArgumentException(nameof(factor));
            //return BlendingFactor.GL_INVALID_ENUM;
        }

        private static GlBlend BlendCompositeOperation(CompositeOperationState op)
        {
            GlBlend blend;
            blend.srcRGB = ConvertBlendFuncFactorSrc(op.srcRGB);
            blend.dstRGB = ConvertBlendFuncFactorDest(op.dstRGB);
            blend.srcAlpha = ConvertBlendFuncFactorSrc(op.srcAlpha);
            blend.dstAlpha = ConvertBlendFuncFactorDest(op.dstAlpha);
            ////if (blend.srcRGB == GL_INVALID_ENUM
            ////    || blend.dstRGB == GL_INVALID_ENUM
            ////    || blend.srcAlpha == GL_INVALID_ENUM
            ////    || blend.dstAlpha == GL_INVALID_ENUM)
            ////{
            ////    blend.srcRGB = GL_ONE;
            ////    blend.dstRGB = GL_ONE_MINUS_SRC_ALPHA;
            ////    blend.srcAlpha = GL_ONE;
            ////    blend.dstAlpha = GL_ONE_MINUS_SRC_ALPHA;
            ////}

            return blend;
        }

        private static int GetMaxVertCount(NVG.Path[] paths, int npaths)
        {
            int i, count = 0;
            for (i = 0; i < npaths; i++)
            {
                count += paths[i].nfill;
                count += paths[i].nstroke;
            }

            return count;
        }

#if NANOVG_GLES2
        private static uint glnvg__nearestPow2(uint num)
        {
            uint n = num > 0 ? num - 1 : 0;
            n |= n >> 1;
            n |= n >> 2;
            n |= n >> 4;
            n |= n >> 8;
            n |= n >> 16;
            n++;
            return n;
        }
#endif
    }
}
