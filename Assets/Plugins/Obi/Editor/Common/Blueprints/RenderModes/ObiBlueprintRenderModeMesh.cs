using UnityEditor;

namespace Obi
{
    public class ObiBlueprintRenderModeMesh : ObiBlueprintRenderMode
    {
        public ObiBlueprintRenderModeMesh(ObiMeshBasedActorBlueprintEditor editor) : base(editor)
        {
        }

        public override string name => "Mesh";

        public ObiMeshBasedActorBlueprintEditor meshBasedEditor => editor as ObiMeshBasedActorBlueprintEditor;

        public override void OnSceneRepaint(SceneView sceneView)
        {
            if (meshBasedEditor.currentTool is ObiPaintBrushEditorTool)
            {
                var paintTool = (ObiPaintBrushEditorTool) meshBasedEditor.currentTool;

                var weights = new float[editor.selectionStatus.Length];
                for (var i = 0; i < weights.Length; i++)
                    if (paintTool.selectionMask && !editor.selectionStatus[i])
                        weights[i] = 0;
                    else
                        weights[i] = 1;

                var wireframeWeights = new float[paintTool.paintBrush.weights.Length];
                for (var i = 0; i < wireframeWeights.Length; i++)
                    wireframeWeights[i] = paintTool.paintBrush.weights[i];

                meshBasedEditor.DrawGradientMesh(weights, wireframeWeights);
            }
            else
            {
                meshBasedEditor.DrawGradientMesh();
            }
        }
    }
}