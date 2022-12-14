using UnityEditor;
using UnityEngine;

namespace Obi
{
    public abstract class ObiBlueprintIntProperty : ObiBlueprintProperty<int>
    {
        protected int? maxValue;
        protected int? minValue;

        public ObiBlueprintIntProperty(int? minValue = null, int? maxValue = null)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
        }

        public override bool Equals(int firstIndex, int secondIndex)
        {
            return Get(firstIndex) == Get(secondIndex);
        }

        public override void PropertyField()
        {
            EditorGUI.BeginChangeCheck();
            value = EditorGUILayout.IntField(name, value);
            if (EditorGUI.EndChangeCheck())
            {
                if (minValue.HasValue)
                    value = Mathf.Max(minValue.Value, value);
                if (maxValue.HasValue)
                    value = Mathf.Min(maxValue.Value, value);
            }
        }

        public override Color ToColor(int index)
        {
            var colorIndex = Get(index) % ObiUtils.colorAlphabet.Length;
            return ObiUtils.colorAlphabet[colorIndex];
        }
    }
}