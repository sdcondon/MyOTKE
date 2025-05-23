﻿using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SharpFont;
using System;
using System.Collections.Generic;

namespace MyOTKE.Components.Reactive.Gui;

/// <summary>
/// See https://www.freetype.org/freetype2/docs/tutorial/step1.html and
/// https://learnopengl.com/In-Practice/Text-Rendering.
/// </summary>
public sealed class Font : IDisposable
{
    private static readonly Library SharpFont = new();

    private readonly Dictionary<char, uint> glyphIndicesByChar = [];
    private readonly object glyphsLock = new();
    private readonly Face face;

    private GlyphInfo[] glyphs;

    /// <summary>
    /// Initializes a new instance of the <see cref="Font"/> class.
    /// </summary>
    /// <param name="filePath">The path to the font file.</param>
    /// <param name="pixelSize"></param>
    public Font(string filePath, uint pixelSize = 16)
    {
        this.face = new Face(SharpFont, filePath);
        this.face.SetPixelSizes(0, pixelSize);
        this.face.SelectCharmap(Encoding.Unicode);
        ////this.face.SetCharSize(0, 16 * 64, 300, 300);

        // Establish maximum glyph dimensions;
        for (uint i = 0; i < face.GlyphCount; i++)
        {
            face.LoadGlyph(i, LoadFlags.Default, LoadTarget.Normal);
            Max = (Math.Max(Max.Width, face.Glyph.Metrics.Width.ToInt32()), Math.Max(Max.Height, face.Glyph.Metrics.Height.ToInt32()));
        }
    }

    /// <summary>
    /// Gets the Id of the (2D array) texture that holds the glyphs for this font.
    /// </summary>
    public int TextureId { get; private set; }

    public short LineHeight => face.Height;

    public (int Width, int Height) Max { get; }

    public GlyphInfo this[char c]
    {
        get
        {
            // Lazily load glyphs on first access because its easy.
            // Would be better to load as soon as device context is available.
            if (glyphs == null)
            {
                lock (glyphsLock)
                {
                    if (glyphs == null)
                    {
                        LoadGlyphs();
                    }
                }
            }

            // Apparently some fonts have char entries for a LOT of characters,
            // so do this lazily rather than looping at ctor time
            if (!glyphIndicesByChar.TryGetValue(c, out uint value))
            {
                glyphIndicesByChar[c] = value = face.GetCharIndex(c);
            }

            return glyphs[value];
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        face?.Dispose();
    }

    private void LoadGlyphs()
    {
        this.glyphs = new GlyphInfo[face.GlyphCount];

        // Create texture array to store glyphs
        // TODO: all direct Gl usage should be in core - add a GlTexture(2dArray?) class.
        GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
        this.TextureId = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2DArray, this.TextureId);
        GL.TexStorage3D(
            target: TextureTarget3d.Texture2DArray,
            levels: 1,
            internalformat: SizedInternalFormat.R8,
            width: Max.Width,
            height: Max.Height,
            depth: face.GlyphCount);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        // Eagerly load all glyphs
        for (uint i = 0; i < face.GlyphCount; i++)
        {
            LoadGlyph(i);
        }
    }

    private void LoadGlyph(uint glyphIndex)
    {
        face.LoadGlyph(glyphIndex, LoadFlags.Default, LoadTarget.Normal);
        face.Glyph.RenderGlyph(RenderMode.Normal);

        if (face.Glyph.Bitmap.Buffer != IntPtr.Zero)
        {
            GL.TexSubImage3D(
                target: TextureTarget.Texture2DArray,
                level: 0,
                xoffset: 0,
                yoffset: 0,
                zoffset: (int)glyphIndex,
                width: face.Glyph.Bitmap.Width,
                height: face.Glyph.Bitmap.Rows,
                depth: 1,
                format: PixelFormat.Red,
                type: PixelType.UnsignedByte,
                pixels: face.Glyph.Bitmap.BufferData);
        }

        this.glyphs[glyphIndex] = new GlyphInfo(
            glyphIndex,
            new Vector2(face.Glyph.Bitmap.Width, face.Glyph.Bitmap.Rows),
            new Vector2(face.Glyph.BitmapLeft, face.Glyph.BitmapTop),
            (uint)(double)face.Glyph.Advance.X);
    }

    /// <summary>
    /// Container for information about a particular glyph.
    /// </summary>
    public readonly struct GlyphInfo(uint zOffset, Vector2 size, Vector2 bearing, uint advance)
    {
        public uint ZOffset { get; } = zOffset;

        public Vector2 Size { get; } = size;

        public Vector2 Bearing { get; } = bearing;

        public uint Advance { get; } = advance;
    }
}
