using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace Obi
{
    [CustomEditor(typeof(ObiRod))]
    public class ObiRodEditor : Editor
    {
        private ObiRod actor;
        private SerializedProperty bend1Compliance;
        private SerializedProperty bend2Compliance;

        private SerializedProperty bendTwistConstraintsEnabled;

        private SerializedProperty chainConstraintsEnabled;

        private SerializedProperty collisionMaterial;

        private GUIStyle editLabelStyle;
        private SerializedProperty plasticCreep;
        private SerializedProperty plasticYield;

        private SerializedProperty rodBlueprint;
        private SerializedProperty selfCollisions;
        private SerializedProperty shear1Compliance;
        private SerializedProperty shear2Compliance;
        private SerializedProperty stretchCompliance;

        private SerializedProperty stretchShearConstraintsEnabled;
        private SerializedProperty surfaceCollisions;
        private SerializedProperty tightness;
        private SerializedProperty torsionCompliance;

        public void OnEnable()
        {
            actor = (ObiRod) target;

            rodBlueprint = serializedObject.FindProperty("m_RodBlueprint");

            collisionMaterial = serializedObject.FindProperty("m_CollisionMaterial");
            selfCollisions = serializedObject.FindProperty("m_SelfCollisions");
            surfaceCollisions = serializedObject.FindProperty("m_SurfaceCollisions");

            stretchShearConstraintsEnabled = serializedObject.FindProperty("_stretchShearConstraintsEnabled");
            stretchCompliance = serializedObject.FindProperty("_stretchCompliance");
            shear1Compliance = serializedObject.FindProperty("_shear1Compliance");
            shear2Compliance = serializedObject.FindProperty("_shear2Compliance");

            bendTwistConstraintsEnabled = serializedObject.FindProperty("_bendTwistConstraintsEnabled");
            torsionCompliance = serializedObject.FindProperty("_torsionCompliance");
            bend1Compliance = serializedObject.FindProperty("_bend1Compliance");
            bend2Compliance = serializedObject.FindProperty("_bend2Compliance");
            plasticYield = serializedObject.FindProperty("_plasticYield");
            plasticCreep = serializedObject.FindProperty("_plasticCreep");

            chainConstraintsEnabled = serializedObject.FindProperty("_chainConstraintsEnabled");
            tightness = serializedObject.FindProperty("_tightness");
        }

        [MenuItem("GameObject/3D Object/Obi/Obi Rod", false, 301)]
        private static void CreateObiRod(MenuCommand menuCommand)
        {
            var go = new GameObject("Obi Rod", typeof(ObiRod), typeof(ObiRopeExtrudedRenderer));
            ObiEditorUtils.PlaceActorRoot(go, menuCommand);
        }

        private void DoEditButton()
        {
            using (new EditorGUI.DisabledScope(actor.rodBlueprint == null))
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(EditorGUIUtility.labelWidth);
                EditorGUI.BeginChangeCheck();
                var edit = GUILayout.Toggle(ToolManager.activeToolType == typeof(ObiPathEditor),
                    new GUIContent(Resources.Load<Texture2D>("EditCurves")), "Button", GUILayout.MaxWidth(36),
                    GUILayout.MaxHeight(24));
                EditorGUILayout.LabelField("Edit path", editLabelStyle, GUILayout.ExpandHeight(true),
                    GUILayout.MaxHeight(24));
                if (EditorGUI.EndChangeCheck())
                {
                    if (edit)
                        ToolManager.SetActiveTool<ObiPathEditor>();
                    else
                        ToolManager.RestorePreviousPersistentTool();

                    SceneView.RepaintAll();
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        public override void OnInspectorGUI()
        {
            if (editLabelStyle == null)
            {
                editLabelStyle = new GUIStyle(GUI.skin.label);
                editLabelStyle.alignment = TextAnchor.MiddleLeft;
            }

            serializedObject.UpdateIfRequiredOrScript();

            if (actor.rodBlueprint != null && actor.rodBlueprint.path.ControlPointCount < 2)
                actor.rodBlueprint.GenerateImmediate();

            using (new EditorGUI.DisabledScope(ToolManager.activeToolType == typeof(ObiPathEditor)))
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(rodBlueprint, new GUIContent("Blueprint"));
                if (EditorGUI.EndChangeCheck())
                {
                    foreach (var t in targets)
                    {
                        (t as ObiRod).RemoveFromSolver();
                        (t as ObiRod).ClearState();
                    }

                    serializedObject.ApplyModifiedProperties();
                    foreach (var t in targets)
                        (t as ObiRod).AddToSolver();
                }
            }

            DoEditButton();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Collisions", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(collisionMaterial, new GUIContent("Collision material"));
            EditorGUILayout.PropertyField(selfCollisions, new GUIContent("Self collisions"));
            EditorGUILayout.PropertyField(surfaceCollisions, new GUIContent("Surface-based collisions"));

            EditorGUILayout.Space();
            ObiEditorUtils.DoToggleablePropertyGroup(stretchShearConstraintsEnabled,
                new GUIContent("Stretch & Shear Constraints",
                    Resources.Load<Texture2D>("Icons/ObiStretchShearConstraints Icon")),
                () =>
                {
                    EditorGUILayout.PropertyField(stretchCompliance, new GUIContent("Stretch compliance"));
                    EditorGUILayout.PropertyField(shear1Compliance, new GUIContent("Shear compliance X"));
                    EditorGUILayout.PropertyField(shear2Compliance, new GUIContent("Shear compliance Y"));
                });

            ObiEditorUtils.DoToggleablePropertyGroup(bendTwistConstraintsEnabled,
                new GUIContent("Bend & Twist Constraints",
                    Resources.Load<Texture2D>("Icons/ObiBendTwistConstraints Icon")),
                () =>
                {
                    EditorGUILayout.PropertyField(torsionCompliance, new GUIContent("Torsion compliance"));
                    EditorGUILayout.PropertyField(bend1Compliance, new GUIContent("Bend compliance X"));
                    EditorGUILayout.PropertyField(bend2Compliance, new GUIContent("Bend compliance Y"));
                    EditorGUILayout.PropertyField(plasticYield, new GUIContent("Plastic yield"));
                    EditorGUILayout.PropertyField(plasticCreep, new GUIContent("Plastic creep"));
                });

            ObiEditorUtils.DoToggleablePropertyGroup(chainConstraintsEnabled,
                new GUIContent("Chain Constraints", Resources.Load<Texture2D>("Icons/ObiChainConstraints Icon")),
                () => { EditorGUILayout.PropertyField(tightness, new GUIContent("Tightness")); });


            if (GUI.changed)
                serializedObject.ApplyModifiedProperties();
        }

        [DrawGizmo(GizmoType.Selected)]
        private static void DrawGizmos(ObiRod actor, GizmoType gizmoType)
        {
            Handles.color = Color.white;
            if (actor.rodBlueprint != null)
                ObiPathHandles.DrawPathHandle(actor.rodBlueprint.path, actor.transform.localToWorldMatrix,
                    actor.rodBlueprint.thickness, 20);
        }
    }
}