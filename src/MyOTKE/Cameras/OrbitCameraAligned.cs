﻿using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;

namespace MyOTKE.Cameras;

/// <summary>
/// Implementation of <see cref="ICamera"/> that rotates around the origin.
/// </summary>
/// <param name="view">The view from which to retrieve input and aspect ratio.</param>
/// <param name="rotationSpeedBase">The base (i.e. at default zoom distance) rotation speed of the camera, in radians per per update.</param>
/// <param name="fieldOfViewRadians">The camera's field of view, in radians.</param>
/// <param name="nearPlaneDistance">The distance of the near plane from the camera.</param>
/// <param name="farPlaneDistance">The ditance of the far plane from the camera.</param>
public class OrbitCameraAligned(
    MyOTKEWindow view,
    float rotationSpeedBase,
    float fieldOfViewRadians,
    float nearPlaneDistance,
    float farPlaneDistance) : ICamera
{
    private readonly MyOTKEWindow view = view;

    private float longitude = 0f;
    private float latitude = 0f;

    private Vector3 forward = Vector3.UnitZ;
    private Vector3 up = Vector3.UnitY;
    private int zoomLevel = 0;

    /// <summary>
    /// Gets or sets the base (i.e. at default zoom distance) rotation speed of the camera in radians per second.
    /// </summary>
    public float RotationSpeedBase { get; set; } = rotationSpeedBase;

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
    /// Gets the current rotation speed of the camera, in radians per second.
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
            latitude += RotationSpeed * (float)elapsed.TotalSeconds;
            latitude = (float)Math.Min(latitude, Math.PI / 2f);
        }
        //// Pan down - rotate forward and up around their cross product
        if (view.IsKeyDown(Keys.S))
        {
            latitude -= RotationSpeed * (float)elapsed.TotalSeconds;
            latitude = (float)Math.Max(latitude, -Math.PI / 2f);
        }
        //// Pan right - rotate forward around up
        if (view.IsKeyDown(Keys.D))
        {
            longitude += RotationSpeed * (float)elapsed.TotalSeconds;
            longitude %= (float)(2f * Math.PI);
        }
        //// Pan left - rotate forward around up
        if (view.IsKeyDown(Keys.A))
        {
            longitude -= RotationSpeed * (float)elapsed.TotalSeconds;
            longitude %= (float)(2f * Math.PI);
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
        var longitudeRot = Quaternion.FromAxisAngle(Vector3.UnitY, longitude);
        var x = Vector3.Transform(Vector3.UnitX, longitudeRot);
        var latitudeRot = Quaternion.FromAxisAngle(Vector3.Cross(x, Vector3.UnitY), latitude);
        this.forward = -Vector3.Transform(x, latitudeRot);
        this.up = Vector3.Transform(Vector3.UnitY, latitudeRot);

        View = Matrix4.LookAt(Position, Vector3.Zero, up);
    }
}
