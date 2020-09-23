namespace NanoVG
{
    internal struct Vertex
    {
        public readonly float x;
        public readonly float y;
        public readonly float u;
        public readonly float v;

        public Vertex(float x, float y, float u, float v)
        {
            this.x = x;
            this.y = y;
            this.u = u;
            this.v = v;
        }
    }
}
