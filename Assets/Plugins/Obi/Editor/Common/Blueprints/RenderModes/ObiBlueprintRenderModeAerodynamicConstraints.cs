using UnityEditor;
using UnityEngine;

namespace Obi
{
    public class ObiBlueprintRenderModeAerodynamicConstraints : ObiBlueprintRenderMode
    {
        public ObiBlueprintRenderModeAerodynamicConstraints(ObiMeshBasedActorBlueprintEditor editor) : base(editor)
        {
        }

        public override string name => "Aerodynamic constraints";

        public ObiMeshBasedActorBlueprintEditor meshBasedEditor => editor as ObiMeshBasedActorBlueprintEditor;

        public override void OnSceneRepaint(SceneView sceneView)
        {
            var meshEditor = editor as ObiMeshBasedActorBlueprintEditor;
            if (meshEditor != null)
            {
                // Get per-particle normals:
                var normals = meshEditor.sourceMesh.normals;
                var particleNormals = new Vector3[meshEditor.blueprint.particleCount];
                for (var i = 0; i < normals.Length; ++i)
                {
                    var welded = meshEditor.VertexToParticle(i);
                    particleNormals[welded] = normals[i];
                }

                using (new Handles.DrawingScope(Color.blue, Matrix4x4.identity))
                {
                    var constraints =
                        editor.blueprint.GetConstraintsByType(Oni.ConstraintType.Aerodynamics) as
                            ObiConstraints<ObiAerodynamicConstraintsBatch>;
                    if (constraints != null)
                    {
                        var lines = new Vector3[constraints.GetActiveConstraintCount() * 2];
                        var lineIndex = 0;

                        foreach (var batch in constraints.batches)
                            for (var i = 0; i < batch.activeConstraintCount; ++i)
                            {
                                var particleIndex = batch.particleIndices[i];
                                var position = editor.blueprint.GetParticlePosition(particleIndex);
                                lines[lineIndex++] = position;
                                lines[lineIndex++] = position + particleNormals[particleIndex] * 0.025f;
                            }

                        Handles.DrawLines(lines);
                    }
                }
            }
        }
    }
}