using System;

namespace Obi
{
    [Serializable]
    public class ObiRotationalMassDataChannel : ObiPathDataChannelIdentity<float>
    {
        public ObiRotationalMassDataChannel() : base(new ObiCatmullRomInterpolator())
        {
        }
    }
}