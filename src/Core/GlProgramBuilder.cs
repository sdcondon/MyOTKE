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
        private readonly List<(ShaderType Type, string Source)> shaderSpecs = [];

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
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return WithShaderFromStream(ShaderType.VertexShader, stream);
        }

        /// <summary>
        /// Adds a fragment shader to be included in the built program, reading the source from a file.
        /// </summary>
        /// <param name="filePath">The path of the file containing the source of the shader (in UTF-8).</param>
        /// <returns>The updated builder.</returns>
        public GlProgramBuilder WithFragmentShaderFromFile(string filePath)
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return WithShaderFromStream(ShaderType.FragmentShader, stream);
        }

        /// <summary>
        /// Adds a vertex shader to be included in the built program, reading the source from a resource embedded in the calling assembly.
        /// </summary>
        /// <param name="resourceName">The name of the resource containing the source of the shader (in UTF-8).</param>
        /// <returns>The updated builder.</returns>
        public GlProgramBuilder WithVertexShaderFromEmbeddedResource(string resourceName)
        {
            var assembly = Assembly.GetCallingAssembly();
            using var stream = assembly.GetManifestResourceStream(resourceName) ?? throw new ArgumentException($"Resource '{resourceName}' not found");

            return WithShaderFromStream(ShaderType.VertexShader, stream);
        }

        /// <summary>
        /// Adds a fragment shader to be included in the built program, reading the source from a resource embedded in the calling assembly.
        /// </summary>
        /// <param name="resourceName">The name of the resource containing the source of the shader (in UTF-8).</param>
        /// <returns>The updated builder.</returns>
        public GlProgramBuilder WithFragmentShaderFromEmbeddedResource(string resourceName)
        {
            var assembly = Assembly.GetCallingAssembly();
            using var stream = assembly.GetManifestResourceStream(resourceName) ?? throw new ArgumentException($"Resource '{resourceName}' not found");

            return WithShaderFromStream(ShaderType.FragmentShader, stream);
        }

        /// <summary>
        /// Registers the set of uniforms required by the program.
        /// </summary>
        /// <typeparam name="TDefaultUniformBlock">The type of the container used for default block uniforms.</typeparam>
        /// <returns>The updated builder.</returns>
        public GlProgramWithDUBBuilder<TDefaultUniformBlock> WithDefaultUniformBlock<TDefaultUniformBlock>()
            where TDefaultUniformBlock : struct
        {
            return new GlProgramWithDUBBuilder<TDefaultUniformBlock>(
                shaderSpecs);
        }

        /// <summary>
        /// Registers a uniform buffer object used by the program - shared with other programs via block name.
        /// </summary>
        /// <typeparam name="T1">The type of the container used for block content.</typeparam>
        /// <returns>The updated builder.</returns>
        /// <remarks>
        /// NB: No checks carried out that the data stored in the buffer object is of the same format as other programs using it. There probably should be..
        /// </remarks>
        public GlProgramBuilder<T1> WithSharedUniformBufferObject<T1>(string blockName, BufferUsageHint usage, T1[] data)
            where T1 : struct
        {
            return new GlProgramBuilder<T1>(
                (blockName, usage, data.Length, data),
                shaderSpecs);
        }

        /// <summary>
        /// Registers a uniform buffer object used by the program - shared with other programs via block name.
        /// </summary>
        /// <typeparam name="T1">The type of the container used for block content.</typeparam>
        /// <returns>The updated builder.</returns>
        /// <remarks>
        /// NB: No checks carried out that the data stored in the buffer object is of the same format as other programs using it. There probably should be..
        /// </remarks>
        public GlProgramBuilder<T1> WithSharedUniformBufferObject<T1>(string blockName, BufferUsageHint usage, int capacity)
            where T1 : struct
        {
            return new GlProgramBuilder<T1>(
                (blockName, usage, capacity, null),
                shaderSpecs);
        }

        /// <summary>
        /// Builds a new <see cref="GlProgram"/> instance based on the state of the builder.
        /// </summary>
        /// <returns>The built program.</returns>
        public GlProgram Build()
        {
            return new GlProgram(
                shaderSpecs);
        }

        private GlProgramBuilder WithShaderFromStream(ShaderType shaderType, Stream sourceStream)
        {
            using var reader = new StreamReader(sourceStream);
            shaderSpecs.Add((shaderType, reader.ReadToEnd()));

            return this;
        }
    }

    /// <summary>
    /// Builder class for <see cref="GlProgram{T1}"/> objects that presents a fluent-ish interface.
    /// </summary>
    /// <typeparam name="T1">The type of the 1st uniform buffer object.</typeparam>
    public sealed class GlProgramBuilder<T1>
        where T1 : struct
    {
        private readonly (string BlockName, BufferUsageHint Usage, int Capacity, T1[] Data) uboSpec1;
        private readonly List<(ShaderType Type, string Source)> shaderSpecs = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="GlProgramBuilder{T1}"/> class.
        /// </summary>
        /// <param name="uboSpec1">Specification for the 1st uniform buffer object.</param>
        /// <param name="shaderSpecs">Specifications for each of the shaders used by this program.</param>
        internal GlProgramBuilder(
            (string BlockName, BufferUsageHint Usage, int Capacity, T1[] Data) uboSpec1,
            List<(ShaderType Type, string Source)> shaderSpecs)
        {
            this.uboSpec1 = uboSpec1;
            this.shaderSpecs.AddRange(shaderSpecs);
        }

        /// <summary>
        /// Adds a vertex shader to be included in the built program, reading the source from a <see cref="Stream"/> object.
        /// </summary>
        /// <param name="sourceStream">The stream containing the source of the shader (in UTF-8).</param>
        /// <returns>The updated builder.</returns>
        public GlProgramBuilder<T1> WithVertexShaderFromStream(Stream sourceStream)
        {
            return WithShaderFromStream(ShaderType.VertexShader, sourceStream);
        }

        /// <summary>
        /// Adds a fragment shader to be included in the built program, reading the source from a <see cref="Stream"/> object.
        /// </summary>
        /// <param name="sourceStream">The stream containing the source of the shader (in UTF-8).</param>
        /// <returns>The updated builder.</returns>
        public GlProgramBuilder<T1> WithFragmentShaderFromStream(Stream sourceStream)
        {
            return WithShaderFromStream(ShaderType.FragmentShader, sourceStream);
        }

        /// <summary>
        /// Adds a vertex shader to be included in the built program, reading the source from a file.
        /// </summary>
        /// <param name="filePath">The path of the file containing the source of the shader (in UTF-8).</param>
        /// <returns>The updated builder.</returns>
        public GlProgramBuilder<T1> WithVertexShaderFromFile(string filePath)
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return WithShaderFromStream(ShaderType.VertexShader, stream);
        }

        /// <summary>
        /// Adds a fragment shader to be included in the built program, reading the source from a file.
        /// </summary>
        /// <param name="filePath">The path of the file containing the source of the shader (in UTF-8).</param>
        /// <returns>The updated builder.</returns>
        public GlProgramBuilder<T1> WithFragmentShaderFromFile(string filePath)
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return WithShaderFromStream(ShaderType.FragmentShader, stream);
        }

        /// <summary>
        /// Adds a vertex shader to be included in the built program, reading the source from a resource embedded in the calling assembly.
        /// </summary>
        /// <param name="resourceName">The name of the resource containing the source of the shader (in UTF-8).</param>
        /// <returns>The updated builder.</returns>
        public GlProgramBuilder<T1> WithVertexShaderFromEmbeddedResource(string resourceName)
        {
            var assembly = Assembly.GetCallingAssembly();
            using var stream = assembly.GetManifestResourceStream(resourceName) ?? throw new ArgumentException($"Resource '{resourceName}' not found");

            return WithShaderFromStream(ShaderType.VertexShader, stream);
        }

        /// <summary>
        /// Adds a fragment shader to be included in the built program, reading the source from a resource embedded in the calling assembly.
        /// </summary>
        /// <param name="resourceName">The name of the resource containing the source of the shader (in UTF-8).</param>
        /// <returns>The updated builder.</returns>
        public GlProgramBuilder<T1> WithFragmentShaderFromEmbeddedResource(string resourceName)
        {
            var assembly = Assembly.GetCallingAssembly();
            using var stream = assembly.GetManifestResourceStream(resourceName) ?? throw new ArgumentException($"Resource '{resourceName}' not found");

            return WithShaderFromStream(ShaderType.FragmentShader, stream);
        }

        /// <summary>
        /// Registers the set of uniforms required by the program.
        /// </summary>
        /// <typeparam name="TDefaultUniformBlock">The type of the container used for default block uniforms.</typeparam>
        /// <returns>The updated builder.</returns>
        public GlProgramWithDUBBuilder<TDefaultUniformBlock, T1> WithDefaultUniformBlock<TDefaultUniformBlock>()
            where TDefaultUniformBlock : struct
        {
            return new GlProgramWithDUBBuilder<TDefaultUniformBlock, T1>(
                uboSpec1,
                shaderSpecs);
        }

        /// <summary>
        /// Builds a new <see cref="GlProgram{T1}"/> instance based on the state of the builder.
        /// </summary>
        /// <returns>The built program.</returns>
        public GlProgram<T1> Build()
        {
            return new GlProgram<T1>(
                uboSpec1,
                shaderSpecs);
        }

        private GlProgramBuilder<T1> WithShaderFromStream(ShaderType shaderType, Stream sourceStream)
        {
            using var reader = new StreamReader(sourceStream);
            shaderSpecs.Add((shaderType, reader.ReadToEnd()));

            return this;
        }
    }
    /// <summary>
    /// Builder class for <see cref="GlProgramWithDUB{TDefaultUniformBlock}"/> objects that presents a fluent-ish interface.
    /// </summary>
    /// <typeparam name="TDefaultUniformBlock">The type of the container used for default block uniforms.</typeparam>
    public sealed class GlProgramWithDUBBuilder<TDefaultUniformBlock>
        where TDefaultUniformBlock : struct
    {
        private readonly List<(ShaderType Type, string Source)> shaderSpecs = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="GlProgramWithDUBBuilder{TDefaultUniformBlock}"/> class.
        /// </summary>
        /// <param name="shaderSpecs">Specifications for each of the shaders used by this program.</param>
        internal GlProgramWithDUBBuilder(
            List<(ShaderType Type, string Source)> shaderSpecs)
        {
            this.shaderSpecs.AddRange(shaderSpecs);
        }

        /// <summary>
        /// Adds a vertex shader to be included in the built program, reading the source from a <see cref="Stream"/> object.
        /// </summary>
        /// <param name="sourceStream">The stream containing the source of the shader (in UTF-8).</param>
        /// <returns>The updated builder.</returns>
        public GlProgramWithDUBBuilder<TDefaultUniformBlock> WithVertexShaderFromStream(Stream sourceStream)
        {
            return WithShaderFromStream(ShaderType.VertexShader, sourceStream);
        }

        /// <summary>
        /// Adds a fragment shader to be included in the built program, reading the source from a <see cref="Stream"/> object.
        /// </summary>
        /// <param name="sourceStream">The stream containing the source of the shader (in UTF-8).</param>
        /// <returns>The updated builder.</returns>
        public GlProgramWithDUBBuilder<TDefaultUniformBlock> WithFragmentShaderFromStream(Stream sourceStream)
        {
            return WithShaderFromStream(ShaderType.FragmentShader, sourceStream);
        }

        /// <summary>
        /// Adds a vertex shader to be included in the built program, reading the source from a file.
        /// </summary>
        /// <param name="filePath">The path of the file containing the source of the shader (in UTF-8).</param>
        /// <returns>The updated builder.</returns>
        public GlProgramWithDUBBuilder<TDefaultUniformBlock> WithVertexShaderFromFile(string filePath)
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return WithShaderFromStream(ShaderType.VertexShader, stream);
        }

        /// <summary>
        /// Adds a fragment shader to be included in the built program, reading the source from a file.
        /// </summary>
        /// <param name="filePath">The path of the file containing the source of the shader (in UTF-8).</param>
        /// <returns>The updated builder.</returns>
        public GlProgramWithDUBBuilder<TDefaultUniformBlock> WithFragmentShaderFromFile(string filePath)
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return WithShaderFromStream(ShaderType.FragmentShader, stream);
        }

        /// <summary>
        /// Adds a vertex shader to be included in the built program, reading the source from a resource embedded in the calling assembly.
        /// </summary>
        /// <param name="resourceName">The name of the resource containing the source of the shader (in UTF-8).</param>
        /// <returns>The updated builder.</returns>
        public GlProgramWithDUBBuilder<TDefaultUniformBlock> WithVertexShaderFromEmbeddedResource(string resourceName)
        {
            var assembly = Assembly.GetCallingAssembly();
            using var stream = assembly.GetManifestResourceStream(resourceName) ?? throw new ArgumentException($"Resource '{resourceName}' not found");

            return WithShaderFromStream(ShaderType.VertexShader, stream);
        }

        /// <summary>
        /// Adds a fragment shader to be included in the built program, reading the source from a resource embedded in the calling assembly.
        /// </summary>
        /// <param name="resourceName">The name of the resource containing the source of the shader (in UTF-8).</param>
        /// <returns>The updated builder.</returns>
        public GlProgramWithDUBBuilder<TDefaultUniformBlock> WithFragmentShaderFromEmbeddedResource(string resourceName)
        {
            var assembly = Assembly.GetCallingAssembly();
            using var stream = assembly.GetManifestResourceStream(resourceName) ?? throw new ArgumentException($"Resource '{resourceName}' not found");

            return WithShaderFromStream(ShaderType.FragmentShader, stream);
        }

        /// <summary>
        /// Registers a uniform buffer object used by the program - shared with other programs via block name.
        /// </summary>
        /// <typeparam name="T1">The type of the container used for block content.</typeparam>
        /// <returns>The updated builder.</returns>
        /// <remarks>
        /// NB: No checks carried out that the data stored in the buffer object is of the same format as other programs using it. There probably should be..
        /// </remarks>
        public GlProgramWithDUBBuilder<TDefaultUniformBlock, T1> WithSharedUniformBufferObject<T1>(string blockName, BufferUsageHint usage, T1[] data)
            where T1 : struct
        {
            return new GlProgramWithDUBBuilder<TDefaultUniformBlock, T1>(
                (blockName, usage, data.Length, data),
                shaderSpecs);
        }

        /// <summary>
        /// Registers a uniform buffer object used by the program - shared with other programs via block name.
        /// </summary>
        /// <typeparam name="T1">The type of the container used for block content.</typeparam>
        /// <returns>The updated builder.</returns>
        /// <remarks>
        /// NB: No checks carried out that the data stored in the buffer object is of the same format as other programs using it. There probably should be..
        /// </remarks>
        public GlProgramWithDUBBuilder<TDefaultUniformBlock, T1> WithSharedUniformBufferObject<T1>(string blockName, BufferUsageHint usage, int capacity)
            where T1 : struct
        {
            return new GlProgramWithDUBBuilder<TDefaultUniformBlock, T1>(
                (blockName, usage, capacity, null),
                shaderSpecs);
        }

        /// <summary>
        /// Builds a new <see cref="GlProgramWithDUB{TDefaultUniformBlock}"/> instance based on the state of the builder.
        /// </summary>
        /// <returns>The built program.</returns>
        public GlProgramWithDUB<TDefaultUniformBlock> Build()
        {
            return new GlProgramWithDUB<TDefaultUniformBlock>(
                shaderSpecs);
        }

        private GlProgramWithDUBBuilder<TDefaultUniformBlock> WithShaderFromStream(ShaderType shaderType, Stream sourceStream)
        {
            using var reader = new StreamReader(sourceStream);
            shaderSpecs.Add((shaderType, reader.ReadToEnd()));

            return this;
        }
    }

    /// <summary>
    /// Builder class for <see cref="GlProgramWithDUB{TDefaultUniformBlock, T1}"/> objects that presents a fluent-ish interface.
    /// </summary>
    /// <typeparam name="TDefaultUniformBlock">The type of the container used for default block uniforms.</typeparam>
    /// <typeparam name="T1">The type of the 1st uniform buffer object.</typeparam>
    public sealed class GlProgramWithDUBBuilder<TDefaultUniformBlock, T1>
        where TDefaultUniformBlock : struct
        where T1 : struct
    {
        private readonly (string BlockName, BufferUsageHint Usage, int Capacity, T1[] Data) uboSpec1;
        private readonly List<(ShaderType Type, string Source)> shaderSpecs = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="GlProgramWithDUBBuilder{TDefaultUniformBlock, T1}"/> class.
        /// </summary>
        /// <param name="uboSpec1">Specification for the 1st uniform buffer object.</param>
        /// <param name="shaderSpecs">Specifications for each of the shaders used by this program.</param>
        internal GlProgramWithDUBBuilder(
            (string BlockName, BufferUsageHint Usage, int Capacity, T1[] Data) uboSpec1,
            List<(ShaderType Type, string Source)> shaderSpecs)
        {
            this.uboSpec1 = uboSpec1;
            this.shaderSpecs.AddRange(shaderSpecs);
        }

        /// <summary>
        /// Adds a vertex shader to be included in the built program, reading the source from a <see cref="Stream"/> object.
        /// </summary>
        /// <param name="sourceStream">The stream containing the source of the shader (in UTF-8).</param>
        /// <returns>The updated builder.</returns>
        public GlProgramWithDUBBuilder<TDefaultUniformBlock, T1> WithVertexShaderFromStream(Stream sourceStream)
        {
            return WithShaderFromStream(ShaderType.VertexShader, sourceStream);
        }

        /// <summary>
        /// Adds a fragment shader to be included in the built program, reading the source from a <see cref="Stream"/> object.
        /// </summary>
        /// <param name="sourceStream">The stream containing the source of the shader (in UTF-8).</param>
        /// <returns>The updated builder.</returns>
        public GlProgramWithDUBBuilder<TDefaultUniformBlock, T1> WithFragmentShaderFromStream(Stream sourceStream)
        {
            return WithShaderFromStream(ShaderType.FragmentShader, sourceStream);
        }

        /// <summary>
        /// Adds a vertex shader to be included in the built program, reading the source from a file.
        /// </summary>
        /// <param name="filePath">The path of the file containing the source of the shader (in UTF-8).</param>
        /// <returns>The updated builder.</returns>
        public GlProgramWithDUBBuilder<TDefaultUniformBlock, T1> WithVertexShaderFromFile(string filePath)
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return WithShaderFromStream(ShaderType.VertexShader, stream);
        }

        /// <summary>
        /// Adds a fragment shader to be included in the built program, reading the source from a file.
        /// </summary>
        /// <param name="filePath">The path of the file containing the source of the shader (in UTF-8).</param>
        /// <returns>The updated builder.</returns>
        public GlProgramWithDUBBuilder<TDefaultUniformBlock, T1> WithFragmentShaderFromFile(string filePath)
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return WithShaderFromStream(ShaderType.FragmentShader, stream);
        }

        /// <summary>
        /// Adds a vertex shader to be included in the built program, reading the source from a resource embedded in the calling assembly.
        /// </summary>
        /// <param name="resourceName">The name of the resource containing the source of the shader (in UTF-8).</param>
        /// <returns>The updated builder.</returns>
        public GlProgramWithDUBBuilder<TDefaultUniformBlock, T1> WithVertexShaderFromEmbeddedResource(string resourceName)
        {
            var assembly = Assembly.GetCallingAssembly();
            using var stream = assembly.GetManifestResourceStream(resourceName) ?? throw new ArgumentException($"Resource '{resourceName}' not found");

            return WithShaderFromStream(ShaderType.VertexShader, stream);
        }

        /// <summary>
        /// Adds a fragment shader to be included in the built program, reading the source from a resource embedded in the calling assembly.
        /// </summary>
        /// <param name="resourceName">The name of the resource containing the source of the shader (in UTF-8).</param>
        /// <returns>The updated builder.</returns>
        public GlProgramWithDUBBuilder<TDefaultUniformBlock, T1> WithFragmentShaderFromEmbeddedResource(string resourceName)
        {
            var assembly = Assembly.GetCallingAssembly();
            using var stream = assembly.GetManifestResourceStream(resourceName) ?? throw new ArgumentException($"Resource '{resourceName}' not found");

            return WithShaderFromStream(ShaderType.FragmentShader, stream);
        }

        /// <summary>
        /// Builds a new <see cref="GlProgramWithDUB{TDefaultUniformBlock, T1}"/> instance based on the state of the builder.
        /// </summary>
        /// <returns>The built program.</returns>
        public GlProgramWithDUB<TDefaultUniformBlock, T1> Build()
        {
            return new GlProgramWithDUB<TDefaultUniformBlock, T1>(
                uboSpec1,
                shaderSpecs);
        }

        private GlProgramWithDUBBuilder<TDefaultUniformBlock, T1> WithShaderFromStream(ShaderType shaderType, Stream sourceStream)
        {
            using var reader = new StreamReader(sourceStream);
            shaderSpecs.Add((shaderType, reader.ReadToEnd()));

            return this;
        }
    }
}