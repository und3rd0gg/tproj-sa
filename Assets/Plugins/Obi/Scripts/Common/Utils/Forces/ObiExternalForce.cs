using UnityEngine;

namespace Obi
{
    public abstract class ObiExternalForce : MonoBehaviour
    {
        public float intensity;
        public float turbulence;
        public float turbulenceFrequency = 1;
        public float turbulenceSeed;
        public ObiSolver[] affectedSolvers;

        public void OnEnable()
        {
            foreach (var solver in affectedSolvers)
                if (solver != null)
                    solver.OnBeginStep += Solver_OnStepBegin;
        }

        public void OnDisable()
        {
            foreach (var solver in affectedSolvers)
                if (solver != null)
                    solver.OnBeginStep -= Solver_OnStepBegin;
        }

        private void Solver_OnStepBegin(ObiSolver solver, float stepTime)
        {
            foreach (var actor in solver.actors)
                if (actor != null)
                    ApplyForcesToActor(actor);
        }

        protected float GetTurbulence(float turbulenceIntensity)
        {
            return Mathf.PerlinNoise(Time.fixedTime * turbulenceFrequency, turbulenceSeed) * turbulenceIntensity;
        }

        public abstract void ApplyForcesToActor(ObiActor actor);
    }
}