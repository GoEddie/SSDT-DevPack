using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using EnvDTE;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using SSDTDevPack.Common.Settings;
using SSDTDevPack.Common.VSPackage;

namespace SSDTDevPack.Clippy
{
    public class ClippyTagger : ITagger<ClippyTag>
    {
        
        internal ClippyTagger(IClassifier aggregator)
        {
            _aggregator = aggregator;
            var settings = SavedSettings.Get();
            _lastCallDelay = settings.Clippy.CallDelayMilliSeconds;

            if (settings.Clippy.StartEnabled)
                ClippySettings.Enabled = true;
        }


        private readonly IClassifier _aggregator;

        private readonly TagStore _store = new TagStore();
        private object _lock = new object();

        private DateTime _lastCallTime;
        private IEnumerable<ITagSpan<ClippyTag>> _lastSpans;
        private int _lastCallDelay;

        public void Reset()
        {
            _lastSpans = null;
            _lastCallTime = DateTime.MinValue;
        }

        IEnumerable<ITagSpan<ClippyTag>> ITagger<ClippyTag>.GetTags(NormalizedSnapshotSpanCollection spans)
        {
            try
            {
                if (spans == null || spans.FirstOrDefault() == null)
                    return null;

                if (spans.FirstOrDefault().Snapshot.ContentType.TypeName != "SQL Server Tools")
                    return null;

                var items = new List<ITagSpan<ClippyTag>>();

                if (_lastCallTime.AddMilliseconds(_lastCallDelay) >= DateTime.Now)
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

                //if (_store == null)
                //{
                //    _store = new TagStore();
                //    Monitor.Exit(_lock);
                //    return _lastSpans;
                //}

                if (_store.Stopped)
                {
                    _store.Stopped = false;
                    Monitor.Exit(_lock);
                    return _lastSpans;
                }


                var text = spans.FirstOrDefault().Snapshot.GetText();

                var glyphs = new OperationsBuilder(spans.FirstOrDefault().Snapshot, _store).GetStatementOptions(text);

                foreach (var g in glyphs)
                {
                    if (g.Menu.Count == 0)
                        continue;

                    var tag = new ClippyTag(g);

                    var tagSpan = new TagSpan<ClippyTag>(new SnapshotSpan(spans.FirstOrDefault().Snapshot, g.StatementOffset, g.StatementLength), tag);
                    tag.Tagger = this;
                    tagSpan.Tag.ParentTag = tagSpan;
                    g.Tag = tagSpan.Tag;
                    items.Add(tagSpan);

                }

                Monitor.Exit(_lock);
                _lastSpans = items;
                return items;

            }
            catch (Exception)
            {
                Monitor.Exit(_lock);
                return null;
            }
        }


#pragma warning disable 67
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
#pragma warning restore 67

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
}