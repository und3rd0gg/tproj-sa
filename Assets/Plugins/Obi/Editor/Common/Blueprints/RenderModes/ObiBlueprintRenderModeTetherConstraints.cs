using UnityEditor;
using UnityEngine;

namespace Obi
{
    public class ObiBlueprintRenderModeTetherConstraints : ObiBlueprintRenderMode
    {
        public ObiBlueprintRenderModeTetherConstraints(ObiActorBlueprintEditor editor) : base(editor)
        {
        }

        public override string name => "Tether constraints";

        public override void OnSceneRepaint(SceneView sceneView)
        {
            using (new Handles.DrawingScope(Color.yellow, Matrix4x4.identity))
            {
                var constraints =
                    editor.blueprint.GetConstraintsByType(Oni.ConstraintType.Tether) as
                        ObiConstraints<ObiTetherConstraintsBatch>;
                if (constraints != null)
                {
                    var lines = new Vector3[constraints.GetActiveConstraintCount() * 2];
                    var lineIndex = 0;

                    foreach (var batch in constraints.batches)
                        for (var i = 0; i < batch.activeConstraintCount; ++i)
                        {
                            lines[lineIndex++] = editor.blueprint.GetParticlePosition(batch.particleIndices[i * 2]);
                            lines[lineIndex++] = editor.blueprint.GetParticlePosition(batch.particleIndices[i * 2 + 1]);
                        }

                    Handles.DrawLines(lines);
                }
            }
        }
    }
}