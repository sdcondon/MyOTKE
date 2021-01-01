using System.Numerics;

namespace MyOTKE.Renderables.Gui
{
    /// <summary>
    /// Container for information about a GUI element vertex.
    /// </summary>
    public struct Vertex
    {
        /// <summary>
        /// The position of the vertex.
        /// </summary>
        public readonly Vector2 Position;

        /// <summary>
        /// The color of the vertex.
        /// </summary>
        public readonly Vector4 Color;

        /// <summary>
        /// The z-ordinate of the texture of the vertex.
        /// </summary>
        public readonly float TexZ;

        /// <summary>
        /// The x- and y-ordinates of the texture of the vertex.
        /// </summary>
        public readonly Vector2 TexXY;

        /// <summary>
        /// Initializes a new instance of the <see cref="Vertex"/> struct.
        /// </summary>
        /// <param name="position">The position of the vertex.</param>
        /// <param name="color">The color of the vertex.</param>
        public Vertex(Vector2 position, Vector4 color)
        {
            this.Position = position;
            this.Color = color;
            this.TexZ = -1;
            this.TexXY = Vector2.Zero;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vertex"/> struct.
        /// </summary>
        /// <param name="position">The position of the vertex.</param>
        /// <param name="color">The color of the vertex.</param>
        /// <param name="texZ">The z-ordinate of the texture of the vertex.</param>
        /// <param name="texXY">The x- and y-ordinates of the texture of the vertex.</param>
        public Vertex(Vector2 position, Vector4 color, int texZ, Vector2 texXY)
        {
            this.Position = position;
            this.Color = color;
            this.TexZ = texZ;
            this.TexXY = texXY;
        }
    }
}
