﻿#pragma warning disable SA1402
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace MyOTKE.Core
{
    /// <summary>
    /// Builder class for <see cref="GlProgram"/> objects that presents a fluent-ish interface.
    /// </summary>
    public sealed class GlProgramBuilder
    {
        private readonly List<(ShaderType Type, string Source)> shaderSpecs = new List<(ShaderType, string)>();

        /// <summary>
        /// Adds a vertex shader to be included in the built program, reading the source from a <see cref="Stream"/> object.
        /// </summary>
        /// <param name="sourceStream">The stream containing the source of the shader (in UTF-8).</param>
        /// <returns>The updated builder.</returns>
        public GlProgramBuilder WithVertexShaderFromStream(Stream sourceStream)
        {
            return WithShaderFromStream(ShaderType.VertexShader, sourceStream);
        }

        /// <summary>
        /// Adds a fragment shader to be included in the built program, reading the source from a <see cref="Stream"/> object.
        /// </summary>
        /// <param name="sourceStream">The stream containing the source of the shader (in UTF-8).</param>
        /// <returns>The updated builder.</returns>
        public GlProgramBuilder WithFragmentShaderFromStream(Stream sourceStream)
        {
            return WithShaderFromStream(ShaderType.FragmentShader, sourceStream);
        }

        /// <summary>
        /// Adds a vertex shader to be included in the built program, reading the source from a file.
        /// </summary>
        /// <param name="filePath">The path of the file containing the source of the shader (in UTF-8).</param>
        /// <returns>The updated builder.</returns>
        public GlProgramBuilder WithVertexShaderFromFile(string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                return WithShaderFromStream(ShaderType.VertexShader, stream);
            }
        }

        /// <summary>
        /// Adds a fragment shader to be included in the built program, reading the source from a file.
        /// </summary>
        /// <param name="filePath">The path of the file containing the source of the shader (in UTF-8).</param>
        /// <returns>The updated builder.</returns>
        public GlProgramBuilder WithFragmentShaderFromFile(string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                return WithShaderFromStream(ShaderType.FragmentShader, stream);
            }
        }

        /// <summary>
        /// Adds a vertex shader to be included in the built program, reading the source from a resource embedded in the calling assembly.
        /// </summary>
        /// <param name="resourceName">The name of the resource containing the source of the shader (in UTF-8).</param>
        /// <returns>The updated builder.</returns>
        public GlProgramBuilder WithVertexShaderFromEmbeddedResource(string resourceName)
        {
            var assembly = Assembly.GetCallingAssembly();
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new ArgumentException($"Resource '{resourceName}' not found");
                }

                return WithShaderFromStream(ShaderType.VertexShader, stream);
            }
        }

        /// <summary>
        /// Adds a fragment shader to be included in the built program, reading the source from a resource embedded in the calling assembly.
        /// </summary>
        /// <param name="resourceName">The name of the resource containing the source of the shader (in UTF-8).</param>
        /// <returns>The updated builder.</returns>
        public GlProgramBuilder WithFragmentShaderFromEmbeddedResource(string resourceName)
        {
            var assembly = Assembly.GetCallingAssembly();
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new ArgumentException($"Resource '{resourceName}' not found");
                }

                return WithShaderFromStream(ShaderType.FragmentShader, stream);
            }
        }

        /// <summary>
        /// Registers the set of uniforms required by the program.
        /// </summary>
        /// <typeparam name="T">The type of the container used for default block uniforms.</typeparam>
        /// <returns>The updated builder.</returns>
        public GlProgramWithDUBBuilder<T> WithDefaultUniformBlock<T>()
            where T : struct
        {
            return new GlProgramWithDUBBuilder<T>(shaderSpecs);
        }

        /// <summary>
        /// Builds a new <see cref="GlProgram"/> instance based on the state of the builder.
        /// </summary>
        /// <returns>The built program.</returns>
        public GlProgram Build()
        {
            return new GlProgram(shaderSpecs);
        }

        private GlProgramBuilder WithShaderFromStream(ShaderType shaderType, Stream sourceStream)
        {
            using (var reader = new StreamReader(sourceStream))
            {
                shaderSpecs.Add((shaderType, reader.ReadToEnd()));
            }

            return this;
        }
    }

    /// <summary>
    /// Builder class for <see cref="GlProgramWithDUB{T}"/> objects that presents a fluent-ish interface.
    /// </summary>
    /// <typeparam name="TDefaultUniformBlock">The type of the container used for default block uniforms.</typeparam>
    public class GlProgramWithDUBBuilder<TDefaultUniformBlock>
        where TDefaultUniformBlock : struct
    {
        private readonly List<(ShaderType Type, string Source)> shaderSpecs = new List<(ShaderType, string)>();

        /// <summary>
        /// Initializes a new instance of the <see cref="GlProgramWithDUBBuilder{TDefaultUniformBlock}"/> class.
        /// </summary>
        /// <param name="shaderSpecs">Initial specifications for each shader in the program.</param>
        internal GlProgramWithDUBBuilder(List<(ShaderType Type, string Source)> shaderSpecs)
        {
            this.shaderSpecs = shaderSpecs;
        }

        /// <summary>
        /// Adds a vertex shader to be included in the built program, reading the source from a file.
        /// </summary>
        /// <param name="filePath">The path of the file containing the source of the shader (in UTF-8).</param>
        /// <returns>The updated builder.</returns>
        public GlProgramWithDUBBuilder<TDefaultUniformBlock> WithVertexShaderFromFile(string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                return WithShaderFromStream(ShaderType.VertexShader, stream);
            }
        }

        /// <summary>
        /// Adds a fragment shader to be included in the built program, reading the source from a file.
        /// </summary>
        /// <param name="filePath">The path of the file containing the source of the shader (in UTF-8).</param>
        /// <returns>The updated builder.</returns>
        public GlProgramWithDUBBuilder<TDefaultUniformBlock> WithFragmentShaderFromFile(string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                return WithShaderFromStream(ShaderType.FragmentShader, stream);
            }
        }

        /// <summary>
        /// Adds a vertex shader to be included in the built program, reading the source from a resource embedded in the calling assembly.
        /// </summary>
        /// <param name="resourceName">The name of the resource containing the source of the shader (in UTF-8).</param>
        /// <returns>The updated builder.</returns>
        public GlProgramWithDUBBuilder<TDefaultUniformBlock> WithVertexShaderFromEmbeddedResource(string resourceName)
        {
            var assembly = Assembly.GetCallingAssembly();
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new ArgumentException($"Resource '{resourceName}' not found");
                }

                return WithShaderFromStream(ShaderType.VertexShader, stream);
            }
        }

        /// <summary>
        /// Adds a fragment shader to be included in the built program, reading the source from a resource embedded in the calling assembly.
        /// </summary>
        /// <param name="resourceName">The name of the resource containing the source of the shader (in UTF-8).</param>
        /// <returns>The updated builder.</returns>
        public GlProgramWithDUBBuilder<TDefaultUniformBlock> WithFragmentShaderFromEmbeddedResource(string resourceName)
        {
            var assembly = Assembly.GetCallingAssembly();
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new ArgumentException($"Resource '{resourceName}' not found");
                }

                return WithShaderFromStream(ShaderType.FragmentShader, stream);
            }
        }

        /// <summary>
        /// Builds a new <see cref="GlProgram"/> instance based on the state of the builder.
        /// </summary>
        /// <returns>The built program.</returns>
        public GlProgramWithDUB<TDefaultUniformBlock> Build()
        {
            return new GlProgramWithDUB<TDefaultUniformBlock>(shaderSpecs);
        }

        private GlProgramWithDUBBuilder<TDefaultUniformBlock> WithShaderFromStream(ShaderType shaderType, Stream sourceStream)
        {
            using (var reader = new StreamReader(sourceStream))
            {
                shaderSpecs.Add((shaderType, reader.ReadToEnd()));
            }

            return this;
        }
    }
}
#pragma warning restore SA1402