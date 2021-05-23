using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

namespace MyOTKE.Core
{
    /// <summary>
    /// Utility methods for marshalling data between .NET and Open GL.
    /// </summary>
    internal static class GlMarshal
    {
        // TODO: These as lambdas is probably less efficient than it could be. Would be nice to be able to use just Call()..
        // Then again, the fact that some args need to be transformed first..
        private static readonly Dictionary<Type, Expression> DefaultBlockUniformSettersByType = new Dictionary<Type, Expression>
        {
            [typeof(Matrix4)] = (Expression<Action<int, Matrix4>>)((i, m) => GL.UniformMatrix4(i, false, ref m)),
            [typeof(Vector3)] = (Expression<Action<int, Vector3>>)((i, v) => GL.Uniform3(i, v)),
            [typeof(float)] = (Expression<Action<int, float>>)((i, f) => GL.Uniform1(i, f)),
            [typeof(int)] = (Expression<Action<int, int>>)((i, iv) => GL.Uniform1(i, iv)),
            [typeof(uint)] = (Expression<Action<int, uint>>)((i, u) => GL.Uniform1(i, u)),
            [typeof(long)] = (Expression<Action<int, long>>)((i, l) => GL.Uniform1(i, l)),
        };

        /// <summary>
        /// Constructs (via Linq expressions) a delegate for setting the default uniform block of a given program from an equivalent .NET struct.
        /// Struct fields and uniforms are matched by name - which must match exactly.
        /// </summary>
        /// <typeparam name="T">The type of .NET struct that is the equivalent of the default uniform block.</typeparam>
        /// <param name="programId">The name of the Open GL program.</param>
        /// <returns>A delegate to invoke to set the default uniform block.</returns>
        /// <remarks>
        /// NB: Much better than reflection every time, but probably less efficient than it could be due to clunky assembly of expressions.
        /// A prime candidate for replacement by source generation.
        /// </remarks>
        public static Action<T> MakeDefaultUniformBlockSetter<T>(int programId)
            where T : struct
        {
            var inputParam = Expression.Parameter(typeof(T));
            var setters = new List<Expression>();

            var publicFields = typeof(T).GetFields();
            GlDebug.WriteLine($"{typeof(T).FullName} public fields that will be mapped to uniforms by name: {string.Join(", ", publicFields.Select(f => f.Name))}");
            foreach (var field in publicFields)
            {
                var uniformLocation = GL.GetUniformLocation(programId, field.Name);
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

            // TODO: Check for any uniforms that we missed, and either throw or debug warn..
            ////GL.GetProgram(programId, GetProgramParameterName.ActiveUniforms, ..)

            return Expression.Lambda<Action<T>>(Expression.Block(setters), inputParam).Compile();
        }

        /// <summary>
        /// Constructs (via Linq expressions) a delegate for setting a uniform block of a given program from an equivalent .NET struct.
        /// Struct fields and uniforms are matched by name - which must match exactly.
        /// </summary>
        /// <typeparam name="T">The type of .NET struct that is the equivalent of the uniform block.</typeparam>
        /// <param name="programId">The name of the Open GL program.</param>
        /// <param name="blockName">The name of the uniform block.</param>
        /// <returns>A delegate to invoke to set the uniform block. In order, the parameters are buffer ID, index within the buffer (in multiples of the block - NOT byte offset), and the value to set.</returns>
        public static Action<int, int, T> MakeBufferObjectUniformSetter<T>(int programId, string blockName)
            where T : struct
        {
            var publicFields = typeof(T).GetFields();
            GlDebug.WriteLine($"{typeof(T).FullName} public fields that will be mapped to uniforms by name: {string.Join(", ", publicFields.Select(f => f.Name))}");

            var uniformNames = publicFields.Select(fi => $"{blockName}.{fi.Name}").ToArray();
            var uniformIndices = new int[uniformNames.Length];
            GL.GetUniformIndices(programId, uniformNames.Length, uniformNames, uniformIndices);
            GlDebug.ThrowIfGlError("getting uniform indices");
            if (uniformIndices.Any(i => i == (int)All.InvalidIndex))
            {
                // TODO: Actually say which ones weren't found
                throw new ArgumentException($"At least one of {string.Join(", ", publicFields.Select(fi => $"{blockName}.{fi.Name}"))} could not be found in the program");
            }

            //// TODO: check for any extras?

            var uniformOffsets = new int[uniformNames.Length];
            GL.GetActiveUniforms(programId, uniformIndices.Length, uniformIndices, ActiveUniformParameter.UniformOffset, uniformOffsets);
            var arrayStrides = new int[uniformNames.Length];
            GL.GetActiveUniforms(programId, uniformIndices.Length, uniformIndices, ActiveUniformParameter.UniformArrayStride, arrayStrides);
            var matrixStrides = new int[uniformNames.Length];
            GL.GetActiveUniforms(programId, uniformIndices.Length, uniformIndices, ActiveUniformParameter.UniformMatrixStride, matrixStrides);

            var pointerParam = Expression.Parameter(typeof(IntPtr));
            var blockParam = Expression.Parameter(typeof(T));
            var blockExpressions = new List<Expression>();
            for (int i = 0; i < publicFields.Length; i++)
            {
                //// TODO: the below is the default for non-array, non-matrix types (but should work when stride is zero?)
                //// if an array or matrix type, need to copy it in pieces, paying attention to strides.. How to tell..
                var structureToPtrMethod = typeof(Marshal)
                    .GetMethod(nameof(Marshal.StructureToPtr), 1, new[] { Type.MakeGenericMethodParameter(0), typeof(IntPtr), typeof(bool) })
                    .MakeGenericMethod(publicFields[i].FieldType);

                blockExpressions.Add(Expression.Call(
                    structureToPtrMethod,
                    Expression.Field(blockParam, publicFields[i]),
                    Expression.Add(pointerParam, Expression.Constant(uniformOffsets[i])),
                    Expression.Constant(false)));
            }

            var setFields = Expression.Lambda<Action<IntPtr, T>>(Expression.Block(blockExpressions), pointerParam, blockParam).Compile();

            void Set(int bufferId, int index, T param)
            {
                GL.BindBuffer(BufferTarget.UniformBuffer, bufferId);
                GlDebug.ThrowIfGlError("binding buffer");
                var bufferPtr = GL.MapBuffer(BufferTarget.UniformBuffer, BufferAccess.WriteOnly);
                GlDebug.ThrowIfGlError("mapping buffer");
                setFields(bufferPtr, param);
                GL.UnmapBuffer(BufferTarget.UniformBuffer);
                GlDebug.ThrowIfGlError("unmapping buffer");
            }

            return Set;
        }
    }
}
