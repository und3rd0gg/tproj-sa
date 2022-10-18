using UnityEngine;

namespace Gameplay
{
    public struct SuctionPointConstrains
    {
        public float _minX { get; }
        public float _maxX { get; }
        public float _minZ { get; }
        public float _maxZ { get; }

        public float GetNormalizedX(float t)
        {
            return Mathf.Lerp(_minX, _maxX, t);
        }

        public float GetNormalizedZ(float t)
        {
            return Mathf.Lerp(_minZ, _maxZ, t);
        }

        public SuctionPointConstrains(float minX, float maxX, float minZ, float maxZ)
        {
            _minX = minX;
            _maxX = maxX;
            _minZ = minZ;
            _maxZ = maxZ;
        }

        public override string ToString()
        {
            return $"SuctionPointConstrains: minX: {_minX} maxX: {_maxX} minZ: {_minZ} maxZ: {_maxZ}";
        }
    }
}