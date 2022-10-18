using System;
using System.Collections.Generic;
using UnityEngine;

namespace Obi
{
    [Serializable]
    public class ObiParticleGroup : ScriptableObject
    {
        public List<int> particleIndices = new List<int>();
        public ObiActorBlueprint m_Blueprint;

        public ObiActorBlueprint blueprint => m_Blueprint;

        public int Count => particleIndices.Count;

        public void SetSourceBlueprint(ObiActorBlueprint blueprint)
        {
            m_Blueprint = blueprint;
        }

        public bool ContainsParticle(int index)
        {
            return particleIndices.Contains(index);
        }
    }
}