using System.Collections;
using UnityEngine;

namespace Obi
{
    public abstract class ObiRopeBlueprintBase : ObiActorBlueprint
    {
        [HideInInspector] [SerializeField] public ObiPath path = new ObiPath();
        public float thickness = 0.1f;

        [Range(0, 1)] public float resolution = 1;

        [HideInInspector] [SerializeField] protected float m_InterParticleDistance;
        [HideInInspector] [SerializeField] protected int totalParticles;
        [HideInInspector] [SerializeField] protected float m_RestLength;

        [HideInInspector] public float[] restLengths;

        public float interParticleDistance => m_InterParticleDistance;

        public float restLength => m_RestLength;

        public void OnEnable()
        {
            path.OnPathChanged.AddListener(GenerateImmediate);
            path.OnControlPointAdded.AddListener(ControlPointAdded);
            path.OnControlPointRemoved.AddListener(ControlPointRemoved);
            path.OnControlPointRenamed.AddListener(ControlPointRenamed);
        }

        public void OnDisable()
        {
            path.OnPathChanged.RemoveAllListeners();
            path.OnControlPointAdded.RemoveAllListeners();
            path.OnControlPointRemoved.RemoveAllListeners();
            path.OnControlPointRenamed.RemoveAllListeners();
        }

        protected void ControlPointAdded(int index)
        {
            var group = InsertNewParticleGroup(path.GetName(index), index);
        }

        protected void ControlPointRenamed(int index)
        {
            SetParticleGroupName(index, path.GetName(index));
        }

        protected void ControlPointRemoved(int index)
        {
            RemoveParticleGroupAt(index);
        }

        protected void CreateSimplices(int numSegments)
        {
            edges = new int[numSegments * 2];
            for (var i = 0; i < numSegments; ++i)
            {
                edges[i * 2] = i % totalParticles;
                edges[i * 2 + 1] = (i + 1) % totalParticles;
            }
        }

        protected override IEnumerator Initialize()
        {
            yield return null;
        }
    }
}