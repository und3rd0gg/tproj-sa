namespace Obi
{
    public class ObiSelectBrushMode : IObiBrushMode
    {
        private readonly ObiBlueprintSelected property;

        public ObiSelectBrushMode(ObiBlueprintSelected property, string customName = "Select")
        {
            this.property = property;
            this.name = customName;
        }

        public string name { get; }

        public bool needsInputValue => true;

        public void ApplyStamps(ObiBrushBase brush, bool modified)
        {
            for (var i = 0; i < brush.weights.Length; ++i)
                if (brush.weights[i] > 0 && !property.Masked(i))
                    property.Set(i, !modified);
        }
    }
}