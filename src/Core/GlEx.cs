using OpenTK.Graphics;
using System;

namespace MyOTKE.Core
{
    /// <summary>
    /// Static OpenGL helper methods.
    /// </summary>
    internal static class GlEx
    {
        /// <summary>
        /// Throws an <see cref="InvalidOperationException"/> if there is no OpenGL context current on the calling thread.
        /// </summary>
        public static void ThrowIfNoCurrentContext()
        {
            if (GraphicsContext.CurrentContext == null)
            {
                throw new InvalidOperationException("Cannot do OpenGL operations because the calling thread has no current OpenGL context");
            }
        }
    }
}
