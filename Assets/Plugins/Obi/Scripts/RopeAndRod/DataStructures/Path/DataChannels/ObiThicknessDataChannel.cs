using System;

namespace Obi
{
    [Serializable]
    public class ObiThicknessDataChannel : ObiPathDataChannelIdentity<float>
    {
        public ObiThicknessDataChannel() : base(new ObiCatmullRomInterpolator())
        {
        }
    }
}