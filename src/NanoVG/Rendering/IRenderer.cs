namespace NanoVG
{
    internal interface IRenderer
    {
        void RenderCreate();

        int RenderCreateTexture(Texture type, int w, int h, ImageFlags imageFlags, byte[] data);

        int RenderDeleteTexture(int image);

        int RenderUpdateTexture(int image, int x, int y, int w, int h, byte[] data);

        int RenderGetTextureSize(int image, out int w, out int h);

        void RenderViewport(float width, float height, float devicePixelRatio);

        void RenderCancel();

        void RenderFlush();

        void RenderFill(ref Paint paint, CompositeOperationState compositeOperation, ref ScissorInfo scissor, float fringe, NVG.Bounds2D bounds, NVG.Path[] paths, int npaths);

        void RenderStroke(ref Paint paint, CompositeOperationState compositeOperation, ref ScissorInfo scissor, float fringe, float strokeWidth, NVG.Path[] paths, int npaths);

        void RenderTriangles(ref Paint paint, CompositeOperationState compositeOperation, ref ScissorInfo scissor, Vertex[] verts, int nverts);

        void RenderDelete();
    }
}
