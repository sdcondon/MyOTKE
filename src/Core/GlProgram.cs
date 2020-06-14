using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;

namespace MyOTKE.Core
{
    /// <summary>
    /// Represents a compiled OpenGL program.
    /// </summary>
    public sealed class GlProgram : IDisposable
    {
        private readonly int id;
        private readonly int[] uniformIds;

        /// <summary>
        /// Initializes a new instance of the <see cref="GlProgram"/> class.
        /// </summary>
        /// <param name="shaderSpecs">Specifications for each of the shaders to be included in the program.</param>
        /// <param name="uniforms">The names of the unifoms used by the shaders.</param>
        internal GlProgram(IEnumerable<(ShaderType Type, string Source)> shaderSpecs, string[] uniforms)
        {
            GlExt.ThrowIfNoCurrentContext();

            // Create program
            this.id = GL.CreateProgram();

            // Compile shaders
            var shaderIds = new List<int>();
            foreach (var shaderSpec in shaderSpecs)
            {
                // Create shader
                var shaderId = GL.CreateShader(shaderSpec.Type);

                // Compile shader
                GlExt.DebugWriteLine("Compiling shader");
                GL.ShaderSource(shaderId, shaderSpec.Source);
                GL.CompileShader(shaderId);

                // Check shader
                GL.GetShader(shaderId, ShaderParameter.InfoLogLength, out var shaderInfoLogLength);
                if (shaderInfoLogLength > 0)
                {
                    Trace.WriteLine(GL.GetShaderInfoLog(shaderId));
                }

                GL.AttachShader(this.id, shaderId);
                shaderIds.Add(shaderId);
            }

            // Link & check program
            GlExt.DebugWriteLine("Linking program");
            GL.LinkProgram(this.id);
            GL.GetProgram(this.id, GetProgramParameterName.InfoLogLength, out var programInfoLogLength);
            if (programInfoLogLength > 0)
            {
                Trace.TraceError(GL.GetProgramInfoLog(this.id));
            }

            // Detach and delete shaders
            foreach (var shaderId in shaderIds)
            {
                GL.DetachShader(this.id, shaderId); // Line not in superbible?
                GL.DeleteShader(shaderId);
            }

            // Get uniform IDs
            this.uniformIds = uniforms.Select(x => GL.GetUniformLocation(this.id, x)).ToArray();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="GlProgram"/> class.
        /// </summary>
        ~GlProgram() => Dispose(false);

        /// <summary>
        /// Installs the program as part of the current rendering state and sets the current uniform values (using the default uniform block).
        /// </summary>
        /// <param name="values">The uniform values (in the order in which they were registered).</param>
        public void UseWithUniformValues(params object[] values)
        {
            GL.UseProgram(this.id);
            for (int i = 0; i < uniformIds.Length; i++)
            {
                switch (values[i])
                {
                    case Matrix4x4 m:
                        // NB: If transpose argument is false, OpenGL expects arrays in column major order.
                        // We set transpose to true for readability (and thus maintainability) - so that
                        // our little matrix array below looks right.
                        var value = new[]
                        {
                            m.M11, m.M12, m.M13, m.M14,
                            m.M21, m.M22, m.M23, m.M24,
                            m.M31, m.M32, m.M33, m.M34,
                            m.M41, m.M42, m.M43, m.M44,
                        };
                        GL.UniformMatrix4(uniformIds[i], 1, true, value);
                        break;
                    case Vector3 v:
                        GL.Uniform3(uniformIds[i], v.X, v.Y, v.Z);
                        break;
                    case float f:
                        GL.Uniform1(uniformIds[i], f);
                        break;
                    case int iv:
                        GL.Uniform1(uniformIds[i], iv);
                        break;
                    case uint u:
                        GL.Uniform1(uniformIds[i], u);
                        break;
                    case long l:
                        GL.Uniform1(uniformIds[i], l);
                        break;
                    default:
                        throw new ArgumentException($"Contains value of unsupported type {values[i].GetType()} at index {i}", nameof(values));
                }
            }
        }

        /// <inheritdoc />
        public void Dispose() => Dispose(true);

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }

            if (GraphicsContext.CurrentContext != null)
            {
                GL.DeleteProgram(this.id);
            }
        }
    }
}
