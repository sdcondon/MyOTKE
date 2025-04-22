using OpenTK.Mathematics;
using System;
using Plane = System.Numerics.Plane;

namespace MyOTKE.Cameras;

/// <summary>
/// Represents a semi-infinite 3D ray, that has a start point and a direction.
/// </summary>
public struct Ray
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Ray"/> struct that originates from a given camera position,
    /// and points in the direction as indicated by the current mouse cursor position in a view.
    /// </summary>
    /// <param name="camera">The camera to project the ray from.</param>
    /// <param name="view">The view from which to retrieve the mouse position to determine the ray's direction.</param>
    /// <remarks>
    /// Explanation can be found at http://antongerdelan.net/opengl/raycasting.html.
    /// </remarks>
    public Ray(ICamera camera, MyOTKEWindow view)
    {
        float x = (2.0f * view.MouseCenterOffset.X) / view.ClientSize.X;
        float y = -(2.0f * view.MouseCenterOffset.Y) / view.ClientSize.Y;
        var ray_clip = new Vector4(x, y, -1.0f, 1.0f);

        var ray_eye = ray_clip * camera.Projection.Inverted();
        ray_eye = new Vector4(ray_eye.X, ray_eye.Y, -1.0f, 0f);

        var ray_world = (ray_eye * camera.View.Inverted()).Xyz;

        Direction = ray_world.Normalized();
        Origin = (new Vector4(Vector3.Zero, 1f) * camera.View.Inverted()).Xyz;
    }

    /// <summary>
    /// Gets the position of the origin of the ray.
    /// </summary>
    public Vector3 Origin { get; private set; }

    /// <summary>
    /// Gets the direction vector of the ray.
    /// </summary>
    public Vector3 Direction { get; private set; }

    /// <summary>
    /// Gets the positions of any intersections of a ray with a sphere.
    /// </summary>
    /// <param name="ray">The ray.</param>
    /// <param name="sphereCentre">The position of the center of the sphere.</param>
    /// <param name="sphereRadius">The radius of the sphere.</param>
    /// <returns>The positions of any intersections.</returns>
    public static Vector3[] GetIntersections(Ray ray, Vector3 sphereCentre, float sphereRadius)
    {
        var offset = ray.Origin - sphereCentre;
        var b = Vector3.Dot(ray.Direction, offset);
        var c = Vector3.Dot(offset, offset) - sphereRadius * sphereRadius;
        var discriminant = b * b - c;

        if (discriminant < 0)
        {
            return [];
        }
        else if (discriminant > 0)
        {
            var t = new[] { -b + (float)Math.Sqrt(discriminant), -b - (float)Math.Sqrt(discriminant) };
            Array.Sort(t);
            return [ ray.Origin + t[0] * ray.Direction, ray.Origin + t[1] * ray.Direction ];
        }
        else
        {
            return [ ray.Origin + -b * ray.Direction ];
        }
    }

    /// <summary>
    /// Gets the position of the intersection of a ray with a plane, or null if no such intersection exists.
    /// </summary>
    /// <param name="ray">The ray.</param>
    /// <param name="plane">The plane.</param>
    /// <returns>The position of the intersection of the ray with the plane, or null if no such intersection exists.</returns>
    public static Vector3? GetIntersection(Ray ray, Plane plane)
    {
        var numerator = Plane.DotNormal(plane, plane.Normal * plane.D - new System.Numerics.Vector3(ray.Origin.X, ray.Origin.Y, ray.Origin.Z));
        var denominator = Plane.DotNormal(plane, new System.Numerics.Vector3(ray.Direction.X, ray.Direction.Y, ray.Direction.Z));

        if (denominator != 0)
        {
            var distanceAlongRay = numerator / denominator;
            return ray.Origin + ray.Direction * distanceAlongRay;
        }

        return null;
    }
}
