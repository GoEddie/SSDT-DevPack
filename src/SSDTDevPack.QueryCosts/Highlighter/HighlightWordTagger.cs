using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using EnvDTE;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using SSDTDevPack.Common.Dac;
using SSDTDevPack.Common.ScriptDom;
using SSDTDevPack.Common.VSPackage;
using SSDTDevPacl.CodeCoverage.Lib;

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
        protected ITextSearchService TextSearchService { get; set; }
        protected ITextStructureNavigator TextStructureNavigator { get; set; }
        private NormalizedSnapshotSpanCollection YellowWordSpans { get; set; }
        private NormalizedSnapshotSpanCollection RedWordSpans { get; set; }
        private SnapshotSpan? CurrentWord { get; set; }
        protected SnapshotPoint RequestedPoint { get; set; }
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

        protected virtual void UpdateWordAdornments()
        {
            //Find the new spans
            
        }

        protected void SynchronousUpdate(SnapshotPoint currentRequest, NormalizedSnapshotSpanCollection newYellowSpans,
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

    class QueryCostHighlightWordTagger : HighlightWordTagger
    {
        public QueryCostHighlightWordTagger(ITextView view, ITextBuffer sourceBuffer, ITextSearchService textSearchService, ITextStructureNavigator textStructureNavigator) : base(view, sourceBuffer, textSearchService, textStructureNavigator)
        {
        }

        protected override void UpdateWordAdornments()
        {
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
                    SynchronousUpdate(currentRequest, new NormalizedSnapshotSpanCollection(yellowWordSpans), new NormalizedSnapshotSpanCollection(redWordSpans), currentWord);
                }
            }

            catch (Exception e)
            {
                MessageBox.Show("2 - e : " + e.Message + " \r\n " + e.StackTrace);
            }

        }
    }

    public static class CodeCoverageTaggerSettings
    {
        public static bool Enabled = false;

        public static OleMenuCommand MenuItem;

        public static Action ClippyDisabled;
    }

    class CodeCoverageHighlightWordTagger : HighlightWordTagger
    {
        public CodeCoverageHighlightWordTagger(ITextView view, ITextBuffer sourceBuffer, ITextSearchService textSearchService, ITextStructureNavigator textStructureNavigator) : base(view, sourceBuffer, textSearchService, textStructureNavigator)
        {
        }

        protected override void UpdateWordAdornments()
        {
            
            try
            {
                var store = CodeCoverageStore.Get;
                var yellowWordSpans = new List<SnapshotSpan>();
                var redWordSpans = new List<SnapshotSpan>();

                if (null != store && CodeCoverageTaggerSettings.Enabled)
                {
                    var dte = (DTE) VsServiceProvider.Get(typeof (DTE));

                    if (dte.ActiveDocument == null)
                    {
                        return;
                    }

                    var documentKey = string.Format("{0}:{1}", RequestedPoint.Snapshot.Length, RequestedPoint.Snapshot.Version.VersionNumber);

                    var path = dte.ActiveDocument.FullName;

                    var script = File.ReadAllText(path);
                    foreach (var proc in ScriptDom.GetProcedures(script))
                    {
                        var name = proc?.ProcedureReference.Name.ToNameString();

                        if (string.IsNullOrEmpty(name))
                            continue;

                        var statements = store.GetCoveredStatements(name, path);

                        if (statements == null)
                            continue;

                        if (statements.Any(p => p.TimeStamp < File.GetLastWriteTimeUtc(path)))
                            continue;

                        foreach (var statement in statements)
                        {
                            var offset = proc.StartOffset + (int) statement.Offset;
                            if (offset + statement.Length > script.Length)
                                continue; //bad things!

                            if (statement.Length > -1)
                            {
                                var span = new SnapshotSpan(new SnapshotPoint(RequestedPoint.Snapshot, offset), (int) statement.Length+1);
                                yellowWordSpans.Add(span);
                            }
                            else
                            {
                                var span = new SnapshotSpan(new SnapshotPoint(RequestedPoint.Snapshot, offset), (proc.FragmentLength - offset)+1);
                                yellowWordSpans.Add(span);
                            }
                        }
                    }

                    foreach (var proc in ScriptDom.GetFunctions(script))
                    {
                        var name = proc?.Name.ToNameString();

                        if (string.IsNullOrEmpty(name))
                            continue;

                        var statements = store.GetCoveredStatements(name, path);

                        if (statements == null)
                            continue;

                        foreach (var statement in statements)
                        {
                            var offset = proc.StartOffset + (int)statement.Offset;
                            if (offset + statement.Length > script.Length)
                                continue; //bad things!

                            if (statement.Length > -1)
                            {
                                var span = new SnapshotSpan(new SnapshotPoint(RequestedPoint.Snapshot, offset), (int)statement.Length + 1);
                                yellowWordSpans.Add(span);
                            }
                            else
                            {
                                var span = new SnapshotSpan(new SnapshotPoint(RequestedPoint.Snapshot, offset), (proc.FragmentLength - offset) + 1);
                                yellowWordSpans.Add(span);
                            }
                        }
                    }

                    if (documentKey != string.Format("{0}:{1}", RequestedPoint.Snapshot.Length, RequestedPoint.Snapshot.Version.VersionNumber))
                        return;
                }

               

                var currentRequest = RequestedPoint;
                var word = TextStructureNavigator.GetExtentOfWord(currentRequest);
                var currentWord = word.Span;

                //If another change hasn't happened, do a real update 
                if (currentRequest == RequestedPoint)
                {
                    SynchronousUpdate(currentRequest, new NormalizedSnapshotSpanCollection(yellowWordSpans), new NormalizedSnapshotSpanCollection(redWordSpans), currentWord);
                }
            }

            catch (Exception e)
            {
               // MessageBox.Show("2 - e : " + e.Message + " \r\n " + e.StackTrace);
            }
        }
    }
}