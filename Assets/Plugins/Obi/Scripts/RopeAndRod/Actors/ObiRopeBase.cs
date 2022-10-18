using System.Collections.Generic;
using UnityEngine;

namespace Obi
{
    public abstract class ObiRopeBase : ObiActor
    {
        [SerializeField] protected bool m_SelfCollisions;
        [HideInInspector] [SerializeField] protected float restLength_;
        [HideInInspector] public List<ObiStructuralElement> elements = new List<ObiStructuralElement>();

        public float restLength => restLength_;

        public ObiPath path
        {
            get
            {
                var ropeBlueprint = sourceBlueprint as ObiRopeBlueprintBase;
                return ropeBlueprint != null ? ropeBlueprint.path : null;
            }
        }

        /**< Elements.*/
        public event ActorCallback OnElementsGenerated;

        /// <summary>
        ///     Calculates and returns current rope length, including stretching/compression.
        /// </summary>
        public float CalculateLength()
        {
            float length = 0;

            if (isLoaded)
            {
                // Iterate trough all distance constraints in order:
                var elementCount = elements.Count;
                for (var i = 0; i < elementCount; ++i)
                    length += Vector4.Distance(solver.positions[elements[i].particle1],
                        solver.positions[elements[i].particle2]);
            }

            return length;
        }

        /// <summary>
        ///     Recalculates the rope's rest length, that is, its length as specified by the blueprint.
        /// </summary>
        public void RecalculateRestLength()
        {
            restLength_ = 0;

            // Iterate trough all distance elements and accumulate their rest lengths.
            var elementCount = elements.Count;
            for (var i = 0; i < elementCount; ++i)
                restLength_ += elements[i].restLength;
        }

        /// <summary>
        ///     Recalculates all particle rest positions, used when filtering self-collisions.
        /// </summary>
        public void RecalculateRestPositions()
        {
            float pos = 0;
            var elementCount = elements.Count;
            for (var i = 0; i < elementCount; ++i)
            {
                solver.restPositions[elements[i].particle1] = new Vector4(pos, 0, 0, 1);
                pos += elements[i].restLength;
                solver.restPositions[elements[i].particle2] = new Vector4(pos, 0, 0, 1);
            }
        }

        /// <summary>
        ///     Regenerates all rope elements using constraints. It's the opposite of RebuildConstraintsFromElements(). This is
        ///     automatically called when loading a blueprint, but should also be called when manually
        ///     altering rope constraints (adding/removing/updating constraints and/or batches).
        /// </summary>
        public void RebuildElementsFromConstraints()
        {
            RebuildElementsFromConstraintsInternal();
            if (OnElementsGenerated != null)
                OnElementsGenerated(this);
        }

        protected abstract void RebuildElementsFromConstraintsInternal();

        /// <summary>
        ///     Regenerates all rope constraints using rope elements. It's the opposite of RebuildElementsFromConstraints().This
        ///     should be called anytime the element representation of the rope
        ///     is changed (adding/removing/updating elements). This is usually the case after tearing the rope or changing its
        ///     length using a cursor.
        /// </summary>
        public virtual void RebuildConstraintsFromElements()
        {
        }


        /// <summary>
        ///     Returns a rope element that contains a length-normalized coordinate. It will also return the length-normalized
        ///     coordinate within the element.
        /// </summary>
        public ObiStructuralElement GetElementAt(float mu, out float elementMu)
        {
            var edgeMu = elements.Count * Mathf.Clamp(mu, 0, 0.99999f);

            var index = (int) edgeMu;
            elementMu = edgeMu - index;

            if (elements != null && index < elements.Count)
                return elements[index];
            return null;
        }
    }
}