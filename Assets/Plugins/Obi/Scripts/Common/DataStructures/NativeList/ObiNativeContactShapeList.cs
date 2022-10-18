﻿using System;

namespace Obi
{
    [Serializable]
    public class ObiNativeContactShapeList : ObiNativeList<Oni.Contact>
    {
        public ObiNativeContactShapeList()
        {
        }

        public ObiNativeContactShapeList(int capacity = 8, int alignment = 16) : base(capacity, alignment)
        {
            for (var i = 0; i < capacity; ++i)
                this[i] = new Oni.Contact();
        }
    }
}