using OpenTK.Graphics.OpenGL;
using System;

namespace MyOTKE.Core
{
    /// <summary>
    /// Managed reference to an OpenGL uniform buffer object.
    /// </summary>
    /// <remarks>
    /// The value of this class (in comparison to <see cref="IBufferObject{T}"/> implementations) is that sometimes
    /// we will want to refer to the same underlying buffer as different types - but still have safe cleanup via
    /// disposal.
    /// <para />
    /// Does reference counting. Ownership of buffers is better from most standpoints, but the library
    /// is simpler to use if we allow shared ownership - and RAD is the focus of this library.
    /// </remarks>
    internal sealed class GlBufferRef : IDisposable
    {
        private readonly GlBuffer buffer;
        private bool isDisposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="GlBufferRef"/> class,
        /// creating a new OpenGL buffer that it references.
        /// </summary>
        public GlBufferRef()
        {
            this.buffer = new GlBuffer();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GlBufferRef"/> class,
        /// that refers to the same buffer as a given existing reference.
        /// </summary>
        /// <param name="bufferRef">The existing buffer reference.</param>
        public GlBufferRef(GlBufferRef bufferRef)
        {
            this.buffer = bufferRef.buffer;
            this.buffer.Use();
        }

        /// <summary>
        /// Gets the ID of the encapsulated buffer object.
        /// </summary>
        /// <remarks>
        /// NB: It's important to keep track of whether this is disposed for cases where a GlBufferRef is shared
        /// (even though this isn't recommended). If we just blindly return the ID, we could be returning an ID
        /// for something that is just about to be deleted. It's still possible if THIS object is
        /// currently being diposed on another thread, but this behaviour does at least rule out SOME bad scenarios.
        /// </remarks>
        public int Id => !isDisposed ? buffer.Id : throw new ObjectDisposedException(GetType().FullName);

        /// <inheritdoc />
        public void Dispose()
        {
            isDisposed = true;
            buffer.Release();
        }

        /// <summary>
        /// Managed low-level wrapper for an OpenGL buffer object, which does (manual) reference counting.
        /// </summary>
        /// <remarks>
        /// This doesn't implement <see cref="IDisposable"/> by design. Explicit freeing is supposed to be done
        /// via calls to <see cref="Release"/> - providing a way to shortcut that via <see cref="IDisposable"/>
        /// goes against the point of the class.
        /// </remarks>
        private class GlBuffer
        {
            private readonly int id;
            private int refCount = 1;
            private bool isDeleted = false;

            public GlBuffer()
            {
                this.id = GL.GenBuffer();
                GlDebug.ThrowIfGlError("generating buffer");
            }

            ~GlBuffer() => Delete(true);

            public int Id => !isDeleted ? id : throw new ObjectDisposedException(GetType().FullName);

            public void Use()
            {
                // Might be fun to try to do this locklessly at some point..
                lock (this)
                {
                    ObjectDisposedException.ThrowIf(refCount == 0, this);
                    refCount++;
                }
            }

            public void Release()
            {
                lock (this)
                {
                    refCount--;

                    if (refCount == 0)
                    {
                        Delete(false);
                    }
                }
            }

            private void Delete(bool finalizing)
            {
                if (!finalizing)
                {
                    GC.SuppressFinalize(this);
                }

                isDeleted = true;
                GL.DeleteBuffer(Id);
                GlDebug.ThrowIfGlError("deleting buffer");
            }
        }
    }
}
