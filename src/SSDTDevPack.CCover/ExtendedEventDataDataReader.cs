using System;
using System.Collections.Concurrent;
using SSDTDevPack.Common.UserMessages;

namespace SSDTDevPacl.CodeCoverage.Lib
{
    public class ExtendedEventDataDataReader
    {
        private readonly string _connectionString;

        public readonly ConcurrentQueue<CoveredStatement> CoveredStatements = new ConcurrentQueue<CoveredStatement>();
        public readonly ConcurrentDictionary<int, string> ObjectNameCache = new ConcurrentDictionary<int, string>();

        private bool _continue = true;

        private string _databaseName;

        private ExtendedEventGateway _gateway;

        public ExtendedEventDataDataReader(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Stop()
        {
            try
            {
                _gateway.StopTrace();

                foreach (var item in _gateway.GetStatements(ObjectNameCache))
                {
                    CoveredStatements.Enqueue(item);
                }
            }
            catch (Exception e)
            {
                OutputPane.WriteMessageAndActivatePane("CodeCoverage, error stopping the trace: {0}", e);
            }
        }

        public void Start()
        {
            try
            {
                _continue = true;
                _gateway = new DatabaseGateway(_connectionString).Get();

                _gateway.StartTrace();
            }
            catch (Exception e)
            {
                OutputPane.WriteMessageAndActivatePane("CodeCoverage, error starting the trace: {0}", e);
            }
        }
    }
}