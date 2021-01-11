﻿#pragma warning disable SA1402
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;

namespace MyOTKE.Core
{
    /// <summary>
    /// Represents a compiled OpenGL program.
    /// </summary>
    public sealed class GlProgram : IDisposable
    {
        private readonly int id;

        /// <summary>
        /// Initializes a new instance of the <see cref="GlProgram"/> class.
        /// </summary>
        /// <param name="shaderSpecs">Specifications for each of the shaders to be included in the program.</param>
        internal GlProgram(IEnumerable<(ShaderType Type, string Source)> shaderSpecs)
        {
            GlEx.ThrowIfNoCurrentContext();

            // Create program
            this.id = GL.CreateProgram();

            // Compile shaders
            var shaderIds = new List<int>();
            foreach (var shaderSpec in shaderSpecs)
            {
                // Create shader
                var shaderId = GL.CreateShader(shaderSpec.Type);

                // Compile shader
                GL.ShaderSource(shaderId, shaderSpec.Source);
                DebugEx.ThrowIfGlError("setting shader source");
                GL.CompileShader(shaderId);
                DebugEx.ThrowIfGlError("compiling shader");

                // Check shader
                GL.GetShader(shaderId, ShaderParameter.CompileStatus, out var compileStatus);
                if (compileStatus != (int)OpenTK.Graphics.OpenGL.Boolean.True)
                {
                    throw new ArgumentException("Shader compilation failed: " + GL.GetShaderInfoLog(shaderId), nameof(shaderSpecs));
                }

                GL.AttachShader(this.id, shaderId);
                DebugEx.ThrowIfGlError("attaching shader");
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
            GL.LinkProgram(this.id);
            DebugEx.ThrowIfGlError("linking program");
            GL.GetProgram(this.id, GetProgramParameterName.LinkStatus, out var linkStatus);
            if (linkStatus != (int)OpenTK.Graphics.OpenGL.Boolean.True)
            {
                throw new ArgumentException("Program linking failed: " + GL.GetProgramInfoLog(this.id), nameof(shaderSpecs));
            }

            // Detach and delete shaders
            foreach (var shaderId in shaderIds)
            {
                GL.DetachShader(this.id, shaderId); // Line not in superbible?
                DebugEx.ThrowIfGlError("detaching shader");
                GL.DeleteShader(shaderId);
                DebugEx.ThrowIfGlError("deleting shader");
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="GlProgram"/> class.
        /// </summary>
        ~GlProgram() => Dispose(false);

        /// <summary>
        /// Installs the program as part of the current rendering state and sets the current uniform values (using the default uniform block).
        /// </summary>
        public void Use()
        {
            GL.UseProgram(this.id);
            DebugEx.ThrowIfGlError("using program");
        }

        /// <inheritdoc />
        public void Dispose() => Dispose(true);

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }

            ////if (GraphicsContext.CurrentContext != null)
            {
                GL.DeleteProgram(this.id);
                DebugEx.ThrowIfGlError("deleting program");
            }
        }
    }

    /// <summary>
    /// Represents a compiled OpenGL program that makes use of uniforms in the default uniform block.
    /// </summary>
    /// <typeparam name="TDefaultUniformBlock">The type of the container used for default block uniforms.</typeparam>
    public sealed class GlProgramWithDUB<TDefaultUniformBlock> : IDisposable
        where TDefaultUniformBlock : struct
    {
        private static readonly Dictionary<Type, Expression> DefaultBlockUniformSettersByType = new Dictionary<Type, Expression>
        {
            [typeof(Matrix4x4)] = (Expression<Action<int, Matrix4x4>>)((i, m) => GL.UniformMatrix4(i, 1, true, new[] { m.M11, m.M12, m.M13, m.M14, m.M21, m.M22, m.M23, m.M24, m.M31, m.M32, m.M33, m.M34, m.M41, m.M42, m.M43, m.M44 })),
            [typeof(Vector3)] = (Expression<Action<int, Vector3>>)((i, v) => GL.Uniform3(i, v.X, v.Y, v.Z)),
            [typeof(float)] = (Expression<Action<int, float>>)((i, f) => GL.Uniform1(i, f)),
            [typeof(int)] = (Expression<Action<int, int>>)((i, iv) => GL.Uniform1(i, iv)),
            [typeof(uint)] = (Expression<Action<int, uint>>)((i, u) => GL.Uniform1(i, u)),
            [typeof(long)] = (Expression<Action<int, long>>)((i, l) => GL.Uniform1(i, l)),
        };

        private readonly int id;
        private readonly Action<TDefaultUniformBlock> setDefaultUniformBlock;

        /// <summary>
        /// Initializes a new instance of the <see cref="GlProgramWithDUB{TDefaultUniformBlock}"/> class.
        /// </summary>
        /// <param name="shaderSpecs">Specifications for each of the shaders to be included in the program.</param>
        internal GlProgramWithDUB(IEnumerable<(ShaderType Type, string Source)> shaderSpecs)
        {
            GlEx.ThrowIfNoCurrentContext();

            // Create program
            this.id = GL.CreateProgram();
            DebugEx.ThrowIfGlError("creating program");

            // Compile shaders
            var shaderIds = new List<int>();
            foreach (var shaderSpec in shaderSpecs)
            {
                // Create shader
                var shaderId = GL.CreateShader(shaderSpec.Type);

                // Compile shader
                GL.ShaderSource(shaderId, shaderSpec.Source);
                DebugEx.ThrowIfGlError("setting shader source");
                GL.CompileShader(shaderId);
                DebugEx.ThrowIfGlError("compiling shader");

                // Check shader
                GL.GetShader(shaderId, ShaderParameter.CompileStatus, out var compileStatus);
                if (compileStatus != (int)OpenTK.Graphics.OpenGL.Boolean.True)
                {
                    throw new ArgumentException("Shader compilation failed: " + GL.GetShaderInfoLog(shaderId), nameof(shaderSpecs));
                }

                GL.AttachShader(this.id, shaderId);
                DebugEx.ThrowIfGlError("attaching shader");
                shaderIds.Add(shaderId);
            }

            // Link & check program
            GL.LinkProgram(this.id);
            GL.GetProgram(this.id, GetProgramParameterName.LinkStatus, out var linkStatus);
            if (linkStatus != (int)OpenTK.Graphics.OpenGL.Boolean.True)
            {
                throw new ArgumentException("Program linking failed: " + GL.GetProgramInfoLog(this.id), nameof(shaderSpecs));
            }

            // Detach and delete shaders
            foreach (var shaderId in shaderIds)
            {
                GL.DetachShader(this.id, shaderId); // Line not in superbible?
                DebugEx.ThrowIfGlError("detaching shader");
                GL.DeleteShader(shaderId);
                DebugEx.ThrowIfGlError("deleting shader");
            }

            // Get uniform IDs
            this.setDefaultUniformBlock = MakeDefaultUniformBlockSetter<TDefaultUniformBlock>();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="GlProgramWithDUB{TDefaultUniformBlock}"/> class.
        /// </summary>
        ~GlProgramWithDUB() => Dispose(false);

        /// <summary>
        /// Installs the program as part of the current rendering state and sets the current uniform values (using the default uniform block).
        /// </summary>
        /// <param name="uniforms">The uniform values.</param>
        public void UseWithUniformValues(TDefaultUniformBlock uniforms)
        {
            GL.UseProgram(this.id);
            DebugEx.ThrowIfGlError("using program");
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

            ////if (GraphicsContext.CurrentContext != null)
            {
                GL.DeleteProgram(this.id);
                DebugEx.ThrowIfGlError("deleting program");
            }
        }

        /// <remarks>
        /// Better than reflection every time, but probably less efficient than it could be due to clunky assembly of expressions.
        /// A prime candidate for replacement by source generation.
        /// </remarks>
        private Action<T> MakeDefaultUniformBlockSetter<T>()
            where T : struct
        {
            var inputParam = Expression.Parameter(typeof(T));
            var setters = new List<Expression>();

            var publicFields = typeof(T).GetFields();
            DebugEx.WriteLine($"{typeof(T).FullName} public fields that will be mapped to uniforms by name: {string.Join(", ", publicFields.Select(f => f.Name))}");
            foreach (var field in publicFields)
            {
                var uniformLocation = GL.GetUniformLocation(this.id, field.Name);
                if (uniformLocation == -1)
                {
                    throw new ArgumentException($"Uniform struct contains field '{field.Name}', which does not exist as a uniform in this program.");
                }

                if (!DefaultBlockUniformSettersByType.TryGetValue(field.FieldType, out var uniformSetter))
                {
                    throw new ArgumentException($"Uniform struct contains field of unsupported type {field.FieldType}", nameof(T));
                }

                setters.Add(Expression.Invoke(
                    uniformSetter,
                    Expression.Constant(uniformLocation),
                    Expression.Field(inputParam, field)));
            }

            return Expression.Lambda<Action<T>>(Expression.Block(setters), inputParam).Compile();
        }
    }
}
#pragma warning restore SA1402