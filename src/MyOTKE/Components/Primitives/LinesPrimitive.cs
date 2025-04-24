using MyOTKE.BufferManagement;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace MyOTKE.Components.Reactive.Primitives;

/// <summary>
/// Container for lines primitive vertex data.
/// </summary>
/// <remarks>
/// TODO: Perhaps should be an immutable struct instead to discourage heap allocations..
/// </remarks>
public sealed class LinesPrimitive
{
    private readonly List<PrimitiveVertex> vertices = [];
    private IListBufferItem<PrimitiveVertex>? bufferItem;

    /// <summary>
    /// Gets the list of vertices that comprise the primitive.
    /// </summary>
    public IReadOnlyList<PrimitiveVertex> Vertices => vertices;

    /// <summary>
    /// Creates a line primitive of constant color.
    /// </summary>
    /// <param name="from">The position of one end of the line.</param>
    /// <param name="to">The position of the other end of the line.</param>
    /// <param name="color">The color of the line.</param>
    /// <returns>The created primitive.</returns>
    public static LinesPrimitive SingleLine(Vector3 from, Vector3 to, Color color)
    {
        return SingleLine(from, to, color, color);
    }

    /// <summary>
    /// Creates a line primitive of graduated color.
    /// </summary>
    /// <param name="from">The position of one end of the line.</param>
    /// <param name="to">The position of the other end of the line.</param>
    /// <param name="colorFrom">The color of one end of the line.</param>
    /// <param name="colorTo">The color of the other end of the line.</param>
    /// <returns>The created primitive.</returns>
    public static LinesPrimitive SingleLine(Vector3 from, Vector3 to, Color colorFrom, Color colorTo)
    {
        LinesPrimitive p = new();
        p.SetSingleLine(from, to, colorFrom, colorTo); 
        return p;
    }

    /// <summary>
    /// Creates a line circle primitive.
    /// </summary>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="worldTransform">The world transform of the circle.</param>
    /// <param name="color">The color of the circle.</param>
    /// <returns>The created primitive.</returns>
    public static LinesPrimitive Circle(float radius, Matrix4 worldTransform, Color color)
    {
        return Ellipse(radius, radius, worldTransform, color);
    }

    /// <summary>
    /// Creates a line ellipse primitive.
    /// </summary>
    /// <param name="radiusX">The X-axis radius of the ellipse.</param>
    /// <param name="radiusY">The Y-axis radius of the ellipse.</param>
    /// <param name="worldTransform">The world transform of the ellipse.</param>
    /// <param name="color">The color of the ellipse.</param>
    /// <returns>The created primitive.</returns>
    public static LinesPrimitive Ellipse(float radiusX, float radiusY, Matrix4 worldTransform, Color color)
    {
        LinesPrimitive p = new();
        p.SetEllipse(radiusX, radiusY, worldTransform, color);
        return p;
    }

    /// <summary>
    /// Creates a line square primitive.
    /// </summary>
    /// <param name="sideLength">The side length the square.</param>
    /// <param name="worldTransform">The world transform of the square.</param>
    /// <param name="color">The color of the square.</param>
    /// <returns>The created primitive.</returns>
    public static LinesPrimitive Square(float sideLength, Matrix4 worldTransform, Color color)
    {
        LinesPrimitive p = new();
        p.SetSquare(sideLength, worldTransform, color);
        return p;
    }

    /// <summary>
    /// Creates a line polygon primitive.
    /// </summary>
    /// <param name="positions">The positions of the vertices of the polygon.</param>
    /// <param name="worldTransform">The world transform of the polygon.</param>
    /// <param name="color">The color of the polygon.</param>
    /// <returns>The created primitive.</returns>
    public static LinesPrimitive Polygon(Vector2[] positions, Matrix4 worldTransform, Color color)
    {
        LinesPrimitive p = new();
        p.SetPolygon(positions, worldTransform, color);
        return p;
    }

    /// <summary>
    /// Sets primitive as a line of constant color.
    /// </summary>
    /// <param name="from">The position of one end of the line.</param>
    /// <param name="to">The position of the other end of the line.</param>
    /// <param name="color">The color of the line.</param>
    public void SetSingleLine(Vector3 from, Vector3 to, Color color)
    {
        SetSingleLine(from, to, color, color);
    }

    /// <summary>
    /// Sets primitive as a line of graduated color.
    /// </summary>
    /// <param name="from">The position of one end of the line.</param>
    /// <param name="to">The position of the other end of the line.</param>
    /// <param name="colorFrom">The color of one end of the line.</param>
    /// <param name="colorTo">The color of the other end of the line.</param>
    public void SetSingleLine(Vector3 from, Vector3 to, Color colorFrom, Color colorTo)
    {
        vertices.Clear();

        AddVertex(from, colorFrom, Vector3.Zero);
        AddVertex(to, colorTo, Vector3.Zero);

        SetBufferItem();
    }

    /// <summary>
    /// Sets primitive as a line circle.
    /// </summary>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="worldTransform">The world transform of the circle.</param>
    /// <param name="color">The color of the circle.</param>
    public void SetCircle(float radius, Matrix4 worldTransform, Color color)
    {
        SetEllipse(radius, radius, worldTransform, color);
    }

    /// <summary>
    /// Sets primitive as a line ellipse.
    /// </summary>
    /// <param name="radiusX">The X-axis radius of the ellipse.</param>
    /// <param name="radiusY">The Y-axis radius of the ellipse.</param>
    /// <param name="worldTransform">The world transform of the ellipse.</param>
    /// <param name="color">The color of the ellipse.</param>
    public void SetEllipse(float radiusX, float radiusY, Matrix4 worldTransform, Color color)
    {
        vertices.Clear();

        var segments = 16;

        Vector3 GetPos(int i)
        {
            var rads = i % segments * 2 * Math.PI / segments;
            return new Vector3((float)Math.Sin(rads) * radiusX, (float)Math.Cos(rads) * radiusY, 0);
        }

        for (var i = 0; i < segments; i++)
        {
            AddVertex(Vector3.TransformPosition(GetPos(i), worldTransform), color, Vector3.Zero);
            AddVertex(Vector3.TransformPosition(GetPos(i + 1), worldTransform), color, Vector3.Zero);
        }

        SetBufferItem();
    }

    /// <summary>
    /// Sets primitive as line square.
    /// </summary>
    /// <param name="sideLength">The side length the square.</param>
    /// <param name="worldTransform">The world transform of the square.</param>
    /// <param name="color">The color of the square.</param>
    public void SetSquare(float sideLength, Matrix4 worldTransform, Color color)
    {
        vertices.Clear();

        AddVertex(Vector3.TransformPosition(new Vector3(-sideLength / 2, -sideLength / 2, 0), worldTransform), color, Vector3.Zero);
        AddVertex(Vector3.TransformPosition(new Vector3(-sideLength / 2, +sideLength / 2, 0), worldTransform), color, Vector3.Zero);
        AddVertex(Vector3.TransformPosition(new Vector3(+sideLength / 2, -sideLength / 2, 0), worldTransform), color, Vector3.Zero);
        AddVertex(Vector3.TransformPosition(new Vector3(+sideLength / 2, +sideLength / 2, 0), worldTransform), color, Vector3.Zero);
        AddVertex(Vector3.TransformPosition(new Vector3(-sideLength / 2, -sideLength / 2, 0), worldTransform), color, Vector3.Zero);
        AddVertex(Vector3.TransformPosition(new Vector3(+sideLength / 2, -sideLength / 2, 0), worldTransform), color, Vector3.Zero);
        AddVertex(Vector3.TransformPosition(new Vector3(-sideLength / 2, +sideLength / 2, 0), worldTransform), color, Vector3.Zero);
        AddVertex(Vector3.TransformPosition(new Vector3(+sideLength / 2, +sideLength / 2, 0), worldTransform), color, Vector3.Zero);

        SetBufferItem();
    }

    /// <summary>
    /// Sets primitive as a line polygon.
    /// </summary>
    /// <param name="positions">The positions of the vertices of the polygon.</param>
    /// <param name="worldTransform">The world transform of the polygon.</param>
    /// <param name="color">The color of the polygon.</param>
    public void SetPolygon(Vector2[] positions, Matrix4 worldTransform, Color color)
    {
        vertices.Clear();

        for (int i = 0; i < positions.Length; i++)
        {
            AddVertex(Vector3.TransformPosition(new Vector3(positions[i]) { Z = 0 }, worldTransform), color, Vector3.Zero);
            AddVertex(Vector3.TransformPosition(new Vector3(positions[(i + 1) % positions.Length]) { Z = 0 }, worldTransform), color, Vector3.Zero);
        }

        SetBufferItem();
    }

    internal void AddToBuffer(ListBuffer<PrimitiveVertex> buffer)
    {
        bufferItem = buffer.Add();
        SetBufferItem();
    }

    internal void RemoveFromBuffer()
    {
        bufferItem.Dispose();
        bufferItem = null;
    }

    private void AddVertex(Vector3 position, Color color, Vector3 normal)
    {
        vertices.Add(new PrimitiveVertex(position, color, normal));
    }

    private void SetBufferItem()
    {
        if (bufferItem != null)
        {
            bufferItem.Set(vertices);
        }
    }
}
