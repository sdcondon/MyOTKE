﻿using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MyOTKE.Core;

/// <summary>
/// Container for information about an OpenGL vertex attribute.
/// </summary>
public readonly struct GlVertexAttributeInfo
{
    /// <summary>
    /// A mapping of .NET types to appropriate primitive OpenGL attribute info for them.
    /// </summary>
    private static readonly Dictionary<Type, (VertexAttribPointerType type, int count)> KnownTypes = new()
    {
        [typeof(Vector4)] = (VertexAttribPointerType.Float, 4),
        [typeof(Vector3)] = (VertexAttribPointerType.Float, 3),
        [typeof(Vector2)] = (VertexAttribPointerType.Float, 2),
        [typeof(float)] = (VertexAttribPointerType.Float, 1),
        [typeof(uint)] = (VertexAttribPointerType.UnsignedInt, 1),
        [typeof(int)] = (VertexAttribPointerType.Int, 1),
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="GlVertexAttributeInfo"/> struct.
    /// </summary>
    /// <param name="type">The OpenGL type for the vertex attribute.</param>
    /// <param name="multiple">The multiple (of the OpenGL type) for the attribute.</param>
    /// <param name="offset">The offset from the start of the buffer to the attribute for the first vertex in the buffer.</param>
    /// <param name="stride">The offset from the attribute for one vertex in the buffer to the next.</param>
    internal GlVertexAttributeInfo(VertexAttribPointerType type, int multiple, int offset, int stride)
    {
        this.Type = type;
        this.Multiple = multiple;
        this.Offset = new IntPtr(offset);
        this.Stride = stride;
    }

    /// <summary>
    /// Gets the OpenGL type for the vertex attribute.
    /// </summary>
    public VertexAttribPointerType Type { get; }

    /// <summary>
    /// Gets the multiple (of the OpenGL type) for the attribute.
    /// </summary>
    public int Multiple { get; }

    /// <summary>
    /// Gets the offset from the start of the buffer to the attribute for the first vertex in the buffer.
    /// </summary>
    public IntPtr Offset { get; }

    /// <summary>
    /// Gets the offset from the attribute for one vertex in the buffer to the next.
    /// </summary>
    public int Stride { get; }

    /// <summary>
    /// Returns attribute info for a given (blittable) type.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <returns>An array of attribute info.</returns>
    internal static GlVertexAttributeInfo[] ForType<T>()
    {
        return ForType(typeof(T));
    }

    /// <summary>
    /// Returns attribute info for a given (blittable) type.
    /// </summary>
    /// <param name="t">The type.</param>
    /// <returns>An array of attribute info.</returns>
    internal static GlVertexAttributeInfo[] ForType(Type t)
    {
        var attributes = new List<GlVertexAttributeInfo>();
        ForType(t, attributes, 0, Marshal.SizeOf(t));
        return [.. attributes];
    }

    private static void ForType(Type t, List<GlVertexAttributeInfo> attributes, int offset, int stride)
    {
        if (KnownTypes.TryGetValue(t, out var glInfo))
        {
            attributes.Add(new GlVertexAttributeInfo(glInfo.type, glInfo.count, offset, stride));
        }
        else if (!t.IsValueType || t.IsAutoLayout)
        {
            throw new ArgumentException($"Unsupported type {t} - passed type must be blittable", t.FullName);
        }
        else
        {
            foreach (var field in t.GetFields())
            {
                ForType(field.FieldType, attributes, offset + (int)Marshal.OffsetOf(t, field.Name), stride);
            }
        }
    }
}