﻿using OpenTK.Graphics.OpenGL;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MyOTKE.Core
{
    /// <summary>
    /// Debug utility methods for Open GL.
    /// </summary>
    public static class GlDebug
    {
        /// <summary>
        /// Registers Open GL debug callback that writes to debug trace listeners (i.e. calls <see cref="Debug.WriteLine(object, string)"/> with "OPENGL" as the category).
        /// </summary>
        [Conditional("DEBUG")]
        public static void RegisterDebugCallback()
        {
            Debug.WriteLine("Registering OpenGL debug handler");
            GL.DebugMessageCallback(OnGlDebugMessage, IntPtr.Zero);

            ////KhronosApi.LogEnabled = true;
            ////KhronosApi.Log += KhronosApi_Log;

            void OnGlDebugMessage(
                DebugSource source,
                DebugType type,
                int id,
                DebugSeverity severity,
                int length,
                IntPtr message,
                IntPtr userParam)
            {
                var messageString = Marshal.PtrToStringAuto(message);
                Debug.WriteLine($"{id} {source} {type} {severity}: {messageString}", "OPENGL");
            }

            ////void KhronosApi_Log(object sender, KhronosLogEventArgs e)
            ////{
            ////    Debug.WriteLine($"{e.Name}({string.Join(',', e.Args)}) {e.ReturnValue}", "KHRONOS API");
            ////}
        }

        /// <summary>
        /// Records a debug message, prefixed by the calling type and method.
        /// </summary>
        /// <param name="message">The message to be recorded.</param>
        [Conditional("DEBUG")]
        internal static void WriteLine(string message)
        {
            var method = new StackFrame(1).GetMethod();
            Debug.WriteLine(message, $"{method.DeclaringType.FullName}::{method.Name}");
        }

        /// <summary>
        /// Throws an exception if the Open GL error flag is set (and clears the error).
        /// </summary>
        /// <param name="action">The action that was just carried out (use the present participle to make the message read correctly - e.g. "doing the thing").</param>
        [Conditional("DEBUG")]
        internal static void ThrowIfGlError(string action)
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
