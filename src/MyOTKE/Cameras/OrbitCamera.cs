using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;

namespace MyOTKE.Cameras;

/// <summary>
/// Implementation of <see cref="ICamera"/> that rotates around the origin.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="OrbitCamera"/> class.
/// </remarks>
/// <param name="view">The view from which to retrieve input and aspect ratio.</param>
/// <param name="rotationSpeedBase">The base (i.e. at default zoom distance) rotation speed of the camera, in radians per per update.</param>
/// <param name="rollSpeed">The roll speed of the camera, in radians per update.</param>
/// <param name="fieldOfViewRadians">The camera's field of view, in radians.</param>
/// <param name="nearPlaneDistance">The distance of the near plane from the camera.</param>
/// <param name="farPlaneDistance">The ditance of the far plane from the camera.</param>
public class OrbitCamera(
    MyOTKEWindow view,
    float rotationSpeedBase,
    float rollSpeed,
    float fieldOfViewRadians,
    float nearPlaneDistance,
    float farPlaneDistance) : ICamera
{
    private readonly MyOTKEWindow view = view;

    private Vector3 forward = new(0f, 0f, 1f);
    private Vector3 up = new(0f, 1f, 0f);
    private int zoomLevel = 0;

    /// <summary>
    /// Gets or sets the base (i.e. at default zoom distance) rotation speed of the camera in radians per update.
    /// </summary>
    public float RotationSpeedBase { get; set; } = rotationSpeedBase;

    /// <summary>
    /// Gets or sets the roll speed of the camera, in radians per update.
    /// </summary>
    public float RollSpeed { get; set; } = rollSpeed;

    /// <summary>
    /// Gets or sets the camera's field of view, in radians.
    /// </summary>
    public float FieldOfViewRadians { get; set; } = fieldOfViewRadians;

    /// <summary>
    /// Gets or sets the distance of the near plane from the camera.
    /// </summary>
    public float NearPlaneDistance { get; set; } = nearPlaneDistance;

    /// <summary>
    /// Gets or sets the distance of the far plane from the camera.
    /// </summary>
    public float FarPlaneDistance { get; set; } = farPlaneDistance;

    /// <summary>
    /// Gets the current distance between the camera and the origin.
    /// </summary>
    public float Distance => (float)(ZoomMinDistance + ZoomDefaultDistance * Math.Pow(ZoomBase, zoomLevel));

    /// <summary>
    /// Gets the current rotation speed of the camera, in radians per update.
    /// </summary>
    public float RotationSpeed => RotationSpeedBase * (Distance - ZoomMinDistance) / ZoomDefaultDistance;

    /// <summary>
    /// Gets the current position of the camera.
    /// </summary>
    public Vector3 Position => -forward * Distance;

    /// <inheritdoc />
    public Matrix4 View { get; private set; }

    /// <inheritdoc />
    public Matrix4 Projection { get; private set; }

    private float ZoomDefaultDistance { get; set; } = 1.5f;

    private float ZoomBase { get; set; } = 0.999f;

    private float ZoomMinDistance => 1f + NearPlaneDistance;

    /// <inheritdoc />
    public void Update(TimeSpan elapsed)
    {
        //// Pan up - rotate forward and up around their cross product
        if (view.IsKeyDown(Keys.W))
        {
            var t = Quaternion.FromAxisAngle(Vector3.Cross(forward, up), -RotationSpeed);
            forward = Vector3.Transform(forward, t);
            up = Vector3.Transform(up, t);
        }
        //// Pan down - rotate forward and up around their cross product
        if (view.IsKeyDown(Keys.S))
        {
            var t = Quaternion.FromAxisAngle(Vector3.Cross(forward, up), RotationSpeed);
            forward = Vector3.Normalize(Vector3.Transform(forward, t));
            up = Vector3.Normalize(Vector3.Transform(up, t));
        }
        //// Pan right - rotate forward around up
        if (view.IsKeyDown(Keys.D))
        {
            forward = Vector3.Normalize(Vector3.Transform(forward, Quaternion.FromAxisAngle(up, RotationSpeed)));
        }
        //// Pan left - rotate forward around up
        if (view.IsKeyDown(Keys.A))
        {
            forward = Vector3.Normalize(Vector3.Transform(forward, Quaternion.FromAxisAngle(up, -RotationSpeed)));
        }
        //// Roll right - rotate up around forward
        if (view.IsKeyDown(Keys.Q))
        {
            up = Vector3.Normalize(Vector3.Transform(up, Quaternion.FromAxisAngle(forward, -RollSpeed)));
        }
        //// Roll left - rotate up around forward
        if (view.IsKeyDown(Keys.E))
        {
            up = Vector3.Normalize(Vector3.Transform(up, Quaternion.FromAxisAngle(forward, RollSpeed)));
        }
        //// Zoom
        zoomLevel += (int)view.MouseState.ScrollDelta.Y;

        // Projection matrix
        Projection = Matrix4.CreatePerspectiveFieldOfView(
            FieldOfViewRadians,
            view.AspectRatio,
            NearPlaneDistance,
            FarPlaneDistance);

        // Camera matrix
        View = Matrix4.LookAt(Position, Vector3.Zero, up);
    }
}
