using OpenTK.Mathematics;

namespace MyOTKE.Components.Primitives;

/// <summary>
/// Container struct for the attributes of a vertex of a primitive.
/// </summary>
/// <param name="position">The position of the vertex.</param>
/// <param name="color">The color of the vertex.</param>
/// <param name="normal">The normal vector of the vertex.</param>
public readonly struct PrimitiveVertex(Vector3 position, Vector4 color, Vector3 normal)
{
    /// <summary>
    /// The position of the vertex.
    /// </summary>
    public readonly Vector3 Position = position;

    /// <summary>
    /// The color of the vertex.
    /// </summary>
    public readonly Vector4 Color = color;

    /// <summary>
    /// The normal vector of the vertex.
    /// </summary>
    public readonly Vector3 Normal = normal;
}
