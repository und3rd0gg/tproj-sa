using System;
using UnityEngine;

namespace Obi
{
    [Serializable]
    public struct DFNode
    {
        public Vector4 distancesA;
        public Vector4 distancesB;
        public Vector4 center;
        public int firstChild;

        // add 12 bytes of padding to ensure correct memory alignment:
        private int pad0;
        private int pad1;
        private int pad2;

        public DFNode(Vector4 center)
        {
            distancesA = Vector4.zero;
            distancesB = Vector4.zero;
            this.center = center;
            firstChild = -1;
            pad0 = 0;
            pad1 = 0;
            pad2 = 0;
        }

        public float Sample(Vector3 position)
        {
            var nPos = GetNormalizedPos(position);

            // trilinear interpolation: interpolate along x axis
            var x = distancesA + (distancesB - distancesA) * nPos[0];

            // interpolate along y axis
            var y0 = x[0] + (x[2] - x[0]) * nPos[1];
            var y1 = x[1] + (x[3] - x[1]) * nPos[1];

            // interpolate along z axis.
            return y0 + (y1 - y0) * nPos[2];
        }

        public Vector3 GetNormalizedPos(Vector3 position)
        {
            var size = center[3] * 2;
            return new Vector3(
                (position[0] - (center[0] - center[3])) / size,
                (position[1] - (center[1] - center[3])) / size,
                (position[2] - (center[2] - center[3])) / size
            );
        }

        public int GetOctant(Vector3 position)
        {
            var index = 0;
            if (position[0] > center[0]) index |= 4;
            if (position[1] > center[1]) index |= 2;
            if (position[2] > center[2]) index |= 1;
            return index;
        }
    }
}