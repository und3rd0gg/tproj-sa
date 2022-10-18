using UnityEditor;
using UnityEngine;

namespace Obi
{
    public abstract class ObiBlueprintMaskProperty : ObiBlueprintIntProperty
    {
        public override void PropertyField()
        {
            value = EditorGUILayout.MaskField(name, value, ObiUtils.categoryNames);
        }

        private int MathMod(int a, int b)
        {
            return (Mathf.Abs(a * b) + a) % b;
        }

        public override Color ToColor(int index)
        {
            var colorIndex = MathMod(Get(index), ObiUtils.colorAlphabet.Length);
            return ObiUtils.colorAlphabet[colorIndex];
        }
    }
}