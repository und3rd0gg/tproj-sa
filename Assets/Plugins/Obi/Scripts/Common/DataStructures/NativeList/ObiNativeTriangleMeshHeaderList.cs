using System;

namespace Obi
{
    [Serializable]
    public class ObiNativeTriangleMeshHeaderList : ObiNativeList<TriangleMeshHeader>
    {
        public ObiNativeTriangleMeshHeaderList()
        {
        }

        public ObiNativeTriangleMeshHeaderList(int capacity = 8, int alignment = 16) : base(capacity, alignment)
        {
            for (var i = 0; i < capacity; ++i)
                this[i] = new TriangleMeshHeader();
        }
    }
}