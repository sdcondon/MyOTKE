﻿using OpenTK.Mathematics;

namespace MyOTKE.Renderables
{
    /// <summary>
    /// A source of view and projection matrices. Used by some <see cref="IRenderable"/> implementations.
    /// </summary>
    public interface IViewProjection
    {
        /// <summary>
        /// Gets the view matrix.
        /// </summary>
        Matrix4 View { get; }

        /// <summary>
        /// Gets the projection matrix.
        /// </summary>
        Matrix4 Projection { get; }
    }
}