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
    }
}
