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

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NanoVG
{
    public static class NanoVGGL
    {
        //// Create flags
        [Flags]
        public enum CreateFlags
        {
            // Flag indicating if geometry based anti-aliasing is used (may not be needed when using MSAA).
            NVG_ANTIALIAS = 1 << 0,
            // Flag indicating if strokes should be drawn using stencil buffer. The rendering will be a little
            // slower, but path overlaps (i.e. self-intersecting or sharp turns) will be drawn just once.
            NVG_STENCIL_STROKES = 1 << 1,
            // Flag indicating that additional debug checks are done.
            NVG_DEBUG = 1 << 2,
        }

        // These are additional flags on top of NVGimageFlags.
        enum ImageFlagsGL
        {
            NVG_IMAGE_NODELETE = 1 << 16,   // Do not delete GL texture handle.
        }

        enum GLNVGshaderType
        {
            NSVG_SHADER_FILLGRAD,
            NSVG_SHADER_FILLIMG,
            NSVG_SHADER_SIMPLE,
            NSVG_SHADER_IMG,
        }

#if NANOVG_GL_USE_UNIFORMBUFFER
        enum GLNVGuniformBindings
        {
            GLNVG_FRAG_BINDING = 0,
        }
#endif

        struct Mat3x4
        {
            public float R1C1;
            public float R2C1;
            public float R3C1;
            public float R4C1;
            public float R1C2;
            public float R2C2;
            public float R3C2;
            public float R4C2;
            public float R1C3;
            public float R2C3;
            public float R3C3;
            public float R4C3;

            public void Clear()
            {
                R1C1
                    = R2C1
                    = R3C1
                    = R4C1
                    = R1C2
                    = R2C2
                    = R3C2
                    = R4C2
                    = R1C3
                    = R2C3
                    = R3C3
                    = R4C3 = 0;
            }
        }

        struct GLNVGshader
        {
            public int prog;
            public int frag;
            public int vert;
            public int locViewSize;
            public int locTex;
            public int locFrag;
        }

        struct GLNVGtexture
        {
            public int id;
            public uint tex;
            public int width;
            public int height;
            public Context.Texture type;
            public ImageFlags flags;
        }

        struct GLNVGblend
        {
            public BlendingFactorSrc srcRGB;
            public BlendingFactorDest dstRGB;
            public BlendingFactorSrc srcAlpha;
            public BlendingFactorDest dstAlpha;
        }

        enum GLNVGcallType
        {
            GLNVG_NONE = 0,
            GLNVG_FILL,
            GLNVG_CONVEXFILL,
            GLNVG_STROKE,
            GLNVG_TRIANGLES,
        }

        struct GLNVGcall
        {
            public GLNVGcallType type;
            public int image;
            public int pathOffset;
            public int pathCount;
            public int triangleOffset;
            public int triangleCount;
            public int uniformOffset;
            public GLNVGblend blendFunc;
        }

        struct GLNVGpath
        {
            public int fillOffset;
            public int fillCount;
            public int strokeOffset;
            public int strokeCount;
        }

        struct GLNVGfragUniforms
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
            public GLNVGshaderType type;
        }

        class GLNVGcontext
        {
            public GLNVGshader shader;
            public Extent2D view;

            public GLNVGtexture[] textures;
            public int ntextures;
            public int ctextures;

            public int textureId;
            public uint vertBuf;
#if NANOVG_GL3
            public uint vertArr;
#endif
            public uint fragBuf;

            public int fragSize;
            public CreateFlags flags;

            // Per frame buffers
            public GLNVGcall[] calls;
            public int ccalls;
            public int ncalls;

            public GLNVGpath[] paths;
            public int cpaths;
            public int npaths;

            public Vertex[] verts;
            public int cverts;
            public int nverts;

            public GLNVGfragUniforms[] uniforms;
            public int cuniforms;
            public int nuniforms;

            // cached state
            public uint boundTexture;
            public uint stencilMask;
            public StencilFunction stencilFunc;
            public int stencilFuncRef;
            public uint stencilFuncMask;
            public GLNVGblend blendFunc;
        }

#if NANOVG_GLES2
        static uint glnvg__nearestPow2(uint num)
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

        static void glnvg__bindTexture(GLNVGcontext gl, uint tex)
        {
            if (gl.boundTexture != tex)
            {
                gl.boundTexture = tex;
                GL.BindTexture(TextureTarget.Texture2D, tex);
            }
        }

        static void glnvg__stencilMask(GLNVGcontext gl, uint mask)
        {
            if (gl.stencilMask != mask)
            {
                gl.stencilMask = mask;
                GL.StencilMask(mask);
            }
        }

        static void glnvg__stencilFunc(GLNVGcontext gl, StencilFunction func, int @ref, uint mask)
        {
            if (gl.stencilFunc != func
                || gl.stencilFuncRef != @ref
                || gl.stencilFuncMask != mask)
            {
                gl.stencilFunc = func;
                gl.stencilFuncRef = @ref;
                gl.stencilFuncMask = mask;
                GL.StencilFunc(func, @ref, mask);
            }
        }

        static void glnvg__blendFuncSeparate(GLNVGcontext gl, ref GLNVGblend blend)
        {
            if (gl.blendFunc.srcRGB != blend.srcRGB
                || gl.blendFunc.dstRGB != blend.dstRGB
                || gl.blendFunc.srcAlpha != blend.srcAlpha
                || gl.blendFunc.dstAlpha != blend.dstAlpha)
            {
                gl.blendFunc = blend;
                GL.BlendFuncSeparate(blend.srcRGB, blend.dstRGB, blend.srcAlpha, blend.dstAlpha);
            }
        }

        static ref GLNVGtexture glnvg__allocTexture(GLNVGcontext gl)
        {
            int texi = -1;

            for (int i = 0; i < gl.ntextures; i++)
            {
                if (gl.textures[i].id == 0)
                {
                    texi = i;
                    break;
                }
            }

            if (texi == -1)
            {
                if (gl.ntextures + 1 > gl.ctextures)
                {
                    int ctextures = Math.Max(gl.ntextures + 1, 4) + gl.ctextures / 2; // 1.5x Overallocate
                    Array.Resize(ref gl.textures, ctextures);
                    gl.ctextures = ctextures;
                }

                texi = ++gl.ntextures;
            }

            gl.textures[texi] = new GLNVGtexture
            {
                id = ++gl.textureId,
            };

            return ref gl.textures[texi];
        }

        static bool glnvg__findTexture(GLNVGcontext gl, int id, out GLNVGtexture tex)
        {
            for (int i = 0; i < gl.ntextures; i++)
            {
                if (gl.textures[i].id == id)
                {
                    tex = gl.textures[i];
                    return true;
                }
            }

            tex = default;
            return false;
        }

        static int glnvg__deleteTexture(GLNVGcontext gl, int id)
        {
            for (int i = 0; i < gl.ntextures; i++)
            {
                if (gl.textures[i].id == id)
                {
                    if (gl.textures[i].tex != 0 && !gl.textures[i].flags.HasFlag(ImageFlagsGL.NVG_IMAGE_NODELETE))
                    {
                        GL.DeleteTextures(1, ref gl.textures[i].tex);
                    }

                    gl.textures[i] = new GLNVGtexture();
                    return 1;
                }
            }

            return 0; // todo: return bool
        }

        static void glnvg__dumpProgramError(int prog, string name)
        {
            var str = GL.GetProgramInfoLog(prog);
            Debug.WriteLine($"Program {name} error: {str}", name, str);
        }

        static void glnvg__checkError(GLNVGcontext gl, string str)
        {
            if (!gl.flags.HasFlag(CreateFlags.NVG_DEBUG))
            {
                return;
            }

            var err = GL.GetError();
            if (err != ErrorCode.NoError)
            {
                Debug.WriteLine($"Error {err} after {str}");
                return;
            }
        }

        static int glnvg__createShader(ref GLNVGshader shader, string name, string header, string opts, string vshader, string fshader)
        {
            bool TryMakeShader(ShaderType type, string body, out int id)
            {
                id = GL.CreateShader(type);
                GL.ShaderSource(id, header + (opts ?? string.Empty) + body);
                GL.CompileShader(id);
                GL.GetShader(id, ShaderParameter.CompileStatus, out var compileStatus);
                if (compileStatus != (int)OpenTK.Graphics.OpenGL.Boolean.True)
                {
                    Debug.WriteLine($"{name} {type} compile error: {GL.GetShaderInfoLog(id)}");
                    return false;
                }

                return true;
            }

            if (!TryMakeShader(ShaderType.VertexShader, vshader, out var vert))
            {
                return 0;
            }

            if (!TryMakeShader(ShaderType.FragmentShader, fshader, out var frag))
            {
                return 0;
            }

            var prog = GL.CreateProgram();
            GL.AttachShader(prog, vert);
            GL.AttachShader(prog, frag);
            GL.BindAttribLocation(prog, 0, "vertex");
            GL.BindAttribLocation(prog, 1, "tcoord");
            GL.LinkProgram(prog);
            GL.GetProgram(prog, GetProgramParameterName.LinkStatus, out var linkStatus);
            if (linkStatus != (int)OpenTK.Graphics.OpenGL.Boolean.True)
            {
                glnvg__dumpProgramError(prog, name);
                return 0;
            }

            shader = new GLNVGshader();
            shader.prog = prog;
            shader.vert = vert;
            shader.frag = frag;

            return 1; // todo: return bool (and rename to try..?)
        }

        static void glnvg__deleteShader(ref GLNVGshader shader)
        {
            if (shader.prog != 0)
            {
                GL.DeleteProgram(shader.prog);
            }

            if (shader.vert != 0)
            {
                GL.DeleteShader(shader.vert);
            }

            if (shader.frag != 0)
            {
                GL.DeleteShader(shader.frag);
            }
        }

        static void glnvg__getUniforms(ref GLNVGshader shader)
        {
            shader.locViewSize = GL.GetUniformLocation(shader.prog, "viewSize");
            shader.locTex = GL.GetUniformLocation(shader.prog, "tex");
            shader.locFrag = GL.GetUniformBlockIndex(shader.prog, "frag");
        }

        static int glnvg__renderCreate(object uptr)
        {
            GLNVGcontext gl = (GLNVGcontext)uptr;
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

            glnvg__checkError(gl, "init");

            if (gl.flags.HasFlag(CreateFlags.NVG_ANTIALIAS))
            {
                if (glnvg__createShader(ref gl.shader, "shader", shaderHeader, "#define EDGE_AA 1\n", fillVertShader, fillFragShader) == 0)
                {
                    return 0;
                }
            }
            else
            {
                if (glnvg__createShader(ref gl.shader, "shader", shaderHeader, null, fillVertShader, fillFragShader) == 0)
                {
                    return 0;
                }
            }

            glnvg__checkError(gl, "uniform locations");
            glnvg__getUniforms(ref gl.shader);

            // Create dynamic vertex array
#if NANOVG_GL3
            GL.GenVertexArrays(1, out gl.vertArr);
#endif
            GL.GenBuffers(1, out gl.vertBuf);

#if NANOVG_GL_USE_UNIFORMBUFFER
            // Create UBOs
            GL.UniformBlockBinding(
                gl.shader.prog,
                gl.shader.locFrag,
                (int)GLNVGuniformBindings.GLNVG_FRAG_BINDING);
            GL.GenBuffers(1, out gl.fragBuf);
            GL.GetInteger(GetPName.UniformBufferOffsetAlignment, out align);
#endif
            gl.fragSize = Marshal.SizeOf(typeof(GLNVGfragUniforms));

            glnvg__checkError(gl, "create done");

            GL.Finish();

            return 1;
        }

        static int glnvg__renderCreateTexture(object cxt, Context.Texture type, int w, int h, ImageFlags imageFlags, byte[] data)
        {
            GLNVGcontext gl = (GLNVGcontext)cxt;
            ref GLNVGtexture tex = ref glnvg__allocTexture(gl);

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
            glnvg__bindTexture(gl, tex.tex);

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

            if (type == Context.Texture.NVG_TEXTURE_RGBA)
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

            if (imageFlags.HasFlag(ImageFlags.IMAGE_GENERATE_MIPMAPS))
            {
                if (imageFlags.HasFlag(ImageFlags.IMAGE_NEAREST))
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
                if (imageFlags.HasFlag(ImageFlags.IMAGE_NEAREST))
                {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                }
                else
                {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                }
            }

            if (imageFlags.HasFlag(ImageFlags.IMAGE_NEAREST))
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            }
            else
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            }

            if (imageFlags.HasFlag(ImageFlags.IMAGE_REPEATX))
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            }
            else
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            }

            var textureWrapModeT = imageFlags.HasFlag(ImageFlags.IMAGE_REPEATY) ? (int)TextureWrapMode.Repeat : (int)TextureWrapMode.ClampToEdge;
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, textureWrapModeT);

            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 4);
#if !NANOVG_GLES2
            GL.PixelStore(PixelStoreParameter.UnpackRowLength, 0);
            GL.PixelStore(PixelStoreParameter.UnpackSkipPixels, 0);
            GL.PixelStore(PixelStoreParameter.UnpackSkipRows, 0);
#endif

            // The new way to build mipmaps on GLES and GL3
#if !NANOVG_GL2
            if (imageFlags.HasFlag(ImageFlags.IMAGE_GENERATE_MIPMAPS))
            {
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            }
#endif

            glnvg__checkError(gl, "create tex");
            glnvg__bindTexture(gl, 0);

            return tex.id;
        }

        static int glnvg__renderDeleteTexture(object cxt, int image)
        {
            GLNVGcontext gl = (GLNVGcontext)cxt;
            return glnvg__deleteTexture(gl, image);
        }

        static int glnvg__renderUpdateTexture(object cxt, int image, int x, int y, int w, int h, byte[] data)
        {
            GLNVGcontext gl = (GLNVGcontext)cxt;
            if (!glnvg__findTexture(gl, image, out var tex)) return 0;

            glnvg__bindTexture(gl, tex.tex);

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

            if (tex.type == Context.Texture.NVG_TEXTURE_RGBA)
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

            glnvg__bindTexture(gl, 0);

            return 1;
        }

        static int glnvg__renderGetTextureSize(object cxt, int image, out int w, out int h)
        {
            GLNVGcontext gl = (GLNVGcontext)cxt;
            if (!glnvg__findTexture(gl, image, out var tex))
            {
                w = h = 0;
                return 0;
            }

            w = tex.width;
            h = tex.height;
            return 1;
        }

        static void glnvg__xformToMat3x4(ref Mat3x4 m3, Transform2D t)
        {
            m3.R1C1 = t.R1C1;
            m3.R2C1 = t.R2C1;
            m3.R3C1 = 0.0f;
            m3.R4C1 = 0.0f;
            m3.R1C2 = t.R1C2;
            m3.R2C2 = t.R2C2;
            m3.R3C2 = 0.0f;
            m3.R4C2 = 0.0f;
            m3.R1C3 = t.R1C3;
            m3.R2C3 = t.R2C3;
            m3.R3C3 = 1.0f;
            m3.R4C3 = 0.0f;
        }

        static Color glnvg__premulColor(Color c)
        {
            c.R *= c.A;
            c.G *= c.A;
            c.B *= c.A;
            return c;
        }

        static int glnvg__convertPaint(
            GLNVGcontext gl,
            ref GLNVGfragUniforms frag,
            ref Paint paint,
            ref Context.nvgScissor scissor,
            float width,
            float fringe,
            float strokeThr)
        {
            Transform2D invxform = new Transform2D();

            frag = new GLNVGfragUniforms();

            frag.innerCol = glnvg__premulColor(paint.innerColor);
            frag.outerCol = glnvg__premulColor(paint.outerColor);

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
                scissor.xform.Inverse(out invxform);
                glnvg__xformToMat3x4(ref frag.scissorMat, invxform);
                frag.scissorExt = scissor.extent;
                frag.scissorScale.X = (float)Math.Sqrt(scissor.xform.R1C1 * scissor.xform.R1C1 + scissor.xform.R1C2 * scissor.xform.R1C2) / fringe;
                frag.scissorScale.Y = (float)Math.Sqrt(scissor.xform.R2C1 * scissor.xform.R2C1 + scissor.xform.R2C2 * scissor.xform.R2C2) / fringe;
            }

            frag.extent = paint.extent;
            frag.strokeMult = (width * 0.5f + fringe * 0.5f) / fringe;
            frag.strokeThr = strokeThr;

            if (paint.image != 0)
            {
                if (!glnvg__findTexture(gl, paint.image, out var tex)) return 0;

                if (tex.flags.HasFlag(ImageFlags.IMAGE_FLIPY))
                {
                    var m1 = Transform2D.Translate(0.0f, frag.extent.Y * 0.5f);
                    Transform2D.Multiply(ref m1, paint.xform);
                    var m2 = Transform2D.Scale(1.0f, -1.0f);
                    Transform2D.Multiply(ref m2, m1);
                    m1 = Transform2D.Translate(0.0f, -frag.extent.Y * 0.5f);
                    Transform2D.Multiply(ref m1, m2);
                    m1.Inverse(out invxform);
                }
                else
                {
                    paint.xform.Inverse(out invxform);
                }

                frag.type = GLNVGshaderType.NSVG_SHADER_FILLIMG;

#if NANOVG_GL_USE_UNIFORMBUFFER
                if (tex.type == Context.Texture.NVG_TEXTURE_RGBA)
                {
                    frag.texType = tex.flags.HasFlag(ImageFlags.IMAGE_PREMULTIPLIED) ? 0 : 1;
                }
                else
                {
                    frag.texType = 2;
                }
#else
                if (tex->type == NVG_TEXTURE_RGBA)
                    frag->texType = (tex->flags & NVG_IMAGE_PREMULTIPLIED) ? 0.0f : 1.0f;
                else
                    frag->texType = 2.0f;
#endif
                    // printf("frag->texType = %d\n", frag->texType);
                }
            else
            {
                frag.type = GLNVGshaderType.NSVG_SHADER_FILLGRAD;
                frag.radius = paint.radius;
                frag.feather = paint.feather;
                paint.xform.Inverse(out invxform);
            }

            glnvg__xformToMat3x4(ref frag.paintMat, invxform);

            return 1;
        }

        static void glnvg__setUniforms(GLNVGcontext gl, int uniformOffset, int image)
        {
#if NANOVG_GL_USE_UNIFORMBUFFER
            GL.BindBufferRange(
                target: BufferRangeTarget.UniformBuffer,
                index: (int)GLNVGuniformBindings.GLNVG_FRAG_BINDING,
                buffer: gl.fragBuf,
                offset: (IntPtr)uniformOffset,
                size: Marshal.SizeOf(typeof(GLNVGfragUniforms)));
#else
            GLNVGfragUniforms* frag = nvg__fragUniformPtr(gl, uniformOffset);
            GL.Uniform4fv(gl->shader.loc[GLNVG_LOC_FRAG], NANOVG_GL_UNIFORMARRAY_SIZE, &(frag->uniformArray[0][0]));
#endif

            if (image != 0)
            {
                var foundtex = glnvg__findTexture(gl, image, out var tex);
                glnvg__bindTexture(gl, foundtex ? tex.tex : 0);
                glnvg__checkError(gl, "tex paint tex");
            }
            else
            {
                glnvg__bindTexture(gl, 0);
            }
        }

        static void glnvg__renderViewport(object cxt, float width, float height, float devicePixelRatio)
        {
            GLNVGcontext gl = (GLNVGcontext)cxt;
            //NVG_NOTUSED(devicePixelRatio);
            gl.view.X = width;
            gl.view.Y = height;
        }

        static void glnvg__fill(GLNVGcontext gl, ref GLNVGcall call)
        {
            var paths = new Span<GLNVGpath>(gl.paths, call.pathOffset, call.pathCount);

            // Draw shapes
            GL.Enable(EnableCap.StencilTest);
            glnvg__stencilMask(gl, 0xff);
            glnvg__stencilFunc(gl, StencilFunction.Always, 0, 0xff);
            GL.ColorMask(false, false, false, false);

            // set bindpoint for solid loc
            glnvg__setUniforms(gl, call.uniformOffset, 0);
            glnvg__checkError(gl, "fill simple");

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

            glnvg__setUniforms(gl, call.uniformOffset + gl.fragSize, call.image);
            glnvg__checkError(gl, "fill fill");

            if (gl.flags.HasFlag(CreateFlags.NVG_ANTIALIAS))
            {
                glnvg__stencilFunc(gl, StencilFunction.Equal, 0x00, 0xff);
                GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);

                // Draw fringes
                for (int i = 0; i < paths.Length; i++)
                {
                    GL.DrawArrays(BeginMode.TriangleStrip, paths[i].strokeOffset, paths[i].strokeCount);
                }
            }

            // Draw fill
            glnvg__stencilFunc(gl, StencilFunction.Notequal, 0x0, 0xff);
            GL.StencilOp(StencilOp.Zero, StencilOp.Zero, StencilOp.Zero);
            GL.DrawArrays(BeginMode.TriangleStrip, call.triangleOffset, call.triangleCount);

            GL.Disable(EnableCap.StencilTest);
        }

        static void glnvg__convexFill(GLNVGcontext gl, ref GLNVGcall call)
        {
            var paths = new Span<GLNVGpath>(gl.paths, call.pathOffset, call.pathCount);

            glnvg__setUniforms(gl, call.uniformOffset, call.image);
            glnvg__checkError(gl, "convex fill");

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

        static void glnvg__stroke(GLNVGcontext gl, ref GLNVGcall call)
        {
            var paths = new Span<GLNVGpath>(gl.paths, call.pathOffset, call.pathCount);

            if (gl.flags.HasFlag(CreateFlags.NVG_STENCIL_STROKES))
            {
                GL.Enable(EnableCap.StencilTest);
                glnvg__stencilMask(gl, 0xff);

                // Fill the stroke base without overlap
                glnvg__stencilFunc(gl, StencilFunction.Equal, 0x0, 0xff);
                GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Incr);
                glnvg__setUniforms(gl, call.uniformOffset + gl.fragSize, call.image);
                glnvg__checkError(gl, "stroke fill 0");
                for (int i = 0; i < paths.Length; i++)
                {
                    GL.DrawArrays(BeginMode.TriangleStrip, paths[i].strokeOffset, paths[i].strokeCount);
                }

                // Draw anti-aliased pixels.
                glnvg__setUniforms(gl, call.uniformOffset, call.image);
                glnvg__stencilFunc(gl, StencilFunction.Equal, 0x00, 0xff);
                GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);
                for (int i = 0; i < paths.Length; i++)
                {
                    GL.DrawArrays(BeginMode.TriangleStrip, paths[i].strokeOffset, paths[i].strokeCount);
                }

                // Clear stencil buffer.
                GL.ColorMask(false, false, false, false);
                glnvg__stencilFunc(gl, StencilFunction.Always, 0x0, 0xff);
                GL.StencilOp(StencilOp.Zero, StencilOp.Zero, StencilOp.Zero);
                glnvg__checkError(gl, "stroke fill 1");
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
                glnvg__setUniforms(gl, call.uniformOffset, call.image);
                glnvg__checkError(gl, "stroke fill");

                // Draw Strokes
                for (int i = 0; i < paths.Length; i++)
                {
                    GL.DrawArrays(BeginMode.TriangleStrip, paths[i].strokeOffset, paths[i].strokeCount);
                }
            }
        }

        static void glnvg__triangles(GLNVGcontext gl, ref GLNVGcall call)
        {
            glnvg__setUniforms(gl, call.uniformOffset, call.image);
            glnvg__checkError(gl, "triangles fill");

            GL.DrawArrays(BeginMode.Triangles, call.triangleOffset, call.triangleCount);
        }

        static void glnvg__renderCancel(object cxt)
        {
            GLNVGcontext gl = (GLNVGcontext)cxt;
            gl.nverts = 0;
            gl.npaths = 0;
            gl.ncalls = 0;
            gl.nuniforms = 0;
        }

        static BlendingFactorSrc glnvg_convertBlendFuncFactorSrc(BlendFactor factor)
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

        static BlendingFactorDest glnvg_convertBlendFuncFactorDest(BlendFactor factor)
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

        static GLNVGblend glnvg__blendCompositeOperation(CompositeOperationState op)
        {
            GLNVGblend blend;
            blend.srcRGB = glnvg_convertBlendFuncFactorSrc(op.srcRGB);
            blend.dstRGB = glnvg_convertBlendFuncFactorDest(op.dstRGB);
            blend.srcAlpha = glnvg_convertBlendFuncFactorSrc(op.srcAlpha);
            blend.dstAlpha = glnvg_convertBlendFuncFactorDest(op.dstAlpha);
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

        static void glnvg__renderFlush(object cxt)
        {
            var gl = (GLNVGcontext)cxt;

            if (gl.ncalls > 0)
            {
                // Setup require GL state.
                GL.UseProgram(gl.shader.prog);

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
                GL.BindBuffer(BufferTarget.UniformBuffer, gl.fragBuf);
                GL.BufferData(BufferTarget.UniformBuffer, gl.nuniforms * gl.fragSize, gl.uniforms, BufferUsageHint.StreamDraw);
#endif

                // Upload vertex data
#if NANOVG_GL3
                GL.BindVertexArray(gl.vertArr);
#endif
                GL.BindBuffer(BufferTarget.ArrayBuffer, gl.vertBuf);
                GL.BufferData(BufferTarget.ArrayBuffer, gl.nverts, gl.verts, BufferUsageHint.StreamDraw);
                GL.EnableVertexAttribArray(0);
                GL.EnableVertexAttribArray(1);
                GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf<Vertex>(), Marshal.OffsetOf<Vertex>(nameof(Vertex.x)));
                GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf<Vertex>(), Marshal.OffsetOf<Vertex>(nameof(Vertex.u)));

                // Set view and texture just once per frame.
                GL.Uniform1(gl.shader.locTex, 0);
                GL.Uniform2(gl.shader.locViewSize, gl.view.X, gl.view.Y);

#if NANOVG_GL_USE_UNIFORMBUFFER
                GL.BindBuffer(BufferTarget.UniformBuffer, gl.fragBuf);
#endif

                for (int i = 0; i < gl.ncalls; i++)
                {
                    ref GLNVGcall call = ref gl.calls[i];
                    glnvg__blendFuncSeparate(gl, ref call.blendFunc);
                    if (call.type == GLNVGcallType.GLNVG_FILL)
                    {
                        glnvg__fill(gl, ref call);
                    }
                    else if (call.type == GLNVGcallType.GLNVG_CONVEXFILL)
                    {
                        glnvg__convexFill(gl, ref call);
                    }
                    else if (call.type == GLNVGcallType.GLNVG_STROKE)
                    {
                        glnvg__stroke(gl, ref call);
                    }
                    else if (call.type == GLNVGcallType.GLNVG_TRIANGLES)
                    {
                        glnvg__triangles(gl, ref call);
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
                glnvg__bindTexture(gl, 0);
            }

            // Reset calls
            gl.nverts = 0;
            gl.npaths = 0;
            gl.ncalls = 0;
            gl.nuniforms = 0;
        }

        static int glnvg__maxVertCount(Context.Path[] paths, int npaths)
        {
            int i, count = 0;
            for (i = 0; i < npaths; i++)
            {
                count += paths[i].nfill;
                count += paths[i].nstroke;
            }

            return count;
        }

        static ref GLNVGcall glnvg__allocCall(GLNVGcontext gl)
        {
            if (gl.ncalls + 1 > gl.ccalls)
            {
                int ccalls = Math.Max(gl.ncalls + 1, 128) + gl.ccalls / 2; // 1.5x Overallocate
                Array.Resize(ref gl.calls, ccalls);
                gl.ccalls = ccalls;
            }

            ref var ret = ref gl.calls[gl.ncalls++];
            ret = new GLNVGcall();
            return ref ret;
        }

        static int glnvg__allocPaths(GLNVGcontext gl, int n)
        {
            int ret = 0;

            if (gl.npaths + n > gl.cpaths)
            {
                int cpaths = Math.Max(gl.npaths + n, 128) + gl.cpaths / 2; // 1.5x Overallocate
                Array.Resize(ref gl.paths, cpaths);
                gl.cpaths = cpaths;
            }

            ret = gl.npaths;
            gl.npaths += n;
            return ret;
        }

        static int glnvg__allocVerts(GLNVGcontext gl, int n)
        {
            int ret = 0;

            if (gl.nverts + n > gl.cverts)
            {
                int cverts = Math.Max(gl.nverts + n, 4096) + gl.cverts / 2; // 1.5x Overallocate
                Array.Resize(ref gl.verts, cverts);
                gl.cverts = cverts;
            }

            ret = gl.nverts;
            gl.nverts += n;
            return ret;
        }

        static int glnvg__allocFragUniforms(GLNVGcontext gl, int n)
        {
            int ret = 0;
            //int structSize = gl.fragSize;

            if (gl.nuniforms + n > gl.cuniforms)
            {
                int cuniforms = Math.Max(gl.nuniforms + n, 128) + gl.cuniforms / 2; // 1.5x Overallocate
                Array.Resize(ref gl.uniforms, cuniforms);
                gl.cuniforms = cuniforms;
            }

            ret = gl.nuniforms;
            gl.nuniforms += n;
            return ret;
        }

        static ref GLNVGfragUniforms nvg__fragUniformPtr(GLNVGcontext gl, int i)
        {
            return ref gl.uniforms[i];
        }

        static void glnvg__renderFill(
            object cxt,
            ref Paint paint,
            CompositeOperationState compositeOperation,
            ref Context.nvgScissor scissor,
            float fringe,
            Context.Bounds2D bounds,
            Context.Path[] paths,
            int npaths)
        {
            GLNVGcontext gl = (GLNVGcontext)cxt;

            ref GLNVGcall call = ref glnvg__allocCall(gl);
            call.type = GLNVGcallType.GLNVG_FILL;
            call.triangleCount = 4;
            call.pathOffset = glnvg__allocPaths(gl, npaths);
            call.pathCount = npaths;
            call.image = paint.image;
            call.blendFunc = glnvg__blendCompositeOperation(compositeOperation);

            if (npaths == 1 && paths[0].convex)
            {
                call.type = GLNVGcallType.GLNVG_CONVEXFILL;
                call.triangleCount = 0;    // Bounding box fill quad not needed for convex fill
            }

            // Allocate vertices for all the paths.
            int maxverts = glnvg__maxVertCount(paths, npaths) + call.triangleCount;
            int offset = glnvg__allocVerts(gl, maxverts);

            for (int i = 0; i < npaths; i++)
            {
                ref GLNVGpath copy = ref gl.paths[call.pathOffset + i];
                ref Context.Path path = ref paths[i];
                copy = new GLNVGpath();
                if (path.nfill > 0)
                {
                    copy.fillOffset = offset;
                    copy.fillCount = path.nfill;
                    Array.Copy(path.fill.Array, path.fill.Offset, gl.verts, offset, path.nfill);
                    offset += path.nfill;
                }

                if (path.nstroke > 0)
                {
                    copy.strokeOffset = offset;
                    copy.strokeCount = path.nstroke;
                    Array.Copy(path.stroke.Array, path.stroke.Offset, gl.verts, offset, path.nstroke);
                    offset += path.nstroke;
                }
            }

            // Setup uniforms for draw calls
            if (call.type == GLNVGcallType.GLNVG_FILL)
            {
                // Quad
                call.triangleOffset = offset;
                var quad = new Span<Vertex>(gl.verts, call.triangleOffset, 4);
                quad[0] = new Vertex(bounds.MaxX, bounds.MaxY, 0.5f, 1.0f);
                quad[1] = new Vertex(bounds.MaxX, bounds.MinY, 0.5f, 1.0f);
                quad[2] = new Vertex(bounds.MinX, bounds.MaxY, 0.5f, 1.0f);
                quad[3] = new Vertex(bounds.MinX, bounds.MinY, 0.5f, 1.0f);

                call.uniformOffset = glnvg__allocFragUniforms(gl, 2);

                // Simple shader for stencil
                ref var frag = ref nvg__fragUniformPtr(gl, call.uniformOffset);
                frag = new GLNVGfragUniforms();
                frag.strokeThr = -1.0f;
                frag.type = GLNVGshaderType.NSVG_SHADER_SIMPLE;

                // Fill shader
                glnvg__convertPaint(gl, ref nvg__fragUniformPtr(gl, call.uniformOffset), ref paint, ref scissor, fringe, fringe, -1.0f);
            }
            else
            {
                call.uniformOffset = glnvg__allocFragUniforms(gl, 1);

                // Fill shader
                glnvg__convertPaint(gl, ref nvg__fragUniformPtr(gl, call.uniformOffset), ref paint, ref scissor, fringe, fringe, -1.0f);
            }
        }

        static void glnvg__renderStroke(
            object uptr,
            ref Paint paint,
            CompositeOperationState compositeOperation,
            ref Context.nvgScissor scissor,
            float fringe,
            float strokeWidth,
            Context.Path[] paths,
            int npaths)
        {
            GLNVGcontext gl = (GLNVGcontext)uptr;

            ref GLNVGcall call = ref glnvg__allocCall(gl);
            call.type = GLNVGcallType.GLNVG_STROKE;
            call.pathOffset = glnvg__allocPaths(gl, npaths);
            call.pathCount = npaths;
            call.image = paint.image;
            call.blendFunc = glnvg__blendCompositeOperation(compositeOperation);

            // Allocate vertices for all the paths.
            int maxverts = glnvg__maxVertCount(paths, npaths);
            int offset = glnvg__allocVerts(gl, maxverts);

            for (int i = 0; i < npaths; i++)
            {
                ref GLNVGpath copy = ref gl.paths[call.pathOffset + i];
                ref Context.Path path = ref paths[i];
                path = new Context.Path();
                if (path.nstroke > 0)
                {
                    copy.strokeOffset = offset;
                    copy.strokeCount = path.nstroke;
                    Array.Copy(path.stroke.Array, path.stroke.Offset, gl.verts, offset, path.nstroke);
                    offset += path.nstroke;
                }
            }

            if (gl.flags.HasFlag(CreateFlags.NVG_STENCIL_STROKES))
            {
                // Fill shader
                call.uniformOffset = glnvg__allocFragUniforms(gl, 2);
                glnvg__convertPaint(gl, ref nvg__fragUniformPtr(gl, call.uniformOffset), ref paint, ref scissor, strokeWidth, fringe, -1.0f);
                glnvg__convertPaint(gl, ref nvg__fragUniformPtr(gl, call.uniformOffset), ref paint, ref scissor, strokeWidth, fringe, 1.0f - 0.5f / 255.0f);
            }
            else
            {
                // Fill shader
                call.uniformOffset = glnvg__allocFragUniforms(gl, 1);
                glnvg__convertPaint(gl, ref nvg__fragUniformPtr(gl, call.uniformOffset), ref paint, ref scissor, strokeWidth, fringe, -1.0f);
            }
        }

        static void glnvg__renderTriangles(
            object uptr,
            ref Paint paint,
            CompositeOperationState compositeOperation,
            ref Context.nvgScissor scissor,
            Vertex[] verts,
            int nverts)
        {
            GLNVGcontext gl = (GLNVGcontext)uptr;

            ref GLNVGcall call = ref glnvg__allocCall(gl);
            call.type = GLNVGcallType.GLNVG_TRIANGLES;
            call.image = paint.image;
            call.blendFunc = glnvg__blendCompositeOperation(compositeOperation);

            // Allocate vertices for all the paths.
            call.triangleOffset = glnvg__allocVerts(gl, nverts);
            call.triangleCount = nverts;

            Array.Copy(verts, 0, gl.verts, call.triangleOffset, nverts);

            // Fill shader
            call.uniformOffset = glnvg__allocFragUniforms(gl, 1);
            ref var frag = ref nvg__fragUniformPtr(gl, call.uniformOffset);
            glnvg__convertPaint(gl, ref frag, ref paint, ref scissor, 1.0f, 1.0f, -1.0f);
            frag.type = GLNVGshaderType.NSVG_SHADER_IMG;
        }

        static void glnvg__renderDelete(object uptr)
        {
            GLNVGcontext gl = (GLNVGcontext)uptr;
            if (gl == null) return;

            glnvg__deleteShader(ref gl.shader);

#if NANOVG_GL3
#if NANOVG_GL_USE_UNIFORMBUFFER
            if (gl.fragBuf != 0)
                GL.DeleteBuffers(1, ref gl.fragBuf);
#endif
            if (gl.vertArr != 0)
                GL.DeleteVertexArrays(1, ref gl.vertArr);
#endif
            if (gl.vertBuf != 0)
                GL.DeleteBuffers(1, ref gl.vertBuf);

            for (int i = 0; i < gl.ntextures; i++)
            {
                if (gl.textures[i].tex != 0 && !gl.textures[i].flags.HasFlag(ImageFlagsGL.NVG_IMAGE_NODELETE))
                    GL.DeleteTextures(1, ref gl.textures[i].tex);
            }

            // TODO> Probably not needed - all ref types
            ////free(gl.textures);
            ////free(gl.paths);
            ////free(gl.verts);
            ////free(gl.uniforms);
            ////free(gl.calls);
            ////free(gl);
        }

        // Creates NanoVG contexts for different OpenGL (ES) versions.
        // Flags should be combination of the create flags above.

#if NANOVG_GL2
        public static Context nvgCreateGL2(CreateFlags flags)
#elif NANOVG_GL3
        public static Context nvgCreateGL3(CreateFlags flags)
#elif NANOVG_GLES2
        public static Context nvgCreateGLES2(CreateFlags flags)
#elif NANOVG_GLES3
        public static Context nvgCreateGLES3(CreateFlags flags)
#endif
        {
            GLNVGcontext gl = new GLNVGcontext();
            gl.flags = flags;

            var @params = new Context.Params();
            @params.renderCreate = glnvg__renderCreate;
            @params.renderCreateTexture = glnvg__renderCreateTexture;
            @params.renderDeleteTexture = glnvg__renderDeleteTexture;
            @params.renderUpdateTexture = glnvg__renderUpdateTexture;
            @params.renderGetTextureSize = glnvg__renderGetTextureSize;
            @params.renderViewport = glnvg__renderViewport;
            @params.renderCancel = glnvg__renderCancel;
            @params.renderFlush = glnvg__renderFlush;
            @params.renderFill = glnvg__renderFill;
            @params.renderStroke = glnvg__renderStroke;
            @params.renderTriangles = glnvg__renderTriangles;
            @params.renderDelete = glnvg__renderDelete;
            @params.userPtr = gl;
            @params.edgeAntiAlias = flags.HasFlag(CreateFlags.NVG_ANTIALIAS) ? 1 : 0;

            return Context.CreateInternal(@params);
        }

#if NANOVG_GL2
       public static void nvgDeleteGL2(Context ctx)
#elif NANOVG_GL3
        public static void nvgDeleteGL3(Context ctx)
#elif NANOVG_GLES2
        public static void nvgDeleteGLES2(Context ctx)
#elif NANOVG_GLES3
        public static void nvgDeleteGLES3(Context ctx)
#endif
        {
            Context.DeleteInternal(ctx);
        }

#if NANOVG_GL2
        static int nvglCreateImageFromHandleGL2(Context ctx, uint textureId, int w, int h, NVGimageFlagsGL imageFlags)
#elif NANOVG_GL3
        static int nvglCreateImageFromHandleGL3(Context ctx, uint textureId, int w, int h, ImageFlags imageFlags)
#elif NANOVG_GLES2
        int nvglCreateImageFromHandleGLES2(Context ctx, uint textureId, int w, int h, NVGimageFlagsGL imageFlags)
#elif NANOVG_GLES3
        int nvglCreateImageFromHandleGLES3(Context ctx, uint textureId, int w, int h, NVGimageFlagsGL imageFlags)
#endif
        {
            GLNVGcontext gl = (GLNVGcontext)ctx.InternalParams().userPtr;
            ref GLNVGtexture tex = ref glnvg__allocTexture(gl);

            tex.type = Context.Texture.NVG_TEXTURE_RGBA;
            tex.tex = textureId;
            tex.flags = imageFlags;
            tex.width = w;
            tex.height = h;

            return tex.id;
        }

#if NANOVG_GL2
        static uint nvglImageHandleGL2(Context ctx, int image)
#elif NANOVG_GL3
        static uint nvglImageHandleGL3(Context ctx, int image)
#elif NANOVG_GLES2
        uint nvglImageHandleGLES2(Context ctx, int image)
#elif NANOVG_GLES3
        uint nvglImageHandleGLES3(Context ctx, int image)
#endif
        {
            GLNVGcontext gl = (GLNVGcontext)ctx.InternalParams().userPtr;
            glnvg__findTexture(gl, image, out var tex);
            return tex.tex;
        }
    }
}
