﻿using System;
using System.Numerics;

namespace OTKOW.Views
{
    /// <summary>
    /// Interface for types that provide view and projection matrices, and can change over time (e.g. in reponse to user input).
    /// </summary>
    public interface ICamera : IViewProjection
    {
        /// <summary>
        /// Gets the current position of the camera.
        /// </summary>
        Vector3 Position { get; }

        /// <summary>
        /// Updates the state of the camera.
        /// </summary>
        /// <param name="elapsed">The elapsed time since the last update.</param>
        void Update(TimeSpan elapsed);
    }
}
