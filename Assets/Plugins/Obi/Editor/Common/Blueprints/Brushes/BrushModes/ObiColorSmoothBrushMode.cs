using UnityEngine;

namespace Obi
{
    public class ObiColorSmoothBrushMode : IObiBrushMode
    {
        private readonly ObiBlueprintColorProperty property;

        public ObiColorSmoothBrushMode(ObiBlueprintColorProperty property)
        {
            this.property = property;
        }

        public string name => "Smooth";

        public bool needsInputValue => false;

        public void ApplyStamps(ObiBrushBase brush, bool modified)
        {
            var averageValue = Color.black;
            float totalWeight = 0;

            for (var i = 0; i < brush.weights.Length; ++i)
                if (!property.Masked(i) && brush.weights[i] > 0)
                {
                    averageValue += property.Get(i) * brush.weights[i];
                    totalWeight += brush.weights[i];
                }

            averageValue /= totalWeight;

            for (var i = 0; i < brush.weights.Length; ++i)
                if (!property.Masked(i) && brush.weights[i] > 0)
                {
                    var currentValue = property.Get(i);
                    var delta = brush.opacity * brush.speed *
                                (Color.Lerp(currentValue, averageValue, brush.weights[i]) - currentValue);

                    property.Set(i, currentValue + delta * (modified ? -1 : 1));
                }
        }
    }
}