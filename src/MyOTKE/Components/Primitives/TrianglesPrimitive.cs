using OpenTK.Mathematics;
using System;

namespace MyOTKE.Components.Primitives;

/// <summary>
/// Container for triangles primitive vertex data.
/// </summary>
/// <remarks>
/// TODO: Perhaps should be an immutable struct instead to discourage heap allocations..
/// </remarks>
public sealed class TrianglesPrimitive : Primitive
{
    /// <summary>
    /// Creates a cuboid primitive.
    /// </summary>
    /// <param name="size">The dimensions of the cuboid.</param>
    /// <param name="worldTransform">The world transform of the cuboid.</param>
    /// <param name="color">The color of the cuboid.</param>
    /// <returns>The created primitive.</returns>
    public static TrianglesPrimitive Cuboid(Vector3 size, Matrix4 worldTransform, Color color)
    {
        TrianglesPrimitive p = new();
        p.SetCuboid(size, worldTransform, color);
        return p;
    }

    /// <summary>
    /// Creates a quad primitive.
    /// </summary>
    /// <param name="size">The dimensions of the quad.</param>
    /// <param name="worldTransform">The world transform of the quad.</param>
    /// <param name="color">The color of the quad.</param>
    /// <returns>The created primitive.</returns>
    public static TrianglesPrimitive Quad(Vector2 size, Matrix4 worldTransform, Color color)
    {
        TrianglesPrimitive p = new();
        p.SetQuad(size, worldTransform, color);
        return p;
    }

    /// <summary>
    /// Sets primitive as a cuboid.
    /// </summary>
    /// <param name="size">The dimensions of the cuboid.</param>
    /// <param name="worldTransform">The world transform of the cuboid.</param>
    /// <param name="color">The color of the cuboid.</param>
    public void SetCuboid(Vector3 size, Matrix4 worldTransform, Color color)
    {
        ClearVertices();

        var xy = new Vector2(size.X, size.Y);
        var xz = new Vector2(size.X, size.Z);
        var zy = new Vector2(size.Z, size.Y);
        var xOffset = Matrix4.CreateTranslation(0, 0, size.X / 2);
        var yOffset = Matrix4.CreateTranslation(0, 0, size.Y / 2);
        var zOffset = Matrix4.CreateTranslation(0, 0, size.Z / 2);

        AddQuad(xy, zOffset * worldTransform, color);
        AddQuad(xy, zOffset * Matrix4.CreateRotationX((float)Math.PI) * worldTransform, color);
        AddQuad(xz, yOffset * Matrix4.CreateRotationX((float)-Math.PI / 2) * worldTransform, color);
        AddQuad(xz, yOffset * Matrix4.CreateRotationX((float)Math.PI / 2) * worldTransform, color);
        AddQuad(zy, xOffset * Matrix4.CreateRotationY((float)-Math.PI / 2) * worldTransform, color);
        AddQuad(zy, xOffset * Matrix4.CreateRotationY((float)Math.PI / 2) * worldTransform, color);

        SetBufferItem();
    }

    /// <summary>
    /// Sets primitive as a quad.
    /// </summary>
    /// <param name="size">The dimensions of the quad.</param>
    /// <param name="worldTransform">The world transform of the quad.</param>
    /// <param name="color">The color of the quad.</param>
    public void SetQuad(Vector2 size, Matrix4 worldTransform, Color color)
    {
        ClearVertices();

        AddQuad(size, worldTransform, color);

        SetBufferItem();
    }

    private void AddQuad(Vector2 size, Matrix4 worldTransform, Color color)
    {
        var normal = Vector3.TransformNormal(Vector3.UnitZ, worldTransform);

        AddVertex(Vector3.TransformPosition(new Vector3(-size.X / 2, -size.Y / 2, 0), worldTransform), color, normal);
        AddVertex(Vector3.TransformPosition(new Vector3(+size.X / 2, -size.Y / 2, 0), worldTransform), color, normal);
        AddVertex(Vector3.TransformPosition(new Vector3(-size.X / 2, +size.Y / 2, 0), worldTransform), color, normal);
        AddVertex(Vector3.TransformPosition(new Vector3(+size.X / 2, +size.Y / 2, 0), worldTransform), color, normal);
        AddVertex(Vector3.TransformPosition(new Vector3(-size.X / 2, +size.Y / 2, 0), worldTransform), color, normal);
        AddVertex(Vector3.TransformPosition(new Vector3(+size.X / 2, -size.Y / 2, 0), worldTransform), color, normal);
    }
}
