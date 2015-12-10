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
                var span = _snapshot.CreateTrackingSpan(_replacement.OriginalOffset, _replacement.OriginalLength, SpanTrackingMode.EdgeNegative).GetSpan(_snapshot);

                if (span.GetText() != _replacement.Original)
                    return;

                var newSpan = span.Snapshot.CreateTrackingSpan(span.Start, _replacement.OriginalLength, SpanTrackingMode.EdgeNegative);
                
                _snapshot.TextBuffer.Replace(newSpan.GetSpan(newSpan.TextBuffer.CurrentSnapshot), _replacement.Replacement);

                glyph.Tag.Tagger.Reset();
            }
            catch (Exception e)
            {
                OutputPane.WriteMessage("error unable to do replacement : {0}", e);
            }
        }
    }
}