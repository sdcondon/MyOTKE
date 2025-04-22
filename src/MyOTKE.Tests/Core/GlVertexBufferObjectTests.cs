using OpenTK.Graphics.OpenGL;
using System;
using Xunit;

namespace MyOTKE.Core.Core;

public class GlVertexBufferObjectTests
{
    [Fact(Skip = "No longer thrown as arg exception from ctor - now an error on type initialization - which doesn't seem to be testable..")]
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
