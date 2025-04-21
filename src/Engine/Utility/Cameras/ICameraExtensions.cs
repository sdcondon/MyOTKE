using OpenTK.Mathematics;

namespace MyOTKE.Engine;

/// <summary>
/// Interface for types that provide view and projection matrices, and can change over time (e.g. in reponse to user input).
/// </summary>
public static class ICameraExtensions
{
    /// <summary>
    /// Gets the current position of a camera in world space.
    /// </summary>
    /// <param name="camera">The camera to get the position of.</param>
    /// <returns>The current position of the camera.</returns>
    /// <remarks>
    /// The view matrix of a camera transforms world space to camera space (i.e. with the camera at the origin).
    /// So to get the position of a camera in world space, we just need to transform the zero vector by the inverse of the view matrix.
    /// </remarks>
    public static Vector3 GetPosition(this ICamera camera) => (new Vector4(Vector3.Zero, 1f) * camera.View.Inverted()).Xyz;
}
