using System;

namespace Obi
{
    [Serializable]
    public class ObiNativeFloatList : ObiNativeList<float>
    {
        public ObiNativeFloatList()
        {
        }

        public ObiNativeFloatList(int capacity = 8, int alignment = 16) : base(capacity, alignment)
        {
            for (var i = 0; i < capacity; ++i)
                this[i] = 0;
        }
    }
}