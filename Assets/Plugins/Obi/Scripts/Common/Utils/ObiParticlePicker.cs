using System;
using UnityEngine;
using UnityEngine.Events;

namespace Obi
{
    public class ObiParticlePicker : MonoBehaviour
    {
        public ObiSolver solver;
        public float radiusScale = 1;

        public ParticlePickUnityEvent OnParticlePicked;
        public ParticlePickUnityEvent OnParticleHeld;
        public ParticlePickUnityEvent OnParticleDragged;
        public ParticlePickUnityEvent OnParticleReleased;

        private Vector3 lastMousePos = Vector3.zero;
        private float pickedParticleDepth;
        private int pickedParticleIndex = -1;

        private void Awake()
        {
            lastMousePos = Input.mousePosition;
        }

        private void LateUpdate()
        {
            if (solver != null)
            {
                // Click:
                if (Input.GetMouseButtonDown(0))
                {
                    pickedParticleIndex = -1;

                    var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                    var closestMu = float.MaxValue;
                    var closestDistance = float.MaxValue;

                    var solver2World = solver.transform.localToWorldMatrix;

                    // Find the closest particle hit by the ray:
                    for (var i = 0; i < solver.renderablePositions.count; ++i)
                    {
                        var worldPos = solver2World.MultiplyPoint3x4(solver.renderablePositions[i]);

                        float mu;
                        var projected = ObiUtils.ProjectPointLine(worldPos, ray.origin, ray.origin + ray.direction,
                            out mu, false);
                        var distanceToRay = Vector3.SqrMagnitude(worldPos - projected);

                        // Disregard particles behind the camera:
                        mu = Mathf.Max(0, mu);

                        var radius = solver.principalRadii[i][0] * radiusScale;

                        if (distanceToRay <= radius * radius && distanceToRay < closestDistance && mu < closestMu)
                        {
                            closestMu = mu;
                            closestDistance = distanceToRay;
                            pickedParticleIndex = i;
                        }
                    }

                    if (pickedParticleIndex >= 0)
                    {
                        pickedParticleDepth = Camera.main.transform
                            .InverseTransformVector(
                                solver2World.MultiplyPoint3x4(solver.renderablePositions[pickedParticleIndex]) -
                                Camera.main.transform.position).z;

                        if (OnParticlePicked != null)
                        {
                            var worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,
                                Input.mousePosition.y, pickedParticleDepth));
                            OnParticlePicked.Invoke(new ParticlePickEventArgs(pickedParticleIndex, worldPosition));
                        }
                    }
                }
                else if (pickedParticleIndex >= 0)
                {
                    // Drag:
                    var mouseDelta = Input.mousePosition - lastMousePos;
                    if (mouseDelta.magnitude > 0.01f && OnParticleDragged != null)
                    {
                        var worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,
                            Input.mousePosition.y, pickedParticleDepth));
                        OnParticleDragged.Invoke(new ParticlePickEventArgs(pickedParticleIndex, worldPosition));
                    }
                    else if (OnParticleHeld != null)
                    {
                        var worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,
                            Input.mousePosition.y, pickedParticleDepth));
                        OnParticleHeld.Invoke(new ParticlePickEventArgs(pickedParticleIndex, worldPosition));
                    }

                    // Release:				
                    if (Input.GetMouseButtonUp(0))
                    {
                        if (OnParticleReleased != null)
                        {
                            var worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,
                                Input.mousePosition.y, pickedParticleDepth));
                            OnParticleReleased.Invoke(new ParticlePickEventArgs(pickedParticleIndex, worldPosition));
                        }

                        pickedParticleIndex = -1;
                    }
                }
            }

            lastMousePos = Input.mousePosition;
        }

        public class ParticlePickEventArgs : EventArgs
        {
            public int particleIndex;
            public Vector3 worldPosition;

            public ParticlePickEventArgs(int particleIndex, Vector3 worldPosition)
            {
                this.particleIndex = particleIndex;
                this.worldPosition = worldPosition;
            }
        }

        [Serializable]
        public class ParticlePickUnityEvent : UnityEvent<ParticlePickEventArgs>
        {
        }
    }
}