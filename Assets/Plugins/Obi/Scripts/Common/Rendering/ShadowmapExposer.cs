using UnityEngine;
using UnityEngine.Rendering;

namespace Obi
{
    public class ShadowmapExposer : MonoBehaviour
    {
        public ObiParticleRenderer[] particleRenderers;
        private CommandBuffer afterShadow;
        private Light unityLight;

        public void Awake()
        {
            unityLight = GetComponent<Light>();
        }

        // Use this for initialization
        private void Update()
        {
            var act = gameObject.activeInHierarchy && enabled;
            if (!act || particleRenderers == null || particleRenderers.Length == 0)
            {
                Cleanup();
                return;
            }

            if (afterShadow != null) SetupFluidShadowsCommandBuffer();
        }

        public void OnEnable()
        {
            Cleanup();

            afterShadow = new CommandBuffer();
            afterShadow.name = "FluidShadows";
            unityLight.AddCommandBuffer(LightEvent.AfterShadowMapPass, afterShadow);
        }

        public void OnDisable()
        {
            Cleanup();
        }

        private void Cleanup()
        {
            if (afterShadow != null)
            {
                unityLight.RemoveCommandBuffer(LightEvent.AfterShadowMapPass, afterShadow);
                afterShadow = null;
            }
        }


        public void SetupFluidShadowsCommandBuffer()
        {
            afterShadow.Clear();

            if (particleRenderers == null)
                return;

            foreach (var renderer in particleRenderers)
                if (renderer != null)
                    foreach (var mesh in renderer.ParticleMeshes)
                        afterShadow.DrawMesh(mesh, Matrix4x4.identity, renderer.ParticleMaterial, 0, 1);

            afterShadow.SetGlobalTexture("_MyShadowMap",
                new RenderTargetIdentifier(BuiltinRenderTextureType.CurrentActive));
        }
    }
}