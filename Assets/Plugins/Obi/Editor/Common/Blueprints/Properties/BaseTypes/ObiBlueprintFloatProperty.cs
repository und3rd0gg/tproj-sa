using UnityEditor;
using UnityEngine;

namespace Obi
{
    public abstract class ObiBlueprintFloatProperty : ObiBlueprintProperty<float>
    {
        public bool autoRange = true;
        public ObiActorBlueprintEditor editor;
        protected float maxUserVisualizationValue = 10;
        protected float? maxValue;
        public float maxVisualizationValue = 10;
        protected float minUserVisualizationValue;

        protected float? minValue;
        public float minVisualizationValue;

        public ObiBlueprintFloatProperty(ObiActorBlueprintEditor editor, float? minValue = null, float? maxValue = null)
        {
            this.editor = editor;
            this.minValue = minValue;
            this.maxValue = maxValue;
        }

        public override bool Equals(int firstIndex, int secondIndex)
        {
            var v1 = Get(firstIndex);
            var v2 = Get(secondIndex);
            if (v1 == v2) return true;
            return Mathf.Approximately(v1, v2);
        }

        public override void PropertyField()
        {
            EditorGUI.BeginChangeCheck();
            value = EditorGUILayout.FloatField(name, value);
            if (EditorGUI.EndChangeCheck())
            {
                if (minValue.HasValue)
                    value = Mathf.Max(minValue.Value, value);
                if (maxValue.HasValue)
                    value = Mathf.Min(maxValue.Value, value);
            }
        }

        public override void RecalculateMinMax()
        {
            if (editor != null && autoRange)
            {
                maxVisualizationValue = float.MinValue;
                minVisualizationValue = float.MaxValue;

                for (var i = 0; i < editor.blueprint.activeParticleCount; i++)
                {
                    var v = Get(i);
                    maxVisualizationValue = Mathf.Max(maxVisualizationValue, v);
                    minVisualizationValue = Mathf.Min(minVisualizationValue, v);
                }
            }
            else
            {
                maxVisualizationValue = maxUserVisualizationValue;
                minVisualizationValue = minUserVisualizationValue;
            }
        }

        public override void VisualizationOptions()
        {
            EditorGUI.BeginChangeCheck();
            autoRange = EditorGUILayout.Toggle("Automatic property range", autoRange);
            GUI.enabled = !autoRange;
            EditorGUI.indentLevel++;
            minUserVisualizationValue = EditorGUILayout.FloatField("Min", minUserVisualizationValue);
            maxUserVisualizationValue = EditorGUILayout.FloatField("Max", maxUserVisualizationValue);
            EditorGUI.indentLevel--;
            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                RecalculateMinMax();
                editor.Refresh();
            }
        }

        public override Color ToColor(int index)
        {
            var gradient = ObiEditorSettings.GetOrCreateSettings().propertyGradient;
            if (!Mathf.Approximately(minVisualizationValue, maxVisualizationValue))
                return gradient.Evaluate(Mathf.InverseLerp(minVisualizationValue, maxVisualizationValue, Get(index)));
            return gradient.Evaluate(0);
        }
    }
}