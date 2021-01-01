﻿using OpenTK.Input;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Numerics;

namespace MyOTKE.Renderables
{
    /// <summary>
    /// Implementation of <see cref="ICamera"/> that rotates around the origin.
    /// </summary>
    public class OrbitCamera : ICamera
    {
        private readonly MyOTKEWindow view;

        private Vector3 forward = new Vector3(0f, 0f, 1f);
        private Vector3 up = new Vector3(0f, 1f, 0f);
        private int zoomLevel = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrbitCamera"/> class.
        /// </summary>
        /// <param name="view">The view from which to retrieve input and aspect ratio.</param>
        /// <param name="rotationSpeedBase">The base (i.e. at default zoom distance) rotation speed of the camera, in radians per per update.</param>
        /// <param name="rollSpeed">The roll speed of the camera, in radians per update.</param>
        /// <param name="fieldOfViewRadians">The camera's field of view, in radians.</param>
        /// <param name="nearPlaneDistance">The distance of the near plane from the camera.</param>
        /// <param name="farPlaneDistance">The ditance of the far plane from the camera.</param>
        public OrbitCamera(
            MyOTKEWindow view,
            float rotationSpeedBase,
            float rollSpeed,
            float fieldOfViewRadians,
            float nearPlaneDistance,
            float farPlaneDistance)
        {
            this.view = view;
            RotationSpeedBase = rotationSpeedBase;
            RollSpeed = rollSpeed;
            FieldOfViewRadians = fieldOfViewRadians;
            NearPlaneDistance = nearPlaneDistance;
            FarPlaneDistance = farPlaneDistance;
        }

        /// <summary>
        /// Gets or sets the base (i.e. at default zoom distance) rotation speed of the camera in radians per update.
        /// </summary>
        public float RotationSpeedBase { get; set; } // = 0.01f;

        /// <summary>
        /// Gets or sets the roll speed of the camera, in radians per update.
        /// </summary>
        public float RollSpeed { get; set; } // = 0.01f;

        /// <summary>
        /// Gets or sets the camera's field of view, in radians.
        /// </summary>
        public float FieldOfViewRadians { get; set; } // = (float)Math.PI / 4.0f;

        /// <summary>
        /// Gets or sets the distance of the near plane from the camera.
        /// </summary>
        public float NearPlaneDistance { get; set; } // = 0.01f;

        /// <summary>
        /// Gets or sets the distance of the far plane from the camera.
        /// </summary>
        public float FarPlaneDistance { get; set; } // = 100f;

        /// <summary>
        /// Gets the current distance between the camera and the origin.
        /// </summary>
        public float Distance => (float)(ZoomMinDistance + ZoomDefaultDistance * Math.Pow(ZoomBase, zoomLevel));

        /// <summary>
        /// Gets the current rotation speed of the camera, in radians per update.
        /// </summary>
        public float RotationSpeed => RotationSpeedBase * (Distance - ZoomMinDistance) / ZoomDefaultDistance;

        /// <inheritdoc />
        public Vector3 Position => -forward * Distance;

        /// <inheritdoc />
        public Matrix4x4 View { get; private set; }

        /// <inheritdoc />
        public Matrix4x4 Projection { get; private set; }

        private float ZoomDefaultDistance { get; set; } = 1.5f;

        private float ZoomBase { get; set; } = 0.999f;

        private float ZoomMinDistance => 1f + NearPlaneDistance;

        /// <inheritdoc />
        public void Update(TimeSpan elapsed)
        {
            //// Pan up - rotate forward and up around their cross product
            if (view.IsKeyDown(Keys.W))
            {
                var t = Matrix4x4.CreateFromAxisAngle(Vector3.Cross(forward, up), -RotationSpeed);
                forward = Vector3.Transform(forward, t);
                up = Vector3.Transform(up, t);
            }
            //// Pan down - rotate forward and up around their cross product
            if (view.IsKeyDown(Keys.S))
            {
                var t = Matrix4x4.CreateFromAxisAngle(Vector3.Cross(forward, up), RotationSpeed);
                forward = Vector3.Normalize(Vector3.Transform(forward, t));
                up = Vector3.Normalize(Vector3.Transform(up, t));
            }
            //// Pan right - rotate forward around up
            if (view.IsKeyDown(Keys.D))
            {
                forward = Vector3.Normalize(Vector3.Transform(forward, Matrix4x4.CreateFromAxisAngle(up, RotationSpeed)));
            }
            //// Pan left - rotate forward around up
            if (view.IsKeyDown(Keys.A))
            {
                forward = Vector3.Normalize(Vector3.Transform(forward, Matrix4x4.CreateFromAxisAngle(up, -RotationSpeed)));
            }
            //// Roll right - rotate up around forward
            if (view.IsKeyDown(Keys.Q))
            {
                up = Vector3.Normalize(Vector3.Transform(up, Matrix4x4.CreateFromAxisAngle(forward, -RollSpeed)));
            }
            //// Roll left - rotate up around forward
            if (view.IsKeyDown(Keys.E))
            {
                up = Vector3.Normalize(Vector3.Transform(up, Matrix4x4.CreateFromAxisAngle(forward, RollSpeed)));
            }
            //// Zoom
            zoomLevel += (int)view.MouseState.ScrollDelta.Y;

            // Projection matrix
            Projection = Matrix4x4.CreatePerspectiveFieldOfView(
                FieldOfViewRadians,
                view.AspectRatio,
                NearPlaneDistance,
                FarPlaneDistance);

            // Camera matrix
            View = Matrix4x4.CreateLookAt(Position, Vector3.Zero, up);
        }
    }
}
