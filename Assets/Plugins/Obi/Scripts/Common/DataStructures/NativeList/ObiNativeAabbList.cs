﻿using System;

namespace Obi
{
    [Serializable]
    public class ObiNativeAabbList : ObiNativeList<Aabb>
    {
        public ObiNativeAabbList()
        {
        }

        public ObiNativeAabbList(int capacity = 8, int alignment = 16) : base(capacity, alignment)
        {
            for (var i = 0; i < capacity; ++i)
                this[i] = new Aabb();
        }
    }
}