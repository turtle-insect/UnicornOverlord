using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnicornOverlord
{
    internal static class CommonExtension
    {
        public static int ToInt32(this string str)
        {
            bool success = int.TryParse(str, out int result);
            if (success)
                return result;
            return 0;
        }
        public static long ToLong(this string str)
        {
            bool success = long.TryParse(str, out long result);
            if (success)
                return result;
            return 0;
        }
        public static float ToFloat(this string str)
        {
            bool success = float.TryParse(str, out float result);
            if (success)
                return result;
            return 0;
        }
        public static double ToDouble(this string str)
        {
            bool success = double.TryParse(str, out double result);
            if (success)
                return result;
            return 0;
        }
    }
}
