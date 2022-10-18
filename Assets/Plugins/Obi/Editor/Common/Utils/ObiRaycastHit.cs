using UnityEngine;

namespace Obi
{
    public class ObiRaycastHit
    {
        /// Distance from the Raycast origin to the point of impact.
        public float distance;

        /// The normal in model space of the triangle that this raycast hit.
        public Vector3 normal;

        /// The position in model space where a raycast intercepted a triangle.
        public Vector3 position;

        /// The triangle index of the hit face.
        public int triangle;

        public ObiRaycastHit(float distance, Vector3 position, Vector3 normal, int triangle)
        {
            this.distance = distance;
            this.position = position;
            this.normal = normal;
            this.triangle = triangle;
        }
    }
}