using System;

namespace Obi
{
    [Serializable]
    public class ObiNativeDFNodeList : ObiNativeList<DFNode>
    {
        public ObiNativeDFNodeList()
        {
        }

        public ObiNativeDFNodeList(int capacity = 8, int alignment = 16) : base(capacity, alignment)
        {
            for (var i = 0; i < capacity; ++i)
                this[i] = new DFNode();
        }
    }
}