using Microsoft.VisualStudio.Text.Tagging;

namespace SSDTDevPack.QueryCosts.Highlighter
{
    public class HighlightWordTag : TextMarkerTag
    {
        public HighlightWordTag() : base("QueryCostFormatDefinition/HighlightWordFormatDefinition") { }
    }
}