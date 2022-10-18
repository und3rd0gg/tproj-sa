using UnityEditor;
using UnityEngine;

namespace Obi
{
    public abstract class ObiBlueprintEditorTool
    {
        protected ObiActorBlueprintEditor editor;
        protected Texture m_Icon;
        protected string m_Name;

        public ObiBlueprintEditorTool(ObiActorBlueprintEditor editor)
        {
            this.editor = editor;
        }

        public string name => m_Name;

        public Texture icon => m_Icon;

        public virtual void OnEnable()
        {
        }

        public virtual void OnDisable()
        {
        }

        public virtual void OnDestroy()
        {
        }

        public virtual string GetHelpString()
        {
            return string.Empty;
        }

        public abstract void OnInspectorGUI();

        public virtual void OnSceneGUI(SceneView sceneView)
        {
        }

        public virtual bool Editable(int index)
        {
            return editor.visible[index];
        }
    }
}