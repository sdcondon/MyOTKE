using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace MyOTKE.Core.Internals;

internal class GlUniformBlockBindingRef : IDisposable
{
    private readonly GlUniformBlockBinding binding;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlUniformBlockBindingRef"/> class.
    /// </summary>
    /// <param name="programId"></param>
    /// <param name="blockName"></param>
    public GlUniformBlockBindingRef(int programId, string blockName)
    {
        binding = GlUniformBlockBinding.Bind(programId, blockName);
        binding.Use();
    }

    /// <summary>
    /// Gets the binding point of this binding.
    /// </summary>
    public int BindingPoint => binding.BindingPoint;

    /// <summary>
    /// Gets the buffer reference used by this binding.
    /// </summary>
    public GlBufferRef BufferRef => binding.BufferRef;

    /// <inheritdoc />
    public void Dispose()
    {
        // isDisposed = true;
        binding.Release();
    }

    /// <remarks>
    /// BUG: We don't check for binding points already declared in shaders when creating a new one (see below)
    /// so there's a good chance of clobbering them if a given application uses some shader-specified points,
    /// and some not. I can't find anything in the OpenGL spec to retrieve buffers by binding point..
    /// </remarks>
    private class GlUniformBlockBinding
    {
        private static readonly Dictionary<string, GlUniformBlockBinding> BindingsByBlockName = [];
        private static readonly Dictionary<int, GlUniformBlockBinding> BindingsByBindingPoint = [];

        private bool isDeleted = false;
        private int refCount = 0;

        private GlUniformBlockBinding(int bindingPoint, string key)
        {
            BindingPoint = bindingPoint;
            Key = key;
            BindingsByBlockName[key] = BindingsByBindingPoint[bindingPoint] = this;
            BufferRef = new GlBufferRef();
        }

        // BUG/TODO: Will never fire because of the static dictionaries - could turn them into weak refs..
        ~GlUniformBlockBinding() => Delete(true);

        /// <summary>
        /// Gets the binding point of this binding.
        /// </summary>
        public int BindingPoint { get; }

        /// <summary>
        /// Gets the key of this binding.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Gets the buffer reference used by this binding.
        /// </summary>
        public GlBufferRef BufferRef { get; }

        public static GlUniformBlockBinding Bind(int programId, string blockName)
        {
            GlUniformBlockBinding binding;

            // Get the block index
            var uniformBlockIndex = GL.GetUniformBlockIndex(programId, blockName);
            GlDebug.ThrowIfGlError("getting uniform block index");
            if (uniformBlockIndex == (int)All.InvalidIndex)
            {
                throw new ArgumentException($"Uniform block '{blockName}' not found.", nameof(blockName));
            }

            // Check if this block already has an assigned binding point.
            GL.GetActiveUniformBlock(programId, uniformBlockIndex, ActiveUniformBlockParameter.UniformBlockBinding, out int bindingPointIndex);
            GlDebug.ThrowIfGlError("getting uniform block binding");

            if (bindingPointIndex != 0)
            {
                // Block already has a binding (might be specified in the shader), so..

                // If we've already created a binding ref for this block index, use that.
                // Otherwise create a new one.
                if (!BindingsByBindingPoint.TryGetValue(bindingPointIndex, out binding))
                {
                    binding = new GlUniformBlockBinding(bindingPointIndex, blockName);
                }
            }
            else
            {
                // Block doesn't already have a binding, so..

                // If we've already created a binding point for this block name, use that.
                // Otherwise assign a brand new one.
                if (!BindingsByBlockName.TryGetValue(blockName, out binding))
                {
                    // TODO: Not right. should also skip bindings already in use in OpenGL (because they're specified in shaders)
                    // that we aren't aware of yet. might be better to not maintain a record ourselves and always just query opengl
                    // (but I can't see an obvious way to do that..)
                    int firstUnusedBindingPoint = 1;
                    while (BindingsByBindingPoint.ContainsKey(firstUnusedBindingPoint))
                    {
                        firstUnusedBindingPoint++;
                    }

                    binding = new GlUniformBlockBinding(firstUnusedBindingPoint, blockName);
                }

                // Bind this block to the binding point
                GL.UniformBlockBinding(programId, uniformBlockIndex, binding.BindingPoint);
                GlDebug.ThrowIfGlError("setting uniform block binding");
            }

            return binding;
        }

        public void Use()
        {
            // Might be fun to try to do this locklessly at some point..
            lock (this)
            {
                ObjectDisposedException.ThrowIf(isDeleted, this);
                refCount++;
            }
        }

        public void Release()
        {
            lock (this)
            {
                refCount--;

                if (refCount <= 0)
                {
                    Delete(false);
                }
            }
        }

        private void Delete(bool finalizing)
        {
            isDeleted = true;

            BindingsByBlockName.Remove(Key);
            BindingsByBindingPoint.Remove(BindingPoint);

            if (!finalizing)
            {
                BufferRef.Dispose();
                GC.SuppressFinalize(this);
            }
        }
    }
}
