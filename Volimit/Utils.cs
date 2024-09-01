using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Volimit
{
    public static class Utils
    {
        public static bool IsCloseTo(this float x, float y, float epsilon = 0.005f)
        {
            return Math.Abs(x - y) < epsilon;
        }
    }
}
