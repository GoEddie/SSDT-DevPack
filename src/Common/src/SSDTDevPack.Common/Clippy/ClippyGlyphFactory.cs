using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;

namespace SSDTDevPack.Clippy
{

    
    /// <summary>
    ///     This class implements IGlyphFactory, which provides the visual
    ///     element that will appear in the glyph margin.
    /// </summary>
        internal class ClippyGlyphFactory : IGlyphFactory
    {
        

        public UIElement GenerateGlyph(IWpfTextViewLine line, IGlyphTag tag)
        {
            
            // Ensure we can draw a glyph for this marker. 
            if (tag == null || !(tag is ClippyTag))
            {
                return null;
            }

            
            
            var clippy = tag as ClippyTag;
        //    Debug.WriteLine("Creating Glyph: " + clippy.GetDefinition().Line);
            //Debug.WriteLine("Drawing Glyph: {0} - {1}", clippy.GetDefinition().Line, clippy.GetDefinition().Type);
            
            var grid = clippy.GetEllipses();
            //grid.SetValue(Grid.ZIndexProperty, 999);
            ////grid.AddHandler(new RoutedEvent().HandlerType == )
            //grid.AddHandler(Grid.ContextMenuOpeningEvent, new RoutedEventHandler(OnOpen));
            //grid.AddHandler(Grid.PreviewMouseRightButtonUpEvent, new RoutedEventHandler(OnOpen));

            

            return grid;

        }

        public void OnOpen(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("does this fire?");
        }

        
        
    }
}