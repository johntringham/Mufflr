using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mufflr
{
    public static class Utils
    {
        public static bool IsCloseTo(this float x, float y, float epsilon = 0.005f)
        {
            return Math.Abs(x - y) < epsilon;
        }

        // Framerate independant lerping
        public static float Damp(float current, float target, float lambda, float dt)
        {
            return Lerp(current, target, 1 - MathF.Exp(-lambda * dt));
        }

        public static float Lerp(float a, float b, float t)
        {
            return (b - a) * t + a;
        }
    }
}
