using System;

namespace Obi
{
    [Serializable]
    public class ObiNativeColliderShapeList : ObiNativeList<ColliderShape>
    {
        public ObiNativeColliderShapeList()
        {
        }

        public ObiNativeColliderShapeList(int capacity = 8, int alignment = 16) : base(capacity, alignment)
        {
            for (var i = 0; i < capacity; ++i)
                this[i] = new ColliderShape();
        }
    }
}