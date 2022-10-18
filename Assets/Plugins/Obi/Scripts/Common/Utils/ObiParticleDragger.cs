using UnityEngine;

namespace Obi
{
    [RequireComponent(typeof(LineRenderer))]
    [RequireComponent(typeof(ObiParticlePicker))]
    public class ObiParticleDragger : MonoBehaviour
    {
        public float springStiffness = 500;
        public float springDamping = 50;
        public bool drawSpring = true;

        private LineRenderer lineRenderer;
        private ObiParticlePicker.ParticlePickEventArgs pickArgs;
        private ObiParticlePicker picker;

        private void FixedUpdate()
        {
            var solver = picker.solver;

            if (solver != null && pickArgs != null)
            {
                // Calculate picking position in solver space:
                Vector4 targetPosition = solver.transform.InverseTransformPoint(pickArgs.worldPosition);

                // Calculate effective inverse mass:
                var invMass = solver.invMasses[pickArgs.particleIndex];

                if (invMass > 0)
                {
                    // Calculate and apply spring force:
                    var position = solver.positions[pickArgs.particleIndex];
                    var velocity = solver.velocities[pickArgs.particleIndex];
                    solver.externalForces[pickArgs.particleIndex] =
                        ((targetPosition - position) * springStiffness - velocity * springDamping) / invMass;


                    if (drawSpring)
                    {
                        lineRenderer.positionCount = 2;
                        lineRenderer.SetPosition(0, pickArgs.worldPosition);
                        lineRenderer.SetPosition(1, solver.transform.TransformPoint(position));
                    }
                    else
                    {
                        lineRenderer.positionCount = 0;
                    }
                }
            }
        }

        private void OnEnable()
        {
            lineRenderer = GetComponent<LineRenderer>();
            picker = GetComponent<ObiParticlePicker>();
            picker.OnParticlePicked.AddListener(Picker_OnParticleDragged);
            picker.OnParticleDragged.AddListener(Picker_OnParticleDragged);
            picker.OnParticleReleased.AddListener(Picker_OnParticleReleased);
        }

        private void OnDisable()
        {
            picker.OnParticlePicked.RemoveListener(Picker_OnParticleDragged);
            picker.OnParticleDragged.RemoveListener(Picker_OnParticleDragged);
            picker.OnParticleReleased.RemoveListener(Picker_OnParticleReleased);
            lineRenderer.positionCount = 0;
        }

        private void Picker_OnParticleDragged(ObiParticlePicker.ParticlePickEventArgs e)
        {
            pickArgs = e;
        }

        private void Picker_OnParticleReleased(ObiParticlePicker.ParticlePickEventArgs e)
        {
            pickArgs = null;
            lineRenderer.positionCount = 0;
        }
    }
}