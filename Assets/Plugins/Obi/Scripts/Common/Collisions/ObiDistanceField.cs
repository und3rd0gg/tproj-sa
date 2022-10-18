using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Obi
{
    [CreateAssetMenu(fileName = "distance field", menuName = "Obi/Distance Field", order = 181)]
    [ExecuteInEditMode]
    public class ObiDistanceField : ScriptableObject
    {
        [SerializeProperty("InputMesh")] [SerializeField]
        private Mesh input;

        [HideInInspector] [SerializeField] private float minNodeSize;
        [HideInInspector] [SerializeField] private Bounds bounds;
        [HideInInspector] public List<DFNode> nodes;

        /**< list of distance field nodes*/
        [Range(0.0000001f, 0.1f)] public float maxError = 0.01f;

        [Range(1, 8)] public int maxDepth = 5;

        public bool Initialized => nodes != null;

        public Bounds FieldBounds => bounds;

        public float EffectiveSampleSize => minNodeSize;

        public Mesh InputMesh
        {
            set
            {
                if (value != input)
                {
                    Reset();
                    input = value;
                }
            }
            get => input;
        }

        public void Reset()
        {
            nodes = null;
            if (input != null)
                bounds = input.bounds;
        }

        public IEnumerator Generate()
        {
            Reset();

            if (input == null)
                yield break;

            var tris = input.triangles;
            var verts = input.vertices;

            nodes = new List<DFNode>();
            var buildingCoroutine = ASDF.Build(maxError, maxDepth, verts, tris, nodes);

            while (buildingCoroutine.MoveNext())
                yield return new CoroutineJob.ProgressInfo("Processed nodes: " + nodes.Count, 1);

            // calculate min node size;
            minNodeSize = float.PositiveInfinity;
            for (var j = 0; j < nodes.Count; ++j)
                minNodeSize = Mathf.Min(minNodeSize, nodes[j].center[3] * 2);

            // get bounds:
            var max = Mathf.Max(bounds.size[0], Mathf.Max(bounds.size[1], bounds.size[2])) + 0.2f;
            bounds.size = new Vector3(max, max, max);
        }

        /**
		 * Return a volume texture containing a representation of this distance field.
		 */
        public Texture3D GetVolumeTexture(int size)
        {
            if (!Initialized)
                return null;

            // upper bound of the distance from any point inside the bounds to the surface.
            var maxDist = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);

            var spacingX = bounds.size.x / size;
            var spacingY = bounds.size.y / size;
            var spacingZ = bounds.size.z / size;

            var tex = new Texture3D(size, size, size, TextureFormat.Alpha8, false);

            var cols = new Color[size * size * size];
            var idx = 0;
            var c = Color.black;

            for (var z = 0; z < size; ++z)
            for (var y = 0; y < size; ++y)
            for (var x = 0; x < size; ++x, ++idx)
            {
                var samplePoint = bounds.min + new Vector3(spacingX * x + spacingX * 0.5f,
                    spacingY * y + spacingY * 0.5f,
                    spacingZ * z + spacingZ * 0.5f);

                var distance = ASDF.Sample(nodes, samplePoint);

                if (distance >= 0)
                    c.a = distance.Remap(0, maxDist * 0.1f, 0.5f, 1);
                else
                    c.a = distance.Remap(-maxDist * 0.1f, 0, 0, 0.5f);

                cols[idx] = c;
            }

            tex.SetPixels(cols);
            tex.Apply();
            return tex;
        }
    }
}