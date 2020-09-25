#pragma warning disable SA1402
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
        /// Adds a shader to be included in the built program, reading the source from a <see cref="Stream"/> object.
        /// </summary>
        /// <param name="shaderType">The type of shader to be added.</param>
        /// <param name="sourceStream">The stream containing the source of the shader (in UTF-8).</param>
        /// <returns>The updated builder.</returns>
        public GlProgramBuilder WithShaderFromStream(ShaderType shaderType, Stream sourceStream)
        {
            using (var reader = new StreamReader(sourceStream))
            {
                shaderSpecs.Add((shaderType, reader.ReadToEnd()));
            }

            return this;
        }

        /// <summary>
        /// Adds a shader to be included in the built program, reading the source from a file.
        /// </summary>
        /// <param name="shaderType">The type of shader to be added.</param>
        /// <param name="filePath">The path of the file containing the source of the shader (in UTF-8).</param>
        /// <returns>The updated builder.</returns>
        public GlProgramBuilder WithShaderFromFile(ShaderType shaderType, string filePath)
        {
            DebugEx.WriteLine($"Loading {shaderType} shader from file path '{filePath}'");
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                return WithShaderFromStream(shaderType, stream);
            }
        }

        /// <summary>
        /// Adds a shader to be included in the built program, reading the source from a resource embedded in the calling assembly.
        /// </summary>
        /// <param name="shaderType">The type of shader to be added.</param>
        /// <param name="resourceName">The name of the resource containing the source of the shader (in UTF-8).</param>
        /// <returns>The updated builder.</returns>
        public GlProgramBuilder WithShaderFromEmbeddedResource(ShaderType shaderType, string resourceName)
        {
            var assembly = Assembly.GetCallingAssembly();
            DebugEx.WriteLine($"Loading {shaderType} shader from resource '{resourceName}' embedded in assembly '{assembly.GetName().Name}'");
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new ArgumentException($"Resource '{resourceName}' not found");
                }

                return WithShaderFromStream(shaderType, stream);
            }
        }

        /// <summary>
        /// Registers the set of uniforms required by the program.
        /// </summary>
        /// <typeparam name="T">The type of the container used for default block uniforms.</typeparam>
        /// <returns>The updated builder.</returns>
        public GlProgramBuilder<T> WithDefaultUniformBlock<T>()
            where T : struct
        {
            return new GlProgramBuilder<T>(shaderSpecs);
        }

        /// <summary>
        /// Builds a new <see cref="GlProgram"/> instance based on the state of the builder.
        /// </summary>
        /// <returns>The built program.</returns>
        public GlProgram Build()
        {
            return new GlProgram(shaderSpecs);
        }
    }

    /// <summary>
    /// Builder class for <see cref="GlProgram{T}"/> objects that presents a fluent-ish interface.
    /// </summary>
    /// <typeparam name="TDefaultUniformBlock">The type of the container used for default block uniforms.</typeparam>
    public class GlProgramBuilder<TDefaultUniformBlock>
        where TDefaultUniformBlock : struct
    {
        private readonly List<(ShaderType Type, string Source)> shaderSpecs = new List<(ShaderType, string)>();

        /// <summary>
        /// Initializes a new instance of the <see cref="GlProgramBuilder{TDefaultUniformBlock}"/> class.
        /// </summary>
        /// <param name="shaderSpecs">Initial specifications for each shader in the program.</param>
        internal GlProgramBuilder(List<(ShaderType Type, string Source)> shaderSpecs)
        {
            this.shaderSpecs = shaderSpecs;
        }

        /// <summary>
        /// Adds a shader to be included in the built program, reading the source from a <see cref="Stream"/> object.
        /// </summary>
        /// <param name="shaderType">The type of shader to be added.</param>
        /// <param name="sourceStream">The stream containing the source of the shader (in UTF-8).</param>
        /// <returns>The updated builder.</returns>
        public GlProgramBuilder<TDefaultUniformBlock> WithShaderFromStream(ShaderType shaderType, Stream sourceStream)
        {
            using (var reader = new StreamReader(sourceStream))
            {
                shaderSpecs.Add((shaderType, reader.ReadToEnd()));
            }

            return this;
        }

        /// <summary>
        /// Adds a shader to be included in the built program, reading the source from a file.
        /// </summary>
        /// <param name="shaderType">The type of shader to be added.</param>
        /// <param name="filePath">The path of the file containing the source of the shader (in UTF-8).</param>
        /// <returns>The updated builder.</returns>
        public GlProgramBuilder<TDefaultUniformBlock> WithShaderFromFile(ShaderType shaderType, string filePath)
        {
            DebugEx.WriteLine($"Loading {shaderType} shader from file path '{filePath}'");
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                return WithShaderFromStream(shaderType, stream);
            }
        }

        /// <summary>
        /// Adds a shader to be included in the built program, reading the source from a resource embedded in the calling assembly.
        /// </summary>
        /// <param name="shaderType">The type of shader to be added.</param>
        /// <param name="resourceName">The name of the resource containing the source of the shader (in UTF-8).</param>
        /// <returns>The updated builder.</returns>
        public GlProgramBuilder<TDefaultUniformBlock> WithShaderFromEmbeddedResource(ShaderType shaderType, string resourceName)
        {
            var assembly = Assembly.GetCallingAssembly();
            DebugEx.WriteLine($"Loading {shaderType} shader from resource '{resourceName}' embedded in assembly '{assembly.GetName().Name}'");
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new ArgumentException($"Resource '{resourceName}' not found");
                }

                return WithShaderFromStream(shaderType, stream);
            }
        }

        /// <summary>
        /// Builds a new <see cref="GlProgram"/> instance based on the state of the builder.
        /// </summary>
        /// <returns>The built program.</returns>
        public GlProgram<TDefaultUniformBlock> Build()
        {
            return new GlProgram<TDefaultUniformBlock>(shaderSpecs);
        }
    }
}
#pragma warning restore SA1402