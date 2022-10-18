using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

namespace Obi
{
    [AddComponentMenu("Physics/Obi/Obi Rope Chain Renderer", 885)]
    [ExecuteInEditMode]
    public class ObiRopeChainRenderer : MonoBehaviour
    {
        private static ProfilerMarker m_UpdateChainRopeRendererChunksPerfMarker =
            new ProfilerMarker("UpdateChainRopeRenderer");

        [HideInInspector] [SerializeField] public List<GameObject> linkInstances = new List<GameObject>();

        [SerializeProperty("RandomizeLinks")] [SerializeField]
        private bool randomizeLinks;

        public Vector3 linkScale = Vector3.one;

        /**< Scale of chain links.*/
        public List<GameObject> linkPrefabs = new List<GameObject>();

        [Range(0, 1)] public float twistAnchor;

        /**< Normalized position of twisting origin along rope.*/
        public float sectionTwist;

        /**< Amount of twist applied to each section, in degrees.*/
        private ObiPathFrame frame;

        public bool RandomizeLinks
        {
            get => randomizeLinks;
            set
            {
                if (value != randomizeLinks)
                {
                    randomizeLinks = value;
                    CreateChainLinkInstances(GetComponent<ObiRopeBase>());
                }
            }
        }

        private void Awake()
        {
            ClearChainLinkInstances();
        }

        private void OnEnable()
        {
            GetComponent<ObiRopeBase>().OnInterpolate += UpdateRenderer;
        }

        private void OnDisable()
        {
            GetComponent<ObiRopeBase>().OnInterpolate -= UpdateRenderer;
            ClearChainLinkInstances();
        }

        /**
         * Destroys all chain link instances. Used when the chain must be re-created from scratch, and when the actor is disabled/destroyed.
         */
        public void ClearChainLinkInstances()
        {
            if (linkInstances == null)
                return;

            for (var i = 0; i < linkInstances.Count; ++i)
                if (linkInstances[i] != null)
                    DestroyImmediate(linkInstances[i]);
            linkInstances.Clear();
        }

        public void CreateChainLinkInstances(ObiRopeBase rope)
        {
            ClearChainLinkInstances();

            if (linkPrefabs.Count > 0)
                for (var i = 0; i < rope.particleCount; ++i)
                {
                    var index = randomizeLinks ? Random.Range(0, linkPrefabs.Count) : i % linkPrefabs.Count;

                    GameObject linkInstance = null;

                    if (linkPrefabs[index] != null)
                    {
                        linkInstance = Instantiate(linkPrefabs[index]);
                        linkInstance.transform.SetParent(rope.transform, false);
                        linkInstance.hideFlags = HideFlags.HideAndDontSave;
                        linkInstance.SetActive(false);
                    }

                    linkInstances.Add(linkInstance);
                }
        }

        public void UpdateRenderer(ObiActor actor)
        {
            using (m_UpdateChainRopeRendererChunksPerfMarker.Auto())
            {
                var rope = actor as ObiRopeBase;

                // In case there are no link prefabs to instantiate:
                if (linkPrefabs.Count == 0)
                    return;

                // Regenerate instances if needed:
                if (linkInstances == null || linkInstances.Count < rope.particleCount) CreateChainLinkInstances(rope);

                var blueprint = rope.sourceBlueprint;
                var elementCount = rope.elements.Count;

                var twist = -sectionTwist * elementCount * twistAnchor;

                //we will define and transport a reference frame along the curve using parallel transport method:
                frame.Reset();
                frame.SetTwist(twist);

                var lastParticle = -1;

                for (var i = 0; i < elementCount; ++i)
                {
                    var elm = rope.elements[i];

                    var pos = rope.GetParticlePosition(elm.particle1);
                    var nextPos = rope.GetParticlePosition(elm.particle2);
                    var linkVector = nextPos - pos;
                    var tangent = linkVector.normalized;

                    if (rope.sourceBlueprint.usesOrientedParticles)
                    {
                        frame.Transport(nextPos, tangent, rope.GetParticleOrientation(elm.particle1) * Vector3.up,
                            twist);
                        twist += sectionTwist;
                    }
                    else
                    {
                        frame.Transport(nextPos, tangent, sectionTwist);
                    }

                    if (linkInstances[i] != null)
                    {
                        linkInstances[i].SetActive(true);
                        var linkTransform = linkInstances[i].transform;
                        linkTransform.position = pos + linkVector * 0.5f;
                        linkTransform.localScale = rope.GetParticleMaxRadius(elm.particle1) * 2 * linkScale;
                        linkTransform.rotation = Quaternion.LookRotation(tangent, frame.normal);
                    }

                    lastParticle = elm.particle2;
                }

                for (var i = elementCount; i < linkInstances.Count; ++i)
                    if (linkInstances[i] != null)
                        linkInstances[i].SetActive(false);
            }
        }
    }
}