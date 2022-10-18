using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Obi
{
    [CustomEditor(typeof(ObiMeshBasedActorBlueprint), true)]
    public abstract class ObiMeshBasedActorBlueprintEditor : ObiActorBlueprintEditor
    {
        [Flags]
        public enum ParticleCulling
        {
            Off = 0,
            Back = 1 << 0,
            Front = 1 << 1
        }

        public ParticleCulling particleCulling = ParticleCulling.Back;

        protected Material gradientMaterial;
        protected Material paddingMaterial;
        protected Material textureExportMaterial;

        protected Mesh visualizationMesh;

        public abstract Mesh sourceMesh { get; }

        public override void OnEnable()
        {
            base.OnEnable();
            gradientMaterial = Resources.Load<Material>("PropertyGradientMaterial");
            textureExportMaterial = Resources.Load<Material>("UVSpaceColorMaterial");
            paddingMaterial = Resources.Load<Material>("PaddingMaterial");
        }

        protected void NonReadableMeshWarning(Mesh mesh)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            var icon = EditorGUIUtility.Load("icons/console.erroricon.png") as Texture2D;
            EditorGUILayout.LabelField(
                new GUIContent(
                    "The input mesh is not readable. Read/Write must be enabled in the mesh import settings.", icon),
                EditorStyles.wordWrappedMiniLabel);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Fix now", GUILayout.MaxWidth(100), GUILayout.MinHeight(32)))
            {
                var assetPath = AssetDatabase.GetAssetPath(mesh);
                var modelImporter = AssetImporter.GetAtPath(assetPath) as ModelImporter;
                if (modelImporter != null) modelImporter.isReadable = true;
                modelImporter.SaveAndReimport();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        protected override bool ValidateBlueprint()
        {
            if (sourceMesh != null)
            {
                if (!sourceMesh.isReadable)
                {
                    NonReadableMeshWarning(sourceMesh);
                    return false;
                }

                return true;
            }

            return false;
        }

        public abstract int VertexToParticle(int vertexIndex);

        public override void UpdateParticleVisibility()
        {
            if (sourceMesh != null && Camera.current != null)
            {
                var meshNormals = sourceMesh.normals;
                for (var i = 0; i < sourceMesh.vertexCount; i++)
                {
                    var particle = VertexToParticle(i);

                    if (particle >= 0 && particle < blueprint.positions.Length)
                    {
                        var camToParticle = Camera.current.transform.position - blueprint.positions[particle];

                        sqrDistanceToCamera[particle] = camToParticle.sqrMagnitude;

                        switch (particleCulling)
                        {
                            case ParticleCulling.Off:
                                visible[particle] = true;
                                break;
                            case ParticleCulling.Back:
                                visible[particle] = Vector3.Dot(meshNormals[i], camToParticle) > 0;
                                break;
                            case ParticleCulling.Front:
                                visible[particle] = Vector3.Dot(meshNormals[i], camToParticle) <= 0;
                                break;
                        }
                    }
                }

                if ((renderModeFlags & 1) != 0)
                    Refresh();
            }
        }

        public void DrawGradientMesh(float[] vertexWeights = null, float[] wireframeWeights = null)
        {
            visualizationMesh = Instantiate(sourceMesh);

            if (gradientMaterial.SetPass(0))
            {
                var matrix = Matrix4x4.TRS(Vector3.zero, (blueprint as ObiMeshBasedActorBlueprint).rotation,
                    (blueprint as ObiMeshBasedActorBlueprint).scale);

                var colors = new Color[visualizationMesh.vertexCount];
                for (var i = 0; i < colors.Length; i++)
                {
                    var particle = VertexToParticle(i);
                    if (particle >= 0 && particle < blueprint.particleCount)
                    {
                        float weight = 1;
                        if (vertexWeights != null)
                            weight = vertexWeights[particle];

                        colors[i] = weight * currentProperty.ToColor(particle);
                    }
                    else
                    {
                        colors[i] = Color.gray;
                    }
                }

                visualizationMesh.colors = colors;
                Graphics.DrawMeshNow(visualizationMesh, matrix);

                var wireColor = ObiEditorSettings.GetOrCreateSettings().brushWireframeColor;

                if (gradientMaterial.SetPass(1))
                {
                    for (var i = 0; i < colors.Length; i++)
                    {
                        var particle = VertexToParticle(i);
                        if (particle >= 0 && particle < blueprint.particleCount)
                        {
                            if (wireframeWeights != null)
                                colors[i] = wireColor * wireframeWeights[particle];
                            else
                                colors[i] = wireColor;
                        }
                        else
                        {
                            colors[i] = Color.gray;
                        }
                    }

                    visualizationMesh.colors = colors;
                    GL.wireframe = true;
                    Graphics.DrawMeshNow(visualizationMesh, matrix);
                    GL.wireframe = false;
                }
            }

            DestroyImmediate(visualizationMesh);
        }


        /**
         * Reads particle data from a 2D texture. Can be used to adjust per particle mass, skin radius, etc. using 
         * a texture instead of painting it in the editor. 
         *  
         * Will call onReadProperty once for each particle, passing the particle index and the bilinearly interpolated 
         * color of the texture at its coordinate.
         * 
         * Be aware that, if a particle corresponds to more than
         * one physical vertex and has multiple uv coordinates, 
         * onReadProperty will be called multiple times for that particle.
         */
        public bool ReadParticlePropertyFromTexture(Texture2D source, Action<int, Color> onReadProperty)
        {
            if (source == null || onReadProperty == null)
                return false;

            var uvs = sourceMesh.uv;

            // Iterate over all vertices in the mesh reading back colors from the texture:
            for (var i = 0; i < sourceMesh.vertexCount; ++i)
                try
                {
                    onReadProperty(VertexToParticle(i), source.GetPixelBilinear(uvs[i].x, uvs[i].y));
                }
                catch (UnityException e)
                {
                    Debug.LogException(e);
                    return false;
                }

            return true;
        }

        public bool WriteParticlePropertyToTexture(string path, int width, int height, int padding)
        {
            if (path == null || textureExportMaterial == null || !textureExportMaterial.SetPass(0))
                return false;

            if (visualizationMesh == null) visualizationMesh = Instantiate(sourceMesh);

            var tempRT = RenderTexture.GetTemporary(width, height, 0);
            var paddingRT = RenderTexture.GetTemporary(width, height, 0);

            var old = RenderTexture.active;
            RenderTexture.active = tempRT;

            GL.PushMatrix();
            GL.LoadProjectionMatrix(Matrix4x4.Ortho(0, 1, 0, 1, -1, 1));

            var colors = new Color[sourceMesh.vertexCount];
            for (var i = 0; i < colors.Length; i++)
                colors[i] = currentProperty.ToColor(VertexToParticle(i));

            visualizationMesh.colors = colors;
            Graphics.DrawMeshNow(visualizationMesh, Matrix4x4.identity);

            GL.PopMatrix();

            // Perform padding/edge dilation
            paddingMaterial.SetFloat("_Padding", padding);
            Graphics.Blit(tempRT, paddingRT, paddingMaterial);

            // Read result into our Texture2D.
            RenderTexture.active = paddingRT;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);

            RenderTexture.active = old;
            RenderTexture.ReleaseTemporary(paddingRT);
            RenderTexture.ReleaseTemporary(tempRT);

            var png = texture.EncodeToPNG();
            DestroyImmediate(texture);

            try
            {
                File.WriteAllBytes(path, png);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }

            AssetDatabase.Refresh();

            return true;
        }
    }
}