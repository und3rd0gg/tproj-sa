using UnityEditor;
using UnityEngine;

namespace Obi
{
    public class ObiBlueprintRenderModeBendConstraints : ObiBlueprintRenderMode
    {
        public ObiBlueprintRenderModeBendConstraints(ObiActorBlueprintEditor editor) : base(editor)
        {
        }

        public override string name => "Bend constraints";

        public override void OnSceneRepaint(SceneView sceneView)
        {
            using (new Handles.DrawingScope(Color.magenta, Matrix4x4.identity))
            {
                var constraints =
                    editor.blueprint.GetConstraintsByType(Oni.ConstraintType.Bending) as
                        ObiConstraints<ObiBendConstraintsBatch>;
                if (constraints != null)
                {
                    var lines = new Vector3[constraints.GetActiveConstraintCount() * 2];
                    var lineIndex = 0;

                    foreach (var batch in constraints.batches)
                        for (var i = 0; i < batch.activeConstraintCount; ++i)
                        {
                            lines[lineIndex++] = editor.blueprint.GetParticlePosition(batch.particleIndices[i * 3]);
                            lines[lineIndex++] = editor.blueprint.GetParticlePosition(batch.particleIndices[i * 3 + 1]);
                        }

                    Handles.DrawLines(lines);
                }
            }
        }
    }
}