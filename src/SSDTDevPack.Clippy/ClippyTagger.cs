using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using EnvDTE;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using SSDTDevPack.Common.ScriptDom;
using SSDTDevPack.Common.UserMessages;
using SSDTDevPack.Common.VSPackage;
using SSDTDevPack.Indexes;

namespace SSDTDevPack.Clippy
{
    /// <summary>
    ///     This class implements ITagger for ClippyTag.  It is responsible for creating
    ///     ClippyTag TagSpans, which our GlyphFactory will then create glyphs for.
    /// </summary>
    internal class ClippyTagger : ITagger<ClippyTag>
    {
        
        internal ClippyTagger(IClassifier aggregator)
        {
            _aggregator = aggregator;
        }


        private readonly IClassifier _aggregator;

        private TagStore _store;
        private object _lock = new object();

        private DateTime _lastCallTime;
        private IEnumerable<ITagSpan<ClippyTag>> _lastSpans;


        IEnumerable<ITagSpan<ClippyTag>> ITagger<ClippyTag>.GetTags(NormalizedSnapshotSpanCollection spans)
        {

            var items = new List<ITagSpan<ClippyTag>>();

            if (_lastCallTime.AddMilliseconds(1500) >= DateTime.Now)
                return _lastSpans;

            _lastCallTime = DateTime.Now;

            if (!Monitor.TryEnter(_lock))
            {
                return _lastSpans;
            }
            
            var dte = VsServiceProvider.Get(typeof (DTE));


            if (null == dte || !ClippySettings.Enabled)
            {
                Monitor.Exit(_lock);
                return _lastSpans;
            }

            if (_store == null)
            {
                _store = new TagStore();
                Monitor.Exit(_lock);
                return _lastSpans;
            }

            if (_store.Stopped)
            {
                _store.Stopped = false;
                Monitor.Exit(_lock);
                return _lastSpans;
            }


            var text = spans.FirstOrDefault().Snapshot.GetText();

            var glyphs  = new OperationsBuilder(spans.FirstOrDefault().Snapshot, _store).GetStatementOptions(text);
         
            foreach (var g in glyphs)
            {
                var tag = new ClippyTag(g);
                
                SnapshotPoint line;

                if (g.StatementOffset <= 0)
                {
                    line = spans.FirstOrDefault().Snapshot.GetLineFromPosition(0).Start;
                }
                else
                {
                    line = spans.FirstOrDefault().Snapshot.GetLineFromLineNumber(g.Line - 1).Start;
                }

                var tagSpan = new TagSpan<ClippyTag>(new SnapshotSpan(line, g.StatementLength), tag);
                Debug.WriteLine("TagSpan: start: {0}, {1}", tagSpan.Span.Start, tagSpan.Span.End);
                tagSpan.Tag.ParentTag = tagSpan;
                g.Tag = tagSpan.Tag;
                items.Add(tagSpan);
                    
            }

            Monitor.Exit(_lock);
            _lastSpans = items;
            return items;
    }


#pragma warning disable 67
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
#pragma warning restore 67

    }

    public class TagStore
    {
        public TagStore()
        {
            ClippySettings.ClippyDisabled = Stop;
            Start();
        }

        private void Start()
        {
            _processor = new System.Threading.Thread(DoWork);
            _processor.Start();
        }

        private AutoResetEvent _event = new AutoResetEvent(false);
        
        public bool Stopped { get; set; }

        public void DoWork()
        {
            _stop = false;
            Stopped = false;

            while (!_stop)
            {
                while (!_queuedRequests.IsEmpty)
                {
                    string text;
                    _queuedRequests.TryPop(out text);

                    if (String.IsNullOrEmpty(text))
                        continue;

                    if (_definitions.ContainsKey(text))
                        continue; //already done it....

                    IList<ParseError> errors;
                    var statements = ScriptDom.GetStatements(text, out errors);
                    if (statements.Count > 0)
                    {
                        var glyphDefinitions = GetGlyphDefinitions(statements, text);
                        
                        _definitions[text] = glyphDefinitions;
                    }

                    var rePop = new string[5];
                    var clearPop = new string[100];

                    if (_queuedRequests.Count > 10)
                    {
                        var count = _queuedRequests.TryPopRange(rePop);

                        while (_queuedRequests.Count > 10)
                        {
                            _queuedRequests.TryPopRange(clearPop);
                        }

                        _queuedRequests.PushRange(rePop, 0, count-1);
                    }
                }

                _event.WaitOne();
            }

            Stopped = true;
        }

        
        private List<GlyphDefinition> GetGlyphDefinitions(List<TSqlStatement> statements, string script)
        {
            var definitions = new List<GlyphDefinition>();

            foreach (var statement in statements)
            {
                var definition = new GlyphDefinition();
                definition.Line = statement.StartLine;
                definition.StatementOffset = statement.StartOffset;
                definition.Type = GlyphDefinitonType.Normal;
                definition.LineCount = statement.ScriptTokenStream.LastOrDefault().Line - definition.Line;
                definition.StatementLength = statement.FragmentLength;
                
                var nonSargableRewriter = new NonSargableRewrites(script.Substring(statement.StartOffset, statement.FragmentLength));
                var replacements = nonSargableRewriter.GetReplacements();

                if (replacements.Count > 0)
                {
                    definition.Menu.Add(new MenuDefinition()
                    {
                        Caption = "Replace non-sargable IsNull",
                        Action = () =>
                        {
                        },
                        Type = MenuItemType.Header
                        ,Glyph = definition
                    });

                    var offsettedReplacments = new List<Replacements>();
                    foreach (var replacement in replacements)
                    {
                        var replacement1 = replacement;
                        replacement1.OriginalOffset += statement.StartOffset;
                        offsettedReplacments.Add(replacement1);
                    }

                    if (replacements.Count > 1)
                    {
                        var menu = new MenuDefinition();
                        menu.Operation = new ClippyReplacementOperations(offsettedReplacments);
                        menu.Action = () => PerformAction(menu.Operation, menu.Glyph);

                        menu.Glyph = definition;
                        menu.Caption = GetCaptionForAll(statement);
                        menu.Type = MenuItemType.MenuItem;
                        definition.Menu.Add(menu);
                    }


                    foreach (var replacement in offsettedReplacments)
                    {
                        var menu = new MenuDefinition();
                        menu.Action = () => PerformAction(menu.Operation, menu.Glyph);
                        menu.Glyph = definition;
                        menu.Caption = string.Format("\t\"{0}\" into \"{1}\"", replacement.Original, replacement.Replacement);
                        menu.Type = MenuItemType.MenuItem;
                        menu.Operation = new ClippyReplacementOperation(replacement);
                        definition.Menu.Add(menu);

                    }

                    definition.GenerateKey();
                    definitions.Add(definition);
                }
                
            }

            return definitions;
        }

        private void PerformAction(ClippyOperation operation, GlyphDefinition glyph)
        {
            if (operation is ClippyReplacementOperations)
            {
                (operation as ClippyReplacementOperations).DoOperation(glyph);
                return;
            }

            if (operation is ClippyReplacementOperation)
            {
                (operation as ClippyReplacementOperation).DoOperation(glyph);
                return;
            }
        }


        private string GetCaptionForAll(TSqlStatement statement)
        {
            if (statement is SelectStatement)
                return "All on Statement";

            if (statement.GetType().Name.StartsWith("Create"))
                return "All in Module";

            return "All";
        }


        public List<GlyphDefinition> GetStatements(string text)
        {
            if (_definitions.ContainsKey(text))
                return _definitions[text];

            if (!Stopped)
            {
                _queuedRequests.Push(text);
                _event.Set();
            }

            return new List<GlyphDefinition>();
        }

        ConcurrentDictionary<string, List<GlyphDefinition>> _definitions = new ConcurrentDictionary<string, List<GlyphDefinition>>();
        private ConcurrentStack<string> _queuedRequests = new ConcurrentStack<string>(); 
        private System.Threading.Thread _processor;
        private bool _stop = false;
        public void Stop()
        {
            Debug.WriteLine("Clippy Store Disabled....");
            _stop = true;
        }
        
    }


    class DummyBuilder
    {
        public List<GlyphDefinition> Get(int max)
        {
            List<GlyphDefinition> tags = new List<GlyphDefinition>();

            for (int i = 1; i < max; i++)
            {
                if (i%2 == 0)
                    continue;

                var definition = new GlyphDefinition();
                definition.Line = i;
                definition.LineCount = 1;
                definition.Menu.Add(new MenuDefinition()
                {
                    Caption = "why hello there", Type = MenuItemType.Header, Glyph = definition
                });
                
               
                tags.Add(definition);
            }

            return tags;
        }
    }


    public abstract class ClippyOperation
    {
        public abstract void DoOperation(GlyphDefinition glyph);
    }

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
                    var span = glyph.Tag.ParentTag.Span;
                    var offset = span.GetText().IndexOf(replacement.Original);

                    if (span.GetText().Substring(offset, replacement.OriginalLength) != replacement.Original)
                        return;

                    var newSpan = span.Snapshot.CreateTrackingSpan(glyph.Tag.ParentTag.Span.Start + offset, replacement.OriginalLength, SpanTrackingMode.EdgeNegative);

                    _snapshot.TextBuffer.Replace(newSpan.GetSpan(newSpan.TextBuffer.CurrentSnapshot), replacement.Replacement);
                }
                catch (Exception e)
                {
                    OutputPane.WriteMessage("error unable to do replacement : {0}", e);
                }
            }
        }
    }



    /*
     private void DoAllReplacement(List<Replacements> replacements)
        {

            var span = _snapshot.CreateTrackingSpan(0, _snapshot.Length, SpanTrackingMode.EdgeNegative);

            foreach (var replacement in replacements.OrderByDescending(p => p.OriginalOffset))
            {
                DoReplacement(replacement);
            }
        }
     */

}