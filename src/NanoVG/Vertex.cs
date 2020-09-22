namespace NanoVG
{
    internal struct Vertex
    {
        public Vertex(float x, float y, float u, float v)
        {
            this.x = x;
            this.y = y;
            this.u = u;
            this.v = v;
        }

        public float x { get; }

        public float y { get; }

        public float u { get; }

        public float v { get; }
    }
}
