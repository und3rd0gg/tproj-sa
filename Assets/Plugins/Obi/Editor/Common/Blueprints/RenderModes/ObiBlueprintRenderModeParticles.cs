using UnityEngine;

namespace Obi
{
    public class ObiBlueprintRenderModeParticles : ObiBlueprintRenderMode
    {
        private readonly ParticleImpostorRendering impostorDrawer;
        private Material material;

        private Shader shader;

        public ObiBlueprintRenderModeParticles(ObiActorBlueprintEditor editor) : base(editor)
        {
            impostorDrawer = new ParticleImpostorRendering();
            impostorDrawer.UpdateMeshes(editor.blueprint);
        }

        public override string name => "Particles";

        private void CreateMaterialIfNeeded()
        {
            if (shader == null)
            {
                shader = Shader.Find("Obi/Particles");
                if (shader != null)
                {
                    if (!shader.isSupported)
                        Debug.LogWarning("Particle rendering shader not suported.");

                    if (material == null || material.shader != shader)
                    {
                        Object.DestroyImmediate(material);
                        material = new Material(shader);
                        material.hideFlags = HideFlags.HideAndDontSave;
                    }
                }
            }
        }

        public override void DrawWithCamera(Camera camera)
        {
            CreateMaterialIfNeeded();
            foreach (var mesh in impostorDrawer.Meshes)
                Graphics.DrawMesh(mesh, Matrix4x4.identity, material, 0, camera);
        }

        public override void Refresh()
        {
            impostorDrawer.UpdateMeshes(editor.blueprint, editor.visible, editor.tint);
        }

        public override void OnDestroy()
        {
            Object.DestroyImmediate(material);
            impostorDrawer.ClearMeshes();
        }
    }
}