using UnityEditor;
using UnityEngine;

namespace Obi
{
    public class ObiBlueprintRenderModeDistanceConstraints : ObiBlueprintRenderMode
    {
        public ObiBlueprintRenderModeDistanceConstraints(ObiActorBlueprintEditor editor) : base(editor)
        {
        }

        public override string name => "Distance constraints";

        public override void OnSceneRepaint(SceneView sceneView)
        {
            using (new Handles.DrawingScope(Color.green, Matrix4x4.identity))
            {
                var constraints =
                    editor.blueprint.GetConstraintsByType(Oni.ConstraintType.Distance) as
                        ObiConstraints<ObiDistanceConstraintsBatch>;
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