using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Obi
{
    public class ObiBlueprintRenderModeShapeMatchingConstraints : ObiBlueprintRenderMode
    {
        public ObiBlueprintRenderModeShapeMatchingConstraints(ObiActorBlueprintEditor editor) : base(editor)
        {
        }

        public override string name => "Shape matching clusters";

        public override void OnSceneRepaint(SceneView sceneView)
        {
            using (new Handles.DrawingScope(Color.cyan, Matrix4x4.identity))
            {
                var constraints =
                    editor.blueprint.GetConstraintsByType(Oni.ConstraintType.ShapeMatching) as
                        ObiConstraints<ObiShapeMatchingConstraintsBatch>;
                if (constraints != null)
                {
                    var lines = new List<Vector3>();

                    foreach (var batch in constraints.batches)
                        for (var i = 0; i < batch.activeConstraintCount; ++i)
                        {
                            var first = batch.firstIndex[i];
                            var p1 = editor.blueprint.GetParticlePosition(batch.particleIndices[first]);

                            for (var j = 1; j < batch.numIndices[i]; ++j)
                            {
                                var index = first + j;
                                var p2 = editor.blueprint.GetParticlePosition(batch.particleIndices[index]);

                                lines.Add(p1);
                                lines.Add(p2);
                            }
                        }

                    Handles.DrawLines(lines.ToArray());
                }
            }
        }
    }
}