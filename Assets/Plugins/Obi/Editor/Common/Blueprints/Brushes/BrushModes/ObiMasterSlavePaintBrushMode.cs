namespace Obi
{
    public class ObiMasterSlavePaintBrushMode : IObiBrushMode
    {
        private readonly ObiBlueprintIntProperty property;

        public ObiMasterSlavePaintBrushMode(ObiBlueprintIntProperty property)
        {
            this.property = property;
        }

        public string name => "Master/Slave paint";

        public bool needsInputValue => true;

        public void ApplyStamps(ObiBrushBase brush, bool modified)
        {
            for (var i = 0; i < brush.weights.Length; ++i)
                if (!property.Masked(i) && brush.weights[i] > 1 - brush.opacity)
                {
                    var currentValue = property.Get(i);

                    if (modified)
                        currentValue &= ~(1 << property.GetDefault());
                    else currentValue |= 1 << property.GetDefault();

                    property.Set(i, currentValue);
                }
        }
    }
}