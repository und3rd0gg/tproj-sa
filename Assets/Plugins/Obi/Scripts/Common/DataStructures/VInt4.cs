using System;
using System.Runtime.InteropServices;

namespace Obi
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct VInt4
    {
        public int x;
        public int y;
        public int z;
        public int w;

        public VInt4(int x, int y, int z, int w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public VInt4(int x)
        {
            this.x = x;
            y = x;
            z = x;
            w = x;
        }
    }
}