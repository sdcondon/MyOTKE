#pragma warning disable SA1402
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;

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
        internal GlProgram(
            IEnumerable<(ShaderType Type, string Source)> shaderSpecs)
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
            GlDebug.ThrowIfGlError("linking program");
            GL.GetProgram(this.Id, GetProgramParameterName.LinkStatus, out var linkStatus);
            GlDebug.ThrowIfGlError("getting program link status");
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
        /// Installs the program as part of the current rendering state.
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
    /// Represents a compiled OpenGL program.
    /// </summary>
    /// <typeparam name="T1">The type of the 1st uniform buffer object.</typeparam>
    public sealed class GlProgram<T1> : IDisposable
        where T1 : struct
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GlProgram{T1}"/> class.
        /// </summary>
        /// <param name="uboSpec1">Spec for the 1st uniform buffer.</param>
        /// <param name="shaderSpecs">Specifications for each of the shaders to be included in the program.</param>
        internal GlProgram(
            (string BlockName, BufferUsageHint Usage, int Capacity, T1[] Data) uboSpec1,
            IEnumerable<(ShaderType Type, string Source)> shaderSpecs)
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
            GlDebug.ThrowIfGlError("linking program");
            GL.GetProgram(this.Id, GetProgramParameterName.LinkStatus, out var linkStatus);
            GlDebug.ThrowIfGlError("getting program link status");
            if (linkStatus != (int)OpenTK.Graphics.OpenGL.Boolean.True)
            {
                throw new ArgumentException("Program linking failed: " + GL.GetProgramInfoLog(this.Id), nameof(shaderSpecs));
            }

            // Create uniform buffers
            UniformBuffer1 = new GlUniformBufferObject<T1>(Id, uboSpec1.BlockName, uboSpec1.Usage, uboSpec1.Capacity, uboSpec1.Data);

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
        /// Finalizes an instance of the <see cref="GlProgram{T1}"/> class.
        /// </summary>
        ~GlProgram() => Dispose(false);

        /// <summary>
        /// Gets the OpenGL identifier for this program.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Gets the 1st uniform buffer object used by this program.
        /// </summary>
        public GlUniformBufferObject<T1> UniformBuffer1 { get; }

        /// <summary>
        /// Installs the program as part of the current rendering state.
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
                UniformBuffer1.Dispose();
            }

            GL.DeleteProgram(this.Id);
            GlDebug.ThrowIfGlError("deleting program");
        }
    }

    /// <summary>
    /// Represents a compiled OpenGL program that makes use of the default uniform block.
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
        internal GlProgramWithDUB(
            IEnumerable<(ShaderType Type, string Source)> shaderSpecs)
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
            GlDebug.ThrowIfGlError("linking program");
            GL.GetProgram(this.Id, GetProgramParameterName.LinkStatus, out var linkStatus);
            GlDebug.ThrowIfGlError("getting program link status");
            if (linkStatus != (int)OpenTK.Graphics.OpenGL.Boolean.True)
            {
                throw new ArgumentException("Program linking failed: " + GL.GetProgramInfoLog(this.Id), nameof(shaderSpecs));
            }

            // Build default uniform block setter
            this.setDefaultUniformBlock = GlMarshal.MakeDefaultUniformBlockSetter<TDefaultUniformBlock>(this.Id);

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
        /// Finalizes an instance of the <see cref="GlProgramWithDUB{TDefaultUniformBlock}"/> class.
        /// </summary>
        ~GlProgramWithDUB() => Dispose(false);

        /// <summary>
        /// Gets the OpenGL identifier for this program.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Installs the program as part of the current rendering state and sets the value of the uniforms in the default block.
        /// </summary>
        /// <param name="defaultUniformBlock">The values for the uniforms in the default block.</param>
        public void UseWithDefaultUniformBlock(TDefaultUniformBlock defaultUniformBlock)
        {
            GL.UseProgram(this.Id);
            GlDebug.ThrowIfGlError("using program");
            setDefaultUniformBlock(defaultUniformBlock);
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
    /// Represents a compiled OpenGL program that makes use of the default uniform block.
    /// </summary>
    /// <typeparam name="TDefaultUniformBlock">The type of the container used for default block uniforms.</typeparam>
    /// <typeparam name="T1">The type of the 1st uniform buffer object.</typeparam>
    public sealed class GlProgramWithDUB<TDefaultUniformBlock, T1> : IDisposable
        where TDefaultUniformBlock : struct
        where T1 : struct
    {
        private readonly Action<TDefaultUniformBlock> setDefaultUniformBlock;

        /// <summary>
        /// Initializes a new instance of the <see cref="GlProgramWithDUB{TDefaultUniformBlock, T1}"/> class.
        /// </summary>
        /// <param name="uboSpec1">Spec for the 1st uniform buffer.</param>
        /// <param name="shaderSpecs">Specifications for each of the shaders to be included in the program.</param>
        internal GlProgramWithDUB(
            (string BlockName, BufferUsageHint Usage, int Capacity, T1[] Data) uboSpec1,
            IEnumerable<(ShaderType Type, string Source)> shaderSpecs)
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
            GlDebug.ThrowIfGlError("linking program");
            GL.GetProgram(this.Id, GetProgramParameterName.LinkStatus, out var linkStatus);
            GlDebug.ThrowIfGlError("getting program link status");
            if (linkStatus != (int)OpenTK.Graphics.OpenGL.Boolean.True)
            {
                throw new ArgumentException("Program linking failed: " + GL.GetProgramInfoLog(this.Id), nameof(shaderSpecs));
            }

            // Build default uniform block setter
            this.setDefaultUniformBlock = GlMarshal.MakeDefaultUniformBlockSetter<TDefaultUniformBlock>(this.Id);

            // Create uniform buffers
            UniformBuffer1 = new GlUniformBufferObject<T1>(Id, uboSpec1.BlockName, uboSpec1.Usage, uboSpec1.Capacity, uboSpec1.Data);

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
        /// Finalizes an instance of the <see cref="GlProgramWithDUB{TDefaultUniformBlock, T1}"/> class.
        /// </summary>
        ~GlProgramWithDUB() => Dispose(false);

        /// <summary>
        /// Gets the OpenGL identifier for this program.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Gets the 1st uniform buffer object used by this program.
        /// </summary>
        public GlUniformBufferObject<T1> UniformBuffer1 { get; }

        /// <summary>
        /// Installs the program as part of the current rendering state and sets the value of the uniforms in the default block.
        /// </summary>
        /// <param name="defaultUniformBlock">The values for the uniforms in the default block.</param>
        public void UseWithDefaultUniformBlock(TDefaultUniformBlock defaultUniformBlock)
        {
            GL.UseProgram(this.Id);
            GlDebug.ThrowIfGlError("using program");
            setDefaultUniformBlock(defaultUniformBlock);
        }

        /// <inheritdoc />
        public void Dispose() => Dispose(true);

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                GC.SuppressFinalize(this);
                UniformBuffer1.Dispose();
            }

            GL.DeleteProgram(this.Id);
            GlDebug.ThrowIfGlError("deleting program");
        }
    }
}
#pragma warning restore SA1402