using System;
using UnityEngine;

namespace Obi
{
    [Serializable]
    public class ObiNormalDataChannel : ObiPathDataChannelIdentity<Vector3>
    {
        public ObiNormalDataChannel() : base(new ObiCatmullRomInterpolator3D())
        {
        }
    }
}