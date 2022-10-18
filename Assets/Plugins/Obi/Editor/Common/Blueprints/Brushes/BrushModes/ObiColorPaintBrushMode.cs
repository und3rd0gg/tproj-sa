namespace Obi
{
    public class ObiColorPaintBrushMode : IObiBrushMode
    {
        private readonly ObiBlueprintColorProperty property;

        public ObiColorPaintBrushMode(ObiBlueprintColorProperty property)
        {
            this.property = property;
        }

        public string name => "Paint";

        public bool needsInputValue => true;

        public void ApplyStamps(ObiBrushBase brush, bool modified)
        {
            for (var i = 0; i < brush.weights.Length; ++i)
                if (!property.Masked(i) && brush.weights[i] > 0)
                {
                    var currentValue = property.Get(i);
                    var delta = brush.weights[i] * brush.opacity * brush.speed * (property.GetDefault() - currentValue);

                    property.Set(i, currentValue + delta * (modified ? -1 : 1));
                }
        }
    }
}