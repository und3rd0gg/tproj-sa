using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

namespace Obi
{
    [AddComponentMenu("Physics/Obi/Obi Instanced Particle Renderer", 1001)]
    [ExecuteInEditMode]
    [RequireComponent(typeof(ObiActor))]
    public class ObiInstancedParticleRenderer : MonoBehaviour
    {
        private static ProfilerMarker m_DrawParticlesPerfMarker = new ProfilerMarker("DrawParticles");

        public bool render = true;
        public Mesh mesh;
        public Material material;
        public Vector3 instanceScale = Vector3.one;
        private int batchCount;
        private readonly List<Vector4> colors = new List<Vector4>();

        private readonly List<Matrix4x4> matrices = new List<Matrix4x4>();

        private int meshesPerBatch;
        private MaterialPropertyBlock mpb;

        public void OnEnable()
        {
            GetComponent<ObiActor>().OnInterpolate += DrawParticles;
        }

        public void OnDisable()
        {
            GetComponent<ObiActor>().OnInterpolate -= DrawParticles;
        }

        private void DrawParticles(ObiActor actor)
        {
            using (m_DrawParticlesPerfMarker.Auto())
            {
                if (mesh == null || material == null || !render || !isActiveAndEnabled || !actor.isActiveAndEnabled ||
                    actor.solver == null) return;

                var solver = actor.solver;

                // figure out the size of our instance batches:
                meshesPerBatch = Constants.maxInstancesPerBatch;
                batchCount = actor.particleCount / meshesPerBatch + 1;
                meshesPerBatch = Mathf.Min(meshesPerBatch, actor.particleCount);

                var basis1 = new Vector4(1, 0, 0, 0);
                var basis2 = new Vector4(0, 1, 0, 0);
                var basis3 = new Vector4(0, 0, 1, 0);

                //Convert particle data to mesh instances:
                for (var i = 0; i < batchCount; i++)
                {
                    matrices.Clear();
                    colors.Clear();
                    mpb = new MaterialPropertyBlock();
                    var limit = Mathf.Min((i + 1) * meshesPerBatch, actor.activeParticleCount);

                    for (var j = i * meshesPerBatch; j < limit; ++j)
                    {
                        var solverIndex = actor.solverIndices[j];
                        actor.GetParticleAnisotropy(solverIndex, ref basis1, ref basis2, ref basis3);
                        matrices.Add(Matrix4x4.TRS(actor.GetParticlePosition(solverIndex),
                            actor.GetParticleOrientation(solverIndex),
                            Vector3.Scale(new Vector3(basis1[3], basis2[3], basis3[3]), instanceScale)));
                        colors.Add(actor.GetParticleColor(solverIndex));
                    }

                    if (colors.Count > 0)
                        mpb.SetVectorArray("_Color", colors);

                    // Send the meshes to be drawn:
                    Graphics.DrawMeshInstanced(mesh, 0, material, matrices, mpb);
                }
            }
        }
    }
}