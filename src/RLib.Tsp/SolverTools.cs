using System;

namespace RLib.Tsp
{
    public static class SolverTools
    {
        public static void ApplyNodeSwap(int[] route, int aIndex, int bIndex)
        {
            var swapSize = bIndex - aIndex + 1;
            Array.Reverse(route, aIndex, swapSize);
        }
        public static int[] GetRouteByNodeSwap(int[] route, int aIndex, int bIndex)
        {
            var newRoute = new int[route.Length];

            int size = sizeof(int);
            int length = route.Length * size;
            Buffer.BlockCopy(route, 0, newRoute, 0, length);

            ApplyNodeSwap(newRoute, aIndex, bIndex);

            return newRoute;
        }
    }
}
