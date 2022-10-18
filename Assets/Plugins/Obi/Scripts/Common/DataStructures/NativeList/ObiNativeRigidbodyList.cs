using System;

namespace Obi
{
    [Serializable]
    public class ObiNativeRigidbodyList : ObiNativeList<ColliderRigidbody>
    {
        public ObiNativeRigidbodyList()
        {
        }

        public ObiNativeRigidbodyList(int capacity = 8, int alignment = 16) : base(capacity, alignment)
        {
            for (var i = 0; i < capacity; ++i)
                this[i] = new ColliderRigidbody();
        }
    }
}