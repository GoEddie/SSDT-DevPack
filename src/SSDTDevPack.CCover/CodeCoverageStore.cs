using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SSDTDevPacl.CodeCoverage.Lib
{
    public class CodeCoverageStore
    {
        private readonly ConcurrentDictionary<string, List<CoveredStatement>> _statements = new ConcurrentDictionary<string, List<CoveredStatement>>();

        static CodeCoverageStore()
        {
            Get = new CodeCoverageStore();
        }

        private CodeCoverageStore()
        {
            
        }

        public static CodeCoverageStore Get;

        public List<CoveredStatement> GetCoveredStatements(string objectName, string fileName)
        {
            objectName = objectName.ToLowerInvariant();

            if (_statements.ContainsKey(objectName))
            {
                if (_statements[objectName].OrderBy(p => p.TimeStamp).FirstOrDefault()?.TimeStamp < File.GetLastWriteTime(fileName))
                {
                    List<CoveredStatement> list;
                    _statements.TryRemove(objectName, out list);
                    return null;
                }

                return _statements[objectName];
            }

            objectName = "dbo." + objectName;

            if (_statements.ContainsKey(objectName))
            {
                if (_statements[objectName].OrderBy(p => p.TimeStamp).FirstOrDefault()?.TimeStamp < File.GetLastWriteTime(fileName))
                {
                    List<CoveredStatement> list;
                    _statements.TryRemove(objectName, out list);
                    return null;
                }

                return _statements[objectName];
            }

            return null;
        }

        public void AddStatements(ConcurrentQueue<CoveredStatement> coveredStatements, ConcurrentDictionary<int, string> objectNameCache)
        {
            while (!coveredStatements.IsEmpty)
            {
                CoveredStatement statement;
                
                if (coveredStatements.TryDequeue(out statement))
                {
                    if (!objectNameCache.ContainsKey(statement.ObjectId))
                        continue;
                    
                    var name = objectNameCache[statement.ObjectId].ToLowerInvariant();

                    if (!_statements.ContainsKey(name))
                    {
                        _statements[name] = new List<CoveredStatement>();
                    }

                    var statments = _statements[name];

                    if (statments.All(p => p.Offset != statement.Offset))
                    {
                        statments.Add(statement);
                    }
                    else
                    {
                        statments.Remove(statments.First(p => p.Offset == statement.Offset));
                        statments.Add(statement);
                    }
                    
                }

            }
        }

        public void ClearStatements()
        {
            _statements.Clear();
        }
    }
}