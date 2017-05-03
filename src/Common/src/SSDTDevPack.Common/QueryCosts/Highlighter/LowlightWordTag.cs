using Microsoft.VisualStudio.Text.Tagging;

namespace SSDTDevPack.QueryCosts.Highlighter
{
    public class LowlightWordTag : TextMarkerTag
    {
        public LowlightWordTag() : base("QueryCostFormatDefinition/LowlightWordFormatDefinition") { }
    }
}