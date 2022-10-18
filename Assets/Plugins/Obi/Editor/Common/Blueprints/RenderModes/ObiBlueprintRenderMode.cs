using UnityEditor;
using UnityEngine;

namespace Obi
{
    public abstract class ObiBlueprintRenderMode
    {
        protected ObiActorBlueprintEditor editor;

        public ObiBlueprintRenderMode(ObiActorBlueprintEditor editor)
        {
            this.editor = editor;
        }

        public abstract string name { get; }

        public virtual void DrawWithCamera(Camera camera)
        {
        }

        public virtual void OnSceneRepaint(SceneView sceneView)
        {
        }

        public virtual void Refresh()
        {
        }

        public virtual void OnDestroy()
        {
        }
    }
}