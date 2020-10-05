using OpenTK.Graphics.OpenGL;
using System;
using Xunit;

namespace MyOTKE.Core
{
    public class GlVertexBufferObjectTests
    {
        [Fact]
        public void NonBlittableType()
        {
            Assert.Throws<ArgumentException>(() => new GlVertexBufferObject<NonBlittable>(
                BufferTarget.ElementArrayBuffer,
                BufferUsageHint.StaticDraw,
                10,
                null));
        }

        private struct NonBlittable
        {
            public string MyString;
        }
    }
}
