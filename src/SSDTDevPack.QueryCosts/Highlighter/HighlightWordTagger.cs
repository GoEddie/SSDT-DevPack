using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
/*
 This is basiclaly the sample from the vssdk on how to highlight stuff with a few of my bits thrown in 
 */
namespace SSDTDevPack.QueryCosts.Highlighter
{
    internal class HighlightWordTagger : ITagger<TextMarkerTag>
    {
        private readonly object updateLock = new object();

        public HighlightWordTagger(ITextView view, ITextBuffer sourceBuffer, ITextSearchService textSearchService,
            ITextStructureNavigator textStructureNavigator)
        {
            View = view;
            SourceBuffer = sourceBuffer;
            TextSearchService = textSearchService;
            TextStructureNavigator = textStructureNavigator;
            RedWordSpans = new NormalizedSnapshotSpanCollection();
            YellowWordSpans = new NormalizedSnapshotSpanCollection();
            CurrentWord = null;
            View.Caret.PositionChanged += CaretPositionChanged;
            View.LayoutChanged += ViewLayoutChanged;
        }

        private ITextView View { get; set; }
        private ITextBuffer SourceBuffer { get; set; }
        private ITextSearchService TextSearchService { get; set; }
        private ITextStructureNavigator TextStructureNavigator { get; set; }
        private NormalizedSnapshotSpanCollection YellowWordSpans { get; set; }
        private NormalizedSnapshotSpanCollection RedWordSpans { get; set; }
        private SnapshotSpan? CurrentWord { get; set; }
        private SnapshotPoint RequestedPoint { get; set; }
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public IEnumerable<ITagSpan<TextMarkerTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
                if (CurrentWord == null)
                    yield break;

                // Hold on to a "snapshot" of the word spans and current word, so that we maintain the same
                // collection throughout
                var currentWord = CurrentWord.Value;
                var wordSpans = YellowWordSpans;

                if (wordSpans.Count > 0)
                {
                    if (spans.Count == 0 || wordSpans.Count == 0)
                        yield break;

                    // If the requested snapshot isn't the same as the one our words are on, translate our spans to the expected snapshot 
                    if (spans[0].Snapshot != wordSpans[0].Snapshot)
                    {
                        wordSpans = new NormalizedSnapshotSpanCollection(
                            wordSpans.Select(span => span.TranslateTo(spans[0].Snapshot, SpanTrackingMode.EdgeExclusive)));

                        currentWord = currentWord.TranslateTo(spans[0].Snapshot, SpanTrackingMode.EdgeExclusive);
                    }

                    foreach (var span in NormalizedSnapshotSpanCollection.Overlap(spans, wordSpans))
                    {
                        yield return new TagSpan<TextMarkerTag>(span, new LowlightWordTag());
                    }
                }
                wordSpans = RedWordSpans;

                if (spans.Count == 0 || wordSpans.Count == 0)
                    yield break;

                // If the requested snapshot isn't the same as the one our words are on, translate our spans to the expected snapshot 
                if (spans[0].Snapshot != wordSpans[0].Snapshot)
                {
                    wordSpans = new NormalizedSnapshotSpanCollection(
                        wordSpans.Select(span => span.TranslateTo(spans[0].Snapshot, SpanTrackingMode.EdgeExclusive)));

                    currentWord = currentWord.TranslateTo(spans[0].Snapshot, SpanTrackingMode.EdgeExclusive);
                }

                foreach (var span in NormalizedSnapshotSpanCollection.Overlap(spans, wordSpans))
                {
                    yield return new TagSpan<TextMarkerTag>(span, new HighlightWordTag());
                }
            
        }

        private void ViewLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            try { 
            // If a new snapshot wasn't generated, then skip this layout 
            if (e.NewSnapshot != e.OldSnapshot)
            {
                UpdateAtCaretPosition(View.Caret.Position);
            }
            }
            catch (Exception ec)
            {
                MessageBox.Show("1 - e : " + ec.Message + " \r\n " + ec.StackTrace);
            }
        }

        private void CaretPositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            UpdateAtCaretPosition(e.NewPosition);
        }

        private void UpdateAtCaretPosition(CaretPosition caretPosition)
        {
            var point = caretPosition.Point.GetPoint(SourceBuffer, caretPosition.Affinity);

            if (!point.HasValue)
                return;

            // If the new caret position is still within the current word (and on the same snapshot), we don't need to check it 
            if (CurrentWord.HasValue
                && CurrentWord.Value.Snapshot == View.TextSnapshot
                && point.Value >= CurrentWord.Value.Start
                && point.Value <= CurrentWord.Value.End)
            {
                return;
            }

            RequestedPoint = point.Value;
            UpdateWordAdornments();
        }

        private void UpdateWordAdornments()
        {
            //Find the new spans
            try
            {
                var store = DocumentScriptCosters.GetInstance().GetCoster();
                var yellowWordSpans = new List<SnapshotSpan>();
                var redWordSpans = new List<SnapshotSpan>();

                if (null != store)
                {
                    var statements = store.GetCosts();
                    if (statements != null)
                    {
                        foreach (var s in statements)
                        {
                            if (s.Band == CostBand.Medium)
                            {
                                var findData = new FindData(s.Text, RequestedPoint.Snapshot);
                                findData.FindOptions = FindOptions.Multiline | FindOptions.Wrap;
                                yellowWordSpans.AddRange(TextSearchService.FindAll(findData));
                            }

                            if (s.Band == CostBand.High)
                            {
                                var findData = new FindData(s.Text, RequestedPoint.Snapshot);
                                findData.FindOptions = FindOptions.Multiline | FindOptions.Wrap;
                                redWordSpans.AddRange(TextSearchService.FindAll(findData));
                            }
                        }
                    }
                }

                var currentRequest = RequestedPoint;
                var word = TextStructureNavigator.GetExtentOfWord(currentRequest);
                var currentWord = word.Span;


                //If another change hasn't happened, do a real update 
                if (currentRequest == RequestedPoint)
                {
                    SynchronousUpdate(currentRequest, new NormalizedSnapshotSpanCollection(yellowWordSpans),
                        new NormalizedSnapshotSpanCollection(redWordSpans), currentWord);
                }
            }
        
            catch (Exception e)
            {
                MessageBox.Show("2 - e : " + e.Message + " \r\n " + e.StackTrace);
            }
        }

        private void SynchronousUpdate(SnapshotPoint currentRequest, NormalizedSnapshotSpanCollection newYellowSpans,
            NormalizedSnapshotSpanCollection newRedSpans, SnapshotSpan? newCurrentWord)
        {
            lock (updateLock)
            {
                if (currentRequest != RequestedPoint)
                    return;

                YellowWordSpans = newYellowSpans;
                RedWordSpans = newRedSpans;

                CurrentWord = newCurrentWord;

                var tempEvent = TagsChanged;
                if (tempEvent != null)
                    tempEvent(this,
                        new SnapshotSpanEventArgs(new SnapshotSpan(SourceBuffer.CurrentSnapshot, 0,
                            SourceBuffer.CurrentSnapshot.Length)));
            }
        }
    }
}