#pragma warning disable SA1402
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MyOTKE.Core
{
    /// <summary>
    /// Represents a compiled OpenGL program.
    /// </summary>
    public sealed class GlProgram : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GlProgram"/> class.
        /// </summary>
        /// <param name="shaderSpecs">Specifications for each of the shaders to be included in the program.</param>
        internal GlProgram(IEnumerable<(ShaderType Type, string Source)> shaderSpecs)
        {
            // Create program
            this.Id = GL.CreateProgram();

            // Compile shaders
            var shaderIds = new List<int>();
            foreach (var shaderSpec in shaderSpecs)
            {
                // Create shader
                var shaderId = GL.CreateShader(shaderSpec.Type);

                // Compile shader
                GL.ShaderSource(shaderId, shaderSpec.Source);
                GlDebug.ThrowIfGlError("setting shader source");
                GL.CompileShader(shaderId);
                GlDebug.ThrowIfGlError("compiling shader");

                // Check shader
                GL.GetShader(shaderId, ShaderParameter.CompileStatus, out var compileStatus);
                if (compileStatus != (int)OpenTK.Graphics.OpenGL.Boolean.True)
                {
                    throw new ArgumentException("Shader compilation failed: " + GL.GetShaderInfoLog(shaderId), nameof(shaderSpecs));
                }

                GL.AttachShader(this.Id, shaderId);
                GlDebug.ThrowIfGlError("attaching shader");
                shaderIds.Add(shaderId);

                ////TODO: for blockName/bindingPoint from uboSpecs
                ////uniformBlockIndex = GL.GetUniformBlockIndex(shaderId, blockName);
                ////if (uniformBlockIndex != GL_INVALID_INDEX)
                ////{
                ////    GL.UniformBlockBinding(shaderId, uniformBlockIndex, bindingPoint);
                ////}
            }

            ////TODO: for blockName/bindingPoint from uboSpecs
            ////unsigned int uboExampleBlock;
            ////GL.GenBuffers(1, &uboExampleBlock);
            ////GL.BindBuffer(GL_UNIFORM_BUFFER, uboExampleBlock);
            ////GL.BufferData(GL_UNIFORM_BUFFER, 152, NULL, GL_STATIC_DRAW); // allocate 152 bytes of memory

            // Link & check program
            GL.LinkProgram(this.Id);
            GlDebug.ThrowIfGlError("linking program");
            GL.GetProgram(this.Id, GetProgramParameterName.LinkStatus, out var linkStatus);
            if (linkStatus != (int)OpenTK.Graphics.OpenGL.Boolean.True)
            {
                throw new ArgumentException("Program linking failed: " + GL.GetProgramInfoLog(this.Id), nameof(shaderSpecs));
            }

            // Detach and delete shaders
            foreach (var shaderId in shaderIds)
            {
                GL.DetachShader(this.Id, shaderId); // Line not in superbible?
                GlDebug.ThrowIfGlError("detaching shader");
                GL.DeleteShader(shaderId);
                GlDebug.ThrowIfGlError("deleting shader");
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="GlProgram"/> class.
        /// </summary>
        ~GlProgram() => Dispose(false);

        /// <summary>
        /// Gets the OpenGL identifier for this program.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Installs the program as part of the current rendering state and sets the current uniform values (using the default uniform block).
        /// </summary>
        public void Use()
        {
            GL.UseProgram(this.Id);
            GlDebug.ThrowIfGlError("using program");
        }

        /// <inheritdoc />
        public void Dispose() => Dispose(true);

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }

            GL.DeleteProgram(this.Id);
            GlDebug.ThrowIfGlError("deleting program");
        }
    }

    /// <summary>
    /// Represents a compiled OpenGL program that makes use of uniforms in the default uniform block.
    /// </summary>
    /// <typeparam name="TDefaultUniformBlock">The type of the container used for default block uniforms.</typeparam>
    public sealed class GlProgramWithDUB<TDefaultUniformBlock> : IDisposable
        where TDefaultUniformBlock : struct
    {
        private readonly Action<TDefaultUniformBlock> setDefaultUniformBlock;

        /// <summary>
        /// Initializes a new instance of the <see cref="GlProgramWithDUB{TDefaultUniformBlock}"/> class.
        /// </summary>
        /// <param name="shaderSpecs">Specifications for each of the shaders to be included in the program.</param>
        internal GlProgramWithDUB(IEnumerable<(ShaderType Type, string Source)> shaderSpecs)
        {
            // Create program
            this.Id = GL.CreateProgram();
            GlDebug.ThrowIfGlError("creating program");

            // Compile shaders
            var shaderIds = new List<int>();
            foreach (var shaderSpec in shaderSpecs)
            {
                // Create shader
                var shaderId = GL.CreateShader(shaderSpec.Type);

                // Compile shader
                GL.ShaderSource(shaderId, shaderSpec.Source);
                GlDebug.ThrowIfGlError("setting shader source");
                GL.CompileShader(shaderId);
                GlDebug.ThrowIfGlError("compiling shader");

                // Check shader
                GL.GetShader(shaderId, ShaderParameter.CompileStatus, out var compileStatus);
                if (compileStatus != (int)OpenTK.Graphics.OpenGL.Boolean.True)
                {
                    throw new ArgumentException("Shader compilation failed: " + GL.GetShaderInfoLog(shaderId), nameof(shaderSpecs));
                }

                GL.AttachShader(this.Id, shaderId);
                GlDebug.ThrowIfGlError("attaching shader");
                shaderIds.Add(shaderId);
            }

            // Link & check program
            GL.LinkProgram(this.Id);
            GL.GetProgram(this.Id, GetProgramParameterName.LinkStatus, out var linkStatus);
            if (linkStatus != (int)OpenTK.Graphics.OpenGL.Boolean.True)
            {
                throw new ArgumentException("Program linking failed: " + GL.GetProgramInfoLog(this.Id), nameof(shaderSpecs));
            }

            // Detach and delete shaders
            foreach (var shaderId in shaderIds)
            {
                GL.DetachShader(this.Id, shaderId); // Line not in superbible?
                GlDebug.ThrowIfGlError("detaching shader");
                GL.DeleteShader(shaderId);
                GlDebug.ThrowIfGlError("deleting shader");
            }

            // Get uniform IDs
            this.setDefaultUniformBlock = GlMarshal.MakeDefaultUniformBlockSetter<TDefaultUniformBlock>(this.Id);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="GlProgramWithDUB{TDefaultUniformBlock}"/> class.
        /// </summary>
        ~GlProgramWithDUB() => Dispose(false);

        /// <summary>
        /// Gets the OpenGL identifier for this program.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Installs the program as part of the current rendering state and sets the current uniform values (using the default uniform block).
        /// </summary>
        /// <param name="uniforms">The uniform values.</param>
        public void UseWithUniformValues(TDefaultUniformBlock uniforms)
        {
            GL.UseProgram(this.Id);
            GlDebug.ThrowIfGlError("using program");
            setDefaultUniformBlock(uniforms);
        }

        /// <inheritdoc />
        public void Dispose() => Dispose(true);

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }

            GL.DeleteProgram(this.Id);
            GlDebug.ThrowIfGlError("deleting program");
        }
    }
}
#pragma warning restore SA1402