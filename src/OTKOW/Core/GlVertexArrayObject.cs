﻿using OpenToolkit.Graphics;
using OpenToolkit.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace OTKOW.Core
{
    /// <summary>
    /// Represents an OpenGL vertex array object.
    /// </summary>
    public sealed class GlVertexArrayObject : IVertexArrayObject, IDisposable
    {
        private readonly int id;
        private readonly PrimitiveType primitiveType;
        private readonly GlVertexBufferObject[] attributeBuffers;
        private readonly GlVertexBufferObject indexBuffer;

        /// <summary>
        /// Initializes a new instance of the <see cref="GlVertexArrayObject"/> class. SIDE EFFECT: new VAO will be bound.
        /// </summary>
        /// <param name="primitiveType">OpenGL primitive type.</param>
        /// <param name="attributeBufferSpecs">Specs for the buffers in this VAO.</param>
        /// <param name="indexSpec">The spec for the index of this VAO.</param>
        internal GlVertexArrayObject(
            PrimitiveType primitiveType,
            IList<(BufferUsageHint usage, Type elementType, int capacity, Array data)> attributeBufferSpecs,
            (int capacity, uint[] data) indexSpec)
        {
            GlExt.ThrowIfNoCurrentContext();

            // Record primitive type for use in draw calls, create and bind the VAO
            this.primitiveType = primitiveType;
            this.id = GL.GenVertexArray(); // superbible uses CreateVertexArray?
            GL.BindVertexArray(id);

            // Set up the attribute buffers
            this.attributeBuffers = new GlVertexBufferObject[attributeBufferSpecs.Count];
            int k = 0;
            for (int i = 0; i < attributeBuffers.Length; i++)
            {
                var buffer = attributeBuffers[i] = new GlVertexBufferObject(
                    BufferTarget.ArrayBuffer,
                    attributeBufferSpecs[i].usage,
                    attributeBufferSpecs[i].elementType,
                    attributeBufferSpecs[i].capacity,
                    attributeBufferSpecs[i].data);
                for (uint j = 0; j < buffer.Attributes.Length; j++, k++)
                {
                    var attribute = buffer.Attributes[j];
                    GL.EnableVertexAttribArray(k);
                    GL.VertexAttribPointer(
                        index: k, // must match the layout in the shader
                        size: attribute.Multiple,
                        type: attribute.Type,
                        normalized: false,
                        stride: attribute.Stride,
                        pointer: attribute.Offset);
                }
            }

            // Establish element count & populate index buffer if there is one
            if (indexSpec.capacity > 0)
            {
                this.indexBuffer = new GlVertexBufferObject(BufferTarget.ElementArrayBuffer, BufferUsageHint.DynamicDraw, typeof(uint), indexSpec.capacity, indexSpec.data);
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="GlVertexArrayObject"/> class.
        /// </summary>
        ~GlVertexArrayObject() => Dispose(false);

        /// <summary>
        /// Gets the number of vertices to be rendered.
        /// </summary>
        public int VertexCount => indexBuffer?.Capacity ?? attributeBuffers[0].Capacity;

        /// <inheritdoc />
        public IVertexBufferObject IndexBuffer => this.indexBuffer;

        /// <inheritdoc />
        public IReadOnlyList<IVertexBufferObject> AttributeBuffers => this.attributeBuffers;

        /// <inheritdoc />
        public void Draw(int count = -1)
        {
            GL.BindVertexArray(this.id);

            if (indexBuffer != null)
            {
                // There's an index buffer (which will be bound) - bind it and draw
                GL.DrawElements(
                    mode: this.primitiveType,
                    count: count == -1 ? this.VertexCount : count,
                    type: DrawElementsType.UnsignedInt,
                    indices: IntPtr.Zero);
            }
            else
            {
                // No index - so draw directly from attribute data
                GL.DrawArrays(
                    mode: this.primitiveType,
                    first: 0,
                    count: count == -1 ? this.VertexCount : count);
            }
        }

        /// <inheritdoc />
        public void Dispose() => Dispose(true);

        /*public void ResizeAttributeBuffer(int bufferIndex, int newSize)
        {
            //var newId = Gl.GenBuffer();
            //Gl.NamedBufferData(newId, (uint)(Marshal.SizeOf(elementType) * value), null, usage);
            //Gl.CopyNamedBufferSubData(this.Id, newId, 0, 0, (uint)(Marshal.SizeOf(elementType) * count));
            //count = value;
        }*/

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var buffer in attributeBuffers)
                {
                    buffer.Dispose();
                }

                if (indexBuffer != null)
                {
                    indexBuffer.Dispose();
                }

                GC.SuppressFinalize(this);
            }

            if (GraphicsContext.CurrentContext != null)
            {
                GL.DeleteVertexArrays(1, new[] { this.id });
            }
        }
    }
}
