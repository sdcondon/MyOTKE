namespace NanoVG
{
    internal struct ScissorInfo
    {
        public Transform2D xform;
        public Extent2D extent; // todo: needed? xform could be implicitly of unit square..
    }
}
