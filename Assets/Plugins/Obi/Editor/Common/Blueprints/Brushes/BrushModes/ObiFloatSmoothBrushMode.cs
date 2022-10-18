using UnityEngine;

namespace Obi
{
    public class ObiFloatSmoothBrushMode : IObiBrushMode
    {
        private readonly ObiBlueprintFloatProperty property;

        public ObiFloatSmoothBrushMode(ObiBlueprintFloatProperty property)
        {
            this.property = property;
        }

        public string name => "Smooth";

        public bool needsInputValue => false;

        public void ApplyStamps(ObiBrushBase brush, bool modified)
        {
            var floatProperty = property;

            float averageValue = 0;
            float totalWeight = 0;

            for (var i = 0; i < brush.weights.Length; ++i)
                if (!property.Masked(i) && brush.weights[i] > 0)
                {
                    averageValue += floatProperty.Get(i) * brush.weights[i];
                    totalWeight += brush.weights[i];
                }

            averageValue /= totalWeight;

            for (var i = 0; i < brush.weights.Length; ++i)
                if (!property.Masked(i) && brush.weights[i] > 0)
                {
                    var currentValue = floatProperty.Get(i);
                    var delta = brush.opacity * brush.speed *
                                (Mathf.Lerp(currentValue, averageValue, brush.weights[i]) - currentValue);

                    floatProperty.Set(i, currentValue + delta * (modified ? -1 : 1));
                }
        }
    }
}