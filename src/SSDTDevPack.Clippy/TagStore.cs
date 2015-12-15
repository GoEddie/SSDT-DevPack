using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SSDTDevPack.Clippy.Operations;
using SSDTDevPack.Common.ScriptDom;

namespace SSDTDevPack.Clippy
{
    public class TagStore
    {
        private readonly ConcurrentDictionary<string, List<GlyphDefinition>> _definitions = new ConcurrentDictionary<string, List<GlyphDefinition>>();
        private readonly AutoResetEvent _event = new AutoResetEvent(false);
        private readonly List<ClippyOperationBuilder> _operations = new List<ClippyOperationBuilder>();
        private readonly ConcurrentStack<string> _queuedRequests = new ConcurrentStack<string>();
        private Thread _processor;
        private bool _stop;

        public TagStore()
        {
            ClippySettings.ClippyDisabled = Stop;

            _operations.Add(new QueryCostOperations());
            _operations.Add(new InequalityReWriteOperation());
            _operations.Add(new IsNullReWriteOperation());
            _operations.Add(new OrdinalOrderByReWriteOperation());
            _operations.Add(new DeleteChunkerOperation());
            _operations.Add(new TableNameCorrectCaser());
            Start();
        }

        public bool Stopped { get; set; }

        private void Start()
        {
            _processor = new Thread(DoWork);
            _processor.Start();
        }

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

                    if (string.IsNullOrEmpty(text))
                        continue;

                    if (_definitions.ContainsKey(text))
                        continue; //already done it....

                    try
                    {

                        IList<ParseError> errors;
                        var statements = ScriptDom.GetStatements(text, out errors);
                        if (statements.Count > 0)
                        {
                            var glyphDefinitions = GetGlyphDefinitions(statements, text);

                            _definitions[text] = glyphDefinitions;
                        }
                    }
                    catch (Exception e)
                    {
                        //hmmmmm
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

                        _queuedRequests.PushRange(rePop, 0, count - 1);
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

                var fragment = script.Substring(statement.StartOffset, statement.FragmentLength);
                var queriesInStatement = ScriptDom.GetQuerySpecifications(fragment);
                var deletes = ScriptDom.GetDeleteStatements(fragment);
                
                foreach (var operation in _operations)
                {
                    definition = operation.GetDefintions(fragment, statement, definition, queriesInStatement);
                    definition = operation.GetDefintions(fragment, statement, definition, deletes);

                    definition = operation.GetDefinitions(fragment, statement, definition, new List<TSqlStatement>() {statement});
                    
                }
                

                definitions.Add(definition);
            }

            return definitions;
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

        public void Stop()
        {
            Debug.WriteLine("Clippy Store Disabled....");
            _stop = true;
        }
    }
}