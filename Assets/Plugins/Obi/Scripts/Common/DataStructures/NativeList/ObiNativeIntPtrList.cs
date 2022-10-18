using System;

namespace Obi
{
    public class ObiNativeIntPtrList : ObiNativeList<IntPtr>
    {
        public ObiNativeIntPtrList()
        {
        }

        public ObiNativeIntPtrList(int capacity = 8, int alignment = 16) : base(capacity, alignment)
        {
            for (var i = 0; i < capacity; ++i)
                this[i] = IntPtr.Zero;
        }
    }
}