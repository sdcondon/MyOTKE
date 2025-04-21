namespace MyOTKE.Core
{
    /// <summary>
    /// Extension methods for IVertexArrayObject instances.
    /// </summary>
    public static class IVertexArrayObjectExtensions
    {
        /// <summary>
        /// Installs a given program as part of the current rendering state, then draws this array object.
        /// </summary>
        /// <typeparam name="TAttr1">The type of the 1st attribute buffer object of the VAO.</typeparam>
        /// <param name="vao">The VAO to use.</param>
        /// <param name="program">The program to use.</param>
        /// <param name="count">The number of vertices to draw, or -1 to draw all vertices.</param>
        public static void Draw<TAttr1>(
            this IVertexArrayObject<TAttr1> vao,
            GlProgram program,
            int count = -1)
            where TAttr1 : struct
        {
            program.Use();
            vao.Draw(count);
        }

        /// <summary>
        /// Installs a given program as part of the current rendering state, then draws this array object.
        /// </summary>
        /// <typeparam name="TAttr1">The type of the 1st attribute buffer object of the VAO.</typeparam>
        /// <typeparam name="TDefaultUniformBlock">The type of the container used for default block uniforms for the given program.</typeparam>
        /// <param name="vao">The VAO to use.</param>
        /// <param name="program">The program to use.</param>
        /// <param name="defaultUniformBlock">The values for the uniforms in the default block.</param>
        /// <param name="count">The number of vertices to draw, or -1 to draw all vertices.</param>
        public static void Draw<TAttr1, TDefaultUniformBlock>(
            this IVertexArrayObject<TAttr1> vao,
            GlProgramWithDUB<TDefaultUniformBlock> program,
            TDefaultUniformBlock defaultUniformBlock,
            int count = -1)
            where TAttr1 : struct
            where TDefaultUniformBlock : struct
        {
            program.UseWithDefaultUniformBlock(defaultUniformBlock);
            vao.Draw(count);
        }

        /// <summary>
        /// Installs a given program as part of the current rendering state, then draws this array object.
        /// </summary>
        /// <typeparam name="TAttr1">The type of the 1st attribute buffer object of the VAO.</typeparam>
        /// <typeparam name="TUbo1">The type of the 1st uniform buffer object of the program.</typeparam>
        /// <param name="vao">The VAO to use.</param>
        /// <param name="program">The program to use.</param>
        /// <param name="count">The number of vertices to draw, or -1 to draw all vertices.</param>
        public static void Draw<TAttr1, TUbo1>(
            this IVertexArrayObject<TAttr1> vao,
            GlProgram<TUbo1> program,
            int count = -1)
            where TAttr1 : struct
            where TUbo1 : struct
        {
            program.Use();
            vao.Draw(count);
        }

        /// <summary>
        /// Installs a given program as part of the current rendering state, then draws this array object.
        /// </summary>
        /// <typeparam name="TAttr1">The type of the 1st attribute buffer object of the VAO.</typeparam>
        /// <typeparam name="TDefaultUniformBlock">The type of the container used for default block uniforms for the given program.</typeparam>
        /// <typeparam name="TUbo1">The type of the 1st uniform buffer object of the program.</typeparam>
        /// <param name="vao">The VAO to use.</param>
        /// <param name="program">The program to use.</param>
        /// <param name="defaultUniformBlock">The values for the uniforms in the default block.</param>
        /// <param name="count">The number of vertices to draw, or -1 to draw all vertices.</param>
        public static void Draw<TAttr1, TDefaultUniformBlock, TUbo1>(
            this IVertexArrayObject<TAttr1> vao,
            GlProgramWithDUB<TDefaultUniformBlock, TUbo1> program,
            TDefaultUniformBlock defaultUniformBlock,
            int count = -1)
            where TAttr1 : struct
            where TDefaultUniformBlock : struct
            where TUbo1 : struct
        {
            program.UseWithDefaultUniformBlock(defaultUniformBlock);
            vao.Draw(count);
        }

        /// <summary>
        /// Installs a given program as part of the current rendering state, then draws this array object.
        /// </summary>
        /// <typeparam name="TAttr1">The type of the 1st attribute buffer object of the VAO.</typeparam>
        /// <typeparam name="TAttr2">The type of the 2nd attribute buffer object of the VAO.</typeparam>
        /// <param name="vao">The VAO to use.</param>
        /// <param name="program">The program to use.</param>
        /// <param name="count">The number of vertices to draw, or -1 to draw all vertices.</param>
        public static void Draw<TAttr1, TAttr2>(
            this IVertexArrayObject<TAttr1, TAttr2> vao,
            GlProgram program,
            int count = -1)
            where TAttr1 : struct
            where TAttr2 : struct
        {
            program.Use();
            vao.Draw(count);
        }

        /// <summary>
        /// Installs a given program as part of the current rendering state, then draws this array object.
        /// </summary>
        /// <typeparam name="TAttr1">The type of the 1st attribute buffer object of the VAO.</typeparam>
        /// <typeparam name="TAttr2">The type of the 2nd attribute buffer object of the VAO.</typeparam>
        /// <typeparam name="TDefaultUniformBlock">The type of the container used for default block uniforms for the given program.</typeparam>
        /// <param name="vao">The VAO to use.</param>
        /// <param name="program">The program to use.</param>
        /// <param name="defaultUniformBlock">The values for the uniforms in the default block.</param>
        /// <param name="count">The number of vertices to draw, or -1 to draw all vertices.</param>
        public static void Draw<TAttr1, TAttr2, TDefaultUniformBlock>(
            this IVertexArrayObject<TAttr1, TAttr2> vao,
            GlProgramWithDUB<TDefaultUniformBlock> program,
            TDefaultUniformBlock defaultUniformBlock,
            int count = -1)
            where TAttr1 : struct
            where TAttr2 : struct
            where TDefaultUniformBlock : struct
        {
            program.UseWithDefaultUniformBlock(defaultUniformBlock);
            vao.Draw(count);
        }

        /// <summary>
        /// Installs a given program as part of the current rendering state, then draws this array object.
        /// </summary>
        /// <typeparam name="TAttr1">The type of the 1st attribute buffer object of the VAO.</typeparam>
        /// <typeparam name="TAttr2">The type of the 2nd attribute buffer object of the VAO.</typeparam>
        /// <typeparam name="TUbo1">The type of the 1st uniform buffer object of the program.</typeparam>
        /// <param name="vao">The VAO to use.</param>
        /// <param name="program">The program to use.</param>
        /// <param name="count">The number of vertices to draw, or -1 to draw all vertices.</param>
        public static void Draw<TAttr1, TAttr2, TUbo1>(
            this IVertexArrayObject<TAttr1, TAttr2> vao,
            GlProgram<TUbo1> program,
            int count = -1)
            where TAttr1 : struct
            where TAttr2 : struct
            where TUbo1 : struct
        {
            program.Use();
            vao.Draw(count);
        }

        /// <summary>
        /// Installs a given program as part of the current rendering state, then draws this array object.
        /// </summary>
        /// <typeparam name="TAttr1">The type of the 1st attribute buffer object of the VAO.</typeparam>
        /// <typeparam name="TAttr2">The type of the 2nd attribute buffer object of the VAO.</typeparam>
        /// <typeparam name="TDefaultUniformBlock">The type of the container used for default block uniforms for the given program.</typeparam>
        /// <typeparam name="TUbo1">The type of the 1st uniform buffer object of the program.</typeparam>
        /// <param name="vao">The VAO to use.</param>
        /// <param name="program">The program to use.</param>
        /// <param name="defaultUniformBlock">The values for the uniforms in the default block.</param>
        /// <param name="count">The number of vertices to draw, or -1 to draw all vertices.</param>
        public static void Draw<TAttr1, TAttr2, TDefaultUniformBlock, TUbo1>(
            this IVertexArrayObject<TAttr1, TAttr2> vao,
            GlProgramWithDUB<TDefaultUniformBlock, TUbo1> program,
            TDefaultUniformBlock defaultUniformBlock,
            int count = -1)
            where TAttr1 : struct
            where TAttr2 : struct
            where TDefaultUniformBlock : struct
            where TUbo1 : struct
        {
            program.UseWithDefaultUniformBlock(defaultUniformBlock);
            vao.Draw(count);
        }

        /// <summary>
        /// Installs a given program as part of the current rendering state, then draws this array object.
        /// </summary>
        /// <typeparam name="TAttr1">The type of the 1st attribute buffer object of the VAO.</typeparam>
        /// <typeparam name="TAttr2">The type of the 2nd attribute buffer object of the VAO.</typeparam>
        /// <typeparam name="TAttr3">The type of the 3rd attribute buffer object of the VAO.</typeparam>
        /// <param name="vao">The VAO to use.</param>
        /// <param name="program">The program to use.</param>
        /// <param name="count">The number of vertices to draw, or -1 to draw all vertices.</param>
        public static void Draw<TAttr1, TAttr2, TAttr3>(
            this IVertexArrayObject<TAttr1, TAttr2, TAttr3> vao,
            GlProgram program,
            int count = -1)
            where TAttr1 : struct
            where TAttr2 : struct
            where TAttr3 : struct
        {
            program.Use();
            vao.Draw(count);
        }

        /// <summary>
        /// Installs a given program as part of the current rendering state, then draws this array object.
        /// </summary>
        /// <typeparam name="TAttr1">The type of the 1st attribute buffer object of the VAO.</typeparam>
        /// <typeparam name="TAttr2">The type of the 2nd attribute buffer object of the VAO.</typeparam>
        /// <typeparam name="TAttr3">The type of the 3rd attribute buffer object of the VAO.</typeparam>
        /// <typeparam name="TDefaultUniformBlock">The type of the container used for default block uniforms for the given program.</typeparam>
        /// <param name="vao">The VAO to use.</param>
        /// <param name="program">The program to use.</param>
        /// <param name="defaultUniformBlock">The values for the uniforms in the default block.</param>
        /// <param name="count">The number of vertices to draw, or -1 to draw all vertices.</param>
        public static void Draw<TAttr1, TAttr2, TAttr3, TDefaultUniformBlock>(
            this IVertexArrayObject<TAttr1, TAttr2, TAttr3> vao,
            GlProgramWithDUB<TDefaultUniformBlock> program,
            TDefaultUniformBlock defaultUniformBlock,
            int count = -1)
            where TAttr1 : struct
            where TAttr2 : struct
            where TAttr3 : struct
            where TDefaultUniformBlock : struct
        {
            program.UseWithDefaultUniformBlock(defaultUniformBlock);
            vao.Draw(count);
        }

        /// <summary>
        /// Installs a given program as part of the current rendering state, then draws this array object.
        /// </summary>
        /// <typeparam name="TAttr1">The type of the 1st attribute buffer object of the VAO.</typeparam>
        /// <typeparam name="TAttr2">The type of the 2nd attribute buffer object of the VAO.</typeparam>
        /// <typeparam name="TAttr3">The type of the 3rd attribute buffer object of the VAO.</typeparam>
        /// <typeparam name="TUbo1">The type of the 1st uniform buffer object of the program.</typeparam>
        /// <param name="vao">The VAO to use.</param>
        /// <param name="program">The program to use.</param>
        /// <param name="count">The number of vertices to draw, or -1 to draw all vertices.</param>
        public static void Draw<TAttr1, TAttr2, TAttr3, TUbo1>(
            this IVertexArrayObject<TAttr1, TAttr2, TAttr3> vao,
            GlProgram<TUbo1> program,
            int count = -1)
            where TAttr1 : struct
            where TAttr2 : struct
            where TAttr3 : struct
            where TUbo1 : struct
        {
            program.Use();
            vao.Draw(count);
        }

        /// <summary>
        /// Installs a given program as part of the current rendering state, then draws this array object.
        /// </summary>
        /// <typeparam name="TAttr1">The type of the 1st attribute buffer object of the VAO.</typeparam>
        /// <typeparam name="TAttr2">The type of the 2nd attribute buffer object of the VAO.</typeparam>
        /// <typeparam name="TAttr3">The type of the 3rd attribute buffer object of the VAO.</typeparam>
        /// <typeparam name="TDefaultUniformBlock">The type of the container used for default block uniforms for the given program.</typeparam>
        /// <typeparam name="TUbo1">The type of the 1st uniform buffer object of the program.</typeparam>
        /// <param name="vao">The VAO to use.</param>
        /// <param name="program">The program to use.</param>
        /// <param name="defaultUniformBlock">The values for the uniforms in the default block.</param>
        /// <param name="count">The number of vertices to draw, or -1 to draw all vertices.</param>
        public static void Draw<TAttr1, TAttr2, TAttr3, TDefaultUniformBlock, TUbo1>(
            this IVertexArrayObject<TAttr1, TAttr2, TAttr3> vao,
            GlProgramWithDUB<TDefaultUniformBlock, TUbo1> program,
            TDefaultUniformBlock defaultUniformBlock,
            int count = -1)
            where TAttr1 : struct
            where TAttr2 : struct
            where TAttr3 : struct
            where TDefaultUniformBlock : struct
            where TUbo1 : struct
        {
            program.UseWithDefaultUniformBlock(defaultUniformBlock);
            vao.Draw(count);
        }
    }
}