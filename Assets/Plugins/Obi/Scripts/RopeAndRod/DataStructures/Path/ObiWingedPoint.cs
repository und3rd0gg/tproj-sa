using System;
using UnityEngine;

namespace Obi
{
    [Serializable]
    public struct ObiWingedPoint
    {
        public enum TangentMode
        {
            Aligned,
            Mirrored,
            Free
        }

        public TangentMode tangentMode;
        public Vector3 inTangent;
        public Vector3 position;
        public Vector3 outTangent;

        public ObiWingedPoint(Vector3 inTangent, Vector3 point, Vector3 outTangent)
        {
            tangentMode = TangentMode.Aligned;
            this.inTangent = inTangent;
            position = point;
            this.outTangent = outTangent;
        }

        public Vector3 inTangentEndpoint => position + inTangent;

        public Vector3 outTangentEndpoint => position + outTangent;

        public void SetInTangentEndpoint(Vector3 value)
        {
            var newTangent = value - position;

            switch (tangentMode)
            {
                case TangentMode.Mirrored:
                    outTangent = -newTangent;
                    break;
                case TangentMode.Aligned:
                    outTangent = -newTangent.normalized * outTangent.magnitude;
                    break;
            }

            inTangent = newTangent;
        }

        public void SetOutTangentEndpoint(Vector3 value)
        {
            var newTangent = value - position;

            switch (tangentMode)
            {
                case TangentMode.Mirrored:
                    inTangent = -newTangent;
                    break;
                case TangentMode.Aligned:
                    inTangent = -newTangent.normalized * inTangent.magnitude;
                    break;
            }

            outTangent = newTangent;
        }

        public void SetInTangent(Vector3 value)
        {
            var newTangent = value;

            switch (tangentMode)
            {
                case TangentMode.Mirrored:
                    outTangent = -newTangent;
                    break;
                case TangentMode.Aligned:
                    outTangent = -newTangent.normalized * outTangent.magnitude;
                    break;
            }

            inTangent = newTangent;
        }

        public void SetOutTangent(Vector3 value)
        {
            var newTangent = value;

            switch (tangentMode)
            {
                case TangentMode.Mirrored:
                    inTangent = -newTangent;
                    break;
                case TangentMode.Aligned:
                    inTangent = -newTangent.normalized * inTangent.magnitude;
                    break;
            }

            outTangent = newTangent;
        }

        public void Transform(Vector3 translation, Quaternion rotation, Vector3 scale)
        {
            position += translation;
            inTangent = rotation * Vector3.Scale(inTangent, scale);
            outTangent = rotation * Vector3.Scale(outTangent, scale);
        }
    }
}