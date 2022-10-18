using System;
using System.Collections.Generic;
using UnityEngine;

namespace Obi
{
    [Serializable]
    public class ObiAerodynamicConstraintsBatch : ObiConstraintsBatch
    {
        /// <summary>
        ///     3 floats per constraint: surface area, drag and lift.
        /// </summary>
        [HideInInspector] public ObiNativeFloatList aerodynamicCoeffs = new ObiNativeFloatList();

        protected IAerodynamicConstraintsBatchImpl m_BatchImpl;

        public ObiAerodynamicConstraintsBatch(ObiAerodynamicConstraintsData constraints = null)
        {
        }

        public override Oni.ConstraintType constraintType => Oni.ConstraintType.Aerodynamics;

        public override IConstraintsBatchImpl implementation => m_BatchImpl;

        public void AddConstraint(int index, float area, float drag, float lift)
        {
            RegisterConstraint();

            particleIndices.Add(index);
            aerodynamicCoeffs.Add(area);
            aerodynamicCoeffs.Add(drag);
            aerodynamicCoeffs.Add(lift);
        }

        public override void GetParticlesInvolved(int index, List<int> particles)
        {
            particles.Add(particleIndices[index]);
        }

        public override void Clear()
        {
            base.Clear();
            particleIndices.Clear();
            aerodynamicCoeffs.Clear();
        }

        protected override void SwapConstraints(int sourceIndex, int destIndex)
        {
            particleIndices.Swap(sourceIndex, destIndex);
            aerodynamicCoeffs.Swap(sourceIndex * 3, destIndex * 3);
            aerodynamicCoeffs.Swap(sourceIndex * 3 + 1, destIndex * 3 + 1);
            aerodynamicCoeffs.Swap(sourceIndex * 3 + 2, destIndex * 3 + 2);
        }

        public override void Merge(ObiActor actor, IObiConstraintsBatch other)
        {
            var batch = other as ObiAerodynamicConstraintsBatch;
            var user = actor as IAerodynamicConstraintsUser;

            if (batch != null && user != null)
            {
                if (!user.aerodynamicsEnabled)
                    return;

                particleIndices.ResizeUninitialized(m_ActiveConstraintCount + batch.activeConstraintCount);
                aerodynamicCoeffs.ResizeUninitialized((m_ActiveConstraintCount + batch.activeConstraintCount) * 3);

                for (var i = 0; i < batch.activeConstraintCount; ++i)
                {
                    particleIndices[m_ActiveConstraintCount + i] = actor.solverIndices[batch.particleIndices[i]];
                    aerodynamicCoeffs[(m_ActiveConstraintCount + i) * 3] = batch.aerodynamicCoeffs[i * 3];
                    aerodynamicCoeffs[(m_ActiveConstraintCount + i) * 3 + 1] = user.drag;
                    aerodynamicCoeffs[(m_ActiveConstraintCount + i) * 3 + 2] = user.lift;
                }

                base.Merge(actor, other);
            }
        }

        public override void AddToSolver(ObiSolver solver)
        {
            // Create distance constraints batch directly.
            m_BatchImpl =
                solver.implementation.CreateConstraintsBatch(constraintType) as IAerodynamicConstraintsBatchImpl;

            if (m_BatchImpl != null)
                m_BatchImpl.SetAerodynamicConstraints(particleIndices, aerodynamicCoeffs, m_ActiveConstraintCount);
        }

        public override void RemoveFromSolver(ObiSolver solver)
        {
            //Remove batch:
            solver.implementation.DestroyConstraintsBatch(m_BatchImpl);
        }
    }
}