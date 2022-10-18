using System;

namespace Obi
{
    [Serializable]
    public class ObiNativeAffineTransformList : ObiNativeList<AffineTransform>
    {
        public ObiNativeAffineTransformList()
        {
        }

        public ObiNativeAffineTransformList(int capacity = 8, int alignment = 16) : base(capacity, alignment)
        {
            for (var i = 0; i < capacity; ++i)
                this[i] = new AffineTransform();
        }
    }
}