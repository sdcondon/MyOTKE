﻿using OpenTK.Graphics.OpenGL;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MyOTKE.Core;

/// <summary>
/// Debug utility methods for Open GL.
/// </summary>
public static class GlDebug
{
    /// <summary>
    /// Registers an OpenGL debug callback that writes to debug trace listeners (i.e. calls <see cref="Debug.WriteLine(object, string)"/>), with "OpenGL" as the category.
    /// </summary>
    [Conditional("DEBUG")]
    public static void RegisterDebugCallback()
    {
        WriteLine("Registering OpenGL debug handler");
        GL.DebugMessageCallback(OnGlDebugMessage, IntPtr.Zero);

        ////KhronosApi.LogEnabled = true;
        ////KhronosApi.Log += KhronosApi_Log;

        static void OnGlDebugMessage(
            DebugSource source,
            DebugType type,
            int id,
            DebugSeverity severity,
            int length,
            IntPtr message,
            IntPtr userParam)
        {
            var messageString = Marshal.PtrToStringAuto(message);
            Debug.WriteLine($"{id} {source} {type} {severity}: {messageString}", "OpenGL");
        }

        ////void KhronosApi_Log(object sender, KhronosLogEventArgs e)
        ////{
        ////    Debug.WriteLine($"{e.Name}({string.Join(',', e.Args)}) {e.ReturnValue}", "KHRONOS API");
        ////}
    }

    /// <summary>
    /// Throws an exception if the OpenGL error queue is non-empty (and clears the error).
    /// </summary>
    /// <param name="action">The action that was just carried out (use the present participle to make the message read correctly - e.g. "doing the thing").</param>
    [Conditional("DEBUG")]
    public static void ThrowIfGlError(string action)
    {
        // TODO: There may be many - consume them all (i.e. use a while, not an if) and throw an aggregate if there are multiple?
        var errorCode = GL.GetError();
        if (errorCode != ErrorCode.NoError)
        {
            // TODO: exception type..
            throw new Exception($"Open GL registered an error while {action}: {errorCode}");
        }
    }

    /// <summary>
    /// Records a debug message, prefixed by the calling type and method.
    /// </summary>
    /// <param name="message">The message to be recorded.</param>
    [Conditional("DEBUG")]
    internal static void WriteLine(string message)
    {
        var method = new StackFrame(1).GetMethod();
        Debug.WriteLine($"{method.DeclaringType.FullName}::{method.Name}: {message}", "MyOTKE");
    }
}
