using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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
        /// NB: Better than reflection every time, but probably less efficient than it could be due to clunky assembly of expressions.
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

            return Expression.Lambda<Action<T>>(Expression.Block(setters), inputParam).Compile();
        }
    }
}
