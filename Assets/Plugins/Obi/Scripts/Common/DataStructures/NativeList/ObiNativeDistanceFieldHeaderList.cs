using System;

namespace Obi
{
    [Serializable]
    public class ObiNativeDistanceFieldHeaderList : ObiNativeList<DistanceFieldHeader>
    {
        public ObiNativeDistanceFieldHeaderList()
        {
        }

        public ObiNativeDistanceFieldHeaderList(int capacity = 8, int alignment = 16) : base(capacity, alignment)
        {
            for (var i = 0; i < capacity; ++i)
                this[i] = new DistanceFieldHeader();
        }
    }
}