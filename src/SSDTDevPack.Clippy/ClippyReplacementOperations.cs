using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using SSDTDevPack.Common.UserMessages;
using SSDTDevPack.Rewriter;

namespace SSDTDevPack.Clippy
{
    class ClippyReplacementOperations : ClippyOperation
    {
        private ITextSnapshot _snapshot;
        private readonly List<Replacements> _replacements;

        public ClippyReplacementOperations(List<Replacements> replacement)
        {
            _replacements = replacement;
        }

        public void Configure(ITextSnapshot snapshot)
        {
            _snapshot = snapshot;
        }

        public override void DoOperation(GlyphDefinition glyph)
        {
            foreach (var replacement in _replacements)
            {
                try
                {
                    var span = _snapshot.CreateTrackingSpan(replacement.OriginalOffset, replacement.OriginalLength, SpanTrackingMode.EdgeNegative).GetSpan(_snapshot);

                    if (span.GetText() != replacement.Original)
                        return;

                    var newSpan = span.Snapshot.CreateTrackingSpan(span.Start, replacement.OriginalLength, SpanTrackingMode.EdgeNegative);
                    
                    _snapshot.TextBuffer.Replace(newSpan.GetSpan(newSpan.TextBuffer.CurrentSnapshot), replacement.Replacement);
                }
                catch (Exception e)
                {
                    OutputPane.WriteMessage("error unable to do replacement : {0}", e);
                }
            }
        }
    }
}