using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NanoVG
{
    struct Mat3x4
    {
        public float R1C1;
        public float R2C1;
        public float R3C1;
        public float R4C1;
        public float R1C2;
        public float R2C2;
        public float R3C2;
        public float R4C2;
        public float R1C3;
        public float R2C3;
        public float R3C3;
        public float R4C3;

        public void Clear()
        {
            R1C1
                = R2C1
                = R3C1
                = R4C1
                = R1C2
                = R2C2
                = R3C2
                = R4C2
                = R1C3
                = R2C3
                = R3C3
                = R4C3 = 0;
        }

        public static Mat3x4 FromTransform2D(Transform2D t)
        {
            return new Mat3x4
            {
                R1C1 = t.R1C1,
                R2C1 = t.R2C1,
                R3C1 = 0.0f,
                R4C1 = 0.0f,
                R1C2 = t.R1C2,
                R2C2 = t.R2C2,
                R3C2 = 0.0f,
                R4C2 = 0.0f,
                R1C3 = t.R1C3,
                R2C3 = t.R2C3,
                R3C3 = 1.0f,
                R4C3 = 0.0f,
            };
        }
    }
}
