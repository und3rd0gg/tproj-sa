using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

namespace Obi
{
    [AddComponentMenu("Physics/Obi/Obi Particle Renderer", 1000)]
    [ExecuteInEditMode]
    [RequireComponent(typeof(ObiActor))]
    public class ObiParticleRenderer : MonoBehaviour
    {
        private static ProfilerMarker m_DrawParticlesPerfMarker = new ProfilerMarker("DrawParticles");

        public bool render = true;
        public Shader shader;
        public Color particleColor = Color.white;
        public float radiusScale = 1;
        private ParticleImpostorRendering m_Impostors;

        public IEnumerable<Mesh> ParticleMeshes => impostors.Meshes;

        public ParticleImpostorRendering impostors
        {
            get
            {
                if (m_Impostors == null)
                    m_Impostors = new ParticleImpostorRendering();
                return m_Impostors;
            }
        }

        public Material ParticleMaterial { get; private set; }

        public void OnEnable()
        {
            GetComponent<ObiActor>().OnInterpolate += DrawParticles;
        }

        public void OnDisable()
        {
            GetComponent<ObiActor>().OnInterpolate -= DrawParticles;

            if (m_Impostors != null)
                m_Impostors.ClearMeshes();

            DestroyImmediate(ParticleMaterial);
        }

        private void CreateMaterialIfNeeded()
        {
            if (shader != null)
            {
                if (!shader.isSupported)
                    Debug.LogWarning("Particle rendering shader not suported.");

                if (ParticleMaterial == null || ParticleMaterial.shader != shader)
                {
                    DestroyImmediate(ParticleMaterial);
                    ParticleMaterial = new Material(shader);
                    ParticleMaterial.hideFlags = HideFlags.HideAndDontSave;
                }
            }
        }

        private void DrawParticles(ObiActor actor)
        {
            using (m_DrawParticlesPerfMarker.Auto())
            {
                if (!isActiveAndEnabled || !actor.isActiveAndEnabled || actor.solver == null)
                {
                    impostors.ClearMeshes();
                    return;
                }

                CreateMaterialIfNeeded();

                impostors.UpdateMeshes(actor);

                DrawParticles();
            }
        }

        private void DrawParticles()
        {
            if (ParticleMaterial != null)
            {
                ParticleMaterial.SetFloat("_RadiusScale", radiusScale);
                ParticleMaterial.SetColor("_Color", particleColor);

                // Send the meshes to be drawn:
                if (render)
                {
                    var meshes = ParticleMeshes;
                    foreach (var mesh in meshes)
                        Graphics.DrawMesh(mesh, Matrix4x4.identity, ParticleMaterial, gameObject.layer);
                }
            }
        }
    }
}