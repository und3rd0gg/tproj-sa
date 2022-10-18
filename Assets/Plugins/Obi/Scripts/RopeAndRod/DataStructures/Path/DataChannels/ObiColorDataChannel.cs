using System;
using UnityEngine;

namespace Obi
{
    [Serializable]
    public class ObiColorDataChannel : ObiPathDataChannelIdentity<Color>
    {
        public ObiColorDataChannel() : base(new ObiColorInterpolator3D())
        {
        }
    }
}