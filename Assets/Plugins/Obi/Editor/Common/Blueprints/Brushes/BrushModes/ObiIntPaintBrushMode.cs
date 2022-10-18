namespace Obi
{
    public class ObiIntPaintBrushMode : IObiBrushMode
    {
        private readonly ObiBlueprintIntProperty property;

        public ObiIntPaintBrushMode(ObiBlueprintIntProperty property)
        {
            this.property = property;
        }

        public string name => "Paint";

        public bool needsInputValue => true;

        public void ApplyStamps(ObiBrushBase brush, bool modified)
        {
            for (var i = 0; i < brush.weights.Length; ++i)
                if (!property.Masked(i) && brush.weights[i] > 1 - brush.opacity)
                    property.Set(i, property.GetDefault());
        }
    }
}