using System;

namespace Obi
{
    [Serializable]
    public class ObiNativeHeightFieldHeaderList : ObiNativeList<HeightFieldHeader>
    {
        public ObiNativeHeightFieldHeaderList()
        {
        }

        public ObiNativeHeightFieldHeaderList(int capacity = 8, int alignment = 16) : base(capacity, alignment)
        {
            for (var i = 0; i < capacity; ++i)
                this[i] = new HeightFieldHeader();
        }
    }
}