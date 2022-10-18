using UnityEngine;

namespace Obi
{
    public class ObiAmbientForceZone : ObiExternalForce
    {
        public void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = new Color(0, 0.7f, 1, 1);

            // arrow body:
            ObiUtils.DrawArrowGizmo(0.5f + GetTurbulence(1), 0.2f, 0.3f, 0.2f);
        }

        public override void ApplyForcesToActor(ObiActor actor)
        {
            var l2sTransform = actor.solver.transform.worldToLocalMatrix * transform.localToWorldMatrix;

            Vector4 force = l2sTransform.MultiplyVector(Vector3.forward * (intensity + GetTurbulence(turbulence)));

            if (actor.usesCustomExternalForces)
                for (var i = 0; i < actor.activeParticleCount; ++i)
                    actor.solver.wind[actor.solverIndices[i]] += force;
            else
                for (var i = 0; i < actor.activeParticleCount; ++i)
                    actor.solver.externalForces[actor.solverIndices[i]] += force;
        }
    }
}