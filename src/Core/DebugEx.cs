using OpenTK.Graphics.OpenGL;
using System;
using System.Diagnostics;

namespace MyOTKE.Core
{
    /// <summary>
    /// Debug utility methods.
    /// </summary>
    internal static class DebugEx
    {
        /// <summary>
        /// Records a debug message, prefixed by the calling type and method.
        /// </summary>
        /// <param name="message">The message to be recorded.</param>
        [Conditional("DEBUG")]
        public static void WriteLine(string message)
        {
            var method = new StackFrame(1).GetMethod();
            Debug.WriteLine(message, $"{method.DeclaringType.FullName}::{method.Name}");
        }

        /// <summary>
        /// Throws an exception if the Open GL error flag is set (and clears the error).
        /// </summary>
        /// <param name="action">The action that was just carried out (use the present participle to make the message read correctly - e.g. "doing the thing").</param>
        [Conditional("DEBUG")]
        public static void ThrowIfGlError(string action)
        {
            var errorCode = GL.GetError();
            if (errorCode != ErrorCode.NoError)
            {
                // TODO: exception type..
                throw new Exception($"Open GL registered an error while {action}: {errorCode}");
            }
        }
    }
}
