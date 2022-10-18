using System.Collections.Generic;
using UnityEngine;

namespace Obi
{
    /**
     * This component plugs a prefab instance at each cut in the rope. Optionally, it will also place a couple instances at the start/end of an open rope.
     */
    [RequireComponent(typeof(ObiRope))]
    [RequireComponent(typeof(ObiPathSmoother))]
    public class ObiRopePrefabPlugger : MonoBehaviour
    {
        public GameObject prefab;

        /**< prefab object being instantiated at the rope cuts.*/
        public Vector3 instanceScale = Vector3.one;

        public bool plugTears = true;
        public bool plugStart;
        public bool plugEnd;

        private List<GameObject> instances;

        /**
         * < instances of the prefab being rendered.
         */
        private ObiPathSmoother smoother;

        private void OnEnable()
        {
            instances = new List<GameObject>();
            smoother = GetComponent<ObiPathSmoother>();
            smoother.OnCurveGenerated += UpdatePlugs;
        }

        private void OnDisable()
        {
            smoother.OnCurveGenerated -= UpdatePlugs;
            ClearPrefabInstances();
        }

        private GameObject GetOrCreatePrefabInstance(int index)
        {
            if (index < instances.Count)
                return instances[index];

            var tearPrefabInstance = Instantiate(prefab);
            tearPrefabInstance.hideFlags = HideFlags.HideAndDontSave;
            instances.Add(tearPrefabInstance);
            return tearPrefabInstance;
        }

        public void ClearPrefabInstances()
        {
            for (var i = 0; i < instances.Count; ++i)
                DestroyImmediate(instances[i]);

            instances.Clear();
        }

        // Update is called once per frame
        private void UpdatePlugs(ObiActor actor)
        {
            var rope = actor as ObiRopeBase;

            // cache the rope's transform matrix/quaternion:
            var l2w = rope.transform.localToWorldMatrix;
            var l2wRot = l2w.rotation;

            var instanceIndex = 0;

            // place prefabs at the start/end of each curve:
            for (var c = 0; c < smoother.smoothChunks.Count; ++c)
            {
                var curve = smoother.smoothChunks[c];

                if ((plugTears && c > 0) ||
                    (plugStart && c == 0))
                {
                    var instance = GetOrCreatePrefabInstance(instanceIndex++);
                    instance.SetActive(true);

                    var frame = curve[0];
                    instance.transform.position = l2w.MultiplyPoint3x4(frame.position);
                    instance.transform.rotation = l2wRot * Quaternion.LookRotation(-frame.tangent, frame.binormal);
                    instance.transform.localScale = instanceScale;
                }

                if ((plugTears && c < smoother.smoothChunks.Count - 1) ||
                    (plugEnd && c == smoother.smoothChunks.Count - 1))
                {
                    var instance = GetOrCreatePrefabInstance(instanceIndex++);
                    instance.SetActive(true);

                    var frame = curve[curve.Count - 1];
                    instance.transform.position = l2w.MultiplyPoint3x4(frame.position);
                    instance.transform.rotation = l2wRot * Quaternion.LookRotation(frame.tangent, frame.binormal);
                    instance.transform.localScale = instanceScale;
                }
            }

            // deactivate remaining instances:
            for (var i = instanceIndex; i < instances.Count; ++i)
                instances[i].SetActive(false);
        }
    }
}