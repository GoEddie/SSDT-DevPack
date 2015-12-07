using System;
using Microsoft.VisualStudio.Text;
using SSDTDevPack.Common.UserMessages;
using SSDTDevPack.Rewriter;

namespace SSDTDevPack.Clippy
{
    class ClippyReplacementOperation : ClippyOperation
    {
        private ITextSnapshot _snapshot;
        private Replacements _replacement;

        public ClippyReplacementOperation(Replacements replacement)
        {
            _replacement = replacement;
        }

        public void Configure(ITextSnapshot snapshot)
        {
            _snapshot = snapshot;
        }


        public override void DoOperation(GlyphDefinition glyph)
        {
            try
            {
                var span = glyph.Tag.ParentTag.Span;
                var offset = span.GetText().IndexOf(_replacement.Original);

                if (span.GetText().Substring(offset, _replacement.OriginalLength) != _replacement.Original)
                    return;

                var newSpan = span.Snapshot.CreateTrackingSpan(glyph.Tag.ParentTag.Span.Start+offset, _replacement.OriginalLength, SpanTrackingMode.EdgeNegative);
                
                _snapshot.TextBuffer.Replace(newSpan.GetSpan(newSpan.TextBuffer.CurrentSnapshot), _replacement.Replacement);
            }
            catch (Exception e)
            {
                OutputPane.WriteMessage("error unable to do replacement : {0}", e);
            }
        }
    }
}