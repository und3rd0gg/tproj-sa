using UnityEditor;
using UnityEngine;

namespace Obi
{
    [CustomPropertyDrawer(typeof(Oni.ConstraintParameters))]
    public class ObiConstraintParametersDrawer : PropertyDrawer
    {
        public static float padding = 4;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var propHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            EditorGUI.BeginProperty(position, label, property);

            var enabled = property.FindPropertyRelative("enabled");
            var contRect = new Rect(position.x + padding, position.y + padding, position.width - padding * 2,
                propHeight);

            // Draw a box around the parameters:
            GUI.enabled = enabled.boolValue;
            GUI.Box(position, "", ObiEditorUtils.GetToggleablePropertyGroupStyle());
            GUI.enabled = true;

            // Draw main constraint toggle:
            enabled.boolValue = EditorGUI.ToggleLeft(contRect, label.text, enabled.boolValue, EditorStyles.boldLabel);

            if (enabled.boolValue)
            {
                var evalRect = new Rect(position.x + padding, position.y + propHeight + padding,
                    position.width - padding * 2, propHeight);
                var iterRect = new Rect(position.x + padding, position.y + propHeight * 2 + padding,
                    position.width - padding * 2, propHeight);
                var sorRect = new Rect(position.x + padding, position.y + propHeight * 3 + padding,
                    position.width - padding * 2, EditorGUIUtility.singleLineHeight);

                EditorGUI.indentLevel++;
                var evalCtrl = EditorGUI.PrefixLabel(evalRect, new GUIContent("Evaluation"));
                EditorGUI.PropertyField(evalCtrl, property.FindPropertyRelative("evaluationOrder"), GUIContent.none);

                var iterCtrl = EditorGUI.PrefixLabel(iterRect, new GUIContent("Iterations"));
                EditorGUI.PropertyField(iterCtrl, property.FindPropertyRelative("iterations"), GUIContent.none);

                var sorCtrl = EditorGUI.PrefixLabel(sorRect, new GUIContent("Relaxation"));
                EditorGUI.PropertyField(sorCtrl, property.FindPropertyRelative("SORFactor"), GUIContent.none);
                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var enabled = property.FindPropertyRelative("enabled");
            if (enabled.boolValue)
                return EditorGUIUtility.singleLineHeight * 4 + EditorGUIUtility.standardVerticalSpacing * 3 +
                       padding * 2;
            return EditorGUIUtility.singleLineHeight + padding * 2;
        }
    }
}