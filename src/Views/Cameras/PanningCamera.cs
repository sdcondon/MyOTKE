﻿using OpenTK.Input;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Numerics;

namespace MyOTKE.Views
{
    /// <summary>
    /// Implementation of <see cref="ICamera"/> that moves in the XY plane.
    /// </summary>
    public class PanningCamera : ICamera
    {
        private const float ZoomDefaultDistance = 600f;
        private const float ZoomBase = 0.999f;

        private readonly View view;
        private readonly float movementSpeed;

        private Vector3 target;
        private int zoomLevel = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="PanningCamera"/> class.
        /// </summary>
        /// <param name="view">The view from which to retrieve input and aspect ratio.</param>
        /// <param name="fieldOfViewRadians">The camera's field of view, in radians.</param>
        /// <param name="nearPlaneDistance">The distance of the near plane from the camera.</param>
        /// <param name="farPlaneDistance">The ditance of the far plane from the camera.</param>
        /// <param name="initialTarget">The initial position at which the camera should point.</param>
        /// <param name="movementSpeed">The movement speed of the camera, in units per second per unit distance from target.</param>
        /// <param name="verticalAngle">The initial angle, in radians, between the camera's view direction and the Z-axis.</param>
        public PanningCamera(
            View view,
            float fieldOfViewRadians, // = (float)Math.PI / 4.0f;
            float nearPlaneDistance, // = 0.01f;
            float farPlaneDistance, // = 100f;
            Vector3 initialTarget,
            float movementSpeed,
            float verticalAngle)
        {
            this.view = view;
            FieldOfViewRadians = fieldOfViewRadians;
            NearPlaneDistance = nearPlaneDistance;
            FarPlaneDistance = farPlaneDistance;
            target = initialTarget;
            this.movementSpeed = movementSpeed / Distance;
            this.VerticalAngle = verticalAngle;
        }

        /// <summary>
        /// Gets or sets the camera's field of view, in radians.
        /// </summary>
        public float FieldOfViewRadians { get; set; }

        /// <summary>
        /// Gets or sets the distance of the near plane from the camera.
        /// </summary>
        public float NearPlaneDistance { get; set; }

        /// <summary>
        /// Gets or sets the distance of the far plane from the camera.
        /// </summary>
        public float FarPlaneDistance { get; set; }

        /// <summary>
        /// Gets or sets the current angle, in radians, between the camera's view direction and the Z-axis.
        /// </summary>
        public float VerticalAngle { get; set; }

        /// <summary>
        /// Gets or sets the maximum angle, in radians, between the camera's view direction and the Z-axis.
        /// </summary>
        public float VerticalAngleMax { get; set; } = 0.49f * (float)Math.PI;

        /// <summary>
        /// Gets or sets the vertical rotation speed of the camera, in radians per second.
        /// </summary>
        public float VerticalRotationSpeed { get; set; } = 1.0f;

        /// <summary>
        /// Gets or sets the current angle, in radians, between the camera's view direction and the X-axis.
        /// </summary>
        public float HorizontalAngle { get; set; }

        /// <summary>
        /// Gets or sets the horizontal rotation speed of the camera, in radians per second.
        /// </summary>
        public float HorizontalRotationSpeed { get; set; } = 1.0f;

        /// <summary>
        /// Gets the current distance between the camera and the target.
        /// </summary>
        public float Distance => (float)(ZoomDefaultDistance * Math.Pow(ZoomBase, zoomLevel));

        /// <inheritdoc />
        public Vector3 Position => target + Vector3.Transform(Vector3.UnitZ * Distance, Matrix4x4.CreateRotationZ(HorizontalAngle) * Matrix4x4.CreateRotationX(VerticalAngle));

        /// <inheritdoc />
        public Matrix4x4 View { get; private set; }

        /// <inheritdoc />
        public Matrix4x4 Projection { get; private set; }

        /// <inheritdoc />
        public void Update(TimeSpan elapsed)
        {
            if (view.IsKeyDown(Keys.W))
            {
                target += movementSpeed * (float)elapsed.TotalSeconds * Distance * Vector3.UnitY;
            }

            if (view.IsKeyDown(Keys.S))
            {
                target -= movementSpeed * (float)elapsed.TotalSeconds * Distance * Vector3.UnitY;
            }

            if (view.IsKeyDown(Keys.D))
            {
                target += movementSpeed * (float)elapsed.TotalSeconds * Distance * Vector3.UnitX;
            }

            if (view.IsKeyDown(Keys.A))
            {
                target -= movementSpeed * (float)elapsed.TotalSeconds * Distance * Vector3.UnitX;
            }

            if (view.IsKeyDown(Keys.R))
            {
                VerticalAngle -= VerticalRotationSpeed * (float)elapsed.TotalSeconds;
                VerticalAngle = Math.Max(VerticalAngle, 0);
            }

            if (view.IsKeyDown(Keys.F))
            {
                VerticalAngle += VerticalRotationSpeed * (float)elapsed.TotalSeconds;
                VerticalAngle = Math.Min(VerticalAngle, VerticalAngleMax);
            }

            if (view.IsKeyDown(Keys.Q))
            {
                HorizontalAngle -= HorizontalRotationSpeed * (float)elapsed.TotalSeconds;
                HorizontalAngle = Math.Max(HorizontalAngle, 0);
            }

            if (view.IsKeyDown(Keys.E))
            {
                HorizontalAngle += HorizontalRotationSpeed * (float)elapsed.TotalSeconds;
                HorizontalAngle = Math.Min(HorizontalAngle, (float)Math.PI * 2);
            }

            // Zoom
            zoomLevel += (int)view.MouseState.ScrollDelta.Y;

            // Projection matrix
            Projection = Matrix4x4.CreatePerspectiveFieldOfView(
                FieldOfViewRadians,
                view.AspectRatio,
                NearPlaneDistance,
                FarPlaneDistance);

            // Camera matrix
            View = Matrix4x4.CreateLookAt(Position, target, Vector3.UnitY);
        }
    }
}
