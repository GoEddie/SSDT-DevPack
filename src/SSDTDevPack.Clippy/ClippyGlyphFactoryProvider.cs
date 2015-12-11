using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Input;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System.Linq;

namespace SSDTDevPack.Clippy
{
    [Export(typeof (IGlyphFactoryProvider))]
    [Name("ClippyGlyph")]
    [Order(After = "VsTextMarker")]
    [ContentType("code")]
    [TagType(typeof (ClippyTag))]    
    internal sealed class ClippyGlyphFactoryProvider : IGlyphFactoryProvider
    {
        public IGlyphFactory GetGlyphFactory(IWpfTextView view, IWpfTextViewMargin margin)
        {
            return new ClippyGlyphFactory();
        }
    }


    [Export(typeof(IGlyphMouseProcessorProvider))]    
    [Name("ClippyGlyphMouseProcessorProvider")]
    [Order(After = "VsTextMarker")]
    [ContentType("code")]    
    public sealed class GlyphMouseProcessorProvider : IGlyphMouseProcessorProvider

    {
        [Import]
        private IViewTagAggregatorFactoryService ViewTagAggregatorFactoryService { get; set; }

        public IMouseProcessor GetAssociatedMouseProcessor(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin margin)
        {            
            return new ClippyGlyphMouseProcessor(wpfTextViewHost, ViewTagAggregatorFactoryService);

        }
    }

    //taken from https://code.google.com/p/nbehave/source/browse/src/NBehave.VS2010.Plugin/GherkinFileEditor/Glyphs/PlayGlyphMouseProcessorProvider.cs?r=563be75fe78288c27a321e4689a966903d7f042e
    class ClippyGlyphMouseProcessor : IMouseProcessor
    {
        private readonly IWpfTextViewHost _wpfTextViewHost;
        private readonly IViewTagAggregatorFactoryService _viewTagAggregatorFactoryService;
        private ITagAggregator<ClippyTag> _createTagAggregator;

        public ClippyGlyphMouseProcessor(IWpfTextViewHost wpfTextViewHost, IViewTagAggregatorFactoryService viewTagAggregatorFactoryService)
        {
            _wpfTextViewHost = wpfTextViewHost;
            _viewTagAggregatorFactoryService = viewTagAggregatorFactoryService;

            _createTagAggregator = _viewTagAggregatorFactoryService.CreateTagAggregator<ClippyTag>(_wpfTextViewHost.TextView);
        }

        public void PostprocessDragEnter(DragEventArgs e)
        {
           
        }

        public void PostprocessDragLeave(DragEventArgs e)
        {
           
        }

        public void PostprocessDragOver(DragEventArgs e)
        {
        }

        public void PostprocessDrop(DragEventArgs e)
        {
           
        }

        public void PostprocessGiveFeedback(GiveFeedbackEventArgs e)
        {
            
        }

        public void PostprocessMouseDown(MouseButtonEventArgs e)
        {
            
        }

        public void PostprocessMouseEnter(MouseEventArgs e)
        {
            
        }

        public void PostprocessMouseLeave(MouseEventArgs e)
        {
            
        }

        public void PostprocessMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            
        }

        public void PostprocessMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            
        }

        public void PostprocessMouseMove(MouseEventArgs e)
        {
            
        }

        public void PostprocessMouseRightButtonDown(MouseButtonEventArgs e)
        {
            
        }

        public void PostprocessMouseRightButtonUp(MouseButtonEventArgs e)
        {
            IWpfTextView textView = this._wpfTextViewHost.TextView;
            Point position = e.GetPosition(textView.VisualElement);

            var textViewLine =
                textView.TextViewLines.GetTextViewLineContainingYCoordinate(position.Y + textView.ViewportTop);

            var tags = this._createTagAggregator.GetTags(textViewLine.ExtentAsMappingSpan);
            var glyphs = tags.Select(span => span.Tag);
            var tag = glyphs.LastOrDefault();
            if (null == tag)
                return;
                
                tag.ShowMenu();
            //glyphs.First().Execute();
        }

        public void PostprocessMouseUp(MouseButtonEventArgs e)
        {
            
        }

        public void PostprocessMouseWheel(MouseWheelEventArgs e)
        {
            
        }

        public void PostprocessQueryContinueDrag(QueryContinueDragEventArgs e)
        {
            
        }

        public void PreprocessDragEnter(DragEventArgs e)
        {
            
        }

        public void PreprocessDragLeave(DragEventArgs e)
        {
            
        }

        public void PreprocessDragOver(DragEventArgs e)
        {
            
        }

        public void PreprocessDrop(DragEventArgs e)
        {
        
        }

        public void PreprocessGiveFeedback(GiveFeedbackEventArgs e)
        {
        
        }

        public void PreprocessMouseDown(MouseButtonEventArgs e)
        {
        
        }

        public void PreprocessMouseEnter(MouseEventArgs e)
        {
        
        }

        public void PreprocessMouseLeave(MouseEventArgs e)
        {
            
        }

        public void PreprocessMouseLeftButtonDown(MouseButtonEventArgs e)
        {
           
        }

        public void PreprocessMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            
        }

        public void PreprocessMouseMove(MouseEventArgs e)
        {
            
        }

        public void PreprocessMouseRightButtonDown(MouseButtonEventArgs e)
        {
            
        }

        public void PreprocessMouseRightButtonUp(MouseButtonEventArgs e)
        {
            
        }

        public void PreprocessMouseUp(MouseButtonEventArgs e)
        {
            
        }

        public void PreprocessMouseWheel(MouseWheelEventArgs e)
        {
            
        }

        public void PreprocessQueryContinueDrag(QueryContinueDragEventArgs e)
        {
            
        }
    }


}