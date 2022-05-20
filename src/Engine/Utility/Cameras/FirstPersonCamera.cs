using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;

namespace MyOTKE.Engine
{
    /// <summary>
    /// An <see cref="ICamera"/> implementation that uses an "FPS"-style control scheme.
    /// </summary>
    public class FirstPersonCamera : ICamera
    {
        private readonly MyOTKEWindow view;
        private Vector3 position;
        private float horizontalAngle;
        private float verticalAngle;

        /// <summary>
        /// Initializes a new instance of the <see cref="FirstPersonCamera"/> class.
        /// </summary>
        /// <param name="view">The view from which to retrieve user input.</param>
        /// <param name="movementSpeed">The movement speed of the camera, in units per second.</param>
        /// <param name="rotationSpeed">The "rotation speed" of the camera - the multiplicand of the mouse cursor offset to radians.</param>
        /// <param name="fieldOfViewRadians">The field of view of the camera, in radians.</param>
        /// <param name="nearPlaneDistance">The distance of the near plane from the camera.</param>
        /// <param name="farPlaneDistance">The distance of the far plane from the camera.</param>
        /// <param name="initialPosition">The initial position of the camera.</param>
        /// <param name="initialHorizontalAngleRadians">The initial angle between the camera direction and the Y axis, in radians.</param>
        /// <param name="initialVerticalAngleRadians">The initial angle between between the camera direction and the X axis, in radians.</param>
        public FirstPersonCamera(
            MyOTKEWindow view,
            float movementSpeed,
            float rotationSpeed,
            float fieldOfViewRadians,
            float nearPlaneDistance,
            float farPlaneDistance,
            Vector3 initialPosition,
            float initialHorizontalAngleRadians,
            float initialVerticalAngleRadians)
        {
            this.view = view;
            this.MovementSpeed = movementSpeed;
            this.RotationSpeed = rotationSpeed;
            this.FieldOfViewRadians = fieldOfViewRadians;
            this.NearPlaneDistance = nearPlaneDistance;
            this.FarPlaneDistance = farPlaneDistance;
            this.position = initialPosition;
            this.horizontalAngle = initialHorizontalAngleRadians;
            this.verticalAngle = initialVerticalAngleRadians;
        }

        /// <summary>
        /// Gets or sets the movement speed of the camera, in units per second.
        /// </summary>
        public float MovementSpeed { get; set; } // = 3.0f;

        /// <summary>
        /// Gets or sets the "rotation speed" of the camera - the multiplicand of the mouse cursor offset to radians.
        /// </summary>
        public float RotationSpeed { get; set; } // = 0.005f;

        /// <summary>
        /// Gets or sets the field of view of the camera, in radians.
        /// </summary>
        public float FieldOfViewRadians { get; set; } // = (float)Math.PI / 4.0f;

        /// <inheritdoc />
        public Matrix4 View { get; private set; }

        /// <inheritdoc />
        public Matrix4 Projection { get; private set; }

        /// <summary>
        /// Gets or sets the distance of the near plane from the camera.
        /// </summary>
        private float NearPlaneDistance { get; set; } // = 0.1f;

        /// <summary>
        /// Gets or sets the distance of the far plane from the camera.
        /// </summary>
        private float FarPlaneDistance { get; set; } // = 100f;

        /// <inheritdoc />
        public void Update(TimeSpan elapsed)
        {
            if (view.LockCursor)
            {
                // Compute new orientation
                var xDiff = view.MouseCenterOffset.X;
                xDiff = Math.Abs(xDiff) < 2 ? 0 : xDiff;
                horizontalAngle -= RotationSpeed * xDiff;

                var yDiff = view.MouseCenterOffset.Y;
                yDiff = Math.Abs(yDiff) < 2 ? 0 : yDiff;
                verticalAngle -= RotationSpeed * yDiff;
                verticalAngle = Math.Max(-(float)Math.PI / 2, Math.Min(verticalAngle, (float)Math.PI / 2));
            }

            // Direction: Spherical coordinates to Cartesian coordinates conversion
            var direction = new Vector3(
                (float)(Math.Cos(verticalAngle) * Math.Sin(horizontalAngle)),
                (float)Math.Sin(verticalAngle),
                (float)(Math.Cos(verticalAngle) * Math.Cos(horizontalAngle)));

            // Right vector
            var right = new Vector3(
                (float)Math.Sin(horizontalAngle - 3.14f / 2.0f),
                0,
                (float)Math.Cos(horizontalAngle - 3.14f / 2.0f));

            // Up vector
            var up = Vector3.Cross(right, direction);

            //// Move forward
            if (view.IsKeyDown(Keys.W))
            {
                position += direction * (float)elapsed.TotalSeconds * MovementSpeed;
            }
            //// Move backward
            if (view.IsKeyDown(Keys.S))
            {
                position -= direction * (float)elapsed.TotalSeconds * MovementSpeed;
            }
            //// Strafe right
            if (view.IsKeyDown(Keys.D))
            {
                position += right * (float)elapsed.TotalSeconds * MovementSpeed;
            }
            //// Strafe left
            if (view.IsKeyDown(Keys.A))
            {
                position -= right * (float)elapsed.TotalSeconds * MovementSpeed;
            }

            Projection = Matrix4.CreatePerspectiveFieldOfView(
                FieldOfViewRadians,
                view.AspectRatio,
                NearPlaneDistance,
                FarPlaneDistance);

            // Camera matrix
            View = Matrix4.LookAt(
                position,             // Camera is here
                position + direction, // and looks here : at the same position, plus "direction"
                up);                  // Head is up (set to 0,-1,0 to look upside-down)
        }
    }
}
