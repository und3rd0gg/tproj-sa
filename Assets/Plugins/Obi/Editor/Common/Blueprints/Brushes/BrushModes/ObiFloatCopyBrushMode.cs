namespace Obi
{
    public class ObiFloatCopyBrushMode : IObiBrushMode
    {
        private readonly ObiBlueprintFloatProperty property;
        public ObiBlueprintFloatProperty source;

        public ObiFloatCopyBrushMode(ObiBlueprintFloatProperty property, ObiBlueprintFloatProperty source)
        {
            this.property = property;
            this.source = source;
        }

        public string name => "Copy";

        public bool needsInputValue => false;

        public void ApplyStamps(ObiBrushBase brush, bool modified)
        {
            if (property != null && source != null)
                for (var i = 0; i < brush.weights.Length; ++i)
                    if (!property.Masked(i) && brush.weights[i] > 0)
                    {
                        var currentValue = property.Get(i);
                        var sourceValue = source.Get(i);
                        var delta = brush.weights[i] * brush.opacity * brush.speed * (sourceValue - currentValue);

                        property.Set(i, currentValue + delta * (modified ? -1 : 1));
                    }
        }
    }
}