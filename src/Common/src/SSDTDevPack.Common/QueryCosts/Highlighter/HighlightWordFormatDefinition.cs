using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace SSDTDevPack.QueryCosts.Highlighter
{
    [Export(typeof(EditorFormatDefinition))]
    [Name("QueryCostFormatDefinition/HighlightWordFormatDefinition")]
    [UserVisible(true)]
    class HighlightWordFormatDefinition : MarkerFormatDefinition
    {

        public HighlightWordFormatDefinition()
        {
            var brush = new SolidColorBrush();
            brush.Color = Colors.Red;
            brush.Opacity = 0.45;

            this.Fill = brush;
            
            this.ForegroundColor = Colors.DarkBlue;
            this.DisplayName = "Highlight Word";
            this.ZOrder = 5;
        }
    }
}