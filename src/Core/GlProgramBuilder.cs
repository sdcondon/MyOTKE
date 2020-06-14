﻿using OpenTK.Graphics.OpenGL;
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
        private string[] uniformNames;

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
            GlExt.DebugWriteLine($"Loading {shaderType} shader from file path '{filePath}'");
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
            GlExt.DebugWriteLine($"Loading {shaderType} shader from resource '{resourceName}' embedded in assembly '{assembly.GetName().Name}'");
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
        /// <param name="uniformNames">The names of the uniforms, in the order that they will be provided when calling <see cref="GlProgram.UseWithUniformValues(object[])"/>.</param>
        /// <returns>The updated builder.</returns>
        /// <remarks>
        /// TODO: Better to use a generic type approach for compile-time safety.
        /// </remarks>
        public GlProgramBuilder WithUniforms(params string[] uniformNames)
        {
            this.uniformNames = uniformNames;
            return this;
        }

        /// <summary>
        /// Builds a new <see cref="GlProgram"/> instance based on the state of the builder.
        /// </summary>
        /// <returns>The built program.</returns>
        public GlProgram Build()
        {
            return new GlProgram(shaderSpecs, uniformNames);
        }
    }
}
